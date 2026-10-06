using System;

namespace CoffeeNChill.Functions.DTOs
{
    public class MenuItemResponseDto
    {
        // Category the item belongs to
        public string Category { get; set; } = string.Empty;

        // Unique SKU identifying the item
        public string SKU { get; set; } = string.Empty;

        // Display name returned to the client
        public string Name { get; set; } = string.Empty;

        // Description returned to the client
        public string Description { get; set; } = string.Empty;

        // Current selling price
        public double Price { get; set; }

        // Current availability status
        public bool IsAvailable { get; set; }
    }

}
