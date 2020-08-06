using System.Linq;
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace MongoDB.EntityFramework.Conventions
{
    // https://techblog.adrianlowdon.co.uk/2018/05/30/mongodb-doesnt-serialise-c-read-only-properties/
    // https://stackoverflow.com/questions/39604820/serialize-get-only-properties-on-mongodb
    public class MappingReadOnlyPropertiesConvention : ConventionBase, IClassMapConvention
    {
        private readonly BindingFlags _bindingFlags;

        public MappingReadOnlyPropertiesConvention() : this(BindingFlags.Instance | BindingFlags.Public)
        {
        }

        public MappingReadOnlyPropertiesConvention(BindingFlags bindingFlags)
        {
            _bindingFlags = bindingFlags | BindingFlags.DeclaredOnly;
        }

        public void Apply(BsonClassMap classMap)
        {
            var readOnlyProperties = classMap
                .ClassType
                .GetTypeInfo()
                .GetProperties(_bindingFlags)
                .Where(p => IsReadOnlyProperty(classMap, p))
                .ToList();

            foreach (var property in readOnlyProperties)
            {
                classMap.MapMember(property);
            }
        }

        private static bool IsReadOnlyProperty(BsonClassMap classMap, PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanRead) return false;
            if (propertyInfo.CanWrite) return false;
            if (propertyInfo.GetIndexParameters().Length != 0) return false;

            var getMethodInfo = propertyInfo.GetMethod;

            if (getMethodInfo.IsVirtual && getMethodInfo.GetBaseDefinition().DeclaringType != classMap.ClassType) return false;

            return true;
        }
    }
}