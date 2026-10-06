using System;
using Azure;
using Azure.Data.Tables;

namespace CoffeeNChill.Functions.Models
{
    public class MenuItem : ITableEntity
    {
        // Azure Table storage partition key/category
        public string PartitionKey { get; set; } = string.Empty;

        // Azure Table storage row key/SKU
        public string RowKey { get; set; } = string.Empty;

        // Name of Menu Item
        public string Name { get; set; } = string.Empty;

        // Description of Menu Item
        public string Description { get; set; } = string.Empty;

        // Price of Menu Item
        public double Price { get; set; } = 0.00;

        // Determines if product is available for purchase or not
        public bool IsAvailable { get; set; }

        // Timestamp of when Menu Item was last updated, automatically maintained by table storage service
        public DateTimeOffset? Timestamp { get; set; }

        // Entity tag used to check if Menu Item was updated since we last retrieved it
        public ETag ETag { get; set; }


    }
}