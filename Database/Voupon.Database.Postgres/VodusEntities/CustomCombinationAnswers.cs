using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class CustomCombinationAnswers
    {
        public int Id { get; set; }
        public int CustomCombinationFlowId { get; set; }
        public int AnswerId { get; set; }

        public virtual CustomCombinationLogicFlows CustomCombinationFlow { get; set; } = null!;
    }
}
