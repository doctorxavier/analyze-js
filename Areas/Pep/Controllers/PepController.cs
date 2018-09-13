using System;
using System.Configuration;
using System.IO;
using System.Web.Mvc;

using IDB.Architecture.Cache;
using IDB.Architecture.Language;
using IDB.Architecture.Logging;
using IDB.MW.Application.PEPModule.Messages;
using IDB.MW.Application.PEPModule.Messages.Excel;
using IDB.MW.Application.PEPModule.Services;
using IDB.MW.Application.PEPModule.Services.Excel;
using IDB.MW.Application.PEPModule.Services.ExecutingAgency;
using IDB.MW.Application.PEPModule.Services.Procurement;
using IDB.MW.Business.PEPModule.DTOs;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.MW.Infrastructure.Data.Optima.Repositories.Common;
using IDB.Presentation.MVC4.Areas.Pep.Models;
using IDB.Presentation.MVC4.Controllers;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.Presentation.MVC4.Report;
using IDB.Presentation.MVC4.Areas.PEP.Models;
using IDB.MW.Application.PEPModule.Services.Risk;

namespace IDB.Presentation.MVC4.Areas.Pep.Controllers
{
    public partial class PepController : BaseController
    {
        private readonly IPEPService _pepService;
        private readonly IPepExecutingAgencies _pepExecAgency;
        private readonly IProcurement _pepProcurement;
        private readonly IExportImportExcelPep _exportImportExcelPep;
        private readonly IRiskService _pepRiskService;
        private readonly ViewModelMapperHelper _viewModelMapperHelper;
        private readonly ViewModelMapperHelperExecAgency _viewModelMapperHelperExecAgency;
        private readonly ViewModelMapperHelperProcurementType _viewModelMapperHelperProcurementType;
        private readonly ViewModelMapperHelperPepVersion _viewModelMapperHelperPepVersion;
        private readonly ViewModelMapperHelperRisk _viewModelMapperHelperRisk;
        private ICacheManagement _cacheData = null;
        private string _outputsPhysicalCacheName = string.Empty;
        private string _outputsFinancialCacheName = string.Empty;

        public PepController(IPEPService pepService,
                             IPepExecutingAgencies pepExecAgency,
                             IProcurement pepProcurement,
                             ICacheManagement cacheData,
                             IExportImportExcelPep exportImportExcelPep,
            IRiskService pepRiskService)
        {
            _pepService = pepService;
            _pepExecAgency = pepExecAgency;
            _pepProcurement = pepProcurement;
            _pepRiskService = pepRiskService;
            _viewModelMapperHelper = new ViewModelMapperHelper(pepService);
            _viewModelMapperHelperExecAgency = new ViewModelMapperHelperExecAgency(pepExecAgency);
            _viewModelMapperHelperProcurementType =
                                          new ViewModelMapperHelperProcurementType(pepProcurement);
            _viewModelMapperHelperPepVersion = new ViewModelMapperHelperPepVersion(pepService);
            _viewModelMapperHelperRisk = new ViewModelMapperHelperRisk(pepRiskService);
            _cacheData = cacheData;
            _outputsPhysicalCacheName = string.Format(
                CacheNames.OUTPUTS_PHYSICAL, IDBContext.Current.Operation);
            _outputsFinancialCacheName = string.Format(
                 CacheNames.OUTPUTS_FINANCIAL, IDBContext.Current.Operation);
            _exportImportExcelPep = exportImportExcelPep;
        }

        public virtual ActionResult Index(string operationNumber)
        {
            PepIndexViewModel model = new PepIndexViewModel();
            var response = _pepService.GetGeneralInformation(operationNumber);
            if (response.IsValid)
            {
                UnlockRegister(operationNumber, IDB.MW.Domain.Values.PepGlobalValues.ConcurrenceUrl);
                model = _viewModelMapperHelper.PepIndexViewModel(response);
            }
            else
            {
                throw new Exception(response.ErrorMessage);
            }

            model.OperationNumber = operationNumber;

            return View(model);
        }

