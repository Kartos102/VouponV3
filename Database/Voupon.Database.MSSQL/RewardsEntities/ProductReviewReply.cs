using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class ProductReviewReply
    {
        public Guid Id { get; set; }
        public Guid ProductReviewId { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string CreatedBy { get; set; }

        public virtual ProductReview ProductReview { get; set; }
    }
}
