using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.Common.Blob.Queries;
using Voupon.Rewards.WebApp.Common.Products.Models;
using Voupon.Rewards.WebApp.Services.Cart.Models;
using Voupon.Rewards.WebApp.ViewModels;
using OrderItems = Voupon.Rewards.WebApp.Services.Cart.Models.OrderItems;

namespace Voupon.Rewards.WebApp.Services.Merchants.Page
{

    public class MerchantPage : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public string ExternalMerchantId { get; set; }
        public byte ExternalTypeId { get; set; }
    }

    public class MerchantPageViewModel
    {
        public int Id { get; set; }
        public string ShopName { get; set; }
        public string ShopLogoUrl { get; set; }
        public string Description { get; set; }
        public string ShopCategories { get; set; }
        public List<string> OutletList { get; set; }
        public string WebsiteUrl { get; set; }
        public List<string> OutletImageUrlList { get; set; }
        public decimal Rating { get; set; }
        public string MerchantEmailId { get; set; }

        public List<ProductReview> ProductReviewList { get; set; }
        public List<MerchantCarousel> MerchantCarouselList { get; set; }

        public string ExternalShopUsername { get; set; }
        public string ExternalShopId { get; set; }

        public byte ExternalTypeId { get; set; }
    }

    public class MerchantCarousel
    {
        public long Id { get; set; }
        public int MerchantId { get; set; }
        public string ImageUrl { get; set; }
        public byte StatusId { get; set; }
        public int OrderNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
    }

    public class ProductReview
    {
        //public Guid Id { get; set; }
        public int ProductId { get; set; }
        //public int MerchantId { get; set; }
        //public Guid OrderItemId { get; set; }
        public decimal Rating { get; set; }
        public string Comment { get; set; }
        public string MemberName { get; set; }
        public string ProductTitle { get; set; }
        // public int MasterMemberProfileId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class MerchantPageHandler : IRequestHandler<MerchantPage, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        private readonly IAzureBlobStorage azureBlobStorage;
        public MerchantPageHandler(RewardsDBContext rewardsDBContext, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.azureBlobStorage = azureBlobStorage;
        }

        private static string StripHTML(string htmlString)
        {

            string pattern = @"<(.|\n)*?>";

            return Regex.Replace(htmlString, pattern, string.Empty);
        }

        public async Task<ApiResponseViewModel> Handle(MerchantPage request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                Voupon.Database.Postgres.RewardsEntities.Merchants merchant;

                if (request.Id == 0)
                {
                    merchant = await rewardsDBContext.Merchants.AsNoTracking().Include(x => x.MerchantCarousel).Include(x => x.Outlets).ThenInclude(x => x.PostCode).Include(x => x.Outlets).ThenInclude(x => x.Province).Include(x => x.Outlets).ThenInclude(x => x.District).Include(x => x.Outlets).ThenInclude(x => x.Country).Where(x => x.ExternalId == request.ExternalMerchantId && x.ExternalTypeId == request.ExternalTypeId && x.IsTestAccount == false).FirstOrDefaultAsync();
                }
                else
                {
                    merchant = await rewardsDBContext.Merchants.AsNoTracking().Include(x => x.MerchantCarousel).Include(x => x.Outlets).ThenInclude(x => x.PostCode).Include(x => x.Outlets).ThenInclude(x => x.Province).Include(x => x.Outlets).ThenInclude(x => x.District).Include(x => x.Outlets).ThenInclude(x => x.Country).Where(x => x.Id == request.Id && x.IsTestAccount == false).FirstOrDefaultAsync();
                }

                if (merchant != null)
                {
                    MerchantPageViewModel merchantViewModel = new MerchantPageViewModel();
                    merchantViewModel.Id = merchant.Id;
                    merchantViewModel.ShopName = merchant.DisplayName;
                    merchantViewModel.Description = merchant.Description;
                    if(merchant.ExternalTypeId != null)
                    {
                        merchantViewModel.ExternalTypeId = (byte)merchant.ExternalTypeId;
                        merchantViewModel.ExternalShopId = merchant.ExternalId;
                    }

                    var merchantManagerUser = await rewardsDBContext.Users.Include(x => x.UserRoles).Where(x => x.UserRoles.Any(y => y.MerchantId == merchant.Id && y.RoleId.ToString() == "1A436B3D-15A0-4F03-8E4E-0022A5DD5736")).FirstOrDefaultAsync();
                    if (merchantManagerUser != null)
                    {
                        merchantViewModel.MerchantEmailId = merchantManagerUser.Email;
                    }

                    if (merchant.MerchantCarousel != null)
                    {
                        merchantViewModel.MerchantCarouselList = merchant.MerchantCarousel.Where(x => x.StatusId == 1).Select(x => new MerchantCarousel
                        {
                            Id = x.Id,
                            ImageUrl = x.ImageUrl,
                        }).ToList();
                    }


                    merchantViewModel.WebsiteUrl = merchant.WebsiteSiteUrl;
                    merchantViewModel.ShopLogoUrl = merchant.LogoUrl.Replace("http://", "https://").Replace(":80", "");
                    merchantViewModel.Rating = merchant.Rating;
                    merchantViewModel.OutletList = new List<string>();
                    merchantViewModel.OutletImageUrlList = new List<string>();
                    var merchantProducts = await rewardsDBContext.Products.AsNoTracking().Include(x => x.ProductCategory).Include(x => x.ProductSubCategory).Where(x => x.MerchantId == request.Id && x.IsPublished == true && x.IsActivated == true).ToListAsync();
                    List<string> categoriesAndSubcategoriesList = new List<string>();
                    foreach (var product in merchantProducts)
                    {
                        if (product.ProductCategory != null)
                        {
                            if (!categoriesAndSubcategoriesList.Contains(product.ProductCategory.Name))
                                categoriesAndSubcategoriesList.Add(product.ProductCategory.Name);
                        }
                        if (product.ProductSubCategory != null)
                        {
                                
                            if (!categoriesAndSubcategoriesList.Contains(product.ProductSubCategory.Name))
                                categoriesAndSubcategoriesList.Add(product.ProductSubCategory.Name);
                        }
                    }
                    for (int i = 0; i < categoriesAndSubcategoriesList.Count; i++)
                    {
                        if (i == 9)
                            break;
                        if (i == 0)
                            merchantViewModel.ShopCategories += categoriesAndSubcategoriesList[i];
                        else
                            merchantViewModel.ShopCategories += ", " + categoriesAndSubcategoriesList[i];

                    }

                    foreach (var outlet in merchant.Outlets)
                    {
                        if (!outlet.IsActivated)
                            continue;
                        if (outlet.IsDeleted)
                            continue;
                        string district = rewardsDBContext.Districts.FirstOrDefault(x => x.Id == outlet.DistrictId).Name;
                        string postCode = rewardsDBContext.PostCodes.FirstOrDefault(x => x.Id == outlet.PostCodeId).Name;
                        string province = rewardsDBContext.Provinces.FirstOrDefault(x => x.Id == outlet.ProvinceId).Name;
                        string outletItem = outlet.Name + " - ";
                        outletItem = outletItem + (String.IsNullOrEmpty(outlet.Address_1) ? "" : outlet.Address_1 + ", ") + (String.IsNullOrEmpty(outlet.Address_2) ? "" : outlet.Address_2 + ", ") + district + ", " + postCode + " " + province;
                        merchantViewModel.OutletList.Add(outletItem);
                        merchantViewModel.OutletImageUrlList.AddRange(await GetOutletImageList(outlet.Id));
                    }
                    if (merchantViewModel.OutletImageUrlList.Count > 10)
                        merchantViewModel.OutletImageUrlList.RemoveRange(10, merchantViewModel.OutletImageUrlList.Count - 10);

                    merchantViewModel.ProductReviewList = new List<ProductReview>();

                    var productReviewList = await rewardsDBContext.ProductReview.Where(x => x.MerchantId == merchantViewModel.Id).OrderByDescending(x => x.CreatedAt).ToListAsync();
                    foreach (var review in productReviewList)
                    {
                        ProductReview newReview = new ProductReview();
                        newReview.CreatedAt = review.CreatedAt;
                        newReview.Comment = review.Comment;
                        newReview.MemberName = String.IsNullOrEmpty(review.MemberName) ? "Vodus Customer" : review.MemberName;
                        newReview.ProductId = review.ProductId;
                        newReview.ProductTitle = review.ProductTitle;
                        newReview.Rating = review.Rating;
                        merchantViewModel.ProductReviewList.Add(newReview);
                    }

                    if (request.Id == 0)
                    {
                        merchantViewModel.Id = 0;
                    }

                    response.Successful = true;
                    response.Message = "Get Merchant Successfully";
                    response.Data = merchantViewModel;
                    return response;
                }

                //   check if ID belongs to giftee setup
                if (request.Id == 1000)
                {
                    response.Data = GifteeSushiKing();
                    response.Successful = true;
                    return response;
                }

                else if (request.Id == 1001)
                {
                    response.Data = GifteeLlaollao();
                    response.Successful = true;
                    return response;
                }

                else if (request.Id == 1002)
                {
                    response.Data = GifteeHokkaido();
                    response.Successful = true;
                    return response;
                }

                else if (request.Id == 1003)
                {
                    response.Data = GifteeBigApple();
                    response.Successful = true;
                    return response;
                }

                else if (request.Id == 1004)
                {
                    response.Data = GifteeTeaLive();
                    response.Successful = true;
                    return response;
                }


                response.Successful = false;
                response.Message = "Merchant Not Found";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }

        private MerchantPageViewModel GifteeSushiKing()
        {
            var viewModel = new MerchantPageViewModel
            {
                Id = 1000,
                Description = "<p>Sushi King chain of restaurants serves quality sushi and other Japanese cuisine at affordable prices in a warm and friendly environment.</p><p>What sets Sushi King apart is the personal touch of serving freshly made sushi on the kaiten for customers to pick up and enjoy.</p>",
                WebsiteUrl = "https://sushi-king.com/",
                ShopName = "Sushi king",
                ShopLogoUrl = "https://vodus.my/giftee/sushiking/logo.png",
            };

            viewModel.OutletList = new List<string>();
            viewModel.OutletList.Add("Available in all Sushi King outlets in Malaysia");

            viewModel.OutletImageUrlList = new List<string>();
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/sushiking/sushi-king-outlet-1.jpg");
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/sushiking/sushi-king-outlet-2.jpg");
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/sushiking/sushi-king-outlet-3.jpg");
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/sushiking/sushi-king-outlet-4.jpg");
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/sushiking/sushi-king-outlet-5.jpg");
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/sushiking/sushi-king-outlet-6.jpg");

            return viewModel;
        }

        private MerchantPageViewModel GifteeLlaollao()
        {
            var viewModel = new MerchantPageViewModel
            {
                Id = 1001,
                Description = "<p>llaollao is Frozen Yogurt. Or yogurt ice cream, which is the same thing. And, as it is natural, it is one of the healthiest and most recommended foods worldwide, thanks to its healthy properties and high nutritional value.</p><p>Its preparation using skimmed milk when served and its combination with prime quality toppings – from recently cut seasonal fruits to cereals, fun crunchy toppings and delicious sauces – makes it an even more delicious and healthier product.</p>",
                WebsiteUrl = "https://www.llaollaoweb.com/en/",
                ShopName = "Llao Llao",
                ShopLogoUrl = "https://vodus.my/giftee/llaollao/logo.png"
            };

            viewModel.OutletList = new List<string>();
            viewModel.OutletList.Add("Available in all Llao llao outlets in Malaysia");

            viewModel.OutletImageUrlList = new List<string>();
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/llaollao/llaollao-outlet-1.jpg");
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/llaollao/llaollao-outlet-2.jpg");
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/llaollao/llaollao-outlet-3.jpeg");
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/llaollao/llaollao-outlet-4.jpg");
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/llaollao/llaollao-outlet-5.jpg");
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/llaollao/llaollao-outlet-6.jpg");

            return viewModel;
        }

        private MerchantPageViewModel GifteeHokkaido()
        {
            var viewModel = new MerchantPageViewModel
            {
                Id = 1002,
                Description = "<p>Our resident bakers worked with counterparts from Hokkaido, Japan to further improve the recipe of this baked pastry to ensure sustainable uniqueness in its taste and texture. The soft and creamy centre is made with a blend of three different high-quality specialty cheeses; piped into a crunchy shortcrust pastry base.</p><p>Satisfy the urge of one’s tummy, any time of the day! Tasting best when it is right out from the oven, the Hokkaido Baked Cheese Tart also taste awesome when chilled; leaving you with a smooth and refreshing experience. When frozen, the tart just tastes like a creamy cheesy ice-cream. Try all ways and decide which one you like best!</ p>",
                WebsiteUrl = "http://www.hbct.com.my/",
                ShopName = "Hokkaido Cheese Bake Tart",
                ShopLogoUrl = "https://vodus.my/giftee/hokkaido/logo.jpeg"
            };

            viewModel.OutletList = new List<string>();
            viewModel.OutletList.Add("Available in all Hokkaido outlets in Malaysia");

            viewModel.OutletImageUrlList = new List<string>();
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/hokkaido/hbct-outlet-1.jpg");
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/hokkaido/hbct-outlet-2.jpg");
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/hokkaido/hbct-outlet-3.jpg");
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/hokkaido/hbct-outlet-4.jpg");
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/hokkaido/hbct-outlet-5.jpg");
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/hokkaido/hbct-outlet-6.jpg");

            return viewModel;
        }

        private MerchantPageViewModel GifteeBigApple()
        {
            var viewModel = new MerchantPageViewModel
            {
                Id = 1003,
                Description = "<p>BIG APPLE Donuts & Coffee offers a wide selection of premium donuts, with a range of tantalizing coffees and a charming variety of tea creations. Its exceptional donut qualities come from a unique premix formula with a carefully selected blend of over 20 imported ingredients, producing donuts known for their hallmark freshness and fluffy soft texture.</p><p>Big Apple Donuts & Coffee began as a simple idea for people to come together and share their passion for donuts, and it slowly evolved into a thriving successful business built upon trust, commitment and innovation. Every day, the people at Big Apple Donuts & Coffee strive with the same passion to deliver not just great tasting donuts but a unique customer experience that encapsulates the qualities of the brand.</p>",
                WebsiteUrl = "https://www.bigappledonuts.com",
                ShopName = "Big Apple",
                ShopLogoUrl = "https://vodus.my/giftee/bigapple/logo.png"
            };

            viewModel.OutletList = new List<string>();
            viewModel.OutletList.Add("Available in all Big Apple outlets in Malaysia");

            viewModel.OutletImageUrlList = new List<string>();
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/bigapple/big-apple-outlet-1.jpg");
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/bigapple/big-apple-outlet-2.jpg");
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/bigapple/big-apple-outlet-3.jpg");
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/bigapple/big-apple-outlet-4.jpg");
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/bigapple/big-apple-outlet-5.jpg");
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/bigapple/big-apple-outlet-6.jpg");

            return viewModel;
        }

        private MerchantPageViewModel GifteeTeaLive()
        {
            var viewModel = new MerchantPageViewModel
            {
                Id = 1004,
                Description = "<p>Tealive is Southeast Asia's largest lifestyle tea brand, and our mission is to always bring joyful experiences through tea - Serving a variety of beverages, from signature pearl milk tea to coffee and smoothies.</p>",
                WebsiteUrl = "https://www.tealive.com.my/",
                ShopName = "TeaLive",
                ShopLogoUrl = "https://vodus.my/giftee/tealive/logo.jpeg"
            };

            viewModel.OutletList = new List<string>();
            viewModel.OutletList.Add("Available in all TeaLive outlets in Malaysia");

            viewModel.OutletImageUrlList = new List<string>();
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/tealive/tealive-outlet-1.jpg");
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/tealive/tealive-outlet-2.jpg");
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/tealive/tealive-outlet-3.jpg");
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/tealive/tealive-outlet-4.jpg");
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/tealive/tealive-outlet-5.jpg");
            viewModel.OutletImageUrlList.Add("https://vodus.my/giftee/tealive/tealive-outlet-6.jpg");

            return viewModel;
        }

        private async Task<List<string>> GetOutletImageList(int outletId)
        {
            var filename = await azureBlobStorage.ListBlobsAsync(ContainerNameEnum.Outlets, outletId + "/" + FilePathEnum.Outlets_Images);
            var fileList = new List<string>();
            foreach (var file in filename)
            {
                if (file.StorageUri.PrimaryUri.OriginalString.Contains("normal") || (!file.StorageUri.PrimaryUri.OriginalString.Contains("big") && !file.StorageUri.PrimaryUri.OriginalString.Contains("small") && !file.StorageUri.PrimaryUri.OriginalString.Contains("org")))
                    fileList.Add(file.StorageUri.PrimaryUri.OriginalString.Replace("http://", "https://").Replace(":80", ""));
            }
            return fileList;
        }
    }
}
