using System;
using System.Collections.Generic;

namespace GoldRush.Core
{
    public class SearchResponseData : IResponseData<object>
    {
        public virtual IEnumerable<object> data { get; set; }
        public int? total { get; set; }
        public int? pageSize { get; set; }
        public int? pageCount
        {
            get
            {
                return (int)Math.Ceiling((double)total / (int)pageSize);
            }
        }
        public int? currentPage { get; set; }

        public SearchResponseData()
        {
            data = null;
            total = 0;
            currentPage = 1;
        }
    }

    public class SearchResponseData<T> : IResponseData<T>
    {
        public virtual IEnumerable<T> data { get; set; }
        public int? total { get; set; }
        public int? pageSize { get; set; }
        public int? pageCount
        {
            get
            {
                return (int)Math.Ceiling((double)total / (int)pageSize);
            }
        }

        public int? currentPage { get; set; }

        public SearchResponseData()
        {
            data = null;
            total = 0;
            currentPage = 1;
        }
    }
}