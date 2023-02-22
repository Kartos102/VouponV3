using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class PsyQuestions
    {
        public PsyQuestions()
        {
            CommercialPsy = new HashSet<CommercialPsy>();
            MemberPsychographics = new HashSet<MemberPsychographics>();
            PsyQuestionAnswers = new HashSet<PsyQuestionAnswers>();
        }

        public int Id { get; set; }
        public short QuestionTypeId { get; set; }
        public string Title { get; set; }
        public string Remark { get; set; }
        public string TemplateContentUrl { get; set; }
        public string QuestionTemplateContentUrl { get; set; }
        public string QuestionTemplateContentMobileUrl { get; set; }
        public byte Status { get; set; }
        public int Quota { get; set; }
        public int QuotaCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }

        public virtual ICollection<CommercialPsy> CommercialPsy { get; set; }
        public virtual ICollection<MemberPsychographics> MemberPsychographics { get; set; }
        public virtual ICollection<PsyQuestionAnswers> PsyQuestionAnswers { get; set; }
    }
}
