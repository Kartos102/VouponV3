using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class CommercialPsy
    {
        public CommercialPsy()
        {
            CommercialPsyQuotas = new HashSet<CommercialPsyQuotas>();
        }

        public int Id { get; set; }
        public int CommercialId { get; set; }
        public int PsyQuestionId { get; set; }
        public bool IsActive { get; set; }
        public byte QuotaType { get; set; }
        public int NoneQuota1 { get; set; }
        public int NoneQuota1CompletedCount { get; set; }
        public int NoneQuota2 { get; set; }
        public int NoneQuota2CompletedCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public int Order { get; set; }
        public bool IsContain { get; set; }
        public bool IsAllRequired { get; set; }

        public virtual Commercials Commercial { get; set; }
        public virtual PsyQuestions PsyQuestion { get; set; }
        public virtual ICollection<CommercialPsyQuotas> CommercialPsyQuotas { get; set; }
    }
}
