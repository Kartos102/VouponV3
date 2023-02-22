using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class ClientSurveyQuestionTemplates
    {
        public int Id { get; set; }
        public int ClientSurveyTemplateId { get; set; }
        public short QuestionTypeId { get; set; }
        public string TemplateContent { get; set; } = null!;
        public string QuestionTemplateContent { get; set; } = null!;

        public virtual ClientSurveyTemplates ClientSurveyTemplate { get; set; } = null!;
        public virtual QuestionTypes QuestionType { get; set; } = null!;
    }
}
