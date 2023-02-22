using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class TempUsers
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string MobileNumber { get; set; } = null!;
        public string BusinessName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string TAC { get; set; } = null!;
        public DateTime TACRequestedAt { get; set; }
        public DateTime? TACVerifiedAt { get; set; }
        public Guid? UserId { get; set; }
        public int? CountryId { get; set; }
    }
}
