using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class PartnerWebsiteCommercialSubgroups
    {
        public int Id { get; set; }
        public int CommercialSubgroupId { get; set; }
        public int PartnerWebsiteId { get; set; }
        public decimal CostPerResponse { get; set; }
        public int ResponseCount { get; set; }
        public decimal TotalCost { get; set; }
        public int? AgeId { get; set; }
        public int? EducationId { get; set; }
        public int? GenderId { get; set; }
        public int? OccupationId { get; set; }
        public int? EthnicityId { get; set; }
        public int? StateId { get; set; }
        public int? ReligionId { get; set; }
        public int? MonthlyIncomeId { get; set; }
        public int? MaritalStatusId { get; set; }
        public int? RuralUrbanId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
    }
}
