using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class OrderShopExternal
    {
        public OrderShopExternal()
        {
            OrderItemExternal = new HashSet<OrderItemExternal>();
        }

        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string ExternalShopId { get; set; }
        public byte ExternalTypeId { get; set; }
        public string ExternalShopName { get; set; }
        public string ExternalShopUrl { get; set; }
        public string ExternalOrderId { get; set; }
        public decimal TotalPrice { get; set; }
        public int TotalPoints { get; set; }
        public int TotalItems { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public string LastUpdatedByUser { get; set; }
        public byte OrderShippingExternalStatus { get; set; }
        public decimal ShippingCost { get; set; }
        public string TrackingNo { get; set; }
        public string ShippingDetailsJson { get; set; }
        public string ShippingLatestStatus { get; set; }
        public string AdminAccountDetail { get; set; }
        public decimal ShippingCostSubTotal { get; set; }
        public decimal ShippingCostDiscount { get; set; }
        
        public string ShippingCourier { get; set; }

        public virtual Orders Order { get; set; }
        public virtual ICollection<OrderItemExternal> OrderItemExternal { get; set; }
    }
}
