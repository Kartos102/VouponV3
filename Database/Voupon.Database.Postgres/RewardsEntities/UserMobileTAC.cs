using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class UserMobileTAC
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string MobileNumber { get; set; } = null!;
        public string TAC { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiredAt { get; set; }
        public bool IsUsedToVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }
}
