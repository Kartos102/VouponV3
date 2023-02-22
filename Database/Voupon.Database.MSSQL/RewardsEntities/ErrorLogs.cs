using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class ErrorLogs
    {
        public int Id { get; set; }
        public byte TypeId { get; set; }
        public string ActionName { get; set; }
        public string ActionRequest { get; set; }
        public string Errors { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Token { get; set; }
        public int? MemberProfileId { get; set; }
        public int? MasterProfileId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
