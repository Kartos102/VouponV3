using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class ThirdPartyProducts
    {
        public Guid Id { get; set; }
        public Guid ThirdPartyTypeId { get; set; }
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public byte Status { get; set; }

        public virtual ThirdPartyTypes ThirdPartyType { get; set; }
    }
}
