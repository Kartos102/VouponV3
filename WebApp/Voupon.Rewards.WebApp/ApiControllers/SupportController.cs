using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voupon.Rewards.WebApp.Services.Test.Commands;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.ApiControllers
{
    [Route("api/v1/[controller]")]
    public class SupportController : BaseApiController
    {

        [HttpPost]
        [Route("request")]
        public async Task<ApiResponseViewModel> SendSupportRequest([FromForm]SendSupportRequestCommand command)
        {
            return await Mediator.Send(command);
        }
    }
}
