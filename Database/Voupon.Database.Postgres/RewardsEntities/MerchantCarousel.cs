using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class MerchantCarousel
    {
        public long Id { get; set; }
        public int MerchantId { get; set; }
        public string ImageUrl { get; set; } = null!;
        public short StatusId { get; set; }
        public int OrderNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }

        public virtual Merchants Merchant { get; set; } = null!;
    }
}
