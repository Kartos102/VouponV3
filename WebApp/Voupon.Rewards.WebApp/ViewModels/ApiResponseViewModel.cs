using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Rewards.WebApp.ViewModels
{
    public class ApiResponseViewModel
    {
        private static readonly object EMPTY = new object();
        public bool Successful { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        public ApiResponseViewModel()
        {
            Successful = false;
            Message = null;
            Code = -1;
            Data = EMPTY;
        }
    }
}
