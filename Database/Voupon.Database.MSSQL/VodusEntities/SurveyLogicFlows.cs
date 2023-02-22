using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class SurveyLogicFlows
    {
        public int SurveyLogicFlowId { get; set; }
        public int SurveyQuestionAnswerId { get; set; }
        public int ToSurveyQuestionId { get; set; }

        public virtual SurveyQuestionAnswers SurveyQuestionAnswer { get; set; }
        public virtual SurveyQuestions ToSurveyQuestion { get; set; }
    }
}
