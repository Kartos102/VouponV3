using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.Infrastructures.Helpers;
using Voupon.Rewards.WebApp.Services.Logger;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Identity.Commands
{



    public class UpdatePasswordCommand : IRequest<ApiResponseViewModel>
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }

        public class UpdatePasswordCommandHandler : IRequestHandler<UpdatePasswordCommand, ApiResponseViewModel>
        {
            private readonly VodusV2Context vodusV2Context;
            private readonly RewardsDBContext rewardsDBContext;
            private readonly UserManager<Voupon.Database.Postgres.VodusEntities.Users> userManager;
            private readonly IOptions<AppSettings> appSettings;
            private readonly SignInManager<Voupon.Database.Postgres.VodusEntities.Users> signInManager;

            private string GetRootDomain(string host)
            {
                var filterHost = host.Replace("http://", "").Replace("https://", "");
                return filterHost.Split('/')[0];
            }

            public UpdatePasswordCommandHandler(VodusV2Context vodusV2Context, RewardsDBContext rewardsDBContext,  IOptions<AppSettings> appSettings, UserManager<Voupon.Database.Postgres.VodusEntities.Users> userManager, SignInManager<Voupon.Database.Postgres.VodusEntities.Users> signInManager)
            {
                this.vodusV2Context = vodusV2Context;
                this.appSettings = appSettings;
                this.userManager = userManager;
                this.signInManager = signInManager;
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
            {

                var apiResponseViewModel = new ApiResponseViewModel();            

                try
                {
                    var user = userManager.Users.FirstOrDefault(x => x.Id == request.UserId);

                    if (request.Email != user.Email)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Incorrect Email";
                        return apiResponseViewModel;
                    }
                    var identity = await signInManager.PasswordSignInAsync(user, request.OldPassword, false, false);
                    if (!identity.Succeeded)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Incorrect password";
                        return apiResponseViewModel;
                    }
                    var result = await userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);

                    if (result.Succeeded)
                    {
                        apiResponseViewModel.Message = "Successfully updated Password";
                        apiResponseViewModel.Successful = true;
                    }
                    else
                    {
                        apiResponseViewModel.Message = "";
                        apiResponseViewModel.Successful = false;
                    }
                    return apiResponseViewModel;
                }
                catch (Exception ex)
                {
                    await new Logs
                    {
                        Description = ex.ToString(),
                        Email = request.Email,
                        JsonData = JsonConvert.SerializeObject(request),
                        ActionName = "UpdatePasswordCommand",
                        TypeId = CreateErrorLogCommand.Type.Service,
                        SendgridAPIKey = appSettings.Value.Mailer.Sendgrid.APIKey,
                        RewardsDBContext = rewardsDBContext
                    }.Error();

                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Incorrect email or password";
                    return apiResponseViewModel;
                }
            }

            private async Task<MemberProfiles> SetMaster(int memberMasterId)
            {
                var newMemberProfile = new MemberProfiles();
                var memberProfileList = await vodusV2Context.MemberProfiles.Where(x => x.MasterMemberProfileId == memberMasterId).ToListAsync();
                var idList = memberProfileList.Select(x => x.Id).ToList();


                var responses = await vodusV2Context.SurveyResponses.Where(x => idList.Contains(x.MemberProfileId)).ToListAsync();
                var responseGroup = responses.GroupBy(x => x.MemberProfileId).Select(group =>
                                   new
                                   {
                                       Id = group.Key,
                                       Count = group.Count()
                                   }).OrderByDescending(x => x.Count);

                var orders = await vodusV2Context.Orders.Where(x => x.MasterMemberId == memberMasterId).ToListAsync();
                var usedPoints = 0;
                var bonusPoints = 0;

                bonusPoints = vodusV2Context.BonusPoints.Where(x => x.MasterMemberProfileId == memberMasterId && x.IsActive == true).Sum(x => x.Points);

                if (orders != null && orders.Any())
                {
                    usedPoints = orders.Sum(x => x.TotalPoints);
                }
                if (responseGroup.Any())
                {
                    var item = responseGroup.First();
                    var memberEntity = await vodusV2Context.MemberProfiles.Where(x => x.Id == item.Id).FirstOrDefaultAsync();
                    memberEntity.IsMasterProfile = true;
                    vodusV2Context.MemberProfiles.Update(memberEntity);
                    await vodusV2Context.SaveChangesAsync();

                    //  Update mastermember points
                    var masterEntity = await vodusV2Context.MasterMemberProfiles.Where(x => x.Id == memberMasterId).FirstOrDefaultAsync();
                    masterEntity.AvailablePoints = (memberEntity.AvailablePoints + memberEntity.DemographicPoints + bonusPoints) - usedPoints;
                    vodusV2Context.MasterMemberProfiles.Update(masterEntity);
                    await vodusV2Context.SaveChangesAsync();

                    newMemberProfile = memberEntity;

                    if (responseGroup.Count() > 1)
                    {
                        //  Set other 
                        responseGroup.Where(x => x.Id != item.Id);
                    }
                }
                else
                {
                    var memberListSorted = memberProfileList.OrderByDescending(x => x.AvailablePoints + x.DemographicPoints);

                    if (memberListSorted.Any())
                    {
                        var item = memberListSorted.First();
                        var memberEntity = await vodusV2Context.MemberProfiles.Where(x => x.Id == item.Id).FirstOrDefaultAsync();
                        memberEntity.IsMasterProfile = true;
                        vodusV2Context.Update(memberEntity);
                        await vodusV2Context.SaveChangesAsync();

                        newMemberProfile = memberEntity;

                        //  Update mastermember points
                        var masterEntity = await vodusV2Context.MasterMemberProfiles.Where(x => x.Id == memberMasterId).FirstOrDefaultAsync();
                        masterEntity.AvailablePoints = (memberEntity.AvailablePoints + memberEntity.DemographicPoints + bonusPoints) - usedPoints;
                        vodusV2Context.Update(masterEntity);
                        await vodusV2Context.SaveChangesAsync();

                        if (memberListSorted.Count() > 1)
                        {
                            memberListSorted.Where(x => x.Id != item.Id);
                        }
                    }
                }
                return newMemberProfile;
            }
        }
    }

}
