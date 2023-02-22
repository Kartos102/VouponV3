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

namespace Voupon.API.Functions.Blog
{
    public class GetConfigFunction
    {
        private readonly HttpClient _http;

        public GetConfigFunction(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient();
        }

        [OpenApiOperation(operationId: "Get config", tags: new[] { "Config" }, Description = "Get config", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "appVersion", In = ParameterLocation.Query, Required = true, Type = typeof(int), Description = "App version", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ConfigResponseModel), Summary = "Result of th request")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If no version or invalid version is submitted")]


        [FunctionName("GetConfigFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "config")] HttpRequest req, ILogger log)
        {
            var response = new ConfigResponseModel
            {
            };

            try
            {
                var version = req.Query["appVersion"];

                if (string.IsNullOrEmpty(version))
                {
                    response.ErrorMessage = "App version is required";
                    return new BadRequestObjectResult(response);
                }

                response.Code = 0;
                response.Data = new ConfigData
                {
                    Message = "Successfully retrived version",
                    ApiVersion = "v1",
                    CheckoutAPI = $"{Environment.GetEnvironmentVariable(EnvironmentKey.VOUPON_URL)}/checkout/payment/",
                    GainPointsUrl = "https://media-c.azurewebsites.net?cctoken="

                };

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                //  log
                return new BadRequestObjectResult(new ConfigResponseModel
                {
                    Code = -1,
                    ErrorMessage = "Please update your app to continue using Vodus",
                    Data = new ConfigData
                    {
                        Message = "Please update your app to continue using Vodus",
                        RedirectUrl = "https://vodus.my/app-update-required"
                    }
                });

            }
        }

        private class ConfigResponseModel : ApiResponseViewModel
        {
            public ConfigData Data { get; set; }
        }

        public class ConfigData
        {
            public string ApiVersion { get; set; }
            public string Message { get; set; }
            public string RedirectUrl { get; set; }
            public string GainPointsUrl { get; set; }
            public string CheckoutAPI { get; set; }
        }

    }

}
