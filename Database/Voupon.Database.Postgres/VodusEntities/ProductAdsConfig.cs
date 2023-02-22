using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class ProductAdsConfig
    {
        public int Id { get; set; }
        public int ImpressionCountIdentifier { get; set; }
        public int VPointsCap { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedBy { get; set; }
    }
}
