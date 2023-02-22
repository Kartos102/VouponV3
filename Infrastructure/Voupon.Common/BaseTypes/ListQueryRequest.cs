using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Voupon.Common.BaseTypes
{
    public abstract class ListQueryRequest<T> : IRequest<T>
    {
        public ListQueryRequest()
        {
            PageIndex = 1;
            PageSize = 20;
        }

        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}
