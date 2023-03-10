using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class MerchantFinance
    {
        public MerchantFinance()
        {
            FinanceTransaction = new HashSet<FinanceTransaction>();
        }

        public int Id { get; set; }
        public int MerchantId { get; set; }
        public string MerchantDisplayName { get; set; } = null!;
        public string? Bank { get; set; }
        public string? BankAccount { get; set; }
        public int FinanceSummaryId { get; set; }
        public int TotalTransaction { get; set; }
        public decimal TotalPayout { get; set; }
        public DateOnly? PayoutDate { get; set; }
        public string? Remarks { get; set; }
        public bool IsPaid { get; set; }
        public string? StatementOfAccountUrl { get; set; }

        public virtual FinanceSummary FinanceSummary { get; set; } = null!;
        public virtual Merchants Merchant { get; set; } = null!;
        public virtual ICollection<FinanceTransaction> FinanceTransaction { get; set; }
    }
}
