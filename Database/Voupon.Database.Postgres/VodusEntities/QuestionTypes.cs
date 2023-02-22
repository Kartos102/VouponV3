using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class QuestionTypes
    {
        public QuestionTypes()
        {
            ClientSurveyQuestionTemplates = new HashSet<ClientSurveyQuestionTemplates>();
            SurveyQuestionTemplates = new HashSet<SurveyQuestionTemplates>();
            SurveyQuestions = new HashSet<SurveyQuestions>();
        }

        public short Id { get; set; }
        public string Type { get; set; } = null!;
        public string Description { get; set; } = null!;
        public short MaxLength { get; set; }
        public bool IsActive { get; set; }
        public decimal CostPrice { get; set; }
        public decimal Point { get; set; }

        public virtual ICollection<ClientSurveyQuestionTemplates> ClientSurveyQuestionTemplates { get; set; }
        public virtual ICollection<SurveyQuestionTemplates> SurveyQuestionTemplates { get; set; }
        public virtual ICollection<SurveyQuestions> SurveyQuestions { get; set; }
    }
}
