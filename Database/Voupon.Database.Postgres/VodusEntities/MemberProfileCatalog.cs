using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class MemberProfileCatalog
    {
        public string Guid { get; set; } = null!;
        public int UserId { get; set; }
    }
}
