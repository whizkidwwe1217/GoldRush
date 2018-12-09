using System.Threading.Tasks;

namespace GoldRush.Core.Pipeline
{
    public class Pipeline<T> : IFilterChain<T>
    {
        private IFilter<T> root;

        public Pipeline(bool ignoreExceptions = false)
        {
            IgnoreExceptions = ignoreExceptions;
        }

        public bool IgnoreExceptions { get; set; }

        public async Task<T> Execute(T input)
        {
            if (IgnoreExceptions)
            {
                try
                {
                    return await root.Execute(input);
                }
                catch
                {
                    return input;
                }
            }
            else
                return await root.Execute(input);
        }

        public IFilterChain<T> Register(IFilter<T> filter)
        {
            if (root == null) root = filter;
            else root.Register(filter);
            return this;
        }
    }
}