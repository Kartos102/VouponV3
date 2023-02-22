using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voupon.Rewards.WebApp.ViewModels;
using Voupon.Database.Postgres.VodusEntities;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Voupon.Common.Enum;

namespace Voupon.Rewards.WebApp.Services.Home.Commands
{
    public class UpdateParticipantDetalsFromMasterProfile : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
    }

    public class UpdateParticipantDetalsFromMasterProfileHandler : IRequestHandler<UpdateParticipantDetalsFromMasterProfile, ApiResponseViewModel>
    {
        VodusV2Context vodusV2Context;

        public UpdateParticipantDetalsFromMasterProfileHandler(VodusV2Context vodusV2)
        {
            this.vodusV2Context = vodusV2;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateParticipantDetalsFromMasterProfile request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                /*
                var masterData = await vodusV2Context.MasterMemberProfiles.Include(x => x.MemberProfiles).Include(x => x.User).Where(x => x.Id == request.Id).FirstOrDefaultAsync();
                if (masterData == null)
                {
                    response.Code = -2;
                    response.Successful = false;
                    response.Message = "Please login first";
                    response.Data = new HUTSurveyParticipants();
                    return response;
                }

                if (masterData.User.Email.ToLower() != request.Email.ToLower())
                {
                    response.Successful = false;
                    response.Message = "Please login using the email that you have signed up with for the Vodus Product Test";
                    response.Data = new HUTSurveyParticipants();
                    return response;
                }

                //var surveyParticipant = await vodusV2Context.HUTSurveyParticipants.Where(x => x.Email == masterData.User.Email.ToLower() && x.HUTSurveyProjectId == request.ProjectId && x.IsDeleted != true).FirstOrDefaultAsync();

                var surveyParticipant = await vodusV2Context.HUTSurveyParticipants.Where(x => x.HUTSurveyProjectId == request.ProjectId && x.IsDeleted != true).FirstOrDefaultAsync();
                if (surveyParticipant == null)
                {
                    response.Successful = false;
                    response.Message = "This email is not qualified to do this test";
                    response.Data = surveyParticipant;
                    return response;

                }
                if (surveyParticipant.StatusTypeId == 1)
                {
                    surveyParticipant.Name = masterData.User.FirstName + " " + masterData.User.LastName;

                    var requireDemographic = new List<int>();

                    requireDemographic.Add((int)DemographicTypeEnum.State);

                    var extensions = await vodusV2Context.MemberProfileExtensions.ToListAsync();
                    //var extensions = masterData.MemberProfiles.Where(x => x.IsMasterProfile == true).FirstOrDefault().MemberProfileExtensions.ToList();

                    surveyParticipant.Demographics = DemographicCrossTab(extensions);
                    surveyParticipant.StatusTypeId = 2;
                    surveyParticipant.UserId = masterData.Id;
                    surveyParticipant.UpdatedAt = DateTime.Now;
                    surveyParticipant.UpdatedBy = masterData.User.UserName;
                    surveyParticipant.Address = request.Address;
                    vodusV2Context.HUTSurveyParticipants.Update(surveyParticipant);
                    vodusV2Context.SaveChanges();
                    response.Successful = true;
                    response.Message = "Successfully registered";
                    response.Data = surveyParticipant;
                }
                else
                {
                    response.Successful = false;
                    response.Message = "This account is registered";
                    response.Data = surveyParticipant;
                }
                */

            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        //  Required demographic State X Gender X Age X Race
        private string DemographicCrossTab(List<MemberProfileExtensions> extensions)
        {
            var state = extensions.Where(x => x.DemographicTypeId == (int)DemographicTypeEnum.State).First().Value;
            var gender = extensions.Where(x => x.DemographicTypeId == (int)DemographicTypeEnum.Gender).First().Value;
            var age = extensions.Where(x => x.DemographicTypeId == (int)DemographicTypeEnum.Age).First().Value;
            var ethnicity = extensions.Where(x => x.DemographicTypeId == (int)DemographicTypeEnum.Ethnicity).First().Value;

            return $"{state} X {gender} X {age} X {ethnicity}";
        }
    }



}
