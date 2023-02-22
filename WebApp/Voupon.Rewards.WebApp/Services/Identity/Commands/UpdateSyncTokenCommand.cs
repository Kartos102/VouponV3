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

    public class UpdateSyncTokenCommand : IRequest<ApiResponseViewModel>
    {
        public string SyncToken { get; set; }
        public string Token { get; set; }

        public string UserName { get; set; }

        public class UpdateSyncTokenCommandHandler : IRequestHandler<UpdateSyncTokenCommand, ApiResponseViewModel>
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

            public UpdateSyncTokenCommandHandler(VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings, UserManager<Users> userManager, SignInManager<Users> signInManager)
            {
                this.vodusV2Context = vodusV2Context;
                this.appSettings = appSettings;
                this.userManager = userManager;
                this.signInManager = signInManager;
            }

            public async Task<ApiResponseViewModel> Handle(UpdateSyncTokenCommand request, CancellationToken cancellationToken)
            {

                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    if (string.IsNullOrEmpty(request.SyncToken))
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid request [1]";
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

                    var syncTokenObject = UserToken.FromTokenValue(request.SyncToken);
                    if (syncTokenObject == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid request [3]";
                        return apiResponseViewModel;
                    }

                    var memberprofile = await vodusV2Context.MemberProfiles.Where(x => x.Id == syncTokenObject.MemberProfileId).FirstOrDefaultAsync();

                    if (memberprofile == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid request [4]";
                        return apiResponseViewModel;
                    }

                    if (memberprofile.SyncMemberProfileId == userTokenObject.MemberProfileId)
                    {
                        var master = await vodusV2Context.MasterMemberProfiles.Include(x => x.User).Where(z => z.User.Email == request.UserName).FirstOrDefaultAsync();
                        if(master != null)
                        {
                            memberprofile.MasterMemberProfileId = master.Id;
                        }
                    }

                    memberprofile.SyncMemberProfileId = userTokenObject.MemberProfileId;
                    memberprofile.SyncMemberProfileToken = request.Token;

                    vodusV2Context.MemberProfiles.Update(memberprofile);
                    await vodusV2Context.SaveChangesAsync();

                    apiResponseViewModel.Successful = true;
                    return apiResponseViewModel;
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = ex.ToString();
                    return apiResponseViewModel;
                }
            }
        }
    }

}
