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
using static Voupon.API.Functions.UpdateProfileFunction;
using Azure.Core;
using System.IO;

namespace Voupon.API.Functions
{
    public class UpdateShopeeRequestHeaderFunction
    {
        private readonly VodusV2Context _vodusV2Context;


        public UpdateShopeeRequestHeaderFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context)
        {
            _vodusV2Context = vodusV2Context;
        }

        

        [FunctionName("UpdateShopeeRequestHeaderFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "PUT", Route = "shopee-headers")] HttpRequest req, ILogger log)
        {
            var response = new ApiResponseViewModel()
            {
            };
            

            try
            {
                var dataString = "";
                using (var sr = new StreamReader(req.Body))
                {
                    dataString =  await sr.ReadToEndAsync();
                }

                var requestModel = JsonConvert.DeserializeObject<UpdateShopeeRequestHeaderModel>(dataString);

                if(requestModel != null && requestModel.Headers.Any())
                {
                    var existingHeaders = await _vodusV2Context.RequestHeaders.Where(x=>x.ExternalTypeId == requestModel.ExternalTypeId).ToListAsync();
                    _vodusV2Context.RequestHeaders.RemoveRange(existingHeaders);
                    await _vodusV2Context.SaveChangesAsync();

                    var newHeaderList = new List<Voupon.Database.Postgres.VodusEntities.RequestHeaders>();
                    foreach(var header in requestModel.Headers)
                    {
                        newHeaderList.Add(new Database.Postgres.VodusEntities.RequestHeaders
                        {
                            Id = Guid.NewGuid(),
                            ExternalTypeId = requestModel.ExternalTypeId,
                            Name = header.Name,
                            //Value = header.Value,
                            CreatedAt = DateTime.Now
                        });
                    }
                    if(newHeaderList != null && newHeaderList.Any())
                    {
                        await _vodusV2Context.RequestHeaders.AddRangeAsync(newHeaderList);
                        await _vodusV2Context.SaveChangesAsync();
                    }                    
                }

                response.Code = 0;

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {

                response.Code = -1;
                response.ErrorMessage = "Fail to update headers";
                return new BadRequestObjectResult(response);
            }

        }


        public class UpdateShopeeRequestHeaderModel
        {
            public byte ExternalTypeId { get; set; }
            public ShopeeHeaders[] Headers { get; set; }
        }

        public class ShopeeHeaders
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
    }
}