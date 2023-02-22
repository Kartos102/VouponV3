using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voupon.Merchant.WebApp.ViewModels;
using Voupon.Common.Azure.Blob;
using Voupon.Merchant.WebApp.Common.Services.Blob.Commands.Create;
using Voupon.Merchant.WebApp.Common.Services.StatusTypes.Queries;
using Voupon.Merchant.WebApp.Common.Services.ProductCategories.Queries;
using Voupon.Merchant.WebApp.Common.Services.ProductSubcategories.Queries;
using Voupon.Merchant.WebApp.Common.Services.DealTypes.Queries;
using Voupon.Merchant.WebApp.Common.Services.ExpirationTypes.Queries;
using Voupon.Merchant.WebApp.Common.Services.Outlets.Queries;
using Voupon.Merchant.WebApp.Common.Services.Merchants.Queries;
using Voupon.Merchant.WebApp.Common.Services.Products.Queries;
using Voupon.Merchant.WebApp.Common.Services.DealExpirations.Queries;
using Voupon.Merchant.WebApp.Common.Services.Products.Command;
using Voupon.Merchant.WebApp.Common.Services.ProductOutlets.Queries;
using Voupon.Merchant.WebApp.Common.Services.ProductOutlets.Commands;
using Voupon.Merchant.WebApp.Common.Services.ProductCategories.Commands;
using Voupon.Merchant.WebApp.Common.Services.ProductSubcategories.Commands;
using Voupon.Merchant.WebApp.Common.Services.ProductSubcategories.Commandspdate;
using Voupon.Merchant.WebApp.Common.Services.DealExpirations.Commands;
using Voupon.Merchant.WebApp.Common.Services.Blob.Commands.Delete;
using Voupon.Merchant.WebApp.Common.Services.Blob.Queries;
using Voupon.Common.Enum;
using Voupon.Merchant.WebApp.Common.Services.Products.Models;
using Voupon.Merchant.WebApp.Common.Services.DiscountTypes.Queries;
using Voupon.Merchant.WebApp.Common.Services.ProductDiscounts.Queries;
using Voupon.Merchant.WebApp.Common.Services.ProductDiscounts.Command;
using Voupon.Merchant.WebApp.Common.Services.LuckyDraws.Queries;
using Voupon.Merchant.WebApp.Common.Services.Postcodes.Queries;
using Voupon.Merchant.WebApp.Common.Services.Districts.Queries;
using Voupon.Merchant.WebApp.Common.Services.Provinces.Queries;
using Voupon.Merchant.WebApp.Common.Services.Countries.Queries;
using Voupon.Merchant.WebApp.Areas.Admin.ViewModels.Products;
using Voupon.Merchant.WebApp.Common.Services.Outlets.Commands;
using Voupon.Merchant.WebApp.Infrastructure.Enums;
using Voupon.Merchant.WebApp.Areas.Admin.Services.Products.Pages;
using Voupon.Merchant.WebApp.Common.Services.ProductVariations.Command;
using Voupon.Merchant.WebApp.Common.Services.ProductVariations.Models;
using Voupon.Merchant.WebApp.Common.Services.ProductVariations.Queries;
using Voupon.Merchant.WebApp.Common.Services.ProductDemographicsTarget.Command;
using Voupon.Merchant.WebApp.Common.Services.ProductDemographicsTarget.Models;
using Voupon.Merchant.WebApp.Common.Services.ProductDemographicsTarget.Queries;
using Voupon.Merchant.WebApp.Common.Services.ProductShipping.Queries;
using Voupon.Merchant.WebApp.Common.Services.ProductShipping.Models;
using System.Linq.Dynamic.Core;
using Voupon.Merchant.WebApp.Areas.Admin.Services.ProductAds.Commands;
using Microsoft.AspNetCore.Identity;
using Voupon.Merchant.WebApp.Infrastructure.Extensions;

