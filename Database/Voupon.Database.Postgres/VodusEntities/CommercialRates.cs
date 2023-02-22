using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class CommercialRates
    {
        public int Id { get; set; }
        public short CommercialRateTypeId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Amount { get; set; }
        public string CountryCode { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public string LastUpdatedBy { get; set; } = null!;

        public virtual Ip2nationCountries CountryCodeNavigation { get; set; } = null!;
    }
}
