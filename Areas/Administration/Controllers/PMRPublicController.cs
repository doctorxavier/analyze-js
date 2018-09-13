using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

using Aspose.Pdf;

using IDB.Architecture.Language;
using IDB.Architecture.Logging;
using IDB.MW.Application.Core.Enums;
using IDB.MW.Business.DocumentManagement.Contracts;
using IDB.MW.Business.DocumentManagement.Enums;
using IDB.MW.Business.DocumentManagement.Messages;
using IDB.MW.Domain.Contracts.ModelRepositories.Administration;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.Reports;
using IDB.MW.Domain.Contracts.ModelRepositories.Documents;
using IDB.MW.Domain.Contracts.ModelRepositories.Reports.Generic;
using IDB.MW.Domain.Models.Architecture.MasterDefinitions;
using IDB.MW.Domain.Models.Architecture.Reports.PMRExtraModels.PMRPublic;
using IDB.MW.Domain.Models.Documents;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.Infrastructure.Helpers;
using IDB.MW.Infrastructure.ReportCognos;
using IDB.Presentation.MVC4.Areas.Administration.BusinessLogic;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Report;

namespace IDB.Presentation.MVC4.Areas.Administration.Controllers
{
    public partial class PMRPublicController : BaseController
    {
        private const string TRUSTEE_LIST = "DOCS_USERS='ReadCopyOnly'";
        private const string ENTITY_RELATED_TYPE = "PMR_PUBLIC";
        private const string PDF_FORMAT = "PDF";
        private PMRPublicBusinessLogic _filterBusinesslogic;

        #region ENPOINTS
        private IPMRPublicModelRepository _pmrPublicClient = null;

        public IPMRPublicModelRepository ClientForPMRPublic
        {
            get { return _pmrPublicClient; }
            set { _pmrPublicClient = value; }
        }

        private IReportsGenericRepository _clientForGenericReports = null;

        public IReportsGenericRepository ClientForGenericReports
        {
            get { return _clientForGenericReports; }
            set { _clientForGenericReports = value; }
        }

        private IPmrCycleModelRepository _clientPMRForCycleModel = null;

        public IPmrCycleModelRepository ClientPMRForCycleModel
        {
            get { return _clientPMRForCycleModel; }
            set { _clientPMRForCycleModel = value; }
        }

        private IDocumentManagementService _docManagementService = null;

        public IDocumentManagementService docManagementService
        {
            get { return _docManagementService; }
            set { _docManagementService = value; }
        }

        private IDocumentModelRepository _clientDocumentModel = null;

        public virtual IDocumentModelRepository ClientDocumentModel
        {
            get { return _clientDocumentModel; }
            set { _clientDocumentModel = value; }
        }

        #endregion

        public virtual ActionResult Index(string operationNumber)
        {
            if (!IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR))
            {
                Logger.GetLogger().WriteMessage("PMRPublicController", "No RM Administrator in PMR Public. Security issue?");
                throw new Exception("You don't have permissions to use PMR Public");
            }

            int ini = Environment.TickCount;

            PMRPublicFilterModel filterModel = new PMRPublicFilterModel
            {
                Language = IDBContext.Current.CurrentLanguage
            };

            PMRPublicReportModel model = CreateModel(filterModel);

            int lapsed = Environment.TickCount - ini;

            Logger.GetLogger().WriteDebug("PMRPublicController",
                string.Format("Time lapsed building the model without filtering: {0}s", lapsed / 1000));

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult Index(PMRPublicReportModel model)
        {
            if (!IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR))
            {
                Logger.GetLogger().WriteMessage("PMRPublicController", "No RM Administrator in PMR Public. Security issue?");
                throw new Exception("You don't have permissions to use PMR Public");
            }

            if (!ModelState.IsValid)
                return View(model);

            int ini = Environment.TickCount;

            PMRPublicReportModel resultModel = CreateFilteredModel(model);

            int lapsed = Environment.TickCount - ini;

            Logger.GetLogger().WriteDebug("PMRPublicController",
                string.Format("Time lapsed building the model WITH filtering: {0}s", lapsed / 1000));

