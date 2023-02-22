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
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Ads.Queries
{
    public class RewardsAdsProduct
    {
        public int PointRequired { get; set; }
        public string ImageUrl { get; set; }
        public string ProductUrl { get; set; }
        public string ProductTitle { get; set; }
        public string MerchantName { get; set; }
        public string DiscountedPrice { get; set; }
        public string Price { get; set; }
        public string TotalDiscount { get; set; }


    }
    public class CCResponseAdsQuery : IRequest<ApiResponseViewModel>
    {

        public class CCResponseAdsQueryHandler : IRequestHandler<CCResponseAdsQuery, ApiResponseViewModel>
        {

            RewardsDBContext rewardsDBContext;
            IConnectionMultiplexer connectionMultiplexer;
            private readonly IAzureBlobStorage azureBlobStorage;
            private readonly IOptions<AppSettings> appSettings;
            public CCResponseAdsQueryHandler(RewardsDBContext rewardsDBContext, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage, IOptions<AppSettings> appSettings)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.connectionMultiplexer = connectionMultiplexer;
                this.azureBlobStorage = azureBlobStorage;
                this.appSettings = appSettings;
            }


            public async Task<ApiResponseViewModel> Handle(CCResponseAdsQuery request, CancellationToken cancellationToken)
            {
                var response = new ApiResponseViewModel();
                var productList = new List<RewardsAdsProduct>();
                try
                {
                    var products = await rewardsDBContext.Products.Include(x => x.Merchant).Where(x => x.IsPublished && x.IsActivated).ToListAsync();

                    if(products == null)
                    {
                        response.Successful = false;
                        return response;
                    }

                    foreach(var product in products)
                    {
                        var newProduct = new RewardsAdsProduct();
                        newProduct.MerchantName = product.Merchant.DisplayName;
                        newProduct.ImageUrl = "https://cdn11.bigcommerce.com/s-t04x4i8lh4/images/stencil/2048x2048/products/973/7815/Mystery-Gift__15205.1571852864.jpg?c=2";
                        newProduct.ProductTitle = product.Title;
                        newProduct.ProductUrl = appSettings.Value.App.BaseUrl + "/product/" + product.Id;

                        var filename = await azureBlobStorage.ListBlobsAsync(ContainerNameEnum.Products, product.Id + "/" + FilePathEnum.Products_Images);
                        var fileList = new List<string>();
                        foreach (var file in filename)
                        {
                            fileList.Add(file.StorageUri.PrimaryUri.OriginalString);
                        }

                        if (fileList.Count > 0)
                        {
                            newProduct.ImageUrl = fileList.First();
                        }


                        if (product.DealTypeId == 1 || product.DealTypeId == 3)
                        {
                            newProduct.PointRequired = product.PointsRequired.Value;
                            newProduct.DiscountedPrice = "";
                            newProduct.Price = "";
                        }
                        else
                        {
                            var discount = rewardsDBContext.ProductDiscounts.Where(x => x.ProductId == product.Id).OrderByDescending(x => x.PointRequired).ToList();
                            if (discount != null && discount.Count > 0)
                            {
                                newProduct.PointRequired = discount.First().PointRequired;
                                newProduct.Price = "RM " + (product.DiscountedPrice.HasValue ? product.DiscountedPrice.Value.ToString("F2") : product.Price.Value.ToString("F2"));

                                var lowestPrice = product.DiscountedPrice.HasValue ? product.DiscountedPrice.Value : product.Price.Value;

                                if (discount.First().DiscountTypeId == 1)
                                {
                                    newProduct.TotalDiscount = "Get " + discount.First().PercentageValue.ToString("F0") + "% OFF";
                                    newProduct.DiscountedPrice = "RM " + (lowestPrice * (1 - (discount.First().PercentageValue / 100))).ToString("F2");
                                }
                                else
                                {
                                    newProduct.TotalDiscount = "Get RM" + discount.First().PriceValue + " OFF";
                                    newProduct.DiscountedPrice = "RM " + (lowestPrice - discount.First().PriceValue).ToString("F2");
                                }
                                productList.Add(newProduct);
                            }
                        }
                    }

                    if(productList != null && productList.Any())
                    {
                        response.Successful = true;
                        response.Data = productList;
                    }
                    /*
                    var redisCache = connectionMultiplexer.GetDatabase();
                    var RewardsAdsCache = redisCache.StringGet("VouponRewardsAds");
                    if (!RewardsAdsCache.HasValue)
                    {
                        var rewardsAdsProductList = rewardsDBContext.Products.Where(x => x.IsPublished && x.IsActivated).Include(x => x.Merchant).ToList();
                        foreach (var product in rewardsAdsProductList)
                        {
                            RewardsAdsProduct newProduct = new RewardsAdsProduct();
                            newProduct.MerchantName = product.Merchant.DisplayName;
                            newProduct.ImageUrl = "https://cdn11.bigcommerce.com/s-t04x4i8lh4/images/stencil/2048x2048/products/973/7815/Mystery-Gift__15205.1571852864.jpg?c=2";
                            newProduct.ProductTitle = product.Title;
                            newProduct.ProductUrl = "https://voupon-rewards-uat.azurewebsites.net/Product/" + product.Id;

                            var filename = await azureBlobStorage.ListBlobsAsync(ContainerNameEnum.Products, product.Id + "/" + FilePathEnum.Products_Images);
                            var fileList = new List<string>();
                            foreach (var file in filename)
                            {
                                fileList.Add(file.StorageUri.PrimaryUri.OriginalString);
                            }

                            if (fileList.Count > 0)
                            {
                                newProduct.ImageUrl = fileList.First();
                            }


                            if (product.DealTypeId == 1 || product.DealTypeId == 3)
                            {
                                newProduct.PointRequired = product.PointsRequired.Value;
                                newProduct.DiscountedPrice = "";
                                newProduct.Price = "";
                            }
                            else
                            {
                                var discount = rewardsDBContext.ProductDiscounts.Where(x => x.ProductId == product.Id).OrderByDescending(x => x.PointRequired).ToList();
                                if (discount != null && discount.Count > 0)
                                {

                                    newProduct.PointRequired = discount.First().PointRequired;
                                    newProduct.Price = "RM " + (product.DiscountedPrice.HasValue ? product.DiscountedPrice.Value.ToString("F2") : product.Price.Value.ToString("F2"));

                                    var lowestPrice = product.DiscountedPrice.HasValue ? product.DiscountedPrice.Value : product.Price.Value;

                                    if (discount.First().DiscountTypeId == 1)
                                    {
                                        newProduct.TotalDiscount = "Get " + discount.First().DiscountValue.ToString("F0") + "% OFF";
                                        newProduct.DiscountedPrice = "RM " + (lowestPrice * (1 - (discount.First().DiscountValue / 100))).ToString("F2");
                                    }
                                    else
                                    {
                                        newProduct.TotalDiscount = "Get RM" + discount.First().DiscountValue + " OFF";
                                        newProduct.DiscountedPrice = "RM " + (lowestPrice - discount.First().DiscountValue).ToString("F2");
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
                    */
                }
                catch (Exception ex)
                {
                    response.Message = ex.Message;
                }

                return response;
            }
        }
    }

}
