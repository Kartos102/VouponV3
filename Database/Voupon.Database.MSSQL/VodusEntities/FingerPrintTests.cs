using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class FingerPrintTests
    {
        public int Id { get; set; }
        public string FingerPrintId { get; set; }
        public Guid DeviceId { get; set; }
        public long MemberProfileId { get; set; }
        public int PartnerWebsiteId { get; set; }
        public string Token { get; set; }
        public string ComponentsJSON { get; set; }
        public string UserAgent { get; set; }
        public string IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastCCAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public bool IsIncognito { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string CityName { get; set; }
        public string RegionName { get; set; }
        public string Postcode { get; set; }
    }
}
