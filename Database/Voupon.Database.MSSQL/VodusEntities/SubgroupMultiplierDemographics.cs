using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class SubgroupMultiplierDemographics
    {
        public int SubgroupMultiplierId { get; set; }
        public int DemographicValueId { get; set; }

        public virtual DemographicValues DemographicValue { get; set; }
        public virtual SubgroupMultipliers SubgroupMultiplier { get; set; }
    }
}
