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
using Voupon.API.Util;
using Voupon.API.ViewModels;

namespace Voupon.API.Functions
{
    public class GetProductListByMerchantFunction
    {
        private readonly RewardsDBContext rewardsDBContext;
        private readonly VodusV2Context vodusV2Context;
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly IAzureBlobStorage azureBlobStorage;

        public GetProductListByMerchantFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
            this.connectionMultiplexer = connectionMultiplexer;
            this.azureBlobStorage = azureBlobStorage;
        }

        [OpenApiOperation(operationId: "Get products list by merchant id", tags: new[] { "Merchants" }, Description = "Get products by id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "id", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Merchant id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "s", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "External shop id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "t", In = ParameterLocation.Query, Required = false, Type = typeof(byte), Description = "External type id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "limit", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "limit of items", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "offset", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "offset after number of items", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "keyword", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "keyword of search", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ProductByMerchantResponseModel), Summary = "The paginated result of products")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If no id or merchant id is supplied")]

        [FunctionName("GetProductListByMerchantFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "merchant/products")] HttpRequest req, ILogger log)
        {
            var response = new ProductByMerchantResponseModel
            {
                Data = new ProductByMerchant()
            };

            try
            {
                var merchantId = req.Query["id"];
                var externalShopId = req.Query["s"];
                var externalTypeId = req.Query["t"];
                var tempLimit = req.Query["limit"];
                var tempOffset = req.Query["offset"];
                var keyword = req.Query["keyword"];
                var limit = 0;
                int.TryParse(tempLimit, out limit);

                var offset = 0;
                int.TryParse(tempOffset, out offset);


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

                    var productByMerchant = await GetInternalMerchantProducts(limit, offset, id,keyword);
                    if (productByMerchant == null)
                    {
                        return new BadRequestObjectResult(new ApiResponseViewModel
                        {
                            Code = -1,
                            ErrorMessage = "Fail to get data [002]"
                        });
                    }
                    else
                    {
                        response.Data = productByMerchant;
                    }
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

                    var productByMerchant = await GetExternalMerchantProducts(limit, offset, externalShopId, typeId, keyword);
                    if (productByMerchant == null)
                    {
                        return new BadRequestObjectResult(new ApiResponseViewModel
                        {
                            Code = -1,
                            ErrorMessage = "Fail to get data [002]"
                        });
                    }
                    else
                    {
                        
                        productByMerchant.Merchant = await GetExternalMerchantDetail(externalShopId, typeId);


                        response.Code = 1;
                        response.Data = productByMerchant;
                    }
                }
                return new OkObjectResult(response);
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

        private async Task<ProductByMerchant> GetExternalMerchantProducts(int limit, int offSet, string shopId, byte typeId,string keyword)
        {
            List<ProductModel> productList = new List<ProductModel>();
                var query = rewardsDBContext.Products.
                    Include(x => x.ProductOutlets).
                    ThenInclude(x => x.Outlet).
                    Include(x => x.Merchant).
                    Include(x => x.ProductDiscounts).
                    Include(x => x.ProductSubCategory).
                    Include(x => x.ProductCategory).
                    Include(x => x.StatusType).
                    Include(x => x.DealType).
                    Include(x => x.DealExpirations).
                    Where(x => x.ExternalMerchantId == shopId && x.ExternalTypeId== typeId && x.Merchant.IsPublished);

                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(x => x.Title.Contains(keyword));
                }
                var items = await query.Skip(limit).Take(offSet).ToListAsync();

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
                    newItem.TotalSold = (item.TotalBought.HasValue ? item.TotalBought.Value : 0);
                    newItem.Remarks = item.Remarks;
                    newItem.Rating = item.Rating;
                    newItem.IsPublished = item.IsPublished;
                    if (item.DealExpiration != null)
                    {
                        newItem.ExpirationTypeId = item.DealExpiration.ExpirationTypeId;
                    }
                    else
                    {
                        newItem.ExpirationTypeId = 5;
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

                   /* var AzureBlob = new AzureBlob();
                    var imageResult = await AzureBlob.BlobSmallImagesListQuery(new AzureBlob.BlobSmallImagesListModel
                    {
                        AzureBlobStorage = azureBlobStorage,
                        FilePath = FilePathEnum.Products_Images,
                        Id = item.Id,
                        ContainerName = ContainerNameEnum.Products,
                        GetIListBlobItem = false
                    });
                    if (imageResult != null && imageResult.Any())
                    {
                        newItem.ImageFolderUrl = imageResult;
                    }*/

                    productList.Add(newItem);
                }

            

            var products = productList.Select(x => new Products
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
                TotalSold = x.TotalSold,
                ProductSubCategory = x.ProductSubCategory,
                ProductSubCategoryId = x.ProductSubCategoryId,
                ProductCategoryId = x.ProductCategoryId,
                ProductImage = x.ImageFolderUrl.Count > 0 ? x.ImageFolderUrl[0] : "",//x.ImageFolderUrl[0],
                OutletLocation = x.OutletLocation,
            }).ToList();

            var merchant = await GetExternalMerchantDetail(shopId,typeId);

            var productMerchant = new ProductByMerchant();
            productMerchant.Products = products;
            if (merchant != null)
            {
                productMerchant.Merchant = merchant;
            }
            return productMerchant;
        }

        private async Task<ProductByMerchant> GetInternalMerchantProducts(int limit, int offSet, int merchantId,string keyword)
        {
            List<ProductModel> productList = new List<ProductModel>();

            //  Get Get giftee products if merchant is within this id
            if (merchantId >= 1000 && merchantId <= 1004)
            {
                productList = await GifteeProducts(merchantId);

            }
            else
            {
                var query = rewardsDBContext.Products.
                    Include(x => x.ProductOutlets).
                    ThenInclude(x => x.Outlet).
                    Include(x => x.Merchant).
                    Include(x => x.ProductDiscounts).
                    Include(x => x.ProductSubCategory).
                    Include(x => x.ProductCategory).
                    Include(x => x.StatusType).
                    Include(x => x.DealType).
                    Include(x => x.DealExpirations).
                    Where(x => x.MerchantId == merchantId && x.Merchant.IsPublished);

                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(x => x.Title.Contains(keyword));
                }
                var items = await query.Skip(limit).Take(offSet).ToListAsync();

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
                    newItem.TotalSold = (item.TotalBought.HasValue ? item.TotalBought.Value : 0);
                    newItem.Remarks = item.Remarks;
                    newItem.Rating = item.Rating;
                    newItem.IsPublished = item.IsPublished;
                    if (item.DealExpiration != null)
                    {
                        newItem.ExpirationTypeId = item.DealExpiration.ExpirationTypeId;
                    }
                    else
                    {
                        newItem.ExpirationTypeId = 5;
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

                    var AzureBlob = new AzureBlob();
                    var imageResult = await AzureBlob.BlobSmallImagesListQuery(new AzureBlob.BlobSmallImagesListModel
                    {
                        AzureBlobStorage = azureBlobStorage,
                        FilePath = FilePathEnum.Products_Images,
                        Id = item.Id,
                        ContainerName = ContainerNameEnum.Products,
                        GetIListBlobItem = false
                    });
                    if (imageResult != null && imageResult.Any())
                    {
                        newItem.ImageFolderUrl = imageResult;
                    }

                    productList.Add(newItem);
                }

            }

            var products = productList.Select(x => new Products
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
                TotalSold = x.TotalSold,
                ProductSubCategory = x.ProductSubCategory,
                ProductSubCategoryId = x.ProductSubCategoryId,
                ProductCategoryId = x.ProductCategoryId,
                ProductImage = x.ImageFolderUrl.Count > 0?x.ImageFolderUrl[0] : "" ,//x.ImageFolderUrl[0],
                OutletLocation = x.OutletLocation,
            }).ToList();

