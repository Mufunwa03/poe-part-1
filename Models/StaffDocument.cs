using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeNChill.Functions.Models
{
    public class StaffDocument
    {
        // Name of the uploaded file 
        public string FileName { get; set; } = string.Empty;

        // File extension (.pdf, docx, .jpg, etc.)
        public string FileExtension { get; set; } = string.Empty;

        // MIME type
        public string ContentType { get; set; } = string.Empty;

        //File size in bytes
        public long FileSize { get; set; }

        //Date and Time when the document was uploaded
        public DateTime UploadedOn { get; set; }

        //Azure File blob storage name
        public string ContainerName { get; set; } = "staff-docs";
    }
}