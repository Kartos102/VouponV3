using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class MemberProfileProductAdImpression
    {
        public MemberProfileProductAdImpression()
        {
            MemberProfileProductAdImpressionExtensions = new HashSet<MemberProfileProductAdImpressionExtensions>();
        }

        public int Id { get; set; }
        public long? MemberProfileId { get; set; }
        public int ProductId { get; set; }
        public int AdImpressionCount { get; set; }
        public int AdClickCount { get; set; }
        public decimal? CTR { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<MemberProfileProductAdImpressionExtensions> MemberProfileProductAdImpressionExtensions { get; set; }
    }
}
