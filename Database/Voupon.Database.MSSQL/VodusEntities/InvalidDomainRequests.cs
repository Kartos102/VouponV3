using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class InvalidDomainRequests
    {
        public Guid Id { get; set; }
        public string Domain { get; set; }
        public int VisitCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
    }
}
