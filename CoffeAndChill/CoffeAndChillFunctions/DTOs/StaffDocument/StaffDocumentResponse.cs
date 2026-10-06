using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeNChill.Functions.DTOs.StaffDocuments
{
    public class StaffDocumentResponse
    {
        // Name of the uploaded document
        public string FileName { get; set;  } = string.Empty;

        // File extension
        public string FileExtension { get; set; } = string.Empty;

        // MIME type
        public string ContentType { get; set; } = string.Empty;

        // Size of the file in bytes
        public long FileSize { get; set; }

        // Timestamp of when the document was uploaded
        public DateTime UploadedOn { get; set; }

        // Name of the Azure blob storage container 
        public string ContainerName { get; set; } = string.Empty;



    }
}
