using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voupon.Database.Postgres.VodusEntities;

namespace Voupon.Rewards.WebApp.Services.Home.Models
{
    public class ProductTestRegisterModel
    {
        public int MasterMemberProfileId { get; set; }
        public string MasterMemberProfileAddress { get; set; }
        public string MasterMemberProfileEmail { get; set; }
        public int UserId { get; set; }
        public int SurveyProjectId { get; set; }
        public string SurveyProjectName{ get; set; }
        public DateTime SurveyProjectStartDate { get; set; }
        public string Reward { get; set; }
        public bool IsParticipantEmail { get; set; }
        public bool SurveyProjectStatus{ get; set; }

    }
}
