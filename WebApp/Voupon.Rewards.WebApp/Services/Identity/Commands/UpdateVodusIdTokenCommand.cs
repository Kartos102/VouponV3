using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Identity.Commands
{

    public class UpdateVodusIdTokenCommand : IRequest<ApiResponseViewModel>
    {
        public Guid VodusId { get; set; }
        public string Token { get; set; }

        public string UserName { get; set; }

        public class UpdateVodusIdTokenCommandHandler : IRequestHandler<UpdateVodusIdTokenCommand, ApiResponseViewModel>
        {
            private readonly VodusV2Context vodusV2Context;
            private readonly UserManager<Users> userManager;
            private readonly IOptions<AppSettings> appSettings;
            private readonly SignInManager<Users> signInManager;

            private string GetRootDomain(string host)
            {
                var filterHost = host.Replace("http://", "").Replace("https://", "");
                return filterHost.Split('/')[0];
            }

            public UpdateVodusIdTokenCommandHandler(VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings, UserManager<Users> userManager, SignInManager<Users> signInManager)
            {
                this.vodusV2Context = vodusV2Context;
                this.appSettings = appSettings;
                this.userManager = userManager;
                this.signInManager = signInManager;
            }

            public async Task<ApiResponseViewModel> Handle(UpdateVodusIdTokenCommand request, CancellationToken cancellationToken)
            {

                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    if (request.VodusId == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid request [1]";
                        return apiResponseViewModel;
                    }

                    var existingVodusId = await vodusV2Context.MobileDevices.Where(x => x.VodusId == request.VodusId).FirstOrDefaultAsync();
                    if (existingVodusId == null)
                    {
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                    //usertoken
                    var userTokenObject = UserToken.FromTokenValue(request.Token);
                    if (userTokenObject == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid request [2]";
                        return apiResponseViewModel;
                    }

                    existingVodusId.MemberProfileId = userTokenObject.MemberProfileId;
                    existingVodusId.Token = request.Token;
                    existingVodusId.LastUpdatedAt = DateTime.Now;


                    await vodusV2Context.SaveChangesAsync();
                    vodusV2Context.MobileDevices.Update(existingVodusId);

                    apiResponseViewModel.Successful = true;
                    return apiResponseViewModel;
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Fail to sync token";
                    return apiResponseViewModel;
                }
            }
        }
    }

}
