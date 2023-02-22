using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class ThirdPartyTypes
    {
        public ThirdPartyTypes()
        {
            ThirdPartyProducts = new HashSet<ThirdPartyProducts>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public byte Status { get; set; }

        public virtual ICollection<ThirdPartyProducts> ThirdPartyProducts { get; set; }
    }
}
