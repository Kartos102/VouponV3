﻿using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class ConsoleProductJSON
    {
        public Guid Id { get; set; }
        public string URL { get; set; }
        public string PageUrl { get; set; }
        public string ItemName { get; set; }
        public string ExternalMerchantId { get; set; }
        public byte ExternalTypeId { get; set; }
        public string ExternalId { get; set; }
        public byte StatusId { get; set; }
        public string JSON { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
