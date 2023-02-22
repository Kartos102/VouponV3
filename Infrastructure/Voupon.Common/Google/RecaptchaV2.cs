using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Voupon.Common.Google
{
    public class ReCaptchaV2
    {
        public static bool Validate(string EncodedResponse, string secret)
        {
            using (WebClient wc = new WebClient())
            {
                var googleReply = wc.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secret, EncodedResponse));
                var captchaResponse = JsonSerializer.Deserialize<ReCaptchaV2>(googleReply);
                return captchaResponse.Success;
            }
        }

        private bool success;

        [JsonPropertyName("success")]
        public bool Success
        {
            get { return success; }
            set { success = value; }
        }


        private List<string> errorCodes;
        [JsonPropertyName("error-codes")]
        public List<string> ErrorCodes
        {
            get { return errorCodes; }
            set { errorCodes = value; }
        }


        
    }
}
