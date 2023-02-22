using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class CommercialLeads
    {
        public CommercialLeads()
        {
            CommercialLeadCustomFields = new HashSet<CommercialLeadCustomFields>();
        }

        public int Id { get; set; }
        public int CommercialId { get; set; }
        public string Email { get; set; } = null!;
        public string VerificationCode { get; set; } = null!;
        public string? RefererUrl { get; set; }
        public string? RefererHostName { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public bool IsCompleted { get; set; }
        public string? Remark { get; set; }

        public virtual Commercials Commercial { get; set; } = null!;
        public virtual ICollection<CommercialLeadCustomFields> CommercialLeadCustomFields { get; set; }
    }
}
