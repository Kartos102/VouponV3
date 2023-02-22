using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class DeletedSurveyResponseAnswers
    {
        public int Id { get; set; }
        public int SurveyResponseId { get; set; }
        public int SurveyQuestionId { get; set; }
        public int SurveyQuestionAnswerId { get; set; }
        public int? BillingId { get; set; }
        public string OtherAnswer { get; set; }
        public bool IsEndSurvey { get; set; }
        public short? Order { get; set; }
        public int? PipeSurveyQuestionAnswerId { get; set; }
        public bool IsAutoGenPiping { get; set; }
    }
}
