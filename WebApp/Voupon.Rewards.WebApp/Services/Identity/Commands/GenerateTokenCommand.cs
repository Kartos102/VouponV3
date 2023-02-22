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

    public class GenerateTokenCommand : IRequest<ApiResponseViewModel>
    {
        public Guid TempToken { get; set; }
        public string Token { get; set; }

        public string UserName { get; set; }

        public class GenerateTokenCommandHandler : IRequestHandler<GenerateTokenCommand, ApiResponseViewModel>
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

            public GenerateTokenCommandHandler(VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings, UserManager<Users> userManager, SignInManager<Users> signInManager)
            {
                this.vodusV2Context = vodusV2Context;
                this.appSettings = appSettings;
                this.userManager = userManager;
                this.signInManager = signInManager;
            }

            public async Task<ApiResponseViewModel> Handle(GenerateTokenCommand request, CancellationToken cancellationToken)
            {

                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var existingTempToken = await vodusV2Context.TempTokens.Where(x => x.Id == request.TempToken).FirstOrDefaultAsync();
                    if (request.TempToken != null)
                    {

                        if (existingTempToken != null)
                        {
                            if (existingTempToken.MemberProfileId.HasValue && existingTempToken.MemberProfileId != 0)
                            {
                                if (!string.IsNullOrEmpty(existingTempToken.Token))
                                {
                                    apiResponseViewModel.Data = existingTempToken.Token;
                                    apiResponseViewModel.Successful = true;
                                    return apiResponseViewModel;
                                }
                            }
                        }
                    }

                    var memberProfileDTO = new MemberProfiles()
                    {
                        Guid = Guid.NewGuid().ToString(),
                        CreatedAt = DateTime.Now,
                        CreatedAtCountryCode = "my",
                        CreatedAtPartnerWebsiteId = 0,
                        IsMasterProfile = false
                    };

                    vodusV2Context.MemberProfiles.Add(memberProfileDTO);

                    await vodusV2Context.SaveChangesAsync();

                    existingTempToken.MemberProfileId = memberProfileDTO.Id;
                    vodusV2Context.TempTokens.Update(existingTempToken);
                    await vodusV2Context.SaveChangesAsync();

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
                    apiResponseViewModel.Message = "Fail to sync token";
                    return apiResponseViewModel;
                }
            }
        }
    }

}
