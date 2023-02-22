using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class MemberProfileDailyAllowances
    {
        public long Id { get; set; }
        public int MemberProfileId { get; set; }
        public int PartnerWebsiteId { get; set; }
        public int VisitCount { get; set; }
        public DateOnly VisitDate { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public int DayOfTheYear { get; set; }
    }
}
