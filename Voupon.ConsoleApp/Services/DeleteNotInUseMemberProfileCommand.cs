using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voupon.Database.MSSQL.VodusEntities;

namespace Voupon.ConsoleApp.Services
{
    public static class DeleteNotInUseMemberProfileCommand
    {
        public async static Task<bool> Execute(VodusV2Context vodusV2Context, string sortType, int hours, int duration)
        {
            Console.WriteLine($"Starting DeleteNotInUseMemberProfileCommand.Execute sort: {sortType} hour: {hours} until: {duration}");
            var continueProcess = true;
            const int SLEEP_TIME_IN_MILISECONDS = 600000;

            var oldDemographicIdList = await vodusV2Context.DemographicValues.AsNoTracking().Where(x => x.DemographicTypeId == 1 && x.IsActive == false).Select(x => x.Id).ToListAsync();
            int rowsToDelete = 100;
            long totalDeleteRows = 0;
            var allowedHours = new List<int>();

            if (hours == 0 && duration == 24)
            {
                for (var i = 0; i <= 24; i++)
                {
                    allowedHours.Add(i);
                }
            }
            else
            {
                allowedHours.Add(hours);
                while (duration > 1)
                {
                    hours = hours + 1;
                    allowedHours.Add(hours);
                    duration--;
                }
            }
            /*
            Console.WriteLine($"{DateTime.Now} - start getting rows");
            var memberProfileExtensions = await vodusV2Context.MemberProfileExtensions.Select(x => x.MemberProfileId).ToListAsync();
            var memberProfileExtensionsArray = string.Join(",", memberProfileExtensions);

            var memberResponses = await vodusV2Context.SurveyResponses.Select(x => x.MemberProfileId).ToListAsync();
            var memberResponsesArray = string.Join(",", memberResponses);
            memberProfileExtensions.AddRange(memberResponses);

            var memberPsy = await vodusV2Context.MemberPsychographics.Select(x => x.MemberProfileId).ToListAsync();
            var memberPsyArray = string.Join(",", memberPsy);
            memberProfileExtensions.AddRange(memberPsy);

            var memberMaster = await vodusV2Context.MasterMemberProfiles.Where(x=>x.MemberProfileId != null).Select(x => x.MemberProfileId.Value).ToListAsync();
            var memberMasterArray = string.Join(",", memberMaster);
            memberProfileExtensions.AddRange(memberMaster);
         

            var mem = string.Join(",", memberProfileExtensions.Distinct());
            Console.WriteLine($"{DateTime.Now} - end getting rows");
            Console.WriteLine($"Total rows {mem.LongCount()}");
               */

            long idFrom = 0;
            /*
             *   var user = await vodusV2Context.Users.Where(x => x.Email == "kok.hong@vodus.my").FirstOrDefaultAsync();
            idFrom = Convert.ToInt64(user.FirstName);
           

            var pendingToUpdateList = await vodusV2Context.MemberProfiles.FromSqlRaw($"select top 1 id from MemberProfiles m where id not in (SELECT MemberProfileId FROM MemberProfileExtensions) and id not in (select MemberProfileId from SurveyResponses) and id not in (select MemberProfileId from MemberPsychographics ) order by id asc").Select(x => new Member
            {
                Id = x.Id
            }).ToListAsync();

            if (!pendingToUpdateList.Any())
            {
                Console.WriteLine($"{DateTime.Now} - No more dormant profiles to be delete");
                continueProcess = false;
            }
             */
            var user = await vodusV2Context.Users.Where(x => x.Email == "kok.hong@vodus.my").FirstOrDefaultAsync();
            var arrow = ">";
            if(sortType == "asc")
            {
                idFrom = Convert.ToInt64(user.FirstName);
            }
            else
            {
                arrow = "<";
                idFrom = Convert.ToInt64(user.LastName);
            }
            while (continueProcess)
            {
                if (!allowedHours.Contains(DateTime.Now.Hour))
                {
                    Console.WriteLine($"{DateTime.Now} - Only allowed within allowed hours {string.Join(",", allowedHours.ToArray())}. Checking again in 10 mins");
                    System.Threading.Thread.Sleep(SLEEP_TIME_IN_MILISECONDS);
                    continue;
                }
                Console.WriteLine($"{DateTime.Now} - Start checking");
                vodusV2Context.Database.SetCommandTimeout(30000);

                var members = await vodusV2Context.MemberProfiles.FromSqlRaw($"select top {rowsToDelete} id,lastrespondedat from memberprofiles where id {arrow} {idFrom} order by id {sortType}").Select(x => new Member
                {
                    Id = x.Id,
                    LastRespondedAt = x.LastRespondedAt
                }).ToListAsync();

                foreach (var member in members)
                {
                    idFrom = member.Id;
                    Console.WriteLine($"{DateTime.Now} - Checking:  {member.Id}");
                    var ext = vodusV2Context.MemberProfileExtensions.Where(x => x.MemberProfileId == member.Id).Count();
                    var resp = vodusV2Context.SurveyResponses.Where(x => x.MemberProfileId == member.Id).Count();
                    var psy = vodusV2Context.MemberPsychographics.Where(x => x.MemberProfileId == member.Id).Count();
                    var master = vodusV2Context.MasterMemberProfiles.Where(x => x.MemberProfileId == member.Id).Count();
                    if(ext <= 1 && (resp + psy + master) == 0)
                    {
                        var toBeDeleted = true;
                        if(member.LastRespondedAt.HasValue)
                        {
                            if((DateTime.Now - member.LastRespondedAt.Value).Days < 30)
                            {
                                toBeDeleted = false;
                            }
                        }

                        if (toBeDeleted)
                        {
                            vodusV2Context.Database.ExecuteSqlRaw($"delete from memberprofiles where id = {member.Id}");
                            vodusV2Context.Database.ExecuteSqlRaw($"update users set firstname='{idFrom}' where email='kok.hong@vodus.my'");
                            Console.WriteLine($"{DateTime.Now} - Deleted:  {member.Id}");
                        }
                        totalDeleteRows++;
                    }
                   
                }
                vodusV2Context.Database.ExecuteSqlRaw($"update users set firstname='{idFrom}' where email='kok.hong@vodus.my'");

                //Console.WriteLine($"{DateTime.Now} - Deleting next {rowsToDelete} rows {sortType.ToUpper()}");

                //vodusV2Context.Database.ExecuteSqlRaw($"delete from memberprofiles where id not in (select top {rowsToDelete} id from memberprofiles where id not in({mem}) order by id {sortType})");


                //vodusV2Context.Database.ExecuteSqlRaw($"delete from memberprofiles where MasterMemberProfileid is null and id in (select top {rowsToDelete} Id from MemberProfiles m where id not in (SELECT MemberProfileId FROM MemberProfileExtensions) and id not in (select MemberProfileId from SurveyResponses) and id not in (select MemberProfileId from MemberPsychographics ) order by id {sortType})");
                //Console.WriteLine($"{DateTime.Now} - Done deleting {rowsToDelete} rows");
                //totalDeleteRows += rowsToDelete;
                Console.WriteLine($"{DateTime.Now} - Total rows deleted so far:  {totalDeleteRows}");

            }
            Console.WriteLine("Completed DeleteNotInUseMemberProfileCommand.Execute");
            return true;
        }

        public class Member
        {
            public long Id { get; set; }
            public DateTime? LastRespondedAt { get; set; }
        }

    }
}
