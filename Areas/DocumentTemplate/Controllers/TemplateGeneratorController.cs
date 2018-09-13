using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using IDB.Architecture.Language;
using IDB.Architecture.Logging;
using IDB.MW.Application.OPUSModule.Services.OperationDataService;
using IDB.MW.Business.DocumentManagement.Messages;
using IDB.MW.Business.DocumentTemplate.DTOs;
using IDB.MW.Business.DocumentTemplate.Messages;
using IDB.MW.Business.DocumentTemplate.Services;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Areas.DocumentTemplate.Models;
using IDB.Presentation.MVC4.Helpers;
using MvcExtensions;

namespace IDB.Presentation.MVC4.Areas.DocumentTemplate.Controllers
{
    public partial class TemplateGeneratorController : MVC4.Controllers.ConfluenceController
    {
        #region Constants

        private const string DocumentType = "DOCUMENT_TYPE";

        #endregion

        #region Fields

        private readonly IDocumentTemplateServices _documentTemplateServices;
        private readonly IOperationDataService _operationDataService;

        #endregion

        #region Constructors

        public TemplateGeneratorController(IDocumentTemplateServices documentTemplateServices,
            IOperationDataService operationDataService)
        {
            _documentTemplateServices = documentTemplateServices;
            _operationDataService = operationDataService;
        }

        #endregion

        #region View Methods

        // GET: DocumentTemplate/Index
        public virtual ActionResult Index()
        {
            var model = _documentTemplateServices.GetDocumentTemplatesResponse();
            var masterDataList = _documentTemplateServices.GetConvergenceMasterDataResponse(DocumentType);
            if (masterDataList.IsValid)
            {
                ViewBag.DocumentTypeList = new ViewModelMapperHelper().DocumentTypeList(masterDataList.MasterDataList);
            }

            ViewBag.StatusList = new ViewModelMapperHelper().GetStatusList();

            return model.IsValid ? View(model.ListTemplatesViewModel) : View();
        }

        public virtual ActionResult FilterTemplates(string status, string documentType, string dateFrom, string dateTo)
        {
            var templateList = _documentTemplateServices.GetDocumentTemplatesResponse();
            if (templateList.IsValid)
            {
                var responseList = templateList.ListTemplatesViewModel;

                if (!string.IsNullOrEmpty(status))
                {
                    responseList = responseList.Where(t => t.IsValid == bool.Parse(status)).ToList();
                }

                if (!string.IsNullOrEmpty(documentType))
                {
                    responseList = responseList.Where(t => t.DocumentTypeId == int.Parse(documentType)).ToList();
                }

                responseList = responseList.Where(t => t.DateCreated >= DateTime.Parse(dateFrom) &&
                                                       t.DateCreated <= DateTime.Parse(dateTo)).ToList();

                templateList.ListTemplatesViewModel = responseList.ToList();
                return PartialView("Partials/TemplateList", templateList.ListTemplatesViewModel);
            }

            return PartialView("Partials/TemplateList");
        }

        public virtual ActionResult GenerateDocument(string templateCode, string operationNumber, Dictionary<string, string> parameters, string protect)
        {
            if (string.IsNullOrEmpty(templateCode) || string.IsNullOrEmpty(operationNumber))
                return Json(Localization.GetText("OP.DTG.TemplateGen.Message.MissingParameters"),
                    JsonRequestBehavior.AllowGet);

            if (!_operationDataService.GetOperationBasicData(operationNumber).IsValid)
            {
                return Json(Localization.GetText("OP.DTG.TemplateGen.Message.InvalidOperationNumber"),
                    JsonRequestBehavior.AllowGet);
            }

            var templateUrl = ConfigurationManager.AppSettings.Get("DocumentTemplate_TemplateUrl");
            var templateFolder = ConfigurationManager.AppSettings.Get("DocumentTemplate_TemplateFolder");
            var generatedFolder = ConfigurationManager.AppSettings.Get("DocumentTemplate_GeneratedDocuments");
            var response = _documentTemplateServices.GenerateDocumentResponse(new GenerateDocumentRequest
            {
                TemplateCode = templateCode,
                DataConnectionTemplate = new DataConnectionRequest
                {
                    OperationNumber = operationNumber,
                    Site = templateUrl,
                    Library = templateFolder,
                    CurrentUser = IDBContext.Current.UserName,
                    IsVer = false,
                    Parameters = parameters
                },
                DataConnectionGenerated = new DataConnectionRequest
                {
                    OperationNumber = operationNumber,
                    RelativePath = generatedFolder,
                    CurrentUser = IDBContext.Current.UserName,
                    IsVer = true,
                    Parameters = parameters
                },
                Protected = "true".Equals(protect),
                FileName = "001046-TEMPLATE_TC_DOCUMENT_SPANISH"
            });

            return Json(response.IsValid ? response.GeneratedDocument.UrlGenerated : response.ErrorMessage,
                JsonRequestBehavior.AllowGet);
        }

