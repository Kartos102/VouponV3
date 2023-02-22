using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class HUTSurveyProjects
    {
        public HUTSurveyProjects()
        {
            HUTSurveyForms = new HashSet<HUTSurveyForms>();
            HUTSurveyParticipants = new HashSet<HUTSurveyParticipants>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int MaxResponse { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public string ExternalName { get; set; }
        public string IntroMessage { get; set; }
        public string NextStepMessage { get; set; }
        public string ThankYouMessage { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string VPointsReward { get; set; }
        public bool IsDeleted { get; set; }
        public bool? IsRandomized { get; set; }
        public int LastFormId { get; set; }
        public int TestLength { get; set; }
        public int LanguageId { get; set; }

        public virtual Languages Language { get; set; }
        public virtual ICollection<HUTSurveyForms> HUTSurveyForms { get; set; }
        public virtual ICollection<HUTSurveyParticipants> HUTSurveyParticipants { get; set; }
    }
}
