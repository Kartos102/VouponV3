using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class CommercialLeadCustomFields
    {
        public int Id { get; set; }
        public int CommercialLeadId { get; set; }
        public string CustomField { get; set; }
        public string CustomValue { get; set; }

        public virtual CommercialLeads CommercialLead { get; set; }
    }
}
