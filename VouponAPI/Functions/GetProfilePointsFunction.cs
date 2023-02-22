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
    public class GetProfilePointsFunction
    {
        private readonly RewardsDBContext _rewardsDBContext;
        private readonly VodusV2Context _vodusV2Context;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly IAzureBlobStorage _azureBlobStorage;

        public GetProfilePointsFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage)
        {
            _rewardsDBContext = rewardsDBContext;
            _vodusV2Context = vodusV2Context;
            _connectionMultiplexer = connectionMultiplexer;
            _azureBlobStorage = azureBlobStorage;
        }

        [OpenApiOperation(operationId: "Get profile points", tags: new[] { "Profile" }, Description = "Get profile points", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ProfilePointsResponseModel), Summary = "Request reset password")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If unable to process request")]


        [FunctionName("GetProfilePoints")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "profile/points")] HttpRequest req, ILogger log)
        {
            var response = new ProfilePointsResponseModel
            {
                Data = new ProfilePointsData()
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
                    return new BadRequestObjectResult(new ProfilePointsResponseModel
                    {
                        Code = -1,
                        ErrorMessage = "Invalid request. Invalid profile [001]"
                    });
                }


                if (masterData.MemberProfileId.HasValue)
                {
                    var member = await _vodusV2Context.MemberProfiles.Where(x => x.Id == masterData.MemberProfileId).FirstOrDefaultAsync();
                    if (member != null)
                    {
                        response.Data.DemographicPoints = member.DemographicPoints;

                        var memberResponses = await _vodusV2Context.SurveyResponses.Where(x => x.MemberProfileId == member.Id).ToListAsync();

                        if (memberResponses != null && memberResponses.Any())
                        {
                            response.Data.ResponseCollectedPoints = memberResponses.Sum(x => x.PointsCollected);
                        }
                    }
                }
                else
                {   //   This is a fallback for member's master that's is not updated with memberprofileid yet. [slow querying]
                    var member = await _vodusV2Context.MemberProfiles.Where(x => x.MasterMemberProfileId == masterData.Id).FirstOrDefaultAsync();
                    if (member != null)
                    {
                        response.Data.DemographicPoints = member.DemographicPoints;

                        var memberResponses = await _vodusV2Context.SurveyResponses.Where(x => x.MemberProfileId == member.Id).ToListAsync();

                        if (memberResponses != null && memberResponses.Any())
                        {
                            response.Data.ResponseCollectedPoints = memberResponses.Sum(x => x.PointsCollected);
                        }
                    }
                }

                var bonus = await _vodusV2Context.BonusPoints.Where(x => x.MasterMemberProfileId == masterData.Id).ToListAsync();
                if (bonus != null && bonus.Any())
                {
                    response.Data.BonusPoints = bonus.Sum(x => x.Points);
                }

                var usedPoints = 0;
                var v2Orders = await _vodusV2Context.Orders.Where(x => x.MasterMemberId == masterData.Id).ToListAsync();
                var orders = await _rewardsDBContext.Orders.Where(x => x.MasterMemberProfileId == masterData.Id).ToListAsync();



                if (v2Orders != null && v2Orders.Any())
                {
                    usedPoints += v2Orders.Sum(x => x.TotalPoints);
                }

                if (orders != null && orders.Any())
                {
                    var orderCompleted = orders.Where(x => x.OrderStatus == 2);
                    var orderPendingPayment = orders.Where(x => x.OrderStatus == 1);
                    if (orderCompleted != null && orderCompleted.Any())
                    {
                        usedPoints += orderCompleted.Sum(x => x.TotalPoints);
                    }

                    if (orderPendingPayment != null && orderPendingPayment.Any())
                    {
                        response.Data.PointsOnHold += orderPendingPayment.Sum(x => x.TotalPoints);
                    }
                }
                response.Code = 0;
                response.Data.UsedPoints = usedPoints;
                response.Data.AvailablePoints = masterData.AvailablePoints;

                var availablePromoCodes = await _rewardsDBContext.PromoCodes.Where(x => x.Status == 1 && x.ExpireOn > DateTime.Now).ToListAsync();

                if(availablePromoCodes != null && availablePromoCodes.Any())
                {
                    response.Data.AvailablePromoCodes = new List<PromoCodes>();
                    response.Data.AvailablePromoCodes.AddRange(availablePromoCodes.Select(x => new PromoCodes
                    {
                        Code = x.PromoCode,
                        Name = x.Description,
                        Banner = "https://vodus.my/images/banners/double-points-wednesday.png",
                        Description = x.Description,
                        MaxDiscount = x.MaxDiscountValue
                    }));
                }

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.ErrorMessage = "Unable to generate reset request. Please try again later.";
                new BadRequestObjectResult(response);
            }
            return new OkObjectResult(response);
        }

        public class ProfilePointsRequestModel
        {
            public string Email { get; set; }
            public string Locale { get; set; }
        }

        protected class ProfilePointsResponseModel : ApiResponseViewModel
        {
            public ProfilePointsData Data { get; set; }
        }

        public class ProfilePointsData
        {
            public int AvailablePoints { get; set; }
            public int DemographicPoints { get; set; }
            public int BonusPoints { get; set; }
            public int ResponseCollectedPoints { get; set; }
            public int UsedPoints { get; set; }

            public int PointsOnHold { get; set; }

            public List<PromoCodes> AvailablePromoCodes { get; set; }
        }

        public class PromoCodes
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public string Banner { get; set; }
            public string Description { get; set; }
            public decimal MaxDiscount { get; set; }
        }
    }
}


