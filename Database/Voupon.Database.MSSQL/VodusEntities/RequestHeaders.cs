using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class RequestHeaders
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public byte ExternalTypeId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
