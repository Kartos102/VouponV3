using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class CommercialDFPrices
    {
        public CommercialDFPrices()
        {
            CommercialTargetCosts = new HashSet<CommercialTargetCosts>();
        }

        public short Id { get; set; }
        public short DemographicTypeId { get; set; }
        public short NumberOfTargetFrom { get; set; }
        public short NumberOfTargetTo { get; set; }
        public bool IsActive { get; set; }
        public decimal CostPrice { get; set; }

        public virtual ICollection<CommercialTargetCosts> CommercialTargetCosts { get; set; }
    }
}
