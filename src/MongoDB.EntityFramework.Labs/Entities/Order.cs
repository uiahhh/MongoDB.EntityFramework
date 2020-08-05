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

    public class ItemId
    {
        public ItemId(int id)
        {
            Id = id;
        }

        public int Id { get; set; }

        public override bool Equals(object obj)
        {
            return obj is ItemId id &&
                   Id == id.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }
    }

    public class Item
    {
        public Item(int id, string storeName, decimal totalValue)
        {
            Id = new ItemId(id);
            StoreName = storeName;
            TotalValue = totalValue;
        }

        public ItemId Id { get; set; }

        public string StoreName { get; set; }
        public decimal TotalValue { get; set; }
    }
}