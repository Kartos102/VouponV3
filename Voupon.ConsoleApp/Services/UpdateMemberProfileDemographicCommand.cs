using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voupon.Database.MSSQL.VodusEntities;

namespace Voupon.ConsoleApp.Services
{
    public static class UpdateMemberProfileDemographicCommand
    {
        public static void Execute(VodusV2Context vodusV2Context)
        {
            Console.WriteLine("Starting UpdateMemberProfileDemographicCommand.Execute");
            var continueProcess = true;
            long lastId = 0;
            long totalUpdated = 0;
            var currentBatch = 0;

            var oldDemographicIdList = vodusV2Context.DemographicValues.AsNoTracking().Where(x => x.DemographicTypeId == 1 && x.IsActive == false).Select(x => x.Id).ToList();

            while (continueProcess)
            {
                Console.WriteLine("Getting MemberProfiles from Database...");
                vodusV2Context.Database.SetCommandTimeout(3000);
                var pendingToUpdateList = vodusV2Context.MemberProfiles.FromSqlRaw($"Select top 1000  Id,DemographicAgeId from MemberProfiles where Id > 0 and DemographicAgeId in ({string.Join(",", oldDemographicIdList.ToArray())}) order by id desc").AsNoTracking().Select(x => new MemberProfiles { Id = x.Id, DemographicAgeId = x.DemographicAgeId }).ToList();

                if (!pendingToUpdateList.Any())
                {
                    continueProcess = false;
                    return;
                }
                
                Console.WriteLine("Processing 1K list...");
                foreach (var member in pendingToUpdateList)
                {
                    lastId = member.Id;
                    var newDemographicAgeId = 0;

                    switch (member.DemographicAgeId)
                    {
                        case 2794:
                            newDemographicAgeId = 2832;
                            break;
                        case 2795:
                            newDemographicAgeId = 2833;
                            break;
                        case 2796:
                            newDemographicAgeId = 2833;
                            break;
                        case 2797:
                            newDemographicAgeId = 2834;
                            break;
                        case 2798:
                            newDemographicAgeId = 2834;
                            break;
                        case 2799:
                            newDemographicAgeId = 2835;
                            break;
                        case 2800:
                            newDemographicAgeId = 2835;
                            break;
                        case 2801:
                            newDemographicAgeId = 2836;
                            break;
                        case 2802:
                            newDemographicAgeId = 2836;
                            break;
                        case 2803:
                            newDemographicAgeId = 2837;
                            break;
                        case 2804:
                            newDemographicAgeId = 2838;
                            break;
                        case 2813:
                            newDemographicAgeId = 2838;
                            break;
                        case 2817:
                            newDemographicAgeId = 2832;
                            break;
                        case 2818:
                            newDemographicAgeId = 2832;
                            break;
                        case 2819:
                            newDemographicAgeId = 2833;
                            break;
                        case 2820:
                            newDemographicAgeId = 2834;
                            break;
                        case 2821:
                            newDemographicAgeId = 2835;
                            break;
                        case 2822:
                            newDemographicAgeId = 2836;
                            break;
                        case 2823:
                            newDemographicAgeId = 2837;
                            break;
                        case 2824:
                            newDemographicAgeId = 2838;
                            break;
                        default:
                            newDemographicAgeId = member.DemographicAgeId.Value;
                            Console.WriteLine("Skip mapping");
                            break;
                    }

                    if (member.DemographicAgeId == newDemographicAgeId)
                    {
                        continue;
                    }

                    vodusV2Context.Database.ExecuteSqlRaw($"Update MemberProfiles set DemographicAgeId={newDemographicAgeId} where Id={member.Id}");

                    //  Update extensions
                    vodusV2Context.Database.ExecuteSqlRaw($"Update MemberProfileExtensions set DemographicValueId={newDemographicAgeId} where MemberProfileId={member.Id} and DemographicValueId={member.DemographicAgeId}");
                    currentBatch++;
                    totalUpdated++;
                    if (currentBatch == 500)
                    {
                        vodusV2Context.SaveChanges();
                    }
                    Console.WriteLine($"Completed this batch: {currentBatch}/500. Total updated so far: {totalUpdated}");
                    if (currentBatch == 500)
                    {
                        currentBatch = 0;
                    }
                }
            }
            Console.WriteLine("Completed UpdateMemberProfileDemographicCommand.Execute");
            return;
        }

    }
}
