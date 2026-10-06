using CoffeeNChill.Functions.DTOs;
using CoffeeNChill.Functions.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoffeeNChill.Functions.Interfaces
{
    public interface ITableStorageService
    {
        // Adds a new menu item from the given request
        Task<MenuItem> CreateMenuItemAsync(CreateMenuItemRequest request);

        // Retrieves every menu item across all categories
        Task<List<MenuItem>> GetAllMenuItemsAsync();

        // Retrieves all menu items belonging to a given category
        Task<List<MenuItem>> GetMenuItemsByCategoryAsync(string category);

        // Retrieves a single menu item by category and SKU, or null if not found
        Task<MenuItem?> GetMenuItemAsync(string category, string sku);

        // Applies updates to an existing menu item, returning the updated item or null if not found
        Task<MenuItem?> UpdateMenuItemAsync(
            string category,
            string sku,
            UpdateMenuItemRequest request);

        // Removes a menu item, and returns whether the deletion succeeded
        Task<bool> DeleteMenuItemAsync(
            string category,
            string sku);
    }
}