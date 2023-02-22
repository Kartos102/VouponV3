using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class Products
    {
        public Products()
        {
            OrderItems = new HashSet<OrderItems>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ProductSummary { get; set; }
        public byte ProductStatus { get; set; }
        public string ProductImageUrl { get; set; }
        public string ProductThumbnailImageUrl { get; set; }
        public byte ProductType { get; set; }
        public int PointsRequired { get; set; }
        public int AvailableQuantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LuckyDrawDate { get; set; }
        public int LuckyDrawTicketIssued { get; set; }
        public bool? IsActive { get; set; }
        public int? LuckyDrawWinningTicketId { get; set; }
        public int ThirdPartyProductId { get; set; }

        public virtual ICollection<OrderItems> OrderItems { get; set; }
    }
}
