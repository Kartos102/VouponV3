using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class SurveyQuestionMultiLanguagesTitles
    {
        public int Id { get; set; }
        public int SurveyQuestionId { get; set; }
        public string TitleText { get; set; } = null!;
        public string LanguageCode { get; set; } = null!;
        public bool IsGridRowTitle { get; set; }
        public short TitleSequenceNumber { get; set; }
        public bool IsTextBox { get; set; }

        public virtual SurveyQuestions SurveyQuestion { get; set; } = null!;
    }
}
