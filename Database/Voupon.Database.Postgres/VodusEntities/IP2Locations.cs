using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class IP2Locations
    {
        public long IPFrom { get; set; }
        public long IPTo { get; set; }
        public string CountryCode { get; set; } = null!;
        public string CountryName { get; set; } = null!;
    }
}
