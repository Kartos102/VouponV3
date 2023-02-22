using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class AdvertisementResponseDemographic
    {
        public int Id { get; set; }
        public int? AdvertisementResponseId { get; set; }
        public short? DemographicTypeId { get; set; }
        public string? Value { get; set; }
    }
}
