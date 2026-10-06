namespace CoffeeNChill.Functions.DTOs
{
    public class CreateMenuItemRequest
    {
        // Category the item belongs to 
        public string Category { get; set; } = string.Empty;

        // Distinct SKU code identifying the item 
        public string SKU { get; set; } = string.Empty;

        // Display name of the item
        public string Name { get; set; } = string.Empty;

        // Short description shown to customers
        public string Description { get; set; } = string.Empty;

        // Cost of the item
        public double Price { get; set; }

        // Whether the item can currently be ordered
        public bool IsAvailable { get; set; }
    }
}