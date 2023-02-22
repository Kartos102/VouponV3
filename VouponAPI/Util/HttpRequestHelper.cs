using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Voupon.API.Util
{
    public static class HttpRequestHelper
    {
        public static T DeserializeModel<T>(this HttpRequest request)
        {
            using (var reader = new StreamReader(request.Body))
            using (var textReader = new JsonTextReader(reader))
            {
                request.Body.Seek(0, SeekOrigin.Begin);
                var serializer = JsonSerializer.Create(new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                return serializer.Deserialize<T>(textReader);
            }
        }
    }
}
