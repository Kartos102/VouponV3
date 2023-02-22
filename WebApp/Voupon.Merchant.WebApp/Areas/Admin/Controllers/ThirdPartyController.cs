using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voupon.Merchant.WebApp.Areas.Admin.Services.Config.Commands;
using Voupon.Merchant.WebApp.Areas.Admin.Services.Config.Pages;
using Voupon.Merchant.WebApp.Areas.Admin.Services.ThirdParty.Commands;
using Voupon.Merchant.WebApp.Areas.Admin.Services.ThirdParty.Pages;
using Voupon.Merchant.WebApp.Controllers;
using Voupon.Merchant.WebApp.Infrastructure.Enums;
using static Voupon.Merchant.WebApp.Areas.Admin.Services.Config.Pages.IndexPage;

namespace Voupon.Merchant.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ThirdPartyController : BaseController
    {
        public async Task<IActionResult> Index()
        {
            var viewModel = await Mediator.Send(new ThirdPartyPage());
            if (!viewModel.Successful)
            {
                return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
            }
            return View((List<ThirdPartyTypeViewModel>)viewModel.Data);
        }

        [Route("update-product-status")]
        [HttpPost]
        public async Task<IActionResult> UpdateConfig(UpdateProductStatusCommand command)
        {
            var result = await Mediator.Send(command);
            if (!result.Successful)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }
    }
}