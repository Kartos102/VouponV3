using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class PaymentStatuses
    {
        public short Id { get; set; }
        public string? Description { get; set; }
    }
}
