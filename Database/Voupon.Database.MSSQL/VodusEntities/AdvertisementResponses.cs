using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class AdvertisementResponses
    {
        public int Id { get; set; }
        public int? CommercialId { get; set; }
        public int? MemberProfileId { get; set; }
        public bool? IsClick { get; set; }
        public string UserAgent { get; set; }
        public string TcpIpProtocol { get; set; }
        public string Host { get; set; }
        public string Origin { get; set; }
        public string Referer { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
