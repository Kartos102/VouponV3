using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class AdditionalDiscounts
    {
        public int Id { get; set; }
        public short StatusId { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public short Points { get; set; }
    }
}
