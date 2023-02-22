using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class SurveyQuestionTemplates
    {
        public int SurveyTemplateId { get; set; }
        public short QuestionTypeId { get; set; }
        public string? TemplateContent { get; set; }
        public string? QuestionTemplateContent { get; set; }

        public virtual QuestionTypes QuestionType { get; set; } = null!;
        public virtual SurveyTemplates SurveyTemplate { get; set; } = null!;
    }
}
