using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class ProductCarousel
    {
        public ProductCarousel()
        {
        }

        public long Id { get; set; }
        public int ProductId { get; set; }
        public string ImageUrl { get; set; }
        public byte StatusId { get; set; }
        public int OrderNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; } 

    }
}
