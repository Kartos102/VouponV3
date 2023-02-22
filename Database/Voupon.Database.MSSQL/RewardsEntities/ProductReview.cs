using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class ProductReview
    {
        public ProductReview()
        {
            ProductReviewReply = new HashSet<ProductReviewReply>();
            ProductReviewUploads = new HashSet<ProductReviewUploads>();
        }

        public Guid Id { get; set; }
        public int ProductId { get; set; }
        public int MerchantId { get; set; }
        public Guid OrderItemId { get; set; }
        public decimal Rating { get; set; }
        public string Comment { get; set; }
        public int MasterMemberProfileId { get; set; }
        public string MemberName { get; set; }
        public string ProductTitle { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Merchants Merchant { get; set; }
        public virtual OrderItems OrderItem { get; set; }
        public virtual Products Product { get; set; }
        public virtual ICollection<ProductReviewReply> ProductReviewReply { get; set; }
        public virtual ICollection<ProductReviewUploads> ProductReviewUploads { get; set; }
    }
}
