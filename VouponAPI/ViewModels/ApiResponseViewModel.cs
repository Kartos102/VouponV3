using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Voupon.API.ViewModels
{
    public class ApiResponseViewModel
    {
        [JsonProperty("code")]
        public int Code { get; set; }
        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }

        public ApiResponseViewModel()
        {
            Code = -1;
        }

        public bool RequireLogin { get; set; } = false;
    }
}
