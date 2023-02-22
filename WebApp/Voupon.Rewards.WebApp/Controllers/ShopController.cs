using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Voupon.Rewards.WebApp.Infrastructures.Enums;
using Voupon.Rewards.WebApp.Services.Aggregators.Queries;
using Voupon.Rewards.WebApp.Services.Merchants.Page;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Controllers
{
    public class ShopController : BaseController
    {
        [Route("Shop/{id}")]
        public async Task<IActionResult> Index(int? id)
        {
            if (!id.HasValue)
            {
                return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
            }

            if (id.Value == 0)
            {
                var externalShopId = Request.Query["s"];
                var externalTypeId = Request.Query["t"];

                if (string.IsNullOrEmpty(externalShopId) || string.IsNullOrEmpty(externalTypeId))
                {
                    return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
                }

                var shopResult = await Mediator.Send(new AggregatorShopQuery
                {
                    ExternalShopId = externalShopId,
                    ExternalTypeId = byte.Parse(externalTypeId),

                });

                ApiResponseViewModel response = await Mediator.Send(new MerchantPage() { ExternalTypeId = byte.Parse(externalTypeId), ExternalMerchantId = externalShopId });
                return View((MerchantPageViewModel)response.Data);

            }
            else
            {
                ApiResponseViewModel response = await Mediator.Send(new MerchantPage() { Id = id.Value });
                return View((MerchantPageViewModel)response.Data);
            }
        }
    }
}