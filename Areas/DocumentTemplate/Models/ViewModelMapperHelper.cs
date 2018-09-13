using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.DataLayer;
using IDB.Architecture.Language;
using IDB.MW.Business.DocumentTemplate.DTOs;
using IDB.MW.Business.DocumentTemplate.Enums;
using IDB.MW.Domain.Entities;
using IDB.MW.Domain.EntityHelpers;

namespace IDB.Presentation.MVC4.Areas.DocumentTemplate.Models
{
    public class ViewModelMapperHelper
    {
        public List<TemplateFieldsViewModel> TemplateFields(List<DocumentTemplateFieldsDTO> templateFieldsDto)
        {
            return templateFieldsDto.Select(fields => new TemplateFieldsViewModel
            {
                Name = fields.Name,
                Code = fields.Code,
                ManualField = fields.ManualField,
                FieldType = fields.FieldType,
                Description = fields.Description,
                DocumentTemplateFieldId = fields.DocumentTemplateFieldId,
                TypeOfField = fields.TypeOfField,
                RepositoryName = fields.RepositoryName,
                DataSourceQuery = fields.DataSourceQuery,
                TableFieldName = fields.TableFieldName
            }).ToList();
        }

        public List<DocumentTemplateFieldsDTO> TemplateFieldsDto(List<TemplateFieldsViewModel> viewModel)
        {
            return viewModel.Select(fields => new DocumentTemplateFieldsDTO
            {
                Name = fields.Name,
                Code = fields.Code,
                ManualField = fields.ManualField,
                FieldType = fields.FieldType,
                DataSourceQuery = fields.DataSourceQuery,
                Description = fields.Description,
                DocumentTemplateFieldId = fields.DocumentTemplateFieldId,
                RepositoryName = fields.RepositoryName,
                TableFieldName = fields.TableFieldName,
                TypeOfField = fields.TypeOfField
            }).ToList();
        }

        public DocumentTemplateDTO TemplateDto(TemplateViewModel viewModel)
        {
            return new DocumentTemplateDTO
            {
                DocumentTemplateId = viewModel.DocumentTemplateId,
                Code = viewModel.Code,
                DateCreated = viewModel.DateCreated,
                DateExpired = viewModel.DateExpired,
                Description = viewModel.Description,
                DocumentTypeId = viewModel.DocumentTypeId,
                IsValid = viewModel.IsValid,
                Parameters = viewModel.Parameters,
                FullPathDownloadFolder = viewModel.FullPathDownloadFolder,
                SourceDocumentNumber = viewModel.SourceDocumentNumber,
                SourceDocumentVersion = viewModel.SourceDocumentVersion,
                TemplateFieldsList = TemplateFieldsDto(viewModel.TemplateFieldsList)
            };
        }

        public List<TemplateFieldsViewModel> TemplateFieldsDto(List<DocumentTemplateFieldsDTO> viewModel)
        {
            return viewModel.Select(fields => new TemplateFieldsViewModel
            {
                Name = fields.Name,
                Code = fields.Code,
                ManualField = fields.ManualField,
                FieldType = fields.FieldType,
                DataSourceQuery = fields.DataSourceQuery,
                Description = fields.Description,
                DocumentTemplateFieldId = fields.DocumentTemplateFieldId,
                RepositoryName = fields.RepositoryName,
                TableFieldName = fields.TableFieldName,
                TypeOfField = (short)((DataTypeEnum)Enum.Parse(typeof(DataTypeEnum),
                    fields.TypeOfField.Replace("System.", string.Empty))) + string.Empty
            }).ToList();
        }

        public List<SelectListItem> DataTypeList()
        {
            return (from short dataType in Enum.GetValues(typeof(DataTypeEnum))
                    select new SelectListItem
                    {
                        Text = Enum.GetName(typeof(DataTypeEnum), dataType),
                        Value = dataType.ToString()
                    }).ToList();
        }

        public List<SelectListItem> FieldTypeList()
        {
            return (from short dataType in Enum.GetValues(typeof(FieldTypeEnum))
                    select new SelectListItem
                    {
                        Text = Enum.GetName(typeof(FieldTypeEnum), dataType),
                        Value = dataType.ToString()
                    }).ToList();
        }

        public List<SelectListItem> DocumentTypeList(List<ConvergenceMasterData> masterDataList)
        {
            var typesList = new List<SelectListItem>();
            if (masterDataList != null)
            {
                typesList = masterDataList.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.ConvergenceMasterDataId.ToString(),
                    Text = o.GetNameLanguage(Localization.CurrentLanguage)
                }).ToList();
            }

            return typesList;
        }

        public List<SelectListItem> GetStatusList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = Localization.GetText("OP.DTG.Homepage.Status.Active"),
                    Value = "true"
                },
                new SelectListItem
                {
                    Text = Localization.GetText("OP.DTG.Homepage.Status.Inactive"),
                    Value = "false"
                }
            };
        } 

        public bool VerifySqlCommands(string sqlQuery)
        {
            var commandList = new List<string> { "UPDATE ", "DELETE ", "INSERT ", "CREATE ", "ALTER ", "DROP " };
            return !commandList.Any(sqlQuery.ToUpper().Contains);
        }

        public string TestSqlQuery(string sqlQuery, string opNumber)
        {
            try
            {
                var sqlResult = string.Empty;
                var parameters = new[]
                {
                    new SqlParameter("@OperationNumber", opNumber)
                };

                QueryRunner.RunSelectQuery(
                    sqlQuery, 
                    parameters, 
                    delegate(SqlDataReader reader)
                    {
                        sqlResult = reader[0] + string.Empty;
                    });

                return string.IsNullOrEmpty(sqlResult) ?
                    Localization.GetText("OP.DTG.TemplateGen.Message.EmptyResult") : string.Empty;
            }
            catch (Exception ex)
            {
                return ex.GetBaseException().Message;
            }
        }
    }
}