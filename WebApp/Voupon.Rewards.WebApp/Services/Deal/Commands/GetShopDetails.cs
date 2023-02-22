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

namespace Voupon.Rewards.WebApp.Services.Deal.Commands
{

    public class GetShopDetails : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
    }

    public class MerchantPageViewModel
    {
        public int Id { get; set; }
        public string ShopName { get; set; }
        public string ShopLogoUrl { get; set; }
        public int ShopProductsNo { get; set; }
        public DateTime ShopJoinedTime{ get; set; }
        public string WebsiteUrl { get; set; }
        public decimal Rating { get; set; }
        public string ExternalShopUsername { get; set; }
        public string ExternalShopId { get; set; }
        public string MerchantEmailId { get; set; }
        public string MerchantName { get; set; }

        public byte ExternalTypeId { get; set; }
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
    public class GetShopDetailsHandler : IRequestHandler<GetShopDetails, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        private readonly IAzureBlobStorage azureBlobStorage;
        public GetShopDetailsHandler(RewardsDBContext rewardsDBContext, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.azureBlobStorage = azureBlobStorage;
        }

        private static string StripHTML(string htmlString)
        {

            string pattern = @"<(.|\n)*?>";

            return Regex.Replace(htmlString, pattern, string.Empty);
        }

        public async Task<ApiResponseViewModel> Handle(GetShopDetails request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var merchant = await rewardsDBContext.Merchants.AsNoTracking().Where(x => x.Id == request.Id && x.IsTestAccount == false).FirstOrDefaultAsync();
                if (merchant != null)
                {
                    var merchantManagerUser = await rewardsDBContext.Users.Include(x => x.UserRoles).Where(x => x.UserRoles.Any(y => y.MerchantId == merchant.Id && y.RoleId.ToString() == "1A436B3D-15A0-4F03-8E4E-0022A5DD5736")).FirstOrDefaultAsync();
                    MerchantPageViewModel merchantViewModel = new MerchantPageViewModel();
                    merchantViewModel.Id = merchant.Id;
                    merchantViewModel.ShopName = merchant.DisplayName;
                    var shopProductNo = await rewardsDBContext.Products.AsNoTracking().Where(x => x.MerchantId == request.Id && x.IsPublished == true && x.IsActivated == true).CountAsync();
                    merchantViewModel.ShopProductsNo = shopProductNo;
                    merchantViewModel.ShopJoinedTime = merchant.CreatedAt;
                    if (merchantManagerUser != null)
                    {
                        merchantViewModel.MerchantEmailId = merchantManagerUser.Email;
                        merchantViewModel.MerchantName = merchant.DisplayName;
                    }


                    merchantViewModel.WebsiteUrl = merchant.WebsiteSiteUrl;
                    merchantViewModel.ShopLogoUrl = merchant.LogoUrl.Replace("http://", "https://").Replace(":80", "");
                    merchantViewModel.Rating = merchant.Rating;


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

                WebsiteUrl = "https://sushi-king.com/",
                ShopName = "Sushi king",
                ShopLogoUrl = "https://vodus.my/giftee/sushiking/logo.png",
            };

            return viewModel;
        }

        private MerchantPageViewModel GifteeLlaollao()
        {
            var viewModel = new MerchantPageViewModel
            {
                Id = 1001,

                WebsiteUrl = "https://www.llaollaoweb.com/en/",
                ShopName = "Llao Llao",
                ShopLogoUrl = "https://vodus.my/giftee/llaollao/logo.png"
            };

            return viewModel;
        }

        private MerchantPageViewModel GifteeHokkaido()
        {
            var viewModel = new MerchantPageViewModel
            {
                Id = 1002,

                WebsiteUrl = "http://www.hbct.com.my/",
                ShopName = "Hokkaido Cheese Bake Tart",
                ShopLogoUrl = "https://vodus.my/giftee/hokkaido/logo.jpeg"
            };

            return viewModel;
        }

        private MerchantPageViewModel GifteeBigApple()
        {
            var viewModel = new MerchantPageViewModel
            {
                Id = 1003,

                WebsiteUrl = "https://www.bigappledonuts.com",
                ShopName = "Big Apple",
                ShopLogoUrl = "https://vodus.my/giftee/bigapple/logo.png"
            };

            return viewModel;
        }

        private MerchantPageViewModel GifteeTeaLive()
        {
            var viewModel = new MerchantPageViewModel
            {
                Id = 1004,
                WebsiteUrl = "https://www.tealive.com.my/",
                ShopName = "TeaLive",
                ShopLogoUrl = "https://vodus.my/giftee/tealive/logo.jpeg"
            };

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
