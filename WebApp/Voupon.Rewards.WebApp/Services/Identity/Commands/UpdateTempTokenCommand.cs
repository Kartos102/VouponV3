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

    public class UpdateTempTokenCommand : IRequest<ApiResponseViewModel>
    {
        public Guid TempToken { get; set; }
        public string Token { get; set; }

        public string UserName { get; set; }

        public class UpdateTempTokenCommandHandler : IRequestHandler<UpdateTempTokenCommand, ApiResponseViewModel>
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

            public UpdateTempTokenCommandHandler(VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings, UserManager<Users> userManager, SignInManager<Users> signInManager)
            {
                this.vodusV2Context = vodusV2Context;
                this.appSettings = appSettings;
                this.userManager = userManager;
                this.signInManager = signInManager;
            }

            public async Task<ApiResponseViewModel> Handle(UpdateTempTokenCommand request, CancellationToken cancellationToken)
            {

                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    if (request.TempToken == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid request [1]";
                        return apiResponseViewModel;
                    }

                    var tempToken = await vodusV2Context.TempTokens.Where(x => x.Id == request.TempToken).FirstOrDefaultAsync();
                    if (tempToken == null)
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

                    if (userTokenObject != null)
                    {
                        tempToken.MemberProfileId = userTokenObject.MemberProfileId;
                        tempToken.Token = request.Token;
                        tempToken.LastUpdatedAt = DateTime.Now;

                        if(tempToken.MemberProfileId.HasValue && tempToken.MemberProfileId.Value > 0)
                        {
                            var existing = await vodusV2Context.MemberProfiles.Where(x => x.Id == tempToken.MemberProfileId.Value).FirstOrDefaultAsync();
                            if(existing != null)
                            {
                                existing.SyncMemberProfileId = userTokenObject.MemberProfileId;
                                existing.SyncMemberProfileToken = request.Token;
                                vodusV2Context.MemberProfiles.Update(existing);

                            }
                        }

                        vodusV2Context.TempTokens.Update(tempToken);
                        await vodusV2Context.SaveChangesAsync();
                    }


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
