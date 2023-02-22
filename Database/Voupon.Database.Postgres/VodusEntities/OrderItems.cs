using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class OrderItems
    {
        public OrderItems()
        {
            OrderItemRedemptions = new HashSet<OrderItemRedemptions>();
            OrderRedemptions = new HashSet<OrderRedemptions>();
        }

        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string ProductTypeName { get; set; } = null!;
        public short Quantity { get; set; }
        public int SubTotalPoints { get; set; }
        public int TotalPoints { get; set; }
        public string? ThirdPartyRedemptionUrl { get; set; }
        public string? ThirdPartyResponse { get; set; }

        public virtual Orders Order { get; set; } = null!;
        public virtual Products Product { get; set; } = null!;
        public virtual ICollection<OrderItemRedemptions> OrderItemRedemptions { get; set; }
        public virtual ICollection<OrderRedemptions> OrderRedemptions { get; set; }
    }
}
