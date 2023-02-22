using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class CommercialPsyQuotas
    {
        public int Id { get; set; }
        public int CommercialPsyId { get; set; }
        public int AnswerId { get; set; }
        public string AnswerText { get; set; } = null!;
        public bool IsRequired { get; set; }
        public int Quota { get; set; }
        public int QuotaRequired { get; set; }
        public int QuotaCompletedCount { get; set; }
        public bool IsCompleted { get; set; }
        public int Quota2 { get; set; }
        public int Quota2CompletedCount { get; set; }

        public virtual CommercialPsy CommercialPsy { get; set; } = null!;
    }
}
