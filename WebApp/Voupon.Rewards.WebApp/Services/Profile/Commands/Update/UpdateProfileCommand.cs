using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Enum;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.Infrastructures.Helpers;
using Voupon.Rewards.WebApp.Services.Logger;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Profile.Commands.Update
{
    public class UpdateProfileCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public int AvailablePoints { get; set; }
        [Required]
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        [Required]
        public string Postcode { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        public Nullable<short> CountryId { get; set; }
        [Required]
        public string MobileCountryCode { get; set; }
        [Required]
        public string MobileNumber { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        public int DateOfBirthDay { get; set; }
        public int DateOfBirthMonth { get; set; }
        public int DateOfBirthYear { get; set; }

        public IEnumerable<MasterMemberShippingAddress> AlternateShippingAddress { get; set; }

        public IEnumerable<SelectListItem> CountryList { get; set; }
        public IEnumerable<SelectListItem> LanguageList { get; set; }

        public IEnumerable<SelectListItem> DayList { get; set; }
        public IEnumerable<SelectListItem> MonthList { get; set; }
        public IEnumerable<SelectListItem> YearList { get; set; }

        public int DemographicGenderId { get; set; }
        public IEnumerable<SelectListItem> GenderList { get; set; }
        public int DemographicEthnicityId { get; set; }
        public IEnumerable<SelectListItem> EthnicityList { get; set; }
        public int DemographicEducationId { get; set; }
        public IEnumerable<SelectListItem> EducationList { get; set; }
        public int DemographicMaritalStatusId { get; set; }
        public IEnumerable<SelectListItem> MaritalStatusList { get; set; }
        public int DemographicRuralUrbanId { get; set; }
        public IEnumerable<SelectListItem> RuralUrbanList { get; set; }
        public int DemographicMonthlyIncomeId { get; set; }
        public IEnumerable<SelectListItem> IncomeList { get; set; }
        public int DemographicStateId { get; set; }
        public IEnumerable<SelectListItem> StateList { get; set; }
        public int DemographicMonthlyHouseHoldIncomeId { get; set; }
        public IEnumerable<SelectListItem> MonthlyHouseHoldIncomeList { get; set; }

        public string PreferLanguage { get; set; }

        public class LoginResponseViewModel
        {
            public int MasterMemberProfileId { get; set; }
            public string Email { get; set; }
            public string Token { get; set; }
            public string PreferredLanguage { get; set; }
            public int Points { get; set; }
            public string RedirectUrl { get; set; }
        }

        public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, ApiResponseViewModel>
        {
            private readonly VodusV2Context vodusV2Context;
            private readonly RewardsDBContext rewardsDBContext;
            private readonly IOptions<AppSettings> appSettings;

            private string GetRootDomain(string host)
            {
                var filterHost = host.Replace("http://", "").Replace("https://", "");
                return filterHost.Split('/')[0];
            }

            public UpdateProfileCommandHandler(VodusV2Context vodusV2Context, RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
            {
                this.vodusV2Context = vodusV2Context;
                this.rewardsDBContext = rewardsDBContext;
                this.appSettings = appSettings;
            }

            public async Task<ApiResponseViewModel> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
            {

                var apiResponseViewModel = new ApiResponseViewModel();
                var loginResponseViewModel = new LoginResponseViewModel();

                try
                {
                    var newDob = new DateTime();

                    try
                    {
                        newDob = new DateTime(request.DateOfBirthYear, request.DateOfBirthMonth, request.DateOfBirthDay);
                    }
                    catch
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Make sure your date of birth date is valid";
                        return apiResponseViewModel;
                    }

                    //  Update user profile

                    var user = await vodusV2Context.Users.Include(x => x.MasterMemberProfiles).Where(x => x.Email == request.Email).FirstOrDefaultAsync();
                    if (user == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid request";
                        return apiResponseViewModel;
                    }
                    var masterprofileId = user.MasterMemberProfiles.First().Id;
                    user.FirstName = request.FirstName;
                    user.LastName = request.LastName;
                    user.Email = request.Email;

                    vodusV2Context.Update(user);
                    await vodusV2Context.SaveChangesAsync();

                    var master = await vodusV2Context.MasterMemberProfiles.Where(x => x.Id == masterprofileId).FirstOrDefaultAsync();
                    if (master == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid request";
                        return apiResponseViewModel;
                    }

                    var member = await vodusV2Context.MemberProfiles.Where(x => x.Id == master.MemberProfileId).FirstOrDefaultAsync();

                    var demographicState = await vodusV2Context.DemographicValues.Where(x => x.Id == request.DemographicStateId).FirstOrDefaultAsync();

                    master.PreferLanguage = request.PreferLanguage;
                    master.DateOfBirth = newDob;
                    master.AddressLine1 = request.AddressLine1;
                    master.AddressLine2 = request.AddressLine2;
                    master.City = request.City;
                    master.CountryId = request.CountryId;
                    
                    
                    master.Postcode = request.Postcode;


                    //Check Mobile Number is new
                    var fullMobile = request.MobileCountryCode + request.MobileNumber;
                    var fullCurrentNumber = master.MobileCountryCode + master.MobileNumber;

                    if (fullMobile.Equals(fullCurrentNumber) &&  master.MobileNumber.Equals("Y"))
                    {
                        master.MobileNumber = request.MobileNumber;
                        master.MobileCountryCode = request.MobileCountryCode;
                    }
                    

                    else
                    {
                        master.MobileVerified = "N";
                        master.MobileNumber = request.MobileNumber;
                        master.MobileCountryCode = request.MobileCountryCode;
                        
                    }


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
                         await UpdateDemographic((int)DemographicTypeEnum.Education, request.DemographicEducationId, demographicValues.Where(x => x.Id == request.DemographicEducationId).FirstOrDefault().DisplayValue, member.Id);
                     }
                     else
                     {
                         vodusV2Context.MemberProfileExtensions.Add(new MemberProfileExtensions
                         {
                             DemographicTypeId = (int)DemographicTypeEnum.Education,
                             DemographicValueId = request.DemographicEducationId,
                             MemberProfileId = member.Id,
                             CreatedAt = DateTime.Now,
                             Value = demographicValues.Where(x => x.Id == request.DemographicEducationId).First().DisplayValue
                         });
                     }
                    
                    if (userDemographicExtensions != null && userDemographicExtensions.Where(x => x.DemographicTypeId == (int)DemographicTypeEnum.Ethnicity).Any())
                    {
                        await UpdateDemographic((int)DemographicTypeEnum.Ethnicity, request.DemographicEthnicityId, demographicValues.Where(x => x.Id == request.DemographicEthnicityId).FirstOrDefault().DisplayValue, member.Id);
                    }
                    else
                    {
                        vodusV2Context.MemberProfileExtensions.Add(new MemberProfileExtensions
                        {
                            DemographicTypeId = (int)DemographicTypeEnum.Ethnicity,
                            DemographicValueId = request.DemographicEthnicityId,
                            MemberProfileId = member.Id,
                            CreatedAt = DateTime.Now,
                            Value = demographicValues.Where(x => x.Id == request.DemographicEthnicityId).First().DisplayValue
                        });
                    }

                    if (userDemographicExtensions != null && userDemographicExtensions.Where(x => x.DemographicTypeId == (int)DemographicTypeEnum.MonthlyIncome).Any())
                    {
                        await UpdateDemographic((int)DemographicTypeEnum.MonthlyIncome, request.DemographicMonthlyIncomeId, demographicValues.Where(x => x.Id == request.DemographicMonthlyIncomeId).FirstOrDefault().DisplayValue, member.Id);
                    }
                    else
                    {
                        vodusV2Context.MemberProfileExtensions.Add(new MemberProfileExtensions
                        {
                            DemographicTypeId = (int)DemographicTypeEnum.MonthlyIncome,
                            DemographicValueId = request.DemographicMonthlyIncomeId,
                            MemberProfileId = member.Id,
                            CreatedAt = DateTime.Now,
                            Value = demographicValues.Where(x => x.Id == request.DemographicMonthlyIncomeId).First().DisplayValue
                        });
                    }

                    if (userDemographicExtensions != null && userDemographicExtensions.Where(x => x.DemographicTypeId == (int)DemographicTypeEnum.MonthlyHouseholdIncome).Any())
                    {
                        await UpdateDemographic((int)DemographicTypeEnum.MonthlyHouseholdIncome, request.DemographicMonthlyHouseHoldIncomeId, demographicValues.Where(x => x.Id == request.DemographicMonthlyHouseHoldIncomeId).FirstOrDefault().DisplayValue, member.Id);
                    }
                    else
                    {
                        vodusV2Context.MemberProfileExtensions.Add(new MemberProfileExtensions
                        {
                            DemographicTypeId = (int)DemographicTypeEnum.MonthlyHouseholdIncome,
                            DemographicValueId = request.DemographicMonthlyHouseHoldIncomeId,
                            MemberProfileId = member.Id,
                            CreatedAt = DateTime.Now,
                            Value = demographicValues.Where(x => x.Id == request.DemographicMonthlyHouseHoldIncomeId).First().DisplayValue
                        });
                    }
                    */
                    if (userDemographicExtensions != null && userDemographicExtensions.Where(x => x.DemographicTypeId == (int)DemographicTypeEnum.State).Any())
                    {
                        await UpdateDemographic((int)DemographicTypeEnum.State, request.DemographicStateId, demographicValues.Where(x => x.Id == request.DemographicStateId).FirstOrDefault().DisplayValue, member.Id);
                    }
                    else
                    {
                        vodusV2Context.MemberProfileExtensions.Add(new MemberProfileExtensions
                        {
                            DemographicTypeId = (int)DemographicTypeEnum.State,
                            DemographicValueId = request.DemographicStateId,
                            MemberProfileId = member.Id,
                            CreatedAt = DateTime.Now,
                            Value = demographicValues.Where(x => x.Id == request.DemographicStateId).First().DisplayValue
                        });
                    }

                    if (userDemographicExtensions != null && userDemographicExtensions.Where(x => x.DemographicTypeId == (int)DemographicTypeEnum.Gender).Any())
                    {
                        await UpdateDemographic((int)DemographicTypeEnum.Gender, request.DemographicGenderId, demographicValues.Where(x => x.Id == request.DemographicGenderId).FirstOrDefault().DisplayValue, member.Id);
                    }
                    else
                    {
                        vodusV2Context.MemberProfileExtensions.Add(new MemberProfileExtensions
                        {
                            DemographicTypeId = (int)DemographicTypeEnum.Gender,
                            DemographicValueId = request.DemographicGenderId,
                            MemberProfileId = member.Id,
                            CreatedAt = DateTime.Now,
                            Value = demographicValues.Where(x => x.Id == request.DemographicGenderId).First().DisplayValue
                        });
                    }

                    member.DemographicAgeId = newAgeDemographicId;
                    member.DemographicGenderId = request.DemographicGenderId;
                    member.DemographicStateId = request.DemographicStateId;

                    /*member.DemographicEducationId = request.DemographicEducationId;
                      member.DemographicEthnicityId = request.DemographicEthnicityId;
                      member.DemographicMaritalStatusId = request.DemographicMaritalStatusId;
                      member.DemographicMonthlyIncomeId = request.DemographicMonthlyIncomeId;
                      member.DemographicMonthlyHouseHoldIncomeId = request.DemographicMonthlyHouseHoldIncomeId;
                    */


                    //   if users have more than 8 points, move the extra points to bonus points
                    
                    var extraPoints = 0;
                    if (member.DemographicPoints > 8)
                    {
                        extraPoints = member.DemographicPoints - 8;
                        member.DemographicPoints = 8;
                        var newBonusPoints = new BonusPoints
                        {
                            CreatedAt = DateTime.Now,
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

                    apiResponseViewModel.Data = extraPoints;
                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Message = "Successfully updated profile";

                    return apiResponseViewModel;

                }
                catch (Exception ex)
                {
                    await new Logs
                    {
                        Description = ex.ToString(),
                        Email = request.Email,
                        JsonData = JsonConvert.SerializeObject(request),
                        ActionName = "UpdatePasswordCommand",
                        TypeId = CreateErrorLogCommand.Type.Service,
                        SendgridAPIKey = appSettings.Value.Mailer.Sendgrid.APIKey,
                        RewardsDBContext = rewardsDBContext
                    }.Error();

                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Failed to update profile";
                    return apiResponseViewModel;
                }
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
            private async Task<MemberProfiles> SetMaster(int memberMasterId)
            {
                var newMemberProfile = new MemberProfiles();
                var memberProfileList = await vodusV2Context.MemberProfiles.Where(x => x.MasterMemberProfileId == memberMasterId).ToListAsync();
                var idList = memberProfileList.Select(x => x.Id).ToList();


                var responses = await vodusV2Context.SurveyResponses.Where(x => idList.Contains(x.MemberProfileId)).ToListAsync();
                var responseGroup = responses.GroupBy(x => x.MemberProfileId).Select(group =>
                                   new
                                   {
                                       Id = group.Key,
                                       Count = group.Count()
                                   }).OrderByDescending(x => x.Count);

                var orders = await vodusV2Context.Orders.Where(x => x.MasterMemberId == memberMasterId).ToListAsync();
                var usedPoints = 0;
                var bonusPoints = 0;

                bonusPoints = vodusV2Context.BonusPoints.Where(x => x.MasterMemberProfileId == memberMasterId && x.IsActive == true).Sum(x => x.Points);

                if (orders != null && orders.Any())
                {
                    usedPoints = orders.Sum(x => x.TotalPoints);
                }
                if (responseGroup.Any())
                {
                    var item = responseGroup.First();
                    var memberEntity = await vodusV2Context.MemberProfiles.Where(x => x.Id == item.Id).FirstOrDefaultAsync();
                    memberEntity.IsMasterProfile = true;
                    vodusV2Context.MemberProfiles.Update(memberEntity);
                    await vodusV2Context.SaveChangesAsync();

                    //  Update mastermember points
                    var masterEntity = await vodusV2Context.MasterMemberProfiles.Where(x => x.Id == memberMasterId).FirstOrDefaultAsync();
                    masterEntity.AvailablePoints = (memberEntity.AvailablePoints + memberEntity.DemographicPoints + bonusPoints) - usedPoints;
                    vodusV2Context.MasterMemberProfiles.Update(masterEntity);
                    await vodusV2Context.SaveChangesAsync();

                    newMemberProfile = memberEntity;

                    if (responseGroup.Count() > 1)
                    {
                        //  Set other 
                        responseGroup.Where(x => x.Id != item.Id);
                    }
                }
                else
                {
                    var memberListSorted = memberProfileList.OrderByDescending(x => x.AvailablePoints + x.DemographicPoints);

                    if (memberListSorted.Any())
                    {
                        var item = memberListSorted.First();
                        var memberEntity = await vodusV2Context.MemberProfiles.Where(x => x.Id == item.Id).FirstOrDefaultAsync();
                        memberEntity.IsMasterProfile = true;
                        vodusV2Context.Update(memberEntity);
                        await vodusV2Context.SaveChangesAsync();

                        newMemberProfile = memberEntity;

                        //  Update mastermember points
                        var masterEntity = await vodusV2Context.MasterMemberProfiles.Where(x => x.Id == memberMasterId).FirstOrDefaultAsync();
                        masterEntity.AvailablePoints = (memberEntity.AvailablePoints + memberEntity.DemographicPoints + bonusPoints) - usedPoints;
                        vodusV2Context.Update(masterEntity);
                        await vodusV2Context.SaveChangesAsync();

                        if (memberListSorted.Count() > 1)
                        {
                            memberListSorted.Where(x => x.Id != item.Id);
                        }
                    }
                }
                return newMemberProfile;
            }
        }
    }

}
