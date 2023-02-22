using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.ViewModels;
using static Voupon.Rewards.WebApp.Services.HUT.Pages.ProductTestRegistrationPage;

namespace Voupon.Rewards.WebApp.Services.HUT.Pages
{
    public class ProductTestRegistrationPage : IRequest<ProductTestRegistrationViewModel>
    {
        public int MasterMemberprofileId { get; set; }
        public int ProjectId { get; set; }
        public string Email { get; set; }
        private class ProductTestRegistrationPageHandler : IRequestHandler<ProductTestRegistrationPage, ProductTestRegistrationViewModel>
        {
            RewardsDBContext rewardsDBContext;
            VodusV2Context vodusV2Context;
            IOptions<AppSettings> appSettings;
            public ProductTestRegistrationPageHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.vodusV2Context = vodusV2Context;
                this.appSettings = appSettings;
            }

            public async Task<ProductTestRegistrationViewModel> Handle(ProductTestRegistrationPage request, CancellationToken cancellationToken)
            {
                var viewModel = new ProductTestRegistrationViewModel();
            
                try
                {
                    var masterData = await vodusV2Context.MasterMemberProfiles.Include(x => x.User).Where(x => x.Id == request.MasterMemberprofileId).FirstOrDefaultAsync();

                    if (masterData == null)
                    {
                        return null;
                    }

                    if (masterData.User.Email.ToLower() != request.Email.ToLower())
                    {
                        var hutProjectObject = await vodusV2Context.HUTSurveyProjects.Include(x => x.HUTSurveyForms).Include(x => x.HUTSurveyParticipants).Where(x => x.Id == request.ProjectId).FirstOrDefaultAsync();

                        var participantObject = hutProjectObject.HUTSurveyParticipants.Where(x => x.Email == request.Email && x.IsQualified == true && x.IsDeleted == false).Any();

                        if (hutProjectObject == null)
                        {
                            viewModel.ErrorMessage = "Invalid request [001]";
                            viewModel.IsValidRequest = false;
                            return viewModel;
                        }
                        viewModel.SurveyProjectStatus = hutProjectObject.IsActive;
                        viewModel.SurveyProjectLanguageId = hutProjectObject.LanguageId;
                        viewModel.IsDifferentEmail = true;
                        viewModel.MasterMemberProfileId = masterData.Id;
                        viewModel.SurveyProjectEmail = request.Email.ToLower();
                        viewModel.IsParticipantEmail = participantObject;
                        return viewModel;
                    }
                    var memberProfile = await vodusV2Context.MemberProfiles.Where(x => x.Id == masterData.MemberProfileId).FirstOrDefaultAsync();

                    if (masterData.AddressLine1 == null || masterData.City == null
                       || masterData.CountryId == null || !masterData.DateOfBirth.HasValue
                       || memberProfile.DemographicAgeId == 0 || memberProfile.DemographicAgeId == null
                       //|| memberProfile.DemographicEducationId == 0 || memberProfile.DemographicEducationId == null
                       //|| memberProfile.DemographicEthnicityId == 0 || memberProfile.DemographicEthnicityId == null
                       || memberProfile.DemographicGenderId == 0 || memberProfile.DemographicGenderId == null
                       //|| memberProfile.DemographicMaritalStatusId == 0 || memberProfile.DemographicMaritalStatusId == null
                       //|| memberProfile.DemographicMonthlyHouseHoldIncomeId == 0 || memberProfile.DemographicMonthlyHouseHoldIncomeId == null
                       //|| memberProfile.DemographicMonthlyIncomeId == 0 || memberProfile.DemographicMonthlyIncomeId == null
                       || memberProfile.DemographicStateId == 0 || memberProfile.DemographicStateId == null
                       || string.IsNullOrEmpty(masterData.User.FirstName) || string.IsNullOrEmpty(masterData.User.LastName) || string.IsNullOrEmpty(masterData.MobileCountryCode) || string.IsNullOrEmpty(masterData.MobileNumber)
                       || string.IsNullOrEmpty(masterData.Postcode))
                    {
                        //  Set user's email to verified to skip verification since user's click email links to reach here
                        var user = await vodusV2Context.Users.Where(x => x.Email == request.Email).FirstOrDefaultAsync();

                        if (user != null)
                        {
                            user.EmailConfirmed = true;
                            vodusV2Context.Users.Update(user);
                            await vodusV2Context.SaveChangesAsync();
                        }

                        viewModel.IsValidRequest = false;
                        viewModel.RedirectUrl = $"{appSettings.Value.App.BaseUrl}/profile/edit?from=ProductTestRegister&productId={request.ProjectId}&email={request.Email}";
                    }

                    var hutProject = await vodusV2Context.HUTSurveyProjects.Include(x => x.HUTSurveyForms).Include(x => x.HUTSurveyParticipants).Where(x => x.Id == request.ProjectId).FirstOrDefaultAsync();

                    if (hutProject == null)
                    {
                        viewModel.ErrorMessage = "Invalid request [001]";
                        viewModel.IsValidRequest = false;
                        return viewModel;
                    }

                    var participant = hutProject.HUTSurveyParticipants.Where(x => x.Email.ToLower() == masterData.User.Email.ToLower() && x.IsQualified == true && x.IsDeleted == false).Any();

                    if (string.IsNullOrEmpty(masterData.AddressLine2))
                    {
                        viewModel.MasterMemberProfileAddress = masterData.AddressLine1 + ", " + masterData.AddressLine2;
                    }
                    else
                    {
                        viewModel.MasterMemberProfileAddress = masterData.AddressLine1;
                    }
                    viewModel.MasterMemberProfileCity = masterData.City;
                    viewModel.MasterMemberProfileState = masterData.State;
                    viewModel.MasterMemberProfilePostcode = masterData.Postcode;
                    viewModel.MasterMemberProfilePhoneNumber = masterData.MobileCountryCode + masterData.MobileNumber;


                    viewModel.MasterMemberProfileEmail = masterData.User.Email;
                    viewModel.MasterMemberProfileId = masterData.Id;

                    viewModel.SurveyProjectStatus = hutProject.IsActive;
                    viewModel.Reward = hutProject.VPointsReward;
                    viewModel.SurveyProjectName = hutProject.ExternalName;
                    viewModel.SurveyProjectLanguageId = hutProject.LanguageId;
                    viewModel.SurveyProjectId = hutProject.Id;
                    viewModel.SurveyProjectStartDate = hutProject.StartDate.Value;
                    viewModel.IsParticipantEmail = participant;
                }
                catch (Exception ex)
                {
                    var lala = ex.ToString();
                    viewModel.IsValidRequest = false;
                    viewModel.ErrorMessage = "Hmm. Something went wrong. Please try again later [999]";
                }
                return viewModel;
                
            }
        }
        public class ProductTestRegistrationViewModel
        {
            public int MasterMemberProfileId { get; set; }
            public string MasterMemberProfileAddress { get; set; }
            public string MasterMemberProfileCity { get; set; }
            public string MasterMemberProfileState { get; set; }
            public string MasterMemberProfilePostcode { get; set; }
            public string MasterMemberProfilePhoneNumber { get; set; }
            public string MasterMemberProfileEmail { get; set; }
            public int UserId { get; set; }
            public int SurveyProjectId { get; set; }
            public string SurveyProjectName { get; set; }
            public string SurveyProjectEmail { get; set; }
            public int SurveyProjectLanguageId { get; set; }
            public DateTime SurveyProjectStartDate { get; set; }
            public string Reward { get; set; }
            public bool IsParticipantEmail { get; set; }
            public bool SurveyProjectStatus { get; set; }
            public bool IsDifferentEmail { get; set; }

            public bool IsValidRequest { get; set; }
            public string ErrorMessage { get; set; }

            public string RedirectUrl { get; set; }

        }

    }
}
