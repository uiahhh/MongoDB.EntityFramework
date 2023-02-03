using AutoMapper;
using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace MongoDB.EntityFramework.Samples.Entities
{
    public class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<EntityGuid, EntityDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(source => source.Id.ToString()));

            CreateMap<EntityObjectId, EntityDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(source => source.Id.ToString()));

            CreateMap<Order, EntityDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(source => source.Id.ToString()));
        }
    }

    public class Box
    {
        public Box(BoxId id, int measures)
        {
            Id = id;
            Measures = measures;
        }

        public BoxId Id { get; set; }

        public int Measures { get; set; }

        public List<int> Numbers { get; set; }

        public List<Configuration> Configurations { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Box box &&
                   EqualityComparer<BoxId>.Default.Equals(Id, box.Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }
    }

    public class TenantId
    {
        public int Value { get; set; }
    }

    public class Configuration
    {
        public TenantId TenantId { get; set; }
    }

    public class BoxId
    {
        public BoxId(string value)
        {
            Value = value;
        }

        public string Value { get; set; }

        public override bool Equals(object obj)
        {
            return obj is BoxId id &&
                   Value == id.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }
    }

    public class EntityDTO
    {
        public string Id { get; set; }

        public string StoreName { get; set; }

        public decimal? TotalValue { get; set; }
    }

    public interface IEntity<TId>
        where TId : IEquatable<TId>
    {
        TId Id { get; set; }

        TId ProjectId { get; set; }

        string StoreName { get; set; }

        decimal? TotalValue { get; set; }

        DateTime CreatedAt { get; set; }
    }

    public static class Ids
    {
        private static int totalRegisters = 2_000_000 * 2;
        private static int totalRegistersPerProject = 20_000;
        private static int totalIds = totalRegisters / totalRegistersPerProject;

        private static Guid[] guids;
        private static ObjectId[] objectIds;
        private static int[] ints;

        public static Guid GetGuid()
        {
            if (guids == null)
            {
                var ids = new List<Guid>();
                for (int i = 0; i < totalIds; i++)
                {
                    ids.Add(Guid.NewGuid());
                }

                guids = ids.ToArray();
            }

            var index = new Random().Next(0, totalIds - 1);
            return guids[index];
        }

        public static ObjectId GetObjectId()
        {
            if (objectIds == null)
            {
                var ids = new List<ObjectId>();
                for (int i = 0; i < totalIds; i++)
                {
                    ids.Add(ObjectId.GenerateNewId());
                }

                objectIds = ids.ToArray();
            }

            var index = new Random().Next(0, totalIds - 1);
            return objectIds[index];
        }

        public static int GetInt()
        {
            if (ints == null)
            {
                var ids = new List<int>();
                for (int i = 0; i < totalIds; i++)
                {
                    ids.Add(i+1);
                }

                ints = ids.ToArray();
            }

            var index = new Random().Next(0, totalIds - 1);
            return ints[index];
        }
    }

    public class EntityGuid : IEntity<Guid>
    {
        public EntityGuid()
        {
            Id = Guid.NewGuid();
            StoreName = "some name 01";
            TotalValue = 11.0M;
            ProjectId = Ids.GetGuid();
            CreatedAt = RandomDay();
        }

        public Guid Id { get; set; }

        public Guid ProjectId { get; set; }

        public string StoreName { get; set; }

        public decimal? TotalValue { get; set; }

        public DateTime CreatedAt { get; set; }

        private DateTime RandomDay()
        {
            DateTime start = new DateTime(1995, 1, 1);
            int range = (DateTime.Today - start).Days;
            return start.AddDays(new Random().Next(range));
        }
    }

    public class EntityObjectId : IEntity<ObjectId>
    {
        public EntityObjectId()
        {
            Id = ObjectId.GenerateNewId();
            StoreName = "some name 02";
            TotalValue = 12.0M;
            ProjectId = Ids.GetObjectId();
            CreatedAt = RandomDay();
        }

        public ObjectId Id { get; set; }

        public ObjectId ProjectId { get; set; }

        public string StoreName { get; set; }

        public decimal? TotalValue { get; set; }

        public DateTime CreatedAt { get; set; }

        private DateTime RandomDay()
        {
            DateTime start = new DateTime(1995, 1, 1);
            int range = (DateTime.Today - start).Days;
            return start.AddDays(new Random().Next(range));
        }
    }

    public interface IOrder
    {
        bool IsFlat { get; }
    }

    public class Order : IOrder
    {
        public Order(Guid id, string storeName, decimal totalValue)
        {
            Id = id;
            StoreName = storeName;
            TotalValue = totalValue;
        }

        public Guid Id { get; set; }

        public string StoreName { get; set; }

        public decimal? TotalValue { get; set; }

        public bool IsFlat => false;
    }

    public class OrderFlat : IOrder
    {
        public OrderFlat(Guid id, string storeName, decimal totalValue)
        {
            Id = id;
            StoreName = storeName;
            TotalValue = totalValue;
        }

        public Guid Id { get; set; }

        public string StoreName { get; set; }

        public decimal? TotalValue { get; set; }

        public bool IsFlat => true;
    }

    public struct ItemId
    {
        public ItemId(int id, string code)
        {
            Id = id;
            Code = code;
        }

        public int Id { get; }
        public string Code { get; }

        public override bool Equals(object obj)
        {
            return obj is ItemId id &&
                   Id == id.Id &&
                   Code == id.Code;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Code);
        }
    }

    public struct ItemCode
    {
        public ItemCode(string code)
        {
            Id = Guid.NewGuid();
            Code = new ItemId(199, code);
        }

        public Guid Id { get; }
        public ItemId Code { get; }

        public override bool Equals(object obj)
        {
            return obj is ItemCode id &&
                   Id == id.Id &&
                   Code.Equals(id.Code);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Code);
        }
    }

    public class Item
    {
        public Item(int id, string storeName, decimal totalValue)
        {
            Code = new ItemCode(totalValue.ToString());
            Id = new ItemId(id, storeName);
            IdOriginal = new ItemId(id + 111, storeName + storeName);
            StoreName = storeName;
            TotalValue = totalValue;
            MyProperty = 10;
        }

        public ItemId Id { get; set; }

        public ItemId IdOriginal { get; set; }

        public ItemCode Code { get; set; }

        public decimal? MyProperty { get; set; }

        public string StoreName { get; private set; }
        public decimal TotalValue { get; set; }
    }
}