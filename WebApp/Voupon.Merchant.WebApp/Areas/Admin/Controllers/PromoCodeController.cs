using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voupon.Common.BaseTypes;
using Voupon.Merchant.WebApp.Areas.Admin.Services.Config.Commands;
using Voupon.Merchant.WebApp.Areas.Admin.Services.Config.Pages;
using Voupon.Merchant.WebApp.Areas.Admin.Services.PromoCode.Commands;
using Voupon.Merchant.WebApp.Areas.Admin.Services.PromoCode.Queries.List;
using Voupon.Merchant.WebApp.Areas.Admin.Services.SendInvouicesEmailCommand.Commands;
using Voupon.Merchant.WebApp.Areas.Admin.ViewModels.PromoCode;
using Voupon.Merchant.WebApp.Common.Services.Products.Command;
using Voupon.Merchant.WebApp.Controllers;
using Voupon.Merchant.WebApp.Infrastructure.Enums;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class PromoCodeController : BaseController
    {
        public async Task<IActionResult> Index()
        {
            return View(new IndexPageViewModel());
        }

        [HttpPost]
        [Route("UpdateStatus")]
        public async Task<ApiResponseViewModel> UpdateStatus([FromForm] UpdateStatusCommand command)
        {
            return await Mediator.Send(command);
        }

        [HttpPost]
        [Route("Create")]
        public async Task<ApiResponseViewModel> Create([FromForm] IndexPageViewModel model)
        {
            if (model.AddPromoCodeViewModel.DiscountType == 0)
            {
                model.AddPromoCodeViewModel.DiscountType = 1;
            }

            return await Mediator.Send(new CreateCommand
            {
                Description = model.AddPromoCodeViewModel.Description,
                PromoCode = model.AddPromoCodeViewModel.PromoCode,
                DiscountType = model.AddPromoCodeViewModel.DiscountType,
                DiscountValue = model.AddPromoCodeViewModel.DiscountValue,
                ExpireOn = model.AddPromoCodeViewModel.ExpireOn,
                IsFirstTimeUserOnly = model.AddPromoCodeViewModel.IsFirstTimeUserOnly,
                IsNewSignupUserOnly = model.AddPromoCodeViewModel.IsNewSignupUserOnly,
                IsSelectedUserOnly = model.AddPromoCodeViewModel.IsSelectedUserOnly,
                MaxDiscountValue = model.AddPromoCodeViewModel.MaxDiscountValue,
                MinSpend = model.AddPromoCodeViewModel.MinSpend,
                Status = 1,
                TotalAllowedPerUser = model.AddPromoCodeViewModel.TotalAllowedPerUser,
                TotalRedeemed = 0,
                TotalRedemptionAllowed = model.AddPromoCodeViewModel.TotalRedemptionAllowed,



                //1781
                IsShipCostDeduct = model.AddPromoCodeViewModel.IsShipCostDeduct
            });
        }

        [HttpPost]
        [Route("Update")]
        public async Task<ApiResponseViewModel> Update([FromForm] IndexPageViewModel model)
        {
            return await Mediator.Send(new UpdateCommand
            {
                Description = model.EditPromoCodeViewModel.Description,
                PromoCode = model.EditPromoCodeViewModel.PromoCode,
                DiscountValue = model.EditPromoCodeViewModel.DiscountValue,
                GeneratedId = model.EditPromoCodeViewModel.GeneratedId,
                ExpireOn = model.EditPromoCodeViewModel.ExpireOn,
                IsFirstTimeUserOnly = model.EditPromoCodeViewModel.IsFirstTimeUserOnly,
                IsNewSignupUserOnly = model.EditPromoCodeViewModel.IsNewSignupUserOnly,
                IsSelectedUserOnly = model.EditPromoCodeViewModel.IsSelectedUserOnly,

                //1781
                IsShipCostDeduct = model.EditPromoCodeViewModel.IsShipCostDeduct,

                MaxDiscountValue = model.EditPromoCodeViewModel.MaxDiscountValue,
                MinSpend = model.EditPromoCodeViewModel.MinSpend,
                Status = model.EditPromoCodeViewModel.Status,
                TotalAllowedPerUser = model.EditPromoCodeViewModel.TotalAllowedPerUser,
                TotalRedeemed = model.EditPromoCodeViewModel.Status,
                TotalRedemptionAllowed = model.EditPromoCodeViewModel.TotalRedemptionAllowed
            }); ;
        }

        [Route("list")]
        public async Task<ListView<PromoCodeListQuery.PromoCodeViewModel>> GetList(int pageIndex)
        {
            return await Mediator.Send(new PromoCodeListQuery
            {
                PageIndex = pageIndex,
                PageSize = 1000
            });
        }

        [Route("api/mailinglist")]
        public async Task<ListView<MailingListQuery.MailingListViewModel>> GetMailingList(int pageIndex)
        {
            return await Mediator.Send(new MailingListQuery
            {
                PageIndex = pageIndex,
                PageSize = 1000
            });
        }

        [Route("MailingList")]
        public async Task<IActionResult> MailingList()
        {
            return View();
        }
    }
}