        public virtual JsonResult TaskJsonResult(string operationNumber, bool isUploadExcel)
        {
            var responseJsonResult = new JsonResult();
            try
            {
                operationNumber = commonBusinessRules.GetOperationNumberParentIfAny(operationNumber);
                var response = _pepService.GetAllTaskResponse(operationNumber, isUploadExcel);
                if (response.IsValid)
                {
                    responseJsonResult = Json(response.ParentTask, JsonRequestBehavior.AllowGet);
                    responseJsonResult.MaxJsonLength = int.MaxValue;
                }
                else
                {
                    var responseError = ResponseError(response.ErrorMessage);
                    responseJsonResult = Json(responseError, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger().WriteMessage("PepController.TaskJsonResult Get main data",
                                                ex.InnerException.Message);
                var responseError = ResponseError(ex.Message);
                responseJsonResult = Json(responseError, JsonRequestBehavior.AllowGet);
            }

            return responseJsonResult;
        }

        public virtual JsonResult SavePep(string operationNumber, PepTransactionDto records)
        {
            var response = new IDB.Architecture.Services.BaseResponse { IsValid = true };
            var result = new JsonResult();

            try
            {
                bool permission = IDBContext.Current.HasPermission(Permission.PEP_POA_WRITE);
                if (permission)
                {
                    if (records.IsUploadExcel)
                    {
                        _pepService.ObtainDiferencesInUpload(operationNumber, ref records);
                    }

                    response = _pepService.SaveInPep(operationNumber, IDBContext.Current.UserLoginName, records);
                    if (response.IsValid)
                    {
                        if (records.IsUploadExcel)
                        {
                            _pepService.DeleteUploadExcelFile(operationNumber);
                        }

                        _cacheData.Remove(_outputsPhysicalCacheName, System.Web.Caching.CacheItemRemovedReason.DependencyChanged);
                        _cacheData.Remove(_outputsFinancialCacheName, System.Web.Caching.CacheItemRemovedReason.DependencyChanged);

                        result.MaxJsonLength = int.MaxValue;
                        result = Json(response, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        result = Json(response, JsonRequestBehavior.AllowGet);
                    }

                    UnlockRegister(operationNumber, IDB.MW.Domain.Values.PepGlobalValues.ConcurrenceUrl);
                }
                else
                {
                    response.IsValid = false;
                    response.ErrorMessage = Localization.GetText("PEP.Save.Not.Permissions");
                    result = Json(response, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                response.IsValid = false;
                response.ErrorMessage = e.Message;
                result = Json(response, JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        public virtual JsonResult BasicData(string operationNumber)
        {
            JsonResult result;

            try
            {
                operationNumber = commonBusinessRules
                                                   .GetOperationNumberParentIfAny(operationNumber);
                var request = _pepService.GetBasicDataResponse(operationNumber);
                if (request.IsValid)
                {
                    var response = _viewModelMapperHelper
                                                  .BasicDataViewModel(request.BasicDataViewModels);
                    result = Json(response, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var responseError = ResponseError(request.ErrorMessage);
                    result = Json(responseError, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger().WriteMessage("PepController.BasicData Get Basic Data",
                                                ex.InnerException.Message);
                var responseError = ResponseError(ex.Message);
                result = Json(responseError, JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        public virtual JsonResult ApprovalCurrency(string operationNumber)
        {
            JsonResult result;
            try
            {
                operationNumber = commonBusinessRules.GetOperationNumberParentIfAny(operationNumber);
                var request = _pepService.GetApprovalCurrencies(operationNumber);
                if (request.IsValid)
                {
                    var response = _viewModelMapperHelper
                                          .ApprovalCurrenciesViewModel(request.ApprovalCurrencies);
                    result = Json(response, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var responseError = ResponseError(request.ErrorMessage);
                    result = Json(responseError, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger()
                         .WriteMessage("PepController.ApprovalCurrency Get Data Approval Currency",
                                       ex.InnerException.Message);
                var responseError = ResponseError(ex.Message);
                result = Json(responseError, JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        public virtual JsonResult ComboBoxYear(string operationNumber)
        {
            JsonResult result;
            try
            {
                operationNumber = commonBusinessRules.GetOperationNumberParentIfAny(operationNumber);
                var request = _pepService.ComboBoxYearResponse(operationNumber);
                if (request.IsValid)
                {
                    var response = _viewModelMapperHelper
                                                      .ComboBoxYearViewModel(request.ComboBoxYear);
                    result = Json(response, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var responseError = ResponseError(request.ErrorMessage);
                    result = Json(responseError, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger().WriteMessage("PepController.ComboBoxYear Get Data year",
                                                ex.InnerException.Message);
                var responseError = ResponseError(ex.Message);
                result = Json(responseError, JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        public virtual JsonResult ComboBoxExecutingAgency(string operationNumber)
        {
            JsonResult result;
            try
            {
                operationNumber = commonBusinessRules.GetOperationNumberParentIfAny(operationNumber);
                var request = _pepExecAgency.ComboExecAgencyResponse(operationNumber);
                if (request.IsValid)
                {
                    var responsee = _viewModelMapperHelperExecAgency
                                              .ComboExecAgencyResponse(request.ComboBoxExecAgency);
                    result = Json(responsee, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var responseError = ResponseError(request.ErrorMessage);
                    result = Json(responseError, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger()
                        .WriteMessage("PepController.ComboBoxExecutingAgency Get Executing Agency",
                                      ex.InnerException.Message);
                var responseError = ResponseError(ex.Message);
                result = Json(responseError, JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        public virtual JsonResult GetDataExcel(string initDate,
                                               string endDate,
                                               string operationNumber)
        {
            var response = new ResponseBase();
            var result = new JsonResult();
            try
            {
                if (!response.IsValid)
                {
                    var responseError = ResponseError("POA Report");
                    result = Json(responseError, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger().WriteMessage("PepController.GeneratePOA Get Generate POA",
                                                ex.InnerException.Message);
                var responseError = ResponseError(ex.Message);
                result = Json(responseError, JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        public virtual JsonResult UpdateOrderNumber(string operationNumber,
                                                    OrderNumberRequest record)
        {
            ResponseBase response;
            operationNumber = commonBusinessRules.GetOperationNumberParentIfAny(operationNumber);
            response = _pepService.UpdateOrderNumber(record, operationNumber);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult ComboBoxProcurementType(string operationNumber, int countryId)
        {
            JsonResult result;
            try
            {
                operationNumber = commonBusinessRules
                                                   .GetOperationNumberParentIfAny(operationNumber);
                var request = _pepProcurement.ComboProcurementTypeResponse(operationNumber, countryId);
                if (request.IsValid)
                {
                    var response = _viewModelMapperHelperProcurementType
                                    .ComboProcurementTypeResponse(request.ComboBoxProcurementType);
                    result = Json(response, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var responseError = ResponseError(request.ErrorMessage);
                    result = Json(responseError, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger()
                      .WriteMessage("PepController.ComboBoxProcurementType Exception Get Data " +
                                    "ProcurementType",
                    ex.InnerException.Message);
                var responseError = ResponseError(ex.Message);
                result = Json(responseError, JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        public virtual FileResult DownloadChangeLogReport(string operationNumber,
                                                          string formatType)
        {
            formatType = "xls";
            operationNumber = commonBusinessRules.GetOperationNumberParentIfAny(operationNumber);
            var changeLogRecords = _pepService.GetChangeLogReport(operationNumber);
            var fileName = string.Format("{0}_Change_Log_Report", operationNumber
                                                                               .Replace("-", "_"));
            var application = "application/";
            application = application + "vnd.ms-excel";
            fileName = fileName + "." + formatType;

            return File(changeLogRecords.File, application, fileName);
        }

        public virtual JsonResult GetDataFinancial(string operationNumber, string year, bool isUploadExcel)
        {
            if (string.IsNullOrEmpty(year))
            {
                DateTime date = DateTime.Now;
                year = date.Year.ToString();
            }

            JsonResult responseJsonResult;
            try
            {
                operationNumber = commonBusinessRules
                                                   .GetOperationNumberParentIfAny(operationNumber);
                var responseAplicattion = _pepService.GetDataFinancial(operationNumber, year, isUploadExcel);
                var currentTotalCost = _pepService.GetTotalCostByOperation(operationNumber, year);

                if (responseAplicattion.IsValid && currentTotalCost.IsValid)
                {
                    responseAplicattion.FinancialProgress.SumTotalCost =
                                                                        currentTotalCost.TotalCost;
                    responseJsonResult = Json(_viewModelMapperHelper
                            .ConvertToFinancialProgressList(responseAplicattion.FinancialProgress),
                            JsonRequestBehavior.AllowGet);
                    responseJsonResult.MaxJsonLength = int.MaxValue;
                }
                else
                {
                    var responseError = ResponseError(responseAplicattion.ErrorMessage);
                    responseJsonResult = Json(responseError, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger()
                                .WriteMessage("GetDataFinancial EntityException ex InnerException",
                                              ex.InnerException.Message);
                var responseError = ResponseError(ex.Message);
                responseJsonResult = Json(responseError, JsonRequestBehavior.AllowGet);
            }

            return responseJsonResult;
        }

        public virtual JsonResult DataChangeLog(string operationNumber)
        {
            JsonResult result;
            try
            {
                var responseApp = _pepService.ChangeLogResponseData(operationNumber);
                if (responseApp.IsValid)
                {
                    var responseMapHelper = _viewModelMapperHelper
                                               .ChangeLogViewModel(responseApp.ChangeLogViewModel);
                    result = Json(responseMapHelper, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var responseError = ResponseError(responseApp.ErrorMessage);
                    result = Json(responseError, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger()
                         .WriteMessage("PepController.DataChangeLog Exception Get Data Change Log",
                                        ex.InnerException.Message);
                var responseError = ResponseError(ex.Message);
                result = Json(responseError, JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        public virtual FileResult DataChangeLogReport(string operationNumber)
        {
            JsonResult result;
            var response = _pepService.GetDataOperation(operationNumber);
            var url = string.Empty;

            if (response.IsValid)
            {
                url = "&OPERATION_ID=" + response.OperationId
                      + "&OPERATION_NUMBER=" + operationNumber
                      + "&OPERATION_NAME=" + response.OperationName;
            }

            var reportName = "[" + operationNumber + "]_PEP_POA_CHANGELOG.xlsx";

            return File(ReportBuilder.DownloadReport(
                ConfigurationManager.AppSettings["ParamForPepReportChangeLog"],
                url,
                "EXCELOPENXML"),
                "application/vnd.ms-excel",
                reportName);
        }

        public virtual JsonResult DataHistoryChangeLog(int idTaskBucket)
        {
            JsonResult responseJsonResult;
            try
            {
                var responseApp = _pepService.ChangeLogHistoryDataResponse(idTaskBucket);
                if (responseApp.IsValid)
                {
                    var responseMapHelper = _viewModelMapperHelper
                       .ConvertChangeLogHistoryViewModel(responseApp.ChangeLogHistoryAllViewModel);
                    responseJsonResult = Json(responseMapHelper, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var responseError = ResponseError(responseApp.ErrorMessage);
                    responseJsonResult = Json(responseError, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger().WriteMessage("PepController.DataHistoryChangeLog Exception " +
                                                "Get Data Change Log History",
                                                 ex.InnerException.Message);
                var responseError = ResponseError(ex.Message);
                responseJsonResult = Json(responseError, JsonRequestBehavior.AllowGet);
            }

            return responseJsonResult;
        }

        public virtual JsonResult ComboBoxProcurementMethod(string operationNumber, int countryId)
        {
            JsonResult responseJson;
            try
            {
                var responseCombo = _pepProcurement.ProcurementMethodData(operationNumber, countryId);
                if (responseCombo.IsValid)
                {
                    var response = _viewModelMapperHelper
                                   .ProcurementMethodResponse(responseCombo.ProcurementMethodData);
                    responseJson = Json(response, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var responseError = ResponseError(responseCombo.ErrorMessage);
                    responseJson = Json(responseError, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger()
                       .WriteMessage("ComboBoxProcurementMethod EntityException ex InnerException",
                                     ex.InnerException.Message);
                var responseError = ResponseError(ex.Message);
                responseJson = Json(responseError, JsonRequestBehavior.AllowGet);
            }

            return responseJson;
        }

        public virtual JsonResult ComboSupervisionType(string operationNumber, int countryId)
        {
            JsonResult responseJson;
            try
            {
                var responseCombo = _pepProcurement.ComboSupervisionType(operationNumber, countryId);
                if (responseCombo.IsValid)
                {
                    responseJson = Json(_viewModelMapperHelper
                          .ComboSupervisionMethodResponse(responseCombo.ComboBoxSupervisionMethod),
                                                          JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var responseError = ResponseError(responseCombo.ErrorMessage);
                    responseJson = Json(responseError, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger()
                            .WriteMessage("ComboSupervisionType EntityException ex InnerException",
                                           ex.InnerException.Message);
                var responseError = ResponseError(ex.Message);
                responseJson = Json(responseError, JsonRequestBehavior.AllowGet);
            }

            return responseJson;
        }

        public virtual JsonResult CommentPep(int pepTaskId)
        {
            JsonResult result;
            try
            {
                var commentsResponse = _pepService.CommentPepTask(pepTaskId);
                if (commentsResponse.IsValid)
                {
                    var responseMapper = _viewModelMapperHelper
                                          .ConvertCommentPepViewModel(commentsResponse.CommentPep);
                    result = Json(responseMapper, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var responseError = ResponseError(commentsResponse.ErrorMessage);
                    result = Json(responseError, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var responseError = ResponseError(ex.Message);
                result = Json(responseError, JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        public virtual JsonResult Contracts(string operationNumber)
        {
            JsonResult result;
            try
            {
                if (string.IsNullOrEmpty(operationNumber))
                {
                    throw new Exception();
                }

                operationNumber = commonBusinessRules.GetOperationNumberParentIfAny(operationNumber);
                var comboResponse = _pepService.ComboContract(operationNumber);
                if (comboResponse.IsValid)
                {
                    var response = _viewModelMapperHelper
                                             .ConvertToComboContract(comboResponse.ComboContracts);
                    result = Json(response, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var responseError = ResponseError(comboResponse.ErrorMessage);
                    result = Json(responseError, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger().WriteMessage("Contracts EntityException ex InnerException",
                                                 ex.InnerException.Message);
                var ressponseError = ResponseError(ex.Message);
                result = Json(ressponseError, JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        public virtual FileResult ExportExcelPep(string operationNumber,
                                                 string initDate,
                                                 string endDate,
                                                 int countryId)
        {
            DateTime? fromDate = null;
            DateTime? toDate = null;
            if (initDate != null)
            {
                fromDate = Convert.ToDateTime(initDate);
            }

            if (endDate != null)
            {
                toDate = Convert.ToDateTime(endDate);
            }

            var pepExcel = _exportImportExcelPep.ExportToExcelPep(operationNumber,
                                                                  fromDate,
                                                                  toDate,
                                                                  countryId);
            if (pepExcel.IsValid)
            {
                var reportName = "[" + operationNumber + "]_PEP_POA.xlsx";
                return File(pepExcel.ExcelFile, "application/vnd.ms-excel", reportName);
            }
            else
            {
                throw new ArgumentException(pepExcel.ErrorMessage);
            }
        }

        public virtual FileResult ExportPoaReportByPepVersion(string operationNumber,
            int countryId,
            int filterType,
            int filterValue,
            int versionNumber)
        {
            var reportResponse = _exportImportExcelPep.ExportPoaReportToExcel(operationNumber,
                countryId,
                filterType,
                filterValue,
                versionNumber);

            if (reportResponse.IsValid)
            {
                var reportName = "[" + operationNumber + "]_PEP_POA.xlsx";
                return File(reportResponse.ExcelFile, "application/vnd.ms-excel", reportName);
            }
            else
            {
                throw new ArgumentException(reportResponse.ErrorMessage);
            }
        }

        public virtual JsonResult RequestNonObjection(string operationNumber,
                                                        string workFlowCode,
                                                        string entityType,
                                                        int entityId,
                                                        string moduleName)
        {
            if (string.IsNullOrEmpty(operationNumber) ||
                string.IsNullOrEmpty(workFlowCode) ||
                string.IsNullOrEmpty(entityType) ||
                string.IsNullOrEmpty(moduleName))
            {
                throw new Exception();
            }

            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult ContractFund(string operationNumber)
        {
            JsonResult result;
            try
            {
                if (string.IsNullOrEmpty(operationNumber))
                {
                    throw new Exception();
                }

                operationNumber = commonBusinessRules
                                                   .GetOperationNumberParentIfAny(operationNumber);
                var difFundResponse = _pepService.ContractsFundsDif(operationNumber);
                if (difFundResponse.IsValid)
                {
                    var response = _viewModelMapperHelper
                                             .ConvertToDifFundsTotal(difFundResponse.ContractFund);
                    result = Json(response, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var responseError = ResponseError(difFundResponse.ErrorMessage);
                    result = Json(responseError, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger().WriteMessage("Contracts EntityException ex InnerException",
                                                 ex.InnerException.Message);
                var ressponseError = ResponseError(ex.Message);
                result = Json(ressponseError, JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        public virtual JsonResult ImportExcelFile(string operationNumber)
        {
            var result = new JsonResult();
            ResponseProcessImportFile response;
            var file = Request.Files[0];
            operationNumber = commonBusinessRules
                                   .GetOperationNumberParentIfAny(operationNumber);

            if (file != null && file.ContentLength > 0)
            {
                string extension = Path.GetExtension(file.FileName);
                if (PepGlobalValues.EXTENSION_XLSX != extension)
                {
                    result.Data = new
                    {
                        success = false,
                        fileName = file.FileName,
                        message = PepGlobalValues.MESSAGE_INVALID_EXTENSION + extension
                    };

                    return result;
                }

                RequestExcelImport request = new RequestExcelImport
                {
                    ExcelFile = file.InputStream,
                    OperationNumber = operationNumber
                };

                response = _exportImportExcelPep.ProcessImportExcelFile(request);
                if (!response.IsValid)
                {
                    result.Data = new
                    {
                        success = false,
                        fileName = file.FileName,
                        message = response.ErrorMessage,
                        errors = response.ValidationErrors
                    };
                }
                else
                {
                    result.Data = new
                    {
                        success = true,
                        fileName = file.FileName
                    };
                }
            }
            else
            {
                result.Data = new
                {
                    success = false,
                    fileName = string.Empty,
                    message = PepGlobalValues.MESSAGE_FILE_EXCEL_NOT_FOUND
                };
            }

            return result;
        }

        public virtual JsonResult ComboBoxRisk(string operationNumber)
        {
            JsonResult result;
            try
            {
                operationNumber = commonBusinessRules
                    .GetOperationNumberParentIfAny(operationNumber);

                var serviceResponse = _pepRiskService.ComboRisk(operationNumber);

                if (serviceResponse.IsValid)
                {
                    var mapperResponse = _viewModelMapperHelperRisk
                        .ComboRiskResponse(serviceResponse.ComboBoxRisk);
                    result = Json(mapperResponse, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var responseError = ResponseError(serviceResponse.ErrorMessage);
                    result = Json(responseError, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger()
                      .WriteMessage("PepController.ComboBoxRisk Exception Get Data " +
                                    "Risk",
                    ex.InnerException.Message);
                var responseError = ResponseError(ex.Message);
                result = Json(responseError, JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        public virtual JsonResult GetRiskMatrixData(string operationNumber, int pepTaskId)
        {
            JsonResult result;
            try
            {
                var riskMatrixResponse = _pepRiskService
                    .GetAssociatedRiskData(operationNumber, pepTaskId);

                if (riskMatrixResponse.IsValid)
                {
                    result = Json(riskMatrixResponse, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var responseError = ResponseError(riskMatrixResponse.ErrorMessage);
                    result = Json(responseError, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger()
                    .WriteMessage("PepController.GetRiskMatrixData ", ex.InnerException.Message);
                var responseError = ResponseError(ex.Message);
                result = Json(responseError, JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        public virtual JsonResult GetFinancialExecutionData(string operationNumber, string year, bool isUploadExcel)
        {
            if (string.IsNullOrEmpty(year))
            {
                DateTime date = DateTime.Now;
                year = date.Year.ToString();
            }

            JsonResult responseJsonResult;

            try
            {
                operationNumber = commonBusinessRules
                                   .GetOperationNumberParentIfAny(operationNumber);
                var responseAplicattion = _pepService
                    .GetFinancialExecutionData(operationNumber, year, isUploadExcel);

                responseJsonResult = Json(responseAplicattion.FinancialExecution, JsonRequestBehavior.AllowGet);
                responseJsonResult.MaxJsonLength = int.MaxValue;
            }
            catch (Exception ex)
            {
                Logger.GetLogger()
                                .WriteMessage("GetDataFinancial EntityException ex InnerException",
                                              ex.InnerException.Message);
                var responseError = ResponseError(ex.Message);
                responseJsonResult = Json(responseError, JsonRequestBehavior.AllowGet);
            }

            return responseJsonResult;
        }

        public virtual JsonResult GetVerifyContentValidations(string operationNumber)
        {
            JsonResult result;

            try
            {
                operationNumber = commonBusinessRules
                    .GetOperationNumberParentIfAny(operationNumber);

                var verifyContentResponse = _pepService
                    .GetVerifyContentValidations(operationNumber);

                if (verifyContentResponse.IsValid)
                {
                    var response = _viewModelMapperHelper
                        .ConvertToVerifyContentViewModel(verifyContentResponse.ValidationErrors);
                    result = Json(response, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var responseError = ResponseError(verifyContentResponse.ErrorMessage);
                    result = Json(responseError, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger()
                    .WriteMessage("GetVerifyContentValidations EntityException ex InnerException",
                    ex.InnerException.Message);
                var responseError = ResponseError(ex.Message);
                result = Json(responseError, JsonRequestBehavior.AllowGet);
            }

            return result;
        }

        public virtual JsonResult ComboBoxPepVersions(string operationNumber, int filterType)
        {
            JsonResult jsonResponse;

            try
            {
                operationNumber = commonBusinessRules
                                                   .GetOperationNumberParentIfAny(operationNumber);

                var response = _pepService.GetPepVersions(operationNumber, filterType);

                if (response.IsValid)
                {
                    jsonResponse = Json(_viewModelMapperHelperPepVersion
                        .ComboPepVersionResponse(response.PepVersions),
                        JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var responseError = ResponseError(response.ErrorMessage);
                    jsonResponse = Json(responseError, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger()
                      .WriteMessage("PepController.ComboBoxPepVersions Exception Get Data " +
                                    "GetPepVersionList",
                    ex.InnerException.Message);
                var responseError = ResponseError(ex.Message);
                jsonResponse = Json(responseError, JsonRequestBehavior.AllowGet);
            }

            return jsonResponse;
        }

        public virtual FileResult ExportExcelPepByDateRange(string operationNumber,
                                         string initDate,
                                         string endDate,
                                         int countryId)
        {
            DateTime? fromDate = null;
            DateTime? toDate = null;
            if (initDate != null)
            {
                fromDate = Convert.ToDateTime(initDate);
            }

            if (endDate != null)
            {
                toDate = Convert.ToDateTime(endDate);
            }

            var pepExcel = _exportImportExcelPep
                .ExportExcelPepByDateRange(operationNumber,
                fromDate,
                toDate,
                countryId);

            if (pepExcel.IsValid)
            {
                var reportName = "[" + operationNumber + "]_PEP_POA.xlsx";
                return File(pepExcel.ExcelFile, "application/vnd.ms-excel", reportName);
            }
            else
            {
                throw new ArgumentException(pepExcel.ErrorMessage);
            }
        }

        protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonResult()
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior,
                MaxJsonLength = int.MaxValue
            };
        }

        private ResponseBase ResponseError(string message)
        {
            var responseError = new ResponseBase();
            responseError.IsValid = false;
            responseError.ErrorMessage = message;

            return responseError;
        }
    }
}