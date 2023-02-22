using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voupon.Merchant.WebApp.Areas.Admin.Services.Analytics.Queries.List;
using Voupon.Merchant.WebApp.Areas.Admin.Services.Config.Commands;
using Voupon.Merchant.WebApp.Areas.Admin.Services.Config.Pages;
using Voupon.Merchant.WebApp.Areas.Admin.Services.Config.Queries.List;
using Voupon.Merchant.WebApp.Areas.Admin.Services.SendInvouicesEmailCommand.Commands;
using Voupon.Merchant.WebApp.Common.Services.Products.Command;
using Voupon.Merchant.WebApp.Controllers;
using Voupon.Merchant.WebApp.Infrastructure.Enums;
using static Voupon.Merchant.WebApp.Areas.Admin.Services.Config.Pages.IndexPage;

namespace Voupon.Merchant.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AnalyticsController : BaseController
    {
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [Route("signup")]
        public async Task<IActionResult> GetSignup(DateTime from, DateTime to)
        {
            var viewModel = await Mediator.Send(new ListSignupQuery
            {
                From = from,
                To = to
            });
            if (!viewModel.Successful)
            {
                new BadRequestObjectResult(viewModel);
            }
            return new OkObjectResult(viewModel);
        }

        [Route("vpoints")]
        public async Task<IActionResult> GetVPoints(DateTime from, DateTime to)
        {
            var viewModel = await Mediator.Send(new ListVPointsQuery
            {
                From = from,
                To = to
            });
            if (!viewModel.Successful)
            {
                new BadRequestObjectResult(viewModel);
            }
            return new OkObjectResult(viewModel);
        }

        [Route("orders")]
        public async Task<IActionResult> GetOrders(DateTime from, DateTime to)
        {
            var viewModel = await Mediator.Send(new ListOrderQuery
            {
                From = from,
                To = to
            });
            if (!viewModel.Successful)
            {
                new BadRequestObjectResult(viewModel);
            }
            return new OkObjectResult(viewModel);
        }


    }
}