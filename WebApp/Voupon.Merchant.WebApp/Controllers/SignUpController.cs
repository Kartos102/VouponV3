using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SendGrid;
using SendGrid.Helpers.Mail;
using Voupon.Merchant.WebApp.Common.Services.Merchants.Queries;
using Voupon.Merchant.WebApp.Infrastructure.Enums;
using Voupon.Merchant.WebApp.Infrastructure.Extensions;
using Voupon.Merchant.WebApp.Services.Identity.Commands;
using Voupon.Merchant.WebApp.Services.SignUp.Commands.Create;
using Voupon.Merchant.WebApp.Services.SignUp.Commands.Update;
using Voupon.Merchant.WebApp.Services.SignUp.Page;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Controllers
{
   // [Route("[controller]")]
    public class SignUpController : BaseController
    {
        [HttpGet]
        [Route("signup")]
        public IActionResult Index()
        {
            //var memberId = User.Identity.GetMerchantId();
            if (User.Claims != null && User.Claims.Count() > 0)
            {
                var roles = User.Claims.FirstOrDefault(x => x.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role").Value;
                if (roles == "Admin")
                {
                    return Redirect("/Admin/Dashboard");
                }
                else
                    return Redirect("/App/Dashboard");
            }
            return View(new IndexPageViewModel());
        }

        [HttpGet]
        [Route("access-denied")]
        public  IActionResult AccessDenied()
        {            
            return  View(ErrorPageEnum.NOT_ALLOWED_PAGE);
        }

        [Route("{*url}", Order = 999)]
        public IActionResult CatchAll()
        {
            Response.StatusCode = 404;
            return View(ErrorPageEnum.ERROR_404);
        }

        [HttpGet]
        [Route("forgetPassword")]
        public IActionResult ForgetPassword()
        {
            
            return View();
        }

        [HttpGet]
        [Route("ResetPassword")]
        public IActionResult ResetPassword(string email, string code)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code))
            {
                return RedirectToAction("Index", "Login");
            }
            var model = new ResetPasswordViewModel();
            model.Email = email;
            model.Code = code;
            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("UpdatePassword")]
        public async Task<IActionResult> UpdatePassword(ResetPasswordCommand command)
        {
            var result = await Mediator.Send(command);
            if (result.Successful)
            {
                return Ok(result);
            }
            return BadRequest(result.Message);
        }

        //[HttpGet]
        //[Route("resetpassword")]
        //public IActionResult Resetpassword()
        //{
        //    var memberId = User.Identity.GetMerchantId();
        //    return View();
        //}

        [HttpPost]
        [Route("SendResetLink")]
        public async Task<IActionResult> SendResetLink(SendResetPasswordCommand command)
        {
            var result = await Mediator.Send(command);
            if (result.Successful)
            {
                return Ok(result);
            }
            return BadRequest(result.Message);
        }
        [HttpGet]
        [Route("login")]
        public async Task<IActionResult> Login()
        {
            if (User.Claims != null && User.Claims.Count() > 0)
            {
                var roles = User.Claims.FirstOrDefault(x => x.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role").Value;
                if (roles == "Admin")
                {                   
                    return Redirect("/Admin/Dashboard");
                }
                else
                    return Redirect("/App/Dashboard");
            }
            ViewData["IsLogin"] = true;
           
            return View("Index", new IndexPageViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("signup/create")]
        public async Task<IActionResult> Post(CreateTempUserCommand command)
        {
            command.CountryId = 1;
            //  Map recaptcha response 
            if (string.IsNullOrEmpty(Request.Form["g-recaptcha-response"]))
            {
                return BadRequest(new ApiResponseViewModel
                {
                    Successful = false,
                    Message = "Invalid request"
                });
            }
            command.Email = command.Email.ToUpper().Trim();
            //
            var countryName = Request.Form["CountryId"];
            if(countryName == "Malaysia")
            {
                command.CountryId = 1;
            }
            command.GoogleRecaptchaResponse = Request.Form["g-recaptcha-response"];
            var result = await Mediator.Send(command);
            if (result.Successful)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet]
        [Route("signup/verify-tac/{id}")]
        public async Task<IActionResult> VerifyTAC(Guid id)
        {
            if (id == null)
            {
                return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
            }

            var result = await Mediator.Send(new VerifyTACPage
            {
                Id = id
            });

            if (result == null)
            {
                return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
            }

            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("signup/verify-tac/regenerate")]
        public async Task<IActionResult> Regenerate(RegenerateTACCommand command)
        {
            var result = await Mediator.Send(command);
            if (result.Successful)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("signup/verify-tac/validate")]
        public async Task<IActionResult> PostVerifyTAC(VerifyTACCommand command)
        {
            var result = await Mediator.Send(command);
            if(result.Successful)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet]
        [Route("signup/setup-password/{id}")]
        public async Task<IActionResult> SetupPassword(Guid id)
        {
           
            if (id == null)
            {
                return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
            }

            var result = await Mediator.Send(new SetupPasswordPage
            {
                Id = id
            });

            if (result == null)
            {
                return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
            }

            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("signup/setup-password/validate")]
        public async Task<IActionResult> PostSetupPassword(SetupPasswordCommand command)
        {
            
            var result = await Mediator.Send(command);
            if (result.Successful)
            {
                await SendWelcomeMessage(command.Email);
                AuthenticateUserCommand LoginCommand = new AuthenticateUserCommand();
                LoginCommand.Email = command.Email;
                LoginCommand.Password = command.Password;
                var loginResult = await Mediator.Send(LoginCommand);
                if (loginResult.Successful)
                {
                    var roleList = (List<string>)loginResult.Data;
                    if (roleList.Contains("Merchant") || roleList.Contains("Staff"))
                    {
                        result.Data = "/App/Business";
                    }
                    else
                        result.Data = "/admin/Dashboard";
                    return Ok(result);
                }
                else
                {
                   // result.Data = "/login";
                    return BadRequest(loginResult);
                }
               
            }
            return BadRequest(result);
        }

        private async Task SendWelcomeMessage(string email)
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            var apiKey = "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";
            string templateId = "757f1f3e-6ce8-4c7c-aa2a-3e0d98423a44";
            string subject = "Welcome To Vodus Merchant";
            var from = new SendGrid.Helpers.Mail.EmailAddress("noreply@vodus.my", "Vodus Merchant");
            var to = new SendGrid.Helpers.Mail.EmailAddress(email, "");
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage();
            msg.From = from;
            msg.TemplateId = templateId;
            msg.Personalizations = new System.Collections.Generic.List<Personalization>();
            var personalization = new Personalization();
            personalization.Substitutions = new Dictionary<string, string>();    
            personalization.Subject = subject;
            personalization.Tos = new List<EmailAddress>();
            personalization.Tos.Add(to);
            msg.Personalizations.Add(personalization);
            var response = await client.SendEmailAsync(msg).ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                apiResponseViewModel.Successful = true;
                apiResponseViewModel.Message = "Successfully email to customer";
            }
            else
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Failed email to customer";

            }   
        }

    }
}