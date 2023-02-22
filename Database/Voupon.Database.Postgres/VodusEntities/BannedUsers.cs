using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class BannedUsers
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string? Reason { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public DateTime? BanDate { get; set; }
        public int? VPointsGained { get; set; }
        public string? UserId { get; set; }
    }
}
