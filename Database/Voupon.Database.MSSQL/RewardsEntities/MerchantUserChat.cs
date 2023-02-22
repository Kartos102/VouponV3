using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class MerchantUserChat
    {
        public int Id { get; set; }
        public string MessageText { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid MerchantUserId { get; set; }
        public Guid MemberUserId { get; set; }
    }
}
