using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Voupon.Rewards.WebApp.Infrastructures.Enums;
using Voupon.Rewards.WebApp.Services.Deal.Page;
using Voupon.Rewards.WebApp.ViewModels;
using static Voupon.Rewards.WebApp.Services.Deal.Page.DetailPage;
using Voupon.Rewards.WebApp.Services.Deal.Commands;
using Voupon.Rewards.WebApp.Common.Services.ProductVariations.Queries;
using Voupon.Rewards.WebApp.Common.Blob.Queries;
using Voupon.Common.Azure.Blob;
using Voupon.Rewards.WebApp.Common.Services.ProductShipping.Queries;
using Voupon.Rewards.WebApp.Services.Aggregators.Queries;
using Newtonsoft.Json;
using Voupon.Rewards.WebApp.Common.Products.Queries;
using Voupon.Rewards.WebApp.Common.Products.Models;
using Voupon.Rewards.WebApp.Services.AppConfig.Queries;
using Voupon.Rewards.WebApp.Services.Config.Queries;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.Services.Products.Commands;

namespace Voupon.Rewards.WebApp.Controllers
{
    public class DealController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("GetProductReview/{id}")]
        public async Task<IActionResult> GetProductReview(int id, int page)
        {
            int numberOfItem = 30;
            var result = await Mediator.Send(new Common.ProductReview.Queries.GetReviewByIdQuery() { ProductId = id });
            if (result.Successful)
            {
                if (page != 0)
                {
                    var list = (List<Voupon.Rewards.WebApp.Common.ProductReview.Queries.ProductReview>)result.Data;
                    result.Data = list.Skip((page - 1) * numberOfItem).Take(numberOfItem).ToList();
                }
                return Ok(result);
            }
            return BadRequest(result.Message);
        }

        [HttpGet]
        [Route("GetProductReviewByMerchant/{id}")]
        public async Task<IActionResult> GetProductReviewByMerchant(int id, int page)
        {
            int numberOfItem = 30;
            var result = await Mediator.Send(new Common.ProductReview.Queries.GetReviewByMerchantIdQuery() { MerchantId = id });
            if (result.Successful)
            {
                if (page != 0)
                {
                    var list = (List<Voupon.Rewards.WebApp.Common.ProductReview.Queries.ProductReview>)result.Data;
                    result.Data = list.Skip((page - 1) * numberOfItem).Take(numberOfItem).ToList();
                }
                return Ok(result);
            }
            return BadRequest(result.Message);
        }


        [Route("Product/{id}")]
        public async Task<IActionResult> Detail(int? id)
        {
            if (!id.HasValue)
            {
                return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
            }

            var appConfig = await Mediator.Send(new AppConfigQuery());
            var additionalDiscount = await Mediator.Send(new AdditionalDiscountsQuery());

            if (id.Value == 0)
            {
                var externalItemId = Request.Query["i"];
                var externalShopId = Request.Query["s"];
                var externalTypeId = Request.Query["t"];

                if (string.IsNullOrEmpty(externalItemId) || string.IsNullOrEmpty(externalShopId) || string.IsNullOrEmpty(externalTypeId))
                {
                    return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
                }

                var externalResult = await Mediator.Send(new DetailPage
                {
                    Id = id.Value,
                    ExternaId = externalItemId,
                    ExternalTypeId = byte.Parse(externalTypeId),
                    ExternalMerchantId = externalShopId
                });

                if (!externalResult.Successful && externalResult.Code == -1)
                {
                    return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
                }
                if (!externalResult.Successful && externalResult.Code == -2)
                {
                    return View(ErrorPageEnum.Produc_No_Longer_Available);
                }

                var externalModel = (DetailPageViewModel)externalResult.Data;

                if (externalModel.ProductImageList == null || externalModel.ProductImageList.Count() == 0)
                {
                    return View(ErrorPageEnum.Produc_No_Longer_Available);
                }

                externalModel.VPointsMultiplier = appConfig.VPointsMultiplier;
                externalModel.VPointsMultiplierCap = appConfig.VPointsMultiplierCap;
                externalModel.IsVPointsMultiplierEnabled = appConfig.IsVPointsMultiplierEnabled;
                externalModel.AdditionalDiscounts = additionalDiscount.Where(x => externalModel.DiscountedPrice < x.MaxPrice).ToList();
                return View(externalModel);

            }

            var result = await Mediator.Send(new DetailPage
            {
                Id = id.Value
            });

            if (!result.Successful && result.Code == -1)
            {
                return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
            }
            if (!result.Successful && result.Code == -2)
            {
                return View(ErrorPageEnum.Produc_No_Longer_Available);
            }

            var model = (DetailPageViewModel)result.Data;

            if (model.ProductImageList == null || model.ProductImageList.Count() == 0)
            {
                return View(ErrorPageEnum.Produc_No_Longer_Available);
            }

            model.VPointsMultiplier = appConfig.VPointsMultiplier;
            model.VPointsMultiplierCap = appConfig.VPointsMultiplierCap;
            model.IsVPointsMultiplierEnabled = appConfig.IsVPointsMultiplierEnabled;
            model.AdditionalDiscounts = additionalDiscount;

            return View(model);
        }

        [Route("api/v1/deal/related-deal/{id}")]
        public async Task<IActionResult> Detaildd(int? id)
        {
            if (!id.HasValue)
            {
                return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
            }

            var result = await Mediator.Send(new DetailPage
            {
                Id = id.Value
            });

            if (!result.Successful)
            {
                return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
            }

            return View((DetailPageViewModel)result.Data);
        }

        [Route("Product/GetOfferProduct")]
        public async Task<ApiResponseViewModel> GetOfferProduct(int? ProductCategoryId, int? ProductSubCategoryId, int? Id)
        {
            var result = await Mediator.Send(new GetRelatedOffers
            {
                ProductCategoryId = ProductCategoryId.Value,
                ProductSubCategoryId = ProductSubCategoryId.Value,
                Id = Id.Value
            });
            var response = new ApiResponseViewModel();
            if (!result.Successful)
            {
                response.Successful = false;

                response.Message = "fail to get related offer";
                return response;
            }
            response.Successful = true;
            response.Data = result.Data;
            response.Message = "Successfully get related offer";

            return response;
        }

        [HttpPost]
        [Route("product/UpdateProductClickCount/{productId}")]
        public async Task<ApiResponseViewModel> UpdateProductClickCount(int productId)
        {
            long memberProfileId = 0;
            var user = GetTokenData(Request.HttpContext);
            if (user != null)
            {
                memberProfileId = user.MemberProfileId;
            }

            ApiResponseViewModel response = await Mediator.Send(new UpdateProductClickCount() { ProductId = productId, MemberProfileId = memberProfileId });

            return response;
        }

        [HttpGet]
        [Route("Product/GetProductVariation/{productId}")]
        public async Task<ApiResponseViewModel> GetProductVariation(int productId)
        {
            var externalItemId = Request.Query["i"];
            var externalMerchantId = Request.Query["s"];
            var externalTypeId = Request.Query["t"];
            byte externalTypeIdByte = 0;
            if (!string.IsNullOrEmpty(externalTypeId))
            {
                externalTypeIdByte = byte.Parse(externalTypeId);
            }
            return await Mediator.Send(new ProductVariationListQuery() { ProductId = productId, ExternalId = externalItemId, ExternalMerchantId = externalMerchantId, ExternalTypeId = externalTypeIdByte });
        }

        [HttpGet]
        [Route("Product/GetProductVariationImages/{productId}")]
        public async Task<ApiResponseViewModel> GetProductVariationImages(int productId)
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            BlobNormalImagesListQuery command = new BlobNormalImagesListQuery()
            {
                Id = productId,
                ContainerName = ContainerNameEnum.Products,
                FilePath = FilePathEnum.Products_Variation_Images
            }
            ;
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpGet]
        [Route("GetProductShippingCostByProvinceId")]
        public async Task<ApiResponseViewModel> GetProductShippingCost(int productId, int provinceId, string externalItemId, string externalShopId, byte externalTypeId, int quantity, string productTitle, int shareShippingCostSameItem, decimal productPrice)
        {

            GetProductShippingCostByProvinceId command = new GetProductShippingCostByProvinceId() { ProductId = productId, ProvinceId = provinceId, ExternalItemId = externalItemId, ExternalShopId = externalShopId, ExternalTypeId = externalTypeId, Quantity = quantity, ProductTitle = productTitle, ShareShippingCostSameItem = shareShippingCostSameItem, ProductPrice = productPrice };
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpGet]
        [Route("Product/get-Shop-details/{id}")]
        public async Task<ApiResponseViewModel> Get(int? id, bool isExternalProduct, string externalTypeId)
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();

            if (!id.HasValue)
            {
                return apiResponseViewModel;
            }

            if (isExternalProduct)
            {
                var externalShopId = id;

                if (externalShopId == 0 || string.IsNullOrEmpty(externalTypeId))
                {
                    return apiResponseViewModel;
                }

                var shopResult = await Mediator.Send(new AggregatorShopQuery
                {
                    ExternalShopId = externalShopId.ToString(),
                    ExternalTypeId = byte.Parse(externalTypeId),

                });

                if (shopResult.Successful)
                {
                    var productData = JsonConvert.DeserializeObject<MerchantPageViewModel>(shopResult.Data.ToString());
                    apiResponseViewModel.Data = productData;
                    apiResponseViewModel.Successful = true;
                    return apiResponseViewModel;
                }
                else
                {
                    return apiResponseViewModel;
                }

            }
            else
            {
                ApiResponseViewModel response = await Mediator.Send(new GetShopDetails() { Id = id.Value });
                return response;
            }
        }

        [HttpGet]
        [Route("Product/get-product-list-by-merchant")]
        public async Task<ApiResponseViewModel> GetProductListByMerchant(string merchantId, double productId, bool isExternalProduct, byte externalTypeId)
        {
            if (isExternalProduct)
            {
                ApiResponseViewModel response = await Mediator.Send(new GetProductsFromSameShop() { ExternalMerchantId = merchantId, ExternalTypeId = externalTypeId });
                if (response.Successful)
                {
                    var productList = (List<ProductModel>)response.Data;
                    var newList = productList.Where(x => x.IsPublished == true && x.IsActivated == true && (((x.ExpirationTypeId == 1 || x.ExpirationTypeId == 2)) || x.ExpirationTypeId == 4 || x.ExpirationTypeId == 5) && x.Id != productId).OrderBy(x => x.Title).Take(30).ToList();
                    foreach (var item in newList)
                    {
                        ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                        Voupon.Rewards.WebApp.Common.Blob.Queries.BlobSmallImagesListQuery command = new Voupon.Rewards.WebApp.Common.Blob.Queries.BlobSmallImagesListQuery()
                        {
                            Id = item.Id,
                            ContainerName = ContainerNameEnum.Products,
                            FilePath = FilePathEnum.Products_Images
                        };
                        ApiResponseViewModel imageResponse = await Mediator.Send(command);
                        if (imageResponse.Successful)
                        {
                            item.ImageFolderUrl = (List<string>)imageResponse.Data;
                        }
                    }


                    var resultList = new List<SearchProductViewModel>();
                    if (newList != null && newList.Any())
                    {
                        response.Data = newList.Select(x => new SearchProductViewModel
                        {
                            DealExpirationId = x.DealExpirationId,
                            DealType = x.DealType,
                            DealTypeId = x.DealTypeId,
                            DiscountedPrice = x.DiscountedPrice,
                            DiscountRate = x.DiscountRate,
                            ExpirationTypeId = x.ExpirationTypeId,
                            Id = 0,
                            PointsRequired = x.PointsRequired,
                            Price = x.Price,
                            Rating = x.Rating,
                            Title = x.Title,
                            TotalSold = (x.TotalBought.HasValue ? (int)x.TotalBought : 0),
                            ProductSubCategory = x.ProductSubCategory,
                            ProductSubCategoryId = x.ProductSubCategoryId,
                            ProductCategoryId = x.ProductCategoryId,
                            ProductImage = (x.ImageFolderUrl != null && x.ImageFolderUrl.Count() > 0 ? x.ImageFolderUrl[0] : ""),
                            OutletLocation = x.OutletLocation,
                            ExternalItemId = x.ExternalId,
                            ExternalShopId = x.ExternalMerchantId,
                            ExternalTypeId = x.ExternalTypeId

                        });
                    }
                }
                return response;
            }
            else
            {
                ApiResponseViewModel response = await Mediator.Send(new GetProductsFromSameShop() { MerchantId = int.Parse(merchantId) });
                if (response.Successful)
                {
                    var productList = (List<ProductModel>)response.Data;
                    var newList = productList.Where(x => x.IsPublished == true && x.IsActivated == true && (((x.ExpirationTypeId == 1 || x.ExpirationTypeId == 2)) || x.ExpirationTypeId == 4 || x.ExpirationTypeId == 5) && x.Id != productId).OrderBy(x => x.Title).Take(30).ToList();
                    foreach (var item in newList)
                    {
                        ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                        Voupon.Rewards.WebApp.Common.Blob.Queries.BlobSmallImagesListQuery command = new Voupon.Rewards.WebApp.Common.Blob.Queries.BlobSmallImagesListQuery()
                        {
                            Id = item.Id,
                            ContainerName = ContainerNameEnum.Products,
                            FilePath = FilePathEnum.Products_Images
                        };
                        ApiResponseViewModel imageResponse = await Mediator.Send(command);
                        if (imageResponse.Successful)
                        {
                            item.ImageFolderUrl = (List<string>)imageResponse.Data;
                        }
                    }


                    var resultList = new List<SearchProductViewModel>();
                    if (newList != null && newList.Any())
                    {
                        response.Data = newList.Select(x => new SearchProductViewModel
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
                            ProductImage = (x.ImageFolderUrl != null && x.ImageFolderUrl.Count() > 0 ? x.ImageFolderUrl[0] : ""),
                            OutletLocation = x.OutletLocation,
                            ExternalItemId = x.ExternalId,
                            ExternalShopId = x.ExternalMerchantId,
                            ExternalTypeId = x.ExternalTypeId

                        });
                    }
                }
                return response;
            }
        }



        [HttpGet]
        [Route("Product/get-update-value")]
        public async Task<ApiResponseViewModel> GetUpdateValue(int discountId, int quantity, bool isVariant, int variantId, int productId)
        {


            CalculationValueProductCommand command = new CalculationValueProductCommand() { DiscountId = discountId, Quantity = quantity, IsVariant = isVariant, ProductId = productId, VariantId = variantId };
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;

        }
    }
}