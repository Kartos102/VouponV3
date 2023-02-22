using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class Temp_CommercialFilters
    {
        public int CommercialId { get; set; }
        public int SequenceNumber { get; set; }
        public short? DemographicTypeId { get; set; }
        public byte QuotaType { get; set; }
        public bool? Edited { get; set; }

        public virtual Commercials Commercial { get; set; }
        public virtual DemographicTypes DemographicType { get; set; }
    }
}
