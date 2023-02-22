using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class AppConfig
    {
        public int Id { get; set; }
        public decimal RinggitPerVpoints { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedBy { get; set; }
        public bool? IsCheckoutEnabled { get; set; }
        public bool IsPassPaymentGatewayEnabled { get; set; }
        public bool IsErrorLogEmailEnabled { get; set; }
        public bool IsVPointsMultiplierEnabled { get; set; }
        public decimal VPointsMultiplier { get; set; }
        public decimal VPointsMultiplierCap { get; set; }
        public int MaxQuantityPerVPounts { get; set; }
        public decimal DefaultCommission { get; set; }
        public short MaxOrderFilter { get; set; }
        public bool IsAggregatorEnabled { get; set; }
        public int AggregatorSleepMiliseconds { get; set; }
        public DateTime AggregatorErrorLastEmailedAt { get; set; }
    }
}
