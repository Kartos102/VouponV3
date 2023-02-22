using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class IP2Locations
    {
        public long IPFrom { get; set; }
        public long IPTo { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
    }
}
