using System.Collections.Generic;
using System.Linq;

namespace GoldRush.Infrastructure
{
    public class DbContextResult
    {
        private static readonly DbContextResult _success = new DbContextResult { Succeeded = true };
        private List<DbContextError> _errors = new List<DbContextError>();

        public DbContextResult() { }

        public static DbContextResult Success { get => _success; }
        public bool Succeeded { get; protected set; }
        public IEnumerable<DbContextError> Errors { get => _errors; }
        public static DbContextResult Failed(params DbContextError[] errors)
        {
            var result = new DbContextResult { Succeeded = false };
            if (errors != null)
            {
                result._errors.AddRange(errors);
            }
            return result;
        }

        public override string ToString()
        {
            return Succeeded ? 
                   "Succeeded" : 
                   string.Format("{0} : {1}", "Failed", string.Join(",", Errors.Select(x => x.Code).ToList()));
        }
    }
}