using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class MerchantUserChat
    {
        public int Id { get; set; }
        public string MessageText { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public Guid MerchantUserId { get; set; }
        public Guid MemberUserId { get; set; }
    }
}
