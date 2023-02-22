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
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.API.ViewModels;
using Voupon.API.Util;
using System.ComponentModel;
using Microsoft.Extensions.Options;
using System.Net.Http;

namespace Voupon.API.Functions
{
    public class GetProductSearchFunction
    {
        private readonly RewardsDBContext _rewardsDBContext;
        private readonly VodusV2Context _vodusV2Context;
        private string _aggregatorUrl;
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly IAzureBlobStorage azureBlobStorage;
        public GetProductSearchFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage)
        {
            this._rewardsDBContext = rewardsDBContext;
            this._vodusV2Context = vodusV2Context;
            this.connectionMultiplexer = connectionMultiplexer;
            this.azureBlobStorage = azureBlobStorage;
        }

        [OpenApiOperation(operationId: "Get products search", tags: new[] { "Product" }, Description = "Get products search", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ProductSerachResponseModel), Summary = "Get product search")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If unable to retrieve it from the server")]

        [FunctionName("GetProductSearch")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "product/search")] HttpRequest req, ILogger log)
        {

            var response = new ProductSerachResponseModel
            {
                Data = new ProductData()
            };
            try
            {
                var query = SetQueryParam(req);
                var productList = await GetProductFiltered(query);
                //var crawler = await GetProductCrawler(query);
               // productList.AddRange(crawler);
                response.Code = 0;
                response.Data.IsSuccessful = true;
                response.Data.Message = "Successfully get item";
                response.Data.Products = productList;
                return new OkObjectResult(response); ;
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

        }
        protected async Task<List<SearchProductViewModel>> GetProductCrawler(ProductSearchQuery request)
        {
    
            List<SearchProductViewModel> resultList = new List<SearchProductViewModel>();
            try
            {
                string querySearch = CrawlerQueryProcessor(request);

                var aggregatorUrl = await _rewardsDBContext.AggregatorApiUrls.Where(x => x.StatusId == 1 && x.TypeId == 2).ToListAsync();
                if (bool.Parse(Environment.GetEnvironmentVariable(EnvironmentKey.USE_LOCAL_AGGREGATOR)) == true)
                {

                    var agg = aggregatorUrl.Where(x => x.IsLocalAggregator == true).FirstOrDefault();
                    if (agg == null)
                    {
                        return null;
                    }
                    _aggregatorUrl = aggregatorUrl.Where(x => x.IsLocalAggregator == true).FirstOrDefault().Url;
                }
                else
                {
                    var agg = aggregatorUrl.Where(x => x.IsLocalAggregator == false).OrderBy(x => x.LastUpdatedAt).FirstOrDefault();

                    if (agg == null)
                    {
                        return null;
                    }

                    agg.LastUpdatedAt = DateTime.Now;
                    _rewardsDBContext.Update(agg);
                    await _rewardsDBContext.SaveChangesAsync();
                    _aggregatorUrl = agg.Url;
                }
                
                StringContent httpContent = new StringContent(querySearch, System.Text.Encoding.UTF8, "application/json");
                var httpClient = new HttpClient();
                var result = await httpClient.PostAsync($"{_aggregatorUrl}/aggregator/product-search", httpContent);
                var resultString = await result.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(resultString))
                {
                    /*apiResponseViewModel.Data = null;
                    apiResponseViewModel.Successful = false;*/
                    return null;
                }

                var crawlerResult = JsonConvert.DeserializeObject<CrawlerResult>(resultString);
                
                if (crawlerResult.Successful)
                {
                    var aggregatorResultList = new List<SearchProductViewModel>();
                    var aggregatorData = JsonConvert.DeserializeObject<List<SearchProductViewModel>>(crawlerResult.Data.ToString());
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
            catch (Exception ex)
            {
                var error = ex.ToString();
            }
            return resultList;
        }
        protected async Task<List<SearchProductViewModel>> GetProductFiltered(ProductSearchQuery request )
        {

                var provinces = await _rewardsDBContext.Provinces.AsNoTracking().ToListAsync();

                var queryable = this._rewardsDBContext.Products.AsNoTracking().
                    Include(x => x.ProductOutlets).ThenInclude(x => x.Outlet).
                    Include(x => x.Merchant).
                    Include(x => x.ProductDiscounts).
                    Include(x => x.ProductSubCategory).
                    Include(x => x.ProductCategory).
                    Include(x => x.StatusType).
                    Include(x => x.DealType).
                    Include(x => x.DealExpirations).
                    Where((x => x.IsPublished == true && x.IsActivated == true));

                if (request.IsExternal)
                {
                    queryable = queryable.Where(x => x.ExternalId != null);
                }
                else
                {
                    queryable = queryable.Where(x => x.ExternalId == null);
                }

                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    queryable = queryable.Where(x => x.Title.Contains(request.SearchText));
                }

                if (request.SubCategory.Length > 0 && request.IsCategory)
                {
                    queryable = queryable.Where(x => request.SubCategory.Contains(x.ProductSubCategoryId.Value));
                }

                if (request.IsCategory && request.ProductTypeId == 0)
                {
                    queryable = queryable.Where(x => request.SubCategory.Contains(x.ProductCategoryId.Value));
                }
                if (request.ProductTypeId != 0)
                {
                    queryable = queryable.Where(x => x.ProductCategoryId == request.ProductTypeId);
                }

                if (request.IsPriceFilter)
                {
                    queryable = queryable.Where(x => x.DiscountedPrice.Value >= request.MinPrice && x.DiscountedPrice.Value <= request.MaxPrice);
                }

                if (request.Location.Length > 0)
                {
                    if (!request.Location.Contains(20) || !(request.Location.Contains(18) && request.Location.Contains(17)))
                    {
                        var locationFilter = GetListLocation(request.Location);
                        if (locationFilter.Count > 0)
                        {
                            queryable = queryable.Where(x => locationFilter.Contains((int)x.ProductOutlets.Select(y => y.Outlet.ProvinceId.Value).FirstOrDefault()));
                        }
                    }
                }

                //var test = await queryable.OrderBy(x => x.Title).Skip(offset).Take(limit).ToListAsync();
                var items = await queryable.OrderBy(x => x.Title).Skip(request.Limit).Take(request.Offset).ToListAsync();

                List<ProductModel> list = new List<ProductModel>();
                foreach (var item in items)
                {
                    ProductModel newItem = new ProductModel();
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
                    //newItem.ImageFolderUrl = new List<string>();
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

                    var AzureBlob = new AzureBlob();
                    var imageResult = await AzureBlob.BlobSmallImagesListQuery(new AzureBlob.BlobSmallImagesListModel
                    {
                        AzureBlobStorage = azureBlobStorage,
                        FilePath = FilePathEnum.Products_Images,
                        ContainerName = ContainerNameEnum.Products,
                        GetIListBlobItem = false,
                        Id = newItem.Id
                    });

                    if (imageResult != null && imageResult.Any())
                    {
                        newItem.ProductImage = imageResult[0];
                    }
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
                    list.Add(newItem);
                }

                List<SearchProductViewModel> resultList = new List<SearchProductViewModel>();
                if (list != null && list.Any())
                {
                    resultList.AddRange(list.Select(x => new SearchProductViewModel
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
                        ProductImage = x.ProductImage,
                        OutletLocation = x.OutletLocation,

                    }));
                }


            return resultList;
        }

        protected List<int> GetListLocation(int[] filterLocationList)
        {
            List<ProductModel> filteredLocationList = new List<ProductModel>();
            List<int> filterLocations = new List<int>();
            foreach (var filter in filterLocationList)
            {
                if (filter == 17)
                {
                    if (!filterLocationList.Contains(15))
                        filterLocations.Add(15);
                    if (!filterLocationList.Contains(11))
                        filterLocations.Add(11);
                    if (!filterLocationList.Contains(10))
                        filterLocations.Add(10);
                }
                if (filter == 18)
                {
                    if (!filterLocationList.Contains(1))
                        filterLocations.Add(1);
                    if (!filterLocationList.Contains(2))
                        filterLocations.Add(2);
                    if (!filterLocationList.Contains(3))
                        filterLocations.Add(3);
                    if (!filterLocationList.Contains(4))
                        filterLocations.Add(4);
                    if (!filterLocationList.Contains(5))
                        filterLocations.Add(5);
                    if (!filterLocationList.Contains(6))
                        filterLocations.Add(6);
                    if (!filterLocationList.Contains(7))
                        filterLocations.Add(7);
                    if (!filterLocationList.Contains(8))
                        filterLocations.Add(8);
                    if (!filterLocationList.Contains(9))
                        filterLocations.Add(9);
                    if (!filterLocationList.Contains(12))
                        filterLocations.Add(12);
                    if (!filterLocationList.Contains(13))
                        filterLocations.Add(13);
                    if (!filterLocationList.Contains(14))
                        filterLocations.Add(14);
                    if (!filterLocationList.Contains(16))
                        filterLocations.Add(16);
                }
                else
                {
                    if (!filterLocations.Contains(filter))
                        filterLocations.Add(filter);
                }
            }
            return filterLocations;
        }

        protected ProductSearchQuery SetQueryParam(HttpRequest req) 
        {

            ProductSearchQuery query = new ProductSearchQuery();
            query.SearchText = req.Query["searchText"];

            query.SubCategory = new int[] { };
            if (!String.IsNullOrEmpty(req.Query["subCategory"])) {
                var filter1 = JsonConvert.DeserializeObject<string[]>(req.Query["subCategory"]);

                query.SubCategory = new int[filter1.Length];
                for (int i = 0; i < filter1.Length; i++)
                {
                    query.SubCategory[i] = Int32.Parse(filter1[i]);
                }
            }


            query.IsCategory = bool.Parse(req.Query["isCategory"]);

            int typeId = 0;
            if (int.TryParse(req.Query["productTypeId"], out typeId)) 
            {
                query.ProductTypeId = typeId;
            }
            //string.IsNullOrEmpty()
            query.IsPriceFilter = bool.Parse(req.Query["isPriceFilter"]) ;

            int _priceMin = 0;
            if (int.TryParse(req.Query["priceMin"], out _priceMin))
            {
                query.MinPrice = _priceMin;
            }

            int _priceMax = 0;
            if (int.TryParse(req.Query["priceMax"], out _priceMax))
            {
                query.MaxPrice = _priceMax;
            }
            else 
            {
                query.MaxPrice = int.MaxValue;
            }

            query.Location = new int[] { };
            if (!String.IsNullOrEmpty(req.Query["locationFilter"]))
            {
                var filter3 = JsonConvert.DeserializeObject<string[]>(req.Query["locationFilter"]);
                query.Location = new int[filter3.Length];
                for (int i = 0; i < filter3.Length; i++)
                {
                    query.Location[i] = Int32.Parse(filter3[i]);
                }
            }

            int _limit = 1;
            if (int.TryParse(req.Query["limit"], out _limit))
            {
                query.Limit = _limit;
            }

            int _offset = 0;
            if (int.TryParse(req.Query["offset"], out _offset))
            {
                query.Offset = _offset;
            }


            if (query.SearchText != "")
            {
                var serachType = GetEnumValueByDescription(query.SearchText.ToUpper());
                var productTypeId = serachType;
                if (productTypeId != 0)
                {
                    query.SearchText = "";
                    query.IsCategory = true;
                    query.ProductTypeId = productTypeId;
                }
            }

            query.IsExternal = false;
            if (!String.IsNullOrEmpty(req.Query["IsExternal"]))
            {
                query.IsExternal = Convert.ToBoolean(req.Query["IsExternal"]);
            }

            return query;
        }

        protected class ProductSerachResponseModel : ApiResponseViewModel
        {
            public ProductData Data { get; set; }

        }

        protected class ProductSearchQuery 
        {
            public string SearchText { get; set; }
            public int[] SubCategory { get; set; }

            public bool IsCategory { get; set; }
            public bool IsExternal { get; set; }

            public int ProductTypeId { get; set; }

            public bool IsPriceFilter { get; set; }

            public int MinPrice { get; set; }

            public int MaxPrice { get; set; }

            public int[] Location { get; set; }

            public int Limit { get; set; }
            public int Offset { get; set; }
        }

        protected class ProductModel
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

            public string ProductImage { get; set; }
            //public List<string> ImageFolderUrl { get; set; }
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
        protected class ProductData
        {
            public List<SearchProductViewModel> Products { get; set; }
            public bool IsSuccessful { get; set; }

            public string Message { get; set; }
        }


        protected int GetEnumValueByDescription(string searchText)
        {
            if (searchText.ToUpper() == "Women's fashion".ToUpper()) 
            {
                return 1;
            }
            if (searchText.ToUpper() == "Men's fashion".ToUpper()) 
            {
                return 2;
            }
            if (searchText.ToUpper() == "Gadgets and Accessories".ToUpper())
            {
                return 3;
            }
            if (searchText.ToUpper() == "Home and Family".ToUpper())
            {
                return 4;
            }
            if (searchText.ToUpper() == "Leisure and Entertainment".ToUpper())
            {
                return 5;
            }
            if (searchText.ToUpper() == "Beauty, Health and Groceries".ToUpper())
            {
                return 6;
            }
            if (searchText.ToUpper() == "Sports and Outdoor".ToUpper())
            {
                return 7;
            }
            if (searchText.ToUpper() == "Vouchers and Others".ToUpper())
            {
                return 8;
            }
            return 0;
        }

        protected class CrawlerResult
        {
            private static readonly object EMPTY = new object();
            public bool Successful { get; set; }
            public int Code { get; set; }
            public string Message { get; set; }
            public object Data { get; set; }

            public CrawlerResult()
            {
                Successful = false;
                Message = null;
                Code = -1;
                Data = EMPTY;
            }
        }

        protected class SearchProductViewModel
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

        protected class CrawlerSearchFilter
        {
           
            public int[] SubcategoriesList { get; set; }
            public int MinPrice { get; set; }

            public int MaxPrice { get; set; }
            public List<int> LocationFilter { get; set; }

            public int PageNumber { get; set; }
        }

        protected class CrawlerSearchFilterWithText  
        {
            public string SearchQuery { get; set; }
            public int MinPrice { get; set; }

            public int MaxPrice { get; set; }
            public List<int> LocationFilter { get; set; }

            public int PageNumber { get; set; }
        }
        
        protected string CrawlerQueryProcessor(ProductSearchQuery request) 
        {
            var queryFilter = new CrawlerSearchFilter();
            var queryFilterWithText = new CrawlerSearchFilterWithText();
            string querySearch = "";
            if (request.Limit == 0)
            {
                request.Limit = 1;
            }
            if (request.ProductTypeId != 0)
            {
                if (request.ProductTypeId == 1)
                {
                    request.SearchText = "Women's fashion";
                }
                else if (request.ProductTypeId == 2)
                {
                    request.SearchText = "Men's fashion";
                }
                else if (request.ProductTypeId == 6)
                {
                    request.SearchText = "Beauty, Health and Groceries";
                }
                else if (request.ProductTypeId == 3)
                {
                    request.SearchText = "Gadgets and Accessories";
                }
                else if (request.ProductTypeId == 4)
                {
                    request.SearchText = "Home and Family";
                }
                else if (request.ProductTypeId == 5)
                {
                    request.SearchText = "Leisure and Entertainment";
                }
                else if (request.ProductTypeId == 7)
                {
                    request.SearchText = "Sports and Outdoor";
                }
                else if (request.ProductTypeId == 8)
                {
                    request.SearchText = "Vouchers and Others";
                }
            }
            
            if (request.SearchText != "")
            {
                queryFilterWithText.SearchQuery = request.SearchText;
                queryFilterWithText.MinPrice = request.MinPrice;
                queryFilterWithText.MaxPrice = request.MaxPrice;
                queryFilterWithText.LocationFilter = request.Location.ToList();
                decimal pageNumber = (request.Offset / request.Limit);
                queryFilterWithText.PageNumber = (int)Math.Round(pageNumber);
                querySearch = JsonConvert.SerializeObject(queryFilterWithText);
            }
            else
            {
                queryFilter.SubcategoriesList = request.SubCategory;
                queryFilter.MinPrice = request.MinPrice;
                queryFilter.MaxPrice = request.MaxPrice;
                queryFilter.LocationFilter = request.Location.ToList();
                decimal pageNumber = (request.Offset / request.Limit);
                queryFilter.PageNumber = (int)Math.Round(pageNumber);
                querySearch = JsonConvert.SerializeObject(queryFilter);
            }
            return querySearch;
        }
    }
}
