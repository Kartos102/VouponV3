using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
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
        public string ProductName { get; set; }
        public string ProductTypeName { get; set; }
        public short Quantity { get; set; }
        public int SubTotalPoints { get; set; }
        public int TotalPoints { get; set; }
        public string ThirdPartyRedemptionUrl { get; set; }
        public string ThirdPartyResponse { get; set; }

        public virtual Orders Order { get; set; }
        public virtual Products Product { get; set; }
        public virtual ICollection<OrderItemRedemptions> OrderItemRedemptions { get; set; }
        public virtual ICollection<OrderRedemptions> OrderRedemptions { get; set; }
    }
}
