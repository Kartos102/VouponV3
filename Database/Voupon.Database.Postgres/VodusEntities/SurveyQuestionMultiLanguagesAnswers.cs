using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class SurveyQuestionMultiLanguagesAnswers
    {
        public int Id { get; set; }
        public int SurveyQuestionId { get; set; }
        public short AnswerSequenceNumber { get; set; }
        public string AnswerValue { get; set; } = null!;
        public string LanguageCode { get; set; } = null!;

        public virtual SurveyQuestions SurveyQuestion { get; set; } = null!;
    }
}
