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

namespace Voupon.API.Functions
{
    public class GetCartShippingAddressFunction
    {
        private readonly RewardsDBContext _rewardsDBContext;
        private readonly VodusV2Context _vodusV2Context;
        private int _masterMemberProfileId;
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly IAzureBlobStorage azureBlobStorage;

        public GetCartShippingAddressFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage)
        {
            this._rewardsDBContext = rewardsDBContext;
            this._vodusV2Context = vodusV2Context;
            this.connectionMultiplexer = connectionMultiplexer;
            this.azureBlobStorage = azureBlobStorage;
        }

        [OpenApiOperation(operationId: "Get cart profile for checkout", tags: new[] { "Cart" }, Description = "Get profile for checkout", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CartProfileCheckoutResponseModel), Summary = "Get cart checkout profile")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If unable to retrieve it from the server")]

        [FunctionName("GetCartShippingAddressFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cart/profile-checkout")] HttpRequest req, ILogger log)
        {

            var response = new CartProfileCheckoutResponseModel();

            var auth = new Authentication(req);
            if (!auth.IsValid)
            {
                response.RequireLogin = true;
                response.ErrorMessage = "Invalid token provided. Please re-login first.";
                return new BadRequestObjectResult(response);
            }
            _masterMemberProfileId = auth.MasterMemberProfileId;
            try
            {
                var master = await _vodusV2Context.MasterMemberProfiles.Where(x => x.Id == _masterMemberProfileId).FirstOrDefaultAsync();

                if (master == null)
                {
                    response.ErrorMessage = "Invalid token. Please login first.";
                    return new BadRequestObjectResult(response);
                }


                var user = await _vodusV2Context.Users.Where(x => x.Id == master.UserId).FirstOrDefaultAsync();
                var member = await _vodusV2Context.MemberProfiles.AsNoTracking().Where(x => x.Id == master.MemberProfileId).FirstOrDefaultAsync();

                response.Data = new CartProfileCheckoutData();
                response.Data.Profile = new CartProfileCheckout
                {
                    Id = master.Id,
                    MobileCountryCode = master.MobileCountryCode,
                    MobileNumber = master.MobileNumber,
                    MobileVerified = master.MobileVerified,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    AddressLine1 = master.AddressLine1,
                    AddressLine2 = master.AddressLine2,
                    State = master.State,
                    Postcode = master.Postcode,
                    City = master.City,
                    CountryId = master.CountryId,
                    CountryName = "Malaysia"
                };
                var today = DateTime.Now.DayOfWeek.ToString();
                if (today.ToUpper() == "WEDNESDAY")
                {
                    var promoCode = "EveryWednesday2XDiscount";
                    var promo = await _rewardsDBContext.PromoCodes.Where(x => x.PromoCode == promoCode).FirstOrDefaultAsync();
                    if (promo != null)
                    {
                        response.Data.Profile.PromoCode = promo.PromoCode;
                    }
                }
                if (member != null)
                {
                    if (member.DemographicStateId.HasValue)
                    {
                        response.Data.Profile.StateId = member.DemographicStateId.Value;
                        if (response.Data.Profile.StateId != 0)
                        {
                            var state = _rewardsDBContext.Provinces.Where(x => x.DemographicId == response.Data.Profile.StateId).FirstOrDefault();
                            if (state != null)
                                response.Data.Profile.StateId = state.Id;
                        }
                    }
                }

                if (string.IsNullOrEmpty(response.Data.Profile.State))
                {
                    if (member != null)
                    {
                        var state = await _vodusV2Context.DemographicValues.Where(x => x.Id == member.DemographicStateId).FirstOrDefaultAsync();

                        if (state != null)
                        {
                            response.Data.Profile.State = state.DisplayValue;
                            response.Data.Profile.StateId = state.Id;

                        }
                    }
                }

                return new OkObjectResult(response);

            }
            catch (Exception ex)
            {
                //  log
                response.ErrorMessage = "Fail to get profile for cart checkout";
                return new BadRequestObjectResult(response);
            }
        }

        public class CartProfileCheckoutResponseModel : ApiResponseViewModel
        {
            public CartProfileCheckoutData Data { get; set; }
        }

        public class CartProfileCheckout
        {
            public int Id { get; set; }
            public int AvailablePoints { get; set; }
            public string AddressLine1 { get; set; }
            public string AddressLine2 { get; set; }
            public string Postcode { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public int StateId { get; set; }
            public short? CountryId { get; set; }
            public string MobileCountryCode { get; set; }
            public string MobileNumber { get; set; }
            public string MobileVerified { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string CountryName { get; set; }
            public string PromoCode { get; set; }
        }

        public class CartProfileCheckoutData
        {
            public CartProfileCheckout Profile { get; set; }
        }
    }
}
