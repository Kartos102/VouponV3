using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Identity.Queries
{
    public class GetMemberProfileTokenFromTempTokenQuery : IRequest<ApiResponseViewModel>
    {
        public Guid Id { get; set; }
    }
    public class GetMemberProfileTokenFromTempTokenQueryHandler : IRequestHandler<GetMemberProfileTokenFromTempTokenQuery, ApiResponseViewModel>
    {
        private readonly VodusV2Context _vodusV2Context;


        public GetMemberProfileTokenFromTempTokenQueryHandler(VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings)
        {
            _vodusV2Context = vodusV2Context;

        }
        public async Task<ApiResponseViewModel> Handle(GetMemberProfileTokenFromTempTokenQuery request, CancellationToken cancellationToken)
        {
            var apiResponseViewModel = new ApiResponseViewModel();
            try
            {
                var tempTokens = await _vodusV2Context.TempTokens.Where(x => x.Id == request.Id).FirstOrDefaultAsync();
                if (tempTokens == null)
                {
                    apiResponseViewModel.Successful = false;
                    return apiResponseViewModel;
                }

                var memberProfileDTO = await _vodusV2Context.MemberProfiles.Where(x => x.Id == tempTokens.MemberProfileId).FirstOrDefaultAsync();

                var newTokenObject = new UserToken
                {
                    Guid = memberProfileDTO.Guid,
                    MemberMasterId = 0,
                    MemberProfileId = memberProfileDTO.Id,
                    CurrentCommercialId = 0,
                    LastAnsweredSurveyQuestionId = 0,
                    CreatedAt = DateTime.Now
                };

                var token = newTokenObject.ToTokenValue();
                apiResponseViewModel.Data = token;

                apiResponseViewModel.Successful = true;

                return apiResponseViewModel;
            }
            catch (Exception ex)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Fail to get temptoken";
                return apiResponseViewModel;

            }
        }
    }
}
