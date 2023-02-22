using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class Products
    {
        public Products()
        {
            OrderItems = new HashSet<OrderItems>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? ProductSummary { get; set; }
        public short ProductStatus { get; set; }
        public string ProductImageUrl { get; set; } = null!;
        public string ProductThumbnailImageUrl { get; set; } = null!;
        public short ProductType { get; set; }
        public int PointsRequired { get; set; }
        public int AvailableQuantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = null!;
        public DateTime? LuckyDrawDate { get; set; }
        public int LuckyDrawTicketIssued { get; set; }
        public bool IsActive { get; set; }
        public int? LuckyDrawWinningTicketId { get; set; }
        public int ThirdPartyProductId { get; set; }

        public virtual ICollection<OrderItems> OrderItems { get; set; }
    }
}
