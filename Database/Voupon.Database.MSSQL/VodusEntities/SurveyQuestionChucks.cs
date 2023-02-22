using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class SurveyQuestionChucks
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public byte ChunkNumber { get; set; }
        public bool? IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string LastUpdatedby { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
    }
}
