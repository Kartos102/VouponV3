using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class Billings
    {
        public int Id { get; set; }
        public int CommercialId { get; set; }
        public string InvoiceNumber { get; set; }
        public decimal BillAmount { get; set; }
        public short PaymentStatusId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
