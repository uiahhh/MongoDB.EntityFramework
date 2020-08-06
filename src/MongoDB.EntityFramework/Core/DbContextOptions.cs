namespace MongoDB.EntityFramework.Core
{
    public class DbContextOptions : IDbContextOptions
    {
        public bool EnableMappingReadOnlyProperties { get; set; }

        public bool EnableStructSerializer { get; set; }

        public bool AnyOptionEnabled => EnableMappingReadOnlyProperties || EnableStructSerializer;
    }
}