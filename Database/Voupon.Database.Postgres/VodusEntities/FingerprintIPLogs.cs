using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class FingerprintIPLogs
    {
        public long Id { get; set; }
        public Guid FingerprintId { get; set; }
        public string VisitorId { get; set; } = null!;
        public Guid DeviceId { get; set; }
        public string IpAddress { get; set; } = null!;
        public string CountryCode { get; set; } = null!;
        public string CountryName { get; set; } = null!;
        public string RegionName { get; set; } = null!;
        public string CityName { get; set; } = null!;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string ZipCode { get; set; } = null!;
        public string? UserAgent { get; set; }
        public short Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastCCAt { get; set; }
    }
}
