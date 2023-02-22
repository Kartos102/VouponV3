using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Token.Queries
{
    public class MasterProfileQuery : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
    }
    public class OrderHistoryListQueryHandler : IRequestHandler<MasterProfileQuery, ApiResponseViewModel>
    {
        VodusV2Context vodusV2Context;
        public OrderHistoryListQueryHandler(VodusV2Context vodusV2Context)
        {
            this.vodusV2Context = vodusV2Context;
        }

        public async Task<ApiResponseViewModel> Handle(MasterProfileQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel apiResponse = new ApiResponseViewModel();
            try
            {
                var master = await vodusV2Context.MasterMemberProfiles.Where(x => x.Id == request.Id).FirstOrDefaultAsync();

                if (master != null)
                {
                    apiResponse.Successful = true;
                    apiResponse.Message = master.PreferLanguage;
                    return apiResponse;
                }

                return apiResponse;
            }
            catch (Exception ex)
            {
                apiResponse.Successful = false;
            }
            return apiResponse;
        }
    }
}
