﻿using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class OrderItemsExternalStatusTypes
    {
        public short Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActivated { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
