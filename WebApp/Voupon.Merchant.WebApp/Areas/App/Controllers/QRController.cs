using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voupon.Merchant.WebApp.Areas.App.Services.QR.Commands;
using Voupon.Merchant.WebApp.Areas.App.Services.QR.Pages;
using Voupon.Merchant.WebApp.Controllers;
using Voupon.Merchant.WebApp.Infrastructure.Extensions;
using static Voupon.Merchant.WebApp.Areas.App.Services.QR.Pages.QRValidatePage;

namespace Voupon.Merchant.WebApp.Areas.App.Controllers
{
    [Authorize]
    [Route("QR")]
    public class QRController : BaseController
    {
        [Route("v/{token}")]
        public async Task<IActionResult> Index(QRValidatePage page)
        {
            var result = await Mediator.Send(page);

            if (result.Successful)
            {
                return View("~/areas/app/views/qr/validate.cshtml", (QRValidatePageViewModel)result.Data);
            }
            else
            {
                ViewData["Error"] = result.Message;
                return View("~/areas/app/views/qr/validate.cshtml");
            }
        }

        [Route("confirm-redemption")]
        [HttpPost]
        public async Task<IActionResult> ConfirmRedemption(ConfirmQRRedemptionCommand command)
        {
            command.MerchantId = User.Identity.GetMerchantId();
            command.UserId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await Mediator.Send(command);

            if (result.Successful)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result.Message);
            }
        }
    }
}