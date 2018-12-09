using System.Threading.Tasks;

namespace GoldRush.Core.Pipeline
{
    public abstract class AsyncFilter<T> : IFilter<T>
    {
        private IFilter<T> next;
        protected abstract Task<T> Process(T input);
        public virtual async Task<T> Execute(T input)
        {
            T value = await Process(input);
            if (next != null) value = await next.Execute(value);
            return value;
        }

        public void Register(IFilter<T> filter)
        {
            if (next == null) next = filter;
            else next.Register(filter);
        }
    }
}