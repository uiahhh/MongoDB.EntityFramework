using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MongoDB.EntityFramework.Serializers
{
    public static class TypeExtensions
    {
        public static bool IsStruct(this Type type)
        {
            if (IsPrimitiveType(type) == true)
                return false;

            if (type.IsValueType == false)
                return false;

            return true;
        }

        public static bool IsPrimitiveType(this Type type)
        {
            if (type.GetTypeInfo().IsPrimitive == true)
                return true;

            if (type.GetTypeInfo().IsEnum == true)
                return true;

            if (type == typeof(decimal))
                return true;

            if (type == typeof(string))
                return true;

            if (type == typeof(DateTime))
                return true;

            if (type == typeof(DateTimeOffset))
                return true;

            if (type == typeof(TimeSpan))
                return true;

            if (type == typeof(Guid))
                return true;

            return false;
        }

        public static IEnumerable<FieldInfo> GetSerializableFields(this Type type)
        {
            return type.GetFields().Where(property => IsFieldSerializable(type, property));
        }

        public static IEnumerable<PropertyInfo> GetSerializableProperties(this Type type)
        {
            var x1 = type.GetProperties().Where(property => IsPropertySerializable(type, property));

            return type.GetProperties().Where(property => IsPropertySerializable(type, property));
        }

        public static bool IsFieldSerializable(this Type type, FieldInfo field)
        {
            //if (field.IsInitOnly == true)
            //    return false;

            if (field.IsLiteral == true)
                return false;

            if (field.IsDefined(typeof(CompilerGeneratedAttribute), false) == true)
                return false;

            return true;
        }

        public static bool IsPropertySerializable(this Type type, PropertyInfo property)
        {
            if (property.CanRead == false)
                return false;

            //if (property.CanWrite == false)
            //    return false;

            if (property.GetIndexParameters().Length != 0)
                return false;

            if (property.GetMethod.IsVirtual && property.GetMethod.GetBaseDefinition().DeclaringType != type)
                return false;

            return true;
        }
    }
}