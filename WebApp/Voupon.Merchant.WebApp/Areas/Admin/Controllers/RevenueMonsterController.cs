using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voupon.Merchant.WebApp.Areas.Admin.Services.Config.Commands;
using Voupon.Merchant.WebApp.Areas.Admin.Services.Config.Pages;
using Voupon.Merchant.WebApp.Areas.Admin.Services.ErrorLog.Queries.List;
using Voupon.Merchant.WebApp.Common.Services.Products.Command;
using Voupon.Merchant.WebApp.Common.Services.RevenueMonster.Queries;
using Voupon.Merchant.WebApp.Controllers;
using Voupon.Merchant.WebApp.Infrastructure.Enums;
using static Voupon.Merchant.WebApp.Areas.Admin.Services.Config.Pages.IndexPage;

namespace Voupon.Merchant.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class RevenueMonsterController : BaseController
    {
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [Route("get-by-short-id")]
        [HttpPost]
        public async Task<IActionResult> GetbyShortId(RevenueMonsterPaymentStatusByOrderIdCommand command)
        {

            var response = await Mediator.Send(command);
            return Ok(response);
        }

        [Route("generate-order-payment")]
        [HttpPost]
        public async Task<IActionResult> GenerateOrderPayment(RevenueMonsterPaymentStatusByOrderIdCommand command)
        {
            command.GenerateOrderPayment = true;
            var response = await Mediator.Send(command);
            return Ok(response);
        }
    }
}