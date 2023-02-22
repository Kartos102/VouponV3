using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class FinanceSummary
    {
        public FinanceSummary()
        {
            MerchantFinance = new HashSet<MerchantFinance>();
        }

        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalMerchant { get; set; }
        public decimal TotalPayout { get; set; }
        public DateTime? PayoutDate { get; set; }

        public virtual ICollection<MerchantFinance> MerchantFinance { get; set; }
    }
}
