using CoffeeNChill.Functions.Models;
using Microsoft.AspNetCore.Http;

namespace CoffeeNChill.Functions.Interfaces
{
    public interface IFileStorageService
    {
        // Uploads a document and returns the resulting record
        Task<StaffDocument> UploadDocumentAsync(IFormFile file);

        // Retrieves a document's contents as a stream, or null if not found
        Task<Stream?> DownloadDocumentAsync(string fileName);

        // Removes a document and returns whether the deletion succeeded
        Task<bool> DeleteDocumentAsync(string fileName);

        // Retrieves every stored document
        Task<List<StaffDocument>> GetAllDocumentsAsync();
    }
}




