using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class ActivityLogs
    {
        public Guid Id { get; set; }
        public string ActionName { get; set; }
        public string ActionFunction { get; set; }
        public string ActionId { get; set; }
        public string ActionData { get; set; }
        public string Message { get; set; }
        public bool IsSuccessful { get; set; }
        public string TriggerBy { get; set; }
        public string TriggerFor { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
