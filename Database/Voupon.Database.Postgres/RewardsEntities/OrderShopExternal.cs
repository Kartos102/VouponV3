using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class OrderShopExternal
    {
        public OrderShopExternal()
        {
            OrderItemExternal = new HashSet<OrderItemExternal>();
        }

        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string ExternalShopId { get; set; } = null!;
        public short ExternalTypeId { get; set; }
        public string ExternalShopName { get; set; } = null!;
        public string ExternalShopUrl { get; set; } = null!;
        public string? ExternalOrderId { get; set; }
        public decimal TotalPrice { get; set; }
        public int TotalPoints { get; set; }
        public int TotalItems { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public string? LastUpdatedByUser { get; set; }
        public short OrderShippingExternalStatus { get; set; }
        public decimal ShippingCost { get; set; }
        public string? TrackingNo { get; set; }
        public string? ShippingDetailsJson { get; set; }
        public string? ShippingLatestStatus { get; set; }
        public string? AdminAccountDetail { get; set; }
        public decimal ShippingCostSubTotal { get; set; }
        public decimal ShippingCostDiscount { get; set; }
        
        public string ShippingCourier { get; set; }

        public virtual Orders Order { get; set; } = null!;
        public virtual ICollection<OrderItemExternal> OrderItemExternal { get; set; }
    }
}