namespace Voupon.Merchant.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : BaseAdminController
    {
        public IActionResult Index()
        {
            return View();
        }

        [Route("Edit/{productId}")]
        public async Task<IActionResult> Edit(int? productId)
        {
            if (!productId.HasValue)
            {
                return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
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

        [Route("DealTypes")]
        public async Task<IActionResult> DealTypes()
        {
            var response = await Mediator.Send(new DealTypeListQuery());
            return View(response.Data);
        }

        [Route("ExpirationTypes")]
        public async Task<IActionResult> ExpirationTypes()
        {
            var response = await Mediator.Send(new ExpirationTypeListQuery());
            return View(response.Data);
        }

        [Route("ProductCategories")]
        public async Task<IActionResult> ProductCategories()
        {
            var response = await Mediator.Send(new ProductCategoryListQuery());
            return View(response.Data);
        }

        [Route("ProductSubCategories")]
        public async Task<IActionResult> ProductSubCategories()
        {
            var response = await Mediator.Send(new ProductSubcategoryListQuery());
            return View(response.Data);
        }



        #region Get
        [HttpGet]
        [Route("GetProductPendingChangesImages/{productId}")]
        public async Task<ApiResponseViewModel> GetProductPendingChangesImages(int productId)
        {
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
        [Route("GetProductImages/{productId}")]
        public async Task<ApiResponseViewModel> GetProductImages(int productId)
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();

            BlobSmallImagesListQuery command = new BlobSmallImagesListQuery()
            {
                Id = productId,
                ContainerName = ContainerNameEnum.Products,
                FilePath = FilePathEnum.Products_Images
            }
            ;
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }


        [HttpGet]
        [Route("GetDealTypeList")]
        public async Task<ApiResponseViewModel> GetDealTypeList()
        {
            ApiResponseViewModel response = await Mediator.Send(new DealTypeListQuery());
            return response;
        }

        [HttpGet]
        [Route("GetDiscountTypeList")]
        public async Task<ApiResponseViewModel> GetDiscountTypeList()
        {
            ApiResponseViewModel response = await Mediator.Send(new DiscountTypeListQuery());
            return response;
        }

        [HttpGet]
        [Route("GetProductDiscountList/{productId}")]
        public async Task<ApiResponseViewModel> GetProductDiscountList(int productId)
        {
            ApiResponseViewModel response = await Mediator.Send(new ProductDiscountListQuery() { ProductId = productId });
            return response;
        }

        [HttpGet]
        [Route("GetProductLuckyDraw/{productId}")]
        public async Task<ApiResponseViewModel> GetProductLuckyDraw(int productId)
        {
            ApiResponseViewModel response = await Mediator.Send(new LuckyDrawQuery() { ProductId = productId });
            return response;
        }


        [HttpGet]
        [Route("GetProductDiscount/{Id}")]
        public async Task<ApiResponseViewModel> GetProductDiscount(int Id)
        {
            ApiResponseViewModel response = await Mediator.Send(new ProductDiscountQuery() { Id = Id });
            return response;
        }



        [HttpGet]
        [Route("GetExpirationTypeList")]
        public async Task<ApiResponseViewModel> GetExpirationTypeList()
        {
            ApiResponseViewModel response = await Mediator.Send(new ExpirationTypeListQuery());
            return response;
        }


        [HttpGet]
        [Route("GetMerchantList")]
        public async Task<ApiResponseViewModel> GetMerchantList()
        {
            ApiResponseViewModel response = await Mediator.Send(new MerchantListQuery());
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
            ApiResponseViewModel response = await Mediator.Send(new ProductOutletListQuery() { ProductId = productId });
            return response;
        }

        [HttpGet]
        [Route("GetMerchantOutletList/{merchantId}")]
        public async Task<ApiResponseViewModel> GetMerchantOutletList(int merchantId)
        {
            ApiResponseViewModel response = await Mediator.Send(new MerchantOutletListQuery() { MerchantId = merchantId });
            return response;
        }

        [HttpGet]
        [Route("GetProduct/{productId}")]
        public async Task<ApiResponseViewModel> GetProduct(int productId)
        {
            ApiResponseViewModel response = await Mediator.Send(new ProductQuery() { ProductId = productId });
            if (response.Successful)
            {
                var product = (ProductModel)response.Data;
                ApiResponseViewModel productResponse = await Mediator.Send(new ProductPendingChangesQuery() { ProductId = product.Id });
                if (productResponse.Successful)
                {
                    var tempProduct = (ProductModel)productResponse.Data;
                    product.StatusTypeId = tempProduct.StatusTypeId;
                    product.StatusType = tempProduct.StatusType;
                    product.Remarks = tempProduct.Remarks;
                }
                response.Data = product;
            }
            return response;
        }

        [HttpGet]
        [Route("GetProductPendingChanges/{productId}")]
        public async Task<ApiResponseViewModel> GetProductPendingChanges(int productId)
        {
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
        [Route("GetProductList")]
        public async Task<ApiResponseViewModel> GetProductList()
        {
            ApiResponseViewModel response = await Mediator.Send(new ProductListQuery());
            if (response.Successful)
            {
                var productlist = (List<ProductModel>)response.Data;
                foreach (var product in productlist)
                {
                    ApiResponseViewModel productResponse = await Mediator.Send(new ProductPendingChangesQuery() { ProductId = product.Id });
                    if (productResponse.Successful)
                    {
                        var tempProduct = (ProductModel)productResponse.Data;
                        product.StatusTypeId = tempProduct.StatusTypeId;
                        product.StatusType = tempProduct.StatusType;
                        product.Remarks = tempProduct.Remarks;
                    }
                }
                var grouplist = productlist.GroupBy(x => x.StatusTypeId);
                List<ProductModel> newlist = new List<ProductModel>();
                var pendingReviewList = grouplist.FirstOrDefault(x => x.Key == StatusTypeEnum.PENDING_REVIEW);
                var pendingRevisionList = grouplist.FirstOrDefault(x => x.Key == StatusTypeEnum.PENDING_REVISION);
                var approveList = grouplist.FirstOrDefault(x => x.Key == StatusTypeEnum.APPROVED);
                var draftList = grouplist.FirstOrDefault(x => x.Key == StatusTypeEnum.DRAFT);
                if (pendingReviewList != null)
                {
                    newlist.AddRange(pendingReviewList.OrderBy(x => x.MerchantId).OrderByDescending(x => x.CreatedAt));
                }

                if (pendingRevisionList != null)
                {
                    newlist.AddRange(pendingRevisionList.OrderBy(x => x.MerchantId).OrderByDescending(x => x.CreatedAt));
                }

                if (approveList != null)
                {
                    newlist.AddRange(approveList.OrderBy(x => x.MerchantId).OrderByDescending(x => x.CreatedAt));
                }
                if (draftList != null)
                {
                    newlist.AddRange(draftList.OrderBy(x => x.MerchantId).OrderByDescending(x => x.CreatedAt));
                }
                response.Data = newlist;// productlist.OrderBy(x=>x.Id).ToList();
            }
            return response;
        }

        [HttpGet]
        [Route("GetTestMerchantProductList")]
        public async Task<ApiResponseViewModel> GetTestMerchantProductList()
        {
            ApiResponseViewModel response = await Mediator.Send(new TestMerchantProductListQuery());
            if (response.Successful)
            {
                var productlist = (List<ProductModel>)response.Data;
                foreach (var product in productlist)
                {
                    if (product.StatusTypeId == StatusTypeEnum.APPROVED)
                    {
                        ApiResponseViewModel productResponse = await Mediator.Send(new ProductPendingChangesQuery() { ProductId = product.Id });
                        if (productResponse.Successful)
                        {
                            var tempProduct = (ProductModel)productResponse.Data;
                            if (tempProduct.StatusTypeId == StatusTypeEnum.PENDING_REVIEW)
                            {
                                product.StatusTypeId = tempProduct.StatusTypeId;
                                product.StatusType = tempProduct.StatusType;
                                product.Remarks = tempProduct.Remarks;
                            }
                        }
                    }
                }
                var grouplist = productlist.GroupBy(x => x.StatusTypeId);
                List<ProductModel> newlist = new List<ProductModel>();
                var pendingReviewList = grouplist.FirstOrDefault(x => x.Key == StatusTypeEnum.PENDING_REVIEW);
                var pendingRevisionList = grouplist.FirstOrDefault(x => x.Key == StatusTypeEnum.PENDING_REVISION);
                var approveList = grouplist.FirstOrDefault(x => x.Key == StatusTypeEnum.APPROVED);
                var draftList = grouplist.FirstOrDefault(x => x.Key == StatusTypeEnum.DRAFT);
                if (pendingReviewList != null)
                {
                    newlist.AddRange(pendingReviewList.OrderBy(x => x.MerchantId).OrderByDescending(x => x.CreatedAt));
                }

                if (pendingRevisionList != null)
                {
                    newlist.AddRange(pendingRevisionList.OrderBy(x => x.MerchantId).OrderByDescending(x => x.CreatedAt));
                }

                if (approveList != null)
                {
                    newlist.AddRange(approveList.OrderBy(x => x.MerchantId).OrderByDescending(x => x.CreatedAt));
                }
                if (draftList != null)
                {
                    newlist.AddRange(draftList.OrderBy(x => x.MerchantId).OrderByDescending(x => x.CreatedAt));
                }
                response.Data = newlist;// productlist.OrderBy(x=>x.Id).ToList();
            }
            return response;
        }

        [HttpGet]
        [Route("GetProductCategoryList")]
        public async Task<ApiResponseViewModel> GetProductCategoryList()
        {
            ApiResponseViewModel response = await Mediator.Send(new ProductCategoryListQuery());
            return response;
        }

        [HttpGet]
        [Route("GetProductSubCategoryList/{categoryId}")]
        public async Task<ApiResponseViewModel> GetProductSubCategoryList(int categoryId)
        {
            ApiResponseViewModel response = await Mediator.Send(new ProductSubcategoryListQuery() { CategoryId = categoryId });
            return response;
        }

        [HttpGet]
        [Route("GetPostcodeList/{districtId}")]
        public async Task<ApiResponseViewModel> GetPostcodeList(int districtId)
        {
            ApiResponseViewModel response = await Mediator.Send(new PostcodeListQuery() { DistrictId = districtId });
            return response;
        }

        [HttpGet]
        [Route("GetDistrictList/{provinceId}")]
        public async Task<ApiResponseViewModel> GetDistrictList(int provinceId)
        {
            ApiResponseViewModel response = await Mediator.Send(new DistrictListQuery() { ProvinceId = provinceId });
            return response;
        }

        [HttpGet]
        [Route("GetProvinceList/{countryId}")]
        public async Task<ApiResponseViewModel> GetProvinceList(int countryId)
        {
            ApiResponseViewModel response = await Mediator.Send(new ProvinceListQuery() { CountryId = countryId });
            return response;
        }

        [HttpGet]
        [Route("GetCountryList")]
        public async Task<ApiResponseViewModel> GetCountryList()
        {
            ApiResponseViewModel response = await Mediator.Send(new CountryListQuery());
            return response;
        }
        #endregion

        #region Post
        [HttpPost]
        [Route("UpdateProductDefaultCommission")]
        public async Task<ApiResponseViewModel> UpdateProductDefaultCommission([FromForm] UpdateProductDefaultCommissionCommand command)
        {
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpPost]
        [Route("DeleteOutletImages/{outletId}")]
        public async Task<ApiResponseViewModel> DeleteOutletImages(int outletId, string[] removeFiles)
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();

            if (removeFiles == null)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "No files to delete";
                return apiResponseViewModel;
            }
            DeleteFilesCommand command = new DeleteFilesCommand()
            {
                Id = outletId,
                Files = removeFiles,
                ContainerName = ContainerNameEnum.Outlets,
                FilePath = FilePathEnum.Outlets_Images
            }
            ;
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpPost]
        [Route("UpdateOutlet")]
        public async Task<IActionResult> UpdateOutlet(UpdateOutletCommand command)
        {
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            var result = await Mediator.Send(command);
            if (result.Successful)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost]
        [Route("CreateOutlet")]
        public async Task<IActionResult> CreateOutlet(CreateOutletCommand command)
        {
            command.IsActivated = true;
            command.CreatedAt = DateTime.Now;
            command.CreatedByUserId = new Guid(User.Identity.GetUserId());
            var result = await Mediator.Send(command);
            if (result.Successful)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }


        [HttpPost]
        [Route("UploadOutletImages/{outletId}")]
        public async Task<ApiResponseViewModel> UploadOutletImages(int outletId)
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            var fileContents = HttpContext.Request.Form.Files;

            if (fileContents == null)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Invalid request";
                return apiResponseViewModel;
            }
            CreateImagesCommand command = new CreateImagesCommand()
            {
                Id = outletId,
                Files = fileContents,
                ContainerName = ContainerNameEnum.Outlets,
                FilePath = FilePathEnum.Outlets_Images
            }
            ;
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }


        [HttpPost]
        [Route("DeleteProductImages/{productId}")]
        public async Task<ApiResponseViewModel> DeleteProductImages(int productId, string[] removeFiles)
        {
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
                FilePath = FilePathEnum.Products_Images
            }
            ;
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }


        [HttpPost]
        [Route("UploadProductImages/{productId}")]
        public async Task<ApiResponseViewModel> UploadProductImages(int productId)
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            var fileContents = HttpContext.Request.Form.Files;

            if (fileContents == null)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Invalid request";
                return apiResponseViewModel;
            }
            CreateImagesCommand command = new CreateImagesCommand()
            {
                Id = productId,
                Files = fileContents,
                ContainerName = ContainerNameEnum.Products,
                FilePath = FilePathEnum.Products_Images
            }
            ;
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }
        [HttpPost]
        [Route("UploadProductImagesForOldImagesTemp")]
        public async Task<ApiResponseViewModel> UploadProductImagesForOldImagesTemp()
        {
            ApiResponseViewModel response = new ApiResponseViewModel();

            ApiResponseViewModel responseProducts = await Mediator.Send(new ProductListQuery());
            if (responseProducts.Successful)
            {
                var productlist = (List<ProductModel>)responseProducts.Data;
                foreach (var product in productlist)
                {


                    //int productId = 135;

                    CreateImagesCommandForExistingImages command = new CreateImagesCommandForExistingImages()
                    {
                        Id = product.Id,
                        //Files = fileContents,
                        ContainerName = ContainerNameEnum.Products,
                        FilePath = FilePathEnum.Products_Images
                    }
                    ;
                    response = await Mediator.Send(command);
                }

            }

            //// For Testing Single Id
            //int productId = 82;

            //CreateImagesCommandForExistingImages command = new CreateImagesCommandForExistingImages()
            //{
            //    Id = productId,
            //    //Files = fileContents,
            //    ContainerName = ContainerNameEnum.Products,
            //    FilePath = FilePathEnum.Products_Images
            //}
            //;
            //response = await Mediator.Send(command);

            return response;
        }
        [HttpPost]
        [Route("DeleteResizedImagesAndKeepOriginal")]
        public async Task<ApiResponseViewModel> DeleteResizedImagesAndKeepOriginal()
        {
            ApiResponseViewModel response = new ApiResponseViewModel();

            ApiResponseViewModel responseProducts = await Mediator.Send(new ProductListQuery());
            if (responseProducts.Successful)
            {
                var productlist = (List<ProductModel>)responseProducts.Data;
                foreach (var product in productlist)
                {


                    //int productId = 135;

                    DeleteResizedImagesAndKeepOriginal command = new DeleteResizedImagesAndKeepOriginal()
                    {
                        Id = product.Id,
                        //Files = fileContents,
                        ContainerName = ContainerNameEnum.Products,
                        FilePath = FilePathEnum.Products_Images
                    }
                    ;
                    response = await Mediator.Send(command);
                }

            }


            //// For Testing Single Id
            //int productId = 79;

            //DeleteResizedImagesAndKeepOriginal command = new DeleteResizedImagesAndKeepOriginal()
            //{
            //    Id = productId,
            //    //Files = fileContents,
            //    ContainerName = ContainerNameEnum.Products,
            //    FilePath = FilePathEnum.Products_Images
            //}
            //;
            //response = await Mediator.Send(command);

            return response;
        }

        [HttpPost]
        [Route("UpdateProductPublishedStatus")]
        public async Task<ApiResponseViewModel> UpdateProductPublishedStatus([FromForm] UpdateProductPublishedStatusCommand command)
        {
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpPost]
        [Route("UpdateProductActivatedStatus")]
        public async Task<ApiResponseViewModel> UpdateProductActivatedStatus([FromForm] UpdateProductActivatedStatusCommand command)
        {
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpPost]
        [Route("UpdateProductDiscount")]
        public async Task<ApiResponseViewModel> UpdateProductDiscount([FromForm] UpdateProductDiscountCommand command)
        {
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }



        [HttpPost]
        [Route("UpdateLuckyDrawDate")]
        public async Task<ApiResponseViewModel> UpdateLuckyDrawDate([FromForm] UpdateLuckyDrawDateCommand command)
        {
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpPost]
        [Route("UpdateLuckyDrawWinningTicket")]
        public async Task<ApiResponseViewModel> UpdateLuckyDrawWinningTicket([FromForm] UpdateLuckyDrawWinningTicketCommand command)
        {
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }


        [HttpPost]
        [Route("UpdateProduct")]
        public async Task<ApiResponseViewModel> UpdateProduct([FromForm] UpdateProductCommand command)
        {
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            if (command.DealTypeId == 2)
            {
                if (command.Price == 0)
                    command.DiscountRate = 0;
                else
                    command.DiscountRate = Convert.ToInt32(100 * (command.Price - command.DiscountedPrice) / command.Price);
            }
            ApiResponseViewModel response = await Mediator.Send(command);
            if (response.Successful)
            {
                if (!string.IsNullOrEmpty(command.ImageFolderUrl))
                {
                    BlobFilesListQuery deleteblobcommand = new BlobFilesListQuery()
                    {
                        Id = command.ProductId,
                        ContainerName = ContainerNameEnum.Products,
                        FilePath = FilePathEnum.Products_Temporary_Images
                    };
                    ApiResponseViewModel deleteblobresponse = await Mediator.Send(deleteblobcommand);
                    if (deleteblobresponse.Successful)
                    {
                        var fileList = (List<string>)deleteblobresponse.Data;
                        DeleteFilesCommand deleteCommand = new DeleteFilesCommand()
                        {
                            Id = command.ProductId,
                            ContainerName = ContainerNameEnum.Products,
                            FilePath = FilePathEnum.Products_Temporary_Images,
                            Files = fileList.ToArray()
                        };
                        ApiResponseViewModel deleteresponse = await Mediator.Send(deleteCommand);
                    }


                    BlobFilesListQuery blobcommand = new BlobFilesListQuery()
                    {
                        Id = command.ProductId,
                        ContainerName = ContainerNameEnum.Products,
                        FilePath = FilePathEnum.Products_Images
                    };
                    ApiResponseViewModel blobresponse = await Mediator.Send(blobcommand);
                    if (blobresponse.Successful)
                    {
                        var fileList = (List<string>)blobresponse.Data;
                        foreach (var file in fileList)
                        {
                            var documentResponse = await Mediator.Send(new CreateFileWithUrlCommand()
                            {
                                Id = command.ProductId,
                                Url = file,
                                ContainerName = ContainerNameEnum.Products,
                                FilePath = FilePathEnum.Products_Temporary_Images
                            });
                        }
                    }
                }
            }
            return response;
        }


        [HttpPost]
        [Route("AddProduct")]
        public async Task<ApiResponseViewModel> AddProduct([FromForm] CreateDefaultProductCommand command)
        {
            command.CreatedAt = DateTime.Now;
            command.CreatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }


        [HttpPost]
        [Route("AddProductDiscount")]
        public async Task<ApiResponseViewModel> AddProductDiscount([FromForm] CreateProductDiscountCommand command)
        {
            command.CreatedAt = DateTime.Now;
            command.CreatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
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
                    command.Remarks = "Merchant comment: " + command.Remarks;
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

        [HttpPost]
        [Route("UpdateProductDealExpiration")]
        public async Task<ApiResponseViewModel> UpdateProductDealExpiration([FromForm] UpdateProductDealExpirationCommand command)
        {
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpPost]
        [Route("UpdateProductDiscountsStatus")]
        public async Task<ApiResponseViewModel> UpdateProductDiscountsStatus([FromForm] UpdateProductDiscountsStatusCommand command)
        {
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpPost]
        [Route("UpdateProductOutlets")]
        public async Task<ApiResponseViewModel> UpdateProductOutlets([FromForm] UpdateProductOutletsCommand command)
        {
            command.CreatedAt = DateTime.Now;
            command.CreatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpPost]
        [Route("UpdateProductCategoryStatus")]
        public async Task<ApiResponseViewModel> UpdateProductCategoryStatus([FromForm] UpdateProductCategoryStatusCommand command)
        {
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpPost]
        [Route("UpdateProductSubCategoryStatus")]
        public async Task<ApiResponseViewModel> UpdateProductSubCategoryStatus([FromForm] UpdateProductSubCategoryStatusCommand command)
        {
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }
        [HttpPost]
        [Route("AddProductSubCategory")]
        public async Task<ApiResponseViewModel> AddProductSubCategory([FromForm] AddProductSubCategoryCommand command)
        {
            command.CreatedAt = DateTime.Now;
            command.CreatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }


        [HttpPost]
        [Route("UpdateProductSubCategory")]
        public async Task<ApiResponseViewModel> UpdateProductSubCategory([FromForm] UpdateProductSubCategoryCommand command)
        {
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }


        [HttpPost]
        [Route("AddProductCategory")]
        public async Task<ApiResponseViewModel> AddProductCategory([FromForm] AddProductCategoryCommand command)
        {
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpPost]
        [Route("UpdateProductCategory")]
        public async Task<ApiResponseViewModel> UpdateProductCategory([FromForm] UpdateProductCategoryCommand command)
        {
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
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
            ApiResponseViewModel response = await Mediator.Send(new ProductVariationListQuery() { ProductId = productId });

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
                IsCallFromMerchant = false
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
                command.IsCallFromMerchant = false;
                response = await Mediator.Send(command);
            }
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
        [Route("GetProducts")]
        public async Task<ApiResponseViewModel> GetProducts(int merchantId, string start, string length, string sortColumn, string sortColumnDirection, string searchValue , string method)
        {
            try
            {
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;
                ApiResponseViewModel response = await Mediator.Send(new ProductListAdminQuery() { method = method , searchValue = searchValue });
                if (response.Successful)
                {
                    var productlist = (List<ProductAdminModel>)response.Data;
                    var grouplist = productlist.GroupBy(x => x.StatusTypeId);
                    List<ProductAdminModel> newlist = new List<ProductAdminModel>();
                    var pendingReviewList = grouplist.FirstOrDefault(x => x.Key == StatusTypeEnum.PENDING_REVIEW);
                    var pendingRevisionList = grouplist.FirstOrDefault(x => x.Key == StatusTypeEnum.PENDING_REVISION);
                    var approveList = grouplist.FirstOrDefault(x => x.Key == StatusTypeEnum.APPROVED);
                    var draftList = grouplist.FirstOrDefault(x => x.Key == StatusTypeEnum.DRAFT);
                    if (pendingReviewList != null)
                    {
                        newlist.AddRange(pendingReviewList.OrderBy(x => x.MerchantId).OrderByDescending(x => x.CreatedAt));
                    }

                    if (pendingRevisionList != null)
                    {
                        newlist.AddRange(pendingRevisionList.OrderBy(x => x.MerchantId).OrderByDescending(x => x.CreatedAt));
                    }

                    if (approveList != null)
                    {
                        newlist.AddRange(approveList.OrderBy(x => x.MerchantId).OrderByDescending(x => x.CreatedAt));
                    }
                    if (draftList != null)
                    {
                        newlist.AddRange(draftList.OrderBy(x => x.MerchantId).OrderByDescending(x => x.CreatedAt));
                    }
                    var productsData = newlist;
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
    }
}