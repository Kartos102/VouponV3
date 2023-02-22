using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class InvalidDomainRequests
    {
        public Guid Id { get; set; }
        public string Domain { get; set; } = null!;
        public int VisitCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
    }
}
