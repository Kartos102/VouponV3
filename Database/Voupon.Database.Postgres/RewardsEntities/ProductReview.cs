using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class ProductReview
    {
        public ProductReview()
        {
            ProductReviewReply = new HashSet<ProductReviewReply>();
        }

        public Guid Id { get; set; }
        public int ProductId { get; set; }
        public int MerchantId { get; set; }
        public Guid OrderItemId { get; set; }
        public decimal Rating { get; set; }
        public string Comment { get; set; } = null!;
        public int MasterMemberProfileId { get; set; }
        public string MemberName { get; set; } = null!;
        public string ProductTitle { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public virtual Merchants Merchant { get; set; } = null!;
        public virtual Products Product { get; set; } = null!;
        public virtual ICollection<ProductReviewReply> ProductReviewReply { get; set; }
    }
}
