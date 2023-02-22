using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class SubgroupsV2
    {
        public SubgroupsV2()
        {
            ProductAdSubgroups = new HashSet<ProductAdSubgroups>();
        }

        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal R { get; set; }
        public int ASSR { get; set; }
        public int CR { get; set; }
        public int CASSR { get; set; }
        public short RankNumber { get; set; }
        public int? AvailableMemberCount { get; set; }
        public int? UpdateProgress { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public bool RInprogress { get; set; }
        public DateTime? RLastUpdatedAt { get; set; }
        public decimal RecordValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? SubgroupMembers { get; set; }

        public virtual ICollection<ProductAdSubgroups> ProductAdSubgroups { get; set; }
    }
}
