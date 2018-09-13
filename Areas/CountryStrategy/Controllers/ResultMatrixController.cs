using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.MW.Application.CountryStrategyModule.Enums;
using IDB.MW.Application.CountryStrategyModule.Services.ResultMatrix;
using IDB.MW.Application.CountryStrategyModule.ViewModels.ResultMatrix;
using IDB.MW.Application.IndicatorsModuleNew.Services.LinkPredefinedIndicator;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.MW.Infrastructure.SecurityService.Enums;
using IDB.Presentation.MVC4.Areas.CountryStrategy.Mappers;
using IDB.Presentation.MVC4.General;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.CountryStrategyModule.Services.CSUpdatePortfolio;
using IDB.MW.Infrastructure.ReportManager.Enums;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Contracts.ModelRepositories.Security;

namespace IDB.Presentation.MVC4.Areas.CountryStrategy.Controllers
{
    public partial class ResultMatrixController : BaseController
    {
        #region Constants

        public static string SAVE_MODE = "SaveMode";
        private static string TAB_NAME_PORTFOLIO = "tabPortfolioSummary";
        private static string TAB_NAME_RM = "tabResultMatrix";
        private static string CHART_RESULT_MATRIX = "UI-CS-005-ResultsMatrix";
        private static string CHART_PORTFOLIO = "UI-CS-009-PortfolioSummary";
        private static string CS_RESULTS_MATRIX_URL = "/CountryStrategy/ResultMatrix/ResultMatrix";
        private static string CS_PORTFOLIO_URL = "/CountryStrategy/ResultMatrix/Portfolio";

        #endregion

        #region Fields
        private readonly IAuthorizationService _authorizationService;
        private readonly ICSResultMatrixService _resultMatrixService;
        private readonly IEnumMappingService _enumMappingService;
        private readonly ILinkPredefinedIndicatorService _linkPredefinedIndicatorService;
        private readonly ICSUpdatePortfolioService _csUpdatePortfolioService;
        private readonly ISecurityModelRepository _securityModelRepository;
        #endregion

        #region Contructors

        public ResultMatrixController(
            IAuthorizationService authorizationService,
            ILinkPredefinedIndicatorService linkPredefinedIndicatorService,
            ICSResultMatrixService resultMatrixService,
            IEnumMappingService enumMappingService,
            ICSUpdatePortfolioService csUpdatePortfolioService,
            ISecurityModelRepository securityModelRepository)
            : base(authorizationService)
        {
            _authorizationService = authorizationService;
            _resultMatrixService = resultMatrixService;
            _linkPredefinedIndicatorService = linkPredefinedIndicatorService;
            _enumMappingService = enumMappingService;
            _csUpdatePortfolioService = csUpdatePortfolioService;
            _securityModelRepository = securityModelRepository;
        }

        #endregion

        #region Action Methods

        public virtual ActionResult Read(string operationNumber, string tabName = null, string errorMessage = null)
        {
            ViewBag.ActiveTab = tabName ?? TAB_NAME_RM;

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                ViewBag.ErrorMessage = errorMessage;
            }

            var resultMatrixModel = GetResultMatrix(operationNumber, CHART_RESULT_MATRIX);            

            if (resultMatrixModel != null  && 
               (resultMatrixModel.Status == CountryStrategicStatusEnum.Active   || 
                    resultMatrixModel.Status == CountryStrategicStatusEnum.Expired))
            {                
                var portfolioModel = GetPortfolioSummary(operationNumber, CHART_PORTFOLIO);
                portfolioModel.ResultMatrix = resultMatrixModel;
                SetViewBagPortfolio();
                return View("ReadPortfolio", portfolioModel);
            }

            return View("Read", resultMatrixModel);
        }

