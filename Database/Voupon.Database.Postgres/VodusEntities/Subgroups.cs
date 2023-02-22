using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class Subgroups
    {
        public Subgroups()
        {
            MemberProfileSubgroups = new HashSet<MemberProfileSubgroups>();
            DemographicValue = new HashSet<DemographicValues>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal R { get; set; }
        public int ASSR { get; set; }
        public int CR { get; set; }
        public int CASSR { get; set; }
        public short RankNumber { get; set; }
        public int? AgeId { get; set; }
        public int? EducationId { get; set; }
        public int? GenderId { get; set; }
        public int? OccupationId { get; set; }
        public int? EthnicityId { get; set; }
        public int? StateId { get; set; }
        public int? ReligionId { get; set; }
        public int? RuralUrbanId { get; set; }
        public int? MonthlyIncomeId { get; set; }
        public int? MaritalStatusId { get; set; }
        public int? MonthlyHouseHoldIncomeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public int AvailableMemberCount { get; set; }
        public bool RInprogress { get; set; }
        public DateTime? RLastUpdatedAt { get; set; }
        public decimal RecordValue { get; set; }

        public virtual ICollection<MemberProfileSubgroups> MemberProfileSubgroups { get; set; }

        public virtual ICollection<DemographicValues> DemographicValue { get; set; }
    }
}
