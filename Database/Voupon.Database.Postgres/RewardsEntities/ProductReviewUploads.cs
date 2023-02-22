using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class ProductReviewUploads
    {
        public Guid Id { get; set; }
        public Guid ProductReviewId { get; set; }
        public string MimeType { get; set; } = null!;
        public string FileUrl { get; set; } = null!;
    }
}
