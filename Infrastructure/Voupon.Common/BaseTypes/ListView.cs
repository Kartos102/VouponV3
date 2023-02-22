using System;
using System.Collections.Generic;
using System.Text;

namespace Voupon.Common.BaseTypes
{
    public class ListView<T>
    {
        public int PageCount { get; private set; }
        public long PageIndex { get; private set; }
        public long TotalCount { get; private set; }
        public string Error { get; private set; }

        public ListView(long registersCount, int pageSize, int pageIndex, string error = null)
        {
            TotalCount = registersCount;
            PageCount = (int)Math.Ceiling((decimal)registersCount / (pageSize == 0 ? 1 : pageSize));
            PageIndex = pageIndex;
            Error = (error != null ? error : null);
            Items = new List<T>();
        }

        public ICollection<T> Items { get; set; }
    }
}
