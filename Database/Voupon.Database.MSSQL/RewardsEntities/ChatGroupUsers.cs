using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class ChatGroupUsers
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public Guid ChatGroupId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool? IsActive { get; set; }
        public int UserTypeId { get; set; }
        public string UserName { get; set; }
        public string UserProfileImageUrl { get; set; }
        public int UnreadedMessagesCount { get; set; }
        public bool IsUserDeleted { get; set; }
        public bool IsMerchantDeleted { get; set; }

        public virtual ChatGroup ChatGroup { get; set; }
    }
}
