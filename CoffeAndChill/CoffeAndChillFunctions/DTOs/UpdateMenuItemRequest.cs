namespace CoffeeNChill.Functions.DTOs
{
    public class UpdateMenuItemRequest
    {
        // New display name to apply to the item
        public string Name { get; set; } = string.Empty;

        // New description to apply to the item
        public string Description { get; set; } = string.Empty;

        // New selling price to apply to the item
        public double Price { get; set; }

        // New availability status to apply to the item
        public bool IsAvailable { get; set; }
    }
}