        public virtual ActionResult Cancel(string operationNumber, string tabName = null)
        {
            ViewBag.ActiveTab = tabName ?? TAB_NAME_RM;

            if (tabName == null || tabName == TAB_NAME_RM)
            {
                SynchronizationHelper.TryReleaseLock(CS_RESULTS_MATRIX_URL, operationNumber, IDBContext.Current.UserLoginName);
            }

            if (tabName == TAB_NAME_PORTFOLIO)
            {
                SynchronizationHelper.TryReleaseLock(CS_PORTFOLIO_URL, operationNumber, IDBContext.Current.UserLoginName);
            }

            var resultMatrixModel = GetResultMatrix(operationNumber, CHART_RESULT_MATRIX);

            if (resultMatrixModel != null &&
               (resultMatrixModel.Status == CountryStrategicStatusEnum.Active ||
                    resultMatrixModel.Status == CountryStrategicStatusEnum.Expired))
            {
                var portfolioModel = GetPortfolioSummary(operationNumber, CHART_PORTFOLIO);
                portfolioModel.ResultMatrix = resultMatrixModel;
                SetViewBagPortfolio();
                return View("ReadPortfolio", portfolioModel);
            }

            return View("Read", resultMatrixModel);
        }

        public virtual ActionResult Edit(string operationNumber)
        {
            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, CS_RESULTS_MATRIX_URL, operationNumber, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return RedirectToAction("Read", new { tabName = TAB_NAME_RM, errorMessage = errorMessage });
            }

