using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voupon.Merchant.WebApp.Areas.Admin.Services.Dashboard.Models;
using Voupon.Merchant.WebApp.Areas.Admin.Services.Dashboard.Queries.Single;
using Voupon.Merchant.WebApp.Common.Services.Merchants.Queries;
using Voupon.Merchant.WebApp.Common.Services.Products.Queries;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class DashboardController : BaseAdminController
    {
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpGet]
        [Route("GetDashboard")]
        public async Task<ApiResponseViewModel> GetDashboard()
        {
            ApiResponseViewModel response = await Mediator.Send(new DashboardQuery());
            if (response.Successful)
            {
                if (response.Data != null)
                {
                    var model = (DashboardModel)response.Data;
                    ApiResponseViewModel MerchantPendingReviewResponse = await Mediator.Send(new MerchantListByStatusQuery() { Status = Voupon.Common.Enum.StatusTypeEnum.PENDING_REVIEW });
                    if (MerchantPendingReviewResponse.Successful)
                    {
                        if (MerchantPendingReviewResponse.Data != null)
                        {
                            var merchantlist = (List<Common.Services.Merchants.Models.MerchantModel>)MerchantPendingReviewResponse.Data;
                            model.MerchantPendingReview = merchantlist.Count;
                        }
                    }
                    ApiResponseViewModel MerchantPendingRevisionResponse = await Mediator.Send(new MerchantListByStatusQuery() { Status = Voupon.Common.Enum.StatusTypeEnum.PENDING_REVISION });
                    if (MerchantPendingRevisionResponse.Successful)
                    {
                        if (MerchantPendingRevisionResponse.Data != null)
                        {
                            var merchantlist = (List<Common.Services.Merchants.Models.MerchantModel>)MerchantPendingRevisionResponse.Data;
                            model.MerchantPendingRevision = merchantlist.Count;
                        }
                    }

                    ApiResponseViewModel ProductPendingReviewResponse = await Mediator.Send(new ProductListByStatusQuery() { Status = Voupon.Common.Enum.StatusTypeEnum.PENDING_REVIEW });
                    if (ProductPendingReviewResponse.Successful)
                    {
                        if (ProductPendingReviewResponse.Data != null)
                        {
                            var productlist = (List<Common.Services.Products.Models.ProductModel>)ProductPendingReviewResponse.Data;
                            model.ProductPendingReview = productlist.Count;
                        }
                    }

                    ApiResponseViewModel ProductPendingRevisionResponse = await Mediator.Send(new ProductListByStatusQuery() { Status = Voupon.Common.Enum.StatusTypeEnum.PENDING_REVISION });
                    if (ProductPendingRevisionResponse.Successful)
                    {
                        if (ProductPendingRevisionResponse.Data != null)
                        {
                            var productlist = (List<Common.Services.Products.Models.ProductModel>)ProductPendingRevisionResponse.Data;
                            model.ProductPendingRevision = productlist.Count;
                        }
                    }
                    response.Data = model;
                }
            }
            return response;
        }

        [HttpGet]
        [Route("GetPendingReviewMerchants")]
        public async Task<ApiResponseViewModel> GetPendingReviewMerchants()
        {
            ApiResponseViewModel response = await Mediator.Send(new MerchantListByStatusQuery() {  Status= Voupon.Common.Enum.StatusTypeEnum.PENDING_REVIEW });         
            return response;
        }

        [HttpGet]
        [Route("GetPendingReviewProducts")]
        public async Task<ApiResponseViewModel> GetPendingReviewProducts()
        {
            ApiResponseViewModel response = await Mediator.Send(new ProductListByStatusQuery() { Status = Voupon.Common.Enum.StatusTypeEnum.PENDING_REVIEW });
            return response;
        }

    }

  
}