using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class FinanceTransaction
    {
        public int Id { get; set; }
        public int MerchantFinanceId { get; set; }
        public int ProductId { get; set; }
        public Guid OrderItemId { get; set; }
        public string ProductTitle { get; set; } = null!;
        public decimal TotalPrice { get; set; }
        public decimal MerchantProfit { get; set; }
        public decimal AdminProfit { get; set; }
        public decimal DefaultCommission { get; set; }

        public virtual MerchantFinance MerchantFinance { get; set; } = null!;
        public virtual OrderItems OrderItem { get; set; } = null!;
        public virtual Products Product { get; set; } = null!;
    }
}
