using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class GifteeTokens
    {
        public int Id { get; set; }
        public string VoucherName { get; set; } = null!;
        public string Token { get; set; } = null!;
        public DateTime IssuedDate { get; set; }
        public int? DigitalRedemptionId { get; set; }
    }
}