            return PartialView(
                "~/Areas/Administration/Views/PMRPublic/Partial/_PMRPublicTablePartial.cshtml",
                resultModel);
        }

        public virtual JsonResult PMRPublicFilterCountries(PMRPublicReportModel model)
        {
            _filterBusinesslogic = new PMRPublicBusinessLogic(); 

            model.Countries = _filterBusinesslogic.FilterCountries(ClientForGenericReports
                .GetCountriesFilter(
                    IDBContext.Current.CurrentLanguage, 
                    model.SelectedCountryDepartments));

            return new JsonResult() { Data = model.Countries }; 
        }

        public virtual JsonResult PMRPublicFilterDivisions(PMRPublicReportModel model)
        {
            if (model.SelectedSectorDepartments.Count > 0)
            {
                model.Divisions = FilterDivisionsForSectorDepartments(model.SelectedSectorDepartments);

                return new JsonResult() { Data = model.Divisions };
            }

            LoadSectorsAndDivisions(model);

            return new JsonResult() { Data = model.Divisions };
        }

        [HttpGet]
        public virtual FileResult GeneratePDFCompositeReport(string reportParametersPDF)
        {
            if (!IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR))
            {
                Logger.GetLogger().WriteMessage(
                    "PMRPublicController",
                    "No RM Administrator in PMR Public. Security issue?");
                throw new Exception("You don't have permissions to use PMR Public");
            }

            try
            {
                int pmrCycleId = GetPmrCycleId(reportParametersPDF);

                string pmrCycleName = GetPmrCycleName(pmrCycleId);

                ReportResponse reportResponse;
                string reportName = string.Empty;

                string cookieDomain = ConfigurationManager.AppSettings["cookie:Domain"].ToString();
                bool cookieHttpOnly = bool.Parse(ConfigurationManager.AppSettings["cookie:HttpOnly"].ToString());
                bool cookieSecure = bool.Parse(ConfigurationManager.AppSettings["cookie:Secure"].ToString());

                Response.AppendCookie(new HttpCookie(
                    PMRGlobalValues.COOKIE_REPORT_READY_PMR_PUBLIC_PDF_COMPOSITE,
                    PMRGlobalValues.COOKIE_REPORT_READY_PMR_PUBLIC_PDF_COMPOSITE)
                {
                    Domain = cookieDomain,
                    HttpOnly = cookieHttpOnly,
                    Secure = cookieSecure
                });
                
                List<string> operationNumbers = GetOperations(reportParametersPDF);

                Document fullDoc = new Document();

                License license = new License();
                string licenseFile = Path.Combine(HttpRuntime.AppDomainAppPath, "Aspose.Total.lic");
                license.SetLicense(licenseFile);

                foreach (var operationNumber in operationNumbers)
                {
                    var reportCognos = new CognosReportBuilder();

                    var reportParameter = new GenerationParameter
                    {
                        OutputFormat = CognosGlobalValues.FORMAT_PDF,
                        ReportParameters = new Dictionary<string, string>
                        {
                            { CognosGlobalValues.P_OPER_NUMBER, operationNumber },
                            { CognosGlobalValues.P_CYCLE, pmrCycleId.ToString() },
                            { CognosGlobalValues.P_SECTION, CognosGlobalValues.ALL_SECTIONS }
                        }
                    };

                    reportResponse = reportCognos.Generate(reportParameter);

                    if (!reportResponse.IsValid)
                        throw new Exception(reportResponse.ErrorMessage);

                    reportName = operationNumber + " " + pmrCycleName + "-Public Report.pdf";

                    if (operationNumbers.Count == 1)
                    {
                        return File(
                            reportResponse.Data,
                            FileContentTypeEnum.Pdf.GetEnumDescription(),
                            reportName);
                    }

                    MemoryStream str = new MemoryStream(reportResponse.Data);
                    Document doc = new Document(str);
                    fullDoc.Pages.Add(doc.Pages);
                }

                using (MemoryStream fullstr = new MemoryStream())
                {
                    fullDoc.Save(fullstr, SaveFormat.Pdf);
                    return File(
                        fullstr.ToArray(),
                        FileContentTypeEnum.Pdf.GetEnumDescription(),
                        "PMR Public Composite.pdf");
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger().WriteError(
                    "PMRPublicController", "Error when generating Cognos report", ex);
                throw;
            }
        }

