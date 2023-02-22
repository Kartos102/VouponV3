using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class ProductReviewReply
    {
        public Guid Id { get; set; }
        public Guid ProductReviewId { get; set; }
        public string Comment { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string CreatedBy { get; set; } = null!;

        public virtual ProductReview ProductReview { get; set; } = null!;
    }
}
