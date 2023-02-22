﻿using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class Temp_CommercialWeightedScoreDemographics
    {
        public int CommercialId { get; set; }
        public short SequenceNumber { get; set; }
        public short SubSequenceNumber { get; set; }
        public short DemographicTypeId { get; set; }
        public int? DemographicValueId { get; set; }
        public string? Value { get; set; }
        public string? Value2 { get; set; }
        public bool? Edited { get; set; }

        public virtual Commercials Commercial { get; set; } = null!;
        public virtual DemographicTypes DemographicType { get; set; } = null!;
        public virtual DemographicValues? DemographicValue { get; set; }
    }
}
