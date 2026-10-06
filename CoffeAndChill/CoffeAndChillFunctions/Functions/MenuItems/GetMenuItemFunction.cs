using CoffeeNChill.Functions.DTOs;
using CoffeeNChill.Functions.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
//using static Google.Protobuf.Collections.MapField<TKey, TValue>;

// ** This class was not part of the assignment scope, but was added for convenience **

namespace CoffeeNChill.Functions.Functions.MenuItems
{
    public class GetMenuItemFunction
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly ILogger<GetMenuItemFunction> _logger;

        public GetMenuItemFunction(
            ITableStorageService tableStorageService,
            ILogger<GetMenuItemFunction> logger)
        {
            _tableStorageService = tableStorageService;
            _logger = logger;
        }

        // Returns a single menu item identified by category (PartitionKey) and SKU (RowKey)
        [Function("GetMenuItem")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "menu/item/{category}/{sku}")]
            HttpRequestData req,
            string category,
            string sku)
        {
            _logger.LogInformation(
                "Retrieving menu item. Category: {Category}, SKU: {SKU}",
                category,
                sku);

            try
            {
                // Both category and SKU are required to find a specific item
                // rejects request early if either is missing
                if (string.IsNullOrWhiteSpace(category))
                {
                    return await WriteJsonResponse(req, HttpStatusCode.BadRequest,
                        new { error = "Category is required." });
                }

                if (string.IsNullOrWhiteSpace(sku))
                {
                    return await WriteJsonResponse(req, HttpStatusCode.BadRequest,
                        new { error = "SKU is required." });
                }

                // Finds exact entity by PartitionKey + RowKey
                // This is a direct point lookup, so it's the fastest possible table query
                var menuItem = await _tableStorageService.GetMenuItemAsync(category, sku);

                // No entity matched with category/SKU combination
                if (menuItem == null)
                {
                    return await WriteJsonResponse(req, HttpStatusCode.NotFound,
                        new { error = "Menu item not found." });
                }

                // Maps table entites into the response DTO, renaming PartitionKey/RowKey to Category/SKU
                var responseDto = new MenuItemResponseDto
                {
                    Category = menuItem.PartitionKey,
                    SKU = menuItem.RowKey,
                    Name = menuItem.Name,
                    Description = menuItem.Description,
                    Price = menuItem.Price,
                    IsAvailable = menuItem.IsAvailable
                };

                return await WriteJsonResponse(req, HttpStatusCode.OK, responseDto);
            }
            catch (Exception ex)
            {
                // Any table storage failure, such as connectivity, is caught here so function displays an error instead of an unhandled exception
                _logger.LogError(
                    ex,
                    "Error retrieving menu item. Category: {Category}, SKU: {SKU}",
                    category,
                    sku);

                return await WriteJsonResponse(req, HttpStatusCode.InternalServerError,
                    new { error = "An unexpected error occurred while retrieving the menu item." });
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