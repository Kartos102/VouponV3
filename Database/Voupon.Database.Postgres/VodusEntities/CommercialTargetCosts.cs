﻿using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class CommercialTargetCosts
    {
        public CommercialTargetCosts()
        {
            CommercialTargets = new HashSet<CommercialTargets>();
        }

        public int Id { get; set; }
        public short CommercialIdDFPriceId { get; set; }
        public short DemographicTypeId { get; set; }
        public short NumberOfTargetFrom { get; set; }
        public short NumberOfTargetTo { get; set; }
        public decimal CostPrice { get; set; }

        public virtual CommercialDFPrices CommercialIdDFPrice { get; set; } = null!;
        public virtual DemographicTypes DemographicType { get; set; } = null!;
        public virtual ICollection<CommercialTargets> CommercialTargets { get; set; }
    }
}
