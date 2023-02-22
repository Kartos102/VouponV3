using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class InvoiceItems
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string ItemDescription { get; set; }
        public byte RateType { get; set; }
        public decimal RateAmount { get; set; }

        public virtual Invoices Invoice { get; set; }
    }
}
