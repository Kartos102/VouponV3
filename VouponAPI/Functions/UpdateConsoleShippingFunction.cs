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
using Voupon.Common.Enum;
using Azure.Core;
using System.ComponentModel.DataAnnotations;
using MediatR;
using static Voupon.API.Functions.UpdateProfileShippingDetailsFunction;

namespace Voupon.API.Functions
{
    public class UpdateConsoleShippingFunction
    {
        private readonly RewardsDBContext _rewardsDBContext;
        private readonly VodusV2Context vodusV2Context;
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly IAzureBlobStorage azureBlobStorage;

        public UpdateConsoleShippingFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage)
        {
            _rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
            this.connectionMultiplexer = connectionMultiplexer;
            this.azureBlobStorage = azureBlobStorage;
        }

        [FunctionName("UpdateConsoleShippingFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "console/shipping")] HttpRequest req, ILogger log)
        {
            try
            {
                var requestModel = HttpRequestHelper.DeserializeModel<UpdateConsoleShippingModel>(req);

                if (requestModel.OrderTypeId == 1)
                {

                }
                else
                 {
                    var orderShopExternal = await _rewardsDBContext.OrderShopExternal.Where(x => x.Id == requestModel.OrderItemId).FirstOrDefaultAsync();
                    if (orderShopExternal == null)
                    {
                        return new BadRequestObjectResult(new ApiResponseViewModel
                        {
                            Code = 999,
                            ErrorMessage = "Fail to update shipping data. Invalid Id"
                        });

                    }
                    orderShopExternal.ShippingLatestStatus = requestModel.LatestStatus;
                    orderShopExternal.OrderShippingExternalStatus = (byte)requestModel.LatestStatusId;
                    orderShopExternal.ShippingDetailsJson = JsonConvert.SerializeObject(requestModel);
                    orderShopExternal.LastUpdatedAt = DateTime.Now;

                    _rewardsDBContext.OrderShopExternal.Update(orderShopExternal);
                    await _rewardsDBContext.SaveChangesAsync();

                }
                return new OkObjectResult(new ApiResponseViewModel());

            }

            catch(Exception ex)
            {
                return new BadRequestObjectResult(new ApiResponseViewModel
                {
                    Code = 999,
                    ErrorMessage = "Fail to update shipping data" + ex.ToString()
                });
            }

        }

        public class UpdateConsoleShippingModel
        {
            public Guid OrderId { get; set; }
            public Guid OrderItemId { get; set; }
            public short OrderTypeId { get; set; }
            public string LatestStatus { get; set; }
            public short LatestStatusId { get; set; }
            public DateTime LatestUpdatedAt { get; set; }

            public List<ShippingModel> Shipping { get; set; }
        }

        public class ShippingModel
        {
            public DateTime Date { get; set; }
            public string status { get; set; }
            public short StatusId { get; set; }
            public string Content { get; set; }
            public string Location { get; set; }

        }
    }
}
