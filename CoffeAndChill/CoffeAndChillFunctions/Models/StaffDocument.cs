using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeNChill.Functions.Models
{
    public class StaffDocument
    {
        // Name of the uploaded Staff document file
        public string FileName { get; set; } = string.Empty;

        // File extension of the Staff document file (.pdf, docx, .jpg, etc.)
        public string FileExtension { get; set; } = string.Empty;

        // MIME type
        public string ContentType { get; set; } = string.Empty;

        // Size of Staff Documents file in bytes
        public long FileSize { get; set; }

        // Time and date when the Staff Document was uploaded
        public DateTime UploadedOn { get; set; }

        // Name of the Azure File blob storage
        public string ContainerName { get; set; } = "staff-docs";
    }
}