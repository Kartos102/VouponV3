using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class CommercialWeightedScores
    {
        public int CommercialId { get; set; }
        public short SequenceNumber { get; set; }
        public short SubSequenceNumber { get; set; }
        public double WeightedScore { get; set; }
        public int? QuotaVolume { get; set; }

        public virtual Commercials Commercial { get; set; }
    }
}
