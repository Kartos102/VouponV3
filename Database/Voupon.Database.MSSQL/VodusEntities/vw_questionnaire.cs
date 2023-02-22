using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class vw_questionnaire
    {
        public int Id { get; set; }
        public short QuestionTypeId { get; set; }
        public string QuestionTitle { get; set; }
        public string TemplateContent { get; set; }
        public string QuestionTemplateContent { get; set; }
        public string QuestionTemplateContentMobile { get; set; }
        public short TierNumber { get; set; }
        public int AnswerId { get; set; }
        public string AnswerValue { get; set; }
        public string Template { get; set; }
        public int? ToSurveyQuestionId { get; set; }
        public int CommercialId { get; set; }
    }
}
