using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class OrderRedemptions
    {
        public int Id { get; set; }
        public int OrderItemId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsWinner { get; set; }
        public string UniqueCode { get; set; }

        public virtual OrderItems OrderItem { get; set; }
    }
}
