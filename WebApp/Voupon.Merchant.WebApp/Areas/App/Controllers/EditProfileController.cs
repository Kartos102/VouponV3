using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voupon.Merchant.WebApp.Infrastructure.Enums;
using Voupon.Merchant.WebApp.Services.Profile.Page;
using Voupon.Merchant.WebApp.Services.Identity.Commands.Update;
using Voupon.Merchant.WebApp.Controllers;

namespace Voupon.Merchant.WebApp.Areas.App.Controllers
{
    [Area("App")]
    [Route("App/[controller]")]
    [Authorize(Roles = "Merchant,Staff")]
    public class EditProfileController : BaseController
    {      
        public async Task<IActionResult> Index()
        {
            var request = new ResetPasswordPage
            {
                Email = User.Identity.Name
            };
            var result = await Mediator.Send(request);
            result.OldPassword = "";
            if (string.IsNullOrEmpty(result.Email))
            {
                return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
            }
            return View(result);
        }

        [Route("UpdatePassword")]
     //   [Authorize(Roles = "Merchant")]

        public async Task<IActionResult> UpdatePassword(ResetPasswordCommand command)
        {
            command.Email = User.Identity.Name;
            var result = await Mediator.Send(command);

            if (result.Successful)
            {
                return Ok(result);
            }
            return BadRequest(result.Message);
        }

    }
}