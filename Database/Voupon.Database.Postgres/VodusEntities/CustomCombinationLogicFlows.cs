using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class CustomCombinationLogicFlows
    {
        public CustomCombinationLogicFlows()
        {
            CustomCombinationAnswers = new HashSet<CustomCombinationAnswers>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int QuestionId { get; set; }
        public int ToSurveyQuestionId { get; set; }
        public short CombinationType { get; set; }

        public virtual ICollection<CustomCombinationAnswers> CustomCombinationAnswers { get; set; }
    }
}
