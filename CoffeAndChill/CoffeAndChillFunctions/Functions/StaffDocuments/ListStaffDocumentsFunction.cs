using CoffeeNChill.Functions.DTOs.StaffDocuments;
using CoffeeNChill.Functions.Interfaces;
using CoffeeNChill.Functions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CoffeeNChill.Functions.Functions.StaffDocuments
{
    public class ListStaffDocumentsFunction
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<ListStaffDocumentsFunction> _logger;

        public ListStaffDocumentsFunction(
            IFileStorageService fileStorageService,
            ILogger<ListStaffDocumentsFunction> logger)
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        // Retrieves staff document metadata if requested
        // Stuff like file name, size, extension, upload date, etc.

        [Function("ListStaffDocuments")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents")]
            HttpRequestData req)
        {
            _logger.LogInformation("Retrieving all staff documents.");

            try
            {
                // Prompts storage service for full list of stored documents
                var documents = await _fileStorageService.GetAllDocumentsAsync();

                var responseDtos = BuildResponseList(documents);

                _logger.LogInformation("Retrieved {Count} staff document(s).", responseDtos.Count);

                return await WriteJsonResponse(req, HttpStatusCode.OK, responseDtos);
            }
            catch (Exception ex)
            {
                // Any storage failures show here as a generic error without crashing the function
                _logger.LogError(ex, "Error retrieving all staff documents.");

                return await WriteJsonResponse(
                    req,
                    HttpStatusCode.InternalServerError,
                    new { message = "An unexpected error occurred while retrieving staff documents." });
            }
        }

        // Maps each stored document to its outward-facing response DTO
        private static List<StaffDocumentResponse> BuildResponseList(List<StaffDocument> documents)
        {
            return documents
                .Select(document => new StaffDocumentResponse
                {
                    FileName = document.FileName,
                    FileExtension = document.FileExtension,
                    ContentType = document.ContentType,
                    FileSize = document.FileSize,
                    UploadedOn = document.UploadedOn,
                    ContainerName = document.ContainerName
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