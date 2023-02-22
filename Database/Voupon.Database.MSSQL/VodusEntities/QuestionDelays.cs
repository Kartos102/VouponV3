using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class QuestionDelays
    {
        public QuestionDelays()
        {
            SurveyQuestions = new HashSet<SurveyQuestions>();
        }

        public short Id { get; set; }
        public short DelayInSeconds { get; set; }
        public bool IsActive { get; set; }
        public decimal CostPrice { get; set; }
        public decimal Point { get; set; }

        public virtual ICollection<SurveyQuestions> SurveyQuestions { get; set; }
    }
}
