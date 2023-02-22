using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class DemographicValues
    {
        public DemographicValues()
        {
            CommercialTargets = new HashSet<CommercialTargets>();
            MemberProfileExtensions = new HashSet<MemberProfileExtensions>();
            MemberProfileExtensions_Old = new HashSet<MemberProfileExtensions_Old>();
            PartnerWebsiteVisits = new HashSet<PartnerWebsiteVisits>();
            Temp_CommercialWeightedScoreDemographics = new HashSet<Temp_CommercialWeightedScoreDemographics>();
            Subgroup = new HashSet<Subgroups>();
            SubgroupMultiplier = new HashSet<SubgroupMultipliers>();
        }

        public int Id { get; set; }
        public short? DemographicTypeId { get; set; }
        public string? DisplayValue { get; set; }
        public short? Sequence { get; set; }
        public bool? IsActive { get; set; }
        public string? Value1 { get; set; }
        public string? Value2 { get; set; }
        public short? ParentId { get; set; }
        public decimal TargetRate { get; set; }
        public decimal TargetWeight { get; set; }
        public string? CountryCode { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public string? LastUpdatedBy { get; set; }

        public virtual DemographicTypes? DemographicType { get; set; }
        public virtual ICollection<CommercialTargets> CommercialTargets { get; set; }
        public virtual ICollection<MemberProfileExtensions> MemberProfileExtensions { get; set; }
        public virtual ICollection<MemberProfileExtensions_Old> MemberProfileExtensions_Old { get; set; }
        public virtual ICollection<PartnerWebsiteVisits> PartnerWebsiteVisits { get; set; }
        public virtual ICollection<Temp_CommercialWeightedScoreDemographics> Temp_CommercialWeightedScoreDemographics { get; set; }

        public virtual ICollection<Subgroups> Subgroup { get; set; }
        public virtual ICollection<SubgroupMultipliers> SubgroupMultiplier { get; set; }
    }
}