            var nextPageUrl = "";
            if (products.Count >= limit)
            {
                nextPageUrl = Environment.GetEnvironmentVariable("BASE_URL") + $"/v1/merchant/products?id={merchantId}&limit={limit}&offset={offSet + limit}";
            }
            var merchant = await GetInternalMerchantDetail(merchantId);

            var productMerchant = new ProductByMerchant();
            productMerchant.Products = products;
            if (merchant != null) {
               productMerchant.Merchant = merchant;
            }
            return productMerchant;
        }


        private async Task<MerchantInfo> GetInternalMerchantDetail(int merchantId) {
            var merchantInfo = new MerchantInfo();
            var merchant = await rewardsDBContext.Merchants.Where(x => x.Id == merchantId).FirstOrDefaultAsync();
            if (merchant != null)
            {
                merchantInfo.Id = merchant.Id.ToString();
                merchantInfo.Name = merchant.DisplayName;
                merchantInfo.ShopLogoUrl = (!string.IsNullOrEmpty(merchant.LogoUrl) ? merchant.LogoUrl.Replace("http://", "https://").Replace(":80", "") : "");
                merchantInfo.Rating = merchant.Rating;


                var merchantProducts = await rewardsDBContext.Products.AsNoTracking().Include(x => x.ProductCategory).Include(x => x.ProductSubCategory).Where(x => x.MerchantId == merchantId && x.IsPublished == true && x.IsActivated == true).ToListAsync();
                List<string> categoriesAndSubcategoriesList = new List<string>();
                foreach (var product in merchantProducts)
                {
                    if (!categoriesAndSubcategoriesList.Contains(product.ProductCategory.Name))
                        categoriesAndSubcategoriesList.Add(product.ProductCategory.Name);
                    if (product.ProductSubCategory != null) {
                        if (!categoriesAndSubcategoriesList.Contains(product.ProductSubCategory.Name))
                            categoriesAndSubcategoriesList.Add(product.ProductSubCategory.Name);
                    }

                }
                for (int i = 0; i < categoriesAndSubcategoriesList.Count; i++)
                {
                    if (i == 9)
                        break;
                    if (i == 0)
                        merchantInfo.ShopCategories += categoriesAndSubcategoriesList[i];
                    else
                        merchantInfo.ShopCategories += ", " + categoriesAndSubcategoriesList[i];

                }

                //   check if ID belongs to giftee setup
                if (merchantId == 1000)
                {
                    merchantInfo = GifteeSushiKing();
                }

                else if (merchantId == 1001)
                {
                    merchantInfo = GifteeLlaollao();
                }

                else if (merchantId == 1002)
                {
                    merchantInfo = GifteeHokkaido();
                }

                else if (merchantId == 1003)
                {
                    merchantInfo = GifteeBigApple();
                }

                else if (merchantId == 1004)
                {
                    merchantInfo = GifteeTeaLive();
                }

                merchantInfo.FollowerCount = GenerateRandomFollowers();
                merchantInfo.ResponseRate = GenerateRandomPercentage();

            }

            return merchantInfo;
        }

        private async Task<MerchantInfo> GetExternalMerchantDetail(string externalShopId, byte externalTypeId)
        {
            var merchantInfo = new MerchantInfo();
            var merchant = await rewardsDBContext.Merchants.Where(x => x.ExternalId == externalShopId && x.ExternalTypeId == externalTypeId).FirstOrDefaultAsync();
            if (merchant != null)
            {
                merchantInfo.Id = merchant.Id.ToString();
                merchantInfo.Name = merchant.DisplayName;
                merchantInfo.ShopLogoUrl = (!string.IsNullOrEmpty(merchant.LogoUrl) ? merchant.LogoUrl.Replace("http://", "https://").Replace(":80", "") : "");
                merchantInfo.Rating = merchant.Rating;


                var merchantProducts = await rewardsDBContext.Products.AsNoTracking()
                    .Include(x => x.ProductCategory)
                    .Include(x => x.ProductSubCategory)
                    .Where(x => x.ExternalMerchantId == externalShopId && x.ExternalTypeId == externalTypeId && x.IsPublished == true && x.IsActivated == true).ToListAsync();
                List<string> categoriesAndSubcategoriesList = new List<string>();
                foreach (var product in merchantProducts)
                {
                    if (!categoriesAndSubcategoriesList.Contains(product.ProductCategory.Name))
                        categoriesAndSubcategoriesList.Add(product.ProductCategory.Name);
                    if (product.ProductSubCategory != null)
                    {
                        if (!categoriesAndSubcategoriesList.Contains(product.ProductSubCategory.Name))
                            categoriesAndSubcategoriesList.Add(product.ProductSubCategory.Name);
                    }

                }
                for (int i = 0; i < categoriesAndSubcategoriesList.Count; i++)
                {
                    if (i == 9)
                        break;
                    if (i == 0)
                        merchantInfo.ShopCategories += categoriesAndSubcategoriesList[i];
                    else
                        merchantInfo.ShopCategories += ", " + categoriesAndSubcategoriesList[i];

                }
                merchantInfo.FollowerCount = GenerateRandomFollowers();
                merchantInfo.ResponseRate = GenerateRandomPercentage();

            }

            return merchantInfo;

        }
        private async Task<List<ProductModel>> GifteeProducts(int merchantId)
        {
            var list = new List<ProductModel>();

            var idList = "";

            if (merchantId == 1000)
            {
                idList = Environment.GetEnvironmentVariable("THIRD_PARTY_GIFTEE_PRODUCTS_SUSHIKING").ToString();
            }
            else if (merchantId == 1001)
            {
                idList = Environment.GetEnvironmentVariable("THIRD_PARTY_GIFTEE_PRODUCTS_LAOLAO").ToString();
            }
            else if (merchantId == 1002)
            {
                idList = Environment.GetEnvironmentVariable("THIRD_PARTY_GIFTEE_PRODUCTS_HOKKAIDO").ToString();
            }
            else if (merchantId == 1003)
            {
                idList = Environment.GetEnvironmentVariable("THIRD_PARTY_GIFTEE_PRODUCTS_BIGAPPLE").ToString();
            }
            else if (merchantId == 1004)
            {
                idList = Environment.GetEnvironmentVariable("THIRD_PARTY_GIFTEE_PRODUCTS_TEALIVE").ToString();
            }

            if (string.IsNullOrEmpty(idList))
            {
                return null;
            }

            var itemId = idList.Split(",");

            for (var i = 0; i < itemId.Length; i++)
            {
                var id = int.Parse(itemId[i].ToString());

                var item = await rewardsDBContext.Products.Include(x => x.ProductOutlets).ThenInclude(x => x.Outlet).Include(x => x.Merchant).Include(x => x.ProductSubCategory).Include(x => x.ProductCategory).Include(x => x.StatusType).Include(x => x.DealType).Include(x => x.DealExpirations).Where(x => x.Id == id).FirstOrDefaultAsync();

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
                newItem.TotalSold = (item.TotalBought.HasValue ? item.TotalBought.Value : 0);
                newItem.Remarks = item.Remarks;
                newItem.IsPublished = item.IsPublished;
                if ((item.DealExpirations != null && item.DealExpirations.Count > 0) && item.DealExpirations.FirstOrDefault().ExpirationType != null)
                {
                    newItem.ExpirationTypeId = item.DealExpirations.FirstOrDefault().ExpirationTypeId;
                }
                else
                {
                    newItem.ExpirationTypeId = 5;
                }
                newItem.OutletName = item.ProductOutlets.Select(x => x.Outlet.Name).ToList();
                newItem.OutletProvince = item.ProductOutlets.Select(x => x.Outlet.ProvinceId).ToList();
                newItem.TotalOutlets = item.ProductOutlets.Count();

                var AzureBlob = new AzureBlob();
                var imageResult = await AzureBlob.BlobSmallImagesListQuery(new AzureBlob.BlobSmallImagesListModel
                {
                    AzureBlobStorage = azureBlobStorage,
                    FilePath = FilePathEnum.Products_Images,
                    Id = merchantId,
                    ContainerName = ContainerNameEnum.Products,
                    GetIListBlobItem = false
                });
                if (imageResult != null && imageResult.Any())
                {
                    newItem.ImageFolderUrl = imageResult;
                }

                list.Add(newItem);

            }
            return list;
        }

        private string GenerateRandomFollowers() {
            Random rnd = new Random();
            int fol = rnd.Next(1, 100);

            return $"{fol}k";

        }
        private string GenerateRandomPercentage()
        {
            Random rnd = new Random();
            int per = rnd.Next(30, 100);

            return $"{per}%";

        }

        private MerchantInfo GifteeSushiKing()
        {
            var viewModel = new MerchantInfo
            {
                Id = "1000",
                Description = "<p>Sushi King chain of restaurants serves quality sushi and other Japanese cuisine at affordable prices in a warm and friendly environment.</p><p>What sets Sushi King apart is the personal touch of serving freshly made sushi on the kaiten for customers to pick up and enjoy.</p>",
                WebsiteUrl = "https://sushi-king.com/",
                Name = "Sushi king",
                ShopLogoUrl = "https://vodus.my/giftee/sushiking/logo.png",
            };
            return viewModel;
        }

        private MerchantInfo GifteeLlaollao()
        {
            var viewModel = new MerchantInfo
            {
                Id = "1001",
                Description = "<p>llaollao is Frozen Yogurt. Or yogurt ice cream, which is the same thing. And, as it is natural, it is one of the healthiest and most recommended foods worldwide, thanks to its healthy properties and high nutritional value.</p><p>Its preparation using skimmed milk when served and its combination with prime quality toppings – from recently cut seasonal fruits to cereals, fun crunchy toppings and delicious sauces – makes it an even more delicious and healthier product.</p>",
                WebsiteUrl = "https://www.llaollaoweb.com/en/",
                Name = "Llao Llao",
                ShopLogoUrl = "https://vodus.my/giftee/llaollao/logo.png"
            };
            return viewModel;
        }

        private MerchantInfo GifteeHokkaido()
        {
            var viewModel = new MerchantInfo
            {
                Id = "1002",
                Description = "<p>Our resident bakers worked with counterparts from Hokkaido, Japan to further improve the recipe of this baked pastry to ensure sustainable uniqueness in its taste and texture. The soft and creamy centre is made with a blend of three different high-quality specialty cheeses; piped into a crunchy shortcrust pastry base.</p><p>Satisfy the urge of one’s tummy, any time of the day! Tasting best when it is right out from the oven, the Hokkaido Baked Cheese Tart also taste awesome when chilled; leaving you with a smooth and refreshing experience. When frozen, the tart just tastes like a creamy cheesy ice-cream. Try all ways and decide which one you like best!</ p>",
                WebsiteUrl = "http://www.hbct.com.my/",
                Name = "Hokkaido Cheese Bake Tart",
                ShopLogoUrl = "https://vodus.my/giftee/hokkaido/logo.jpeg"
            };

            return viewModel;
        }

        private MerchantInfo GifteeBigApple()
        {
            var viewModel = new MerchantInfo
            {
                Id = "1003",
                Description = "<p>BIG APPLE Donuts & Coffee offers a wide selection of premium donuts, with a range of tantalizing coffees and a charming variety of tea creations. Its exceptional donut qualities come from a unique premix formula with a carefully selected blend of over 20 imported ingredients, producing donuts known for their hallmark freshness and fluffy soft texture.</p><p>Big Apple Donuts & Coffee began as a simple idea for people to come together and share their passion for donuts, and it slowly evolved into a thriving successful business built upon trust, commitment and innovation. Every day, the people at Big Apple Donuts & Coffee strive with the same passion to deliver not just great tasting donuts but a unique customer experience that encapsulates the qualities of the brand.</p>",
                WebsiteUrl = "https://www.bigappledonuts.com",
                Name = "Big Apple",
                ShopLogoUrl = "https://vodus.my/giftee/bigapple/logo.png"
            };

            return viewModel;
        }

        private MerchantInfo GifteeTeaLive()
        {
            var viewModel = new MerchantInfo
            {
                Id = "1004",
                Description = "<p>Tealive is Southeast Asia's largest lifestyle tea brand, and our mission is to always bring joyful experiences through tea - Serving a variety of beverages, from signature pearl milk tea to coffee and smoothies.</p>",
                WebsiteUrl = "https://www.tealive.com.my/",
                Name = "TeaLive",
                ShopLogoUrl = "https://vodus.my/giftee/tealive/logo.jpeg"
            };

            return viewModel;
        }


        private class AggregatorParams
        {
            public int Limit { get; set; }
            public int Offset { get; set; }
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

        protected class ProductByMerchantResponseModel : ApiResponseViewModel
        {
            public ProductByMerchant Data { get; set; }
        }

        protected class ProductByMerchant
        {
            //public string NextPageUrl { get; set; }
            public List<Products> Products { get; set; }

            public MerchantInfo Merchant { get; set; }


        }

        protected class ProductMerchantExternal {

            public string NextPageUrl { get; set; }
            public List<Products> Items { get; set; }
        }

        protected class MerchantInfo {
            public string Id { get; set; }
            public string Name { get; set; }

            public string Description { get; set; }

            public string ShopLogoUrl { get; set; }

            public string ShopCategories { get; set; }

            public byte ExternalTypeId { get; set; }

            public decimal Rating { get; set; }

            public string FollowerCount { get; set; }

            public string ResponseRate { get; set; }
            public string WebsiteUrl { get; set; }
        }
        public class Products
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

        private class ProductModel
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
            public int TotalSold { get; set; }
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

        protected class MerchantExternal
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

            public long FollowerCount { get; set; }

            public long ResponseRate { get; set; }


        }
    }
}
