using CoffeeNChill.Functions.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CoffeeNChill.Functions.Functions.StaffDocuments
{
    public class DeleteStaffDocumentFunction
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<DeleteStaffDocumentFunction> _logger;

        public DeleteStaffDocumentFunction(
            IFileStorageService fileStorageService,
            ILogger<DeleteStaffDocumentFunction> logger)
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        // Deletes staff document if requested
        [Function("DeleteStaffDocument")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "documents/{fileName}")]
            HttpRequestData req,
            string fileName)
        {
            _logger.LogInformation("Deleting staff document: {FileName}", fileName);

            try
            {
                // Throws an error if file name is missing/blank
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return await WriteJsonResponse(req, HttpStatusCode.BadRequest,
                        new { error = "A file name is required." });
                }

                // Attempts to delete staff documents file. Returns false if file name doesn't exist, rather than throwing
                bool deleted = await _fileStorageService.DeleteDocumentAsync(fileName);

                if (!deleted)
                {
                    _logger.LogWarning("Staff document was not found for deletion: {FileName}", fileName);

                    return await WriteJsonResponse(req, HttpStatusCode.NotFound,
                        new { error = $"Document '{fileName}' was not found." });
                }

                _logger.LogInformation("Staff document deleted successfully: {FileName}", fileName);

                return await WriteJsonResponse(req, HttpStatusCode.OK,
                    new { message = $"Document '{fileName}' was deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting staff document: {FileName}", fileName);

                return await WriteJsonResponse(req, HttpStatusCode.InternalServerError,
                    new { error = "An unexpected error occurred while deleting the document." });
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