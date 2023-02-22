using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Merchant.WebApp.Infrastructures.Helpers
{
    public class SignatureUtil
    {
        public static string GenerateCompactJson(Object data)
        {
            string dataStr = JsonConvert.SerializeObject(data);
            //sorting purpose
            JObject obj = JObject.Parse(dataStr);
            Sort(obj);
            string output = obj.ToString(Formatting.None);
            return output;
        }

        public static void Sort(JObject jObj)
        {
            var props = jObj.Properties().ToList();
            foreach (var prop in props)
            {
                prop.Remove();
            }

            foreach (var prop in props.OrderBy(p => p.Name))
            {
                jObj.Add(prop);
                if (prop.Value is JObject)
                    Sort((JObject)prop.Value);
            }
        }

    }
}
