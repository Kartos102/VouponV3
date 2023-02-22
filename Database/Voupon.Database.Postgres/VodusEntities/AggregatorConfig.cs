using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class AggregatorConfig
    {
        public int Id { get; set; }
        public short ShopeeCrawlerStatus { get; set; }
        public string ShopeeCrawlerEndpoint { get; set; } = null!;
        public DateTime ShopeeCrawlerLastVerifiedAt { get; set; }
        public short LazadaCrawlerStatus { get; set; }
        public string LazadaCrawlerEndpoint { get; set; } = null!;
        public DateTime LazadaCrawlerLastVerifiedAt { get; set; }
    }
}
