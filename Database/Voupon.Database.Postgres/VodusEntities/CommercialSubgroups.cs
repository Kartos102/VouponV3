using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class CommercialSubgroups
    {
        public int Id { get; set; }
        public int CommercialId { get; set; }
        public string SubgroupName { get; set; } = null!;
        public int? AgeId { get; set; }
        public int? EducationId { get; set; }
        public int? GenderId { get; set; }
        public int? OccupationId { get; set; }
        public int? EthnicityId { get; set; }
        public int? StateId { get; set; }
        public int? ReligionId { get; set; }
        public int? MonthlyIncomeId { get; set; }
        public int? MaritalStatusId { get; set; }
        public int? QuotaVolume { get; set; }
        public int? QuotaVolumeCompletedCount { get; set; }
        public bool? QuotaVolumeIsCompleted { get; set; }
        public int? QuotaMultiplierVolume { get; set; }
        public int? QuotaMultiplierCompletedCount { get; set; }
        public bool? QuotaMultiplierIsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal MValue { get; set; }
        public int? RuralUrbanId { get; set; }
        public bool IsTemplate { get; set; }
        public int? ChunkId { get; set; }
        public int? MonthlyHouseHoldIncomeId { get; set; }

        public virtual SurveyChunkings? Chunk { get; set; }
        public virtual Commercials Commercial { get; set; } = null!;
    }
}
