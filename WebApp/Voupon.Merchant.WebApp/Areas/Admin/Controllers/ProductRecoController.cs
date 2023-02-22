using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Voupon.Merchant.WebApp.ViewModels;
using Voupon.Merchant.WebApp.Areas.Admin.Services.ProductAds.Queries;
using Voupon.Merchant.WebApp.Areas.Admin.ViewModels.ProductAds;
using Voupon.Merchant.WebApp.Areas.Admin.Services.ProductAds.Commands;
using System.Security.Claims;
using Voupon.Merchant.WebApp.Common.Services.ProductDemographicsTarget.Queries;
using Voupon.Merchant.WebApp.Common.Services.ProductDemographicsTarget.Models;
using Voupon.Merchant.WebApp.Common.Services.ProductDemographicsTarget.Command;
using Microsoft.AspNetCore.Identity;
using Voupon.Merchant.WebApp.Infrastructure.Extensions;

namespace Voupon.Merchant.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ProductRecoController : BaseAdminController
    {
        public IActionResult Index()
        {
            return View();
        }
        [Route("ProductSubgroupReco/{productAdId}")]
        public IActionResult ProductSubgroupAds(int? productAdId)
        {
            return View();
        }

        [Route("ProductRecoPartnerDomains/{productAdId}")]
        public IActionResult ProductAdsPartnerDomains(int? productAdId)
        {
            return View();
        }

        [HttpGet]
        [Route("GetProductRecoList")]
        public async Task<ApiResponseViewModel> GetProductAdsList()
        {
            return await Mediator.Send(new ProductAdsListQuery
            {
                AdImpressionCountType = 1
            });
        }
        #region Product Demographics Target

        [HttpGet]
        [Route("GetProductDemographics")]
        public async Task<ApiResponseViewModel> GetProductDemographics()
        {
            ApiResponseViewModel response = await Mediator.Send(new ProductDemographicsListQuery());
            return response;
        }

        [HttpPost]
        [Route("UpdateProductDemographicsTargetCommand/{productId}")]
        public async Task<ApiResponseViewModel> UpdateProductDemographicsTargetCommand([FromBody] List<ProductDemographicTargets> model, int productId)
        {

            ApiResponseViewModel response = new ApiResponseViewModel();

            UpdateProductDemographicsTargetCommand command = new UpdateProductDemographicsTargetCommand();
            command.ProductId = productId;
            command.ProductDemographicTargetsModels = model;
            command.CreatedAt = DateTime.Now;
            command.CreatedByUserId = new Guid(User.Identity.GetUserId());
            response = await Mediator.Send(command);
            return response;
        }
        #endregion
        [Route("NewProductReco")]
        public IActionResult NewProductReco()
        {
            return View();
        }

        [HttpGet]
        [Route("GetProductRecoConfig")]
        public async Task<ApiResponseViewModel> GetProductAdsConfig()
        {
            ApiResponseViewModel response = await Mediator.Send(new GetProductAdsConfigImpressionIdentifierQuery());
            if (response.Successful)
            {
                int ImpressionCountIdentifier = (int)response.Data;

                response.Data = ImpressionCountIdentifier;
            }
            return response;
        }

        [HttpGet]
        [Route("GetNewProductRecoList")]
        public async Task<ApiResponseViewModel> GetNewProductAdsList()
        {
            return await Mediator.Send(new ProductAdsListQuery
            {
                AdImpressionCountType = 2
            });
        }

        [HttpGet]
        [Route("GetServerlessUrl")]
        public async Task<ApiResponseViewModel> GetServerlessUrl()
        {
            ApiResponseViewModel response = await Mediator.Send(new GetServerlessUrlQuery());

            return response;
        }

        [HttpGet]
        [Route("UpdateProductRecoConfig")]
        public async Task<ApiResponseViewModel> UpdateProductAdsConfig(int ImpressionCountIdentifier)
        {
            var UpdatedBy = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
            ApiResponseViewModel response = await Mediator.Send(new UpdateProductAdsConigImpressionIdentifierCommand() { ImpressionCountIdentifier = ImpressionCountIdentifier, UpdatedBy = UpdatedBy });
            return response;
        }

        [HttpPost]
        [Route("UpdateProductRecoDetailsCommand")]
        public async Task<ApiResponseViewModel> UpdateProductAdsDetailsCommand([FromForm] UpdateProductAdsDetailsCommand command)
        {
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpGet]
        [Route("GetProductSubgroupRecoList")]
        public async Task<ApiResponseViewModel> GetProductSubgroupAdsList(int id)
        {
            ApiResponseViewModel response = await Mediator.Send(new ProductAdSubgroupsListQuery() { Id = id});
            return response;
        }

        [HttpGet]
        [Route("GetProductRecoPartnerDomainsList")]
        public async Task<ApiResponseViewModel> GetProductAdsPartnerDomainsList(int id)
        {
            ApiResponseViewModel response = await Mediator.Send(new ProductAdsPartnerDomainsListQuery() { Id = id });
            return response;
        }


        [HttpPost]
        [Route("UpdateProductRecoStatus")]
        public async Task<ApiResponseViewModel> UpdateProductAdStatus([FromForm] UpdateProductAdsStatusCommand command)
        {
            ApiResponseViewModel response = await Mediator.Send(command);
            await Mediator.Send(new ClearProductAdsCacheCommand());
            return response;
        }
    }
}