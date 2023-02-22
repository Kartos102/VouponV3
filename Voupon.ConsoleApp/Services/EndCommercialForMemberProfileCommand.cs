using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Voupon.Database.MSSQL.RewardsEntities;
using Voupon.Database.MSSQL.VodusEntities;

namespace Voupon.ConsoleApp.Services
{
    public static class EndCommercialForMemberProfileCommand
    {
        public static void Execute(VodusV2Context vodusV2Context, RewardsDBContext rewardsDBContext)
        {
            Console.WriteLine("Starting EndCommercialForMemberProfileCommand.Execute");

            var COMMERCIAL_ID = 2177;
            var PSY_QUESTION_ID = 1223;
            var ANSWER_ID_TO_END = 2489;
            var memberPsy = vodusV2Context.MemberPsychographics.Include(x => x.MemberPsychographicAnswers).Where(x => x.PsyQuestionId == PSY_QUESTION_ID).ToList();

            foreach (var answer in memberPsy)
            {
                if (answer.MemberPsychographicAnswers.Where(x=> x.AnswerId == ANSWER_ID_TO_END).Any())
                {
                    var memberEnded = vodusV2Context.MemberProfileEndedCommercials.Where(x => x.MemberProfileId == answer.MemberProfileId && x.CommercialId == COMMERCIAL_ID).FirstOrDefault();

                    if (memberEnded == null)
                    {
                        var newEnded = new MemberProfileEndedCommercials
                        {
                            CommercialId = COMMERCIAL_ID,
                            MemberProfileId = answer.MemberProfileId,
                            EndedAt = DateTime.Now,
                            EndType = 5
                        };

                        vodusV2Context.MemberProfileEndedCommercials.Add(newEnded);
                        vodusV2Context.SaveChanges();
                    }
                    else
                    {
                        memberEnded.EndedAt = DateTime.Now;
                        memberEnded.EndType = 5;

                        vodusV2Context.MemberProfileEndedCommercials.Update(memberEnded);
                        vodusV2Context.SaveChanges();
                    }
                }
            }
            Console.WriteLine($"===============================================================================");
            Console.WriteLine("");
            Console.WriteLine("Completed EndCommercialForMemberProfileCommand.Execute");
            return;

            /*
            StreamReader reader = new StreamReader(File.OpenRead($"{AppDomain.CurrentDomain.BaseDirectory}/App_Data/1854.csv"));
            var currentCount = 0;
            var batchInsertCount = 0;
            var newList = new List<IPLookups>();

            while (!reader.EndOfStream)
            {
                currentCount++;
                string line = reader.ReadLine();
                if(currentCount == 1)
                {
                    continue;
                }
                var memberProfileId = long.Parse(line.Split(",")[0]);
                var existingOpeningChunk = vodusV2Context.MemberProfileChunks.Where(x => x.ChunkId == 2233 && x.MemberProfileId == memberProfileId).FirstOrDefault();
                if(existingOpeningChunk == null)
                {
                    var memberProfileEnded = new MemberProfileEndedCommercials
                    {
                        MemberProfileId = memberProfileId,
                        EndedAt = DateTime.Now,
                        EndType = 1,
                        CommercialId = 1854
                    };

                    vodusV2Context.MemberProfileEndedCommercials.AddAsync(memberProfileEnded);
                    vodusV2Context.SaveChanges();
                }
               
                Console.WriteLine($"Completed item no: {currentCount}");
            }

            */


        }

    }
}
