using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class PromoCodes
    {
        public PromoCodes()
        {
            Invoices = new HashSet<Invoices>();
        }

        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public short NumberOfUsePerClient { get; set; }
        public decimal DepositRequiredPercentage { get; set; }
        public decimal FinalPaymentDiscountPercentage { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = null!;
        public int TotalUseCount { get; set; }
        public int TotalUseAllowed { get; set; }

        public virtual ICollection<Invoices> Invoices { get; set; }
    }
}
