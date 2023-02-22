using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class MemberProfileChunks
    {
        public long Id { get; set; }
        public long MemberProfileId { get; set; }
        public int ChunkId { get; set; }
        public bool IsOpening { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedat { get; set; }
    }
}
