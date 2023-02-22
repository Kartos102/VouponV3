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
using Microsoft.Extensions.Options;

namespace Voupon.Rewards.WebApp.Common.Products.Queries
{
    public class ProductListAdsQuery : IRequest<ApiResponseViewModel>
    {
        public class RewardsAdsProduct
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

        public class ProductListAdsQueryHandler : IRequestHandler<ProductListAdsQuery, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            VodusV2Context vodusV2Context;
            private readonly IConnectionMultiplexer connectionMultiplexer;
            private IDatabase redisCache;
            private readonly IAzureBlobStorage azureBlobStorage;
            private IOptions<AppSettings> appSettings;
            public ProductListAdsQueryHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage, IOptions<AppSettings> appSettings)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.vodusV2Context = vodusV2Context;
                this.connectionMultiplexer = connectionMultiplexer;
                this.azureBlobStorage = azureBlobStorage;
                this.appSettings = appSettings;
            }

            public async Task<ApiResponseViewModel> Handle(ProductListAdsQuery request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                var redisProductListAdsKey = "ProductListAds";
                redisCache = connectionMultiplexer.GetDatabase();
                var redisProductListAds = redisCache.StringGet(redisProductListAdsKey);
                List<RewardsAdsProduct> productlist = new List<RewardsAdsProduct>();
                var productModel = new List<ProductModel>();
                try
                {
                    if (!redisProductListAds.HasValue)
                    {
                        int VPointCap = Int16.Parse(appSettings.Value.RewardAds.VPointCap);

                        var items = await rewardsDBContext.Products.AsNoTracking().Include(x => x.ProductOutlets).ThenInclude(x => x.Outlet).Include(x => x.Merchant).Include(x => x.ProductDiscounts).Include(x => x.ProductSubCategory).Include(x => x.ProductCategory).Include(x => x.StatusType).Include(x => x.DealType).Include(x => x.DealExpirations).Where(x => x.Merchant.IsPublished && x.IsActivated && x.IsPublished).ToListAsync();

                        var rewardRecoList = await vodusV2Context.ProductAds.AsNoTracking().ToArrayAsync();

                        List<ProductModel> list = new List<ProductModel>();
                        var tempList = new List<ProductModel>();
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
                            newProduct.ProductUrl = $"{appSettings.Value.App.BaseUrl}/Product/{product.Id}";
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
                                    while (discount.First().PointRequired > VPointCap)
                                    {
                                        discount.RemoveAt(0);
                                    }
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
                        List<ProductModel> matchList = new List<ProductModel>();

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
                            redisCache.StringSet("ProductListAds", System.Text.Json.JsonSerializer.Serialize<List<ProductModel>>(matchList));
                            apiResponseViewModel.Data = matchList;
                        }
                        else
                        {
                            if(newList != null && newList.Any())
                            {
                                var filteredList = newList.Count > 12 ? newList.Take(12).ToList() : newList;
                                foreach(var item in filteredList)
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
                                apiResponseViewModel.Data = filteredList;
                            }
                        }

                    }
                    else
                    {
                        apiResponseViewModel.Data = System.Text.Json.JsonSerializer.Deserialize<List<ProductModel>>(redisProductListAds);
                    }
                    apiResponseViewModel.Successful = true;
                }
                catch (Exception ex)
                {
                    var errorLogs = new ErrorLogs
                    {
                        TypeId = CreateErrorLogCommand.Type.Service,
                        ActionName = "ProductListAdsQuery",
                        ActionRequest = JsonConvert.SerializeObject(request),
                        CreatedAt = DateTime.Now,
                        Errors = ex.ToString()
                    };

                    await rewardsDBContext.ErrorLogs.AddAsync(errorLogs);
                    await rewardsDBContext.SaveChangesAsync();

                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Fail to get products";
                }

                return apiResponseViewModel;
            }
        }
    }

}
