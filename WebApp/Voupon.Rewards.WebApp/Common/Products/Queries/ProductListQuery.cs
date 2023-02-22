using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.Common.Blob.Queries;
using Voupon.Rewards.WebApp.Common.Products.Models;
using Voupon.Rewards.WebApp.Services.Logger;
using Voupon.Rewards.WebApp.ViewModels;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Collections;
using Voupon.Rewards.WebApp.Services.Aggregators.Queries;
using Azure.Core;
using System.Net.Http;
using Microsoft.Extensions.Options;

namespace Voupon.Rewards.WebApp.Common.Products.Queries
{
    public class ProductListQuery : IRequest<ApiResponseViewModel>
    {
        public int? Category { get; set; }
        public int PageNumber { get; set; }
    }
    public class ProductListQueryHandler : IRequestHandler<ProductListQuery, ApiResponseViewModel>
    {
        RewardsDBContext _rewardsDBContext;
        IAzureBlobStorage _azureBlobStorage;
        VodusV2Context _vodusV2Context;
        IConnectionMultiplexer _connectionMultiplexer;
        IOptions<AppSettings> _appSettings;
        string _aggregatorUrl = "";

        public ProductListQueryHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IAzureBlobStorage azureBlobStorage, IConnectionMultiplexer connectionMultiplexer, IOptions<AppSettings> appSettings)
        {
            _azureBlobStorage = azureBlobStorage;
            _rewardsDBContext = rewardsDBContext;
            _vodusV2Context = vodusV2Context;
            _connectionMultiplexer = connectionMultiplexer;
            _appSettings = appSettings;
        }

