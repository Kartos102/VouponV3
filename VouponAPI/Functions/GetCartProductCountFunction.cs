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
    public class GetCartProductCountFunction
    {
        private readonly RewardsDBContext _rewardsDBContext;
        private readonly VodusV2Context _vodusV2Context;
        private int _masterMemberProfileId;
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly IAzureBlobStorage azureBlobStorage;

        public GetCartProductCountFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage)
        {
            this._rewardsDBContext = rewardsDBContext;
            this._vodusV2Context = vodusV2Context;
            this.connectionMultiplexer = connectionMultiplexer;
            this.azureBlobStorage = azureBlobStorage;
        }

        [OpenApiOperation(operationId: "Get cart products count", tags: new[] { "Cart" }, Description = "Get cart product count", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CartProductsCountResponseModel), Summary = "Get cart products")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If unable to retrieve it from the server")]

        [FunctionName("GetCartProductCountFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cart/products-count")] HttpRequest req, ILogger log)
        {

            var response = new CartProductsCountResponseModel
            {
                Data = new CartProductCountData()
                {
                    TotalItemCount = 0
                }
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

                var cartProductsList = _rewardsDBContext.CartProducts.Where(x => x.MasterMemberProfileId == _masterMemberProfileId).Count();
                var cartProductExternalCount = _rewardsDBContext.CartProductExternal.Where(x => x.MasterMemberProfileId == _masterMemberProfileId).Count();

                response.Code = 0;
                response.Data.TotalItemCount = cartProductsList + cartProductExternalCount;
                return new OkObjectResult(response);

            }
            catch (Exception ex)
            {
                response.ErrorMessage = "Fail to get cart item count";
                return new BadRequestObjectResult(response);
            }
        }

        public class CartProductsCountResponseModel : ApiResponseViewModel
        {
            public CartProductCountData Data { get; set; }
        }

        public class CartProductCountData
        {
            public int TotalItemCount { get; set; }
        }
    }
}
