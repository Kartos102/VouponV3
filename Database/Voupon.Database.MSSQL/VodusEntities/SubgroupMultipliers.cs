using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class SubgroupMultipliers
    {
        public SubgroupMultipliers()
        {
            SubgroupMultiplierDemographics = new HashSet<SubgroupMultiplierDemographics>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int R { get; set; }
        public int ASSR { get; set; }
        public int CR { get; set; }
        public int CASSR { get; set; }
        public byte Rank { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }

        public virtual ICollection<SubgroupMultiplierDemographics> SubgroupMultiplierDemographics { get; set; }
    }
}
