using CoffeeNChill.Functions.DTOs;
using CoffeeNChill.Functions.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CoffeeNChill.Functions.Functions.MenuItems
{
    public class GetMenuItemsByCategoryFunction
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly ILogger<GetMenuItemsByCategoryFunction> _logger;

        public GetMenuItemsByCategoryFunction(
            ITableStorageService tableStorageService,
            ILogger<GetMenuItemsByCategoryFunction> logger)
        {
            _tableStorageService = tableStorageService;
            _logger = logger;
        }

        // Returns every menu item belonging to one category, such as "Hot Drinks"
        // This is a partition-scoped query — more efficient than scanning the whole table
        [Function("GetMenuItemsByCategory")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "menu/category/{category}")]
            HttpRequestData req,
            string category)
        {
            _logger.LogInformation("Retrieving menu items for category: {Category}", category);

            try
            {
                // Rejects blank categories
                if (string.IsNullOrWhiteSpace(category))
                {
                    return await WriteJsonResponse(req, HttpStatusCode.BadRequest,
                        new { error = "Category is required." });
                }

                // Only queries the rows whose PartitionKey matches this category
                var menuItems = await _tableStorageService.GetMenuItemsByCategoryAsync(category);

                // Maps table entities into the response DTO, renaming PartitionKey/RowKey to Category/SKU
                var responseDtos = menuItems.Select(menuItem => new MenuItemResponseDto
                {
                    Category = menuItem.PartitionKey,
                    SKU = menuItem.RowKey,
                    Name = menuItem.Name,
                    Description = menuItem.Description,
                    Price = menuItem.Price,
                    IsAvailable = menuItem.IsAvailable
                }).ToList();

                // No matching rows still returns 200 with an empty array here, rather than a 404 error
                return await WriteJsonResponse(req, HttpStatusCode.OK, responseDtos);
            }
            catch (Exception ex)
            {
                // Any table storage failure is caught here so the client gets an error instead of an unhandled exception
                _logger.LogError(ex, "Error retrieving menu items for category: {Category}", category);

                return await WriteJsonResponse(req, HttpStatusCode.InternalServerError,
                    new { error = "An unexpected error occurred while retrieving menu items by category." });
            }
        }

        // Builds an HttpResponseData with given status code and JSON body
        // Centralizes response construction so each branch doesn't repeat it
        private static async Task<HttpResponseData> WriteJsonResponse(
            HttpRequestData req,
            HttpStatusCode statusCode,
            object body)
        {
            var response = req.CreateResponse(statusCode);
            await response.WriteAsJsonAsync(body);
            return response;
        }
    }
}