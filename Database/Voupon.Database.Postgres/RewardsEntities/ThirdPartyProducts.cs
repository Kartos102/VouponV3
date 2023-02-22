using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class ThirdPartyProducts
    {
        public Guid Id { get; set; }
        public Guid ThirdPartyTypeId { get; set; }
        public string ExternalId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public short Status { get; set; }

        public virtual ThirdPartyTypes ThirdPartyType { get; set; } = null!;
    }
}
