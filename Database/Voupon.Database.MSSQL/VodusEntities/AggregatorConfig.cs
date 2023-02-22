using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class AggregatorConfig
    {
        public int Id { get; set; }
        public byte ShopeeCrawlerStatus { get; set; }
        public string ShopeeCrawlerEndpoint { get; set; }
        public DateTime ShopeeCrawlerLastVerifiedAt { get; set; }
        public byte LazadaCrawlerStatus { get; set; }
        public string LazadaCrawlerEndpoint { get; set; }
        public DateTime LazadaCrawlerLastVerifiedAt { get; set; }
    }
}
