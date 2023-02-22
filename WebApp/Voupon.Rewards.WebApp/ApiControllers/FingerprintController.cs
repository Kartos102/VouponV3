using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voupon.Rewards.WebApp.Services.Ads.Queries;
using Voupon.Rewards.WebApp.Services.Fingerprint.Queries;
using Voupon.Rewards.WebApp.Services.Order.Commands;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.ApiControllers
{
    [Route("api/v1/[controller]")]
    public class FingerprintController : BaseApiController
    {
        [HttpGet]
        [Route("data/{id}")]
        public async Task<IActionResult> GetFingerprintData(string id)
        {
            var result = await Mediator.Send(new FingerprintDataQuery
            {
                FingerprintVisitorId = id
            });

            if (result.Successful)
            {
                return new OkObjectResult(result);
            }
            return new BadRequestObjectResult(result);

        }
    }
}
