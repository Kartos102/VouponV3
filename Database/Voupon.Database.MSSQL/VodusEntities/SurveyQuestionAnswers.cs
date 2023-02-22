using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class SurveyQuestionAnswers
    {
        public SurveyQuestionAnswers()
        {
            SurveyLogicFlows = new HashSet<SurveyLogicFlows>();
            SurveyResponseAnswers = new HashSet<SurveyResponseAnswers>();
        }

        public int Id { get; set; }
        public int SurveyQuestionId { get; set; }
        public short AnswerSequenceNumber { get; set; }
        public string Template { get; set; }
        public string AnswerValue { get; set; }
        public bool IsActive { get; set; }
        public string RedirectUrl { get; set; }

        public virtual SurveyQuestions SurveyQuestion { get; set; }
        public virtual ICollection<SurveyLogicFlows> SurveyLogicFlows { get; set; }
        public virtual ICollection<SurveyResponseAnswers> SurveyResponseAnswers { get; set; }
    }
}
