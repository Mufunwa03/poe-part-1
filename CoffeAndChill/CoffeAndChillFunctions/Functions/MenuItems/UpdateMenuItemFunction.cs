using CoffeeNChill.Functions.DTOs;
using CoffeeNChill.Functions.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace CoffeeNChill.Functions.Functions.MenuItems
{
    public class UpdateMenuItemFunction
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly ILogger<UpdateMenuItemFunction> _logger;

        public UpdateMenuItemFunction(
            ITableStorageService tableStorageService,
            ILogger<UpdateMenuItemFunction> logger)
        {
            _tableStorageService = tableStorageService;
            _logger = logger;
        }

        // Updates Name, Description, and Price of an existing menu item
        // Identified in the table by category (PartitionKey) and SKU (RowKey)
        [Function("UpdateMenuItem")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "menu/{category}/{sku}")]
            HttpRequestData req,
            string category,
            string sku)
        {
            _logger.LogInformation(
                "Updating menu item. Category: {Category}, SKU: {SKU}",
                category,
                sku);

            try
            {
                // PartitionKey and RowKey are required to identify which row to update
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

                // Makes request body case insensitive, so "name" and "Name" will both work
                var request = await JsonSerializer.DeserializeAsync<UpdateMenuItemRequest>(
                    req.Body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (request == null)
                {
                    return await WriteJsonResponse(req, HttpStatusCode.BadRequest,
                        new { error = "Invalid request body." });
                }

                // Validates fields the client is expected to supply
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return await WriteJsonResponse(req, HttpStatusCode.BadRequest,
                        new { error = "Name is required." });
                }

                if (string.IsNullOrWhiteSpace(request.Description))
                {
                    return await WriteJsonResponse(req, HttpStatusCode.BadRequest,
                        new { error = "Description is required." });
                }

                if (request.Price <= 0)
                {
                    return await WriteJsonResponse(req, HttpStatusCode.BadRequest,
                        new { error = "Price must be greater than zero." });
                }

                // Delegates actual update to storage service. Returns null if no entity exists for that category/SKU combination
                var updatedMenuItem = await _tableStorageService.UpdateMenuItemAsync(category, sku, request);

                if (updatedMenuItem == null)
                {
                    return await WriteJsonResponse(req, HttpStatusCode.NotFound,
                        new { error = "Menu item not found." });
                }

                // Maps updated table entites into the response DTO, renaming PartitionKey/RowKey to Category/SKU
                var responseDto = new MenuItemResponseDto
                {
                    Category = updatedMenuItem.PartitionKey,
                    SKU = updatedMenuItem.RowKey,
                    Name = updatedMenuItem.Name,
                    Description = updatedMenuItem.Description,
                    Price = updatedMenuItem.Price,
                    IsAvailable = updatedMenuItem.IsAvailable
                };

                return await WriteJsonResponse(req, HttpStatusCode.OK, responseDto);
            }
            catch (JsonException ex)
            {
                // Throws error if JSON body is malformed/invalid
                _logger.LogError(ex, "Invalid JSON received while updating menu item.");

                return await WriteJsonResponse(req, HttpStatusCode.BadRequest,
                    new { error = "The request body contains invalid JSON." });
            }
            catch (Exception ex)
            {
                // Any other failure, such as table storage issue
                _logger.LogError(ex, "Error updating menu item.");

                return await WriteJsonResponse(req, HttpStatusCode.InternalServerError,
                    new { error = "An unexpected error occurred while updating the menu item." });
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