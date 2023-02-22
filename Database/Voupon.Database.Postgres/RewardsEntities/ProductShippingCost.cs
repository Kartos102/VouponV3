using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class ProductShippingCost
    {
        public ProductShippingCost()
        {
            ShippingCost = new HashSet<ShippingCost>();
        }

        public int Id { get; set; }
        public int ProductId { get; set; }
        public decimal ConditionalShippingCost { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUser { get; set; }
        public int ShippingTypeId { get; set; }

        public virtual Products Product { get; set; } = null!;
        public virtual ICollection<ShippingCost> ShippingCost { get; set; }
    }
}
