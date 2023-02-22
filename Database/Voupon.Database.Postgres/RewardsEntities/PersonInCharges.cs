using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class PersonInCharges
    {
        public int Id { get; set; }
        public int MerchantId { get; set; }
        public string? Name { get; set; }
        public string? Contact { get; set; }
        public string? IdentityNumber { get; set; }
        public string? Position { get; set; }
        public int StatusTypeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUser { get; set; }
        public string? Remarks { get; set; }
        public string? PendingChanges { get; set; }
        public string? DocumentUrl { get; set; }

        public virtual Merchants Merchant { get; set; } = null!;
        public virtual StatusTypes StatusType { get; set; } = null!;
    }
}
