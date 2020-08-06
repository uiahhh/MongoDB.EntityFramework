using System;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace MongoDB.EntityFramework.Serializers
{
    //TODO: refactor and test performance

    public class StructSerializer<TStruct> : IBsonSerializer<TStruct>
    {
        public Type ValueType { get; }

        public StructSerializer()
        {
            ValueType = typeof(TStruct);
        }

        //TODO: fazer cache da estrutura por type
        private void SerializeObject(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            if (value == null)
            {
                context.Writer.WriteNull();
            }
            else
            {
                context.Writer.WriteStartDocument();

                var fields = ValueType.GetSerializableFields();
                foreach (var field in fields)
                {
                    context.Writer.WriteName(field.Name);
                    BsonSerializer.Serialize(context.Writer, field.FieldType, field.GetValue(value), null, args);
                }

                var properties = ValueType.GetSerializableProperties();
                foreach (var property in properties)
                {
                    context.Writer.WriteName(property.Name);
                    args.NominalType = property.PropertyType;
                    BsonSerializer.Serialize(context.Writer, property.PropertyType, property.GetValue(value), null, args);
                }

                context.Writer.WriteEndDocument();
            }
        }

        //TODO: gravar os passos para deserializar por type, assim faz menos ifs
        private object DeserializeObject(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonType = context.Reader.GetCurrentBsonType();
            if (bsonType == BsonType.Null)
            {
                context.Reader.ReadNull();
                return null;
            }
            else
            {
                var obj = Activator.CreateInstance(ValueType);

                context.Reader.ReadStartDocument();

                while (context.Reader.CurrentBsonType != BsonType.EndOfDocument)
                {
                    {
                        var name = string.Empty;

                        //TODO: remove o try, pois isso passa por cada struct de cada registro do retorno de uma consulta
                        try
                        {
                            name = context.Reader.ReadName();
                        }
                        catch (Exception)
                        {
                            continue;
                        }

                        var property = ValueType.GetProperty(name);
                        if (property == null && name == "_id")
                        {
                            property = ValueType.GetProperty("Id");
                        }

                        if (property != null)
                        {
                            var value = BsonSerializer.Deserialize(context.Reader, property.PropertyType);

                            if (property.CanWrite)
                            {
                                property.SetValue(obj, value, null);
                            }
                            else
                            {
                                var readOnlyField = ValueType.GetField($"<{name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                                readOnlyField.SetValue(obj, value);
                            }
                        }
                        else
                        {
                            var field = ValueType.GetField(name);
                            if (field == null && name == "_id")
                            {
                                field = ValueType.GetField("Id");
                            }

                            if (field != null)
                            {
                                var value = BsonSerializer.Deserialize(context.Reader, field.FieldType);
                                field.SetValue(obj, value);
                            }
                        }
                    }
                }

                context.Reader.ReadEndDocument();

                return obj;
            }
        }

        TStruct IBsonSerializer<TStruct>.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            return (TStruct)this.DeserializeObject(context, args);
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TStruct value)
        {
            this.SerializeObject(context, args, value);
        }

        public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            return this.DeserializeObject(context, args);
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            this.SerializeObject(context, args, value);
        }
    }
}