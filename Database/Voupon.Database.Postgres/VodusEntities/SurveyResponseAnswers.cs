using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class SurveyResponseAnswers
    {
        public int Id { get; set; }
        public int SurveyResponseId { get; set; }
        public int SurveyQuestionId { get; set; }
        public int SurveyQuestionAnswerId { get; set; }
        public int? BillingId { get; set; }
        public string? OtherAnswer { get; set; }
        public bool IsEndSurvey { get; set; }
        public short? Order { get; set; }
        public int? PipeSurveyQuestionAnswerId { get; set; }
        public bool IsAutoGenPiping { get; set; }

        public virtual SurveyQuestions SurveyQuestion { get; set; } = null!;
        public virtual SurveyQuestionAnswers SurveyQuestionAnswer { get; set; } = null!;
        public virtual SurveyResponses SurveyResponse { get; set; } = null!;
    }
}
