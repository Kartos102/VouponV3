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

namespace Voupon.API.Functions
{
    public class GetProfileFunction
    {
        private readonly RewardsDBContext rewardsDBContext;
        private readonly VodusV2Context vodusV2Context;
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly IAzureBlobStorage azureBlobStorage;

        public GetProfileFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
            this.connectionMultiplexer = connectionMultiplexer;
            this.azureBlobStorage = azureBlobStorage;
        }

        [OpenApiOperation(operationId: "Get profile", tags: new[] { "Profile" }, Description = "Get profile", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "Profile detail")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If JWT token provided")]


        [FunctionName("GetProfileFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "profile")] HttpRequest req, ILogger log)
        {
            var response = new ProfileResponseModel
            {
                Data = new ProfileData()
            };

            var auth = new Authentication(req);
            if (!auth.IsValid)
            {
                response.RequireLogin = true;
                response.ErrorMessage = "Invalid token provided. Please re-login first.";
                return new BadRequestObjectResult(response);
            }

            var masterData = await vodusV2Context.MasterMemberProfiles.Include(x => x.User).Where(x => x.Id == auth.MasterMemberProfileId).FirstOrDefaultAsync();

            var countryList = await vodusV2Context.Countries.ToListAsync();

            if (masterData == null)
            {
                return new BadRequestObjectResult(new ApiResponseViewModel
                {
                    ErrorMessage = "Invalid request [001]"
                });
            }

            var totalItemsInCart = rewardsDBContext.CartProducts.Where(x => x.MasterMemberProfileId == masterData.Id).Count();
            totalItemsInCart += rewardsDBContext.CartProductExternal.Where(x => x.MasterMemberProfileId == masterData.Id).Count();

            response.Data.Profile = new Profile
            {
                Id = masterData.Id,
                Email = masterData.User.Email,
                EmailConfirmed = masterData.User.EmailConfirmed,
                AddressLine1 = masterData.AddressLine1,
                AddressLine2 = masterData.AddressLine2,
                AvailablePoints = masterData.AvailablePoints,
                CountryId = masterData.CountryId,
                City = masterData.City,
                FirstName = masterData.User.FirstName,
                LastName = masterData.User.LastName,
                MobileCountryCode = masterData.MobileCountryCode,
                MobileVerified = masterData.MobileVerified,
                MobileNumber = masterData.MobileNumber,
                Postcode = masterData.Postcode,
                State = masterData.State,
                TotalIncompletedDemographics = 0,
                TotalItemsInCart = totalItemsInCart,
                PreferLanguage = (!string.IsNullOrEmpty(masterData.PreferLanguage) ? masterData.PreferLanguage : "en"),
                PhotoUrl = "https://www.vodus.my/images/default-profile-photo.jpg"
            };

            response.Data.Profile.Demographics = new Demographics();

            if (masterData.DateOfBirth.HasValue)
            {
                response.Data.Profile.DateOfBirthDay = masterData.DateOfBirth.Value.Day;
                response.Data.Profile.DateOfBirthMonth = masterData.DateOfBirth.Value.Month;
                response.Data.Profile.DateOfBirthYear = masterData.DateOfBirth.Value.Year;
            }

            var allDemographic = await vodusV2Context.DemographicValues.Where(x => x.IsActive == true).ToListAsync();

            var memberProfile = new MemberProfiles();

            if (masterData.MemberProfileId.HasValue && masterData.MemberProfileId.Value != 0)
            {
                memberProfile = await vodusV2Context.MemberProfiles.Where(x => x.Id == masterData.MemberProfileId.Value).Take(1).Select(x => new MemberProfiles
                {
                    Id = x.Id,
                    DemographicAgeId = x.DemographicAgeId,
                    DemographicEducationId = x.DemographicEducationId,
                    DemographicEthnicityId = x.DemographicEthnicityId,
                    DemographicGenderId = x.DemographicGenderId,
                    DemographicMaritalStatusId = x.DemographicMaritalStatusId,
                    DemographicMonthlyIncomeId = x.DemographicMonthlyIncomeId,
                    DemographicMonthlyHouseHoldIncomeId = x.DemographicMonthlyHouseHoldIncomeId,
                    DemographicStateId = x.DemographicStateId

                }).FirstOrDefaultAsync();
            }
            else
            {
                memberProfile = await vodusV2Context.MemberProfiles.Where(x => x.IsMasterProfile == true && x.MasterMemberProfileId == masterData.Id).Take(1).Select(x => new MemberProfiles
                {
                    Id = x.Id,
                    DemographicAgeId = x.DemographicAgeId,
                    DemographicEducationId = x.DemographicEducationId,
                    DemographicEthnicityId = x.DemographicEthnicityId,
                    DemographicGenderId = x.DemographicGenderId,
                    DemographicMaritalStatusId = x.DemographicMaritalStatusId,
                    DemographicMonthlyIncomeId = x.DemographicMonthlyIncomeId,
                    DemographicMonthlyHouseHoldIncomeId = x.DemographicMonthlyHouseHoldIncomeId,
                    DemographicStateId = x.DemographicStateId

                }).FirstOrDefaultAsync();
            }

            var extenstion = await vodusV2Context.MemberProfileExtensions.Where(x => x.MemberProfileId == memberProfile.Id).ToListAsync();

            response.Data.GenderList = allDemographic.Where(x => x.IsActive == true && x.DemographicTypeId == (int)DemographicTypeEnum.Gender).Select(x => new DemographicData
            {
                Id = (byte)x.Id,
                Name = x.DisplayValue.ToString()
            });

            if (memberProfile.DemographicGenderId.HasValue)
            {
                response.Data.Profile.Demographics.GenderId = memberProfile.DemographicGenderId.Value;
            }
            else if (extenstion.Any(x => x.DemographicTypeId == (int)DemographicTypeEnum.Gender))
            {
                response.Data.Profile.Demographics.GenderId = extenstion.First(x => x.DemographicTypeId == (int)DemographicTypeEnum.Gender).DemographicValueId;
            }

            response.Data.EthnicityList = allDemographic.Where(x => x.IsActive == true && x.DemographicTypeId == (int)DemographicTypeEnum.Ethnicity).Select(x => new DemographicData
            {
                Id = (byte)x.Id,
                Name = x.DisplayValue
            });
            if (memberProfile.DemographicEthnicityId.HasValue)
            {
                response.Data.Profile.Demographics.EthnicityId = memberProfile.DemographicEthnicityId.Value;
            }
            else if (extenstion.Any(x => x.DemographicTypeId == (int)DemographicTypeEnum.Ethnicity))
            {
                response.Data.Profile.Demographics.EthnicityId = extenstion.First(x => x.DemographicTypeId == (int)DemographicTypeEnum.Ethnicity).DemographicValueId;
            }

            response.Data.IncomeList = allDemographic.Where(x => x.IsActive == true && x.DemographicTypeId == (int)DemographicTypeEnum.MonthlyIncome).Select(x => new DemographicData
            {
                Id = (byte)x.Id,
                Name = x.DisplayValue
            });
            if (memberProfile.DemographicMonthlyIncomeId.HasValue)
            {
                response.Data.Profile.Demographics.MonthlyHouseHoldIncomeId = memberProfile.DemographicMonthlyIncomeId.Value;
            }
            else if (extenstion.Any(x => x.DemographicTypeId == (int)DemographicTypeEnum.MonthlyIncome))
            {
                response.Data.Profile.Demographics.MonthlyHouseHoldIncomeId = extenstion.First(x => x.DemographicTypeId == (int)DemographicTypeEnum.MonthlyIncome).DemographicValueId;
            }

            response.Data.EducationList = allDemographic.Where(x => x.IsActive == true && x.DemographicTypeId == (int)DemographicTypeEnum.Education).Select(x => new DemographicData
            {
                Id = (byte)x.Id,
                Name = x.DisplayValue
            });
            if (memberProfile.DemographicEducationId.HasValue)
            {
                response.Data.Profile.Demographics.EducationId = memberProfile.DemographicEducationId.Value;
            }
            else if (extenstion.Any(x => x.DemographicTypeId == (int)DemographicTypeEnum.Education))
            {
                response.Data.Profile.Demographics.EducationId = extenstion.First(x => x.DemographicTypeId == (int)DemographicTypeEnum.Education).DemographicValueId;
            }

            response.Data.StateList = allDemographic.Where(x => x.IsActive == true && x.CountryCode == "my" && x.DemographicTypeId == (int)DemographicTypeEnum.State).Select(x => new DemographicData
            {
                Id = (byte)x.Id,
                Name = x.DisplayValue
            });
            if (memberProfile.DemographicStateId.HasValue)
            {

                //response.Data.Profile.Demographics.StateId = memberProfile.DemographicStateId.Value;

                //Replace with new province ID
                var provinces = await rewardsDBContext.Provinces.Where(x => x.DemographicId == memberProfile.DemographicStateId).FirstOrDefaultAsync();

                response.Data.Profile.Demographics.StateId = Convert.ToInt32(provinces.Id); 

            }
            else if (extenstion.Any(x => x.DemographicTypeId == (int)DemographicTypeEnum.State))
            {
                response.Data.Profile.Demographics.StateId = extenstion.First(x => x.DemographicTypeId == (int)DemographicTypeEnum.State).DemographicValueId;
            }
            response.Data.MaritalStatusList = allDemographic.Where(x => x.IsActive == true && x.DemographicTypeId == (int)DemographicTypeEnum.MaritalStatus).Select(x => new DemographicData
            {
                Id = (byte)x.Id,
                Name = x.DisplayValue
            });
            if (memberProfile.DemographicMaritalStatusId.HasValue)
            {
                response.Data.Profile.Demographics.MaritalStatusId = memberProfile.DemographicMaritalStatusId.Value;
            }
            else if (extenstion.Any(x => x.DemographicTypeId == (int)DemographicTypeEnum.MaritalStatus))
            {
                response.Data.Profile.Demographics.MaritalStatusId = extenstion.First(x => x.DemographicTypeId == (int)DemographicTypeEnum.MaritalStatus).DemographicValueId;
            }

            response.Data.MonthlyHouseHoldIncomeList = allDemographic.Where(x => x.IsActive == true && x.DemographicTypeId == (int)DemographicTypeEnum.MonthlyHouseholdIncome).Select(x => new DemographicData
            {
                Id = (byte)x.Id,
                Name = x.DisplayValue
            });
            if (memberProfile.DemographicMonthlyHouseHoldIncomeId.HasValue)
            {
                response.Data.Profile.Demographics.MonthlyHouseHoldIncomeId = memberProfile.DemographicMonthlyHouseHoldIncomeId.Value;
            }
            else if (extenstion.Any(x => x.DemographicTypeId == (int)DemographicTypeEnum.MonthlyHouseholdIncome))
            {
                response.Data.Profile.Demographics.MonthlyHouseHoldIncomeId = extenstion.First(x => x.DemographicTypeId == (int)DemographicTypeEnum.MonthlyHouseholdIncome).DemographicValueId;
            }

            response.Data.Profile.AlternateShippingAddress = await vodusV2Context.MasterMemberShippingAddress.Where(x => x.MasterMemberProfileId == response.Data.Profile.Id).Select(x => new AlternateShippingAddress
            {
                Id = x.Id,
                AddressLine1 = x.AddressLine1,
                AddressLine2 = x.AddressLine2,
                City = x.City,
                CountryId = x.CountryId,
                FirstName = x.FirstName,
                LastName = x.LastName,
                isLastSelected = x.IsLastSelected,
                MasterMemberProfileId = x.MasterMemberProfileId,
                Postcode = x.Postcode,
                State = x.State,
                CreatedAt = x.CreatedAt
            }).ToListAsync();

            var preferLanguage = masterData.PreferLanguage == null ? "0" : masterData.PreferLanguage;
            var lstAllLanguages = await vodusV2Context.Languages.Where(x => x.IsActive == true).OrderByDescending(x => x.IsDefault).OrderBy(x => x.Id).ToListAsync();
            response.Data.LanguageList = lstAllLanguages.Select(x => new LanguageData
            {
                Id = x.Id.ToString(),
                Name = x.LanguageCode
            }).ToList();

            response.Data.Profile.PreferLanguage = preferLanguage;

            #region Check  Total incomplete demographic
            if (response.Data.Profile.DateOfBirthYear == 0)
            {
                response.Data.Profile.TotalIncompletedDemographics++;
            }
            if (response.Data.Profile.Demographics.GenderId == 0)
            {
                response.Data.Profile.TotalIncompletedDemographics++;
            }
            if (response.Data.Profile.Demographics.EthnicityId == 0)
            {
                response.Data.Profile.TotalIncompletedDemographics++;
            }
            if (response.Data.Profile.Demographics.EducationId == 0)
            {
                response.Data.Profile.TotalIncompletedDemographics++;
            }

            if (response.Data.Profile.Demographics.MaritalStatusId == 0)
            {
                response.Data.Profile.TotalIncompletedDemographics++;
            }
            if (response.Data.Profile.Demographics.MonthlyHouseHoldIncomeId == 0)
            {
                response.Data.Profile.TotalIncompletedDemographics++;
            }
            if (response.Data.Profile.Demographics.MonthlyIncomeId == 0)
            {
                response.Data.Profile.TotalIncompletedDemographics++;
            }
            if (response.Data.Profile.Demographics.StateId == 0)
            {
                response.Data.Profile.TotalIncompletedDemographics++;
            }
            #endregion
            return new OkObjectResult(response);
        }

        protected class AggregatorParams
        {
            public string ExternalShopId { get; set; }
            public byte ExternalTypeId { get; set; }
        }

        protected class AggregatorSearchByKeywordQuery
        {
            public string SearchQuery { get; set; }
            public List<int> PriceFilter { get; set; }
            public List<int> LocationFilter { get; set; }

            public int PageNumber { get; set; }
        }

        protected class ProfileResponseModel : ApiResponseViewModel
        {
            public ProfileData Data { get; set; }
        }

        public class ProfileData
        {
            public IEnumerable<DemographicData> CountryList { get; set; }
            public IEnumerable<LanguageData> LanguageList { get; set; }
            public IEnumerable<DemographicData> GenderList { get; set; }
            public IEnumerable<DemographicData> EthnicityList { get; set; }
            public IEnumerable<DemographicData> EducationList { get; set; }
            public IEnumerable<DemographicData> MaritalStatusList { get; set; }
            public IEnumerable<DemographicData> RuralUrbanList { get; set; }
            public IEnumerable<DemographicData> IncomeList { get; set; }
            public IEnumerable<DemographicData> StateList { get; set; }
            public IEnumerable<DemographicData> MonthlyHouseHoldIncomeList { get; set; }
            public Profile Profile { get; set; }
        }

        public class DemographicData
        {
            public byte Id { get; set; }
            public string Name { get; set; }
        }

        public class LanguageData
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public class Profile
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string AddressLine1 { get; set; }
            public string AddressLine2 { get; set; }
            public string Postcode { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public short? CountryId { get; set; }
            public string CountryName { get; set; }
            public string PhotoUrl { get; set; }
            public string MobileVerified { get; set; }

            public int AvailablePoints { get; set; }
            public string MobileCountryCode { get; set; }
            public string MobileNumber { get; set; }
            public string Email { get; set; }
            public bool EmailConfirmed { get; set; }
            public int? DateOfBirthDay { get; set; }
            public int? DateOfBirthMonth { get; set; }
            public int? DateOfBirthYear { get; set; }
            public string PreferLanguage { get; set; }
            public int TotalItemsInCart { get; set; }
            public byte TotalIncompletedDemographics { get; set; }
            public System.DateTime CreatedAt { get; set; }
            public IEnumerable<AlternateShippingAddress> AlternateShippingAddress { get; set; }
            public Demographics Demographics { get; set; }
        }


        public class AlternateShippingAddress
        {
            public int Id { get; set; }
            public int MasterMemberProfileId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string AddressLine1 { get; set; }
            public string AddressLine2 { get; set; }
            public string Postcode { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public short CountryId { get; set; }
            public string CountryName { get; set; }
            public System.DateTime CreatedAt { get; set; }

            public bool isLastSelected { get; set; }
        }

        public class Demographics
        {
            public int EducationId { get; set; }
            public int MaritalStatusId { get; set; }
            public int RuralUrbanId { get; set; }
            public int MonthlyIncomeId { get; set; }
            public int StateId { get; set; }
            public int EthnicityId { get; set; }
            public int GenderId { get; set; }
            public int MonthlyHouseHoldIncomeId { get; set; }
        }

        public class EditPageViewModel
        {
            public int TotalIncompletedDemographics { get; set; }
            public int Id { get; set; }
            public string UserId { get; set; }
            public System.DateTime CreatedAt { get; set; }


        }
    }
}