            var model = GetResultMatrix(operationNumber, CHART_RESULT_MATRIX);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            return View(model);
        }

        public virtual ActionResult SaveActiveRM(string operationNumber)
        {
            var responseView = new SaveResponse() { IsValid = true };

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<ResultMatrixViewModel>(jsonDataRequest.SerializedData);
            var formData = jsonDataRequest.ClientFieldData;

            viewModel.UpdateResultMatrixViewModel(formData);

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, CS_RESULTS_MATRIX_URL, operationNumber, IDBContext.Current.UserLoginName);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            var response = _resultMatrixService.SaveResultMatrix(operationNumber, viewModel);

            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;
            responseView.UrlRedirect = Url.Action("Read", "ResultMatrix", new { area = "CountryStrategy", tabName = TAB_NAME_RM });

            if (responseView.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(CS_RESULTS_MATRIX_URL, operationNumber, IDBContext.Current.UserLoginName);
                responseView.UrlRedirect = Url.Action("Read", "ResultMatrix", new { area = "CountryStrategy", tabName = TAB_NAME_RM });
            }

            return Json(responseView);
        }

        public virtual ActionResult EditPortfolio(string operationNumber)
        {
            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, CS_PORTFOLIO_URL, operationNumber, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return RedirectToAction("Read", new { tabName = TAB_NAME_PORTFOLIO, errorMessage = errorMessage });
            }

            var portfolioModel = GetPortfolioSummary(operationNumber, CHART_PORTFOLIO);
            portfolioModel.ResultMatrix = GetResultMatrix(operationNumber, CHART_RESULT_MATRIX);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(portfolioModel);
            return View(portfolioModel);
        }

        public virtual ActionResult Save(string operationNumber)
        {
            ResponseBase response = new ResponseBase() { IsValid = true };
            var responseView = new SaveResponse() { IsValid = true };

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<ResultMatrixViewModel>(jsonDataRequest.SerializedData);
            var formData = jsonDataRequest.ClientFieldData;

            viewModel.UpdateResultMatrixViewModel(formData);

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, CS_RESULTS_MATRIX_URL, operationNumber, IDBContext.Current.UserLoginName);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            var submitMode = formData.First(x => x.Name == "submitMode").Value;
            if (submitMode == SAVE_MODE)
            {
                response = _resultMatrixService.SaveResultMatrix(operationNumber, viewModel);
            }

            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;
            responseView.UrlRedirect = Url.Action("Read", "ResultMatrix", new { area = "CountryStrategy" });

            if (responseView.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(CS_RESULTS_MATRIX_URL, operationNumber, IDBContext.Current.UserLoginName);
                responseView.UrlRedirect = Url.Action("Read", "ResultMatrix", new { area = "CountryStrategy", tabName = TAB_NAME_RM });
            }

            return Json(responseView);
        }

        public virtual ActionResult DetailsOperationAlignmentDataRead(string operationNumber, int rowId, CSPortfolioAlignmentTypeEnum typeOperationPortfolio, int csObjectiveId, int csResultMatrixId, bool isOtherArea)
        {
            var operationsDetails = GetOperationsAlignmentDataDetails(operationNumber, rowId, typeOperationPortfolio, csObjectiveId, csResultMatrixId, isOtherArea);

            return View("DetailsOperationRead", operationsDetails);
        }

        public virtual ActionResult DetailsOperationsApprovedDataRead(string operationNumber, int rowId, CSOperationsApprovedType typeInstrument)
        {
            var operationsDetails = GetOperationsApprovedDataDetails(operationNumber, rowId, typeInstrument);

            return View("DetailsOperationRead", operationsDetails);
        }

        public virtual FileResult CSResultsMatrixExportToPDF(string operationNumber)
        {
            var response = _resultMatrixService.ExportCountryStrategyResultsMatrixFile(operationNumber, OutputFormatEnum.PDF);

            if (!response.IsValid)
            {
                return null;
            }

            return File(response.File, "application/doc", "Country Strategy " + operationNumber + ".pdf");
        }

        public virtual FileResult CSResultsMatrixExportToExcel(string operationNumber)
        {
            var response = _resultMatrixService.ExportCountryStrategyResultsMatrixFile(operationNumber, OutputFormatEnum.Excel);

            if (!response.IsValid)
            {
                return null;
            }

            return File(response.File, "application/doc", "Country Strategy " + operationNumber + ".xls");
        }

        public virtual FileResult CSResultsMatrixExportToWord(string operationNumber)
        {
            var response = _resultMatrixService.ExportCountryStrategyResultsMatrixFile(operationNumber, OutputFormatEnum.Word);

            if (!response.IsValid)
            {
                return null;
            }
            
            return File(response.File, "application/doc", "Country Strategy " + operationNumber + ".doc");
        }

        #endregion

        #region Private Methods
        private ResultMatrixViewModel GetResultMatrix(string operationNumber, string pageChart = null)
        {
            ResultMatrixViewModel model = null;
            try
            {
                SetViewBagPermissionAndCheckAny(operationNumber, ActionEnum.FWIndicatorWritePermission);

                var response = _resultMatrixService.GetResultMatrix(operationNumber);
                SetViewBagErrorMessageInvalidResponse(response);
                model = response.ResultMatrix;
                if (pageChart != null)
                {
                    var fieldAccessList = _securityModelRepository.GetFieldBehaviorByPermissions(
                        IDBContext.Current.Operation,
                        pageChart,
                        IDBContext.Current.Permissions,
                        0,
                        0).ToList();
                    response.ResultMatrix.FieldAccessList = fieldAccessList;
                    response.ResultMatrix.Components.ForEach(o => o.Objectives.ForEach(a =>
                               a.FieldAccessList = fieldAccessList));
                }

                SetViewBag(operationNumber, model);
            }
            catch (Exception e)
            {
                ViewBag.ErrorMessage = e.Message;
                ViewBag.IsValid = false;
            }

            return model;
        }

        private PortfolioModeViewModel GetPortfolioSummary(string operationNumber, string pageChart = null)
        {
            PortfolioModeViewModel model = null;
            try
            {
                SetViewBagPermissionAndCheckAny(operationNumber, ActionEnum.FWIndicatorWritePermission);
                var response = _resultMatrixService.GetPortfolioSummary(operationNumber);
                SetViewBagErrorMessageInvalidResponse(response);
                model = response.Portfolio;
                if (pageChart != null)
                {
                    response.Portfolio.FieldAccessList = _securityModelRepository.GetFieldBehaviorByPermissions(
                        IDBContext.Current.Operation,
                        pageChart,
                        IDBContext.Current.Permissions,
                        0,
                        0)
                        .ToList();
                }
            }
            catch (Exception e)
            {
                ViewBag.ErrorMessage = e.Message;
                ViewBag.IsValid = false;
            }

            return model;
        }

        private void SetViewBagPortfolio()
        {
            ViewBag.DialogueArea = _enumMappingService.GetMappingCode(CSStrObjCurrentApprovedTypeEnum.DialogueArea);
            ViewBag.NewAreas = _enumMappingService.GetMappingCode(CSStrObjCurrentApprovedTypeEnum.NewAreas);
        }

        private void SetViewBag(string operationNumber, ResultMatrixViewModel resultMatrix)
        {
            var outcomeResponse = _linkPredefinedIndicatorService.GetLinkIndicatorModelForCSResultMatrix(false, "LinkIndicator");
            ViewBag.LinkIndicatorModel = outcomeResponse.Filter;

            var expiredObjectiveResponse = _resultMatrixService.GetExpiredObjectives(operationNumber);
            ViewBag.AssociatedObjectiveList = expiredObjectiveResponse.ExpiredObjectives;

            ViewBag.CRFIndicators = new List<SelectListItem>();
            if (resultMatrix != null)
            {
                var allOutcomes = resultMatrix.Components.SelectMany(x => x.Objectives).SelectMany(y => y.ExpectedOutcomeIndicators);
                var allCRFIndicatorIdUsed = allOutcomes.SelectMany(x => x.LinkedIndicators).Distinct();
                var crfIndicatorResponse = _resultMatrixService.GetCRFIndicatorsById(allCRFIndicatorIdUsed);
                ViewBag.CRFIndicators = crfIndicatorResponse.CRFIndicators;
            }

            ViewBag.TCType = _enumMappingService.GetMappingCode(CSOperationTypeEnum.TechnicalCooperation);
            ViewBag.SGType = _enumMappingService.GetMappingCode(CSOperationTypeEnum.LoanOperation);
        }

        private OperationsDetailsViewModel GetOperationsApprovedDataDetails(string operationNumber, int rowId, CSOperationsApprovedType typeInstrument)
        {
            var model = new OperationsDetailsViewModel();

            try
            {
                var response = _resultMatrixService.GetOperationsDetailByInstrument(operationNumber, rowId, typeInstrument);
                if (response.IsValid)
                {
                    model.Title = response.OperationDetails.Title;
                    model.OperationsByInstrument = response.OperationDetails.OperationsByInstrument;
                }
            }
            catch (Exception e)
            {
                ViewBag.ErrorMessage = e.Message;
                ViewBag.IsValid = false;
            }

            return model;
        }

        private OperationsDetailsViewModel GetOperationsAlignmentDataDetails(string operationNumber, int rowId, CSPortfolioAlignmentTypeEnum typeOperationPortfolio, int csObjectiveId, int csResultMatrixId, bool isOtherArea)
        {
            var model = new OperationsDetailsViewModel();

            try
            {
                var response = _resultMatrixService.GetOperationsDetailByObjective(operationNumber, rowId, typeOperationPortfolio, csObjectiveId, csResultMatrixId, isOtherArea);
                if (response.IsValid)
                {
                    model.Title = response.OperationDetails.Title;
                    model.IDBLoanGuaIgrOperations = response.OperationDetails.IDBLoanGuaIgrOperations;
                    model.IDBTCOperations = response.OperationDetails.IDBTCOperations;
                    model.IICOperations = response.OperationDetails.IICOperations;
                    model.MIFLonGuaIgrEquOperations = response.OperationDetails.MIFLonGuaIgrEquOperations;
                    model.MIFTCOperations = response.OperationDetails.MIFTCOperations;
                }
            }
            catch (Exception e)
            {
                ViewBag.ErrorMessage = e.Message;
                ViewBag.IsValid = false;
            }
            
            return model;
        }
        #endregion
    }
}