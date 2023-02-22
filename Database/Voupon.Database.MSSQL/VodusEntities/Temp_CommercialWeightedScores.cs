using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class Temp_CommercialWeightedScores
    {
        public int CommercialId { get; set; }
        public short SequenceNumber { get; set; }
        public short SubSequenceNumber { get; set; }
        public int? QuotaVolume { get; set; }
        public double? WeightedScore { get; set; }
        public bool? Edited { get; set; }
        public int? QuotaVolumeCompletedCount { get; set; }
        public bool? QuotaVolumeIsCompleted { get; set; }
        public int? QuotaMultiplierVolume { get; set; }
        public int? QuotaMultiplierCompletedCount { get; set; }
        public bool? QuotaMultiplierIsCompleted { get; set; }

        public virtual Commercials Commercial { get; set; }
    }
}
