using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class Advertisements
    {
        public int Id { get; set; }
        public int CommercialId { get; set; }
        public short QuestionDelayId { get; set; }
        public decimal Point { get; set; }
        public string Template { get; set; }
    }
}
