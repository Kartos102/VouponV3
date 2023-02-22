using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class CommercialTargets
    {
        public int Id { get; set; }
        public int? CommercialId { get; set; }
        public short? DemographicTypeId { get; set; }
        public string Value { get; set; }
        public string Value2 { get; set; }
        public int? CommercialTargetCostId { get; set; }
        public int? DemographicValueId { get; set; }
        public decimal? DemographicRates { get; set; }

        public virtual Commercials Commercial { get; set; }
        public virtual CommercialTargetCosts CommercialTargetCost { get; set; }
        public virtual DemographicTypes DemographicType { get; set; }
        public virtual DemographicValues DemographicValue { get; set; }
    }
}
