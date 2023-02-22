using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class FinanceSummary
    {
        public FinanceSummary()
        {
            MerchantFinance = new HashSet<MerchantFinance>();
        }

        public int Id { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int TotalMerchant { get; set; }
        public decimal TotalPayout { get; set; }
        public DateOnly? PayoutDate { get; set; }

        public virtual ICollection<MerchantFinance> MerchantFinance { get; set; }
    }
}
