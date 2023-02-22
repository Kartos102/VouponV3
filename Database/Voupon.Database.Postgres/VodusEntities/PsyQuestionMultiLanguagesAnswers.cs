using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class PsyQuestionMultiLanguagesAnswers
    {
        public int Id { get; set; }
        public int PsyQuestionId { get; set; }
        public short AnswerSequenceNumber { get; set; }
        public string AnswerValue { get; set; } = null!;
        public string LanguageCode { get; set; } = null!;
    }
}
