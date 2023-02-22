using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class PsyQuestionAnswers
    {
        public int Id { get; set; }
        public int PsyQuestionId { get; set; }
        public short SequenceNumber { get; set; }
        public string TemplateUrl { get; set; }
        public string AnswerValue { get; set; }
        public bool? IsActive { get; set; }
        public int QuotaCount { get; set; }

        public virtual PsyQuestions PsyQuestion { get; set; }
    }
}
