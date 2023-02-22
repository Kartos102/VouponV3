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

namespace Voupon.Rewards.WebApp.Services.Device.Queries
{
    public class DeviceDataQuery : IRequest<ApiResponseViewModel>
    {
        public class DeviceViewModel
        {
            public string Id { get; set; }
            public int MemberProfileId { get; set; }
            public int MasterProfileId { get; set; }
            public string Email { get; set; }
            public int AvailablePoints { get; set; }
        }
        public Guid DeviceId { get; set; }
        public class DeviceDataQueryHandler : IRequestHandler<DeviceDataQuery, ApiResponseViewModel>
        {

            VodusV2Context vodusV2Context;
            IConnectionMultiplexer connectionMultiplexer;
            private readonly IAzureBlobStorage azureBlobStorage;
            private readonly IOptions<AppSettings> appSettings;
            public DeviceDataQueryHandler(VodusV2Context vodusV2Context, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage, IOptions<AppSettings> appSettings)
            {
                this.vodusV2Context = vodusV2Context;
                this.connectionMultiplexer = connectionMultiplexer;
                this.azureBlobStorage = azureBlobStorage;
                this.appSettings = appSettings;
            }


            public async Task<ApiResponseViewModel> Handle(DeviceDataQuery request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var devices = await vodusV2Context.Devices.AsNoTracking().Where(x => x.Id == request.DeviceId).FirstOrDefaultAsync();
                    if (devices == null)
                    {
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                    var memberProfile = await vodusV2Context.MemberProfiles.AsNoTracking().Where(x => x.Id == devices.MemberProfileId).FirstOrDefaultAsync();
                    if (!memberProfile.MasterMemberProfileId.HasValue)
                    {
                        apiResponseViewModel.Successful = true;
                        apiResponseViewModel.Data = new DeviceViewModel
                        {
                            Id = devices.Id.ToString(),
                            //MemberProfileId = devices.MemberProfileId,
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
                        Id = devices.Id.ToString(),
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
