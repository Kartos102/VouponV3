using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class SubgroupDemographicValues
    {
        public int SubgroupId { get; set; }
        public int DemographicValueId { get; set; }

        public virtual DemographicValues DemographicValue { get; set; }
        public virtual Subgroups Subgroup { get; set; }
    }
}
