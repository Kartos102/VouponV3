using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class ProductDemographicsTarget
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int DemographicTypeId { get; set; }
        public int DemographicValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public bool IsActive { get; set; }

        public virtual Products Product { get; set; }
    }
}