        public async Task<ApiResponseViewModel> Handle(ProductListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            var redisCache = _connectionMultiplexer.GetDatabase();
            var resultList = new List<SearchProductViewModel>();
            try
            {
                if (request.PageNumber == 1)
                {

                    if (request.Category.HasValue)
                    {
                        var vouponProductListByCategory = redisCache.StringGet("VOUPON_PRODUCTLIST_BY_CATEGORY_" + request.Category.ToString());

                        if (vouponProductListByCategory.HasValue)
                        {
                            response.Successful = true;
                            response.Message = "Get Product List Successfully";
                            response.Data = System.Text.Json.JsonSerializer.Deserialize<List<SearchProductViewModel>>(vouponProductListByCategory);
                            return response;
                        }
                    }

                    var provinces = await _rewardsDBContext.Provinces.AsNoTracking().ToListAsync();
                    var queryable = _rewardsDBContext.Products.
                        AsNoTracking().
                        Include(x => x.ProductOutlets).
                        ThenInclude(x => x.Outlet).
                        Include(x => x.Merchant).
                        Include(x => x.ProductDiscounts).
                        Include(x => x.ProductSubCategory).
                        Include(x => x.ProductCategory).
                        Include(x => x.StatusType).
                        Include(x => x.DealType).
                        Include(x => x.DealExpirations).
                        Where(x => x.Merchant.IsPublished && x.IsActivated && x.IsPublished).AsQueryable();

                    if (request.Category != null)
                    {
                        queryable = queryable.Where(x => x.ProductCategoryId == request.Category);
                    }

                    var groupedItems = queryable.ToList().GroupBy(x => x.MerchantId);
                    var items = new List<Voupon.Database.Postgres.RewardsEntities.Products>();

                    foreach (var grp in groupedItems)
                    {
                        var grpItem = grp.Skip(0).Take(6).ToList();
                        items.AddRange(grpItem);
                    }



                    var rewardRecoList = await _vodusV2Context.ProductAds.AsNoTracking().ToArrayAsync();

                    List<ProductModel> list = new List<ProductModel>();
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
                        newItem.ExternalId = item.ExternalId;
                        newItem.ExternalMerchantId = item.ExternalMerchantId;
                        if (item.ExternalTypeId != null && item.ExternalTypeId.HasValue)
                        {
                            newItem.ExternalTypeId = item.ExternalTypeId.Value;
                        }

                        if (item.ProductOutlets.Select(z => z.Outlet).Any())
                        {
                            newItem.OutletLocation = provinces.Where(x => x.Id == item.ProductOutlets.Select(z => z.Outlet).First().ProvinceId).First().Name;

                        }
                        else
                        {
                            newItem.OutletLocation = "";
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
                    //Weighted Randomization
                    int totalCTR = (int)list.Sum(x => x.CTR.Value * 1000);

                    foreach (var item in list)
                    {
                        Random random = new Random();

                        item.WeightedCTR = random.Next(0, totalCTR);


                        BlobSmallImagesListQuery command = new BlobSmallImagesListQuery()
                        {
                            Id = item.Id,
                            ContainerName = ContainerNameEnum.Products,
                            FilePath = FilePathEnum.Products_Images
                        };

                        var filename = await _azureBlobStorage.ListBlobsAsync(ContainerNameEnum.Products, item.Id + "/" + FilePathEnum.Products_Images);
                        var fileList = new List<string>();
                        foreach (var file in filename)
                        {
                            if (file.StorageUri.PrimaryUri.OriginalString.Contains("small"))
                            {
                                fileList.Add(file.StorageUri.PrimaryUri.OriginalString.Replace("http://", "https://"));
                            }
                            else if (file.StorageUri.PrimaryUri.OriginalString.Contains("normal"))
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
                        ProductImage = (x.ImageFolderUrl != null && x.ImageFolderUrl.Any() ? x.ImageFolderUrl[0] : ""),
                        OutletLocation = x.OutletLocation,

                    }));
                }


                //  Add aggregator items
                var searchText = "";
                if (request.Category == 1)
                {
                    searchText = "Women's fashion";
                }
                else if (request.Category == 2)
                {
                    searchText = "Men's fashion";
                }
                else if (request.Category == 6)
                {
                    searchText = "Beauty, Health and Groceries";
                }
                else if (request.Category == 3)
                {
                    searchText = "Gadgets and Accessories";
                }
                else if (request.Category == 4)
                {
                    searchText = "Home and Family";
                }
                else if (request.Category == 5)
                {
                    searchText = "Leisure and Entertainment";
                }
                else if (request.Category == 7)
                {
                    searchText = "Sports and Outdoor";
                }
                else if (request.Category == 8)
                {
                    searchText = "Vouchers and Others";
                }

                if (!string.IsNullOrEmpty(searchText))
                {
                    var skipSearch = false;
                    var appConfig = await _rewardsDBContext.AppConfig.FirstOrDefaultAsync();
                    if (appConfig == null || !appConfig.IsAggregatorEnabled)
                    {
                        skipSearch = true;
                    }

                    if (!skipSearch)
                    {
                        var aggregatorUrl = await _rewardsDBContext.AggregatorApiUrls.Where(x => x.StatusId == 1 && x.TypeId == 2).ToListAsync();
                        if (_appSettings.Value.App.UseLocalAggregator)
                        {
                            var agg = aggregatorUrl.Where(x => x.IsLocalAggregator == true).FirstOrDefault();
                            if (agg == null)
                            {
                                response.Successful = false;
                                response.Data = null;
                                return response;
                            }
                            _aggregatorUrl = aggregatorUrl.Where(x => x.IsLocalAggregator == true).FirstOrDefault().Url;
                        }
                        else
                        {
                            var agg = aggregatorUrl.Where(x => x.IsLocalAggregator == false).OrderBy(x => x.LastUpdatedAt).FirstOrDefault();

                            if (agg == null)
                            {
                                response.Successful = false;
                                response.Data = null;
                                return response;
                            }

                            agg.LastUpdatedAt = DateTime.Now;
                            _rewardsDBContext.Update(agg);
                            await _rewardsDBContext.SaveChangesAsync();
                            _aggregatorUrl = agg.Url;
                        }

                        var searchQueryModel = new SearchQueryModel
                        {
                            SearchQuery = searchText,
                            PageNumber = 1
                        };

                        StringContent httpContent = new StringContent(JsonConvert.SerializeObject(searchQueryModel), System.Text.Encoding.UTF8, "application/json");
                        var httpClient = new HttpClient();
                        var result = await httpClient.PostAsync($"{_aggregatorUrl}/aggregator/product-search", httpContent);
                        var resultString = await result.Content.ReadAsStringAsync();

                        if (!string.IsNullOrEmpty(resultString))
                        {
                            try
                            {
                                var crawlerResult = JsonConvert.DeserializeObject<ApiResponseViewModel>(resultString);
                                if (crawlerResult.Successful && crawlerResult.Data != null)
                                {
                                    var aggregatorData = JsonConvert.DeserializeObject<List<SearchProductViewModel>>(crawlerResult.Data.ToString());
                                    if (aggregatorData != null && resultList.Any())
                                    {
                                        resultList.AddRange(aggregatorData);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                //  TODO log error
                            }
                        }


                    }
                }


                if (request.PageNumber == 1)
                {
                    redisCache.StringSet("VOUPON_PRODUCTLIST_BY_CATEGORY_" + request.Category.ToString(), System.Text.Json.JsonSerializer.Serialize<List<SearchProductViewModel>>(resultList.Take(14).ToList()));
                }

                response.Successful = true;
                response.Message = "Get Product List Successfully";
                response.Data = resultList;
            }
            catch (Exception ex)
            {
                var errorLogs = new Voupon.Database.Postgres.RewardsEntities.ErrorLogs
                {
                    TypeId = CreateErrorLogCommand.Type.Service,
                    ActionName = "ProductListQuery",
                    ActionRequest = JsonConvert.SerializeObject(request),
                    CreatedAt = DateTime.Now,
                    Errors = ex.ToString()
                };

                await _rewardsDBContext.ErrorLogs.AddAsync(errorLogs);
                await _rewardsDBContext.SaveChangesAsync();

                response.Successful = false;
                response.Data = null;
                response.Message = "Fail to get products";
            }

            return response;
        }

        public class SearchQueryModel
        {
            public string SearchQuery { get; set; }
            public int MinPrice { get; set; }

            public int MaxPrice { get; set; }
            public List<int> LocationFilter { get; set; }

            public int PageNumber { get; set; }
        }

    }
}
