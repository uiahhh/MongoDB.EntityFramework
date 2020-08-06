using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.EntityFramework.Serializers
{
    internal class SerializationProvider : BsonSerializationProviderBase
    {
        private static readonly object locker = new object();
        private static Dictionary<Type, IBsonSerializer> structSerializers;
        private static DecimalSerializer decimalSerializer;

        static SerializationProvider()
        {
            structSerializers = new Dictionary<Type, IBsonSerializer>();
            decimalSerializer = new DecimalSerializer();
        }

        public override IBsonSerializer GetSerializer(Type type, IBsonSerializerRegistry serializerRegistry)
        {
            if (type == typeof(decimal))
            {
                return decimalSerializer;
            }
            else if (type.IsStruct() && type != typeof(ObjectId))
            {
                IBsonSerializer structSerializer = null;

                if (structSerializers.TryGetValue(type, out structSerializer) == false)
                {
                    lock (locker)
                    {
                        if (structSerializers.TryGetValue(type, out structSerializer) == false)
                        {
                            var structSerializerType = typeof(StructSerializer<>);
                            var structSerializerGenericType = structSerializerType.MakeGenericType(type);
                            structSerializer = Activator.CreateInstance(structSerializerGenericType) as IBsonSerializer;
                            structSerializers.Add(type, structSerializer);
                        }
                    }
                }

                return structSerializer;
            }
            else
            {
                return null;
            }
        }
    }
}