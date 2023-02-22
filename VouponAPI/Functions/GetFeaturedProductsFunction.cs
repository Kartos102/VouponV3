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

namespace Voupon.API.Functions
{
    public class GetFeaturedProductsFunction
    {
        private readonly RewardsDBContext rewardsDBContext;
        private readonly VodusV2Context vodusV2Context;
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly IAzureBlobStorage azureBlobStorage;
        const string REDIS_FEATURED_PRODUCTS = "FEATURED_PRODUCTS";

        public GetFeaturedProductsFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
            this.connectionMultiplexer = connectionMultiplexer;
            this.azureBlobStorage = azureBlobStorage;
        }

        [OpenApiOperation(operationId: "Get featured products", tags: new[] { "Products" }, Description = "Get featured products", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(FeaturedProductResponseModel), Summary = "Get the list of featured products by category")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If no category is supplied")]

        [FunctionName("GetFeaturedProductFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/featured")] HttpRequest req, ILogger log)
        {
            var apiResponseViewModel = new FeaturedProductResponseModel
            {
                Data = new FeaturedProductData()
            };

            var redisCache = connectionMultiplexer.GetDatabase();
            var redisProductListAds = redisCache.StringGet(REDIS_FEATURED_PRODUCTS);
            List<RewardsAdsProduct> productlist = new List<RewardsAdsProduct>();
            var productModel = new List<Products>();

            try
            {
                var vouponBaseUrl = "";
                var vPointCap = 0;
                //int vPointCap = 0;// Int16.Parse(appSettings.Value.RewardAds.vPointCap);
                if (!redisProductListAds.HasValue)
                {
                    var items = await rewardsDBContext.Products.AsNoTracking().Include(x => x.ProductOutlets).ThenInclude(x => x.Outlet).Include(x => x.Merchant).Include(x => x.ProductDiscounts).Include(x => x.ProductSubCategory).Include(x => x.ProductCategory).Include(x => x.StatusType).Include(x => x.DealType).Include(x => x.DealExpirations).Where(x => x.Merchant.IsPublished && x.IsActivated && x.IsPublished).ToListAsync();

                    var rewardRecoList = await vodusV2Context.ProductAds.AsNoTracking().ToArrayAsync();

                    List<Products> list = new List<Products>();
                    var tempList = new List<Products>();
                    foreach (var item in items)
                    {
                        decimal productCrt = 0;
                        var productAd = rewardRecoList.Where(x => x.ProductId == item.Id).FirstOrDefault();
                        if (productAd != null)
                        {
                            if(productAd.CTR.HasValue)
                            {
                                productCrt = productAd.CTR.Value;
                            }

                        }
                        Products newItem = new Products();
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
                        newItem.ImageFolderUrl = new List<string>();
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
                        tempList.Add(newItem);
                    }

                    var rewardsAdsProductList = rewardsDBContext.Products.Where(x => x.IsPublished && x.IsActivated && x.PointsRequired <= 500 && x.Merchant.IsTestAccount == false && x.AvailableQuantity > 0).Include(x => x.DealExpirations).Include(x => x.Merchant).ToList();
                    var rewardAdsIds = vodusV2Context.ProductAds.Where(x => x.IsActive == true).ToList().Select(x => x.ProductId).ToList();
                    if (rewardAdsIds.Count > 0)
                        rewardsAdsProductList = rewardsAdsProductList.Where(x => rewardAdsIds.Contains(x.Id)).ToList();
                    foreach (var product in rewardsAdsProductList)
                    {
                        var rewardAd = vodusV2Context.ProductAds.Where(x => x.ProductId == product.Id).FirstOrDefault();
                        RewardsAdsProduct newProduct = new RewardsAdsProduct();
                        newProduct.MerchantName = product.Merchant.DisplayName;
                        newProduct.ImageUrl = "https://cdn11.bigcommerce.com/s-t04x4i8lh4/images/stencil/2048x2048/products/973/7815/Mystery-Gift__15205.1571852864.jpg?c=2";
                        newProduct.ProductTitle = product.Title;
                        newProduct.ProductUrl = $"{vouponBaseUrl}/Product/{product.Id}";
                        if (rewardAd != null)
                            newProduct.ProductRecoId = rewardAd.Id;

                        var filename = await azureBlobStorage.ListBlobsAsync(ContainerNameEnum.Products, product.Id + "/" + FilePathEnum.Products_Images);
                        var fileList = new List<string>();

                        var smallImages = filename.Where(x => x.StorageUri.PrimaryUri.OriginalString.Contains("small_"));
                        if (smallImages != null && smallImages.Any())
                        {
                            newProduct.ImageUrl = smallImages.First().StorageUri.PrimaryUri.OriginalString.Replace("http://", "https://");
                        }
                        else
                        {
                            foreach (var file in filename)
                            {
                                if (file.StorageUri.PrimaryUri.OriginalString.Contains("small"))
                                {
                                    newProduct.ImageUrl = file.StorageUri.PrimaryUri.OriginalString.Replace("http://", "https://");
                                    break;
                                    //fileList.Add(file.StorageUri.PrimaryUri.OriginalString);
                                }
                                else if (!file.StorageUri.PrimaryUri.OriginalString.Contains("big") && !file.StorageUri.PrimaryUri.OriginalString.Contains("normal") && !file.StorageUri.PrimaryUri.OriginalString.Contains("org"))
                                {
                                    newProduct.ImageUrl = file.StorageUri.PrimaryUri.OriginalString.Replace("http://", "https://");
                                    break;
                                    //fileList.Add(file.StorageUri.PrimaryUri.OriginalString);
                                }
                            }
                        }

                        if (product.DealTypeId == 1 || product.DealTypeId == 3)
                        {
                            newProduct.PointRequired = product.PointsRequired.Value;
                            newProduct.DiscountedPrice = "";
                            newProduct.Price = "";
                        }
                        else
                        {
                            var discount = rewardsDBContext.ProductDiscounts.Where(x => x.ProductId == product.Id && x.IsActivated == true).OrderByDescending(x => x.PointRequired).ToList();
                            if (discount != null && discount.Count > 0)
                            {
                                /*
                                while (discount.First().PointRequired > vPointCap)
                                {
                                    discount.RemoveAt(0);
                                }
                                */
                                newProduct.PointRequired = discount.First().PointRequired;
                                //newProduct.Price = "RM " + (product.DiscountedPrice.HasValue ? product.DiscountedPrice.Value.ToString("F2") : product.Price.Value.ToString("F2"));
                                newProduct.Price = "RM " + product.Price.Value.ToString("F2");


                                var lowestPrice = product.DiscountedPrice.HasValue ? product.DiscountedPrice.Value : product.Price.Value;
                                newProduct.PriceValue = discount.First().PriceValue;
                                newProduct.PercentageValue = discount.First().PercentageValue;
                                var extraDiscount = product.Price.Value - lowestPrice;

                                if (discount.First().DiscountTypeId == 1)
                                {
                                    newProduct.TotalDiscount = ((discount.First().PercentageValue) > 100 ? 100 : (discount.First().PercentageValue)).ToString("F0") + "% OFF";
                                    newProduct.DiscountedPrice = "RM " + ((lowestPrice * (1 - ((discount.First().PercentageValue + extraDiscount) / 100))) < 0 ? 0 : (lowestPrice * (1 - ((discount.First().PercentageValue + extraDiscount) / 100)))).ToString("F2");
                                }
                                else
                                {
                                    newProduct.TotalDiscount = "RM" + (((discount.First().PriceValue + extraDiscount)) > product.Price.Value ? product.Price.Value : (discount.First().PriceValue + extraDiscount)) + " OFF";
                                    newProduct.DiscountedPrice = "RM " + ((lowestPrice - (discount.First().PriceValue + extraDiscount)) < 0 ? 0 : (lowestPrice - (discount.First().PriceValue + extraDiscount))).ToString("F2");
                                }
                                productlist.Add(newProduct);
                            }
                        }
                    }

                    var newList = tempList.Where(x => x.IsPublished == true && x.IsActivated == true && (((x.ExpirationTypeId == 1 || x.ExpirationTypeId == 2)) || x.ExpirationTypeId == 4 || x.ExpirationTypeId == 5)).OrderBy(x => x.Title).ToList();
                    List<Products> matchList = new List<Products>();

                    foreach (var product in productlist)
                    {
                        int Id = Int32.Parse(product.ProductUrl.Split("/").Last());
                        var matchItem = newList.FirstOrDefault(x => x.Id == Id);
                        if (matchItem != null)
                        {
                            matchList.Add(matchItem);
                        }
                    }
                    if (matchList.Count > 12)
                        matchList = matchList.OrderBy(x => Guid.NewGuid()).Take(12).ToList();
                    foreach (var item in matchList)
                    {

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
                        item.ImageFolderUrl = fileList;
                    }

                    if (matchList != null && matchList.Any())
                    {
                        redisCache.StringSet(REDIS_FEATURED_PRODUCTS, System.Text.Json.JsonSerializer.Serialize<List<Products>>(matchList));
                    }
                    else
                    {
                        if (newList != null && newList.Any())
                        {
                            var filteredList = newList.Count > 12 ? newList.Take(12).ToList() : newList;
                            foreach (var item in filteredList)
                            {
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
                                item.ImageFolderUrl = fileList;
                            }
                            apiResponseViewModel.Data.Products = filteredList;
                        }
                    }

                }
                else
                {
                    apiResponseViewModel.Data.Products = System.Text.Json.JsonSerializer.Deserialize<List<Products>>(redisProductListAds);
                }
            }
            catch (Exception ex)
            {
                //  log
                return new BadRequestObjectResult(new ApiResponseViewModel
                {
                    ErrorMessage = "Fail to get featured products"
                });
            }

            return new OkObjectResult(apiResponseViewModel);
        }

        protected class AggregatorSearchByKeywordQuery
        {
            public string SearchQuery { get; set; }
            public List<int> PriceFilter { get; set; }
            public List<int> LocationFilter { get; set; }

            public int PageNumber { get; set; }
        }

        private class FeaturedProductResponseModel : ApiResponseViewModel
        {
            public FeaturedProductData Data { get; set; }
        }

        private class FeaturedProductData
        {
            public List<Products> Products { get; set; }
        }

        private class Products
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
        private class RewardsAdsProduct
        {
            public decimal PriceValue { get; set; }
            public decimal PercentageValue { get; set; }
            public int PointRequired { get; set; }
            public string ImageUrl { get; set; }
            public string ProductUrl { get; set; }
            public string ProductTitle { get; set; }
            public string MerchantName { get; set; }
            public string DiscountedPrice { get; set; }
            public string Price { get; set; }
            public string TotalDiscount { get; set; }
            public int? ProductRecoId { get; set; }

        }
    }
}
