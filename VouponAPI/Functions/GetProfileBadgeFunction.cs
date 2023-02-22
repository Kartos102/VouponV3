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
using Voupon.Common.Enum;
using SendGrid.Helpers.Mail;
using SendGrid;
using Voupon.API.ViewModels;

namespace Voupon.API.Functions
{
    public class GetProfileBadgeFunction
    {
        private readonly RewardsDBContext _rewardsDBContext;
        private readonly VodusV2Context _vodusV2Context;

        public GetProfileBadgeFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context)
        {
            _rewardsDBContext = rewardsDBContext;
            _vodusV2Context = vodusV2Context;
        }

        [OpenApiOperation(operationId: "Get profile points", tags: new[] { "Profile" }, Description = "Get profile badge", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ProfileBadgeResponseModel), Summary = "Request reset password")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If unable to process request")]


        [FunctionName("GetProfileBadge")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "profile/badge")] HttpRequest req, ILogger log)
        {
            var response = new ProfileBadgeResponseModel
            {
                Data = new ProfileBadgeData()
            };
            try
            {
                var auth = new Authentication(req);
                if (!auth.IsValid)
                {
                    response.RequireLogin = true;
                    response.ErrorMessage = "Invalid token provided. Please re-login first.";
                    return new BadRequestObjectResult(response);
                }


                var masterData = await _vodusV2Context.MasterMemberProfiles.Include(x => x.User).Where(x => x.Id == auth.MasterMemberProfileId).FirstOrDefaultAsync();

                if (masterData == null)
                {
                    return new BadRequestObjectResult(new ProfileBadgeResponseModel
                    {
                        Code = -1,
                        ErrorMessage = "Invalid request. Invalid profile [001]"
                    });
                }


                var totalCartItem = 0;
                totalCartItem += await _rewardsDBContext.CartProducts.Where(x => x.MasterMemberProfileId == auth.MasterMemberProfileId).CountAsync();
                totalCartItem += await _rewardsDBContext.CartProductExternal.Where(x => x.MasterMemberProfileId == auth.MasterMemberProfileId).CountAsync();

                response.Data = new ProfileBadgeData
                {
                    AvailablePoints = masterData.AvailablePoints,
                    TotalCartItem = totalCartItem
                };
                
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.ErrorMessage = "Unable to get profile badge. Please try again later.";
                new BadRequestObjectResult(response);
            }
            return new OkObjectResult(response);
        }

        protected class ProfileBadgeResponseModel : ApiResponseViewModel
        {
            public ProfileBadgeData Data { get; set; }
        }

        public class ProfileBadgeData
        {
            public int AvailablePoints { get; set; }
            public int TotalCartItem { get; set; }
        }
    }
}


