using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class CommercialRequests
    {
        public int Id { get; set; }
        public int CommercialId { get; set; }
        public long? MemberProfileId { get; set; }
        public int? PartnerWebsiteId { get; set; }
        public string UserAgent { get; set; }
        public string IpAddress { get; set; }
        public string Host { get; set; }
        public string Origin { get; set; }
        public string Referer { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
