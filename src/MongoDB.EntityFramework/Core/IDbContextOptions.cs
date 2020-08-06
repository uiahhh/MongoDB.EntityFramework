namespace MongoDB.EntityFramework.Core
{
    public interface IDbContextOptions
    {
        bool EnableMappingReadOnlyProperties { get; }

        bool EnableStructSerializer { get; }

        bool AnyOptionEnabled { get; }
    }
}