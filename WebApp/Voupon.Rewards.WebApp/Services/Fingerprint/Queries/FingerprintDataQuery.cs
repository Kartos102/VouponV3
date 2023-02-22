using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Fingerprint.Queries
{
    public class FingerprintDataQuery : IRequest<ApiResponseViewModel>
    {
        public class FingerprintViewModel
        {
            public string Id { get; set; }
            public int MemberProfileId { get; set; }
            public int MasterProfileId { get; set; }
            public string Email { get; set; }
            public int AvailablePoints { get; set; }

            public string VisitorId { get; set; }
            public string DeviceId { get; set; }
        }
        public string FingerprintVisitorId { get; set; }
        public class FingerprintDataQueryHandler : IRequestHandler<FingerprintDataQuery, ApiResponseViewModel>
        {

            VodusV2Context vodusV2Context;
            IConnectionMultiplexer connectionMultiplexer;
            private readonly IAzureBlobStorage azureBlobStorage;
            private readonly IOptions<AppSettings> appSettings;
            public FingerprintDataQueryHandler(VodusV2Context vodusV2Context, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage, IOptions<AppSettings> appSettings)
            {
                this.vodusV2Context = vodusV2Context;
                this.connectionMultiplexer = connectionMultiplexer;
                this.azureBlobStorage = azureBlobStorage;
                this.appSettings = appSettings;
            }


            public async Task<ApiResponseViewModel> Handle(FingerprintDataQuery request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var fingerprint = await vodusV2Context.Fingerprints.AsNoTracking().Where(x => x.VisitorId == request.FingerprintVisitorId).FirstOrDefaultAsync();
                    if (fingerprint == null || fingerprint.MemberProfileId == 0)
                    {
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                    var memberProfile = await vodusV2Context.MemberProfiles.AsNoTracking().Where(x => x.Id == fingerprint.MemberProfileId).FirstOrDefaultAsync();
                    if (!memberProfile.MasterMemberProfileId.HasValue)
                    {
                        apiResponseViewModel.Successful = true;
                        apiResponseViewModel.Data = new FingerprintViewModel
                        {
                            Id = fingerprint.Id.ToString(),
                            VisitorId = fingerprint.VisitorId,
                            //MemberProfileId = fingerprint.MemberProfileId,
                            AvailablePoints = memberProfile.AvailablePoints + memberProfile.DemographicPoints
                        };
                        return apiResponseViewModel;
                    }

                    var master = await vodusV2Context.MasterMemberProfiles.AsNoTracking().Include(x => x.User).Where(x => x.Id == memberProfile.MasterMemberProfileId.Value).FirstOrDefaultAsync();
                    if (master == null)
                    {
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Data = new FingerprintViewModel
                    {
                        Id = fingerprint.Id.ToString(),
                        VisitorId = fingerprint.VisitorId,
                        AvailablePoints = master.AvailablePoints,
                        //MemberProfileId = memberProfile.Id,
                        MasterProfileId = master.Id,
                        Email = master.User.Email
                    };

                    return apiResponseViewModel;
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Message = ex.Message;
                }

                return apiResponseViewModel;
            }
        }
    }

}
