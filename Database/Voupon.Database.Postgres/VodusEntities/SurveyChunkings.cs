using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class SurveyChunkings
    {
        public SurveyChunkings()
        {
            CommercialSubgroups = new HashSet<CommercialSubgroups>();
            SurveyQuestions = new HashSet<SurveyQuestions>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int CommercialId { get; set; }
        public bool? IsIntroChunk { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string LastUpdatedBy { get; set; } = null!;
        public DateTime LastUpdatedAt { get; set; }
        public string? JsonFileUrl { get; set; }
        public int? TotalRespondent { get; set; }
        public int? TotalPassFilterRespondent { get; set; }
        public decimal? TotalResponseCost { get; set; }

        public virtual Commercials Commercial { get; set; } = null!;
        public virtual ICollection<CommercialSubgroups> CommercialSubgroups { get; set; }
        public virtual ICollection<SurveyQuestions> SurveyQuestions { get; set; }
    }
}
