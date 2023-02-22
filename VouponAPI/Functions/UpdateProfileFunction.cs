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

namespace Voupon.API.Functions
{
    public class UpdateProfileFunction
    {
        private readonly RewardsDBContext rewardsDBContext;
        private readonly VodusV2Context vodusV2Context;
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly IAzureBlobStorage azureBlobStorage;

        public UpdateProfileFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
            this.connectionMultiplexer = connectionMultiplexer;
            this.azureBlobStorage = azureBlobStorage;
        }

        [OpenApiOperation(operationId: "Update profile", tags: new[] { "Profile" }, Description = "Update profile", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(UpdateProfileRequestModel), Description = "JSON request body ")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "Update profile response message")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If no JWT token provided or fail to update")]


        [FunctionName("UpdateProfileFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "profile")] HttpRequest req, ILogger log)
        {
            var response = new UpdateProfileResponseModel
            {
            };

            var auth = new Authentication(req);
            if (!auth.IsValid)
            {
                response.RequireLogin = true;
                response.ErrorMessage = "Invalid token provided. Please re-login first.";
                return new BadRequestObjectResult(response);
            }


            var requestModel = HttpRequestHelper.DeserializeModel<UpdateProfileRequestModel>(req);

            try
            {
                var newDob = new DateTime();

                try
                {
                    newDob = new DateTime(requestModel.DateOfBirthYear, requestModel.DateOfBirthMonth, requestModel.DateOfBirthDay);
                }
                catch
                {
                    return new BadRequestObjectResult(new UpdateProfileResponseModel
                    {
                        Code = -1,
                        ErrorMessage = "Make sure your date of birth date is valid"
                    });
                }

                //  Update user profile

                var user = await vodusV2Context.Users.Include(x => x.MasterMemberProfiles).Where(x => x.Email == auth.Email).FirstOrDefaultAsync();
                if (user == null)
                {
                    return new BadRequestObjectResult(new UpdateProfileResponseModel
                    {
                        Code = -1,
                        ErrorMessage = "Invalid request. Please relogin"
                    });

                }
                var masterprofileId = user.MasterMemberProfiles.First().Id;
                user.FirstName = requestModel.FirstName;
                user.LastName = requestModel.LastName;

                vodusV2Context.Update(user);
                await vodusV2Context.SaveChangesAsync();

                var master = await vodusV2Context.MasterMemberProfiles.Where(x => x.Id == masterprofileId).FirstOrDefaultAsync();
                if (master == null)
                {
                    return new BadRequestObjectResult(new ApiResponseViewModel
                    {
                        Code = -1,
                        ErrorMessage = "Invalid request. Please relogin [002]"
                    });
                }

                var member = await vodusV2Context.MemberProfiles.Where(x => x.Id == master.MemberProfileId).FirstOrDefaultAsync();

                //Get correct demographic id based on ID Passed from frontend
                var provinces = await rewardsDBContext.Provinces.Where(x => x.DemographicId == requestModel.DemographicStateId).FirstOrDefaultAsync();

                if (provinces == null)
                {
                    return new BadRequestObjectResult(new ApiResponseViewModel
                    {
                        Code = -1,
                        ErrorMessage = "Invalid request"
                    });
                }

                var demographicState = await vodusV2Context.DemographicValues.Where(x => x.Id == requestModel.DemographicStateId).FirstOrDefaultAsync();

                master.PreferLanguage = requestModel.PreferredLanguage;
                master.DateOfBirth = newDob;
                master.AddressLine1 = requestModel.AddressLine1;
                master.AddressLine2 = requestModel.AddressLine2;
                master.City = requestModel.City;
                master.CountryId = requestModel.CountryId;

                var fullMobile = requestModel.MobileCountryCode + requestModel.MobileNumber;
                var fullCurrentNumber = master.MobileCountryCode + master.MobileNumber;

                if (fullMobile.Equals(fullCurrentNumber) && master.MobileNumber.Equals("Y")) 
                {
                   
                    master.MobileNumber = requestModel.MobileNumber;
                    master.MobileCountryCode = requestModel.MobileCountryCode;
                }


                else
                {
                    master.MobileVerified = "N";
                    master.MobileNumber = requestModel.MobileNumber;
                    master.MobileCountryCode = requestModel.MobileCountryCode;

                }

                master.Postcode = requestModel.Postcode;
                if (demographicState != null)
                {
                    master.State = demographicState.DisplayValue;
                }

                int age = CalculateAge(master.DateOfBirth.Value);

                var ageList = await vodusV2Context.DemographicValues.Where(x => x.DemographicTypeId == (int)DemographicTypeEnum.Age && x.IsActive == true).ToListAsync();
                var newAgeDemographicId = 1;
                var newAgeDemographicName = "";

                if (ageList != null)
                {

                    foreach (var item in ageList)
                    {
                        if (age >= 65 && int.Parse(item.Value1) >= 65)
                        {
                            member.DemographicAgeId = item.Id;
                            newAgeDemographicId = item.Id;
                            newAgeDemographicName = item.Value1 + " - " + item.Value2;
                            break;
                        }
                        else
                        {
                            int value2 = 0;
                            if (int.TryParse(item.Value2, out value2))
                            {
                                if (age >= int.Parse(item.Value1) && age <= value2)
                                {
                                    member.DemographicAgeId = item.Id;
                                    newAgeDemographicId = item.Id;
                                    newAgeDemographicName = item.Value1 + " - " + item.Value2;
                                    break;
                                }
                            }
                            else
                            {
                                if (age >= int.Parse(item.Value1))
                                {
                                    member.DemographicAgeId = item.Id;
                                    newAgeDemographicId = item.Id;
                                    newAgeDemographicName = item.Value1 + item.Value2;
                                    break;
                                }
                            }
                        }
                    }
                }

                var userDemographicExtensions = await vodusV2Context.MemberProfileExtensions.Where(x => x.MemberProfileId == member.Id).ToListAsync();
                var demographicValues = await vodusV2Context.DemographicValues.ToListAsync();

                var existingDemographicCount = 0;
                var demographicPoints = 0;
                var demographicBonusPoints = 0;

                //  Update demographics
                if (userDemographicExtensions != null && userDemographicExtensions.Where(x => x.DemographicTypeId == (int)DemographicTypeEnum.Age).Any())
                {
                    existingDemographicCount = userDemographicExtensions.Count();
                    await UpdateDemographic((int)DemographicTypeEnum.Age, newAgeDemographicId, newAgeDemographicName, member.Id);
                }
                else
                {
                    vodusV2Context.MemberProfileExtensions.Add(new MemberProfileExtensions
                    {
                        DemographicTypeId = (int)DemographicTypeEnum.Age,
                        DemographicValueId = newAgeDemographicId,
                        MemberProfileId = member.Id,
                        CreatedAt = DateTime.Now,
                        Value = newAgeDemographicName
                    });
                }

                //  Previously 1 point per response
                /*
                if(member.DemographicPoints == existingDemographicCount)
                {

                }
                else
                {
                    if(member.DemographicPoints % 2 == 0)
                    {

                    }
                }
                */

                /* if (userDemographicExtensions != null && userDemographicExtensions.Where(x => x.DemographicTypeId == (int)DemographicTypeEnum.Education).Any())
                 {
                     await UpdateDemographic((int)DemographicTypeEnum.Education, requestModel.DemographicEducationId, demographicValues.Where(x => x.Id == requestModel.DemographicEducationId).FirstOrDefault().DisplayValue, member.Id);
                 }
                 else
                 {
                     vodusV2Context.MemberProfileExtensions.Add(new MemberProfileExtensions
                     {
                         DemographicTypeId = (int)DemographicTypeEnum.Education,
                         DemographicValueId = requestModel.DemographicEducationId,
                         MemberProfileId = member.Id,
                         CreatedAt = DateTime.Now,
                         Value = demographicValues.Where(x => x.Id == requestModel.DemographicEducationId).First().DisplayValue
                     });
                 }

                if (userDemographicExtensions != null && userDemographicExtensions.Where(x => x.DemographicTypeId == (int)DemographicTypeEnum.Ethnicity).Any())
                {
                    await UpdateDemographic((int)DemographicTypeEnum.Ethnicity, requestModel.DemographicEthnicityId, demographicValues.Where(x => x.Id == requestModel.DemographicEthnicityId).FirstOrDefault().DisplayValue, member.Id);
                }
                else
                {
                    vodusV2Context.MemberProfileExtensions.Add(new MemberProfileExtensions
                    {
                        DemographicTypeId = (int)DemographicTypeEnum.Ethnicity,
                        DemographicValueId = requestModel.DemographicEthnicityId,
                        MemberProfileId = member.Id,
                        CreatedAt = DateTime.Now,
                        Value = demographicValues.Where(x => x.Id == requestModel.DemographicEthnicityId).First().DisplayValue
                    });
                }

                if (userDemographicExtensions != null && userDemographicExtensions.Where(x => x.DemographicTypeId == (int)DemographicTypeEnum.MonthlyIncome).Any())
                {
                    await UpdateDemographic((int)DemographicTypeEnum.MonthlyIncome, requestModel.DemographicMonthlyIncomeId, demographicValues.Where(x => x.Id == requestModel.DemographicMonthlyIncomeId).FirstOrDefault().DisplayValue, member.Id);
                }
                else
                {
                    vodusV2Context.MemberProfileExtensions.Add(new MemberProfileExtensions
                    {
                        DemographicTypeId = (int)DemographicTypeEnum.MonthlyIncome,
                        DemographicValueId = requestModel.DemographicMonthlyIncomeId,
                        MemberProfileId = member.Id,
                        CreatedAt = DateTime.Now,
                        Value = demographicValues.Where(x => x.Id == requestModel.DemographicMonthlyIncomeId).First().DisplayValue
                    });
                }

                if (userDemographicExtensions != null && userDemographicExtensions.Where(x => x.DemographicTypeId == (int)DemographicTypeEnum.MonthlyHouseholdIncome).Any())
                {
                    await UpdateDemographic((int)DemographicTypeEnum.MonthlyHouseholdIncome, requestModel.DemographicMonthlyHouseHoldIncomeId, demographicValues.Where(x => x.Id == requestModel.DemographicMonthlyHouseHoldIncomeId).FirstOrDefault().DisplayValue, member.Id);
                }
                else
                {
                    vodusV2Context.MemberProfileExtensions.Add(new MemberProfileExtensions
                    {
                        DemographicTypeId = (int)DemographicTypeEnum.MonthlyHouseholdIncome,
                        DemographicValueId = requestModel.DemographicMonthlyHouseHoldIncomeId,
                        MemberProfileId = member.Id,
                        CreatedAt = DateTime.Now,
                        Value = demographicValues.Where(x => x.Id == requestModel.DemographicMonthlyHouseHoldIncomeId).First().DisplayValue
                    });
                }
                */
                if (userDemographicExtensions != null && userDemographicExtensions.Where(x => x.DemographicTypeId == (int)DemographicTypeEnum.State).Any())
                {
                    await UpdateDemographic((int)DemographicTypeEnum.State, requestModel.DemographicStateId, demographicValues.Where(x => x.Id == requestModel.DemographicStateId).FirstOrDefault().DisplayValue, member.Id);
                }
                else
                {
                    vodusV2Context.MemberProfileExtensions.Add(new MemberProfileExtensions
                    {
                        DemographicTypeId = (int)DemographicTypeEnum.State,
                        DemographicValueId = requestModel.DemographicStateId,
                        MemberProfileId = member.Id,
                        CreatedAt = DateTime.Now,
                        Value = demographicValues.Where(x => x.Id == requestModel.DemographicStateId).First().DisplayValue
                    });
                }

                if (userDemographicExtensions != null && userDemographicExtensions.Where(x => x.DemographicTypeId == (int)DemographicTypeEnum.Gender).Any())
                {
                    await UpdateDemographic((int)DemographicTypeEnum.Gender, requestModel.DemographicGenderId, demographicValues.Where(x => x.Id == requestModel.DemographicGenderId).FirstOrDefault().DisplayValue, member.Id);
                }
                else
                {
                    vodusV2Context.MemberProfileExtensions.Add(new MemberProfileExtensions
                    {
                        DemographicTypeId = (int)DemographicTypeEnum.Gender,
                        DemographicValueId = requestModel.DemographicGenderId,
                        MemberProfileId = member.Id,
                        CreatedAt = DateTime.Now,
                        Value = demographicValues.Where(x => x.Id == requestModel.DemographicGenderId).First().DisplayValue
                    });
                }

                member.DemographicAgeId = newAgeDemographicId;
                member.DemographicGenderId = requestModel.DemographicGenderId;
                member.DemographicStateId = requestModel.DemographicStateId;

                /*member.DemographicEducationId = requestModel.DemographicEducationId;
                  member.DemographicEthnicityId = requestModel.DemographicEthnicityId;
                  member.DemographicMaritalStatusId = requestModel.DemographicMaritalStatusId;
                  member.DemographicMonthlyIncomeId = requestModel.DemographicMonthlyIncomeId;
                  member.DemographicMonthlyHouseHoldIncomeId = requestModel.DemographicMonthlyHouseHoldIncomeId;
                */


                //   if users have more than 8 points, move the extra points to bonus points
                var extraPoints = 0;
                if (member.DemographicPoints > 8)
                {
                    extraPoints = member.DemographicPoints - 8;
                    var newBonusPoints = new BonusPoints
                    {
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "SYSTEM",
                        IsActive = true,
                        MasterMemberProfileId = master.Id,
                        Points = extraPoints,
                        Remark = "Bonus VPoints from old demographic"
                    };


                    master.AvailablePoints = master.AvailablePoints + extraPoints;

                    vodusV2Context.MasterMemberProfiles.Update(master);
                    await vodusV2Context.BonusPoints.AddAsync(newBonusPoints);
                    extraPoints = master.AvailablePoints;
                }

                vodusV2Context.MemberProfiles.Update(member);
                await vodusV2Context.SaveChangesAsync();

                response.Code = 0;
                response.Data = new UpdateProfile
                {
                    Message = "Successfully updated profile"
                };

                return new OkObjectResult(response);

            }
            catch (Exception ex)
            {

                return new BadRequestObjectResult(new UpdateProfileResponseModel
                {
                    Code = 999,
                    ErrorMessage = "Fail to update profile"
                });            }
        }

        private async Task<bool> UpdateDemographic(int demographicTypeId, int demographicValueId, string displayValue, long memberProfileId)
        {
            var result = await vodusV2Context.Database.ExecuteSqlRawAsync($"Update MemberProfileExtensions Set DemographicValueId={demographicValueId}, Value='{displayValue}' where MemberProfileId={memberProfileId} and DemographicTypeId={demographicTypeId}");
            return (result > 1 ? true : false);
        }

        /// <summary>  
        /// For calculating age  
        /// </summary>  
        /// <param name="Dob">Enter Date of Birth to Calculate the age</param>  
        /// <returns> years</returns>  
        private int CalculateAge(DateTime Dob)
        {
            DateTime Now = DateTime.Now;
            int Years = new DateTime(DateTime.Now.Subtract(Dob).Ticks).Year - 1;
            DateTime PastYearDate = Dob.AddYears(Years);
            int Months = 0;
            for (int i = 1; i <= 12; i++)
            {
                if (PastYearDate.AddMonths(i) == Now)
                {
                    Months = i;
                    break;
                }
                else if (PastYearDate.AddMonths(i) >= Now)
                {
                    Months = i - 1;
                    break;
                }
            }
            int Days = Now.Subtract(PastYearDate.AddMonths(Months)).Days;
            int Hours = Now.Subtract(PastYearDate).Hours;
            int Minutes = Now.Subtract(PastYearDate).Minutes;
            int Seconds = Now.Subtract(PastYearDate).Seconds;
            return Years;
            // return String.Format("Age: {0} Year(s) {1} Month(s) {2} Day(s) {3} Hour(s) {4} Second(s)",
            // Years, Months, Days, Hours, Seconds);
        }
        protected class UpdateProfileResponseModel : ApiResponseViewModel
        {
            public UpdateProfile Data { get; set; }
        }

        protected class UpdateProfile
        {
            public string Message { get; set; }
        }

        public class UpdateProfileRequestModel
        {
            [JsonIgnore]
            public int Id { get; set; }
            [JsonIgnore]
            public string UserId { get; set; }
            [JsonIgnore]
            public int AvailablePoints { get; set; }
            public string AddressLine1 { get; set; }
            public string AddressLine2 { get; set; }
            public string Postcode { get; set; }
            public string City { get; set; }
            //public string State { get; set; }
            public Nullable<short> CountryId { get; set; }
            public string MobileCountryCode { get; set; }
            public string MobileNumber { get; set; }
            public string MobileVerified { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }

            public int DateOfBirthDay { get; set; }
            public int DateOfBirthMonth { get; set; }
            public int DateOfBirthYear { get; set; }

            public int DemographicGenderId { get; set; }
            public int DemographicStateId { get; set; }

            public string PreferredLanguage { get; set; }
            public string Locale { get; set; }
        }
    }
}
