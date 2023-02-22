using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class CommercialWeightedScoreD
    {
        public int CommercialId { get; set; }
        public short SequenceNumber { get; set; }
        public short SubSequenceNumber { get; set; }
        public int CommercialTrgetId { get; set; }
    }
}
