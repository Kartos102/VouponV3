using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Voupon.API.ViewModels
{
    public class LegacyApiResponseViewModel
    {
        private static readonly object EMPTY = new object();

        [JsonProperty("successful")]
        public bool Successful { get; set; }

        [JsonProperty("code")]
        public int Code { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("data")]
        public object Data { get; set; }

        public LegacyApiResponseViewModel()
        {
            Successful = false;
            Message = null;
            Code = -1;
            Data = EMPTY;
        }
    }
}
