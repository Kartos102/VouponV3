using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voupon.Database.MSSQL.RewardsEntities;
using Voupon.Database.MSSQL.VodusEntities;

namespace Voupon.ConsoleApp.Services
{
    public static class UpdateMasterMemberProfileCommand
    {
        public static void Execute(VodusV2Context vodusV2Context, RewardsDBContext rewardsDBContext)
        {
            Console.WriteLine("Starting UpdateMasterMemberProfileCommand.Execute");
            var continueProcess = true;
            var totalUpdated = 0;

            //var allMaster = vodusV2Context.MasterMemberProfiles.AsNoTracking().ToList();
            var totalMaster = vodusV2Context.MasterMemberProfiles.Where(x => x.MemberProfileId.HasValue == false).Count();
            while (continueProcess)
            {
                Console.WriteLine("Getting MemberProfiles from Database...");
                var allMaster = vodusV2Context.MasterMemberProfiles.AsNoTracking().Where(x => x.MemberProfileId.HasValue == false).Take(50).ToList();
                //vodusV2Context.Database.SetCommandTimeout(30000);

                foreach (var master in allMaster)
                {
                    long newMasterMemberProfileId = 0;
                    //var memberProfiles = vodusV2Context.MemberProfiles.FromSqlRaw($"Select Id From MemberProfiles where MasterMemberProfileId= {master.Id}").AsNoTracking().Select(x => new MemberProfiles { Id = x.Id }).ToList();
                    var memberProfileList = vodusV2Context.MemberProfiles.AsNoTracking().Where(x => x.MasterMemberProfileId == master.Id).Select(x => new MemberProfiles
                    {
                        Id = x.Id,
                        AvailablePoints = x.AvailablePoints,
                        DemographicPoints = x.DemographicPoints,
                        IsMasterProfile = x.IsMasterProfile
                    }).ToList();

                    if (memberProfileList == null || !memberProfileList.Any())
                    {
                        continue;
                    }

                    var idList = memberProfileList.Where(x => x.IsMasterProfile == true).Select(x => x.Id).ToList();
                    var responses = vodusV2Context.SurveyResponses.Where(x => idList.Contains(x.MemberProfileId)).ToList();
                    var responseGroup = responses.GroupBy(x => x.MemberProfileId).Select(group =>
                                       new
                                       {
                                           Id = group.Key,
                                           Count = group.Count(),
                                           Points = group.Sum(x => x.PointsCollected)
                                       }).OrderByDescending(x => x.Points).ToList();

                    var orders = vodusV2Context.Orders.Where(x => x.MasterMemberId == master.Id).ToList();
                    var ordersFromRewards = rewardsDBContext.Orders.Where(x => x.MasterMemberProfileId == master.Id && x.OrderStatus == 2).ToList();
                    var usedPoints = 0;
                    var bonusPoints = 0;

                    var bonusPointsList = vodusV2Context.BonusPoints.Where(x => x.MasterMemberProfileId == master.Id && x.IsActive == true).ToList();

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

                    if (responseGroup.Any())
                    {
                        var item = responseGroup.First();
                        var memberEntity = vodusV2Context.MemberProfiles.Where(x => x.Id == item.Id).Select(x => new MemberProfiles
                        {
                            Id = x.Id,
                            AvailablePoints = x.AvailablePoints,
                            DemographicPoints = x.DemographicPoints
                        }).FirstOrDefault();

                        var availablePoints = (memberEntity.AvailablePoints + memberEntity.DemographicPoints + bonusPoints) - usedPoints;

                        vodusV2Context.Database.ExecuteSqlRaw(string.Format("Update MemberProfiles set IsMasterProfile=1,MasterMemberProfileId={1} where Id={0}", item.Id, master.Id));
                        vodusV2Context.Database.ExecuteSqlRaw(string.Format("Update MasterMemberProfiles set AvailablePoints={0}, MemberProfileid={2} where Id={1}", availablePoints, master.Id, item.Id));
                        newMasterMemberProfileId = item.Id;
                    }
                    else
                    {
                        var memberListSorted = memberProfileList.OrderByDescending(x => x.AvailablePoints + x.DemographicPoints);

                        var item = memberListSorted.First();

                        var memberEntity = vodusV2Context.MemberProfiles.Where(x => x.Id == item.Id).Select(x => new MemberProfiles
                        {
                            Id = x.Id,
                            AvailablePoints = x.AvailablePoints,
                            DemographicPoints = x.DemographicPoints
                        }).FirstOrDefault();

                        var availablePoints = (memberEntity.AvailablePoints + memberEntity.DemographicPoints + bonusPoints) - usedPoints;

                        vodusV2Context.Database.ExecuteSqlRaw(string.Format("Update MemberProfiles set IsMasterProfile=1,MasterMemberProfileId={1} where Id={0}", item.Id, master.Id));
                        vodusV2Context.Database.ExecuteSqlRaw(string.Format("Update MasterMemberProfiles set AvailablePoints={0}, MemberProfileId={2} where Id={1}", availablePoints, master.Id, item.Id));
                        newMasterMemberProfileId = item.Id;
                    }
                    vodusV2Context.SaveChanges();
                    //  Update others to non member
                    foreach (var member in memberProfileList)
                    {
                        if (member.Id == newMasterMemberProfileId)
                        {
                            continue;
                        }
                        vodusV2Context.Database.ExecuteSqlRaw(string.Format("Update MemberProfiles set IsMasterProfile=0,MasterMemberProfileId=null where Id={0}", member.Id));
                    }
                    vodusV2Context.SaveChanges();

                    totalUpdated++;
                    Console.WriteLine($"Total updated: {totalUpdated} / {totalMaster} , Current MasterMemberProfileId: {master.Id}");
                }

                if (allMaster.Count() < 50)
                {
                    continueProcess = false;
                }
            }
            Console.WriteLine("Completed UpdateMasterMemberProfileCommand.Execute");
            return;
        }

    }
}
