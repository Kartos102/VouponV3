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
    public class GetConsoleMerchantToCrawlFunction
    {
        private readonly HttpClient _http;
        private readonly RewardsDBContext _rewardsDBContext;

        public GetConsoleMerchantToCrawlFunction(IHttpClientFactory httpClientFactory, RewardsDBContext rewardsDBContext)
        {
            _http = httpClientFactory.CreateClient();
            _rewardsDBContext = rewardsDBContext;
        }

        [FunctionName("MerchantToCrawl")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "console/merchant-to-crawl")] HttpRequest req, ILogger log)
        {
            var response = new MerchantToCrawlViewModel
            {
            };

            try
            {
                var merchants = await _rewardsDBContext.ConsoleMerchantToCrawl.AsNoTracking().Where(x => x.StatusId == 1).OrderBy(x => x.LastCrawledAt).ToListAsync();

                response.Code = 0;
                response.Data = merchants.Select(x => new Merchant
                {
                    Id = x.Id,
                    URL = x.Url,

                }).ToList();

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                //  log
                response.ErrorMessage = "Fail to get merchant list" + ex.ToString();
                return new BadRequestObjectResult(response);

            }
        }

        private class MerchantToCrawlViewModel : ApiResponseViewModel
        {
            public List<Merchant> Data { get; set; }
        }

        public class Merchant
        {
            public Guid Id { get; set; }
            public string URL { get; set; }
            public byte ExternalTypeId { get; set; }
            public byte StatusId { get; set; }
            public byte CurrentProcess { get; set; }
            public string Remark { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastUpdatedAt { get; set; }
        }

    }

}
