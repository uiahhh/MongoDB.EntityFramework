using System;

namespace MongoDB.EntityFramework.Samples.Entities
{
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

        public decimal TotalValue { get; set; }
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
            Code = code;
        }

        public Guid Id { get; }
        public string Code { get; }

        public override bool Equals(object obj)
        {
            return obj is ItemCode id &&
                   Id == id.Id &&
                   Code == id.Code;
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
        }

        public ItemId Id { get; set; }

        public ItemId IdOriginal { get; set; }

        public ItemCode Code { get; set; }

        public string StoreName { get; private set; }
        public decimal TotalValue { get; set; }
    }
}