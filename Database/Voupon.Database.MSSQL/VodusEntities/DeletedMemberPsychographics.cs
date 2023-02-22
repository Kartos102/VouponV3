using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class DeletedMemberPsychographics
    {
        public long Id { get; set; }
        public long MemberProfileId { get; set; }
        public int PsyQuestionId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