        [HttpPost]
        public virtual ActionResult GenerateReportsAndSendToIDBDocs(PMRPublicReportModel model)
        {
            if (string.IsNullOrEmpty(model.Operations))
                return null;

            if (!IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR))
            {
                Logger.GetLogger().WriteMessage(
                    "PMRPublicController",
                    "No RM Administrator in PMR Public. Security issue?");
                throw new Exception("You don't have permissions to use PMR Public");
            }

            try
            {
                string[] operationsToSend = model.Operations.Split('|');

                int ini, buildReportTime, createDocTime, addDocToDBTime;

                foreach (string operation in operationsToSend)
                {
                    var reportResponse = new ReportResponse();

                    //Parse the operation information sent
                    string[] operationInfo = operation.Split(';');
                    string operationId = operationInfo[0];
                    string operationNumber = operationInfo[1];

                    ini = Environment.TickCount;

                    var reportCognos = new CognosReportBuilder();

                    var reportParameter = new GenerationParameter
                    {
                        OutputFormat = CognosGlobalValues.FORMAT_PDF,
                        ReportParameters = new Dictionary<string, string>
                    {
                        { CognosGlobalValues.P_OPER_NUMBER, operationNumber },
                        { CognosGlobalValues.P_CYCLE, model.PmrCycleId },
                        { CognosGlobalValues.P_SECTION, CognosGlobalValues.ALL_SECTIONS }
                    }
                    };

                    reportResponse = reportCognos.Generate(reportParameter);

                    if (!reportResponse.IsValid)
                        throw new Exception(reportResponse.ErrorMessage);

                    buildReportTime = Environment.TickCount;

                    string docNum = CreateIDBDocsDocument(
                        new UploadDocumentRequest
                        {
                            OperationNumber = operationNumber,
                            FileStream = reportResponse.Data,
                            FileName =
                                operationNumber + " " + model.PmrCycleName + "-Public Report.pdf",
                            TrusteeList = TRUSTEE_LIST,
                            BusinessAreaCode = BusinessAreaCodeEnum.BA_PMR,
                            AccessInformation = AccessInformationCategoryEnum.PUBLIC,
                            StageCode = "PMR PUBLIC",
                            TypeId = "Report"
                        });

                    createDocTime = Environment.TickCount;

                    if (!CreateDocumentInDatabase(
                        operationId, operationNumber, docNum, model.PmrCycleId))
                    {
                        DeleteDocumentInIDBDocs(new DeleteDocumentRequest
                        {
                            DocumentNumber = docNum,
                            VersionId = string.Empty
                        });

                        throw new Exception(
                            "An error occurred when saving the document in the database." +
                            " Refer to the log for further information");
                    }

                    addDocToDBTime = Environment.TickCount;

                    Logger.GetLogger().WriteDebug("PMRPublicController", string.Format(
                        "--- TOTAL TIME GENERATING PMR DOCUMENT FOR OPERATION {0} ---",
                        operationNumber));
                    Logger.GetLogger().WriteDebug("PMRPublicController", string.Format(
                        "TOTAL: {0}s", (addDocToDBTime - ini) / 1000));
                    Logger.GetLogger().WriteDebug("PMRPublicController", string.Format(
                        "GENERATE REPORT: {0}s", (buildReportTime - ini) / 1000));
                    Logger.GetLogger().WriteDebug("PMRPublicController", string.Format(
                        "CREATE DOC IN IDBDOCS: {0}s", (createDocTime - buildReportTime) / 1000));
                    Logger.GetLogger().WriteDebug("PMRPublicController", string.Format(
                        "CREATE ENTRY IN DATABASE: {0}s", (addDocToDBTime - createDocTime) / 1000));
                }

                PMRPublicReportModel resultModel = CreateFilteredModel(model);

                return PartialView(
                    "~/Areas/Administration/Views/PMRPublic/Partial/_PMRPublicTablePartial.cshtml",
                    resultModel);
            }
            catch (Exception e)
            {
                Logger.GetLogger().WriteError(
                    "PMRPublicController", "Error when uploading Cognos report", e);
                throw;
            }
        }

        public virtual ActionResult GetDocument(string docNum)
        {
            try
            {
                return Redirect(MW.Domain.EntityHelpers.DownloadDocumentHelper.GetDocumentLink(docNum));
            }
            catch (Exception e)
            {
                Logger.GetLogger().WriteError(
                    "PMRPublicController", "Error when getting report: ", e);
                throw;
            }
        }

        public virtual ActionResult GetDocument(DownloadRequest request)
        {
            try
            {
                var document = _docManagementService.Download(request);

                if (document == null)
                    throw new Exception("The file could not be retrieved.");

                CommonDocument documentObject = new CommonDocument();
                var contentType = documentObject.GetContentType(document.Document.FileName);
                return File(document.Document.File, contentType, document.Document.FileName);
            }
            catch (Exception e)
            {
                Logger.GetLogger().WriteError(
                    "PMRPublicController", "Error when getting report: ", e);
                throw;
            }
        }

        [HttpPost]
        public virtual ActionResult AuthorizeReports(PMRPublicReportModel model)
        {
            if (string.IsNullOrEmpty(model.AuthorizeDocuments))
                return null;

            if (!IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR))
            {
                Logger.GetLogger().WriteMessage(
                    "PMRPublicController",
                    "No RM Administrator in PMR Public. Security issue?");
                throw new Exception("You don't have permissions to use PMR Public");
            }

            string[] documentsToSend = model.AuthorizeDocuments.Split('|');

            foreach (string document in documentsToSend)
            {
                int ini = Environment.TickCount;

                AuthorizeDocumentInIDBDocs(new AuthorizeDocumentRequest
                { 
                    DocumentNumber = document
                });

                int authTime = Environment.TickCount;

                AuthorizeDocumentInDatabase(document);

                int dbTime = Environment.TickCount;

                Logger.GetLogger().WriteDebug(
                    "PMRPublicController",
                    "--- TOTAL TIME AUTHORIZING PMR DOCUMENT ---");
                Logger.GetLogger().WriteDebug("PMRPublicController", string.Format(
                    "TOTAL: {0}s", (dbTime - ini) / 1000));
                Logger.GetLogger().WriteDebug("PMRPublicController", string.Format(
                    "AUTH IN IDBDOCS: {0}s", (authTime - ini) / 1000));
                Logger.GetLogger().WriteDebug("PMRPublicController", string.Format(
                    "AUTH IN DB: {0}s", (dbTime - authTime) / 1000));
            }

            PMRPublicReportModel resultModel = CreateFilteredModel(model);

            return PartialView(
                "~/Areas/Administration/Views/PMRPublic/Partial/_PMRPublicTablePartial.cshtml",
                resultModel);
        }

        [HttpPost]
        public virtual ActionResult DeleteDocument(PMRPublicReportModel model)
        {
            if (string.IsNullOrEmpty(model.DocumentNumber))
                return null;

            if (!IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR))
            {
                Logger.GetLogger().WriteMessage("PMRPublicController", "No RM Administrator in PMR Public. Security issue?");
                throw new Exception("You don't have permissions to use PMR Public");
            }

            int ini = Environment.TickCount;

            DeleteDocumentInIDBDocs(new DeleteDocumentRequest
            { 
                DocumentNumber = model.DocumentNumber,
                VersionId = string.Empty
            });

            int deleteIDBTime = Environment.TickCount;

            DeleteDocumentInDatabase(model.DocumentNumber);

            int deleteDBTime = Environment.TickCount;

            Logger.GetLogger().WriteDebug("PMRPublicController", "--- TOTAL TIME DELETING PMR DOCUMENT ---");
            Logger.GetLogger().WriteDebug("PMRPublicController", string.Format(
                "TOTAL: {0}s", (deleteDBTime - ini) / 1000));
            Logger.GetLogger().WriteDebug("PMRPublicController", string.Format(
                "DELETE IN IDBDOCS: {0}s", (deleteIDBTime - ini) / 1000));
            Logger.GetLogger().WriteDebug("PMRPublicController", string.Format(
                "DELETE IN DB: {0}s", (deleteDBTime - deleteIDBTime) / 1000));

            PMRPublicReportModel resultModel = CreateFilteredModel(model);

            return PartialView(
                "~/Areas/Administration/Views/PMRPublic/Partial/_PMRPublicTablePartial.cshtml",
                resultModel);
        }

        private PMRPublicReportModel CreateFilteredModel(PMRPublicReportModel model)
        {
            PMRPublicFilterModel filterModel = new PMRPublicFilterModel
            {
                Language = IDBContext.Current.CurrentLanguage,
                CountryDepartment = model.SelectedCountryDepartments.Count == 0 ? null 
                    : string.Join(",", model.SelectedCountryDepartments),
                SectorDepartment = model.SelectedSectorDepartments.Count == 0 ? null 
                    : string.Join(",", model.SelectedSectorDepartments),
                PMRCycle = model.PMRCycles[0].PmrCycleId,
                PMRValidationStage = model.SelectedPMRValidationStages.Count == 0 ? null
                    : string.Join(",", model.SelectedPMRValidationStages),
                OperationOverallStage = model.SelectedOperationOverallStages.Count == 0 ? null
                    : string.Join(",", model.SelectedOperationOverallStages),
                Country = model.SelectedCountries.Count == 0 ? null
                    : string.Join(",", model.SelectedCountries),
                Division = model.SelectedDivisions.Count == 0 ? null
                    : string.Join(",", model.SelectedDivisions),
                Uploaded = model.IsActive[0].Value == -1 ? null : model.IsActive[0].Value,
                Authorize = model.IsAuthorize[0].Value == -1 ? null : model.IsAuthorize[0].Value
            };

            return CreateModel(filterModel);
        }

        private PMRPublicReportModel CreateModel(PMRPublicFilterModel filterModel)
        {
            PMRPublicReportModel result = new PMRPublicReportModel();
            string language = IDBContext.Current.CurrentLanguage;
            _filterBusinesslogic = new PMRPublicBusinessLogic();

            result.CountryDepartments = 
                _filterBusinesslogic.FilterCountryDepartments(
                ClientForGenericReports.GetCountryDepartments(language));

            result.PMRCycles = ClientPMRForCycleModel
                .GetPMRCycles(language).OrderByDescending(x => x.StartDate).ToList();

            // don't include DRAFT validation stages, no necessary to list them
            result.PMRValidationStages = 
                _filterBusinesslogic.FilterPMRValidationStages(
                ClientForGenericReports.GetPMRValidationStages(language));

            result.OperationOverallStages = 
                ClientForGenericReports.GetOperationOverallStages(language)
                .OrderBy(x => x.Name).ToList();

            result.Countries = _filterBusinesslogic.FilterCountries(
                ClientForGenericReports.GetCountries(language));

            LoadSectorsAndDivisions(result);

            List<TriStateFilter> triState = new List<TriStateFilter>();
            triState.Add(new TriStateFilter { Name = Localization.GetText("All"), Value = -1 });
            triState.Add(new TriStateFilter { Name = Localization.GetText("Yes"), Value = 1 });
            triState.Add(new TriStateFilter { Name = Localization.GetText("No"), Value = 0 });

            result.IsActive = triState;
            result.IsAuthorize = triState;

            result.Table = ClientForPMRPublic.GetPMRPublicTableData(filterModel);

            return result;
        }

        private IList<ConvergenceMasterDataModel> FilterDivisionsForSectorDepartments(
            List<int> sectorIds)
        {
            _filterBusinesslogic = new PMRPublicBusinessLogic(); 

            return _filterBusinesslogic.FilterDivisions(
                ClientForGenericReports.GetDivisionFilter(
                IDBContext.Current.CurrentLanguage, sectorIds));
        }

        private void LoadSectorsAndDivisions(PMRPublicReportModel model)
        {
            _filterBusinesslogic = new PMRPublicBusinessLogic(); 

            model.SectorDepartments = 
                _filterBusinesslogic.FilterSectorDepartment(ClientForGenericReports
                    .GetSectorDepartment(IDBContext.Current.CurrentLanguage));

            List<int> sectorIds = 
                model.SectorDepartments.Select(x => x.ConvergenceMasterDataId).ToList();

            model.Divisions = FilterDivisionsForSectorDepartments(sectorIds);
        }

        private byte[] GetReport(string operationId, string pmrCycle)
        {
            string url = string.Format("&OPERATION={0}&CYCLE={1}&LANG={2}",
                    operationId, 
                    pmrCycle, 
                    IDBContext.Current.CurrentLanguage);
            try
            {
                return ReportBuilder.DownloadReport(
                    ConfigurationManager.AppSettings["PMRPublicReport"], url, PDF_FORMAT);
            }
            catch (Exception e)
            {
                Logger.GetLogger().WriteError("PMRPublicController", "Error when building the report: ", e);
                throw;
            }
        }

        private string CreateIDBDocsDocument(UploadDocumentRequest request)
        {
            Logger.GetLogger().WriteDebug("PMRPublicController",
                   "Report byte array size: " + request.FileStream.Length);
            try
            {
                var uploadResponse = _docManagementService.Upload(request);
                if (!uploadResponse.IsValid)
                {
                    throw new Exception(
                        "The document was not properly generated; returned a null doc reference");
                }        
                       
                Logger.GetLogger().WriteDebug("PMRPublicController",
                    "Sending doc with criteria: " + uploadResponse.Criteria);

                return uploadResponse.DocumentNumber;
            }
            catch (Exception e)
            {
                Logger.GetLogger().WriteError("PMRPublicController", "Error when sending the document: ", e);
                throw;
            }
        }

        private bool CreateDocumentInDatabase(
            string opId, string opNumber, string docNum, string pmrCycleId)
        {
            DocumentsViewModel model = new DocumentsViewModel()
            {
                OperationId = int.Parse(opId),
                OperationNumber = opNumber,
                MainOperationNumber = opNumber,
                EntityRelated = ENTITY_RELATED_TYPE,
                IDBDocNumber = docNum,
                PmrCycleId = int.Parse(pmrCycleId)
            };

            try
            {
                return ClientDocumentModel.AddDocument(model, IDBContext.Current.UserName);
            }
            catch (Exception e)
            {
                Logger.GetLogger().WriteError("PMRPublicController",
                    "An error ocurred when adding the document entry in the database. Error: ", 
                    e);
                return false;
            }
        }

        private void AuthorizeDocumentInIDBDocs(string document)
        {
            try
            {
                var authorizeResponse = _docManagementService.Authorize(new AuthorizeDocumentRequest() 
                {
                    DocumentNumber = document
                });

                if (!authorizeResponse.IsValid)
                {
                    throw new HttpException("The authorization process did not finish properly in IDBDocs"); 
                }               
            }
            catch (Exception e)
            {
                Logger.GetLogger().WriteError(
                    "PMRPublicController", "Error when authorizing report in IDBDocs: ", e);
                throw;
            }
        }

        private void AuthorizeDocumentInIDBDocs(AuthorizeDocumentRequest request)
        {
            try
            {
                var authRequest = _docManagementService.Authorize(request);
                if (!authRequest.IsValid)
                {
                    throw new Exception("The authorization process did not success"); 
                }
            }
            catch (Exception e)
            {
                Logger.GetLogger().WriteError(
                    "PMRPublicController", "Error when authorizing report: ", e);
                throw;
            }
        }

        private void AuthorizeDocumentInDatabase(string document)
        {
            try
            {
                if (!ClientDocumentModel.UpdateDocument(
                    document, "AUTHORIZE", IDBContext.Current.UserName))
                {
                    throw new Exception("The document was not authorized in database!");
                }
            }
            catch (Exception e)
            {
                Logger.GetLogger().WriteError(
                    "PMRPublicController", 
                    "Error when authorizing report in DATABASE: ", 
                    e);
                Logger.GetLogger().WriteError("PMRPublicController", 
                    string.Empty, 
                    new Exception("THIS ERROR LEADS TO INCONSISTENCY BETWEEN THE DATABASE AND IDBDOCS!!"));
                throw;
            }
        }

        private void DeleteDocumentInIDBDocs(DeleteDocumentRequest request)
        {
            try
            {
                var response = _docManagementService.Delete(request);
                if (!response.IsValid)
                {
                    throw new Exception("The file was not deleted");
                }
            }
            catch (Exception e)
            {
                Logger.GetLogger().WriteError(
                    "PMRPublicController", "Error when deleting report in IDBDocs: ", e);
                throw;
            }
        }

        private void DeleteDocumentInIDBDocs(string document)
        {
            try
            {
                var deleteResponse = _docManagementService.Delete(new DeleteDocumentRequest() 
                {
                    DocumentNumber = document
                });

                if (!deleteResponse.IsValid)
                {
                    throw new Exception("The file was not deleted in IDBDocs"); 
                }   
            }
            catch (Exception e)
            {
                Logger.GetLogger().WriteError(
                    "PMRPublicController", "Error when deleting report in IDBDocs: ", e);
                throw;
            }
        }

        private void DeleteDocumentInDatabase(string document)
        {
            try
            {
                if (!ClientDocumentModel.UpdateDocument(
                    document, "DELETE", IDBContext.Current.UserName))
                {
                    throw new Exception("The document was not deactivated in database!");
                }
            }
            catch (Exception e)
            {
                Logger.GetLogger().WriteError(
                    "PMRPublicController", "Error when deleting report in DATABASE: ", e);
                Logger.GetLogger().WriteError("PMRPublicController", 
                    string.Empty, 
                    new Exception("THIS ERROR LEADS TO INCONSISTENCY BETWEEN THE DATABASE AND IDBDOCS!!"));
                throw;
            }
        }

        private string GetLanguage()
        {
            if (IDBContext.Current.CurrentLanguage.ToLower() == "es")
            {
                return "SPANISH";
            }

            if (IDBContext.Current.CurrentLanguage.ToLower() == "pt")
            {
                return "PORTUGUESE";
            }

            if (IDBContext.Current.CurrentLanguage.ToLower() == "fr")
            {
                return "FRENCH";
            }

            return "ENGLISH";
        }

        private List<string> GetOperations(string reportParametersPDF)
        {
            List<string> operationNumberList = new List<string>();

            string[] reportParams = reportParametersPDF.Split('&');

            foreach (string reportParam in reportParams)
            {
                if (string.IsNullOrEmpty(reportParam))
                    continue;

                string[] keyValuePair = reportParam.Split('=');

                if (keyValuePair[0] == "OPERATION")
                {
                    operationNumberList.Add(keyValuePair[1]);
                }
            }

            return operationNumberList;
        }

        private int GetPmrCycleId(string reportParametersPDF)
        {
            int pmrCycleId = 0;

            string[] reportParams = reportParametersPDF.Split('&');

            foreach (string reportParam in reportParams)
            {
                if (string.IsNullOrEmpty(reportParam))
                    continue;

                string[] keyValuePair = reportParam.Split('=');

                if (keyValuePair[0] == "CYCLE")
                {
                    pmrCycleId = Convert.ToInt32(keyValuePair[1]);
                }
            }

            return pmrCycleId;
        }

        private string GetPmrCycleName(int pmrCycleId)
        {
            var pmrCycles = ClientPMRForCycleModel
                .GetPMRCycles(IDBContext.Current.CurrentLanguage)
                .OrderByDescending(x => x.StartDate)
                .ToList();

            return pmrCycles
                .Where(pc => pc.PmrCycleId == pmrCycleId)
                .Select(p => p.PmrCycleName)
                .FirstOrDefault();
        }
    }
}