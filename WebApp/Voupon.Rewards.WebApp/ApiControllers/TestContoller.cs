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
    public class TestController : BaseApiController
    {
        [HttpGet]
        [Route("hut-blast-email/{language}")]
        public async Task<ApiResponseViewModel> HUTBlastEmail(string language)
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            if (string.IsNullOrEmpty(language))
            {
                apiResponseViewModel.Message = "Invalid request [001]";
                return apiResponseViewModel;
            }

            return await Mediator.Send(new HUTBlastEmailCommand
            {
                Language = language
            });
        }
    }
}
