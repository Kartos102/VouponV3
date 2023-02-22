using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class MemberProfileEndedCommercials
    {
        public long Id { get; set; }
        public long MemberProfileId { get; set; }
        public int CommercialId { get; set; }
        public DateTime EndedAt { get; set; }
        public short EndType { get; set; }

        public virtual Commercials Commercial { get; set; } = null!;
    }
}
