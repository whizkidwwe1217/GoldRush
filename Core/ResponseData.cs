using System.Collections.Generic;

namespace GoldRush.Core
{
    public class ResponseData : IResponseData<object>
    {
        public IEnumerable<object> data { get; set; }
        public int? total { get; set; }
    }
}