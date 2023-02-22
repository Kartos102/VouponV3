using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Services.Orders.Queries
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SyncMasterMemberProfilepointsQuery : IRequest<ApiResponseViewModel>
    {
        public int MasterMemberProfileId { get; set; }
        public class SyncMasterMemberProfilepointsQueryHandler : IRequestHandler<SyncMasterMemberProfilepointsQuery, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            VodusV2Context vodusV2Context;
            public SyncMasterMemberProfilepointsQueryHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.vodusV2Context = vodusV2Context;
            }

            public async Task<ApiResponseViewModel> Handle(SyncMasterMemberProfilepointsQuery request, CancellationToken cancellationToken)
            {
                PartnerWebsites partnerWebsite = null;
                //var loginResponseViewModel = new LoginResponseViewModel();
                MemberProfiles memberProfileObject = null;
                var masterMemberProfiles = new MasterMemberProfiles();
                var apiResponseViewModel = new ApiResponseViewModel
                {
                    Successful = true
                };
                try
                {
                    masterMemberProfiles = await vodusV2Context.MasterMemberProfiles.AsNoTracking().Where(x => x.Id == request.MasterMemberProfileId).FirstOrDefaultAsync();
                    var memberProfileList = new List<MemberProfiles>();
                    memberProfileList = await vodusV2Context.MemberProfiles.AsNoTracking().Where(x => x.MasterMemberProfileId == request.MasterMemberProfileId).Select(x => new MemberProfiles
                    {
                        Id = x.Id,
                        AvailablePoints = x.AvailablePoints,
                        DemographicPoints = x.DemographicPoints
                    }).ToListAsync();


                    if (memberProfileList == null || memberProfileList.Count == 0)
                    {
                        var memberProfileDTO = new MemberProfiles()
                        {
                            Guid = Guid.NewGuid().ToString(),
                            CreatedAt = DateTime.Now,
                            CreatedAtCountryCode = "my",
                            IsMasterProfile = true
                        };

                        vodusV2Context.MemberProfiles.Add(memberProfileDTO);
                        await vodusV2Context.SaveChangesAsync();

                        memberProfileList.Add(memberProfileDTO);

                    }
                    var idList = memberProfileList.Select(x => x.Id).ToList();


                    var model = new TotalUsedPointsByMemberQueryModel();

                    var responses = await vodusV2Context.SurveyResponses.Where(x => idList.Contains(x.MemberProfileId)).ToListAsync();
                    var responseGroup = responses.GroupBy(x => x.MemberProfileId).Select(group =>
                                       new
                                       {
                                           Id = group.Key,
                                           Count = group.Count(),
                                           Points = group.Sum(x => x.PointsCollected)
                                       }).OrderByDescending(x => x.Points).ToList();

                    var orders = await vodusV2Context.Orders.Where(x => x.MasterMemberId == request.MasterMemberProfileId).ToListAsync();
                    var ordersFromRewards = await rewardsDBContext.Orders.Where(x => x.OrderStatus == 2 && x.MasterMemberProfileId == request.MasterMemberProfileId).ToListAsync();
                    var usedPoints = 0;
                    var bonusPoints = 0;
                    var refundedPoints = 0;
                    var psychographicpoints = 0;
                    var deletedResponsesPoints = 0;
                    var deletedPsychographicpoints = 0;

                    var refunds = await rewardsDBContext.Refunds.Where(x => x.Status == 2 && x.MasterMemberProfileId == request.MasterMemberProfileId).ToListAsync();
                    if (refunds != null && refunds.Any())
                    {
                        refundedPoints = refunds.Sum(x => x.PointsRefunded);
                    }

                    var bonusPointsList = await vodusV2Context.BonusPoints.Where(x => x.MasterMemberProfileId == request.MasterMemberProfileId && x.IsActive == true).ToListAsync();

                    if (bonusPointsList.Any())
                    {
                        bonusPoints = bonusPointsList.Sum(x => x.Points);
                    }

                    if (orders != null && orders.Any())
                    {
                        usedPoints = orders.Sum(x => x.TotalPoints);
                    }

                    if (ordersFromRewards != null && ordersFromRewards.Any())
                    {
                        usedPoints += ordersFromRewards.Sum(x => x.TotalPoints);
                    }

                    psychographicpoints = await vodusV2Context.MemberPsychographics.AsNoTracking().Where(x => x.MemberProfileId == masterMemberProfiles.MemberProfileId).CountAsync();
                    deletedPsychographicpoints = await vodusV2Context.DeletedMemberPsychographics.AsNoTracking().Where(x => x.MemberProfileId == masterMemberProfiles.MemberProfileId).CountAsync();

                    deletedResponsesPoints = await vodusV2Context.DeletedSurveyResponses.AsNoTracking().Where(x => x.MemberProfileId == masterMemberProfiles.MemberProfileId).SumAsync(x => x.PointsCollected);

                    if (responseGroup != null && responseGroup.Any())
                    {
                        var item = responseGroup.First();

                        var memberEntity = await vodusV2Context.MemberProfiles.Where(x => x.Id == item.Id).Select(x => new MemberProfiles
                        {
                            Id = x.Id,
                            AvailablePoints = item.Points,
                            DemographicPoints = x.DemographicPoints
                        }).FirstOrDefaultAsync();

                        var availablePoints = (memberEntity.AvailablePoints + memberEntity.DemographicPoints + bonusPoints + refundedPoints + psychographicpoints + deletedResponsesPoints + deletedPsychographicpoints) - usedPoints;

                        await vodusV2Context.Database.ExecuteSqlRawAsync(string.Format("Update dbo.\"MemberProfiles\" set \"IsMasterProfile\"=true,\"MasterMemberProfileId\"={1},\"AvailablePoints\"={2} where \"Id\"={0}", item.Id, request.MasterMemberProfileId, memberEntity.AvailablePoints));
                        await vodusV2Context.Database.ExecuteSqlRawAsync(string.Format("Update dbo.\"MasterMemberProfiles\" set \"AvailablePoints\"={0}, \"MemberProfileId\"={2} where \"Id\"={1}", availablePoints, request.MasterMemberProfileId, item.Id));

                        memberProfileObject = memberEntity;
                        //loginResponseViewModel.Points = availablePoints;
                        apiResponseViewModel.Data = model;
                        return apiResponseViewModel;
                    }
                    else
                    {
                        var memberListSorted = memberProfileList.OrderByDescending(x => x.AvailablePoints + x.DemographicPoints);
                        if (memberListSorted != null && memberListSorted.Any())
                        {
                            var item = memberListSorted.First();

                            var memberEntity = await vodusV2Context.MemberProfiles.Where(x => x.Id == item.Id).Select(x => new MemberProfiles
                            {
                                Id = x.Id,
                                AvailablePoints = item.AvailablePoints,
                                DemographicPoints = x.DemographicPoints
                            }).FirstOrDefaultAsync();

                            var availablePoints = (memberEntity.AvailablePoints + memberEntity.DemographicPoints + bonusPoints + refundedPoints + psychographicpoints + deletedResponsesPoints) - usedPoints;

                            await vodusV2Context.Database.ExecuteSqlRawAsync(string.Format("Update dbo.\"MemberProfiles\" set \"IsMasterProfile\"=true,\"MasterMemberProfileId\"={1} where \"Id\"={0}", item.Id, masterMemberProfiles.Id));
                            await vodusV2Context.Database.ExecuteSqlRawAsync(string.Format("Update dbo.\"MasterMemberProfiles\" set \"AvailablePoints\"={0}, \"MemberProfileId\"={2} where \"Id\"={1}", availablePoints, masterMemberProfiles.Id, item.Id));

                            //loginResponseViewModel.Points = availablePoints;
                            memberProfileObject = memberEntity;
                        }
                        else
                        {
                            memberProfileObject = memberProfileList.First();
                        }

                    }

                    foreach (var member in memberProfileList)
                    {
                        if (member.Id == memberProfileObject.Id)
                        {
                            continue;
                        }
                        vodusV2Context.Database.ExecuteSqlRaw(string.Format("Update dbo.\"MemberProfiles\" set \"SyncMemberProfileId\"={0} where \"Id\"={1}", memberProfileObject.Id, member.Id));
                    }
                    await vodusV2Context.SaveChangesAsync();

                    return apiResponseViewModel;
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = ex.Message;

                    return apiResponseViewModel;
                }
            }
            public class TotalUsedPointsByMemberQueryModel
            {
                public int TotalPointsUsed { get; set; }
                public int TotalPointsOnHold { get; set; }
            }
        }

    }
}
