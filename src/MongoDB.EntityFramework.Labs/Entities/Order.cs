using System;
using System.Collections.Generic;

namespace MongoDB.EntityFramework.Samples.Entities
{
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

    public class Order
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