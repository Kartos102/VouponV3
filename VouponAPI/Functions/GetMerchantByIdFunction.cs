using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.API.ViewModels;
using Voupon.API.Util;

namespace Voupon.API.Functions
{
    public class GetMerchantByIdFunction
    {
        private readonly RewardsDBContext rewardsDBContext;
        private readonly VodusV2Context vodusV2Context;
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly IAzureBlobStorage azureBlobStorage;


        public GetMerchantByIdFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
            this.connectionMultiplexer = connectionMultiplexer;
            this.azureBlobStorage = azureBlobStorage;
        }

        [OpenApiOperation(operationId: "Get merchant by id", tags: new[] { "Merchants" }, Description = "Get merchant by id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "id", In = ParameterLocation.Query, Required = false, Type = typeof(int), Summary = "merchant id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "s", In = ParameterLocation.Query, Required = false, Type = typeof(string), Summary = "external shop id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "t", In = ParameterLocation.Query, Required = false, Type = typeof(byte), Summary = "External type id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(MerchantResponseModel), Summary = "Merchant detail")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If no id or external id is supplied")]


        [FunctionName("GetMerchantByIdFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "merchants/detail")] HttpRequest req, ILogger log)
        {
            var response = new MerchantResponseModel
            {
                Data = new MerchantData()
            };

            var merchantId = req.Query["id"];
            var externalShopId = req.Query["s"];
            var externalTypeId = req.Query["t"];

            if (string.IsNullOrEmpty(externalShopId))
            {
                int id = 0;
                if (!int.TryParse(merchantId, out id))
                {
                    return new BadRequestObjectResult(new ApiResponseViewModel
                    {
                        Code = -1,
                        ErrorMessage = "Fail to get data [001]"
                    });
                }

                response.Data.Merchant = await GetInternalMerchantDetail(id);
            }
            else
            {
                if (string.IsNullOrEmpty(externalShopId) || string.IsNullOrEmpty(externalShopId))
                {
                    return new BadRequestObjectResult(new ApiResponseViewModel
                    {
                        Code = -1,

                    });
                }
                byte typeId = 0;
                if (!byte.TryParse(externalTypeId, out typeId))
                {
                    return new BadRequestObjectResult(new ApiResponseViewModel
                    {
                        Code = -1,
                        ErrorMessage = "Fail to get data [002]"
                    });
                };

                response.Data.Merchant = await GetExternalMerchantDetail(externalShopId, typeId);
            }

            return new OkObjectResult(response);
        }

        private async Task<Merchants> GetInternalMerchantDetail(int merchantId)
        {
            var merchantModel = new Merchants();

            //Database.Postgres.RewardsEntities.Merchants merchant = await rewardsDBContext.Merchants.AsNoTracking().Include(x => x.Outlets).ThenInclude(x => x.PostCode).Include(x => x.Outlets).ThenInclude(x => x.Province).Include(x => x.Outlets).ThenInclude(x => x.District).Include(x => x.Outlets).ThenInclude(x => x.Country).Where(x => x.Id == merchantId && x.IsTestAccount == false).FirstOrDefaultAsync();
            Database.Postgres.RewardsEntities.Merchants merchant = await rewardsDBContext.Merchants.AsNoTracking().Include(x => x.Outlets).ThenInclude(x => x.PostCode).Include(x => x.Outlets).ThenInclude(x => x.Province).Include(x => x.Outlets).ThenInclude(x => x.District).Include(x => x.Outlets).ThenInclude(x => x.Country).Where(x => x.Id == merchantId && x.IsTestAccount == false).Include(x => x.MerchantCarousel).FirstOrDefaultAsync();

            if (merchant != null)
            {
                merchantModel.Id = merchant.Id;
                merchantModel.ShopName = merchant.DisplayName;
                merchantModel.Description = merchant.Description;

                /*
                var merchantManagerUser = await rewardsDBContext.Users.Include(x => x.UserRoles).Where(x => x.UserRoles.Any(y => y.MerchantId == merchant.Id && y.RoleId.ToString() == "1A436B3D-15A0-4F03-8E4E-0022A5DD5736")).FirstOrDefaultAsync();
                if (merchantManagerUser != null)
                {
                    merchantModel.MerchantEmailId = merchantManagerUser.Email;
                }
                */


                merchantModel.WebsiteUrl = merchant.WebsiteSiteUrl;
                merchantModel.ShopLogoUrl = (string.IsNullOrEmpty(merchant.LogoUrl) ? merchant.LogoUrl.Replace("http://", "https://").Replace(":80", "") : "");
                merchantModel.Rating = merchant.Rating;
                merchantModel.OutletList = new List<string>();
                merchantModel.OutletImageUrlList = new List<string>();

                if (merchant.MerchantCarousel != null)
                {
                    merchantModel.MerchantCarouselList = merchant.MerchantCarousel.Where(x => x.StatusId == 1).Select(x => new MerchantCarousel
                    {
                        Id = x.Id,
                        ImageUrl = x.ImageUrl,
                    }).ToList();
                }


                var merchantProducts = await rewardsDBContext.Products.AsNoTracking().Include(x => x.ProductCategory).Include(x => x.ProductSubCategory).Where(x => x.MerchantId == merchantId && x.IsPublished == true && x.IsActivated == true).ToListAsync();
                List<string> categoriesAndSubcategoriesList = new List<string>();
                foreach (var product in merchantProducts)
                {
                    if (!categoriesAndSubcategoriesList.Contains(product.ProductCategory.Name))
                        categoriesAndSubcategoriesList.Add(product.ProductCategory.Name);
                    if (!categoriesAndSubcategoriesList.Contains(product.ProductSubCategory.Name))
                        categoriesAndSubcategoriesList.Add(product.ProductSubCategory.Name);
                }
                for (int i = 0; i < categoriesAndSubcategoriesList.Count; i++)
                {
                    if (i == 9)
                        break;
                    if (i == 0)
                        merchantModel.ShopCategories += categoriesAndSubcategoriesList[i];
                    else
                        merchantModel.ShopCategories += ", " + categoriesAndSubcategoriesList[i];

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
                    merchantModel.OutletList.Add(outletItem);
                    merchantModel.OutletImageUrlList.AddRange(await GetOutletImageList(outlet.Id));
                }
                if (merchantModel.OutletImageUrlList.Count > 10)
                    merchantModel.OutletImageUrlList.RemoveRange(10, merchantModel.OutletImageUrlList.Count - 10);


                return merchantModel;
            }

            //   check if ID belongs to giftee setup
            if (merchantId == 1000)
            {
                merchantModel = GifteeSushiKing();
            }

            else if (merchantId == 1001)
            {
                merchantModel = GifteeLlaollao();
            }

            else if (merchantId == 1002)
            {
                merchantModel = GifteeHokkaido();
            }

            else if (merchantId == 1003)
            {
                merchantModel = GifteeBigApple();
            }

            else if (merchantId == 1004)
            {
                merchantModel = GifteeTeaLive();
            }
            return merchantModel;
        }

        private async Task<Merchants> GetExternalMerchantDetail(string externalShopId, byte externalTypeId)
        {
            var _aggregatorUrl = "";
            var aggregatorUrl = await vodusV2Context.AggregatorApiUrls.AsNoTracking().ToListAsync();
            if (bool.Parse(Environment.GetEnvironmentVariable(EnvironmentKey.USE_LOCAL_AGGREGATOR)) == true)
            {
                _aggregatorUrl = aggregatorUrl.Where(x => x.IsLocalAggregator == true && x.Status == 1).FirstOrDefault().Url;
            }
            else
            {
                _aggregatorUrl = aggregatorUrl.Where(x => x.IsLocalAggregator == false && x.Status == 1).OrderBy(x => x.LastUpdatedAt).First().Url;
            }

            StringContent httpContent = new StringContent(JsonConvert.SerializeObject(new AggregatorParams()
            {
                ExternalShopId = externalShopId,
                ExternalTypeId = externalTypeId

            }), System.Text.Encoding.UTF8, "application/json");
            var httpClient = new HttpClient();
            var result = await httpClient.PostAsync($"{_aggregatorUrl}/v1/shop", httpContent);
            var resultString = await result.Content.ReadAsStringAsync();
            var crawlerResult = JsonConvert.DeserializeObject<LegacyApiResponseViewModel>(resultString);
            if (!crawlerResult.Successful)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<Merchants>(crawlerResult.Data.ToString());

        }


        private Merchants GifteeSushiKing()
        {
            var viewModel = new Merchants
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

        private Merchants GifteeLlaollao()
        {
            var viewModel = new Merchants
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

        private Merchants GifteeHokkaido()
        {
            var viewModel = new Merchants
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

        private Merchants GifteeBigApple()
        {
            var viewModel = new Merchants
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

        private Merchants GifteeTeaLive()
        {
            var viewModel = new Merchants
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

        protected class AggregatorParams
        {
            public string ExternalShopId { get; set; }
            public byte ExternalTypeId { get; set; }
        }

        protected class AggregatorSearchByKeywordQuery
        {
            public string SearchQuery { get; set; }
            public List<int> PriceFilter { get; set; }
            public List<int> LocationFilter { get; set; }

            public int PageNumber { get; set; }
        }

        protected class MerchantResponseModel : ApiResponseViewModel
        {
            public MerchantData Data { get; set; }
        }

        protected class MerchantData
        {
            public Merchants Merchant { get; set; }
        }
        protected class Merchants
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
            public string ExternalShopUsername { get; set; }
            public string ExternalShopId { get; set; }

            public byte ExternalTypeId { get; set; }

            public List<MerchantCarousel> MerchantCarouselList { get; set; }

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

    }
}
