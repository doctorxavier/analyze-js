using System;

namespace IDB.Presentation.MVC4.Areas.DocumentTemplate.Models
{
    [Serializable]
    public class TemplateFieldsViewModel
    {
        public int DocumentTemplateFieldId { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public bool ManualField { get; set; }

        public int FieldType { get; set; }

        public string Description { get; set; }

        public string TypeOfField { get; set; }

        public string DataSourceQuery { get; set; }

        public string RepositoryName { get; set; }

        public string TableFieldName { get; set; }
    }
}