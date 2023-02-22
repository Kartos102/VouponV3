using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Token.Commands
{
    public class UpdateLanguageCommand : IRequest<ApiResponseViewModel>
    {
        public long MemberProfileId { get; set; }
        public string Language { get; set; }
        public class UpdateLanguageCommandHandler : IRequestHandler<UpdateLanguageCommand, ApiResponseViewModel>
        {
            private readonly VodusV2Context vodusV2Context;
            private readonly IOptions<AppSettings> appSettings;
            public UpdateLanguageCommandHandler(VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings)
            {
                this.vodusV2Context = vodusV2Context;
                this.appSettings = appSettings;
            }

            public async Task<ApiResponseViewModel> Handle(UpdateLanguageCommand request, CancellationToken cancellationToken)
            {

                var apiResponseViewModel = new ApiResponseViewModel();

                try
                {
                    var member = await vodusV2Context.MemberProfiles.Where(x => x.Id == request.MemberProfileId).FirstOrDefaultAsync();

                    if (member == null)
                    {
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                    if (member.IsMasterProfile.HasValue && member.MasterMemberProfileId.HasValue)
                    {
                        if (member.IsMasterProfile.Value)
                        {
                            var master = await vodusV2Context.MasterMemberProfiles.Where(x => x.Id == member.MasterMemberProfileId).FirstOrDefaultAsync();
                            if (master != null)
                            {
                                master.PreferLanguage = request.Language;
                                vodusV2Context.MasterMemberProfiles.Update(master);
                                await vodusV2Context.SaveChangesAsync();

                                apiResponseViewModel.Successful = true;
                                return apiResponseViewModel;
                            }
                        }

                    }
                    apiResponseViewModel.Successful = false;
                    return apiResponseViewModel;

                }
                catch (Exception ex)
                {
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
