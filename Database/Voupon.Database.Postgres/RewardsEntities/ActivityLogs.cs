using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class ActivityLogs
    {
        public Guid Id { get; set; }
        public string ActionName { get; set; } = null!;
        public string? ActionFunction { get; set; }
        public string ActionId { get; set; } = null!;
        public string? ActionData { get; set; }
        public string Message { get; set; } = null!;
        public bool IsSuccessful { get; set; }
        public string TriggerBy { get; set; } = null!;
        public string TriggerFor { get; set; } = null!;
        public string CreatedAt2 { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
