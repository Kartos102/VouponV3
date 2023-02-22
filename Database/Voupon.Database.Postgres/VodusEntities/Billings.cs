using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class Billings
    {
        public int Id { get; set; }
        public int CommercialId { get; set; }
        public string InvoiceNumber { get; set; } = null!;
        public decimal BillAmount { get; set; }
        public short PaymentStatusId { get; set; }
        public string CreatedBy { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
