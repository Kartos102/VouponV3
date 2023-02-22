using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class SurveyTemplates
    {
        public SurveyTemplates()
        {
            SurveyQuestionTemplates = new HashSet<SurveyQuestionTemplates>();
        }

        public int Id { get; set; }
        public string TemplateName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }

        public virtual ICollection<SurveyQuestionTemplates> SurveyQuestionTemplates { get; set; }
    }
}
