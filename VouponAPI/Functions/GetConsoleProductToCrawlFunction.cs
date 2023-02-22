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
    public class GetConsoleProductToCrawlFunction
    {
        private readonly RewardsDBContext _rewardsDBContext;

        public GetConsoleProductToCrawlFunction(IHttpClientFactory httpClientFactory, RewardsDBContext rewardsDBContext)
        {
            _rewardsDBContext = rewardsDBContext;
        }

        [FunctionName("ProductToCrawl")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "console/product-to-crawl")] HttpRequest req, ILogger log)
        {
            var response = new ProductoCrawlViewModel
            {
            };

            try
            {
                var totalChunk = 0;
                var chunk = 0;

                var reqTotalChunk = req.Query["total_chunk"];
                var reqChunk = req.Query["chunk_number"];
                if (!string.IsNullOrEmpty(reqChunk))
                {
                    int.TryParse(reqChunk, out chunk);
                }

                if (!string.IsNullOrEmpty(reqTotalChunk))
                {
                    int.TryParse(reqTotalChunk, out totalChunk);
                }

                if (totalChunk == 0)
                {
                    chunk = 1;
                }

                if (chunk == 0)
                {
                    chunk = 1;
                }

                var totalItems = await _rewardsDBContext.ConsoleProductJSON.Where(x => x.StatusId != 3).CountAsync();

                var itemsPerChunk = totalItems / totalChunk;

                var product = await _rewardsDBContext.ConsoleProductJSON.AsNoTracking().Where(x => x.StatusId != 3).OrderBy(x => x.LastUpdatedAt).Skip(((chunk - 1) * itemsPerChunk)).Take(itemsPerChunk).Select(x => new Product
                {
                    Id = x.Id,
                    PageUrl = x.PageUrl,
                    ExternalId = x.ExternalId,
                    ExternalTypeId = x.ExternalTypeId
                }).ToListAsync();

                response.Code = 0;
                response.Data = product;

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                //  log
                response.ErrorMessage = "Fail to get product list" + ex.ToString();
                return new BadRequestObjectResult(response);

            }
        }

        private class ProductoCrawlViewModel : ApiResponseViewModel
        {
            public List<Product> Data { get; set; }
        }

        public class Product
        {
            public Guid Id { get; set; }
            public string ExternalId { get; set; }
            public short ExternalTypeId { get; set; }
            public string PageUrl { get; set; }
        }

    }

}
