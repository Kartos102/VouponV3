using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Identity.Commands
{
    public class LogoutCommand : IRequest<ApiResponseViewModel>
    {
        public string Token { get; set; }
        public Guid DeviceId { get; set; }

        public int PartnerWebsiteId { get; set; }

        public class LogoutPartnerWebsiteViewModel
        {
            public int Id { get; set; }
            public string Url { get; set; }
            public string Name { get; set; }
        }
        public class LogoutCommandHandler : IRequestHandler<LogoutCommand, ApiResponseViewModel>
        {
            private readonly SignInManager<Users> signInManager;
            private readonly VodusV2Context vodusV2Context;
            public LogoutCommandHandler(SignInManager<Users> signInManager, VodusV2Context vodusV2Context)
            {
                this.signInManager = signInManager;
                this.vodusV2Context = vodusV2Context;
            }

            public async Task<ApiResponseViewModel> Handle(LogoutCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                apiResponseViewModel.Data = null;

                try
                {
                    if (request.DeviceId != null)
                    {
                        var device = await vodusV2Context.Devices.Where(x => x.Id == request.DeviceId).FirstOrDefaultAsync();
                        if (device != null)
                        {

                            var newMemberProfile = new MemberProfiles
                            {
                                Guid = Guid.NewGuid().ToString(),
                                CreatedAt = DateTime.Now
                            };

                            vodusV2Context.MemberProfiles.Add(newMemberProfile);
                            await vodusV2Context.SaveChangesAsync();

                            device.MemberProfileId = newMemberProfile.Id;
                            vodusV2Context.Devices.Update(device);
                            await vodusV2Context.SaveChangesAsync();
                        }
                    }

                    if (!string.IsNullOrEmpty(request.Token))
                    {
                        var loggedOutTokens = new LoggedOutTokens
                        {
                            Token = request.Token,
                            CreatedAt = DateTime.Now,
                            CreatedAtTimestamp = int.Parse(DateTime.Now.ToString("yyyyMMdd"))
                        };

                        vodusV2Context.LoggedOutTokens.Add(loggedOutTokens);
                        await vodusV2Context.SaveChangesAsync();
                    }

                    if (request.PartnerWebsiteId != 0)
                    {
                        var partnerWebsite = await vodusV2Context.PartnerWebsites.Where(x => x.Id == request.PartnerWebsiteId).FirstOrDefaultAsync();
                        if (partnerWebsite != null)
                        {
                            var viewModel = new LogoutPartnerWebsiteViewModel
                            {
                                Id = partnerWebsite.Id,
                                Name = partnerWebsite.Name,
                                Url = partnerWebsite.Url
                            };
                            apiResponseViewModel.Data = viewModel;
                        }
                    }

                    await signInManager.SignOutAsync();
                  
                    return apiResponseViewModel;

                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Fail to logout account";
                    return apiResponseViewModel;
                }
            }
        }
    }
}
