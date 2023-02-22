using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Common.Redis.Queries
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
    public class GetRewardsAdsProductQuery : IRequest<ApiResponseViewModel>
    {
    }

    public class GetRewardsAdsProductQueryHandler : IRequestHandler<GetRewardsAdsProductQuery, ApiResponseViewModel>
    {
        VodusV2Context vodusV2;
        RewardsDBContext rewardsDBContext;
        IConnectionMultiplexer connectionMultiplexer;
        private readonly IAzureBlobStorage azureBlobStorage;
        private IOptions<AppSettings> appSettings;
        public GetRewardsAdsProductQueryHandler(VodusV2Context vodusV2, RewardsDBContext rewardsDBContext, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage, IOptions<AppSettings> appSettings)
        {
            this.vodusV2 = vodusV2;
            this.rewardsDBContext = rewardsDBContext;
            this.connectionMultiplexer = connectionMultiplexer;
            this.azureBlobStorage = azureBlobStorage;
            this.appSettings = appSettings;
        }


        public async Task<ApiResponseViewModel> Handle(GetRewardsAdsProductQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            List<RewardsAdsProduct> productlist = new List<RewardsAdsProduct>();
            try
            {
                int VPointCap = Int16.Parse(appSettings.Value.RewardAds.VPointCap);

                var redisCache = connectionMultiplexer.GetDatabase();
                var RewardsAdsCache = redisCache.StringGet("VouponRewardsAds");
                if (!RewardsAdsCache.HasValue)
                {
                    var rewardsAdsProductList = rewardsDBContext.Products.Where(x => x.IsPublished && x.IsActivated && x.PointsRequired <= 500 && x.Merchant.IsTestAccount == false && x.AvailableQuantity > 0).Include(x => x.DealExpirations).Include(x => x.Merchant).ToList();
                    var rewardAdsIds = vodusV2.ProductAds.Where(x => x.IsActive == true).ToList().Select(x => x.ProductId).ToList();
                    if (rewardAdsIds.Count > 0)
                        rewardsAdsProductList = rewardsAdsProductList.Where(x => rewardAdsIds.Contains(x.Id)).ToList();
                    foreach (var product in rewardsAdsProductList)
                    {
                        var rewardAd = vodusV2.ProductAds.Where(x => x.ProductId == product.Id).FirstOrDefault();
                        RewardsAdsProduct newProduct = new RewardsAdsProduct();
                        newProduct.MerchantName = product.Merchant.DisplayName;
                        newProduct.ImageUrl = "https://cdn11.bigcommerce.com/s-t04x4i8lh4/images/stencil/2048x2048/products/973/7815/Mystery-Gift__15205.1571852864.jpg?c=2";
                        newProduct.ProductTitle = product.Title;
                        newProduct.ProductUrl = $"{appSettings.Value.App.BaseUrl}/Product/{product.Id}";
                        if(rewardAd != null)
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
                                newProduct.Price = "RM " +  product.Price.Value.ToString("F2");


                                var lowestPrice = product.DiscountedPrice.HasValue ? product.DiscountedPrice.Value : product.Price.Value;
                                newProduct.PriceValue = discount.First().PriceValue;
                                newProduct.PercentageValue = discount.First().PercentageValue;
                                var extraDiscount = product.Price.Value - lowestPrice;

                                if (discount.First().DiscountTypeId == 1)
                                {
                                    newProduct.TotalDiscount = ((discount.First().PercentageValue) > 100 ?100 : (discount.First().PercentageValue)).ToString("F0") + "% OFF";
                                    newProduct.DiscountedPrice = "RM " + ((lowestPrice * (1 - ((discount.First().PercentageValue + extraDiscount) / 100))) < 0 ? 0 : (lowestPrice * (1 - ((discount.First().PercentageValue + extraDiscount) / 100)))).ToString("F2");
                                }
                                else
                                {
                                    newProduct.TotalDiscount = "RM" + (((discount.First().PriceValue + extraDiscount)) > product.Price.Value ? product.Price.Value : (discount.First().PriceValue + extraDiscount)) + " OFF";
                                    newProduct.DiscountedPrice = "RM " + ((lowestPrice - (discount.First().PriceValue + extraDiscount)) < 0? 0 : (lowestPrice - (discount.First().PriceValue + extraDiscount))).ToString("F2");
                                }
                                productlist.Add(newProduct);
                            }
                        }
                    }
                    redisCache.StringSet("VouponRewardsAds", JsonSerializer.Serialize<List<RewardsAdsProduct>>(productlist));
                }
                else
                {
                    productlist = JsonSerializer.Deserialize<List<RewardsAdsProduct>>(RewardsAdsCache);
                }
                response.Successful = true;
                response.Message = "Create Rewards Ads Redis Successfully";
                response.Data = productlist;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