        // GET: DocumentTemplate/TemplateGenerator
        public virtual ActionResult TemplateGenerator(int templateId = 0, bool clone = false)
        {
            var masterDataList = _documentTemplateServices.GetConvergenceMasterDataResponse(DocumentType);
            ViewBag.DocumentTypeList = new ViewModelMapperHelper().DocumentTypeList(masterDataList.MasterDataList);

            var model = new TemplateViewModel
            {
                TemplateFieldsList = null,
                DocumentTemplateId = templateId,
                Description = string.Empty,
                DateCreated = DateTime.Today,
                DateExpired = DateTime.Today.AddMonths(1)
            };

            if (templateId > 0)
            {
                var template = _documentTemplateServices.GetTemplateResponse(new DocumentTemplateRequest
                {                    
                    TemplateId = templateId
                });
                if (template.IsValid)
                {
                    var templateResponse = template.DocumentTemplateViewModel;
                    var sharePointUrl = ConfigurationManager.AppSettings.Get("DocumentTemplate_TemplateUrl");
                    var templateFolder = ConfigurationManager.AppSettings.Get("DocumentTemplate_TemplateFolder");
                    Logger.GetLogger()
                    .WriteDebug("DOCUMENT TEMPLATE SERVICE", "TemplateGeneratorController - Method: TemplateGenerator");
                    var file =
                        _documentTemplateServices.DownloadBaseDocumentResponse(new DownloadDocumentRequest
                        {
                            DataConnection = new DataConnectionRequest
                            {
                                CurrentUser = IDBContext.Current.UserName, 
                                Site = sharePointUrl, 
                                Library = templateFolder, 
                                IsVer = false
                            },              
                            SharePointDocumentId = templateResponse.SourceDocumentNumber
                        });
                    Logger.GetLogger()
                        .WriteDebug("DOCUMENT TEMPLATE SERVICE",
                            string.Format(
                                "TemplateGeneratorController - Method: TemplateGenerator file.FileName:{0}, file.IsValid:{1}, file.ErrorMessage:{2}",
                                file.FileName, 
                                file.IsValid, 
                                file.ErrorMessage));

                    if (file.IsValid)
                    {
                        Logger.GetLogger()
                            .WriteDebug("DOCUMENT TEMPLATE SERVICE",
                                string.Format(
                                    "TemplateGeneratorController - Method: TemplateGenerator: Enter IF file.Isvalid:{0}",
                                    file.IsValid));
                        model.FileName = file.FileName;
                        model.HtmlDocument =
                            _documentTemplateServices.ConvertDocToHtmlResponse(new DocumentTemplateRequest
                            {
                                DocumentStream = new MemoryStream(file.File)
                            }).HtmlDocument;
                    }
                    else
                    {
                        Logger.GetLogger()
                            .WriteDebug("DOCUMENT TEMPLATE SERVICE",
                                string.Format(
                                    "TemplateGeneratorController - Method: TemplateGenerator: Enter ELSE file.Isvalid:{0}",
                                    file.IsValid));
                        return View();
                    }

                    Logger.GetLogger()
                    .WriteDebug("DOCUMENT TEMPLATE SERVICE", "TemplateGeneratorController - Method: TemplateGenerator END IF");
                    model.Code = templateResponse.Code;
                    model.Description = templateResponse.Description;
                    model.DateCreated = templateResponse.DateCreated;
                    model.DateExpired = templateResponse.DateExpired;
                    model.SourceDocumentNumber = templateResponse.SourceDocumentNumber;
                    model.FullPathDownloadFolder = templateResponse.FullPathDownloadFolder;
                    model.DocumentTypeId = templateResponse.DocumentTypeId;
                    model.DocumentTypeName = templateResponse.DocumentTypeName;
                    model.Parameters = templateResponse.Parameters;

                    var templateFields =
                        _documentTemplateServices.GetTemplateFieldsByIdTemplateResponse(new DocumentTemplateRequest
                        {
                            TemplateId = templateId
                        });
                    if (templateFields.IsValid)
                    {
                        Logger.GetLogger()
                            .WriteDebug("DOCUMENT TEMPLATE SERVICE",
                                string.Format(
                                    "TemplateGeneratorController - Method: TemplateGenerator: templateFields.IsValid:{0}",
                                    templateFields.IsValid));
                        model.TemplateFieldsList = new ViewModelMapperHelper().TemplateFieldsDto(
                            templateFields.ListTemplateFieldsViewModel);
                    }

                    ViewBag.DataTypeList = new ViewModelMapperHelper().DataTypeList();
                }
            }

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            Logger.GetLogger()
                .WriteDebug("DOCUMENT TEMPLATE SERVICE",
                    string.Format(
                        "TemplateGeneratorController - Method: TemplateGenerator: model.FileName:{0}, model.Code:{1}, model.DocumentTemplateId:{2}, model.IsValid:{3}, model.Description:{4}",
                        model.FileName, 
                        model.Code, 
                        model.DocumentTemplateId, 
                        model.IsValid, 
                        model.Description));

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult LoadDocumentResult()
        {
            var model = new TemplateViewModel
            {
                TemplateFieldsList = new List<TemplateFieldsViewModel>()
            };
            var httpFile = Request.Files[0];
            if (httpFile != null)
            {
                if (httpFile.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                {
                    var tagsFields = _documentTemplateServices.ReadDocTagsResponse(new DocumentTemplateRequest
                    {
                        DocumentStream = httpFile.InputStream
                    });
                    if (!tagsFields.IsValid)
                    {
                        return Json(tagsFields, JsonRequestBehavior.AllowGet);
                    }

                    model.TemplateFieldsList =
                        new ViewModelMapperHelper().TemplateFields(tagsFields.ListTemplateFieldsViewModel);
                    model.HtmlDocument = _documentTemplateServices.ConvertDocToHtmlResponse(new DocumentTemplateRequest
                    {
                        DocumentStream = httpFile.InputStream
                    }).HtmlDocument;
                }
                
                ViewBag.DataTypeList = new ViewModelMapperHelper().DataTypeList();
                model.FileName = Guid.NewGuid() + Path.GetExtension(httpFile.FileName);
                model.FullPathDownloadFolder = httpFile.FileName;
                httpFile.SaveAs(Path.GetTempPath() + @"\" + model.FileName);
            }

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            return PartialView("Partials/Template", model);
        }

        [HttpPost]
        public virtual JsonResult ValidateSqlScript(string sqlQuery, string opNumber)
        {
            if (string.IsNullOrEmpty(sqlQuery))
            {
                return Json(Localization.GetText("OP.DTG.TemplateGen.Message.QueryEmpty"), JsonRequestBehavior.AllowGet);
            }

            var response = new ViewModelMapperHelper().VerifySqlCommands(sqlQuery)
                ? new ViewModelMapperHelper().TestSqlQuery(sqlQuery, opNumber)
                : Localization.GetText("OP.DTG.TemplateGen.Message.QueryReservedWords");

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Save Methods

        public virtual JsonResult SaveTemplate()
        {
            var response = new DocumentTemplateResponse
            {
                IsValid = true
            };
            try
            {
                var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
                var viewModelTemplate =
                    PageSerializationHelper.DeserializeObject<TemplateViewModel>(jsonDataRequest.SerializedData);
                viewModelTemplate.UpdateDocumentTemplateViewModel(jsonDataRequest.ClientFieldData);
                viewModelTemplate.SourceDocumentVersion = "1";
                viewModelTemplate.IsValid =
                    DateTime.Compare(viewModelTemplate.DateCreated, viewModelTemplate.DateExpired) <= 0;

                var documentUpload = new UploadRequest();
                if (viewModelTemplate.DocumentTemplateId <= 0)
                {
                    var tempFileName =
                        jsonDataRequest.ClientFieldData.First(e => "templateTempFileName".Equals(e.Name)).Value;
                    var fileName = jsonDataRequest.ClientFieldData.First(e => "templateFileName".Equals(e.Name)).Value;
                    documentUpload = new UploadRequest
                    {
                        File = System.IO.File.ReadAllBytes(Path.GetTempPath() + @"\" + tempFileName),
                        FileName = fileName
                    };
                    System.IO.File.Delete(Path.GetTempPath() + @"\" + tempFileName);
                }

                var sharePointUrl = ConfigurationManager.AppSettings.Get("DocumentTemplate_TemplateUrl");
                var templateFolder = ConfigurationManager.AppSettings.Get("DocumentTemplate_TemplateFolder");

                if (viewModelTemplate.TemplateFieldsList == null)
                {
                    viewModelTemplate.TemplateFieldsList = new List<TemplateFieldsViewModel>();
                }

                var templateDto = new ViewModelMapperHelper().TemplateDto(viewModelTemplate);
                templateDto.DataConnection = new DataConnectionRequest
                {
                    Site = sharePointUrl,
                    Library = templateFolder,
                    CurrentUser = IDBContext.Current.UserName,
                    IsVer = false
                };

                response = _documentTemplateServices.SaveDocumentTemplateResponse(documentUpload, templateDto);
                return Json(response);
            }
            catch (Exception e)
            {
                response.IsValid = false;
                response.ErrorMessage = e.GetBaseException().Message;
                return Json(response);
            }
        }

        #endregion
    }
}
