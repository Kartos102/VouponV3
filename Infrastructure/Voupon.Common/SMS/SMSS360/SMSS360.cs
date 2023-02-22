using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Voupon.Common.SMS.SMSS360
{
    public class SMSS360 : ISMSS360
    {
        private readonly string email;
        private readonly string apiKey;
        public SMSS360(string email, string apiKey)
        {
            this.email = email;
            this.apiKey = apiKey;
        }

        public string GetStatusCodeDescription(int code)
        {
            switch (code)
            {
                case 0: return "Completed successfully";
                case 1126: return "Your account is under the verifying progress. Please contact us for more information.";
                case 1204: return "The query string is empty.";
                case 1206: return "Please enter your email address";
                case 1207: return "Please enter your key";
                case 1208: return "Invalid key or email";
                case 1209: return "Please activate your account";
                case 1210: return "Your account has been suspended";
                case 1602: return "Invalid recipient";
                default:
                    // You can use the default case.
                    return "Invalid";
            }
        }

        public class Templates
        {
            public const string Template1 = "Vodus.com: Your verification code is {0}.";
        }


        public async Task<string> SendMessage(string recipient, string message, string customerReferenceId)
        {
            try
            {
                var client = new HttpClient();
                //https://www.smss360.com/api/sendsms.php?email=[EMAIL]&key=[KEY]&recipient=[RECIPIENT]&message=[MESSGE]&referenceID=[CUSTOMERREFERENCEID]
                var result = await client.GetAsync(string.Format("https://www.smss360.com/api/sendsms.php?email={0}&key={1}&recipient={2}&message={3}&referenceID={4}",
                    email, apiKey, recipient, message, customerReferenceId
                    ));

                var resultContent = await result.Content.ReadAsStreamAsync();

                var resultXml = XDocument.Load(resultContent);

                var statusCode = resultXml.Root.Element("statusCode").Value;
                //var statusMessage = resultXml.Root.Element("statusMsg").Value;
                //var referenceId = resultXml.Root.Element("sms").Element("items").Element("referenceId").Value;

                if (statusCode == "0")
                {
                    return statusCode;
                }

                return statusCode;
            }
            catch (Exception ex)
            {
                var error = ex.ToString();
                return "9999";
            }

        }

    }
}
