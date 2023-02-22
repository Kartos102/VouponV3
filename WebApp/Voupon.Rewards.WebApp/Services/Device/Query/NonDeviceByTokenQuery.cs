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
using Voupon.Common;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Device.Queries
{
    public class NonDeviceByTokenQuery : IRequest<ApiResponseViewModel>
    {
        public class DeviceViewModel
        {
            public string Id { get; set; }
            public int MemberProfileId { get; set; }
            public int MasterProfileId { get; set; }
            public string Email { get; set; }
            public int AvailablePoints { get; set; }
        }
        public string Token { get; set; }

        public class NonDeviceByTokenQueryHandler : IRequestHandler<NonDeviceByTokenQuery, ApiResponseViewModel>
        {

            VodusV2Context vodusV2Context;
            IConnectionMultiplexer connectionMultiplexer;
            private readonly IAzureBlobStorage azureBlobStorage;
            private readonly IOptions<AppSettings> appSettings;
            public NonDeviceByTokenQueryHandler(VodusV2Context vodusV2Context, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage, IOptions<AppSettings> appSettings)
            {
                this.vodusV2Context = vodusV2Context;
                this.connectionMultiplexer = connectionMultiplexer;
                this.azureBlobStorage = azureBlobStorage;
                this.appSettings = appSettings;
            }


            public async Task<ApiResponseViewModel> Handle(NonDeviceByTokenQuery request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var userTokenObject = UserToken.FromTokenValue(request.Token);
                    if (userTokenObject == null)
                    {
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }


                    var memberProfile = await vodusV2Context.MemberProfiles.AsNoTracking().Where(x => x.Id == userTokenObject.MemberProfileId).FirstOrDefaultAsync();
                    if(memberProfile == null)
                    {
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                    if (!memberProfile.MasterMemberProfileId.HasValue)
                    {
                        apiResponseViewModel.Successful = true;
                        apiResponseViewModel.Data = new DeviceViewModel
                        {
                            //MemberProfileId = userTokenObject.MemberProfileId,
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
                    apiResponseViewModel.Data = new DeviceViewModel
                    {
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
