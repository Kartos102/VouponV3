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
using Voupon.API.Util;
using Voupon.API.ViewModels;
namespace Voupon.API.Functions
{
    public class RemoveCartProductFunction
    {
        private readonly RewardsDBContext rewardsDBContext;
        private readonly VodusV2Context vodusV2Context;
        private int _masterMemberProfileId;


        public RemoveCartProductFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
        }

        [OpenApiOperation(operationId: "Remove cart products", tags: new[] { "Cart" }, Description = "Remove cart products", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        //[OpenApiParameter(name: "id", In = ParameterLocation.Query, Required = false, Type = typeof(int), Summary = "Cart id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "if unable to remove from cart")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(RemoveCartResponseModel), Summary = "Sucessfuly remove item from cart")]


        [FunctionName("RemoveCartProduct")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "cart/remove/{id}/{externalId}")] HttpRequest req, int id, string externalId, ILogger log)
        {
            var response = new RemoveCartResponseModel() {
                Data = new RemoveCartData()
            };
            var auth = new Authentication(req);
            if (!auth.IsValid)
            {
                response.RequireLogin = true;
                response.ErrorMessage = "Invalid token provided. Please re-login first.";
                return new BadRequestObjectResult(response);
            }
            _masterMemberProfileId = auth.MasterMemberProfileId;
            //int id = 0;
            try
            {
                if (id == 0)
                {
                    var cartExternalProduct = await rewardsDBContext.CartProductExternal.Where(x => x.ExternalItemId == externalId).FirstOrDefaultAsync();
                    if (cartExternalProduct != null) {
                        rewardsDBContext.CartProductExternal.Remove(cartExternalProduct);
                        rewardsDBContext.SaveChanges();
                    }
                }
                else 
                {
                    var cartProduct = await rewardsDBContext.CartProducts.Where(x => x.Id == id).FirstOrDefaultAsync();
                    if (cartProduct != null)
                    {
                        rewardsDBContext.CartProducts.Remove(cartProduct);
                        rewardsDBContext.SaveChanges();
                    }
                }
                
                response.Code = 0;
                response.Data.Message = "remove item from cart sucesfully";
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.ErrorMessage = "Fail to remove product from cart";
                return new BadRequestObjectResult(response);
            }


   
        }

        private class RemoveCartResponseModel : ApiResponseViewModel
        {
            public RemoveCartData Data { get; set; }
        }

        private class RemoveCartData
        {
            public bool IsSuccessful { get; set; }

            public string Message { get; set; }
        }
    }
}
