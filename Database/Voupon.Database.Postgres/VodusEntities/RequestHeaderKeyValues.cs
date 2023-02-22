using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class RequestHeaderKeyValues
    {
        public Guid Id { get; set; }
        public Guid RequestHeaderId { get; set; }
        public string Name { get; set; } = null!;
        public string Value { get; set; } = null!;
    }
}
