using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Voupon.Common.Azure.Blob;
using Voupon.Common.Enum;
using Voupon.Merchant.WebApp.Areas.App.Services.Products.Pages;
using Voupon.Merchant.WebApp.Areas.App.Services.Products.ViewModels;
using Voupon.Merchant.WebApp.Areas.App.ViewModels.Products;
using Voupon.Merchant.WebApp.Common.Services.Blob.Commands.Create;
using Voupon.Merchant.WebApp.Common.Services.Blob.Commands.Delete;
using Voupon.Merchant.WebApp.Common.Services.Blob.Queries;
using Voupon.Merchant.WebApp.Common.Services.DealExpirations.Commands;
using Voupon.Merchant.WebApp.Common.Services.DealExpirations.Queries;
using Voupon.Merchant.WebApp.Common.Services.DealTypes.Queries;
using Voupon.Merchant.WebApp.Common.Services.DiscountTypes.Queries;
using Voupon.Merchant.WebApp.Common.Services.ExpirationTypes.Queries;
using Voupon.Merchant.WebApp.Common.Services.Merchants.Queries;
using Voupon.Merchant.WebApp.Common.Services.Outlets.Queries;
using Voupon.Merchant.WebApp.Common.Services.ProductCategories.Queries;
using Voupon.Merchant.WebApp.Common.Services.ProductDemographicsTarget.Command;
using Voupon.Merchant.WebApp.Common.Services.ProductDemographicsTarget.Models;
using Voupon.Merchant.WebApp.Common.Services.ProductDemographicsTarget.Queries;
using Voupon.Merchant.WebApp.Common.Services.ProductDiscounts.Command;
using Voupon.Merchant.WebApp.Common.Services.ProductDiscounts.Queries;
using Voupon.Merchant.WebApp.Common.Services.ProductOutlets.Commands;
using Voupon.Merchant.WebApp.Common.Services.ProductOutlets.Queries;
using Voupon.Merchant.WebApp.Common.Services.Products.Command;
using Voupon.Merchant.WebApp.Common.Services.Products.Models;
using Voupon.Merchant.WebApp.Common.Services.Products.Queries;
using Voupon.Merchant.WebApp.Common.Services.ProductShipping.Models;
using Voupon.Merchant.WebApp.Common.Services.ProductShipping.Queries;
using Voupon.Merchant.WebApp.Common.Services.ProductSubcategories.Queries;
using Voupon.Merchant.WebApp.Common.Services.ProductVariations.Command;
using Voupon.Merchant.WebApp.Common.Services.ProductVariations.Models;
using Voupon.Merchant.WebApp.Common.Services.ProductVariations.Queries;
using Voupon.Merchant.WebApp.Common.Services.StatusTypes.Queries;
using Voupon.Merchant.WebApp.Controllers;
using Voupon.Merchant.WebApp.Infrastructure.Enums;
using Voupon.Merchant.WebApp.ViewModels;
using System.Linq.Dynamic.Core;
using Voupon.Merchant.WebApp.Areas.Admin.Services.ProductAds.Commands;
using Voupon.Merchant.WebApp.Infrastructure.Extensions;

namespace Voupon.Merchant.WebApp.Areas.App.Controllers
{
    [Area("App")]
    [Route("App/[controller]")]
    [Authorize(Roles = "Merchant")]
    public class ProductsController : BaseController
    {
        public IActionResult Index()
        {
            var MerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;
            if (String.IsNullOrEmpty(MerchantId))
            {
                return View(Infrastructure.Enums.ErrorPageEnum.NOT_ALLOWED_PAGE);
                //return BadRequest();
            }
            var merchantId = Int32.Parse(MerchantId);
            return View(new NewProductViewModel() { MerchantId = merchantId });
        }

        [Route("Edit/{productId}")]
        public async Task<IActionResult> Edit(int? productId)
        {
            if (!productId.HasValue)
            {
                return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
            }

            if (!await VerifyUserAccessWithProductId(productId.Value))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return View(ErrorPageEnum.NOT_ALLOWED_PAGE);
            }

            var result = await Mediator.Send(new EditProductPage
            {
                ProductId = productId.Value
            });

            if (!result.Successful)
            {
                return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
            }
            return View((EditProductViewModel)result.Data);
        }

