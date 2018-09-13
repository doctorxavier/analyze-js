using System;
using System.Collections.Generic;
using System.Linq;

using IDB.MW.Business.DocumentTemplate.Enums;
using IDB.Presentation.MVC4.Models.ClientFieldData;

namespace IDB.Presentation.MVC4.Areas.DocumentTemplate.Models
{
    public static class ClientFieldDataMappers
    {
        public static void UpdateDocumentTemplateViewModel(this TemplateViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            foreach (var data in clientFieldData)
            {
                int returnInt;
                DateTime returnDate;
                switch (data.Name)
                {
                    case "templateId":
                        int.TryParse(data.Value, out returnInt);
                        viewModel.DocumentTemplateId = returnInt;
                        break;

                    case "templateDocumentNumber":
                        viewModel.SourceDocumentNumber = data.Value;
                        break;

                    case "templateDocumentUrl":
                        viewModel.FullPathDownloadFolder = data.Value;
                        break;

                    case "templateDescription":
                        viewModel.Description = data.Value;
                        break;

                    case "templateDateCreated":
                        DateTime.TryParse(data.Value, out returnDate);
                        viewModel.DateCreated = returnDate;
                        break;

                    case "templateDateExpired":
                        DateTime.TryParse(data.Value, out returnDate);
                        viewModel.DateExpired = returnDate;
                        break;

                    case "templateParameters":
                        viewModel.Parameters = data.Value;
                        break;

                    case "templateDocumentType":
                        int.TryParse(data.Value, out returnInt);
                        viewModel.DocumentTypeId = returnInt;
                        break;
                }
            }

            var templateFields = new List<TemplateFieldsViewModel>();
            foreach (var fieldData in clientFieldData.Where(o => o.Name.Contains("FieldName")))
            {
                var getId = fieldData.Name.Substring(fieldData.Name.IndexOf('[') + 1).Replace("]", string.Empty);
                var dataType = string.Empty;
                var code = string.Empty;
                var query = string.Empty;
                var manual = false;
                var fieldType = 0;
                foreach (var data in clientFieldData)
                {
                    if (data.Name.Equals("DataType[" + getId + "]"))
                    {
                        int returnInt;
                        int.TryParse(data.Value, out returnInt);
                        dataType = "System." + Enum.GetName(typeof(DataTypeEnum), returnInt);
                    }
                    else if (data.Name.Equals("Query[" + getId + "]"))
                    {
                        query = string.IsNullOrEmpty(data.Value) ? null : data.Value;
                    }
                    else if (data.Name.Equals("FieldCode[" + getId + "]"))
                    {
                        code = data.Value;
                    }
                    else if (data.Name.Equals("isManualField[" + getId + "]"))
                    {
                        manual = Convert.ToBoolean(data.Value);
                    }
                    else if (data.Name.Equals("FieldType[" + getId + "]"))
                    {
                        fieldType = Convert.ToInt16(data.Value);
                    }
                }

                templateFields.Add(new TemplateFieldsViewModel
                {
                    DocumentTemplateFieldId = int.Parse(getId),
                    Name = fieldData.Value,
                    Code = code,
                    ManualField = manual,
                    FieldType = fieldType,
                    Description = "Field for " + fieldData.Value,
                    TypeOfField = dataType,
                    DataSourceQuery = query
                });
            }

            if (templateFields.Any())
            {
                viewModel.TemplateFieldsList = templateFields;
            }
        }
    }
}