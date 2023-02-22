using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class Ip2Nations
    {
        public long Ip { get; set; }
        public string CountryCode { get; set; } = null!;

        public virtual Ip2nationCountries CountryCodeNavigation { get; set; } = null!;
    }
}
