using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voupon.Rewards.WebApp.Services.Ads.Queries;
using Voupon.Rewards.WebApp.Services.Aggregators.Commands;
using Voupon.Rewards.WebApp.Services.Device.Queries;
using Voupon.Rewards.WebApp.Services.Order.Commands;
using Voupon.Rewards.WebApp.ViewModels;
using static Voupon.Rewards.WebApp.Services.Device.Queries.NonDeviceByTempTokenQuery;

namespace Voupon.Rewards.WebApp.ApiControllers
{
    [Route("api/v1/[controller]")]
    public class AggregatorController : BaseApiController
    {
        [HttpPost]
        [Route("update-shipping-data")]
        public async Task<ApiResponseViewModel> UpdateOrderExternalShipping(UpdateShippingDataCommand command)
        {
            return await Mediator.Send(command);
        }

    }
}
