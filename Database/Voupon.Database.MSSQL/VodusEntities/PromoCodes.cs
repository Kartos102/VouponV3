using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class PromoCodes
    {
        public PromoCodes()
        {
            Invoices = new HashSet<Invoices>();
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public byte NumberOfUsePerClient { get; set; }
        public decimal DepositRequiredPercentage { get; set; }
        public decimal FinalPaymentDiscountPercentage { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public int TotalUseCount { get; set; }
        public int TotalUseAllowed { get; set; }

        public virtual ICollection<Invoices> Invoices { get; set; }
    }
}
