using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class CommercialLeadCustomFields
    {
        public int Id { get; set; }
        public int CommercialLeadId { get; set; }
        public string CustomField { get; set; } = null!;
        public string CustomValue { get; set; } = null!;

        public virtual CommercialLeads CommercialLead { get; set; } = null!;
    }
}
