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
    public class GetProductsByCategoryFunction
    {
        private readonly RewardsDBContext rewardsDBContext;
        private readonly VodusV2Context vodusV2Context;
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly IAzureBlobStorage azureBlobStorage;

        public GetProductsByCategoryFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
            this.connectionMultiplexer = connectionMultiplexer;
            this.azureBlobStorage = azureBlobStorage;
        }

        [OpenApiOperation(operationId: "Get products by category", tags: new[] { "Products" }, Description = "Get products by category.<br/>Available category 1 = Women's fashion, 2= Men's fashion, 3 = Gadgets, 4 = home, 5 = entertainment, 6 = beauty, 7 = sports, 8 = vouchers", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "categoryid", In = ParameterLocation.Query, Required = true, Type = typeof(byte), Summary = "Category id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "page",  In = ParameterLocation.Query, Required = false, Type = typeof(int), Summary = "Page number, will be set to 1 if no value is supplied", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ProductByCategoryResponseModel), Summary = "The paginated result of products")]       
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If no category is supplied")]

        [FunctionName("GetProductsByCategoryFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products")] HttpRequest req, ILogger log)
        {
            var response = new ProductByCategoryResponseModel();
            try
            {
                var categoryId = 0;
                if (!int.TryParse(req.Query["categoryId"], out categoryId))
                {
                    //  error
                    return new BadRequestObjectResult(new ApiResponseViewModel
                    {
                        Code = -1,
                        ErrorMessage = "Invalid category id [001]"
                    });
                }

                if (categoryId < 0 || categoryId > 8)
                {
                    return new BadRequestObjectResult(new ApiResponseViewModel
                    {
                        Code = -1,
                        ErrorMessage = "Invalid category id [002]"
                    });
                }

                var redisCache = connectionMultiplexer.GetDatabase();

                var vouponProductListByCategory = redisCache.StringGet("TEST_VOUPON_PRODUCTLIST_BY_CATEGORY_" + categoryId);

                if (vouponProductListByCategory.HasValue)
                {
                    response.Data = new ProductByCategoryData();
                    response.Data.Products = System.Text.Json.JsonSerializer.Deserialize<List<Products>>(vouponProductListByCategory);
                    return new OkObjectResult(response);
                }

                var provinces = await rewardsDBContext.Provinces.AsNoTracking().ToListAsync();
                var items = new List<Voupon.Database.Postgres.RewardsEntities.Products>();
                if (categoryId > 0)
                {
                    items = await rewardsDBContext.Products.AsNoTracking().Include(x => x.ProductOutlets).ThenInclude(x => x.Outlet).Include(x => x.Merchant).Include(x => x.ProductDiscounts).Include(x => x.ProductSubCategory).Include(x => x.ProductCategory).Include(x => x.StatusType).Include(x => x.DealType).Include(x => x.DealExpirations).Where(x => x.Merchant.IsPublished && x.IsActivated && x.IsPublished && x.ProductCategoryId == categoryId).ToListAsync();
                }
                else
                {
                    items = await rewardsDBContext.Products.AsNoTracking().Include(x => x.ProductOutlets).ThenInclude(x => x.Outlet).Include(x => x.Merchant).Include(x => x.ProductDiscounts).Include(x => x.ProductSubCategory).Include(x => x.ProductCategory).Include(x => x.StatusType).Include(x => x.DealType).Include(x => x.DealExpirations).Where(x => x.Merchant.IsPublished && x.IsActivated && x.IsPublished).ToListAsync();
                }
                var rewardRecoList = await vodusV2Context.ProductAds.AsNoTracking().ToArrayAsync();

                List<TempProducts> list = new List<TempProducts>();
                foreach (var item in items)
                {
                    decimal productCrt = 0;
                    var productAd = rewardRecoList.Where(x => x.ProductId == item.Id).FirstOrDefault();
                    if (productAd != null)
                    {
                        if (productAd.CTR.HasValue)
                        {
                            productCrt = productAd.CTR.Value;
                        }
                    }
                    var newItem = new TempProducts();
                    newItem.Id = item.Id;
                    newItem.MerchantId = item.MerchantId;
                    newItem.MerchantCode = item.Merchant.Code;
                    newItem.MerchantName = item.Merchant.DisplayName;
                    newItem.Title = item.Title;
                    newItem.Subtitle = item.Subtitle;
                    newItem.Description = item.Description;
                    newItem.AdditionInfo = item.AdditionInfo;
                    newItem.FinePrintInfo = item.FinePrintInfo;
                    newItem.RedemptionInfo = item.RedemptionInfo;
                    newItem.ImageFolderUrl = await GetFromBlob(item.Id, ContainerNameEnum.Products);
                    newItem.ProductCategoryId = item.ProductCategoryId;
                    newItem.ProductCategory = item.ProductCategory != null ? item.ProductCategory.Name : "";
                    newItem.ProductSubCategoryId = item.ProductSubCategoryId;
                    newItem.ProductSubCategory = item.ProductSubCategory != null ? item.ProductSubCategory.Name : "";
                    newItem.DealTypeId = item.DealTypeId;
                    newItem.DealType = item.DealType != null ? item.DealType.Name : "";
                    newItem.StartDate = item.StartDate;
                    newItem.EndDate = item.EndDate;
                    newItem.Price = item.Price;
                    newItem.DiscountedPrice = item.DiscountedPrice;
                    newItem.DiscountRate = item.DiscountRate;
                    newItem.PointsRequired = item.PointsRequired;
                    newItem.AvailableQuantity = item.AvailableQuantity;
                    newItem.DealExpirationId = item.DealExpirationId;
                    newItem.LuckyDrawId = item.LuckyDrawId;
                    newItem.StatusTypeId = item.StatusTypeId;
                    newItem.StatusType = item.StatusType.Name;
                    newItem.IsActivated = item.IsActivated;
                    newItem.CreatedAt = item.CreatedAt;
                    newItem.CreatedByUserId = item.CreatedByUserId;
                    newItem.IsActivated = item.IsActivated;
                    newItem.LastUpdatedAt = item.LastUpdatedAt;
                    newItem.LastUpdatedByUser = item.LastUpdatedByUser;
                    newItem.TotalBought = item.TotalBought;
                    newItem.Remarks = item.Remarks;
                    newItem.Rating = item.Rating;
                    newItem.IsPublished = item.IsPublished;
                    newItem.ThirdPartyProductId = item.ThirdPartyProductId;
                    newItem.ThirdPartyTypeId = item.ThirdPartyTypeId;
                    newItem.CTR = productCrt;

                    if (item.ProductOutlets.Select(z => z.Outlet).Any())
                    {
                        newItem.OutletLocation = provinces.Where(x => x.Id == item.ProductOutlets.Select(z => z.Outlet).First().ProvinceId).First().Name;

                    }
                    else
                    {
                        newItem.OutletLocation = "Global";
                    }

                    if (item.DealExpiration != null)
                    {
                        newItem.ExpirationTypeId = item.DealExpiration.ExpirationTypeId;
                    }
                    else
                    {
                        newItem.ExpirationTypeId = 5;
                    }
                    //check for null values
                    if (newItem.Price == null || newItem.DiscountRate == null || newItem.DiscountedPrice == null)
                    {
                        continue;
                    }
                    // Adding the Higher discount value to the discount rate alonge with the difference between price and discounted price
                    if (item.ProductDiscounts != null && item.ProductDiscounts.Count > 0)
                    {
                        var higherDiscount = item.ProductDiscounts.OrderByDescending(x => x.PointRequired).ToList().FirstOrDefault();

                        if (newItem.Price != 0 && newItem.DiscountedPrice != 0)
                        {
                            if (higherDiscount.DiscountTypeId == 1)
                            {
                                var totalDiscountPrice = ((newItem.DiscountedPrice * higherDiscount.PercentageValue) / 100) + (newItem.Price - newItem.DiscountedPrice);
                                newItem.DiscountRate = (int)(totalDiscountPrice * 100 / newItem.Price);
                                newItem.DiscountedPrice = newItem.Price - totalDiscountPrice;
                            }
                            else
                            {
                                var totalDiscountPrice = (higherDiscount.PriceValue) + (newItem.Price - newItem.DiscountedPrice);
                                newItem.DiscountRate = (int)(totalDiscountPrice * 100 / newItem.Price);
                                newItem.DiscountedPrice = newItem.Price - totalDiscountPrice;
                            }
                        }
                    }

                    newItem.OutletName = item.ProductOutlets.Select(x => x.Outlet.Name).ToList();
                    newItem.OutletProvince = item.ProductOutlets.Select(x => x.Outlet.ProvinceId).ToList();
                    newItem.TotalOutlets = item.ProductOutlets.Count();
                    newItem.Rating = item.Rating;
                    newItem.TotalBought = (item.TotalBought.HasValue ? item.TotalBought.Value : 0);
                    list.Add(newItem);
                }
                //Weighted Randomization
                int totalCTR = (int)list.Sum(x => x.CTR.Value * 1000);

                foreach (var item in list)
                {
                    Random random = new Random();

                    item.WeightedCTR = random.Next(0, totalCTR);

                    var filename = await azureBlobStorage.ListBlobsAsync(ContainerNameEnum.Products, item.Id + "/" + FilePathEnum.Products_Images);
                    var fileList = new List<string>();
                    foreach (var file in filename)
                    {
                        if (file.StorageUri.PrimaryUri.OriginalString.Contains("small"))
                        {
                            fileList.Add(file.StorageUri.PrimaryUri.OriginalString.Replace("http://", "https://"));
                        }
                        else if (!file.StorageUri.PrimaryUri.OriginalString.Contains("big") && !file.StorageUri.PrimaryUri.OriginalString.Contains("normal") && !file.StorageUri.PrimaryUri.OriginalString.Contains("org"))
                        {
                            fileList.Add(file.StorageUri.PrimaryUri.OriginalString.Replace("http://", "https://"));
                        }

                    }

                }

                var resultList = new List<Products>();

                resultList.AddRange(list.Select(x => new Products
                {
                    DealExpirationId = x.DealExpirationId,
                    DealType = x.DealType,
                    DealTypeId = x.DealTypeId,
                    DiscountedPrice = x.DiscountedPrice,
                    DiscountRate = x.DiscountRate,
                    ExpirationTypeId = x.ExpirationTypeId,
                    Id = x.Id,
                    PointsRequired = x.PointsRequired,
                    Price = x.Price,
                    Rating = x.Rating,
                    Title = x.Title,
                    TotalSold = (x.TotalBought.HasValue ? (int)x.TotalBought : 0),
                    ProductSubCategory = x.ProductSubCategory,
                    ProductSubCategoryId = x.ProductSubCategoryId,
                    ProductCategoryId = x.ProductCategoryId,
                    ProductImage = x.ImageFolderUrl.Count > 0 ? x.ImageFolderUrl[0] : "",
                    OutletLocation = x.OutletLocation,

                }));



                //  Add aggregator items
                var searchText = "";
                if (categoryId == 1)
                {
                    searchText = "Women's fashion";
                }
                else if (categoryId == 2)
                {
                    searchText = "Men's fashion";
                }
                else if (categoryId == 6)
                {
                    searchText = "Beauty, Health and Groceries";
                }
                else if (categoryId == 3)
                {
                    searchText = "Gadgets and Accessories";
                }
                else if (categoryId == 4)
                {
                    searchText = "Home and Family";
                }
                else if (categoryId == 5)
                {
                    searchText = "Leisure and Entertainment";
                }
                else if (categoryId == 7)
                {
                    searchText = "Sports and Outdoor";
                }
                else if (categoryId == 8)
                {
                    searchText = "Vouchers and Others";
                }

                if (!string.IsNullOrEmpty(searchText))
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

                    var aggregatorRequest = new AggregatorSearchByKeywordQuery
                    {

                        SearchQuery = searchText,
                        PageNumber = 1
                    };

                    var httpContent = new StringContent(JsonConvert.SerializeObject(aggregatorRequest), System.Text.Encoding.UTF8, "application/json");
                    var httpClient = new HttpClient();
                    var result = await httpClient.PostAsync($"{_aggregatorUrl}/v1/search", httpContent);
                    var resultString = await result.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(resultString))
                    {
                        var crawlerResult = JsonConvert.DeserializeObject<LegacyApiResponseViewModel>(resultString);

                        if (crawlerResult.Successful)
                        {
                            var aggregatorResultList = new List<Products>();
                            var aggregatorData = JsonConvert.DeserializeObject<List<Products>>(crawlerResult.Data.ToString());
                            if (aggregatorData != null && aggregatorData.Any())
                            {
                                aggregatorResultList.AddRange(aggregatorData);
                            }

                            if (aggregatorResultList != null && aggregatorResultList.Any())
                            {
                                aggregatorResultList.OrderBy(x => x.Price);
                                resultList.AddRange(aggregatorResultList);
                            }
                        }
                    }
                }

                redisCache.StringSet("TEST_VOUPON_PRODUCTLIST_BY_CATEGORY_" + categoryId, System.Text.Json.JsonSerializer.Serialize<List<Products>>(resultList.Take(15).ToList()));

                response.Data = new ProductByCategoryData();
                response.Data.Products = resultList;
            }
            catch (Exception ex)
            {
                //  log
                return new BadRequestObjectResult(new ApiResponseViewModel
                {
                    Code = -1,
                    ErrorMessage = "Fail to get data [099]"
                });

            }
            return new OkObjectResult(response);
        }

        public async Task<List<string>> GetFromBlob(int id, string containerName)
        {
            var AzureBlob = new AzureBlob();
            var imageResult = await AzureBlob.BlobSmallImagesListQuery(new AzureBlob.BlobSmallImagesListModel
            {
                AzureBlobStorage = azureBlobStorage,
                FilePath = FilePathEnum.Products_Images,
                ContainerName = containerName,//ContainerNameEnum.Products,
                GetIListBlobItem = false,
                Id = id
            });

            if (imageResult == null)
            {
                imageResult = new List<string>() { "images/not-available-stamp.jpg" };
                //string imageUrl = ;

            }

            return imageResult;
        }
        protected class AggregatorSearchByKeywordQuery
        {
            public string SearchQuery { get; set; }
            public List<int> PriceFilter { get; set; }
            public List<int> LocationFilter { get; set; }

            public int PageNumber { get; set; }
        }

        protected class ProductByCategoryResponseModel : ApiResponseViewModel
        {
            public ProductByCategoryData Data { get; set; }
        }

        protected class ProductByCategoryData
        {
            public List<Products> Products { get; set; }
        }

        protected class TempProducts
        {
            public int Id { get; set; }
            public int MerchantId { get; set; }
            public string MerchantCode { get; set; }
            public string MerchantName { get; set; }
            public string Title { get; set; }
            public string Subtitle { get; set; }
            public string Description { get; set; }
            public string AdditionInfo { get; set; }
            public string FinePrintInfo { get; set; }
            public string RedemptionInfo { get; set; }
            public List<string> ImageFolderUrl { get; set; }
            public int? ProductCategoryId { get; set; }
            public string ProductCategory { get; set; }
            public int? ProductSubCategoryId { get; set; }
            public string ProductSubCategory { get; set; }
            public int? DealTypeId { get; set; }
            public string DealType { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public decimal? Price { get; set; }
            public decimal? DiscountedPrice { get; set; }
            public int? DiscountRate { get; set; }
            public int? PointsRequired { get; set; }
            public int? AvailableQuantity { get; set; }
            public int? DealExpirationId { get; set; }
            public int? ExpirationTypeId { get; set; }
            public int? LuckyDrawId { get; set; }
            public int StatusTypeId { get; set; }
            public string StatusType { get; set; }
            public bool IsActivated { get; set; }
            public bool IsPublished { get; set; }
            public DateTime CreatedAt { get; set; }
            public Guid CreatedByUserId { get; set; }
            public DateTime? LastUpdatedAt { get; set; }
            public Guid? LastUpdatedByUser { get; set; }
            public int? TotalBought { get; set; }
            public string Remarks { get; set; }
            public List<string> OutletName { get; set; }
            public List<int?> OutletProvince { get; set; }
            public int TotalOutlets { get; set; }
            public decimal? CTR { get; set; }
            public int WeightedCTR { get; set; }
            public decimal Rating { get; set; }
            public Guid? ThirdPartyTypeId { get; set; }
            public Guid? ThirdPartyProductId { get; set; }

            public string OutletLocation { get; set; }
        }

        protected class Products
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string ProductImage { get; set; }
            public int? ProductCategoryId { get; set; }
            public string ProductCategory { get; set; }
            public int? ProductSubCategoryId { get; set; }
            public string ProductSubCategory { get; set; }
            public int? DealTypeId { get; set; }
            public string DealType { get; set; }
            public decimal? Price { get; set; }
            public decimal? DiscountedPrice { get; set; }
            public int? DiscountRate { get; set; }
            public int? PointsRequired { get; set; }
            public int? DealExpirationId { get; set; }
            public int? ExpirationTypeId { get; set; }

            public int TotalSold { get; set; }
            public decimal Rating { get; set; }
            public byte ExternalTypeId { get; set; }
            public string ExternalShopId { get; set; }

            public string ExternalItemId { get; set; }

            public string ExternalUrl { get; set; }
            public string OutletLocation { get; set; }

            public decimal? DiscountedPriceMin { get; set; }

            public decimal? DiscountedPriceMax { get; set; }

            public decimal? BeforeDiscountedPriceMin { get; set; }

            public decimal? BeforeDiscountedPriceMax { get; set; }

            public bool IsOriginalGuaranteeProduct { get; set; }

            public string Brand { get; set; }

            public string Description { get; set; }

            public string Language { get; set; }

            public decimal ShippingCost { get; set; }
        }
    }
}
