using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class ChatMessages
    {
        public long Id { get; set; }
        public string CreatedByUserId { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public long? ParentMessageid { get; set; }
        public bool IsCardMessage { get; set; }
        public bool IsFileAttached { get; set; }
        public string? AttachmentsUrl { get; set; }
        public Guid ToGroupId { get; set; }
        public bool IsReaded { get; set; }
        public bool IsUserDeleted { get; set; }
        public bool IsMerchantDeleted { get; set; }

        public virtual ChatGroup ToGroup { get; set; } = null!;
    }
}
