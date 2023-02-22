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

namespace Voupon.Rewards.WebApp.Common.Products.Queries
{
    public class ProductListByProvinceQuery : IRequest<ApiResponseViewModel>
    {
        public int ProvinceId { get; set; }
        public class ProductListByProvinceQueryHandler : IRequestHandler<ProductListByProvinceQuery, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            VodusV2Context vodusV2Context;
            private readonly IConnectionMultiplexer connectionMultiplexer;
            private IDatabase redisCache;
            private readonly IAzureBlobStorage azureBlobStorage;

            public ProductListByProvinceQueryHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.vodusV2Context = vodusV2Context;
                this.connectionMultiplexer = connectionMultiplexer;
                this.azureBlobStorage = azureBlobStorage;

            }

            public async Task<ApiResponseViewModel> Handle(ProductListByProvinceQuery request, CancellationToken cancellationToken)
            {
                ApiResponseViewModel response = new ApiResponseViewModel();
                var redisKeyProductListByProvince = "ProductListByProvince_" + request.ProvinceId;
                try
                {
                    redisCache = connectionMultiplexer.GetDatabase();
                    List<ProductModel> list = new List<ProductModel>();
                    var productListByProvince = redisCache.StringGet(redisKeyProductListByProvince);
                    if (!productListByProvince.HasValue)
                    {
                        var tempList = new List<ProductModel>();
                        var items = await rewardsDBContext.Products.AsNoTracking().Include(x => x.ProductOutlets).ThenInclude(x => x.Outlet).Include(x => x.Merchant).Include(x => x.ProductDiscounts).Include(x => x.ProductSubCategory).Include(x => x.ProductCategory).Include(x => x.StatusType).Include(x => x.DealType).Include(x => x.DealExpirations).Where(x => x.Merchant.IsPublished && x.IsActivated).ToListAsync();

                        var rewardRecoList = await vodusV2Context.ProductAds.AsNoTracking().ToArrayAsync();

                        foreach (var item in items)
                        {
                            decimal productCrt = 0;
                            var productAd = rewardRecoList.Where(x => x.ProductId == item.Id).FirstOrDefault();
                            var productAdsConfig = await vodusV2Context.ProductAdsConfig.AsNoTracking().FirstOrDefaultAsync();
                            int productImpressionThreshold = 1000;
                            if (productAdsConfig != null)
                            {
                                productImpressionThreshold = productAdsConfig.ImpressionCountIdentifier;
                            }
                            if (productAd != null)
                            {
                                if (productAd.AdImpressionCount > productImpressionThreshold)
                                {
                                    productCrt = productAd.CTR.Value;
                                }
                                else
                                {
                                    if (productImpressionThreshold != 0)
                                        productCrt = productAd.AdClickCount / productImpressionThreshold;
                                    else
                                        productCrt = 0;
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
                            newItem.LastUpdatedByUser =item.LastUpdatedByUser;
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

                        if(tempList != null && tempList.Any())
                        {
                            list.AddRange(tempList.Where(x => x.IsPublished == true && x.IsActivated == true && ((x.OutletProvince.IndexOf(request.ProvinceId) != -1 && (x.ExpirationTypeId == 1 || x.ExpirationTypeId == 2)) || x.ExpirationTypeId == 4 || x.ExpirationTypeId == 5))/*.OrderByDescending(x => x.CTR)*/.ToList());

                            foreach (var item in list)
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
                                if (fileList != null && fileList.Any())
                                {
                                    item.ImageFolderUrl.AddRange(fileList);
                                }
                            }

                            redisCache.StringSet(redisKeyProductListByProvince, System.Text.Json.JsonSerializer.Serialize<List<ProductModel>>(list));
                        }
                        
                    }
                    else
                    {
                        list = System.Text.Json.JsonSerializer.Deserialize<List<ProductModel>>(productListByProvince);
                    }

                    //Weighted Randomization
                    int totalCTR = (int)list.Sum(x => x.CTR.Value * 1000);

                    foreach (var item in list)
                    {
                        Random random = new Random();

                        item.WeightedCTR = random.Next(0, totalCTR);
                    }
                    list = list.OrderByDescending(x => ((int)x.CTR * 1000) - x.WeightedCTR).ToList();
                    response.Successful = true;
                    response.Message = "Get Product List Successfully";
                    response.Data = list;
                }
                catch (Exception ex)
                {
                    var errorLogs = new ErrorLogs
                    {
                        TypeId = CreateErrorLogCommand.Type.Service,
                        ActionName = "ProductListQuery",
                        ActionRequest = JsonConvert.SerializeObject(request),
                        CreatedAt = DateTime.Now,
                        Errors = ex.ToString()
                    };

                    await rewardsDBContext.ErrorLogs.AddAsync(errorLogs);
                    await rewardsDBContext.SaveChangesAsync();

                    response.Successful = false;
                    response.Message = "Fail to get products";
                }

                return response;
            }
        }
    }
}

