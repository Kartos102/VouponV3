using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class MemberProfileVisits
    {
        public long Id { get; set; }
        public long MemberProfileId { get; set; }
        public int PartnerWebsiteId { get; set; }
        public int VisitCount { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public int CommercialCloseCount { get; set; }
        public DateTime? CommercialCloseLastUpdatedAt { get; set; }
    }
}
