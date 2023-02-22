using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class OrderShopsExternalStatusTypes
    {
        public short Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActivated { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
