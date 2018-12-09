using System.Buffers;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GoldRush.Core
{
    public class CamelCaseJsonFormatter : JsonOutputFormatter
    {
        public CamelCaseJsonFormatter() : base(
            new JsonSerializerSettings { 
                ContractResolver = new CamelCasePropertyNamesContractResolver(),

            }, ArrayPool<char>.Shared)
        {
            SupportedMediaTypes.Clear();
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json;profile=\"https://en.wikipedia.org/wiki/Camel_case\""));
        }
    }
}