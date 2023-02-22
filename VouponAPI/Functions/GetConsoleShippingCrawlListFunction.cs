using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.API.ViewModels;
using Voupon.API.Util;
using Newtonsoft.Json.Converters;
using System.Globalization;
using Newtonsoft.Json.Linq;
using System.Security.Policy;

namespace Voupon.API.Functions.Blog
{
    public class GetConsoleShippingCrawlListFunction
    {
        private readonly HttpClient _http;
        private readonly RewardsDBContext _rewardsDBContext;

        public GetConsoleShippingCrawlListFunction(IHttpClientFactory httpClientFactory, RewardsDBContext rewardsDBContext)
        {
            _http = httpClientFactory.CreateClient();
            _rewardsDBContext = rewardsDBContext;
        }

        [FunctionName("ConsoleShippingCrawlList")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "console/shipping")] HttpRequest req, ILogger log)
        {
            var response = new MerchantToCrawlViewModel
            {
            };

            try
            {

                var shippingToCheck = await _rewardsDBContext.OrderShopExternal.Where(x => x.TrackingNo != null && x.ShippingLatestStatus == null && x.ShippingCourier != null).ToListAsync();

                if (shippingToCheck == null || !shippingToCheck.Any())
                {
                    return new OkObjectResult(response);
                }

                var trackingList = new List<Tracking>();

                //  Type 2 = external, Type 1 = internal
                foreach (var item in shippingToCheck)
                {
                    trackingList.Add(new Tracking
                    {
                        OrderId = item.OrderId,
                        OrderItemId = item.Id,
                        OrderTypeId = 2,
                        Url = $"https://www.tracking.my/{item.ShippingCourier}/{item.TrackingNo}"
                    });
                }
                //https://www.tracking.my/pos/EU062422600MY
                response.Data = trackingList;

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                //  log
                response.ErrorMessage = "Fail to get tracking list" + ex.ToString();
                return new BadRequestObjectResult(response);

            }
        }

        private class MerchantToCrawlViewModel : ApiResponseViewModel
        {
            public List<Tracking> Data { get; set; }
        }

        public class Tracking
        {
            public string Url { get; set; }
            public Guid OrderId { get; set; }
            public Guid OrderItemId { get; set; }
            public short OrderTypeId { get; set; }
        }

    }

}
