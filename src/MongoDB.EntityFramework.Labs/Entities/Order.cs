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
}