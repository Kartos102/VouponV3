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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.API.Util;
using Voupon.API.ViewModels;
using System.ComponentModel;
namespace Voupon.API.Functions
{
    public class RemovePromoCode
    {
        private readonly RewardsDBContext _rewardsDBContext;
        private int _masterMemberProfileId;

        public RemovePromoCode(RewardsDBContext rewardsDBContext)
        {
            this._rewardsDBContext = rewardsDBContext;
        }

        [OpenApiOperation(operationId: "Remove promo code", tags: new[] { "PromoCode" }, Description = "Remove promo code", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        //[OpenApiParameter(name: "id", In = ParameterLocation.Query, Required = false, Type = typeof(int), Summary = "Cart id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "if unable to remove promocode")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(RemovePromoCodeResponseModel), Summary = "Sucessfuly remove promocode")]

        [FunctionName("RemovePromoCode")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "promocode/remove/{orderId}")] HttpRequest req, Guid orderId, ILogger log) 
        {
            var response = new RemovePromoCodeResponseModel()
            {
                Data = new RemovePromoCodeData()
            };

            var auth = new Authentication(req);
            if (!auth.IsValid)
            {
                response.RequireLogin = true;
                response.ErrorMessage = "Invalid token provided. Please re-login first.";
                return new BadRequestObjectResult(response);
            }
            _masterMemberProfileId = auth.MasterMemberProfileId;


            try
            {
                if (orderId != Guid.Empty)
                {
                    var order = await _rewardsDBContext.Orders.AsTracking().Where(x => x.Id == orderId && x.MasterMemberProfileId == _masterMemberProfileId).FirstOrDefaultAsync();
                    if (order != null)
                    {
                        order.PromoCodeId = null;
                        order.PromoCodeValue = null;
                        order.PromoCodeExpireOn = null;
                        order.PromoCodeMinSpend = null;
                        order.PromoCodeMaxDiscountValue = null;
                        order.PromoCodeDiscountType = null;
                        order.PromoCodeDiscountValue = null;

                        order.TotalPromoCodeDiscount = 0;
                        order.TotalPrice = order.TotalPriceBeforePromoCodeDiscount;

                        await _rewardsDBContext.SaveChangesAsync();
                    }
                    else {
                        response.Code = -1;
                        response.ErrorMessage = "Order not found";
                        return new NotFoundObjectResult(response);
                    }

                }

                response.Code = 0;
                response.Data.Message = "remove promo code sucesfully";
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.ErrorMessage = "Fail to promo code";
                return new BadRequestObjectResult(response);
            }
        }

        private class RemovePromoCodeResponseModel : ApiResponseViewModel
        {
            public RemovePromoCodeData Data { get; set; }
        }

        private class RemovePromoCodeData
        {
            public bool IsSuccessful { get; set; }

            public string Message { get; set; }
        }
    }
}
