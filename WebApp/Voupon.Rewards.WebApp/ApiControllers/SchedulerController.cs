using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voupon.Rewards.WebApp.Services.Ads.Queries;
using Voupon.Rewards.WebApp.Services.Order.Commands;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.ApiControllers
{
    [Route("api/v1/[controller]")]
    public class SchedulerController : BaseApiController
    {
        [HttpGet]
        [Route("check-and-cancel-pending-orders")]
        public async Task<IActionResult> CheckAndCancelPendingOrders()
        {
            if (!IsSecretValid())
            {
                return BadRequest();
            }

            var result = await Mediator.Send(new CheckPendingOrderCommand());
            if (result.Successful)
            {
                return Ok("Done");
            }
            else
            {
                return BadRequest(result.Message);
            }

        }
    }
}
