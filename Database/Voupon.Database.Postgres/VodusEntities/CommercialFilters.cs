using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class CommercialFilters
    {
        public int CommercialId { get; set; }
        public short SequenceNumber { get; set; }
        public short? DemographicTypeId { get; set; }
        public string? QuotaType { get; set; }

        public virtual Commercials Commercial { get; set; } = null!;
        public virtual DemographicTypes? DemographicType { get; set; }
    }
}
