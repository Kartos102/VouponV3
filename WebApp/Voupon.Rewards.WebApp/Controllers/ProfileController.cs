using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voupon.Rewards.WebApp.Infrastructures.Enums;
using Voupon.Rewards.WebApp.Services.Profile.Commands.Update;
using Voupon.Rewards.WebApp.Services.Profile.Page;

namespace Voupon.Rewards.WebApp.Controllers
{
    [Authorize]
    public class ProfileController : BaseController
    {
        [Route("/profile")]
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Points()
        {
            var result = await Mediator.Send(new PointsPage
            {
                Username = User.Identity.Name
            });

            if (!result.Successful)
            {
                RedirectToAction("index", "home");
            }

            return View((ProfilePointsViewModel)result.Data);
        }

        [Route("/profile/Edit")]
        public async Task<IActionResult> Edit()
        {
            var request = new EditPage
            {
                Email = User.Identity.Name
            };

            var result = await Mediator.Send(request);

            if (result.Id == 0)
            {
                return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
            }
            var from = Request.QueryString.Value;
            from = from.Substring(from.LastIndexOf('=') + 1);

            if (result.EmailConfirmed == false)
            {
                if (from != null && from != "")
                {
                    return RedirectToAction("VerifyEmail", "Home", new { from = from });
                }
                else
                {
                    return RedirectToAction("VerifyEmail", "Home", new { from = "editprofile" });

                }
            }
            return View(result);
        }

        [HttpPost]
        [Route("/profile/update-profile")]
        public async Task<IActionResult> Update(UpdateProfileCommand command)
        {
            command.Email = User.Identity.Name;
            var result = await Mediator.Send(command);

            if (result.Successful)
            {
                var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions()
                {
                    Path = "/",
                    Expires = DateTime.Now.AddDays(7),
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None,
                    Secure = true
                };

                var extraPoints = (int)result.Data;
                if (extraPoints > 0)
                {
                    var currentPoints = HttpContext.Request.Cookies["Rewards.Temporary.Points"];
                    int points;
                    if (int.TryParse(currentPoints, out points))
                    {
                        extraPoints = points + extraPoints;
                    }
                    HttpContext.Response.Cookies.Append("Rewards.Temporary.Points", extraPoints.ToString(), cookieOptions);
                }


                return Ok(result);
            }
            return BadRequest(result.Message);
        }

        [HttpPost]
        [Route("/profile/update-address")]
        public async Task<IActionResult> UpdateAddress(UpdateShippingCommand command)
        {
            command.Email = User.Identity.Name;
            var result = await Mediator.Send(command);

            if (result.Successful)
            {
                return Ok(result);
            }
            return BadRequest(result.Message);
        }

        //Request OTP
        [HttpPost]
        [Route("/profile/otp-request")]
        public async Task<IActionResult> RequestOtp(OtpRequestCommand command)
        {
            command.Email = User.Identity.Name;
            var result = await Mediator.Send(command);

            if (result.Successful)
            {
                return Ok(result);
            }
            return BadRequest(result.Message);
        }

        [HttpPost]
        [Route("/profile/otp-verify")]
        public async Task<IActionResult> VerifyOtp(OtpVerifyCommand command)
        {
            command.Email = User.Identity.Name;
            var result = await Mediator.Send(command);

            if (result.Successful)
            {
                return Ok(result);
            }
            return BadRequest(result.Message);
        }


        [Route("/profile/change-password")]
        public async Task<IActionResult> ChangePassword()
        {
            var request = new ResetPasswordPage
            {
                Email = User.Identity.Name
            };
            var result = await Mediator.Send(request);

            if (string.IsNullOrEmpty(result.Email))
            {
                return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
            }
            return View(result);
        }

    }
}