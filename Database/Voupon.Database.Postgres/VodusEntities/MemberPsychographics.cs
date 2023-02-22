﻿using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class MemberPsychographics
    {
        public MemberPsychographics()
        {
            MemberPsychographicAnswers = new HashSet<MemberPsychographicAnswers>();
        }

        public long Id { get; set; }
        public long MemberProfileId { get; set; }
        public int PsyQuestionId { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual PsyQuestions PsyQuestion { get; set; } = null!;
        public virtual ICollection<MemberPsychographicAnswers> MemberPsychographicAnswers { get; set; }
    }
}
