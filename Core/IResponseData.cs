using System.Collections.Generic;

namespace GoldRush.Core
{
    public interface IResponseData<T>
    {
        IEnumerable<T> data { get; set; }
        int? total { get; set; }
    }
}