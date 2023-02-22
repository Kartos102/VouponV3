using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class MailingLists
    {
        public string Email { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public bool IsSubscribe { get; set; }
        public DateTime? LastEmailedAt { get; set; }
        public DateTime? UnSubscribeAt { get; set; }
    }
}
