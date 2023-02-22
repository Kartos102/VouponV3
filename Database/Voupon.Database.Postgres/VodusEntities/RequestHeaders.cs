using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class RequestHeaders
    {
        public Guid Id { get; set; }
        public short ExternalTypeId { get; set; }
        public string Name { get; set; } = null!;
        public short UseCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
    }
}
