using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class MemberProfileProductAdImpressionExtensions
    {
        public int Id { get; set; }
        public int MemberProfileProductAdImpressionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int MediaPartnerId { get; set; }

        public virtual MemberProfileProductAdImpression MemberProfileProductAdImpression { get; set; }
    }
}
