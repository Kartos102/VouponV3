using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class MerchantCarousel
    {
        public MerchantCarousel()
        {
        }

        public long Id { get; set; }
        public int MerchantId { get; set; }
        public string ImageUrl { get; set; }
        public byte StatusId { get; set; }
        public int OrderNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }

        public virtual Merchants Merchant { get; set; }

    }
}
