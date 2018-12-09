using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoldRush.Core.Pipeline
{
    public interface IFilter<T>
    {
        Task<T> Execute(T input);
        void Register(IFilter<T> filter);
    }
}