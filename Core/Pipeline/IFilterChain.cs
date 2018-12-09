using System.Threading.Tasks;

namespace GoldRush.Core.Pipeline
{
    public interface IFilterChain<T>
    {
        bool IgnoreExceptions { get; set; }
        Task<T> Execute(T input);
        IFilterChain<T> Register(IFilter<T> filter);
    }
}