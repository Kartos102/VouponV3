using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class PartnerProfiles
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int PartnerWebsiteId { get; set; }

        public virtual Users User { get; set; }
    }
}
