using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class ChatGroup
    {
        public ChatGroup()
        {
            ChatGroupUsers = new HashSet<ChatGroupUsers>();
            ChatMessages = new HashSet<ChatMessages>();
        }

        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastUpdatedAt { get; set; }

        public virtual ICollection<ChatGroupUsers> ChatGroupUsers { get; set; }
        public virtual ICollection<ChatMessages> ChatMessages { get; set; }
    }
}
