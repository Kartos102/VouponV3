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

namespace Voupon.API.Functions
{
    public class GetFeaturedBrandsFunction
    {
        private readonly RewardsDBContext rewardsDBContext;
        private readonly VodusV2Context vodusV2Context;
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly IAzureBlobStorage azureBlobStorage;

        public GetFeaturedBrandsFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
            this.connectionMultiplexer = connectionMultiplexer;
            this.azureBlobStorage = azureBlobStorage;
        }

        [OpenApiOperation(operationId: "Get featured brands", tags: new[] { "Merchants" }, Description = "Get featured brands", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(FeaturedBrandsResponseModel), Summary = "Featured brands")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "if unable to retrieve it from the server")]

        [FunctionName("GetFeaturedBrandsFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "merchants/featured")] HttpRequest req, ILogger log)
        {
            var apiResponseViewModel = new FeaturedBrandsResponseModel
            {
                Data = new FeaturedBrandsData()
            };

            try
            {
                var merchants = await rewardsDBContext.Merchants.Where(x => x.IsBrandShownInHomePage == true).ToListAsync();
                var statusTypes = await rewardsDBContext.StatusTypes.ToListAsync();
                List<Merchants> list = new List<Merchants>();
                foreach (var item in merchants)
                {
                    Merchants newItem = new Merchants();
                    newItem.Id = item.Id;
                    newItem.Code = item.Code;

                    newItem.RegistrationNumber = item.RegistrationNumber;

                    newItem.CompanyName = item.CompanyName;
                    newItem.Description = item.Description;
                    newItem.DisplayName = item.DisplayName;
                    newItem.LogoUrl = (item.LogoUrl != null ? item.LogoUrl.Replace("http://", "https://").Replace(":80", "") : "");
                    newItem.WebsiteSiteUrl = item.WebsiteSiteUrl;

                    list.Add(newItem);
                }

                apiResponseViewModel.Data.Merchants = list;
                return new OkObjectResult(apiResponseViewModel);

            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new ApiResponseViewModel
                {
                    ErrorMessage = "Fail to get featured brands"
                });
            }
        }

        private class FeaturedBrandsResponseModel : ApiResponseViewModel
        {
            public FeaturedBrandsData Data { get; set; }
        }

        private class FeaturedBrandsData
        {
            public List<Merchants> Merchants { get; set; }
        }

        public class Merchants
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string DisplayName { get; set; }
            public string CompanyName { get; set; }
            public string RegistrationNumber { get; set; }
            public string WebsiteSiteUrl { get; set; }
            public string LogoUrl { get; set; }
            public string Description { get; set; }
        }
    }
}
