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
using Voupon.Common.Enum;
using Azure.Core;
using System.ComponentModel.DataAnnotations;
using MediatR;
using static Voupon.API.Functions.UpdateProfileShippingDetailsFunction;

namespace Voupon.API.Functions
{
    public class UpdateProfileShippingDetailsFunction
    {
        private readonly RewardsDBContext rewardsDBContext;
        private readonly VodusV2Context vodusV2Context;
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly IAzureBlobStorage azureBlobStorage;

        public UpdateProfileShippingDetailsFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
            this.connectionMultiplexer = connectionMultiplexer;
            this.azureBlobStorage = azureBlobStorage;
        }

        [OpenApiOperation(operationId: "Update Shipping Details", tags: new[] { "Cart" }, Description = "Update Shipping details", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(UpdateProfileShippingDetailsModal), Description = "JSON request body ")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "Update shipping details response message")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If no JWT token provided or fail to update")]

        [FunctionName("UpdateProfileShippingDetailsFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "cart/profile-checkout")] HttpRequest req, ILogger log)
        {
            var response = new UpdateProfileShippingDetailsResponseModal
            {
            };

            var auth = new Authentication(req);
            if (!auth.IsValid)
            {
                response.RequireLogin = true;
                response.ErrorMessage = "Invalid token provided. Please re-login first.";
                return new BadRequestObjectResult(response);
            }

            try
            {
                var requestModel = HttpRequestHelper.DeserializeModel<UpdateProfileShippingDetailsModal>(req);

                var user = await vodusV2Context.Users.Include(x => x.MasterMemberProfiles).Where(x => x.Email == auth.Email).FirstOrDefaultAsync();
                if (user == null)
                {
                    response.RequireLogin = false;
                    response.ErrorMessage = "User Not Found";
                    return new BadRequestObjectResult(response);
                }

                var masterprofileId = user.MasterMemberProfiles.First().Id;
                user.FirstName = requestModel.FirstName;
                user.LastName = requestModel.LastName;
                user.Email = auth.Email;

                vodusV2Context.Update(user);
                await vodusV2Context.SaveChangesAsync();

                var master = await vodusV2Context.MasterMemberProfiles.Where(x => x.Id == masterprofileId).FirstOrDefaultAsync();
                if (master == null)
                {
                    response.RequireLogin = false;
                    response.ErrorMessage = "Invalid Request";
                    return new BadRequestObjectResult(response);
                }

                master.AddressLine1 = requestModel.AddressLine1;
                master.AddressLine2 = requestModel.AddressLine2;
                master.City = requestModel.City;
                master.State = requestModel.State;
                //master.MobileCountryCode = requestModel.MobileCountryCode;
                //master.MobileNumber = requestModel.MobileNumber;

                vodusV2Context.MasterMemberProfiles.Update(master);
                await vodusV2Context.SaveChangesAsync();


                response.Code = 0;
                response.Data = new UpdateProfileShippingDetails
                {
                    Message = "Successfully updated profile shipping details"
                };

                return new OkObjectResult(response);

            }

            catch
            {
                return new BadRequestObjectResult(new UpdateProfileShippingDetailsResponseModal
                {
                    Code = 999,
                    ErrorMessage = "Fail to update profile"
                });
            }

        }

        protected class UpdateProfileShippingDetailsResponseModal : ApiResponseViewModel
        {
            public UpdateProfileShippingDetails Data { get; set; }
        }

        protected class UpdateProfileShippingDetails
        {
            public string Message { get; set; }
        }
        public class UpdateProfileShippingDetailsModal
        {
            [JsonIgnore]
            
            public int Id { get; set; }

            //[JsonIgnore]
            //public string UserId { get; set; }
      
            //public int AvailablePoints { get; set; }
            
            public string AddressLine1 { get; set; }
            public string AddressLine2 { get; set; }

            public string Postcode { get; set; }
           
            public string City { get; set; }
          
            public string State { get; set; }

  
            //public string MobileCountryCode { get; set; }
           
            //public string MobileNumber { get; set; }
           
 
            public string FirstName { get; set; }
          
            public string LastName { get; set; }
        }
    }
}
