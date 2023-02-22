using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Enum;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;

namespace Voupon.Rewards.WebApp.Services.Profile.Page
{
    public class EditPage : IRequest<EditPageViewModel>
    {
        public string Email { get; set; }
        private class EditPageHandler : IRequestHandler<EditPage, EditPageViewModel>
        {
            VodusV2Context vodusV2Context;
            RewardsDBContext rewardsDBContext;
            public EditPageHandler(VodusV2Context vodusV2Context, RewardsDBContext rewardsDBContext)
            {
                this.vodusV2Context = vodusV2Context;
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<EditPageViewModel> Handle(EditPage request, CancellationToken cancellationToken)
            {

                var masterData = await vodusV2Context.MasterMemberProfiles.Include(x => x.User).Where(x => x.User.Email == request.Email).FirstOrDefaultAsync();

                var countryList = await vodusV2Context.Countries.ToListAsync();

                if (masterData == null)
                {
                    return null;
                }

                var totalItemsInCart = rewardsDBContext.CartProducts.Where(x => x.MasterMemberProfileId == masterData.Id).Count();
                totalItemsInCart += rewardsDBContext.CartProductExternal.Where(x => x.MasterMemberProfileId == masterData.Id).Count();

                var model = new EditPageViewModel
                {
                    Id = masterData.Id,
                    UserId = masterData.UserId,
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
                    MobileNumber = masterData.MobileNumber,
                    Postcode = masterData.Postcode,
                    State = masterData.State,
                    TotalIncompletedDemographics = 0,
                    CountryList = countryList.Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name,
                        Selected = (x.CodeNumber == "458") ? true : false
                    }),
                    TotalItemsInCart = totalItemsInCart
                };

                List<SelectListItem> dayList = new List<SelectListItem>();
                dayList.Add(
                new SelectListItem
                {
                    Value = "0",
                    Text = "Day"
                });

                for (var day = 1; day <= 31; day++)
                {
                    dayList.Add(
                    new SelectListItem
                    {
                        Value = day.ToString(),
                        Text = day.ToString()
                    });
                }
                model.DayList = dayList;

                List<SelectListItem> monthList = new List<SelectListItem>();
                monthList.Add(
                new SelectListItem
                {
                    Value = "0",
                    Text = "Month"
                });

                for (var month = 1; month <= 12; month++)
                {
                    monthList.Add(
                    new SelectListItem
                    {
                        Value = month.ToString(),
                        Text = month.ToString()
                    });
                }
                model.MonthList = monthList;

                //  Year
                List<SelectListItem> yearList = new List<SelectListItem>();
                yearList.Add(
                new SelectListItem
                {
                    Value = "0",
                    Text = "Year"
                });

                var years = Enumerable.Range(1930, 2010 - 1930 + 1).ToList();

                foreach (var item in years)
                {
                    yearList.Add(
                    new SelectListItem
                    {
                        Value = item.ToString(),
                        Text = item.ToString()
                    });
                }
                model.YearList = yearList;

                if (masterData.DateOfBirth.HasValue)
                {
                    model.DateOfBirthDay = masterData.DateOfBirth.Value.Day;
                    model.DateOfBirthMonth = masterData.DateOfBirth.Value.Month;
                    model.DateOfBirthYear = masterData.DateOfBirth.Value.Year;
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

                model.GenderList = allDemographic.Where(x => x.IsActive == true && x.DemographicTypeId == (int)DemographicTypeEnum.Gender).Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.DisplayValue
                });
                if (memberProfile.DemographicGenderId.HasValue)
                {
                    model.DemographicGenderId = memberProfile.DemographicGenderId.Value;
                }
                else if (extenstion.Any(x => x.DemographicTypeId == (int)DemographicTypeEnum.Gender))
                {
                    model.DemographicGenderId = extenstion.First(x => x.DemographicTypeId == (int)DemographicTypeEnum.Gender).DemographicValueId;
                }

