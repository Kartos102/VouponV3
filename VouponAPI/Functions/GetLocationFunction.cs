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
    public  class GetLocationFunction
    {
        [OpenApiOperation(operationId: "Get location", tags: new[] { "location" }, Description = "Get list location", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(LocationResponseModel), Summary = "The paginated result of purchase history")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If no id or merchant id is supplied")]
        [FunctionName("GetLocationFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "location")] HttpRequest req, ILogger log) 
        {
            List<Location> locations = new List<Location> {
                new Location{
                    Id= 20,
                    Label = "Local"
                } ,
                new Location{
                    Id = 19,
                    Label = "Overseas",
                },
                new Location{
                    Id=18,
                    Label = "West Malaysia"
                },
                new Location{
                    Id=17,
                    Label = "East Malaysia"
                },
                new Location{
                    Id=12,
                    Label = "Selangor"
                },
                new Location{ 
                    Id=14,
                    Label = "Kuala Lumpur"
                },
                new Location{ 
                    Id=7,
                    Label = "Penang",
                },
                new Location{ 
                    Id=1,
                    Label="Johor"
                },
                new Location{ 
                    Id=2,
                    Label="Kedah"
                },
                new Location{
                    Id=8,
                    Label="Perak"
                },
                new Location{
                    Id=4,
                    Label="Melaka"
                },
                new Location{
                    Id=6,
                    Label="Pahang"
                },
                new Location{
                    Id=5,
                    Label="Negeri Sembilan"
                },
                new Location{
                    Id=13,
                    Label="Terengganu"
                },
                new Location{
                    Id=11,
                    Label="Sarawak"
                },
                new Location{
                    Id=10,
                    Label="Sabah"
                },
                new Location{
                    Id=3,
                    Label="Kelantan"
                },
                new Location{
                    Id=9,
                    Label="Perlis"
                },
                new Location{
                    Id=16,
                    Label="Putrajaya"
                },
                new Location{
                    Id=15,
                    Label="Labuan"
                }


            };

            LocationResponseModel response = new LocationResponseModel() 
            { 
                Data = new LocationData()
            };

            response.Data.Location = locations.OrderBy(x=> x.Id).ToList();
            response.Code = 0;
            response.Data.IsSuccessful = true;
            response.Data.Message = "Successfully get location";
            return new OkObjectResult(response); ;
        }


        private class LocationResponseModel : ApiResponseViewModel
        {
            public LocationData Data {get;set;}
        }

        private class LocationData
        {
            public List<Location> Location { get; set; }
            public bool IsSuccessful { get; set; }

            public string Message { get; set; }
        }

        private class Location 
        { 
            public int Id { get; set; }
            public string Label { get; set; }        
        }




    }
}