        #region Post
        [HttpPost]
        [Route("AddProduct")]
        public async Task<ApiResponseViewModel> AddProduct([FromForm] CreateDefaultProductCommand command)
        {
            if (!VerifyUserAccess(command.MerchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }

            command.CreatedAt = DateTime.Now;
            command.CreatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpPost]
        [Route("UploadProductImages/{productId}")]
        public async Task<ApiResponseViewModel> UploadProductImages(int productId)
        {
            if (!await VerifyUserAccessWithProductId(productId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            var fileContents = HttpContext.Request.Form.Files;

            if (fileContents == null)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Invalid request";
                return apiResponseViewModel;
            }
            var passValidation = true;
            foreach (var fileContent in fileContents)
            {
                var fileExtention = fileContent.FileName.Substring(fileContent.FileName.LastIndexOf(".")).ToLower();
                if (fileExtention != ".jpg" && fileExtention != ".jpeg" && fileExtention != ".png" && fileExtention != ".jfif")
                {
                    passValidation = false;
                }
            }

            if (passValidation)
            {
                CreateImagesCommand command = new CreateImagesCommand()
                {
                    Id = productId,
                    Files = fileContents,
                    ContainerName = ContainerNameEnum.Products,
                    FilePath = FilePathEnum.Products_Temporary_Images
                };

                ApiResponseViewModel response = await Mediator.Send(command);
                return response;
            }
            else
            {
                ApiResponseViewModel respons = new ApiResponseViewModel();
                respons.Successful = false;
                respons.Message = "Please upload Images having extensions: png, jpeg, jpg, jfif only";
                return respons;
            }
        }

        [HttpPost]
        [Route("DeleteProductImages/{productId}")]
        public async Task<ApiResponseViewModel> DeleteOutletImages(int productId, string[] removeFiles)
        {
            if (!await VerifyUserAccessWithProductId(productId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();

            if (removeFiles == null)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "No files to delete";
                return apiResponseViewModel;
            }
            DeleteFilesCommand command = new DeleteFilesCommand()
            {
                Id = productId,
                Files = removeFiles,
                ContainerName = ContainerNameEnum.Products,
                FilePath = FilePathEnum.Products_Temporary_Images
            }
            ;
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpPost]
        [Route("UpdateProduct")]
        public async Task<ApiResponseViewModel> UpdateProduct([FromForm] UpdateProductPendingChangesCommand command)
        {
            if (!await VerifyUserAccessWithProductId(command.ProductId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }

            #region Verify Input
            //ApiResponseViewModel productQueryResponse = await Mediator.Send(new ProductPendingChangesQuery() { ProductId = command.ProductId });
            //if (productQueryResponse.Successful)
            //{
            //    var product = (ProductModel)productQueryResponse.Data;

            //    if (product.DealTypeId.HasValue)
            //    {
            //        if ((product.DealTypeId == 1 || product.DealTypeId == 3) && !product.PointsRequired.HasValue)
            //        {
            //            ApiResponseViewModel InvalidResponse = new ApiResponseViewModel();
            //            InvalidResponse.Successful = false;
            //            InvalidResponse.Message = "Information not completed";
            //            return InvalidResponse;
            //        }

            //        if (product.DealTypeId == 2 && (!product.Price.HasValue || !product.DiscountedPrice.HasValue))
            //        {
            //            ApiResponseViewModel InvalidResponse = new ApiResponseViewModel();
            //            InvalidResponse.Successful = false;
            //            InvalidResponse.Message = "Information not completed";
            //            return InvalidResponse;
            //        }
            //    }
            //    if (!product.AvailableQuantity.HasValue || !product.DealExpirationId.HasValue || !product.DealTypeId.HasValue || !product.DefaultCommission.HasValue
            //        || String.IsNullOrEmpty(product.Description) || product.Description == "<p><br></p>" || !product.EndDate.HasValue || String.IsNullOrEmpty(product.FinePrintInfo) || product.FinePrintInfo == "<p><br></p>"
            //        //|| String.IsNullOrEmpty(product.ImageFolderUrl) 
            //        || String.IsNullOrEmpty(product.MerchantId.ToString()) || !product.ProductCategoryId.HasValue
            //        || !product.ProductSubCategoryId.HasValue || String.IsNullOrEmpty(product.RedemptionInfo) || product.RedemptionInfo == "<p><br></p>"
            //        || !product.StartDate.HasValue || String.IsNullOrEmpty(product.Title)
            //        )
            //    {
            //        ApiResponseViewModel InvalidResponse = new ApiResponseViewModel();
            //        InvalidResponse.Successful = false;
            //        InvalidResponse.Message = "Information not completed";
            //        return InvalidResponse;
            //    }
            //}
            //else
            //{
            //    ApiResponseViewModel InvalidResponse = new ApiResponseViewModel();
            //    InvalidResponse.Successful = false;
            //    InvalidResponse.Message = "Information not completed";
            //    return InvalidResponse;
            //}
            #endregion

            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            command.UserEmail = User.Identity.GetUserName();
            ApiResponseViewModel response = await Mediator.Send(command);

            var statusResponse = await Mediator.Send(new UpdateProductPendingChangesStatusCommand() { ProductId = command.ProductId, Remarks = "", StatusTypeId = StatusTypeEnum.PENDING_REVIEW, LastUpdatedAt = command.LastUpdatedAt, LastUpdatedByUserId = command.LastUpdatedByUserId, UserEmail = User.Identity.GetUserName() });
            if (statusResponse.Successful)
            {
                ApiResponseViewModel productResponse = await Mediator.Send(new ProductQuery() { ProductId = command.ProductId });
                if (productResponse.Successful)
                {
                    var product = (ProductModel)productResponse.Data;
                    if (product.StatusTypeId != StatusTypeEnum.APPROVED)
                    {
                        ApiResponseViewModel productStatusResponse = await Mediator.Send(new UpdateProductStatusCommand()
                        {
                            ProductId = command.ProductId,
                            StatusTypeId = StatusTypeEnum.PENDING_REVIEW,
                            Remarks = "",
                            LastUpdatedAt = command.LastUpdatedAt,
                            LastUpdatedByUserId = command.LastUpdatedByUserId
                        });
                        if (!productStatusResponse.Successful)
                            return productStatusResponse;
                    }
                }
                return response;
            }
            else
                return statusResponse;
        }

        [HttpPost]
        [Route("SubmitReview")]
        public async Task<ApiResponseViewModel> SubmitReview([FromForm] UpdateProductPendingChangesStatusCommand command)
        {
            if (!await VerifyUserAccessWithProductId(command.ProductId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }

            if (!string.IsNullOrEmpty(command.Remarks))
            {
                if (User.IsInRole("Admin"))
                {
                    command.Remarks = "Admin: " + command.Remarks;
                }
                else
                {
                    command.Remarks = "Merchant: " + command.Remarks;
                }
            }


            command.StatusTypeId = StatusTypeEnum.PENDING_REVIEW;
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            //var response2 = await Mediator.Send(new UpdateProductPendingChangesStatusCommand() { ProductId = command.ProductId, Remarks = command.Remarks, StatusTypeId = command.StatusTypeId, LastUpdatedAt = command.LastUpdatedAt, LastUpdatedByUserId = command.LastUpdatedByUserId });
            if (response.Successful)
            {
                ApiResponseViewModel productResponse = await Mediator.Send(new ProductQuery() { ProductId = command.ProductId });
                if (productResponse.Successful)
                {
                    var product = (ProductModel)productResponse.Data;
                    if (product.StatusTypeId != StatusTypeEnum.APPROVED)
                    {
                        ApiResponseViewModel productStatusResponse = await Mediator.Send(new UpdateProductStatusCommand()
                        {
                            ProductId = command.ProductId,
                            StatusTypeId = StatusTypeEnum.PENDING_REVIEW,
                            Remarks = command.Remarks,
                            LastUpdatedAt = command.LastUpdatedAt,
                            LastUpdatedByUserId = command.LastUpdatedByUserId
                        });
                        if (!productStatusResponse.Successful)
                            return productStatusResponse;


                        var merchantId = product.MerchantId;

                        ApiResponseViewModel merchantResponse = await Mediator.Send(new MerchantQuery() { MerchantId = merchantId });
                        ApiResponseViewModel merchantPendingChangesResponse = await Mediator.Send(new MerchantPendingChangesQuery() { MerchantId = merchantId });
                        if (merchantResponse.Successful && merchantPendingChangesResponse.Successful)
                        {
                            var merchant1 = (Common.Services.Merchants.Models.MerchantModel)merchantResponse.Data;
                            var merchant2 = (Common.Services.Merchants.Models.MerchantModel)merchantPendingChangesResponse.Data;
                            if (!merchant1.IsPublished && merchant1.StatusTypeId == 1 && merchant2.StatusTypeId == 1)
                            {
                                response.Successful = false;
                                response.Message = "Kindly update business info";
                            }
                        }

                    }
                }
                return response;
            }
            else
                return response;
        }

        [HttpPost]
        [Route("UpdateProductDealExpiration")]
        public async Task<ApiResponseViewModel> UpdateProductDealExpiration([FromForm] UpdateProductDealExpirationCommand command)
        {
            if (!await VerifyUserAccessWithProductId(command.ProductId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpPost]
        [Route("UpdateProductOutlets")]
        public async Task<ApiResponseViewModel> UpdateProductOutlets([FromForm] UpdateProductOutletsCommand command)
        {
            if (!await VerifyUserAccessWithProductId(command.ProductId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            command.CreatedAt = DateTime.Now;
            command.CreatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }


        [HttpPost]
        [Route("DeleteProduct")]
        public async Task<ApiResponseViewModel> DeleteProduct([FromForm] DeleteProductCommand command)
        {
            if (!await VerifyUserAccessWithProductId(command.ProductId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }
        [HttpPost]
        [Route("UpdateProductActivatedStatus")]
        public async Task<ApiResponseViewModel> UpdateProductActivatedStatus([FromForm] UpdateProductActivatedStatusCommand command)
        {
            if (!await VerifyUserAccessWithProductId(command.ProductId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }



        #endregion
        #region Get

        [HttpGet]
        [Route("GetProductImages/{productId}")]
        public async Task<ApiResponseViewModel> GetProductImages(int productId)
        {
            if (!await VerifyUserAccessWithProductId(productId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            BlobSmallImagesListQuery command = new BlobSmallImagesListQuery()
            {
                Id = productId,
                ContainerName = ContainerNameEnum.Products,
                FilePath = FilePathEnum.Products_Temporary_Images
            }
            ;
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }


        [HttpGet]
        [Route("GetProductList/{merchantId}")]
        public async Task<ApiResponseViewModel> GetProductList(int merchantId)
        {
            if (!VerifyUserAccess(merchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }

            ApiResponseViewModel response = await Mediator.Send(new MerchantProductListQuery() { MerchantId = merchantId });
            if (response.Successful)
            {
                var newList = (List<ProductModel>)response.Data;
                foreach (var item in newList)
                {
                    ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                    BlobFilesListQuery command = new BlobFilesListQuery()
                    {
                        Id = item.Id,
                        ContainerName = ContainerNameEnum.Products,
                        FilePath = FilePathEnum.Products_Temporary_Images
                    };
                    ApiResponseViewModel imageResponse = await Mediator.Send(command);
                    if (imageResponse.Successful)
                    {
                        if (imageResponse.Data != null && ((List<string>)imageResponse.Data).Count() != 0)
                        {
                            if (((List<string>)imageResponse.Data).Where(x => x.Contains("small_1")).FirstOrDefault() != null)
                            {
                                item.ImageFolderUrl = ((List<string>)imageResponse.Data).Where(x => x.Contains("small_1")).FirstOrDefault();
                            }
                            else
                            {
                                item.ImageFolderUrl = ((List<string>)imageResponse.Data)[0];
                            }
                        }
                    }
                }
                response.Data = newList.OrderBy(x => x.StatusTypePendingChanges[2]).ThenByDescending(x => x.Id);
            }

            return response;
        }

        [HttpGet]
        [Route("GetProductCategoryList")]
        public async Task<ApiResponseViewModel> GetProductCategoryList()
        {
            ApiResponseViewModel response = await Mediator.Send(new ProductCategoryListQuery());
            if (response.Successful && response.Data != null)
            {
                response.Data = (List<Common.Services.ProductCategories.Models.ProductCategoryModel>)response.Data;
            }
            return response;
        }

        [HttpGet]
        [Route("GetProductSubCategoryList/{categoryId}")]
        public async Task<ApiResponseViewModel> GetProductSubCategoryList(int categoryId)
        {
            ApiResponseViewModel response = await Mediator.Send(new ProductSubcategoryListQuery() { CategoryId = categoryId });
            if (response.Successful && response.Data != null)
            {
                response.Data = (List<Common.Services.ProductSubcategories.Models.ProductSubcategoryModel>)response.Data;
            }
            return response;
        }


        [HttpGet]
        [Route("GetProductDealExpiration/{dealExpirationId}")]
        public async Task<ApiResponseViewModel> GetProductDealExpiration(int dealExpirationId)
        {
            ApiResponseViewModel response = await Mediator.Send(new ProductDealExpirationQuery() { DealExpirationId = dealExpirationId });
            return response;
        }

        [HttpGet]
        [Route("GetProductOutletList/{productId}")]
        public async Task<ApiResponseViewModel> GetProductOutletList(int productId)
        {
            if (!await VerifyUserAccessWithProductId(productId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel response = await Mediator.Send(new ProductOutletListQuery() { ProductId = productId });
            return response;
        }

        [HttpGet]
        [Route("GetMerchantOutletList/{merchantId}")]
        public async Task<ApiResponseViewModel> GetMerchantOutletList(int merchantId)
        {
            if (!VerifyUserAccess(merchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel response = await Mediator.Send(new MerchantOutletListQuery() { MerchantId = merchantId });
            return response;
        }

        [HttpGet]
        [Route("GetProduct/{productId}")]
        public async Task<ApiResponseViewModel> GetProduct(int productId)
        {
            if (!await VerifyUserAccessWithProductId(productId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel response = await Mediator.Send(new ProductPendingChangesQuery() { ProductId = productId });
            return response;
        }
        [HttpGet]
        [Route("GetStatusTypeList")]
        public async Task<ApiResponseViewModel> GetStatusTypeList()
        {
            ApiResponseViewModel response = await Mediator.Send(new StatusTypeListQuery());
            return response;
        }

        [HttpGet]
        [Route("GetDealTypeList")]
        public async Task<ApiResponseViewModel> GetDealTypeList()
        {
            ApiResponseViewModel response = await Mediator.Send(new DealTypeListQuery());
            if (response.Successful)
            {
                if (response.Data != null)
                {
                    var MerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;
                    var merchantId = Int32.Parse(MerchantId);
                    if (merchantId == 1)
                    {
                        var list = (List<Common.Services.DealTypes.Models.DealTypeModel>)response.Data;
                        response.Data = list.ToList();
                    }
                    else
                    {
                        var list = (List<Common.Services.DealTypes.Models.DealTypeModel>)response.Data;
                        response.Data = list.Where(x => x.Id == 2).ToList();
                    }
                }
            }
            return response;
        }

        [HttpGet]
        [Route("GetExpirationTypeList")]
        public async Task<ApiResponseViewModel> GetExpirationTypeList()
        {
            ApiResponseViewModel response = await Mediator.Send(new ExpirationTypeListQuery());
            return response;
        }
        #endregion

        private async Task<bool> VerifyUserAccessWithProductId(int productId)
        {

            int merchantId = await QueriesOutletProductMerchantId(productId);
            return VerifyUserAccess(merchantId);
        }

        private async Task<int> QueriesOutletProductMerchantId(int productId)
        {
            int merchantId = 0;
            ApiResponseViewModel response = await Mediator.Send(new ProductPendingChangesQuery() { ProductId = productId });
            if (response.Successful)
            {
                if (response.Data != null)
                {
                    var product = (ProductModel)response.Data;
                    merchantId = product.MerchantId;
                }
            }
            return merchantId;
        }
        private bool VerifyUserAccess(int merchantId)
        {
            bool IsMatch = false;
            try
            {
                var userid = User.Identity.GetUserId();
                if (User.IsInRole("Admin"))
                {
                    IsMatch = true;
                    return IsMatch;
                }
                var userMerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;
                if (Int32.Parse(userMerchantId) == merchantId)
                {
                    IsMatch = true;
                }
            }
            catch (Exception ex)
            {
                IsMatch = false;
            }
            return IsMatch;
        }


        #region Product Discounts
        [HttpPost]
        [Route("UpdateProductDiscountsStatus")]
        public async Task<ApiResponseViewModel> UpdateProductDiscountsStatus([FromForm] UpdateProductDiscountsStatusCommand command)
        {
            var userMerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;
            if (Int32.Parse(userMerchantId) != 1)
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpPost]
        [Route("AddProductDiscount")]
        public async Task<ApiResponseViewModel> AddProductDiscount([FromForm] CreateProductDiscountCommand command)
        {
            var userMerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;
            if (Int32.Parse(userMerchantId) != 1)
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            command.CreatedAt = DateTime.Now;
            command.CreatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpPost]
        [Route("UpdateProductDiscount")]
        public async Task<ApiResponseViewModel> UpdateProductDiscount([FromForm] UpdateProductDiscountCommand command)
        {
            var userMerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;
            if (Int32.Parse(userMerchantId) != 1)
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpGet]
        [Route("GetProductDiscountList/{productId}")]
        public async Task<ApiResponseViewModel> GetProductDiscountList(int productId)
        {
            var userMerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;
            if (Int32.Parse(userMerchantId) != 1)
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel response = await Mediator.Send(new ProductDiscountListQuery() { ProductId = productId });
            return response;
        }

        [HttpGet]
        [Route("GetDiscountTypeList")]
        public async Task<ApiResponseViewModel> GetDiscountTypeList()
        {
            var userMerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;
            if (Int32.Parse(userMerchantId) != 1)
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel response = await Mediator.Send(new DiscountTypeListQuery());
            return response;
        }

        [HttpGet]
        [Route("GetProductDiscount/{Id}")]
        public async Task<ApiResponseViewModel> GetProductDiscount(int Id)
        {
            var userMerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;
            if (Int32.Parse(userMerchantId) != 1)
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel response = await Mediator.Send(new ProductDiscountQuery() { Id = Id });
            return response;
        }


        #endregion


        #region Product Variation

        [HttpGet]
        [Route("GetProductVariation/{productId}")]
        public async Task<ApiResponseViewModel> GetProductVariation(int productId)
        {
            if (!await VerifyUserAccessWithProductId(productId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel response = await Mediator.Send(new ProductVariationListQuery() { ProductId = productId, UserEmail = User.Identity.GetUserName() });

            return response;
        }

        [HttpGet]
        [Route("GetProductVariationImages/{productId}")]
        public async Task<ApiResponseViewModel> GetProductVariationImages(int productId)
        {
            if (!await VerifyUserAccessWithProductId(productId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            BlobSmallImagesListQuery command = new BlobSmallImagesListQuery()
            {
                Id = productId,
                ContainerName = ContainerNameEnum.Products,
                FilePath = FilePathEnum.Products_Variation_Images
            }
            ;
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }


        [HttpPost]
        [Route("AddProductVariationImages/{productId}")]
        public async Task<ApiResponseViewModel> AddProductVariationImages(int productId)
        {
            if (!await VerifyUserAccessWithProductId(productId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }

            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            var fileContents = HttpContext.Request.Form.Files;

            if (fileContents == null)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Invalid request";
                return apiResponseViewModel;
            }
            var passValidation = true;
            foreach (var fileContent in fileContents)
            {
                var fileExtention = fileContent.FileName.Substring(fileContent.FileName.LastIndexOf(".")).ToLower();
                if (fileExtention != ".jpg" && fileExtention != ".jpeg" && fileExtention != ".png" && fileExtention != ".jfif")
                {
                    passValidation = false;
                }
            }

            if (passValidation)
            {
                CreateImagesCommand commandImage = new CreateImagesCommand()
                {
                    Id = productId,
                    Files = fileContents,
                    ContainerName = ContainerNameEnum.Products,
                    FilePath = FilePathEnum.Products_Variation_Images
                };

                ApiResponseViewModel responseImage = await Mediator.Send(commandImage);
                return responseImage;
            }
            else
            {
                ApiResponseViewModel respons = new ApiResponseViewModel();
                respons.Successful = false;
                respons.Message = "Please upload Images having extensions: png, jpeg, jpg, jfif only";
                return respons;
            }
        }

        [HttpPost]
        [Route("DeleteProductVariationImages/{productId}")]
        public async Task<ApiResponseViewModel> DeleteProductVariationImages(int productId, string[] removeFiles)
        {
            if (!await VerifyUserAccessWithProductId(productId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();

            if (removeFiles == null)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "No files to delete";
                return apiResponseViewModel;
            }
            DeleteFilesCommand command = new DeleteFilesCommand()
            {
                Id = productId,
                Files = removeFiles,
                ContainerName = ContainerNameEnum.Products,
                FilePath = FilePathEnum.Products_Variation_Images
            }
            ;
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }


        [HttpPost]
        [Route("UpdateProductVariationStatus/{productId}")]
        public async Task<ApiResponseViewModel> UpdateProductVariationStatus(int productId, bool status)
        {

            ApiResponseViewModel response = new ApiResponseViewModel();

            if (!await VerifyUserAccessWithProductId(productId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            UpdateProductVariationStatusCommand updateProductVariationStatusCommand = new UpdateProductVariationStatusCommand()
            {
                ProductId = productId,
                Status = status,
                IsCallFromMerchant = true
            };

            response = await Mediator.Send(updateProductVariationStatusCommand);

            return response;
        }

        [HttpPost]
        [Route("AddProductVariation")]
        public async Task<ApiResponseViewModel> AddProductVariation([FromBody] VariationModel model)
        {

            ApiResponseViewModel response = new ApiResponseViewModel();

            if (!await VerifyUserAccessWithProductId(model.ProductId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            BlobSmallImagesListQuery getImagesCommand = new BlobSmallImagesListQuery()
            {
                Id = model.ProductId,
                ContainerName = ContainerNameEnum.Products,
                FilePath = FilePathEnum.Products_Variation_Images
            }
           ;
            ApiResponseViewModel getImagesResponse = await Mediator.Send(getImagesCommand);
            if (getImagesResponse.Successful)
            {
                ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                CreateProductVariationCommand command = new CreateProductVariationCommand();
                command.ProductId = model.ProductId;
                command.ProductVariationDetailsList = model.ProductVariationDetailsList;
                command.VariationList = model.VariationList;
                command.ImagesList = (List<string>)getImagesResponse.Data;
                command.CreatedAt = DateTime.Now;
                command.CreatedByUserId = new Guid(User.Identity.GetUserId());
                command.UserEmail = User.Identity.GetUserName();
                command.IsCallFromMerchant = true;
                response = await Mediator.Send(command);
            }
            return response;
        }
        #endregion

        #region Product Demographics Target

        [HttpGet]
        [Route("GetProductDemographicsTarget/{productId}")]
        public async Task<ApiResponseViewModel> GetProductDemographicsTarget(int productId)
        {
            if (!await VerifyUserAccessWithProductId(productId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel response = await Mediator.Send(new ProductDemographicsTargetListQuery() { ProductId = productId });
            return response;
        }

        [HttpPost]
        [Route("UpdateProductDemographicsTargetCommand/{productId}")]
        public async Task<ApiResponseViewModel> UpdateProductDemographicsTargetCommand([FromBody] List<ProductDemographicTargets> model, int productId)
        {

            ApiResponseViewModel response = new ApiResponseViewModel();

            if (!await VerifyUserAccessWithProductId(productId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }


            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            UpdateProductDemographicsTargetCommand command = new UpdateProductDemographicsTargetCommand();
            command.ProductId = productId;
            command.ProductDemographicTargetsModels = model;
            command.CreatedAt = DateTime.Now;
            command.CreatedByUserId = new Guid(User.Identity.GetUserId());
            response = await Mediator.Send(command);
            return response;
        }
        #endregion

        #region Product Shipping Cost

        [HttpGet]
        [Route("GetProductShippingCost/{productId}")]
        public async Task<ApiResponseViewModel> GetProductShippingCost(int productId)
        {
            if (!await VerifyUserAccessWithProductId(productId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            GetProductShippingDetailsQueryList command = new GetProductShippingDetailsQueryList() { ProductId = productId, UserEmail = User.Identity.GetUserName() };
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpPost]
        [Route("UpdateProductShippingCost")]
        public async Task<ApiResponseViewModel> UpdateProductShippingCost([FromBody] ProductShippingCostModel model)
        {

            ApiResponseViewModel response = new ApiResponseViewModel();

            if (!await VerifyUserAccessWithProductId(model.ProductId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }

            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            UpdateProductShippingCostCommand command = new UpdateProductShippingCostCommand();
            command.ProductShippingCostModel = model;
            command.CreatedAt = DateTime.Now;
            command.CreatedByUserId = new Guid(User.Identity.GetUserId());
            command.UserEmail = User.Identity.GetUserName();
            response = await Mediator.Send(command);
            return response;
        }
        #endregion

        [HttpPost]
        [Route("GetProducts/{merchantId}")]
        public async Task<ApiResponseViewModel> GetProducts(int merchantId, string start, string length, string sortColumn, string sortColumnDirection, string searchValue)
        {
            try
            {
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;
                ApiResponseViewModel response = await Mediator.Send(new MerchantProductListQuery() { MerchantId = merchantId });
                if (response.Successful)
                {


                    var productsData = (List<ProductModel>)response.Data;
                    if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                    {
                        productsData = productsData.AsQueryable().OrderBy(sortColumn + " " + sortColumnDirection).ToList();
                    }
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        productsData = productsData.Where(m => m.Title.ToLower().Contains(searchValue)).ToList();
                    }
                    recordsTotal = productsData.Count();
                    var data = productsData.Skip(skip).Take(pageSize).ToList();
                    var jsonData = new { recordsFiltered = data.Count(), recordsTotal = recordsTotal, data = data };
                    response.Data = jsonData;
                    return response;
                }
                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("UpdateProductStatus")]
        public async Task<ApiResponseViewModel> UpdateProductStatus([FromForm] UpdateProductPendingChangesStatusCommand command)
        {
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());


            if (!string.IsNullOrEmpty(command.Remarks))
            {
                if (User.IsInRole("Admin"))
                {
                    command.Remarks = "Vodus Admin: " + command.Remarks;
                }
                else
                {
                    command.Remarks = "Automatic Comment : " + command.Remarks;
                }
            }

            ApiResponseViewModel response = await Mediator.Send(command);
            if (command.StatusTypeId == StatusTypeEnum.APPROVED)
            {

                var statusresponse = await Mediator.Send(new UpdateProductStatusCommand() { ProductId = command.ProductId, Remarks = command.Remarks, StatusTypeId = command.StatusTypeId, LastUpdatedAt = command.LastUpdatedAt, LastUpdatedByUserId = command.LastUpdatedByUserId });
                if (statusresponse.Successful)
                {
                    ApiResponseViewModel productResponse = await Mediator.Send(new ProductPendingChangesQuery() { ProductId = command.ProductId });
                    if (productResponse.Successful)
                    {
                        var product = (ProductModel)productResponse.Data;
                        var documentUrl = product.ImageFolderUrl;
                        // if (!String.IsNullOrEmpty(product.ImageFolderUrl))
                        //{
                        BlobFilesListQuery deleteblobcommand = new BlobFilesListQuery()
                        {
                            Id = command.ProductId,
                            ContainerName = ContainerNameEnum.Products,
                            FilePath = FilePathEnum.Products_Images
                        };
                        ApiResponseViewModel deleteblobresponse = await Mediator.Send(deleteblobcommand);
                        if (deleteblobresponse.Successful)
                        {
                            var fileList = (List<string>)deleteblobresponse.Data;
                            DeleteFilesCommand deleteCommand = new DeleteFilesCommand()
                            {
                                Id = command.ProductId,
                                ContainerName = ContainerNameEnum.Products,
                                FilePath = FilePathEnum.Products_Images,
                                Files = fileList.ToArray()
                            };
                            ApiResponseViewModel deleteresponse = await Mediator.Send(deleteCommand);
                        }


                        BlobFilesListQuery blobcommand = new BlobFilesListQuery()
                        {
                            Id = command.ProductId,
                            ContainerName = ContainerNameEnum.Products,
                            FilePath = FilePathEnum.Products_Temporary_Images
                        };
                        ApiResponseViewModel blobresponse = await Mediator.Send(blobcommand);
                        if (blobresponse.Successful)
                        {
                            var fileList = (List<string>)blobresponse.Data;
                            foreach (var file in fileList)
                            {
                                ApiResponseViewModel documentResponse = await Mediator.Send(new CreateFileWithUrlCommand()
                                {
                                    Id = command.ProductId,
                                    Url = file,
                                    ContainerName = ContainerNameEnum.Products,
                                    FilePath = FilePathEnum.Products_Images
                                });
                            }
                        }
                        //  }
                        if (product.DealTypeId == 2)
                        {
                            product.DiscountRate = Convert.ToInt32(100 * (product.Price - product.DiscountedPrice) / product.Price);
                        }
                        UpdateProductCommand productCommand = new UpdateProductCommand()
                        {
                            ProductId = command.ProductId,
                            LastUpdatedAt = command.LastUpdatedAt,
                            LastUpdatedByUserId = command.LastUpdatedByUserId,
                            ImageFolderUrl = product.ImageFolderUrl,
                            Title = product.Title,
                            Subtitle = product.Subtitle,
                            Description = product.Description,
                            AdditionInfo = product.AdditionInfo,
                            FinePrintInfo = product.FinePrintInfo,
                            RedemptionInfo = product.RedemptionInfo,
                            ProductCategoryId = product.ProductCategoryId,
                            ProductSubCategoryId = product.ProductSubCategoryId,
                            DealTypeId = product.DealTypeId,
                            StartDate = product.StartDate,
                            EndDate = product.EndDate,
                            Price = product.Price,
                            ActualPriceForVpoints = product.ActualPriceForVpoints,
                            DiscountedPrice = product.DiscountedPrice,
                            DiscountRate = product.DiscountRate,
                            PointsRequired = product.PointsRequired,
                            AvailableQuantity = product.AvailableQuantity,
                            IsDiscountedPriceEnabled = product.IsDiscountedPriceEnabled,
                            IsShareShippingDifferentItem = product.IsShareShippingDifferentItem,
                            ShareShippingCostSameItem = product.ShareShippingCostSameItem
                        };
                        ApiResponseViewModel updateProductResponse = await Mediator.Send(productCommand);
                        if (!updateProductResponse.Successful)
                        {
                            return updateProductResponse;
                        }
                        UpdateProductPublishedStatusCommand publishStatusCommand = new UpdateProductPublishedStatusCommand();
                        publishStatusCommand.LastUpdatedAt = command.LastUpdatedAt;
                        publishStatusCommand.LastUpdatedByUserId = command.LastUpdatedByUserId;
                        publishStatusCommand.Status = true;
                        publishStatusCommand.ProductId = command.ProductId;
                        ApiResponseViewModel publishStatusResponse = await Mediator.Send(publishStatusCommand);
                        if (!publishStatusResponse.Successful)
                        {
                            return updateProductResponse;
                        }

                        //  Port products to product ads
                        await Mediator.Send(new CreateProductAdsCommand
                        {
                            ProductId = command.ProductId
                        });
                    }

                }
                else
                    return statusresponse;
            }
            return response;
        }





    }
}