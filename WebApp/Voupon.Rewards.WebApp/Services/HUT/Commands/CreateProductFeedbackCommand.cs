using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.HUT.Commands
{
    public class CreateProductFeedbackCommand : IRequest<ApiResponseViewModel>
    {
        public int HUTSurveyFormId { get; set; }
        public int HUTSurveyProjectId { get; set; }
        public string Email { get; set; }
        public class CreateProductFeedbackCommandHandler : IRequestHandler<CreateProductFeedbackCommand, ApiResponseViewModel>
        {
            private readonly VodusV2Context vodusV2Context;
            private readonly RewardsDBContext rewardsDBContext;

            public CreateProductFeedbackCommandHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.vodusV2Context = vodusV2Context;
            }

            public async Task<ApiResponseViewModel> Handle(CreateProductFeedbackCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();

                //  Get email from username
                if (!request.Email.Contains("@"))
                {
                    var user = await vodusV2Context.Users.Where(x => x.UserName.ToLower() == request.Email.ToLower()).FirstOrDefaultAsync();
                    if (user == null)
                    {
                        apiResponseViewModel.Message = "Invalid request [001.0]";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }
                    request.Email = user.Email;
                }

                var participant = await vodusV2Context.HUTSurveyParticipants.Where(x => x.Email.ToLower() == request.Email.ToLower() && x.HUTSurveyProjectId == request.HUTSurveyProjectId && x.IsDeleted == false).FirstOrDefaultAsync();
                if (participant == null)
                {
                    apiResponseViewModel.Message = "Invalid request [001]";
                    apiResponseViewModel.Successful = false;
                    return apiResponseViewModel;
                }

                var response = await vodusV2Context.HUTSurveyResponses.Include(x => x.Participant).Where(x => x.Participant.Email.ToLower() == request.Email.ToLower() && x.HUTSurveyFormId == request.HUTSurveyFormId && x.Participant.IsDeleted == false).FirstOrDefaultAsync();

                if (response != null)
                {
                    apiResponseViewModel.Message = "Invalid request. Already responded before [002]";
                    apiResponseViewModel.Successful = false;
                    return apiResponseViewModel;
                }

                var hutSurveyResponses = new HUTSurveyResponses
                {
                    HUTSurveyFormId = request.HUTSurveyFormId,
                    CreatedAt = DateTime.Now,
                    ParticipantId = participant.Id
                };

                var totalForms = vodusV2Context.HUTSurveyForms.Where(x => x.HUTSurveyProjectId == request.HUTSurveyProjectId && x.IsDeleted == false).Count();

                participant.CompletedForms = participant.CompletedForms + 1;

                if (participant.CompletedForms == totalForms)
                {
                    participant.StatusTypeId = 3;
                }

                participant.CurrentFormId = null;

                vodusV2Context.HUTSurveyParticipants.Update(participant);
                vodusV2Context.HUTSurveyResponses.Add(hutSurveyResponses);
                await vodusV2Context.SaveChangesAsync();

                apiResponseViewModel.Successful = true;
                return apiResponseViewModel;
            }
        }
    }

}
