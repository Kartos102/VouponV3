using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class GifteeTokens
    {
        public int Id { get; set; }
        public string VoucherName { get; set; }
        public string Token { get; set; }
        public DateTime IssuedDate { get; set; }
        public int? DigitalRedemptionId { get; set; }
    }
}
