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
    public class GetHUTSurveyProjectDetailsById : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
    }

    public class GetHUTSurveyProjectDetailsByIdHandler : IRequestHandler<GetHUTSurveyProjectDetailsById, ApiResponseViewModel>
    {
        VodusV2Context VodusV2Context;

        public GetHUTSurveyProjectDetailsByIdHandler(VodusV2Context vodusV2)
        {
            this.VodusV2Context = vodusV2;
        }

        public async Task<ApiResponseViewModel> Handle(GetHUTSurveyProjectDetailsById request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var surveyProject = VodusV2Context.HUTSurveyProjects.Where(x => x.Id == request.Id).FirstOrDefault();
                response.Successful = true;
                response.Message = "Get survey project info Successfully";
                response.Data = surveyProject;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
    }

   
   
}
