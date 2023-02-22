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
    public class GetProductsByIdFunction
    {
        private readonly RewardsDBContext rewardsDBContext;
        private readonly VodusV2Context vodusV2Context;
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly IAzureBlobStorage azureBlobStorage;

        public GetProductsByIdFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
            this.connectionMultiplexer = connectionMultiplexer;
            this.azureBlobStorage = azureBlobStorage;
        }

        [OpenApiOperation(operationId: "Get products by id", tags: new[] { "Products" }, Description = "Get products by id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "id", In = ParameterLocation.Query, Required = false, Type = typeof(int), Summary = "Product id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "i", In = ParameterLocation.Query, Required = false, Type = typeof(string), Summary = "External item id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "s", In = ParameterLocation.Query, Required = false, Type = typeof(string), Summary = "External shop id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "t", In = ParameterLocation.Query, Required = false, Type = typeof(string), Summary = "External type id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "locale", In = ParameterLocation.Query, Required = false, Type = typeof(string), Summary = "Locale", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ProductResponseModel), Summary = "Get product by id")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If no id or external id is supplied")]


        [FunctionName("GetProductsByIdFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/detail")] HttpRequest req, ILogger log)
        {
            var response = new ProductResponseModel
            {
                Data = new ProductData()
            };

            try
            {
                var productId = req.Query["id"];
                var externalItemId = req.Query["i"];
                var externalShopId = req.Query["s"];
                var externalTypeId = req.Query["t"];

                int id = 0;
                bool success = int.TryParse(productId, out id);
                if (id > 0)
                {
                    var products = await GetInternalProduct(id);
                    if (products == null)
                    {
                        return new BadRequestObjectResult(new ApiResponseViewModel
                        {
                            Code = -1,
                            ErrorMessage = "Fail to get data [002]"
                        });
                    }
                    else
                    {
                        //  get reviews
                        products.Merchant.TotalReviewCount = rewardsDBContext.ProductReview.Where(x => x.ProductId == id).Count();
                        response.Data.Product = products;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(externalItemId) || string.IsNullOrEmpty(externalShopId) || string.IsNullOrEmpty(externalTypeId))
                    {
                        return new BadRequestObjectResult(new ApiResponseViewModel
                        {
                            Code = -1,
                            ErrorMessage = "Fail to get data [001]"
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
                    var products = await GetExternalProduct(externalItemId, externalShopId, typeId);
                    if (products == null)
                    {
                        return new BadRequestObjectResult(new ApiResponseViewModel
                        {
                            Code = -1,
                            ErrorMessage = "Fail to get data [002]"
                        });
                    }
                    else
                    {
                        response.Data.Product = products;
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

        private async Task<Products> GetExternalProduct(string itemId, string shopId, byte typeId)
        {

            var productObj = await rewardsDBContext.Products.
                Include(x => x.DealExpirations).
                ThenInclude(x => x.ExpirationType).
                Include(x => x.Merchant).
                Include(x => x.ProductDiscounts).
                Include(x => x.ProductCategory).
                Include(x => x.ProductSubCategory).
                Include(x => x.Merchant.MerchantCarousel).

                Include(x => x.ProductOutlets).
                ThenInclude(x => x.Outlet).
                Where(x => x.ExternalId == itemId && x.ExternalMerchantId == shopId && x.ExternalTypeId== typeId && x.Merchant.IsTestAccount == false).FirstOrDefaultAsync();
            Products product = new Products();
            product.Rating = productObj.Rating != 0 ? productObj.Rating : 5;
            product.Id = productObj.Id;
            product.AdditionInfo = productObj.AdditionInfo;
            product.AvailableQuantity = productObj.AvailableQuantity;
            product.DealExpirationId = productObj.DealExpirationId;
            product.DealTypeId = productObj.DealTypeId;
            product.Description = productObj.Description;
            product.DiscountedPrice = productObj.DiscountedPrice;
            product.DiscountRate = productObj.DiscountRate;
            product.FinePrintInfo = productObj.FinePrintInfo;
            product.ImageFolderUrl = productObj.ImageFolderUrl;
            product.IsActivated = productObj.IsActivated;
            product.IsPublished = productObj.IsPublished;
            product.IsProductVariationEnabled = productObj.IsProductVariationEnabled;
            product.LuckyDrawId = productObj.LuckyDrawId;
            product.MerchantId = productObj.MerchantId;
            product.PointsRequired = productObj.PointsRequired;
            product.ActualPriceForVpoints = productObj.ActualPriceForVpoints;
            product.Price = productObj.Price;
            product.ProductCategoryId = productObj.ProductCategoryId;
            product.ProductSubCategoryId = productObj.ProductSubCategoryId;
            product.RedemptionInfo = productObj.RedemptionInfo;
            product.StartDate = productObj.StartDate;
            product.EndDate = productObj.EndDate;
            product.StatusTypeId = productObj.StatusTypeId;
            product.Remarks = productObj.Remarks;
            product.Subtitle = productObj.Subtitle;
            product.Title = productObj.Title;
            product.TotalBought = productObj.TotalBought;
            product.ProductImageList = await GetFromBlob(productObj.Id, ContainerNameEnum.Products);
            
            
            product.VariationImageList = await GetFromBlob(productObj.Id, FilePathEnum.Products_Variation_Images);

            if (product.VariationImageList != null)
            {
                if (product.VariationImageList[0].Equals("images/not-available-stamp.jpg"))
                {
                    product.VariationImageList = null;
                }
            }



            if (productObj.DealExpiration != null)
            {
                var expirationType = new DetailPageExpirationType();
                expirationType.Id = productObj.DealExpiration.Id;
                expirationType.Name = productObj.DealExpiration.ExpirationType.Name;
                expirationType.Type = productObj.DealExpiration.ExpirationTypeId.HasValue ? productObj.DealExpiration.ExpirationTypeId.Value : 0;
                expirationType.TotalValidDays = productObj.DealExpiration.TotalValidDays.HasValue ? productObj.DealExpiration.TotalValidDays.Value : 0;
                expirationType.StartDate = productObj.DealExpiration.StartDate.HasValue ? productObj.DealExpiration.StartDate.Value : new DateTime(1800, 1, 1);
                expirationType.ExpiredDate = productObj.DealExpiration.ExpiredDate.HasValue ? productObj.DealExpiration.ExpiredDate.Value : new DateTime(1800, 1, 1);
            }

            product.Merchant = new DetailPageMerchant()
            {
                Id = productObj.Merchant.Id,
                Code = productObj.Merchant.Code,
                DisplayName = productObj.Merchant.DisplayName,
                LogoUrl = (productObj.Merchant.LogoUrl != null ? productObj.Merchant.LogoUrl.Replace("http://", "https://").Replace(":80", "") : ""),
                OnVacation = false,
                AverageRating = 5,
                TotalReviewCount = 0,
                ChatResponseSpeed = "Within an hour",
                NumberOfProducts = 300,
                JoinDate = "5 years ago"
                
            };

            if (productObj.Merchant.MerchantCarousel != null)
            {
                product.Merchant.MerchantCarouselList = productObj.Merchant.MerchantCarousel.Where(x => x.StatusId == 1).Select(x => new MerchantCarousel
                {
                    Id = x.Id,
                    ImageUrl = x.ImageUrl,
                }).ToList();
            }


            if (productObj.ProductCategory != null)
            {
                product.ProductCategory = new DetailPageProductCategory()
                {
                    Id = productObj.ProductCategory.Id,
                    Name = productObj.ProductCategory.Name
                };
            }

            if (productObj.ProductSubCategory != null)
            {
                product.ProductSubCategory = new DetailPageProductSubCategory()
                {
                    Id = productObj.ProductSubCategory.Id,
                    Name = productObj.ProductSubCategory.Name
                };
            }

            product.ProductOutlets = productObj.ProductOutlets.Select(x => new DetailProductOutlets()
            {
                Id = x.Outlet.Id,
                Name = x.Outlet.Name
            }).ToList();

            product.AdditionalDiscounts = productObj.ProductDiscounts.Where(z => z.IsActivated).Select(x => new AdditionalDiscounts()
            {
                Id = x.Id,
                Name = x.Name,
                Type = x.DiscountTypeId,
                Value = x.DiscountTypeId == 1 ? x.PercentageValue : x.PriceValue,
                Points = (short)x.PointRequired,
            }).ToList();

            /*if (!product.Title.ToLower().Contains("voucher"))
            {
                AdditionalDiscounts noneDiscount =
                new AdditionalDiscounts
                {
                    Id = 0,
                    Name = "None",
                    Value = 0,
                };

                product.AdditionalDiscounts.Add(noneDiscount);
            }*/
            product.AdditionalDiscounts.OrderBy(x => x.Value).ToList();

            product.VariationModel = await GetVariant(product.Id);
            return product;

        }

        private async Task<Products> GetInternalProduct(int id)
        {
            var productObj = await rewardsDBContext.Products.
                Include(x => x.DealExpirations).
                ThenInclude(x=> x.ExpirationType).
                Include(x => x.Merchant).
                Include(x => x.ProductDiscounts).
                Include(x => x.ProductCategory).
                Include(x => x.ProductSubCategory).
                Include(x => x.Merchant.MerchantCarousel).
                Include(x => x.ProductOutlets).
                ThenInclude(x => x.Outlet).
                Where(x => x.Id == id && x.Merchant.IsTestAccount == false).FirstOrDefaultAsync();
            Products product = new Products();
            product.Rating = productObj.Rating != 0? productObj.Rating : 5;
            product.Id = productObj.Id;
            product.AdditionInfo = productObj.AdditionInfo;
            product.AvailableQuantity = productObj.AvailableQuantity;
            product.DealExpirationId = productObj.DealExpirationId;
            product.DealTypeId = productObj.DealTypeId;
            product.Description = productObj.Description;
            product.DiscountedPrice = productObj.DiscountedPrice;
            product.DiscountRate = productObj.DiscountRate;
            product.FinePrintInfo = productObj.FinePrintInfo;
            product.ImageFolderUrl = productObj.ImageFolderUrl;
            product.IsActivated = productObj.IsActivated;
            product.IsPublished = productObj.IsPublished;
            product.IsProductVariationEnabled = productObj.IsProductVariationEnabled;
            product.LuckyDrawId = productObj.LuckyDrawId;
            product.MerchantId = productObj.MerchantId;
            product.PointsRequired = productObj.PointsRequired;
            product.ActualPriceForVpoints = productObj.ActualPriceForVpoints;
            product.Price = productObj.Price;
            product.ProductCategoryId = productObj.ProductCategoryId;
            product.ProductSubCategoryId = productObj.ProductSubCategoryId;
            product.RedemptionInfo = productObj.RedemptionInfo;
            product.StartDate = productObj.StartDate;
            product.EndDate = productObj.EndDate;
            product.StatusTypeId = productObj.StatusTypeId;
            product.Remarks = productObj.Remarks;
            product.Subtitle = productObj.Subtitle;
            product.Title = productObj.Title;
            product.TotalBought = productObj.TotalBought;
            product.ProductImageList = await GetFromBlob(productObj.Id, ContainerNameEnum.Products);
           
            product.VariationImageList = await GetFromBlob(productObj.Id, FilePathEnum.Products_Variation_Images);

            if (product.VariationImageList != null)
            {
                if (product.VariationImageList[0].Equals("images/not-available-stamp.jpg"))
                {
                    product.VariationImageList = null;
                }
            }


            if (productObj.DealExpiration != null) {
                var expirationType = new DetailPageExpirationType();
                expirationType.Id = productObj.DealExpiration.Id;
                expirationType.Name = productObj.DealExpiration.ExpirationType.Name;
                expirationType.Type = productObj.DealExpiration.ExpirationTypeId.HasValue ? productObj.DealExpiration.ExpirationTypeId.Value : 0;
                expirationType.TotalValidDays = productObj.DealExpiration.TotalValidDays.HasValue ? productObj.DealExpiration.TotalValidDays.Value : 0;
                expirationType.StartDate = productObj.DealExpiration.StartDate.HasValue ? productObj.DealExpiration.StartDate.Value : new DateTime(1800, 1, 1);
                expirationType.ExpiredDate = productObj.DealExpiration.ExpiredDate.HasValue ? productObj.DealExpiration.ExpiredDate.Value : new DateTime(1800, 1, 1);
            }

            product.Merchant = new DetailPageMerchant() 
            {
                Id = productObj.Merchant.Id,
                Code = productObj.Merchant.Code,
                DisplayName = productObj.Merchant.DisplayName,
                LogoUrl = (productObj.Merchant.LogoUrl != null ? productObj.Merchant.LogoUrl.Replace("http://", "https://").Replace(":80", "") : ""),
                OnVacation = false,
                AverageRating = 5,
                TotalReviewCount = 0,
                ChatResponseSpeed = "Within an hour",
                NumberOfProducts = 300,
                JoinDate = "5 years ago"


            };

            if (productObj.Merchant.MerchantCarousel != null)
            {
                product.Merchant.MerchantCarouselList = productObj.Merchant.MerchantCarousel.Where(x => x.StatusId == 1).Select(x => new MerchantCarousel
                {
                    Id = x.Id,
                    ImageUrl = x.ImageUrl,
                }).ToList();
            }

            if (productObj.ProductCategory != null) {
                product.ProductCategory = new DetailPageProductCategory()
                {
                    Id = productObj.ProductCategory.Id,
                    Name = productObj.ProductCategory.Name
                };
            }

            if (productObj.ProductSubCategory != null) {
                product.ProductSubCategory = new DetailPageProductSubCategory()
                {
                    Id = productObj.ProductSubCategory.Id,
                    Name = productObj.ProductSubCategory.Name
                };
            }

            product.ProductOutlets = productObj.ProductOutlets.Select(x => new DetailProductOutlets() 
            {
                Id = x.Outlet.Id,
                Name = x.Outlet.Name
            }).ToList();

            product.AdditionalDiscounts = productObj.ProductDiscounts.Where(z => z.IsActivated).Select(x => new AdditionalDiscounts() 
            { 
                Id = x.Id,
                Name= x.Name,
                Type = x.DiscountTypeId,
                Value = x.DiscountTypeId == 1 ? x.PercentageValue : x.PriceValue,
                Points = (short)x.PointRequired,
            }).ToList();

            /*if (!product.Title.ToLower().Contains("voucher"))
            {
                AdditionalDiscounts noneDiscount =
                new AdditionalDiscounts
                {
                    Id = 0,
                    Name = "None",
                    Value = 0,
                };

                product.AdditionalDiscounts.Add(noneDiscount);
            }*/
            product.AdditionalDiscounts.OrderBy(x => x.Value).ToList();

            product.VariationModel = await GetVariant(product.Id);
            return product;
        }
        private async Task<VariationModel> GetVariant(int id)
        {
            var items = await rewardsDBContext.Variations.Include(x => x.VariationOptions).ThenInclude(x => x.VariationCombination).ThenInclude(x => x.ProductVariation).Where(x => x.ProductId == id).ToListAsync();

            VariationModel variationListModel = new VariationModel();
            variationListModel.ProductId = id;
            variationListModel.VariationList = new List<VariationList>();
            variationListModel.ProductVariationDetailsList = new List<ProductVariationDetailsList>();

            foreach (var variation in items)
            {
                VariationList variationModel = new VariationList();
                variationModel.IsFirstVariation = variation.IsFirstVariation;
                variationModel.Name = variation.Name;
                variationModel.ProductId = variation.ProductId;
                variationModel.VariationOptions = new List<VariationOptions>();
                foreach (var variationOption in variation.VariationOptions)
                {
                    VariationOptions variationOptionModel = new VariationOptions();
                    variationOptionModel.Name = variationOption.Name;
                    variationOptionModel.Order = variationOption.Order;
                    variationModel.VariationOptions.Add(variationOptionModel);
                    int variation2Order = 0;
                    // foreach (var productVariation in variationOption.VariationCombination.OrderBy(x => x.ProductVariation.VariationCombination.OptionTwoId))
                    // {
                    //     ProductVariationDetailsList productVariationDetails = new ProductVariationDetailsList();
                    //     productVariationDetails.AvailableQuantity = productVariation.ProductVariation.AvailableQuantity;
                    //     productVariationDetails.Id = productVariation.ProductVariation.Id;
                    //     productVariationDetails.DiscountedPrice = productVariation.ProductVariation.DiscountedPrice.Value;
                    //     productVariationDetails.Price = productVariation.ProductVariation.Price;
                    //     productVariationDetails.Order = variationOption.Order + "," + variation2Order;
                    //     productVariationDetails.IsDiscountedPriceEnabled = productVariation.ProductVariation.IsDiscountedPriceEnabled;
                    //     productVariationDetails.Sku = productVariation.ProductVariation.SKU;
                    //     variationListModel.ProductVariationDetailsList.Add(productVariationDetails);
                    //     variation2Order++;
                    // }
                }
                variationListModel.VariationList.Add(variationModel);

            }
            return variationListModel;
        }

        public async Task<List<string>> GetFromBlob(int id,string containerName)
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


        private class AggRequest
        {
            public string ExternalItemId { get; set; }
            public string ExternalShopId { get; set; }
            public byte ExternalTypeId { get; set; }
        }

        private class ProductResponseModel : ApiResponseViewModel
        {
            public ProductData Data { get; set; }
        }

        private class ProductData
        {
            public Products Product { get; set; }
        }
        private class Products
        {
            public string ExternalItemId { get; set; }
            public string ExternalShopId { get; set; }
            public byte ExternalTypeId { get; set; }
            public string ExternalShopUrl { get; set; }
            public string ExternalItemUrl { get; set; }
            public int Id { get; set; }
            public int MerchantId { get; set; }

            public string Title { get; set; }
            public string Subtitle { get; set; }
            public string Description { get; set; }
            public string AdditionInfo { get; set; }
            public string FinePrintInfo { get; set; }
            public string RedemptionInfo { get; set; }
            public string ImageFolderUrl { get; set; }
            public int? ProductCategoryId { get; set; }
            public int? ProductSubCategoryId { get; set; }
            public int? DealTypeId { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public decimal? Price { get; set; }
            public decimal? ActualPriceForVpoints { get; set; }
            public decimal? DiscountedPrice { get; set; }
            public int? DiscountRate { get; set; }
            public int? PointsRequired { get; set; }
            public int? AvailableQuantity { get; set; }
            public int? DealExpirationId { get; set; }
            public int? LuckyDrawId { get; set; }
            public int StatusTypeId { get; set; }
            public bool IsActivated { get; set; }
            public bool IsProductVariationEnabled { get; set; }
            public string PendingChanges { get; set; }
            public DateTime CreatedAt { get; set; }
            public Guid CreatedByUserId { get; set; }
            public DateTime? LastUpdatedAt { get; set; }
            public Guid? LastUpdatedByUser { get; set; }
            public int? TotalBought { get; set; }
            public string Remarks { get; set; }
            public bool IsPublished { get; set; }
            public decimal Rating { get; set; }
            public List<string> ProductImageList { get; set; }
            public string ProductCartPreviewSmallImage { get; set; }

            public bool IsOriginalGuaranteeProduct { get; set; }

            public int MaxPurchaseLimit { get; set; }
            //public List<SimgleProductImageInfo> ProductImageList { get; set; }

            public DetailPageMerchant Merchant { get; set; }

            public DetailPageProductCategory ProductCategory { get; set; }
            public DetailPageProductSubCategory ProductSubCategory { get; set; }

            public List<DetailProductOutlets> ProductOutlets { get; set; }
            //public List<DetailPageProductDiscounts> ProductDiscounts { get; set; }

            public DetailPageExpirationType DetailPageExpirationType { get; set; }

            //public List<ProductReview> ProductReviewList { get; set; }

            public VariationModel VariationModel { get; set; }

            public List<string> VariationImageList { get; set; }

            public bool IsVPointsMultiplierEnabled { get; set; }
            public decimal VPointsMultiplier { get; set; }
            public decimal VPointsMultiplierCap { get; set; }

            public List<AdditionalDiscounts> AdditionalDiscounts { get; set; }
            public List<Reviews> Reviews { get; set; }
        }

        public class DetailPageExpirationType
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Type { get; set; }
            public int TotalValidDays { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime ExpiredDate { get; set; }
        }

        private class VariationModel
        {
            public int ProductId { get; set; }
            public List<VariationList> VariationList { get; set; }
            public List<ProductVariationDetailsList> ProductVariationDetailsList { get; set; }
        }


        private class VariationList
        {
            public string Name { get; set; }
            public int ProductId { get; set; }
            public bool IsFirstVariation { get; set; }
            public List<VariationOptions> VariationOptions { get; set; }

        }

        public class VariationOptions
        {
            public string Name { get; set; }
            public int Order { get; set; }
        }

        public class ProductVariationDetailsList
        {
            public string ExternalVariationId { get; set; }
            public int Id { get; set; }
            public string Sku { get; set; }
            public int AvailableQuantity { get; set; }
            public string Order { get; set; }
            public decimal Price { get; set; }
            public decimal PriceBeforeDiscount { get; set; }
            public decimal DiscountedPrice { get; set; }
            public bool IsDiscountedPriceEnabled { get; set; }
        }

        public class DetailPageMerchant
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string DisplayName { get; set; }
            public string LogoUrl { get; set; }
            public bool OnVacation { get; set; }
            public double AverageRating { get; set; }
            public int TotalReviewCount { get; set; }
            public string JoinDate { get; set; }
            public string ChatResponseSpeed { get; set; }
            public int NumberOfProducts { get; set; }

            public List<MerchantCarousel> MerchantCarouselList { get; set; }

        }

        public class DetailPageProductCategory
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class DetailPageProductSubCategory
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class DetailProductOutlets
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        /*public class DetailPageProductDiscounts
        {
            public int Id { get; set; }
            public int DiscountTypeId { get; set; }
            public string Name { get; set; }
            public decimal PercentageValue { get; set; }
            public decimal PriceValue { get; set; }
            public int PointRequired { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public bool IsActivated { get; set; }
            public bool IsMultiplierEnabled { get; set; }

        }*/


        public class AdditionalDiscounts
        {
            public int Id { get; set; }

            public int Type { get; set; }

            public string Name { get; set; }
            public decimal Value { get; set; }
            public byte StatusId { get; set; }
            public decimal MaxPrice { get; set; }
            //public decimal DiscountPercentage { get; set; }
            public short Points { get; set; }
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
