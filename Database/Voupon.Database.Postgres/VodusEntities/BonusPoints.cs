using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class BonusPoints
    {
        public int Id { get; set; }
        public int MasterMemberProfileId { get; set; }
        public int Points { get; set; }
        public string Remark { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = null!;
        public bool IsActive { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedAt { get; set; }

        public virtual MasterMemberProfiles MasterMemberProfile { get; set; } = null!;
    }
}
