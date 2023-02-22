using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Enum;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.Services.Logger;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.HUT.Commands
{
    public class UpdateParticipantProductRegistrationCommand : IRequest<ApiResponseViewModel>
    {
        public int MasterMemberProfileId { get; set; }
        public int ProjectId { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Postcode { get; set; }
        public string Email { get; set; }
        public class UpdateParticipantProductRegistrationCommandHandler : IRequestHandler<UpdateParticipantProductRegistrationCommand, ApiResponseViewModel>
        {
            private readonly VodusV2Context vodusV2Context;
            private readonly RewardsDBContext rewardsDBContext;

            public UpdateParticipantProductRegistrationCommandHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.vodusV2Context = vodusV2Context;
            }

            public async Task<ApiResponseViewModel> Handle(UpdateParticipantProductRegistrationCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();

                //var masterData = await vodusV2Context.MasterMemberProfiles.Include(x => x.MemberProfiles).ThenInclude(x => x.MemberProfileExtensions).Include(x => x.User).Where(x => x.Id == request.MasterMemberProfileId).FirstOrDefaultAsync();

                try
                {
                    var masterData = await vodusV2Context.MasterMemberProfiles.Include(x => x.User).Where(x => x.Id == request.MasterMemberProfileId).FirstOrDefaultAsync();
                    if (masterData == null)
                    {
                        apiResponseViewModel.Code = -2;
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Please login first";
                        apiResponseViewModel.Data = new HUTSurveyParticipants();
                        return apiResponseViewModel;
                    }

                    if (masterData.User.Email.ToLower() != request.Email.ToLower())
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Please login using the email that you have signed up with for the Vodus Product Test";
                        apiResponseViewModel.Data = new HUTSurveyParticipants();
                        return apiResponseViewModel;
                    }

                    var surveyParticipant = await vodusV2Context.HUTSurveyParticipants.Where(x => x.Email == masterData.User.Email.ToLower() && x.HUTSurveyProjectId == request.ProjectId && x.IsDeleted == false).FirstOrDefaultAsync();
                    //var surveyParticipant = await vodusV2Context.HUTSurveyParticipants.Where(x => x.HUTSurveyProjectId == request.ProjectId && x.IsDeleted == false).FirstOrDefaultAsync();

                    if (surveyParticipant == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "This email is not qualified to do this test";
                        apiResponseViewModel.Data = surveyParticipant;
                        return apiResponseViewModel;

                    }
                    if (surveyParticipant.StatusTypeId == 1)
                    {
                        surveyParticipant.Name = masterData.User.FirstName + " " + masterData.User.LastName;

                        var requireDemographic = new List<int>();

                        requireDemographic.Add((int)DemographicTypeEnum.State);

                        //var extensions = await vodusV2Context.MemberProfileExtensions.ToListAsync();
                        var memberProfiles = await vodusV2Context.MemberProfiles.Where(x => x.Id == masterData.MemberProfileId).FirstOrDefaultAsync();
                        var extensions = await vodusV2Context.MemberProfileExtensions.Where(x => x.MemberProfileId == masterData.MemberProfileId).ToListAsync();

                        var demographicValues = await vodusV2Context.DemographicValues.ToListAsync();

                        foreach (var item in extensions)
                        {
                            if (item.Value.Contains("@p"))
                            {
                                item.Value = demographicValues.Where(x => x.Id == item.DemographicValueId).FirstOrDefault().DisplayValue;
                                vodusV2Context.MemberProfileExtensions.Update(item);
                            }
                        }

                        if(extensions != null && extensions.Any())
                        {
                            surveyParticipant.Demographics = DemographicCrossTab(extensions);
                        }
                        else
                        {
                            surveyParticipant.Demographics = " X ";
                        }
                        
                        surveyParticipant.StatusTypeId = 2;
                        surveyParticipant.UserId = masterData.Id;
                        surveyParticipant.UpdatedAt = DateTime.Now;
                        surveyParticipant.UpdatedBy = masterData.User.UserName;
                        surveyParticipant.Address = request.Address + ", " + request.Postcode + ", " + request.City + ", " + request.State;
                        surveyParticipant.PhoneNumber = request.PhoneNumber;
                        surveyParticipant.City = request.City;
                        surveyParticipant.Postcode = request.Postcode;
                        vodusV2Context.HUTSurveyParticipants.Update(surveyParticipant);
                        vodusV2Context.SaveChanges();

                        apiResponseViewModel.Successful = true;
                        apiResponseViewModel.Message = "Successfully registered";
                        apiResponseViewModel.Data = surveyParticipant;
                    }
                    else
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "This account is registered";
                        apiResponseViewModel.Data = surveyParticipant;
                    }

                    apiResponseViewModel.Successful = true;
                    return apiResponseViewModel;
                }
                catch (Exception ex)
                {
                    var errorLogs = new ErrorLogs
                    {
                        TypeId = CreateErrorLogCommand.Type.Service,
                        ActionName = "UpdateParticipantProductRegistrationCommand",
                        ActionRequest = JsonConvert.SerializeObject(request),
                        CreatedAt = DateTime.Now,
                        Errors = ex.ToString()
                    };

                    await rewardsDBContext.ErrorLogs.AddAsync(errorLogs);
                    await rewardsDBContext.SaveChangesAsync();

                    apiResponseViewModel.Message = "Something is not right, pleease try again later or contact support for help [099]";
                    apiResponseViewModel.Successful = false;
                    return apiResponseViewModel;
                }
                
            }

            //  Required demographic State X Gender X Age X Race
            private string DemographicCrossTab(List<MemberProfileExtensions> extensions)
            {
                var state = extensions.Where(x => x.DemographicTypeId == (int)DemographicTypeEnum.State).FirstOrDefault();
                var gender = extensions.Where(x => x.DemographicTypeId == (int)DemographicTypeEnum.Gender).FirstOrDefault();
                var age = extensions.Where(x => x.DemographicTypeId == (int)DemographicTypeEnum.Age).FirstOrDefault();
                var ethnicity = extensions.Where(x => x.DemographicTypeId == (int)DemographicTypeEnum.Ethnicity).FirstOrDefault();

                return $"{(state != null ? state.Value : "")} X {(gender != null ? gender.Value : "")} X {(age != null ? age.Value : "")} X {(ethnicity != null ? ethnicity.Value : "")}";
            }

        }
    }

}
