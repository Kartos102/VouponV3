using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Voupon.Rewards.WebApp.Infrastructures.Extensions;
using Voupon.Rewards.WebApp.Services.Identity.Commands;
using Voupon.Rewards.WebApp.Services.Identity.Commands.Create;
using static Voupon.Rewards.WebApp.Services.Identity.Commands.Create.CreateAccountCommand;
using static Voupon.Rewards.WebApp.Services.Identity.Commands.EmailLoginCommand;

namespace Voupon.Rewards.WebApp.Controllers
{
    public class IdentityController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Validate([FromForm]EmailLoginCommand command)
        {
            command.HttpDTO = new HttpDTO();
            command.HttpDTO.Origin = Request.Headers["Origin"];
            command.HttpDTO.UserAgent = Request.Headers["User-Agent"];
            command.HttpDTO.Host = Request.Headers["Host"];
            command.HttpDTO.Referer = Request.Headers["Referer"];
            var result = await Mediator.Send(command);

            if (result.Successful)
            {
                var loginResponseViewModel = (LoginResponseViewModel)result.Data;
                var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions()
                {
                    Path = "/",
                    Expires = DateTime.Now.AddDays(365000),
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None,
                    Secure = true
                };
                if (loginResponseViewModel.PreferredLanguage != null)
                {
                    HttpContext.Response.Cookies.Append("Vodus.Language", loginResponseViewModel.PreferredLanguage, cookieOptions);
                }

                HttpContext.Response.Cookies.Append("Rewards.Temporary.Points", loginResponseViewModel.Points.ToString(), cookieOptions);
                HttpContext.Response.Cookies.Append("Rewards.Account.Email", Uri.UnescapeDataString(loginResponseViewModel.Email), cookieOptions);
                return Ok(result);
            }

            return BadRequest(result.Message);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount(CreateAccountCommand command)
        {

            var result = await Mediator.Send(command);

            if (result.Successful)
            {
                var loginCommand = new EmailLoginCommand();
                loginCommand.HttpDTO = new HttpDTO();
                loginCommand.HttpDTO.Origin = Request.Headers["Origin"];
                loginCommand.HttpDTO.UserAgent = Request.Headers["User-Agent"];
                loginCommand.HttpDTO.Host = Request.Headers["Host"];
                loginCommand.HttpDTO.Referer = Request.Headers["Referer"];
                loginCommand.Email = command.Email;
                loginCommand.Password = command.Password;
                var loginResult = await Mediator.Send(loginCommand);

                if (loginResult.Successful)
                {
                    var loginResponseViewModel = (LoginResponseViewModel)loginResult.Data;
                    var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions()
                    {
                        Path = "/",
                        Expires = DateTime.Now.AddDays(365000),
                        SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None,
                        Secure = true
                    };
                    if (loginResponseViewModel.PreferredLanguage != null)
                    {
                        HttpContext.Response.Cookies.Append("Vodus.Language", loginResponseViewModel.PreferredLanguage, cookieOptions);
                    }

                    HttpContext.Response.Cookies.Append("Rewards.Temporary.Points", loginResponseViewModel.Points.ToString(), cookieOptions);
                    HttpContext.Response.Cookies.Append("Rewards.Account.Email", Uri.UnescapeDataString(loginResponseViewModel.Email), cookieOptions);
                    return Ok(result);
                }

                return Ok(result);
            }
            return BadRequest(result.Message);
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordCommand command)
        {
            command.UserId = User.Identity.GetAccountUserId();
            var result = await Mediator.Send(command);
            if (result.Successful)
            {
                return Ok(result);
            }
            // var apiResponse = new ApiResponse();
            return BadRequest(result.Message);
        }

        [HttpPost]
        public async Task<IActionResult> SendResetLink(SendResetLinkCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordCommand command)
        {
            var result = await Mediator.Send(command);
            if (result.Successful)
            {
                return Ok(result);
            }
            return BadRequest(result.Message);
        }

        [HttpGet]
        public async Task<IActionResult> Google(ExchangeGoogleCodeForAccessTokenCommand command)
        {
            return View();
        }

    }
}