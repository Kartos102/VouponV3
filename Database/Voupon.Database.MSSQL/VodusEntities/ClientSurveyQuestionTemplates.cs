using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class ClientSurveyQuestionTemplates
    {
        public int Id { get; set; }
        public int ClientSurveyTemplateId { get; set; }
        public short QuestionTypeId { get; set; }
        public string TemplateContent { get; set; }
        public string QuestionTemplateContent { get; set; }

        public virtual ClientSurveyTemplates ClientSurveyTemplate { get; set; }
        public virtual QuestionTypes QuestionType { get; set; }
    }
}