                model.EthnicityList = allDemographic.Where(x => x.IsActive == true && x.DemographicTypeId == (int)DemographicTypeEnum.Ethnicity).Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.DisplayValue
                });
                if (memberProfile.DemographicEthnicityId.HasValue)
                {
                    model.DemographicEthnicityId = memberProfile.DemographicEthnicityId.Value;
                }
                else if (extenstion.Any(x => x.DemographicTypeId == (int)DemographicTypeEnum.Ethnicity))
                {
                    model.DemographicEthnicityId = extenstion.First(x => x.DemographicTypeId == (int)DemographicTypeEnum.Ethnicity).DemographicValueId;
                }

                /*
                model.RuralUrbanList = allDemographic.Where(x => x.IsActive == true && x.DemographicTypeId == (int)DemographicTypeEnum.RuralUrban).Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.DisplayValue
                });
                if (memberProfile.DemographicRuralUrbanId.HasValue)
                {
                    model.DemographicRuralUrbanId = memberProfile.DemographicRuralUrbanId.Value;
                }
                else if (extenstion.Any(x => x.DemographicTypeId == (int)DemographicTypeEnum.RuralUrban))
                {
                    model.DemographicRuralUrbanId = extenstion.First(x => x.DemographicTypeId == (int)DemographicTypeEnum.RuralUrban).DemographicValueId;
                }
                */

                model.IncomeList = allDemographic.Where(x => x.IsActive == true && x.DemographicTypeId == (int)DemographicTypeEnum.MonthlyIncome).Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.DisplayValue
                });
                if (memberProfile.DemographicMonthlyIncomeId.HasValue)
                {
                    model.DemographicMonthlyIncomeId = memberProfile.DemographicMonthlyIncomeId.Value;
                }
                else if (extenstion.Any(x => x.DemographicTypeId == (int)DemographicTypeEnum.MonthlyIncome))
                {
                    model.DemographicMonthlyIncomeId = extenstion.First(x => x.DemographicTypeId == (int)DemographicTypeEnum.MonthlyIncome).DemographicValueId;
                }

                model.EducationList = allDemographic.Where(x => x.IsActive == true && x.DemographicTypeId == (int)DemographicTypeEnum.Education).Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.DisplayValue
                });
                if (memberProfile.DemographicEducationId.HasValue)
                {
                    model.DemographicEducationId = memberProfile.DemographicEducationId.Value;
                }
                else if (extenstion.Any(x => x.DemographicTypeId == (int)DemographicTypeEnum.Education))
                {
                    model.DemographicEducationId = extenstion.First(x => x.DemographicTypeId == (int)DemographicTypeEnum.Education).DemographicValueId;
                }

                model.StateList = allDemographic.Where(x => x.IsActive == true && x.CountryCode == "my" && x.DemographicTypeId == (int)DemographicTypeEnum.State).Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.DisplayValue
                });
                if (memberProfile.DemographicStateId.HasValue)
                {
                    model.DemographicStateId = memberProfile.DemographicStateId.Value;
                }
                else if (extenstion.Any(x => x.DemographicTypeId == (int)DemographicTypeEnum.State))
                {
                    model.DemographicStateId = extenstion.First(x => x.DemographicTypeId == (int)DemographicTypeEnum.State).DemographicValueId;
                }
                model.MaritalStatusList = allDemographic.Where(x => x.IsActive == true && x.DemographicTypeId == (int)DemographicTypeEnum.MaritalStatus).Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.DisplayValue
                });
                if (memberProfile.DemographicMaritalStatusId.HasValue)
                {
                    model.DemographicMaritalStatusId = memberProfile.DemographicMaritalStatusId.Value;
                }
                else if (extenstion.Any(x => x.DemographicTypeId == (int)DemographicTypeEnum.MaritalStatus))
                {
                    model.DemographicMaritalStatusId = extenstion.First(x => x.DemographicTypeId == (int)DemographicTypeEnum.MaritalStatus).DemographicValueId;
                }

                model.MonthlyHouseHoldIncomeList = allDemographic.Where(x => x.IsActive == true && x.DemographicTypeId == (int)DemographicTypeEnum.MonthlyHouseholdIncome).Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.DisplayValue
                });
                if (memberProfile.DemographicMonthlyHouseHoldIncomeId.HasValue)
                {
                    model.DemographicMonthlyHouseHoldIncomeId = memberProfile.DemographicMonthlyHouseHoldIncomeId.Value;
                }
                else if (extenstion.Any(x => x.DemographicTypeId == (int)DemographicTypeEnum.MonthlyHouseholdIncome))
                {
                    model.DemographicMonthlyHouseHoldIncomeId = extenstion.First(x => x.DemographicTypeId == (int)DemographicTypeEnum.MonthlyHouseholdIncome).DemographicValueId;
                }

                model.AlternateShippingAddress = await vodusV2Context.MasterMemberShippingAddress.Select(x => new MasterMemberShippingAddressViewModel
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
                model.PreferLanguage = preferLanguage;
                var itemTypes = new List<SelectListItem>();
                itemTypes.Add(new SelectListItem { Text = "-- Select --", Value = "0" });
                foreach (var item in lstAllLanguages)
                {
                    if (preferLanguage == item.LanguageCode)
                    {
                        itemTypes.Add(new SelectListItem { Text = item.LanguageDisplayName, Value = item.LanguageCode, Selected = true });
                    }
                    else
                    {
                        itemTypes.Add(new SelectListItem { Text = item.LanguageDisplayName, Value = item.LanguageCode });
                    }
                }

                model.LanguageList = itemTypes;

                #region Check  Total incomplete demographic
                if (model.DateOfBirthYear == 0)
                {
                    model.TotalIncompletedDemographics++;
                }
                if (model.DemographicGenderId == 0)
                {
                    model.TotalIncompletedDemographics++;
                }
                if (model.DemographicEthnicityId == 0)
                {
                    model.TotalIncompletedDemographics++;
                }
                if (model.DemographicEducationId == 0)
                {
                    model.TotalIncompletedDemographics++;
                }

                if (model.DemographicMaritalStatusId == 0)
                {
                    model.TotalIncompletedDemographics++;
                }
                if (model.DemographicMonthlyHouseHoldIncomeId == 0)
                {
                    model.TotalIncompletedDemographics++;
                }
                if (model.DemographicMonthlyIncomeId == 0)
                {
                    model.TotalIncompletedDemographics++;
                }
                if (model.DemographicStateId == 0)
                {
                    model.TotalIncompletedDemographics++;
                }
                #endregion

                return model;
            }
        }
    }

    public class MasterMemberShippingAddressViewModel
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
    public class EditPageViewModel
    {
        public int TotalIncompletedDemographics { get; set; }
        public int Id { get; set; }
        public string UserId { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public int AvailablePoints { get; set; }
        [Required(ErrorMessage = "The Address field is required.")]
        [StringLength(255, MinimumLength = 5, ErrorMessage = "Maximum 255 characters")]
        public string AddressLine1 { get; set; }

        [StringLength(255, MinimumLength = 5, ErrorMessage = "Maximum 255 characters")]
        public string AddressLine2 { get; set; }

        [Required(ErrorMessage = "The postcode field is required.")]
        [StringLength(15, MinimumLength = 5, ErrorMessage = "Maximum 15 characters")]        
        public string Postcode { get; set; }
        
        [Required(ErrorMessage = "The city field is required.")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Maximum 255 characters")]
        public string City { get; set; }
        
        [Required(ErrorMessage = "The state field is required.")]
        [StringLength(255, MinimumLength = 5, ErrorMessage = "Maximum 255 characters")]
        public string State { get; set; }
        
        public Nullable<short> CountryId { get; set; }
        
        [Required(ErrorMessage = "The country code field is required.")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Maximum 2 characters")]
        public string MobileCountryCode { get; set; }
        
        [Required(ErrorMessage = "The phone number field is required.")]
        [StringLength(20, MinimumLength = 9, ErrorMessage = "Mobile number should be 9 to 10 digits long")]        
        public string MobileNumber { get; set; }
        
        [Required]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "The first name field is required.")]
        public string FirstName { get; set; }
        
        [Required(ErrorMessage = "The last name field is required.")]
        
        public string LastName { get; set; }
        
        public bool EmailConfirmed { get; set; }

        public int DateOfBirthDay { get; set; }
        public int DateOfBirthMonth { get; set; }
        public int DateOfBirthYear { get; set; }

        public IEnumerable<MasterMemberShippingAddressViewModel> AlternateShippingAddress { get; set; }

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

        public int TotalItemsInCart { get; set; }

    }
}
