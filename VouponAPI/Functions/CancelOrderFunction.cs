using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

using System.Linq;

using System.Threading.Tasks;

using Voupon.Database.Postgres.RewardsEntities;

using Voupon.API.ViewModels;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;
using Voupon.API.Util;
using Voupon.Common.Azure.Blob;

namespace Voupon.API.Functions
{
    public class CancelOrderFunction
    {
        public int _masterMemberProfileId { get; set; }
        private readonly RewardsDBContext _rewardsDBContext;
        public CancelOrderFunction(RewardsDBContext rewardsDBContext)
        {
            this._rewardsDBContext = rewardsDBContext;
        }
        [OpenApiOperation(operationId: "Cancel Order", tags: new[] { "order" }, Description = "Cancel Order", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CancelOrderResponseModel), Summary = "the result is order detail")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If no order id is supplied")]

        [FunctionName("CancelOrderFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "order/cancel/{orderId}")] HttpRequest req, ILogger log, Guid orderId)
        {
            var response = new CancelOrderResponseModel();
            var auth = new Authentication(req);
            if (!auth.IsValid)
            {
                response.Code = -1;
                response.RequireLogin = true;
                response.ErrorMessage = "Invalid token provided. Please re-login first.";
                return new BadRequestObjectResult(response);
            }
            _masterMemberProfileId = auth.MasterMemberProfileId;

            try
            {
                response.Data = new CancelOrderData();
                var date = DateTime.Now.AddDays(1);
                var order = await _rewardsDBContext.Orders.Where(x => x.Id == orderId && x.MasterMemberProfileId == _masterMemberProfileId && x.OrderStatus == 1).FirstOrDefaultAsync();
                if (order == null)
                {
                    response.Code = -1;
                    response.Data.IsSuccessful = false;
                    response.ErrorMessage = "Order Not Found";
                    return new NotFoundObjectResult(response);
                }

                order.OrderStatus = 3;
                _rewardsDBContext.Orders.Update(order);
                await _rewardsDBContext.SaveChangesAsync();
                response.Data = new CancelOrderData();

                response.Data.Message = "Successfully cancelled order";
                response.Data.IsSuccessful = true;
                return new NotFoundObjectResult(response);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.ErrorMessage = "Fail to check and update pending orders";
                return new BadRequestObjectResult(response);
            }
        }
        private class CancelOrderResponseModel : ApiResponseViewModel
        {
            public CancelOrderData Data { get; set; }
        }

        private class CancelOrderData
        {
            public bool IsSuccessful { get; set; }

            public string Message { get; set; }
        }


    }
}
