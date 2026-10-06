using System;
using Azure;
using Azure.Data.Tables;

namespace CoffeeNChill.Functions.Models
{
    public class MenuItem : ITableEntity
    {
        //Azure Table storage partition key
        public string PartitionKey { get; set; } = string.Empty;

        //Azure Table storage row key
        public string RowKey { get; set; } = string.Empty;

        //Menu Item name
        public string Name { get; set; } = string.Empty;

        //Description of the menu items
        public string Description { get; set; } = string.Empty;

        //Selling price
        public double Price { get; set; } = 0.00;

        //Indicates whether the item is available for sale
        public bool IsAvailable { get; set; }

        //Automatically mainted by Azure Table storage
        public DateTimeOffset? Timestamp { get; set; }

        //Entity Tag used for concurrency checking
        public ETag ETag { get; set; }

        
    }
}