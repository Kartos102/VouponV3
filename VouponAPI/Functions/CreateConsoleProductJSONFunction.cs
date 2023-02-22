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
    public class CreateConsoleProductJSONFunction
    {
        private readonly RewardsDBContext _rewardsDBContext;

        public CreateConsoleProductJSONFunction(IHttpClientFactory httpClientFactory, RewardsDBContext rewardsDBContext)
        {
            _rewardsDBContext = rewardsDBContext;
        }

        [FunctionName("CreateConsoleProductFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "console/product-json")] HttpRequest req, ILogger log)
        {
            var response = new ApiResponseViewModel
            {
            };

            try
            {
                var request = HttpRequestHelper.DeserializeModel<ProductRequestModel>(req);

                var existingProductJSON = await _rewardsDBContext.ConsoleProductJSON.Where(x => x.PageUrl == request.PageUrl).FirstOrDefaultAsync();
                if(existingProductJSON != null)
                {
                    existingProductJSON.JSON = request.JSON;
                    _rewardsDBContext.ConsoleProductJSON.Update(existingProductJSON);
                }
                else
                {
                    _rewardsDBContext.ConsoleProductJSON.Add(new ConsoleProductJSON
                    {
                        Id = Guid.NewGuid(),
                        URL = request.URL,
                        PageUrl = request.PageUrl,
                        ItemName = request.ItemName,
                        ExternalId = request.ExternalId,
                        ExternalMerchantId = request.ExternalMerchantId,
                        ExternalTypeId = request.ExternalTypeId,
                        StatusId = 1,
                        JSON = request.JSON,
                        CreatedAt = DateTime.Now
                    });
                }



                await _rewardsDBContext.SaveChangesAsync();

                response.Code = 0;

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                //  log
                response.ErrorMessage = "Fail to create product json" + ex.ToString();
                return new BadRequestObjectResult(response);

            }
        }

        private class ProductRequestModel
        {
            public Guid Id { get; set; }
            public string PageUrl { get; set; }
            public string ItemName { get; set; }
            public string URL { get; set; }
            public string ExternalMerchantId { get; set; }
            public byte ExternalTypeId { get; set; }
            public string ExternalId { get; set; }
            public byte StatusId { get; set; }
            public string JSON { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastUpdatedAt { get; set; }

        }

    }
}
