using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voupon.Common;
using Voupon.Database.MSSQL.RewardsEntities;
using Voupon.Database.MSSQL.VodusEntities;

namespace Voupon.ConsoleApp.Services
{
    public static class CalculateFingerprintDuplicateCommand
    {

        private class FingerPrintCompare
        {
            public int Id { get; set; }
            public string FingerPrintId { get; set; }
            public int MemberProfileId { get; set; }
            public int PartnerWebsiteId { get; set; }
            public string Token { get; set; }
            public string ComponentsJSON { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? LastCCAt { get; set; }
            public DateTime? LastUpdatedAt { get; set; }
            public bool IsIncognito { get; set; }
        }

        public static void Execute(VodusV2Context vodusV2Context, RewardsDBContext rewardsDBContext)
        {
            Console.WriteLine("Starting CalculateFingerprintDuplicateCommand.Execute");
            var totalFiltered = 0;
            var totalProcessed = 0;

            var allFingerprintTest = vodusV2Context.FingerPrintTests.AsNoTracking().ToList();
            var total = allFingerprintTest.Count();
            var fpList = new List<FingerPrintCompare>();

            foreach (var item in allFingerprintTest)
            {
                totalProcessed++;
                var tokenObject = UserToken.FromTokenValue(item.Token);
                if (tokenObject == null)
                {
                    continue;
                }

                if (tokenObject.CreatedAt.Date < DateTime.Now.AddDays(-30))
                {
                    continue;
                }

                var fp = new FingerPrintCompare
                {
                    Id = item.Id,
                    FingerPrintId = item.FingerPrintId,
                    Token = item.Token
                };
                fpList.Add(fp);
                totalFiltered++;
                Console.WriteLine($"Row: {totalProcessed} / {total} , Pass: {totalFiltered} , Current Id: {item.Id} : {item.FingerPrintId}");
            }

            Console.WriteLine($"===============================================================================");
            Console.WriteLine($"Total items: {totalProcessed}");
            Console.WriteLine($"Total passed: {totalFiltered}");
            var result = fpList.GroupBy(x => x.FingerPrintId).Select(n => new
            {
                FingerPrint = n.Key,
                Count = n.Count()
            });

            var totalDuplicates = result.Where(x => x.Count > 1).Count();
            Console.WriteLine($"Total duplicates: {totalDuplicates}");

            Console.WriteLine($"Duplicate %: {(((decimal)totalDuplicates / totalFiltered) * 100).ToString("#.##")}%");
            Console.WriteLine("");
            Console.WriteLine("Completed CalculateFingerprintDuplicateCommand.Execute");
            return;
        }
    }
}