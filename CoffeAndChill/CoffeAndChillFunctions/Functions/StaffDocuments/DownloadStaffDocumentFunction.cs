using CoffeeNChill.Functions.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CoffeeNChill.Functions.Functions.StaffDocuments
{
    public class DownloadStaffDocumentFunction
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<DownloadStaffDocumentFunction> _logger;

        public DownloadStaffDocumentFunction(
            IFileStorageService fileStorageService,
            ILogger<DownloadStaffDocumentFunction> logger)
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        // Downloads staff document if requested
        [Function("DownloadStaffDocument")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents/download/{fileName}")]
            HttpRequestData req,
            string fileName)
        {
            _logger.LogInformation("Retrieving staff document: {FileName}", fileName);

            try
            {
                // Shows a message if file name is missing/blank
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return await WriteJsonResponse(req, HttpStatusCode.BadRequest,
                        new { message = "A file name is required." });
                }

                // Attempts to download the staff documents file
                // If the fileStream variable is "null" then the file doesn't exist
                Stream? fileStream = await _fileStorageService.DownloadDocumentAsync(fileName);

                if (fileStream == null)
                {
                    _logger.LogWarning("Staff document not found: {FileName}", fileName);

                    return await WriteJsonResponse(req, HttpStatusCode.NotFound,
                        new { message = "The requested document was not found." });
                }

                // Set headers so the browser/Postman treats this as a downloadable attachment with the right file type, instead of plain text
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", GetContentType(fileName));
                response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");

                // Streams file contents straight into response body instead of loading the whole file into memory first
                await fileStream.CopyToAsync(response.Body);
                await fileStream.DisposeAsync();

                _logger.LogInformation("Staff document retrieved successfully: {FileName}", fileName);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving staff document: {FileName}", fileName);

                return await WriteJsonResponse(req, HttpStatusCode.InternalServerError,
                    new { message = "An unexpected error occurred while retrieving the document." });
            }
        }

        // Builds an HttpResponseData with given status code and JSON body
        // Centralizes response construction so error paths don't repeat it

        private static async Task<HttpResponseData> WriteJsonResponse(
            HttpRequestData req,
            HttpStatusCode statusCode,
            object body)
        {
            var response = req.CreateResponse(statusCode);
            await response.WriteAsJsonAsync(body);
            return response;
        }

        // Identifies what is needed to view the file depending on the file extension
        private static string GetContentType(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }
    }
}