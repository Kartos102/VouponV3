using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class CustomCombinationLogicFlows
    {
        public CustomCombinationLogicFlows()
        {
            CustomCombinationAnswers = new HashSet<CustomCombinationAnswers>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int QuestionId { get; set; }
        public int ToSurveyQuestionId { get; set; }
        public byte CombinationType { get; set; }

        public virtual ICollection<CustomCombinationAnswers> CustomCombinationAnswers { get; set; }
    }
}
