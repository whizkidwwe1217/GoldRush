namespace GoldRush.Infrastructure
{
    public class DbContextErrorDescriber
    {
        public virtual DbContextError DuplicateTenantName(string tenantName)
        {
            return new DbContextError
            {
                Code = nameof(DuplicateTenantName),
                Description = $"Tenant '{tenantName} is already taken."
            };
        }

        public virtual DbContextError ConcurrencyFailure()
        {
            return new DbContextError
            {
                Code = nameof(ConcurrencyFailure),
                Description = "Optimistic concurrency failure, object has been modified."
            };
        }

        public virtual DbContextError MigrationError()
        {
            return new DbContextError
            {
                Code = nameof(MigrationError),
                Description = "Error applying database migrations."
            };
        }

        public virtual DbContextError ErrorGeneratingSeedData(string exception)
        {
            return new DbContextError
            {
                Code = nameof(ErrorGeneratingSeedData),
                Description = $"Error generating default seed data. {exception}"
            };
        }
    }
}