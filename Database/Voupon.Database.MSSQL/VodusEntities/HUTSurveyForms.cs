using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class HUTSurveyForms
    {
        public HUTSurveyForms()
        {
            HUTSurveyResponses = new HashSet<HUTSurveyResponses>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public int HUTSurveyProjectId { get; set; }

        public virtual HUTSurveyProjects HUTSurveyProject { get; set; }
        public virtual ICollection<HUTSurveyResponses> HUTSurveyResponses { get; set; }
    }
}
