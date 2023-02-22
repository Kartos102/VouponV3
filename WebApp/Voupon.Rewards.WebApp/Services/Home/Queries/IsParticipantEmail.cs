using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voupon.Rewards.WebApp.ViewModels;
using Voupon.Database.Postgres.VodusEntities;
using System.Threading;

namespace Voupon.Rewards.WebApp.Services.Home.Queries
{
    public class IsParticipantEmail: IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public string Email { get; set; }
    }
    public class IsParticipantEmailHandler : IRequestHandler<IsParticipantEmail, ApiResponseViewModel>
    {
        VodusV2Context VodusV2Context;

        public IsParticipantEmailHandler(VodusV2Context vodusV2Context)
        {
            this.VodusV2Context = vodusV2Context;
        }

        public async Task<ApiResponseViewModel> Handle(IsParticipantEmail request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            response.Data = false;

            try
            {
                var participant = VodusV2Context.HUTSurveyParticipants.Where(x => x.HUTSurveyProjectId == request.Id && x.IsDeleted == false && x.IsQualified == true && x.Email == request.Email.ToLower()).Count();
                response.Successful = true;
                if (participant > 0)
                {
                    response.Data = true;
                }
                else
                {
                    response.Data = false;

                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
