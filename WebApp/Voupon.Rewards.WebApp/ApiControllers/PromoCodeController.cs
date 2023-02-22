using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voupon.Rewards.WebApp.Services.Ads.Queries;
using Voupon.Rewards.WebApp.Services.PromoCode.Commands;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.ApiControllers
{
    [Route("api/v1/[controller]")]
    public class PromoCodeController : BaseApiController
    {
        [HttpPost]
        [Route("subscribe")]
        public async Task<ApiResponseViewModel> Subscribe([FromForm] SubscriberPromoCodeCommand command)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();

            if (string.IsNullOrEmpty(command.Email))
            {
                response.Successful = false;
                response.Message = "Email is required";
                return response;
            }

            var res = await Mediator.Send(command);

            return response;
        }

        [HttpPost]
        [Route("discount")]
        public async Task<ApiResponseViewModel> Discount([FromForm] ValidateCartPromoCodeCommand command)
        {
            var user = GetTokenData(Request.HttpContext);
            if (user == null)
            {
                ApiResponseViewModel response = new ApiResponseViewModel();
                response.Successful = false;
                response.Message = "Please sign in to continue";
                return new ApiResponseViewModel()
                {
                    Successful = false,
                    Message = "Please relogin to continue"
                };
            }

            command.MasterMemberProfileId = user.MemberMasterId;

            command.UserName = (User.Identity.IsAuthenticated == true ? User.Identity.Name : "");
            return await Mediator.Send(command);
        }
    }
}
