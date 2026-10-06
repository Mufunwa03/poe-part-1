using CoffeeNChill.Functions.DTOs.StaffDocuments;
using CoffeeNChill.Functions.Interfaces;
using CoffeeNChill.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CoffeeNChill.Functions.Functions.StaffDocuments
{
    public class UploadStaffDocumentFunction
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<UploadStaffDocumentFunction> _logger;

        // Cap uploads at 10 MB
        private const long MaxFileSize = 10 * 1024 * 1024;

        // Only these document types are accepted. Each extension is mapped
        // to their respective MIME types and must match a Content-Type header, or else the file will be rejected
        private static readonly Dictionary<string, string> AllowedFileTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                [".pdf"] = "application/pdf",
                [".doc"] = "application/msword",
                [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
            };

        // Dependencies are injected: storage service does the blob upload, logger then records what happened
        public UploadStaffDocumentFunction(
            IFileStorageService fileStorageService,
            ILogger<UploadStaffDocumentFunction> logger)
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        // Uploads staff document if requested.
        // Accepts a multipart/form-data request containing a single file under the field name "file",
        // validates it, uploads it, and returns the metadata
        [Function("UploadStaffDocument")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "documents/upload")]
            HttpRequest req)
        {
            _logger.LogInformation("Staff document upload request received.");

            // Upload can only be read as form data if the client actually sent
            // multipart/form-data — rejects anything else immediately
            if (!req.HasFormContentType)
            {
                _logger.LogWarning("Upload rejected because multipart/form-data was not used.");
                return BadRequest("The request must use multipart/form-data.");
            }

            // Parse the form and pull out the uploaded file
            var form = await req.ReadFormAsync();
            IFormFile? file = form.Files["file"];

            // Runs every validation rule (presence, size, name, extension, MIME type) If anything fails, ValidateFile returns an error message
            var validationError = ValidateFile(file);
            if (validationError is not null)
            {
                return BadRequest(validationError);
            }

            try
            {
                // Makes storage service do the writing to the blob storage
                // mainly for orchestration and validation
                StaffDocument document = await _fileStorageService.UploadDocumentAsync(file!);

                _logger.LogInformation(
                    "Staff document {FileName} uploaded successfully. Size: {FileSize} bytes.",
                    document.FileName,
                    document.FileSize);

                // Return only the fields the client needs (via the response DTO), not the internal storage model
                return new OkObjectResult(MapToResponse(document));
            }
            catch (Exception ex)
            {
                // Catch-all for anything unexpected like storage connectivity issues, so function produces a generic error instead of an unhandled exception
                _logger.LogError(ex, "Unexpected error while uploading staff document.");

                return new ObjectResult(new { error = "An unexpected error occurred while uploading the document." })
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
        }

        // Runs every validation rule against the incoming file.
        // Returns null when the file passes all checks, otherwise produces an error
        private string? ValidateFile(IFormFile? file)
        {
            // No file field in form data
            if (file == null)
            {
                _logger.LogWarning("Upload rejected because no file was supplied.");
                return "Please upload a file using the form-data field named 'file'.";
            }

            // A 0-byte "file" usually means the client sent an empty field by mistake
            if (file.Length == 0)
            {
                _logger.LogWarning("Upload rejected because the file was empty.");
                return "The uploaded file is empty.";
            }

            // Enforces size cap defined above
            if (file.Length > MaxFileSize)
            {
                _logger.LogWarning("Upload rejected because file {FileName} exceeded 10 MB.", file.FileName);
                return "The uploaded file exceeds the maximum allowed size of 10 MB.";
            }

            // Blank file name would break storage naming and downloads
            if (string.IsNullOrWhiteSpace(file.FileName))
            {
                _logger.LogWarning("Upload rejected because the file name was invalid.");
                return "The uploaded file must have a valid file name.";
            }

            // Only PDF/DOC/DOCX are allowed
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedFileTypes.TryGetValue(extension, out var expectedMimeType))
            {
                _logger.LogWarning("Upload rejected because file extension {Extension} is not allowed.", extension);
                return "Invalid file type. Only PDF, DOC and DOCX files are allowed.";
            }

            // Even if the extension looks fine, still checks the browser/client's declared Content-Type actually matches it,
            // to catch a file that's been renamed to a disguised extension
            if (!string.Equals(file.ContentType, expectedMimeType, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "Upload rejected because MIME type {MimeType} does not match extension {Extension}.",
                    file.ContentType,
                    extension);
                return $"Invalid MIME type. A {extension} file must use content type '{expectedMimeType}'.";
            }

            // All checks passed
            return null;
        }

        // Converts internal storage model into the DTO shape returned to clients
        private static StaffDocumentResponse MapToResponse(StaffDocument document) => new()
        {
            FileName = document.FileName,
            FileExtension = document.FileExtension,
            ContentType = document.ContentType,
            FileSize = document.FileSize,
            UploadedOn = document.UploadedOn,
            ContainerName = document.ContainerName
        };

        // Helper so every validation failure returns a consistently shaped { error: "..." } JSON body
        private static BadRequestObjectResult BadRequest(string message) =>
            new(new { error = message });
    }
}