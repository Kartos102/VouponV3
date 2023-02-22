using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class Ip2Nations
    {
        public long Ip { get; set; }
        public string CountryCode { get; set; }

        public virtual Ip2nationCountries CountryCodeNavigation { get; set; }
    }
}
