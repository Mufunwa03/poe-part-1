using CoffeeNChill.Functions.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CoffeeNChill.Functions.Functions.MenuItems
{
    public class DeleteMenuItemFunction
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly ILogger<DeleteMenuItemFunction> _logger;

        public DeleteMenuItemFunction(
            ITableStorageService tableStorageService,
            ILogger<DeleteMenuItemFunction> logger)
        {
            _tableStorageService = tableStorageService;
            _logger = logger;
        }

        // Removes a single menu item from the table, identified by category (PartitionKey) and SKU (RowKey)
        [Function("DeleteMenuItem")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "menu/{category}/{sku}")]
            HttpRequestData req,
            string category,
            string sku)
        {
            _logger.LogInformation(
                "Deleting menu item. Category: {Category}, SKU: {SKU}",
                category,
                sku);

            try
            {
                //PartitionKey and RowKey are required to identify which row to delete
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

                // Asks storage service to delete the entity. Returns false if no row matched that category/SKU
                bool deleted = await _tableStorageService.DeleteMenuItemAsync(category, sku);

                if (!deleted)
                {
                    return await WriteJsonResponse(req, HttpStatusCode.NotFound,
                        new { error = "Menu item not found." });
                }

                // A successful delete has nothing to return, so "No Content" is the logical response
                return req.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                // Any table storage failure is caught here so the client gets an error instead of an unhandled exception
                _logger.LogError(
                    ex,
                    "Error deleting menu item. Category: {Category}, SKU: {SKU}",
                    category,
                    sku);

                return await WriteJsonResponse(req, HttpStatusCode.InternalServerError,
                    new { error = "An unexpected error occurred while deleting the menu item." });
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