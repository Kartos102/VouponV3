using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class IPLookups
    {
        public Guid Id { get; set; }
        public long IPFrom { get; set; }
        public long IPTo { get; set; }
        public string CountryCode { get; set; } = null!;
        public string CountryName { get; set; } = null!;
        public string RegionName { get; set; } = null!;
        public string CityName { get; set; } = null!;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string ZipCode { get; set; } = null!;
        public string TimeZone { get; set; } = null!;
    }
}
