using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using Voupon.Common;
using Voupon.Database.MSSQL.RewardsEntities;
using Voupon.Database.MSSQL.VodusEntities;

namespace Voupon.ConsoleApp.Services
{
    public static class CreatePsyFromResponsesCommand
    {

        public static void Execute(VodusV2Context vodusV2Context, RewardsDBContext rewardsDBContext)
        {
            Console.WriteLine("Starting CreatePsyFromResponsesCommand.Execute");

            var totalItems = 0;
            var currentCount = 0;

            var surveyQuestionIdList = new List<int>();
            surveyQuestionIdList.Add(9182);
            surveyQuestionIdList.Add(9183);

            var responses = vodusV2Context.SurveyResponses.Include(x => x.SurveyResponseAnswers).Where(x => surveyQuestionIdList.Contains(x.SurveyQuestionId.Value)).ToList();
            totalItems = responses.Count;
            Console.WriteLine("Total items: " + totalItems);
            foreach (var item in responses)
            {
                currentCount++;
                Console.WriteLine("Current item: " + totalItems);


                var memberPsy = new MemberPsychographics();
                memberPsy.MemberProfileId = item.MemberProfileId;
                memberPsy.PsyQuestionId = (item.SurveyQuestionId == 9182 ? 1183 : 1184);
                memberPsy.CreatedAt = DateTime.Now;

                if (vodusV2Context.MemberPsychographics.Where(x => x.MemberProfileId == memberPsy.MemberProfileId && x.PsyQuestionId == memberPsy.PsyQuestionId).Any())
                {
                    continue;
                }

                foreach (var answers in item.SurveyResponseAnswers)
                {
                    var memberPsychographicAnswers = new MemberPsychographicAnswers();
                    memberPsychographicAnswers.IsActive = true;
                    var psyAnswerId = 0;
                    if (answers.SurveyQuestionAnswerId == 54572)
                    {
                        psyAnswerId = 2168;
                    }
                    else if (answers.SurveyQuestionAnswerId == 54573)
                    {
                        psyAnswerId = 2169;
                    }
                    else if (answers.SurveyQuestionAnswerId == 54574)
                    {
                        psyAnswerId = 2170;
                    }
                    else if (answers.SurveyQuestionAnswerId == 54575)
                    {
                        psyAnswerId = 2171;
                    }
                    else if (answers.SurveyQuestionAnswerId == 54576)
                    {
                        psyAnswerId = 2172;
                    }
                    else if (answers.SurveyQuestionAnswerId == 54577)
                    {
                        psyAnswerId = 2173;
                    }
                    else if (answers.SurveyQuestionAnswerId == 54578)
                    {
                        psyAnswerId = 2174;

                    }
                    else if (answers.SurveyQuestionAnswerId == 54579)
                    {
                        psyAnswerId = 2175;
                    }
                    memberPsychographicAnswers.AnswerId = psyAnswerId;
                    memberPsy.MemberPsychographicAnswers.Add(memberPsychographicAnswers);
                }
                vodusV2Context.MemberPsychographics.Add(memberPsy);
            }
            vodusV2Context.SaveChanges();


            Console.WriteLine($"===============================================================================");
            Console.WriteLine("");
            Console.WriteLine("Completed CreatePsyFromResponsesCommand.Execute");
            var a = 0;
            return;
        }
    }
}