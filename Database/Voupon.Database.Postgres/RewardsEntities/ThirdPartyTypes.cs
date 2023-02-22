using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class ThirdPartyTypes
    {
        public ThirdPartyTypes()
        {
            ThirdPartyProducts = new HashSet<ThirdPartyProducts>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public short Status { get; set; }

        public virtual ICollection<ThirdPartyProducts> ThirdPartyProducts { get; set; }
    }
}
