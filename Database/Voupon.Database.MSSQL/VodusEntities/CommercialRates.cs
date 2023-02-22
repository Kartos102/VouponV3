using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class CommercialRates
    {
        public int Id { get; set; }
        public byte CommercialRateTypeId { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public string CountryCode { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public string LastUpdatedBy { get; set; }

        public virtual Ip2nationCountries CountryCodeNavigation { get; set; }
    }
}
