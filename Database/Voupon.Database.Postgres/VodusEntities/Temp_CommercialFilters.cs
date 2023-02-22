using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class Temp_CommercialFilters
    {
        public int CommercialId { get; set; }
        public int SequenceNumber { get; set; }
        public short? DemographicTypeId { get; set; }
        public short QuotaType { get; set; }
        public bool? Edited { get; set; }

        public virtual Commercials Commercial { get; set; } = null!;
        public virtual DemographicTypes? DemographicType { get; set; }
    }
}
