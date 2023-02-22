using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class FinanceTransaction
    {
        public int Id { get; set; }
        public int MerchantFinanceId { get; set; }
        public int ProductId { get; set; }
        public Guid OrderItemId { get; set; }
        public string ProductTitle { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal MerchantProfit { get; set; }
        public decimal AdminProfit { get; set; }
        public decimal DefaultCommission { get; set; }

        public virtual MerchantFinance MerchantFinance { get; set; }
        public virtual OrderItems OrderItem { get; set; }
        public virtual Products Product { get; set; }
    }
}
