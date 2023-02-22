using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Voupon.Rewards.WebApp.Infrastructures.Enums;
using Voupon.Rewards.WebApp.Services.HUT.Commands;
using Voupon.Rewards.WebApp.Services.HUT.Pages;
using Voupon.Rewards.WebApp.Services.Identity.Commands;
using Voupon.Rewards.WebApp.Services.Profile.Page;
using Voupon.Rewards.WebApp.ViewModels;
using static Voupon.Rewards.WebApp.Services.HUT.Pages.ProductFeedbackPage;
using static Voupon.Rewards.WebApp.Services.HUT.Pages.ProductTestRegistrationPage;

namespace Voupon.Rewards.WebApp.Controllers
{
    [Route("Hut")]
    public class HutController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("product-test-registration/{id}/{email}")]
        public async Task<IActionResult> ProductTestRegistration(int? id, string email)
        {
            if (!id.HasValue || string.IsNullOrEmpty(email))
            {
                return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
            }

            if (!User.Identity.IsAuthenticated)
            {
                foreach (var cookieKey in Request.Cookies.Keys)
                {
                    Response.Cookies.Delete(cookieKey);
                }
                var reloginViewModel = new ProductTestRegistrationViewModel
                {
                    SurveyProjectStatus = true,
                    MasterMemberProfileId = 0
                };
                return View(reloginViewModel);
            }

            var request = new ProductTestRegistrationPage
            {
                Email = email,
                ProjectId = id.Value,
                MasterMemberprofileId = GetMasterMemberId(Request.HttpContext)
            };

            if (request.MasterMemberprofileId == 0)
            {
                return View(new ProductTestRegistrationViewModel() { SurveyProjectStatus = true });
            }

            var result = await Mediator.Send(request);
            if (!string.IsNullOrEmpty(result.RedirectUrl))
            {
                return Redirect(result.RedirectUrl);
            }

            return View(result);
        }
        [HttpGet]
        [Route("LogoutForHUT")]
        public async Task<ApiResponseViewModel> LogoutForHUT(int projectId, string address, string email)
        {
            var apiResponseViewModel = new ApiResponseViewModel();
            
            HttpContext.Response.Cookies.Delete("Rewards.Temporary.Points");
            HttpContext.Response.Cookies.Delete("Vodus.Token");
            HttpContext.Response.Cookies.Delete("Rewards.Account.Email");
            HttpContext.Response.Cookies.Delete("Vodus.Language");

            await Mediator.Send(new LogoutCommand());
            apiResponseViewModel.Successful = true;
            return apiResponseViewModel;
        }

        [HttpPost]
        [Route("Register-Participant")]
        public async Task<ApiResponseViewModel> RegisterParticipantForTest(int projectId, string address, string email, string postCode, string state, string city, string phoneNumber)
        {
            var apiResponseViewModel = new ApiResponseViewModel();
            if (!User.Identity.IsAuthenticated)
            {
                foreach (var cookieKey in Request.Cookies.Keys)
                {
                    Response.Cookies.Delete(cookieKey);
                }

                apiResponseViewModel.Message = "Please login first";
                apiResponseViewModel.Successful = false;
                return apiResponseViewModel;
            }

            var request = new UpdateParticipantProductRegistrationCommand
            {
                Address = address,
                Email = email,
                ProjectId = projectId,
                Postcode = postCode,
                State = state,
                City = city,
                PhoneNumber = phoneNumber,
                MasterMemberProfileId = GetMasterMemberId(Request.HttpContext)
            };

            if (request.MasterMemberProfileId == 0)
            {
                apiResponseViewModel.Message = "Please login first";
                apiResponseViewModel.Successful = false;
                return apiResponseViewModel;
            }

            return await Mediator.Send(request);
        }

        [HttpGet]
        [Route("Product-Feedback/{id}")]
        public async Task<IActionResult> ProductFeedback(int? id)
        {
            if (!id.HasValue)
            {
                return View(new ProductFeedBackViewModel
                {
                    IsValidRequest = false,
                    ErrorMessage = "Invalid request"
                });
            }

            if (!User.Identity.IsAuthenticated)
            {
                return View(new ProductFeedBackViewModel
                {
                    IsValidRequest = false,
                    ErrorMessage = "Please login or register to continue. <br> <a class=\"login sso-btn\" href=\"#\">Login</a> <a class=\"register sso-btn\" href=\"#\">Register</a> "
                });
            }

            var viewModel = await Mediator.Send(new ProductFeedbackPage
            {
                ProjectId = id.Value,
                Username = User.Identity.Name
            });

            return View(viewModel);
        }


        [HttpPost]
        [Route("Product-Feedback/create")]
        public async Task<IActionResult> CreateOrder([FromForm] CreateProductFeedbackCommand command)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return BadRequest("Invalid request. Please login first");
            }

            if (string.IsNullOrEmpty(User.Identity.Name))
            {
                return BadRequest("Invalid request. Please relogin");
            }

            command.Email = User.Identity.Name;
            //command.Email = "belal@vodus.my";
            var result = await Mediator.Send(command);

            if (result.Successful)
            {
                return Ok();
            }

            return BadRequest(result.Message);
        }
    }
}