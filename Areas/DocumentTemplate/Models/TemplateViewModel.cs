using System;
using System.Collections.Generic;

namespace IDB.Presentation.MVC4.Areas.DocumentTemplate.Models
{
    [Serializable]
    public class TemplateViewModel
    {
        public int DocumentTemplateId { get; set; }

        public int DocumentTypeId { get; set; }

        public string DocumentTypeName { get; set; }

        public string Code { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateExpired { get; set; }

        public string FileName { get; set; }

        public string Description { get; set; }

        public string Parameters { get; set; }

        public string FullPathDownloadFolder { get; set; }

        public string SourceDocumentNumber { get; set; }

        public string SourceDocumentVersion { get; set; }

        public bool IsValid { get; set; }

        public List<TemplateFieldsViewModel> TemplateFieldsList { get; set; }

        public string HtmlDocument { get; set; }
    }
}