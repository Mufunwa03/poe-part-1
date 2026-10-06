using CoffeeNChill.Functions.DTOs;
using CoffeeNChill.Functions.Interfaces;
using CoffeeNChill.Functions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CoffeeNChill.Functions.Functions.MenuItems
{
    public class GetAllMenuItemsFunction
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly ILogger<GetAllMenuItemsFunction> _logger;

        public GetAllMenuItemsFunction(
            ITableStorageService tableStorageService,
            ILogger<GetAllMenuItemsFunction> logger)
        {
            _tableStorageService = tableStorageService;
            _logger = logger;
        }

        // Returns every menu item currently stored in the MenuItems table across all categories
        // This is the full digital menu
        [Function("GetAllMenuItems")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "menu")]
            HttpRequestData req)
        {
            _logger.LogInformation("Retrieving all menu items.");

            try
            {
                // Queries every entity in MenuItems table via storage service
                // Does not filter by category
                var menuItems = await _tableStorageService.GetAllMenuItemsAsync();

                // Maps table entities into response DTO
                // PartitionKey and RowKey are the table's storage-level identifiers and are translated
                // into more meaningful Category/SKU names for the client
                var responseDtos = BuildResponseList(menuItems);

                return await WriteJsonResponse(req, HttpStatusCode.OK, responseDtos);
            }
            catch (Exception ex)
            {
                // Any table storage failure, such as connectivity, is caught here so function displays an error instead of an unhandled exception
                _logger.LogError(ex, "Error retrieving all menu items.");

                return await WriteJsonResponse(
                    req,
                    HttpStatusCode.InternalServerError,
                    new { error = "An unexpected error occurred while retrieving menu items." });
            }
        }

        // Maps MenuItem table entities to an outward-facing response DTO,
        // while also renaming PartitionKey/RowKey to Category/SKU

        private static List<MenuItemResponseDto> BuildResponseList(IEnumerable<MenuItem> menuItems)
        {
            return menuItems.Select(menuItem => new MenuItemResponseDto
            {
                Category = menuItem.PartitionKey,
                SKU = menuItem.RowKey,
                Name = menuItem.Name,
                Description = menuItem.Description,
                Price = menuItem.Price,
                IsAvailable = menuItem.IsAvailable
            })
            .ToList();
        }

        // Builds an HttpResponseData with given status code and JSON body
        // Centralizes response construction so success and error paths share it

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