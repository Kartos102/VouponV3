using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voupon.Rewards.WebApp.Services.Ads.Queries;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.ApiControllers
{
    public class AdsController : BaseApiController
    {
        [HttpGet]
        [Route("cc-response-ads")]
        public async Task<ApiResponseViewModel> CCResponseAds()
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            return response;
        }
    }
}
