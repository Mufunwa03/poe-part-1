using Azure;
using CoffeeNChill.Functions.DTOs;
using CoffeeNChill.Functions.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace CoffeeNChill.Functions.Functions.MenuItems
{
    public class CreateMenuItemFunction
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly ILogger<CreateMenuItemFunction> _logger;

        public CreateMenuItemFunction(
            ITableStorageService tableStorageService,
            ILogger<CreateMenuItemFunction> logger)
        {
            _tableStorageService = tableStorageService;
            _logger = logger;
        }

        // Creates a new menu item entity. Category becomes PartitionKey and SKU becomes RowKey
        [Function("CreateMenuItem")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "menu")]
            HttpRequestData req)
        {
            _logger.LogInformation("Creating a new menu item.");

            try
            {
                // Makes request body case insensitive, so "name" and "Name" will both work
                var request = await JsonSerializer.DeserializeAsync<CreateMenuItemRequest>(
                    req.Body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (request == null)
                {
                    return await WriteJsonResponse(req, HttpStatusCode.BadRequest,
                        new { error = "Invalid request body." });
                }

                // Validates fields the client is expected to supply
                // Category and SKU together form the entity's unique key, so both must be present
                // Name/Description/Price are basic data integrity checks
                if (string.IsNullOrWhiteSpace(request.Category))
                {
                    return await WriteJsonResponse(req, HttpStatusCode.BadRequest,
                        new { error = "Category is required." });
                }

                if (string.IsNullOrWhiteSpace(request.SKU))
                {
                    return await WriteJsonResponse(req, HttpStatusCode.BadRequest,
                        new { error = "SKU is required." });
                }

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

                // Delegates actual insert to storage service.
                var menuItem = await _tableStorageService.CreateMenuItemAsync(request);

                // Maps newly created table entites into the response DTO, renaming PartitionKey/RowKey to Category/SKU
                var responseDto = new MenuItemResponseDto
                {
                    Category = menuItem.PartitionKey,
                    SKU = menuItem.RowKey,
                    Name = menuItem.Name,
                    Description = menuItem.Description,
                    Price = menuItem.Price,
                    IsAvailable = menuItem.IsAvailable
                };

                // "Created" is the conventional response for a successful POST that creates a new resource
                return await WriteJsonResponse(req, HttpStatusCode.Created, responseDto);
            }
            catch (JsonException ex)
            {
                // Throws error if JSON body is malformed/invalid
                _logger.LogWarning(ex, "Invalid JSON received while creating menu item.");

                return await WriteJsonResponse(req, HttpStatusCode.BadRequest,
                    new { error = "The request body contains invalid JSON." });
            }
            catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.Conflict)
            {
                // Azure Table Storage returns error when an entity with the same PartitionKey/RowKey already exists
                _logger.LogWarning(ex, "Duplicate menu item attempted.");

                return await WriteJsonResponse(req, HttpStatusCode.Conflict,
                    new { error = "A menu item with the same category and SKU already exists." });
            }
            catch (Exception ex)
            {
                // Any other failure shows as a generic error
                _logger.LogError(ex, "Error creating menu item.");

                return await WriteJsonResponse(req, HttpStatusCode.InternalServerError,
                    new { error = "An unexpected error occurred while creating the menu item." });
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