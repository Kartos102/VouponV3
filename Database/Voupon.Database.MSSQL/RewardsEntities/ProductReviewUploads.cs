using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class ProductReviewUploads
    {
        public Guid Id { get; set; }
        public Guid ProductReviewId { get; set; }
        public string MimeType { get; set; }
        public string FileUrl { get; set; }

        public virtual ProductReview ProductReview { get; set; }
    }
}
