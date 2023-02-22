using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class ShippingCost
    {
        public int Id { get; set; }
        public decimal Cost { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUser { get; set; }
        public int ProductShippingId { get; set; }
        public int ProvinceId { get; set; }

        public virtual ProductShippingCost ProductShipping { get; set; } = null!;
        public virtual Provinces Province { get; set; } = null!;
    }
}
