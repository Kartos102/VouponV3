using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class AdditionalDiscounts
    {
        public int Id { get; set; }
        public byte StatusId { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public short Points { get; set; }
    }
}
