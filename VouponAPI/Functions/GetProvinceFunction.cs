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
namespace Voupon.API.Functions
{
    public class GetProvinceFunction
    {
        private readonly RewardsDBContext _rewardsDBContext;
        public GetProvinceFunction(RewardsDBContext rewardsDBContext)
        {
            _rewardsDBContext = rewardsDBContext;
        }
        [OpenApiOperation(operationId: "Get province", tags: new[] { "Province" }, Description = "Get province", Visibility = OpenApiVisibilityType.Important)]
        //[OpenApiParameter(name: "countryId", In = ParameterLocation.Query, Required = true, Type = typeof(int), Summary = "Country Id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ProvinceResponseModel), Summary = "Get province")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If unable to retrieve it from the server")]


        [FunctionName("GetProvinceFuntion")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "province")] HttpRequest req, ILogger log)
        {
            var response = new ProvinceResponseModel
            {
                Data = new ProvinceData()
            };
            var items = await _rewardsDBContext.Provinces.Where(x => x.IsActivated == true).OrderBy(x => x.Name).ToListAsync();
            List<ProvinceModel> list = new List<ProvinceModel>();
            foreach (var item in items)
            {
                ProvinceModel newItem = new ProvinceModel
                {
                    Id = item.Id,
                    Name = item.Name
                };
                list.Add(newItem);
            }
            response.Code = 0;
            response.Data.Province = list;
            response.Data.Message = "Get Province List Successfully";
            return new OkObjectResult(response);
        }

        class ProvinceResponseModel : ApiResponseViewModel
        {

            public ProvinceData Data { get; set; }
        }

        private class ProvinceData
        {
            public List<ProvinceModel> Province { get; set; }
            public string Message { get; set; }
        }

        private class ProvinceModel
        {
            public int Id { get; set; }
            public string Name { get; set; }

        }
    }
}
