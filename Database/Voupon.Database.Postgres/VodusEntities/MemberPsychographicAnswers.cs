using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class MemberPsychographicAnswers
    {
        public long Id { get; set; }
        public long MemberPsyId { get; set; }
        public int AnswerId { get; set; }
        public bool IsActive { get; set; }
        public int? PipeSurveyQuestionAnswerId { get; set; }
        public string? OtherAnswer { get; set; }

        public virtual MemberPsychographics MemberPsy { get; set; } = null!;
    }
}
