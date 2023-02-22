using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class SurveyQuestionTiers
    {
        public SurveyQuestionTiers()
        {
            SurveyQuestions = new HashSet<SurveyQuestions>();
        }

        public int Id { get; set; }
        public int CommercialId { get; set; }
        public short SequenceNumber { get; set; }
        public decimal CostPrice { get; set; }
        public short TierNumber { get; set; }
        public int ParentId { get; set; }
        public bool ToBeDeleted { get; set; }

        public virtual ICollection<SurveyQuestions> SurveyQuestions { get; set; }
    }
}
