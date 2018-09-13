using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.DataTables.DataTable.ServerSide;
using IDB.Architecture.Extensions;
using IDB.Architecture.Language;
using IDB.Architecture.Logging;
using IDB.Architecture.Security;
using IDB.Architecture.Security.Models.UserIdentity;
using IDB.MVCControls.General.Helpers;
using IDB.MW.Application.Core.Enums;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Application.Helpers;
using IDB.MW.Application.Mocks.DataTable;
using IDB.MW.Application.TCAbstractModule.Enums;
using IDB.MW.Application.TCAbstractModule.Helpers;
using IDB.MW.Application.TCAbstractModule.Messages.FundInformation;
using IDB.MW.Application.TCAbstractModule.Messages.ReviewTCAbstract;
using IDB.MW.Application.TCAbstractModule.Messages.ReviewTCAbstractPostValidation;
using IDB.MW.Application.TCAbstractModule.Messages.SingleWindowMeeting;
using IDB.MW.Application.TCAbstractModule.Messages.SingleWindowOperation;
using IDB.MW.Application.TCAbstractModule.Services.Common;
using IDB.MW.Application.TCAbstractModule.Services.DonorDecision;
using IDB.MW.Application.TCAbstractModule.Services.EligibilityDecision;
using IDB.MW.Application.TCAbstractModule.Services.ESCDecision;
using IDB.MW.Application.TCAbstractModule.Services.FilterFunds;
using IDB.MW.Application.TCAbstractModule.Services.FundInformation;
using IDB.MW.Application.TCAbstractModule.Services.Mocks.TC;
using IDB.MW.Application.TCAbstractModule.Services.NotifyFunding;
using IDB.MW.Application.TCAbstractModule.Services.RequestIncrease;
using IDB.MW.Application.TCAbstractModule.Services.RevertFundingProcess;
using IDB.MW.Application.TCAbstractModule.Services.ReviewFundCoordination;
using IDB.MW.Application.TCAbstractModule.Services.ReviewRegionalTL;
using IDB.MW.Application.TCAbstractModule.Services.ReviewTCAbstract;
using IDB.MW.Application.TCAbstractModule.Services.ReviewTCAbstractPostValidation;
using IDB.MW.Application.TCAbstractModule.Services.SingleWindowDecision;
using IDB.MW.Application.TCAbstractModule.Services.SingleWindowMeeting;
using IDB.MW.Application.TCAbstractModule.Services.SingleWindowOperation;
using IDB.MW.Application.TCAbstractModule.Services.SingleWindowOperationDecision;
using IDB.MW.Application.TCAbstractModule.Services.TCAbstractService;
using IDB.MW.Application.TCAbstractModule.Services.ValidationTCAbstract;
using IDB.MW.Application.TCAbstractModule.ViewModels.AwardFundEligibility;
using IDB.MW.Application.TCAbstractModule.ViewModels.DonorDecision;
using IDB.MW.Application.TCAbstractModule.ViewModels.EligibilityDecision;
using IDB.MW.Application.TCAbstractModule.ViewModels.ESCDecision;
using IDB.MW.Application.TCAbstractModule.ViewModels.FundInformation;
using IDB.MW.Application.TCAbstractModule.ViewModels.NotifyFunding;
using IDB.MW.Application.TCAbstractModule.ViewModels.RequestIncrease;
using IDB.MW.Application.TCAbstractModule.ViewModels.ReviewFundCoordination;
using IDB.MW.Application.TCAbstractModule.ViewModels.ReviewRegionalTL;
using IDB.MW.Application.TCAbstractModule.ViewModels.ReviewTCAbstract;
using IDB.MW.Application.TCAbstractModule.ViewModels.ReviewTCAbstractPostValidation;
using IDB.MW.Application.TCAbstractModule.ViewModels.SingleWindowOperationDecision;
using IDB.MW.Application.TCAbstractModule.ViewModels.TCAbstractService;
using IDB.MW.Application.TCAbstractModule.ViewModels.ValidationTCAbstract;
using IDB.MW.Business.Core.K2Manager.Contracts;
using IDB.MW.Business.Core.SharepointSecurityService;
using IDB.MW.Business.TCAbstractModule.Contracts;
using IDB.MW.Business.TCAbstractModule.DTOs;
using IDB.MW.Domain.Contracts.DomainServices;
using IDB.MW.Domain.Entities;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.MW.DomainModel.Entities.Core.K2;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.MW.Infrastructure.BaseClasses;
using IDB.MW.Infrastructure.Caching.Contracts;
using IDB.MW.Infrastructure.Configuration;
using IDB.MW.Infrastructure.Helpers;
using IDB.MW.Infrastructure.ReportManager.Enums;
using IDB.MW.Infrastructure.SecurityService;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.MW.Infrastructure.SecurityService.Enums;
using IDB.Presentation.MVC4.Areas.TC.Enums;
using IDB.Presentation.MVC4.Areas.TC.Models;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;
using IDB.Architecture.Security.Messages.UserIdentity;

namespace IDB.Presentation.MVC4.Areas.TC.Controllers
{
    public partial class ViewController : BaseController
    {
        #region Constants

        private const string TC_ERROR_MSGS = "_ErrorMessages";
        private const string TRUST_FUND_ID = "trustFundId";
        private const string PARTIAL_TC_ABSTRACT = "Partials/TCAbstract";

        #endregion

        #region Fields

        readonly IAuthorizationService _authorizationService;
        readonly ITCAbstractService _tcAbstractService;
        readonly ICatalogService _catalogService;
        readonly ICacheStorageService _cacheStorageService;
        readonly ITCMockService _tcMockService;
        readonly INotifyFundingService _notifyFundingService;
        readonly IK2Manager _k2Manager;
        readonly IReviewTCAbstractService _reviewTcAbstractService;
        readonly IValidationTCAbstractService _validationTcAbstractService;
        readonly IReviewFundingCoordinatorService _reviewFundingCoordinatorService;
        readonly IReviewRegionalTLService _reviewRegionalTLService;
        readonly ISingleWindowMeetingService _singleWindowMeetingService;
        readonly ISingleWindowOperationService _singleWindowOperationService;
        readonly ISingleWindowOperationDecisionService _singleWindowOperationDecisionService;
        readonly ISingleWindowDecisionService _singleWindowDecisionService;
        readonly IEligibilityDecisionService _eligibilityDecisionService;
        readonly IDonorDecisionService _donorDecisionService;
        readonly IESCDecisionService _escDecisionService;
        readonly IRequestIncreaseService _requestIncreaseService;
        readonly IReviewTCAbstractPostValidationService _reviewTCAbstractPostValidationService;
        readonly IFilterFundsService _filterFundsService;
        readonly IFundInformationService _fundInformationService;
        readonly IWorkflowInstanceRepository _workflowInstanceRepository;
        readonly IDataTableMockService _dataTableMockService;
        readonly IEnumMappingService _enumMappingService;
        readonly IFundsSharepointService _fundsSharepointService;
        readonly K2IntegrationHelper _k2IntegrationHelper;
        readonly ITCCommonService _tcCommonService;
        readonly IRevertFundingProcessService _revertFundingProcessService;

        #endregion

        #region Contructors

        public ViewController(ITCAbstractService tcAbstractService,
            ICatalogService catalogService,
            ICacheStorageService cacheStorageService,
            ITCMockService tcMockService,
            INotifyFundingService notifyFundingService,
            IReviewTCAbstractService reviewTcAbstractService,
            IValidationTCAbstractService validationTcAbstractService,
            IReviewFundingCoordinatorService reviewFundingCoordinatorService,
            ISingleWindowOperationDecisionService singleWindowOperationDecisionService,
            IReviewRegionalTLService reviewRegionalTLService,
            ISingleWindowMeetingService singleWindowMeetingService,
            ISingleWindowOperationService singleWindowOperationService,
            ISingleWindowDecisionService singleWindowDecisionService,
            IEligibilityDecisionService eligibilityDecisionService,
            IDonorDecisionService donorDecisionService,
            IESCDecisionService escDecisionService,
            IRequestIncreaseService requestIncreaseService,
            IReviewTCAbstractPostValidationService reviewTCAbstractPostValidationService,
            IDataTableMockService dataTableMockService,
            IFilterFundsService filterFundsService,
            IFundInformationService fundInformationService,
            IEnumMappingService enumMappingService,
            IK2Manager k2Manager,
            IFundsSharepointService fundsSharepointService,
            IWorkflowInstanceRepository workflowInstanceRepository,
            ITCCommonService tcCommonService,
            IWorkflowInstanceTaskRepository workflowInstanceTaskRepository,
            IRevertFundingProcessService revertFundingProcessService)
        {
            _authorizationService = AuthorizationServiceFactory.Current;
            _tcAbstractService = tcAbstractService;
            _catalogService = catalogService;
            _cacheStorageService = cacheStorageService;
            _tcMockService = tcMockService;
            _notifyFundingService = notifyFundingService;
            _reviewTcAbstractService = reviewTcAbstractService;
            _validationTcAbstractService = validationTcAbstractService;
            _reviewFundingCoordinatorService = reviewFundingCoordinatorService;
            _singleWindowOperationDecisionService = singleWindowOperationDecisionService;
            _reviewRegionalTLService = reviewRegionalTLService;
            _singleWindowMeetingService = singleWindowMeetingService;
            _singleWindowOperationService = singleWindowOperationService;
            _singleWindowDecisionService = singleWindowDecisionService;
            _eligibilityDecisionService = eligibilityDecisionService;
            _donorDecisionService = donorDecisionService;
            _escDecisionService = escDecisionService;
            _requestIncreaseService = requestIncreaseService;
            _reviewTCAbstractPostValidationService = reviewTCAbstractPostValidationService;
            _filterFundsService = filterFundsService;
            _fundInformationService = fundInformationService;
            _dataTableMockService = dataTableMockService;
            _enumMappingService = enumMappingService;
            _k2Manager = k2Manager;
            _fundsSharepointService = fundsSharepointService;
            _k2IntegrationHelper = new K2IntegrationHelper(
                k2Manager,
                workflowInstanceTaskRepository,
                workflowInstanceRepository);
            _workflowInstanceRepository = workflowInstanceRepository;
            _tcCommonService = tcCommonService;
            _revertFundingProcessService = revertFundingProcessService;
        }

        #endregion

        #region Heartbeat

        public virtual JsonResult Heartbeat()
        {
            return Json("beat");
        }

        #endregion

        #region TCAbstract UI-FP-001

        public virtual ActionResult TCAbstract(string operationNumber)
        {
            var response = GetTCAbstractViewModel(operationNumber);

            if (!response.IsValid)
            {
                SetViewBagErrorMessageInvalidResponse(response);
                return View(TC_ERROR_MSGS);
            }

            var model = response.Model;

            SetViewBagRoles(operationNumber,
                ActionEnum.ConvergenceReadPermission,
                ActionEnum.TCAbstractWritePermission,
                ActionEnum.TCAbstractTLWritePermission);

            var responseRedirect = _tcCommonService
                .GetRedirectionToTCAbstractTask(model.TCId != 0, operationNumber, ViewBag.WriteRole);

            if (!responseRedirect.IsValid)
            {
                SetViewBagErrorMessageInvalidResponse(responseRedirect);
                return View(TC_ERROR_MSGS);
            }

            if (responseRedirect.TaskDataModel != null)
            {
                var responseStore = StoreTaskData(operationNumber, responseRedirect.TaskDataModel);

                if (!responseStore.IsValid)
                {
                    SetViewBagErrorMessageInvalidResponse(responseStore);
                    return View(TC_ERROR_MSGS);
                }

                if (responseRedirect.IsRedirectable)
                    return RedirectToAction(
                        responseRedirect.TaskDataModel.ActionMethod,
                        new { operationNumber = responseRedirect.TaskDataModel.OperationNumber });
            }

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            ViewBag.IsEditable = responseRedirect.IsTCAEditable &&
                model.IsTCAbstractEditableByState;

            model.Header.HasRevertFundingProcessPermission = HasRevertFundingProcessPermission();

            return View(model);
        }

        public virtual ActionResult TCAbstractEdit(string operationNumber, string accessType)
        {
            SetViewBagRoles(operationNumber,
                ActionEnum.ConvergenceReadPermission,
                ActionEnum.TCAbstractWritePermission,
                ActionEnum.TCAbstractTLWritePermission);

            if (!ViewBag.WriteRole)
                return Json(new
                {
                    ErrorMessage = Localization.GetText(TCGlobalValues.NO_WRITE_PERMISSION)
                });

            var errorMessage = SynchronizationHelper.AccessToResources(
                accessType,
                URLConstants.URL_TCABSTRACT,
                operationNumber,
                IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
                return Json(new { ErrorMessage = errorMessage });

            var response = GetTCAbstractViewModel(operationNumber);

            if (!response.IsValid)
                return Json(new { ErrorMessage = response.ErrorMessage });

            var model = response.Model;

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return PartialView(PARTIAL_TC_ABSTRACT, model);
        }

        public virtual FileResult TCAbstractExportToWord(string operationNumber)
        {
            string language = IDBContext.Current.CurrentLanguage;

            var response = _tcAbstractService
                .ExportTCAbstractToFile(operationNumber, OutputFormatEnum.WORDOPENXML, language);

            if (!response.IsValid)
                return null;

            var fileFormat = GetApplicationAndNameReport(
                OutputFormatEnum.WORDOPENXML,
                string.Format("{0}.", operationNumber));

            return File(response.File, fileFormat.ElementAt(0), fileFormat.ElementAt(1));
        }

        public virtual FileResult TCAbstractExportToPDF(string operationNumber)
        {
            var response = _tcAbstractService
                .ExportTCAbstractToFile(operationNumber, OutputFormatEnum.PDF, string.Empty);

            if (!response.IsValid)
                return null;

            var fileFormat = GetApplicationAndNameReport(
                OutputFormatEnum.PDF,
                string.Format("{0}.", operationNumber));

            return File(response.File, fileFormat.ElementAt(0), fileFormat.ElementAt(1));
        }

        #endregion

        #region ValidationTCAbstract UI-FP-002

        public virtual ActionResult ValidationTCAbstract(string operationNumber)
        {
            var model = GetValidationTCAbstractViewModel(operationNumber);

            SetViewBagRoles(
                operationNumber,
                ActionEnum.ConvergenceReadPermission,
                ActionEnum.SWCoordinatorWritePermission);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return View(model);
        }

        public virtual ActionResult ValidationTCAbstractEdit(
            string operationNumber,
            string accessType)
        {
            SetViewBagRoles(
                operationNumber,
                ActionEnum.ConvergenceReadPermission,
                ActionEnum.SWCoordinatorWritePermission);

            var errorMessage = SynchronizationHelper.AccessToResources(
                accessType,
                URLConstants.URL_VALIDATION_TC_ABSTRACT,
                operationNumber,
                IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
                return Json(new { ErrorMessage = errorMessage });

            var model = GetValidationTCAbstractViewModel(operationNumber);

            if (!string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
                return Json(new { ErrorMessage = ViewBag.ErrorMessage });

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return PartialView("Partials/ValidationTCValidation", model);
        }

        #endregion

        #region ReviewTCAbstract UI-FP-003

        public virtual ActionResult ReviewTCAbstract(string operationNumber)
        {
            var response = GetReviewTCAbstractViewModel(operationNumber);

            if (!response.IsValid)
            {
                SetViewBagErrorMessageInvalidResponse(response);
                return View(TC_ERROR_MSGS);
            }

            var model = response.ReviewTCAbstractViewModel ??
                ViewModelInitializerFactory.InitializeReviewTcAbstractViewModel();

            SetViewBagRoles(operationNumber,
                ActionEnum.ConvergenceReadPermission,
                ActionEnum.TCAbstractWritePermission,
                ActionEnum.TCAbstractTLWritePermission);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return View(model);
        }

        public virtual ActionResult ReviewTCAbstractEdit(string operationNumber, string accessType)
        {
            SetViewBagRoles(operationNumber, ActionEnum.ConvergenceReadPermission);

            var errorMessage = SynchronizationHelper.AccessToResources(
                accessType,
                URLConstants.URL_REVIEW_TC_ABSTRACT,
                operationNumber,
                IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
                return Json(new { ErrorMessage = errorMessage });

            var response = GetReviewTCAbstractViewModel(operationNumber);

            if (!response.IsValid)
                return Json(new { ErrorMessage = response.ErrorMessage });

            var model = response.ReviewTCAbstractViewModel ??
                ViewModelInitializerFactory.InitializeReviewTcAbstractViewModel();

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return PartialView(PARTIAL_TC_ABSTRACT, model.TCAbstract);
        }

        #endregion

        #region NotifyFC UI-FP-004

        public virtual ActionResult NotifyFC(string operationNumber)
        {
            SetViewBagRoles(
                operationNumber,
                ActionEnum.ConvergenceReadPermission,
                ActionEnum.SWCoordinatorWritePermission);

            var response = GetReviewersToNotify(operationNumber);

            if (!response.IsValid)
            {
                SetViewBagErrorMessageInvalidResponse(response);

                return View(TC_ERROR_MSGS);
            }

            ViewBag.SerializedViewModel = PageSerializationHelper
                .SerializeObject(response.Model);

            return View(response.Model);
        }

        public virtual ActionResult NotifyFCEdit(string operationNumber, string accessType)
        {
            SetViewBagRoles(
                operationNumber,
                ActionEnum.ConvergenceReadPermission,
                ActionEnum.SWCoordinatorWritePermission);

            var errorMessage = SynchronizationHelper.AccessToResources(
                accessType,
                URLConstants.URL_NOTIFY_FC,
                operationNumber,
                IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
                return Json(new { ErrorMessage = errorMessage });

            var response = GetReviewersToNotify(operationNumber);

            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
                return Json(new { ErrorMessage = response.ErrorMessage });

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(response.Model);

            return PartialView("Partials/NotifyFC/NotifyFCPartial", response.Model);
        }

        #endregion

        #region PriorityTCAbstract UI-FP-005

        public virtual ActionResult PriorityTCAbstract(string operationNumber, TaskDataModel model)
        {
            SetViewBagRoles(ActionEnum.ConvergenceReadPermission);

            SetViewBagRoleCoordinator(RoleEnum.CountryLiaison, operationNumber);

            var response = _reviewRegionalTLService
                .GetPriorityTCAbstract(operationNumber, model.UserProfile);

            if (!response.IsValid)
            {
                SetViewBagErrorMessageInvalidResponse(response);

                return View(TC_ERROR_MSGS);
            }

            SetViewPriorityTCAbstract(response.Model);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(response.Model);

            return View(response.Model);
        }

        public virtual ActionResult PriorityTCAbstractEdit(
            string operationNumber,
            int countryLiaisonId,
            string accessType)
        {
            SetViewBagRoles(ActionEnum.ConvergenceReadPermission);

            SetViewBagRoleCoordinator(RoleEnum.CountryLiaison, operationNumber);

            var errorMessage = SynchronizationHelper.AccessToResources(
                accessType,
                URLConstants.URL_PRIORITY_TC_ABSTRACT,
                operationNumber,
                IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
                return Json(new { ErrorMessage = errorMessage });

            var response = _reviewRegionalTLService
                .GetPriorityTCAbstract(operationNumber, countryLiaisonId);

            if (!response.IsValid)
            {
                SetViewBagErrorMessageInvalidResponse(response);

                return View(TC_ERROR_MSGS);
            }

            SetViewPriorityTCAbstract(response.Model);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(response.Model);

            return PartialView("Partials/PriorityTCPriority", response.Model);
        }

        #endregion

        #region FundCoordinatorTCAbstract UI-FP-006

        public virtual ActionResult FundCoordinatorTCAbstract(
            string operationNumber,
            TaskDataModel model)
        {
            var fundId = 0;
            var responseFundID = TryParseFundID(operationNumber, model, out fundId);

            if (!responseFundID.IsValid)
            {
                SetViewBagErrorMessageInvalidResponse(responseFundID);
                return View(TC_ERROR_MSGS);
            }

            var viewModel = GetFundCoordinatorTCAbstractViewModel(operationNumber, fundId);

            SetViewBagRoles(ActionEnum.ConvergenceReadPermission);

            SetViewBagRoleCoordinator(RoleEnum.FundCoordinator, operationNumber);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(viewModel);

            ViewBag.HiddenTrustFundId = fundId;

            return View(viewModel);
        }

        public virtual ActionResult FundCoordinatorTCAbstractEdit(
            string operationNumber,
            int trustFundId,
            string accessType)
        {
            SetViewBagRoles(ActionEnum.ConvergenceReadPermission);

            SetViewBagRoleCoordinator(RoleEnum.FundCoordinator, operationNumber);

            var errorMessage = SynchronizationHelper.AccessToResources(
                accessType,
                URLConstants.URL_FUND_COORDINATOR_TC_ABSTRACT,
                operationNumber,
                IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
                return Json(new { ErrorMessage = errorMessage });

            var model = GetFundCoordinatorTCAbstractViewModel(operationNumber, trustFundId);

            if (ViewBag.IsValid == false && !string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
                return Json(new { ErrorMessage = ViewBag.ErrorMessage });

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            var contentHTML = this.RenderRazorViewToString(
                "Partials/FundCoordinatorReview", model);

            return Json(new { ViewBag.ErrorMessage, ViewBag.IsValid, ContentHTML = contentHTML });
        }

        #endregion

        #region SingleWindowMeeting UI-FP-007

        public virtual ActionResult SingleWindowMeeting(
            string fromDate = null,
            string toDate = null)
        {
            DateTime? fromDateParsed = null;
            DateTime? toDateParsed = null;

            if (!string.IsNullOrWhiteSpace(fromDate))
                fromDateParsed = DateTime.ParseExact(
                    fromDate,
                    ClientFieldDataMappers.DATETIME_PARSE_FORMAT,
                    CultureInfo.InvariantCulture);

            if (!string.IsNullOrWhiteSpace(toDate))
                toDateParsed = DateTime.ParseExact(
                    toDate,
                    ClientFieldDataMappers.DATETIME_PARSE_FORMAT,
                    CultureInfo.InvariantCulture);

            var model = GetSingleWindoMeetingViewModel(fromDateParsed, toDateParsed);

            SetViewBagRoles(
                ActionEnum.MyMeetingsReadPermission,
                ActionEnum.MyMeetingsWritePermission);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return View(model);
        }

        public virtual FileResult SWMeetingCreateExternalMinutesWord(
            string operationNumber,
            string fromDate,
            string toDate)
        {
            DateTime? fromDateParsed = null;
            DateTime? toDateParsed = null;

            if (!string.IsNullOrWhiteSpace(fromDate))
                fromDateParsed = DateTime.ParseExact(
                    fromDate,
                    ClientFieldDataMappers.DATETIME_PARSE_FORMAT,
                    CultureInfo.InvariantCulture);

            if (!string.IsNullOrWhiteSpace(toDate))
                toDateParsed = DateTime.ParseExact(
                    toDate,
                    ClientFieldDataMappers.DATETIME_PARSE_FORMAT,
                    CultureInfo.InvariantCulture);

            var response = _singleWindowMeetingService.CreateExternalMinutes(
                OutputFormatEnum.Word,
                IDBContext.Current.CurrentLanguage,
                fromDateParsed,
                toDateParsed);

            if (!response.IsValid)
                return null;

            return File(response.File, "application/doc", "SWindowMeetingExternalMinutes.doc");
        }

        public virtual FileResult SWMeetingCreateExternalMinutesExcel(
            string operationNumber,
            string fromDate,
            string toDate)
        {
            DateTime? fromDateParsed = null;
            DateTime? toDateParsed = null;

            if (!string.IsNullOrWhiteSpace(fromDate))
                fromDateParsed = DateTime.ParseExact(
                    fromDate,
                    ClientFieldDataMappers.DATETIME_PARSE_FORMAT,
                    CultureInfo.InvariantCulture);

            if (!string.IsNullOrWhiteSpace(toDate))
                toDateParsed = DateTime.ParseExact(
                    toDate,
                    ClientFieldDataMappers.DATETIME_PARSE_FORMAT,
                    CultureInfo.InvariantCulture);

            var response = _singleWindowMeetingService.CreateExternalMinutes(
                OutputFormatEnum.Excel,
                IDBContext.Current.CurrentLanguage,
                fromDateParsed,
                toDateParsed);

            if (!response.IsValid)
                return null;

            return File(
                response.File,
                "application/xls",
                "SWindowMeetingExternalMinutesExcel.xls");
        }

        public virtual FileResult SingleWindowMeetingExportToExcel(
            string operationNumber,
            string fromDate,
            string toDate)
        {
            return SingleWindowMeetingExportAllFormat(
                OutputFormatEnum.Excel,
                operationNumber,
                fromDate,
                toDate);
        }

        public virtual FileResult SingleWindowMeetingExportToWord(
            string operationNumber,
            string fromDate,
            string toDate)
        {
            return SingleWindowMeetingExportAllFormat(
                OutputFormatEnum.Word,
                operationNumber,
                fromDate,
                toDate);
        }

        public virtual FileResult SingleWindowMeetingExportToPDF(
            string operationNumber,
            string fromDate,
            string toDate)
        {
            return SingleWindowMeetingExportAllFormat(
                OutputFormatEnum.PDF,
                operationNumber,
                fromDate,
                toDate);
        }

        public virtual FileResult SWMeetingCreateInternalMinutesExcel(
            string operationNumber,
            string fromDate,
            string toDate)
        {
            DateTime? fromDateParsed = null;
            DateTime? toDateParsed = null;

            if (!string.IsNullOrWhiteSpace(fromDate))
                fromDateParsed = DateTime.ParseExact(
                    fromDate,
                    ClientFieldDataMappers.DATETIME_PARSE_FORMAT,
                    CultureInfo.InvariantCulture);

            if (!string.IsNullOrWhiteSpace(toDate))
                toDateParsed = DateTime.ParseExact(
                    toDate,
                    ClientFieldDataMappers.DATETIME_PARSE_FORMAT,
                    CultureInfo.InvariantCulture);

            var response = _singleWindowMeetingService
                .CreateInternalMinutes(OutputFormatEnum.Excel, fromDateParsed, toDateParsed);

            if (!response.IsValid)
                return null;

            return File(
                response.File,
                "application/xls",
                "SWindowMeetingInternalMinutesExcel.xls");
        }

        public virtual FileResult SWMeetingCreateInternalMinutesWord(
            string operationNumber,
            string fromDate,
            string toDate)
        {
            DateTime? fromDateParsed = null;
            DateTime? toDateParsed = null;

            if (!string.IsNullOrWhiteSpace(fromDate))
                fromDateParsed = DateTime.ParseExact(
                    fromDate,
                    ClientFieldDataMappers.DATETIME_PARSE_FORMAT,
                    CultureInfo.InvariantCulture);

            if (!string.IsNullOrWhiteSpace(toDate))
                toDateParsed = DateTime.ParseExact(
                    toDate,
                    ClientFieldDataMappers.DATETIME_PARSE_FORMAT,
                    CultureInfo.InvariantCulture);

            var response = _singleWindowMeetingService
                .CreateInternalMinutes(OutputFormatEnum.Word, fromDateParsed, toDateParsed);

            if (!response.IsValid)
                return null;

            return File(response.File, "application/doc", "SWindowMeetingInternalMinutes.doc");
        }

        public virtual ActionResult SingleWindowMeetingFilter(
            string fromDate,
            string toDate,
            string accessType)
        {
            DateTime? fromDateParsed = null;
            DateTime? toDateParsed = null;

            if (!string.IsNullOrWhiteSpace(fromDate))
                fromDateParsed = DateTime.ParseExact(
                    fromDate,
                    ClientFieldDataMappers.DATETIME_PARSE_FORMAT,
                    CultureInfo.InvariantCulture);

            if (!string.IsNullOrWhiteSpace(toDate))
                toDateParsed = DateTime.ParseExact(
                    toDate,
                    ClientFieldDataMappers.DATETIME_PARSE_FORMAT,
                    CultureInfo.InvariantCulture);

            var model = GetSingleWindoMeetingViewModel(fromDateParsed, toDateParsed);

            if (!string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
                return Json(new { ErrorMessage = ViewBag.ErrorMessage });

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return PartialView("Partials/DataTables/SingleWindowMeetingTable", model);
        }

        public virtual ActionResult SWMeetingGeneralComments(int tcAbstractId)
        {
            var response = GetMeetingGeneralComments(tcAbstractId);

            if (response.IsValid)
            {
                ViewBag.UserName = IDBContext.Current.UserName;
                ViewBag.SerializedViewModel = PageSerializationHelper
                    .SerializeObject(response.MeetingListGeneralComments);

                return PartialView(
                    "Partials/SingleWindowMeeting/SWmeetingComments",
                    response.MeetingListGeneralComments);
            }

            return Json(response);
        }

        public virtual ActionResult SWMeetingHistoricalFunds(int tcAbstractId)
        {
            var response = GetMeetingHistoricalFunds(tcAbstractId);
            response.IsValid = true;

            if (response.IsValid)
                return PartialView(
                    "Partials/SingleWindowMeeting/SWmeetingHistoricalFunds",
                    response.MeetingHistoricalFunds);

            return Json(response);
        }

        #endregion

        #region SingleWindowOperations UI-FP-008

        public virtual ActionResult SingleWindowOperations(
            SWTypeDecisionFilterEnum typeDecision = SWTypeDecisionFilterEnum.AllOperations,
            string fromDate = null,
            string toDate = null)
        {
            DateTime? fromDateParsed = null;
            DateTime? toDateParsed = null;

            if (!string.IsNullOrWhiteSpace(fromDate))
                fromDateParsed = DateTime.ParseExact(
                    fromDate,
                    ClientFieldDataMappers.DATETIME_PARSE_FORMAT,
                    CultureInfo.InvariantCulture);

            if (!string.IsNullOrWhiteSpace(toDate))
                toDateParsed = DateTime.ParseExact(
                    toDate,
                    ClientFieldDataMappers.DATETIME_PARSE_FORMAT,
                    CultureInfo.InvariantCulture);

            var model = GetSingleWindowOperationsViewModel(
                typeDecision, fromDateParsed, toDateParsed);

            SetViewBagRoles(
                ActionEnum.SingleWindowOperationsReadPermission,
                ActionEnum.SingleWindowOperationsWritePermission);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return View(model);
        }

        public virtual FileResult SingleWindowOperationsExportToExcel(
            SWTypeDecisionFilterEnum typeDecision = SWTypeDecisionFilterEnum.AllOperations,
            string fromDate = null,
            string toDate = null)
        {
            return SingleWindowOperationsExportAllFormat(
                OutputFormatEnum.Excel,
                typeDecision,
                fromDate,
                toDate);
        }

        public virtual FileResult SingleWindowOperationsExportToWord(
            SWTypeDecisionFilterEnum typeDecision = SWTypeDecisionFilterEnum.AllOperations,
            string fromDate = null,
            string toDate = null)
        {
            return SingleWindowOperationsExportAllFormat(
                OutputFormatEnum.Word,
                typeDecision,
                fromDate,
                toDate);
        }

        public virtual FileResult SingleWindowOperationsExportToPDF(
            SWTypeDecisionFilterEnum typeDecision = SWTypeDecisionFilterEnum.AllOperations,
            string fromDate = null,
            string toDate = null)
        {
            return SingleWindowOperationsExportAllFormat(
                OutputFormatEnum.PDF,
                typeDecision,
                fromDate,
                toDate);
        }

        public virtual ActionResult SingleWindowOperationsFilter(
            SWTypeDecisionFilterEnum typeDecision = SWTypeDecisionFilterEnum.AllOperations,
            string fromDate = null,
            string toDate = null,
            string accessType = null)
        {
            DateTime? fromDateParsed = null;
            DateTime? toDateParsed = null;

            if (!string.IsNullOrWhiteSpace(fromDate))
                fromDateParsed = DateTime.ParseExact(
                    fromDate,
                    ClientFieldDataMappers.DATETIME_PARSE_FORMAT,
                    CultureInfo.InvariantCulture);

            if (!string.IsNullOrWhiteSpace(toDate))
                toDateParsed = DateTime.ParseExact(
                    toDate,
                    ClientFieldDataMappers.DATETIME_PARSE_FORMAT,
                    CultureInfo.InvariantCulture);

            var model = GetSingleWindowOperationsViewModel(
                typeDecision, fromDateParsed, toDateParsed);

            if (!string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
                return Json(new { ErrorMessage = ViewBag.ErrorMessage });

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return PartialView("Partials/DataTables/SingleWindowOperationsTable", model);
        }

        public virtual ActionResult SWOperationsGeneralComments(int tcAbstractId)
        {
            var response = GetOperationGeneralComments(tcAbstractId);

            if (response.IsValid)
            {
                ViewBag.SerializedViewModel = PageSerializationHelper
                    .SerializeObject(response.MeetingGeneralCommentsViewModel);

                return PartialView(
                    "Partials/SingleWindowOperations/SWOperationsComments",
                    response.MeetingGeneralCommentsViewModel);
            }

            return Json(response);
        }

        #endregion

        #region DecisionSWOperation UI-FP-009

        public virtual ActionResult DecisionSWOperation(
            string operationNumber,
            SWTypeDecisionFilterEnum typeDecision = SWTypeDecisionFilterEnum.AllOperations,
            DateTime? SWDateFrom = null,
            DateTime? SWDateTo = null)
        {
            var model = GetDecisionSingleWindowOperation(operationNumber);

            ViewBag.SWDateFrom = SWDateFrom;
            ViewBag.SWDateTo = SWDateTo;
            ViewBag.TypeDecision = typeDecision;

            SetViewBagRoles(
                ActionEnum.SingleWindowOperationsReadPermission,
                ActionEnum.SingleWindowOperationsWritePermission,
                operationNumber);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return View(model);
        }

        public virtual ActionResult DecisionSWOperationEdit(
            string operationNumber,
            string accessType)
        {
            SetViewBagRoles(
                ActionEnum.SingleWindowOperationsReadPermission,
                ActionEnum.SingleWindowOperationsWritePermission,
                operationNumber);

            if (!ViewBag.WriteRole)
                return Json(
                    new
                    {
                        ErrorMessage = Localization.GetText(TCGlobalValues.NO_WRITE_PERMISSION)
                    });

            var errorMessage = SynchronizationHelper.AccessToResources(
                accessType,
                URLConstants.URL_DECISION_SW_OPERATION,
                operationNumber,
                IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
                return Json(new { ErrorMessage = errorMessage });

            var model = GetDecisionSingleWindowOperation(operationNumber);

            if (!string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
                return Json(new { ErrorMessage = ViewBag.ErrorMessage });

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return PartialView("Partials/DecisionSWOperationTCSWOperations", model);
        }

        #endregion

        #region SingleWindowDecision UI-FP-010

        public virtual ActionResult SingleWindowDecision(
            string operationNumber,
            DateTime? SWMDateFrom,
            DateTime? SWMDateTo)
        {
            var model = GetSingleWindowDecisionViewModel(operationNumber);

            ViewBag.SWMDateFrom = SWMDateFrom;
            ViewBag.SWMDateTo = SWMDateTo;

            SetViewBagRoles(
                ActionEnum.MyMeetingsReadPermission,
                ActionEnum.MyMeetingsWritePermission,
                operationNumber);

            SetViewBagRoleCoordinator(RoleEnum.SingleWindowCoordinator);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            ViewBag.WorkFlowRunning = _k2Manager
                .IsWorkflowRunning(WorkFlowTypeEnum.WFFP002.GetEnumDescription(), operationNumber)
                .IsWorkflowActive;

            return View(model);
        }

        public virtual ActionResult SingleWindowDecisionEdit(
            string operationNumber,
            string accessType)
        {
            SetViewBagRoles(
                ActionEnum.MyMeetingsReadPermission,
                ActionEnum.MyMeetingsWritePermission,
                 operationNumber);

            if (!ViewBag.WriteRole)
                return Json(
                    new
                    {
                        ErrorMessage = Localization.GetText(TCGlobalValues.NO_WRITE_PERMISSION)
                    });

            var errorMessage = SynchronizationHelper.AccessToResources(
                accessType,
                URLConstants.URL_SINGLE_WINDOW_DECISION,
                operationNumber,
                IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
                return Json(new { ErrorMessage = errorMessage });

            var model = GetSingleWindowDecisionViewModel(operationNumber);

            if (!string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
                return Json(new { ErrorMessage = ViewBag.ErrorMessage });

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            string workflowTypeFP1 = WorkflowCodes.WF_FP_001.GetStringValue();

            ViewBag.IsPossibleToAddMoreFunds = _workflowInstanceRepository
                .GetByCriteria(w =>
                    w.WorkflowType.Code == workflowTypeFP1 &&
                    w.FolioId.Contains(operationNumber) &&
                    w.Status == TCGlobalValues.APPROVED)
                .Any();

            SetViewBagRoleCoordinator(RoleEnum.SingleWindowCoordinator);

            return PartialView("Partials/SingleWDecision/SingleWindowDecision", model);
        }

        public virtual FileResult DownloadEmailEligibilityDecision(string operationNumber)
        {
            var response = _singleWindowDecisionService
                .DownloadFileSingleWindowDecision(operationNumber);

            if (!response.IsValid)
                return null;

            return File(response.File, "application/docx", "SingleWindowDecisionReport.docx");
        }

        #endregion

        #region EligibilityDecision UI-FP-011

        public virtual ActionResult EligibilityDecision(
            string operationNumber,
            TaskDataModel model)
        {
            var fundId = 0;
            var responseFundID = TryParseFundID(operationNumber, model, out fundId);

            if (!responseFundID.IsValid)
            {
                SetViewBagErrorMessageInvalidResponse(responseFundID);
                return View(TC_ERROR_MSGS);
            }

            if (fundId == 0)
            {
                SetViewBagErrorMessageInvalidResponse(new ResponseBase
                {
                    IsValid = false,
                    ErrorMessage = Localization.GetText(TCGlobalValues.GET_DECISION_ERROR_MESSAGE)
                });
                return View(TC_ERROR_MSGS);
            }

            var viewModel = GetEligibilityDecisionViewModel(operationNumber, fundId);

            SetViewBagRoles(ActionEnum.ConvergenceReadPermission);

            SetViewBagRoleCoordinator(RoleEnum.FundCoordinator, operationNumber);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(viewModel);

            SetCanEditViewBag(
                (bool)ViewBag.IsCoordinator,
                !viewModel.IsRejected && !viewModel.IsPendingFunding &&
                    viewModel.AwardFundEligibility.IsCommitEnabled);

            return View(viewModel);
        }

        public virtual ActionResult EligibilityDecisionEdit(
            string operationNumber,
            int trustFundId,
            string accessType)
        {
            SetViewBagRoles(ActionEnum.ConvergenceReadPermission);

            SetViewBagRoleCoordinator(RoleEnum.FundCoordinator, operationNumber);

            var errorMessage = SynchronizationHelper.AccessToResources(
                accessType,
                URLConstants.URL_ELIGIBILITY_DECISION,
                operationNumber,
                IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
                return Json(new { ErrorMessage = errorMessage });

            var model = GetEligibilityDecisionViewModel(operationNumber, trustFundId);

            if (ViewBag.IsValid == false && !string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
                return Json(new { ErrorMessage = ViewBag.ErrorMessage });

            SetCanEditViewBag(
                (bool)ViewBag.IsCoordinator,
                !model.IsRejected && !model.IsPendingFunding &&
                    model.AwardFundEligibility.IsCommitEnabled);

            if ((SynchronizationHelper.EDIT_MODE == accessType) && !ViewBag.CanEdit)
                return Json(
                    new
                    {
                        ErrorMessage = Localization.GetText(TCGlobalValues.NO_WRITE_PERMISSION)
                    });

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            var contentHTML = this.RenderRazorViewToString("Partials/EligibilityDecision", model);

            return Json(new { ViewBag.ErrorMessage, ViewBag.IsValid, ContentHTML = contentHTML });
        }

        #endregion

        #region ReviewPostValidation UI-FP-012

        public virtual ActionResult ReviewPostValidation(
            string operationNumber,
            TaskDataModel model)
        {
            if (model == null || model.Parameters == null)
            {
                var responseRetrieve = RetrieveTaskDataToFund(
                    operationNumber,
                    WorkflowTaskTypeCodes.WF_FP_002_T4.GetStringValue(),
                    out model);

                if (!responseRetrieve.IsValid)
                {
                    SetViewBagErrorMessageInvalidResponse(responseRetrieve);
                    return View(TC_ERROR_MSGS);
                }
            }

            var fundId = 0;
            var responseFundID = TryParseFundID(operationNumber, model, out fundId);

            if (!responseFundID.IsValid)
            {
                SetViewBagErrorMessageInvalidResponse(responseFundID);
                return View(TC_ERROR_MSGS);
            }

            if (fundId == 0)
            {
                SetViewBagErrorMessageInvalidResponse(new ResponseBase
                {
                    IsValid = false,
                    ErrorMessage = Localization.GetText(TCGlobalValues.GET_DECISION_ERROR_MESSAGE)
                });
                return View(TC_ERROR_MSGS);
            }

            var response = GetReviewPostValidationViewModel(operationNumber, fundId);

            if (!response.IsValid)
            {
                SetViewBagErrorMessageInvalidResponse(response);
                return View(TC_ERROR_MSGS);
            }

            var viewModel = response.ReviewTCAbstractPostValidationViewModel ??
                ViewModelInitializerFactory.InitializeReviewPostValidationViewModel();

            SetViewBagRoles(operationNumber,
                ActionEnum.ConvergenceReadPermission,
                ActionEnum.TCAbstractWritePermission,
                ActionEnum.TCAbstractTLWritePermission);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(viewModel);

            SetCanEditViewBag((bool)ViewBag.WriteRole,
                !viewModel.IsRejected && !viewModel.IsPendingFunding);

            return View(viewModel);
        }

        public virtual ActionResult ReviewPostValidationEdit(
            string operationNumber,
            string accessType,
            int trustFundId)
        {
            SetViewBagRoles(operationNumber, ActionEnum.ConvergenceReadPermission);

            var errorMessage = SynchronizationHelper.AccessToResources(
                accessType,
                URLConstants.URL_REVIEW_POST_VALIDATION,
                operationNumber,
                IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
                return Json(new { ErrorMessage = errorMessage });

            var response = GetReviewPostValidationViewModel(operationNumber, trustFundId);

            if (!response.IsValid)
                return Json(new { ErrorMessage = response.ErrorMessage });

            var model = response.ReviewTCAbstractPostValidationViewModel ??
                ViewModelInitializerFactory.InitializeReviewPostValidationViewModel();

            SetCanEditViewBag(
                (bool)ViewBag.ReadRole, !model.IsRejected && !model.IsPendingFunding);

            if (SynchronizationHelper.EDIT_MODE == accessType && !ViewBag.CanEdit)
                return Json(
                    new
                    {
                        ErrorMessage = Localization.GetText(TCGlobalValues.NO_WRITE_PERMISSION)
                    });

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return PartialView(PARTIAL_TC_ABSTRACT, model.TCAbstract);
        }

        #endregion

        #region DonorDecision UI-FP-013

        public virtual ActionResult DonorDecision(string operationNumber, TaskDataModel model)
        {
            var fundId = 0;
            var responseFundID = TryParseFundID(operationNumber, model, out fundId);

            if (!responseFundID.IsValid)
            {
                SetViewBagErrorMessageInvalidResponse(responseFundID);
                return View(TC_ERROR_MSGS);
            }

            if (fundId == 0)
            {
                SetViewBagErrorMessageInvalidResponse(new ResponseBase
                {
                    IsValid = false,
                    ErrorMessage = Localization.GetText(TCGlobalValues.GET_DECISION_ERROR_MESSAGE)
                });

                return View(TC_ERROR_MSGS);
            }

            var viewModel = GetDonorDecisionViewModel(operationNumber, fundId);

            SetViewBagRoles(ActionEnum.ConvergenceReadPermission);

            SetViewBagRoleCoordinator(RoleEnum.FundCoordinator, operationNumber);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(viewModel);

            SetCanEditViewBag(
                (bool)ViewBag.IsCoordinator,
                !viewModel.IsRejected && !viewModel.IsPendingFunding &&
                    viewModel.AwardFundEligibility.IsCommitEnabled);

            return View(viewModel);
        }

        public virtual ActionResult DonorDecisionEdit(
            string operationNumber,
            int trustFundId,
            int singleWindowsMeetingId,
            string accessType)
        {
            SetViewBagRoles(ActionEnum.ConvergenceReadPermission);

            SetViewBagRoleCoordinator(RoleEnum.FundCoordinator, operationNumber);

            var errorMessage = SynchronizationHelper.AccessToResources(
                accessType,
                URLConstants.URL_DONOR_DECISION,
                operationNumber,
                IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
                return Json(new { ErrorMessage = errorMessage });

            var model = GetDonorDecisionViewModel(operationNumber, trustFundId);

            if (ViewBag.IsValid == false && !string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
                return Json(new { ErrorMessage = ViewBag.ErrorMessage });

            SetCanEditViewBag(
                (bool)ViewBag.IsCoordinator,
                !model.IsRejected && !model.IsPendingFunding &&
                    model.AwardFundEligibility.IsCommitEnabled);

            if ((SynchronizationHelper.EDIT_MODE == accessType) && !ViewBag.CanEdit)
                return Json(
                    new
                    {
                        ErrorMessage = Localization.GetText(TCGlobalValues.NO_WRITE_PERMISSION)
                    });

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            var contentHTML = this.RenderRazorViewToString("Partials/DonorDecision", model);

            return Json(new { ViewBag.ErrorMessage, ViewBag.IsValid, ContentHTML = contentHTML });
        }

        #endregion

        #region EscDecision UI-FP-014

        public virtual ActionResult ESCDecision(string operationNumber, TaskDataModel model)
        {
            var fundId = 0;
            var responseFundID = TryParseFundID(operationNumber, model, out fundId);

            if (!responseFundID.IsValid)
            {
                SetViewBagErrorMessageInvalidResponse(responseFundID);
                return View(TC_ERROR_MSGS);
            }

            if (fundId == 0)
            {
                SetViewBagErrorMessageInvalidResponse(new ResponseBase
                {
                    IsValid = false,
                    ErrorMessage = Localization.GetText(TCGlobalValues.GET_DECISION_ERROR_MESSAGE)
                });
                return View(TC_ERROR_MSGS);
            }

            var viewModel = GetEscDecisionViewModel(operationNumber, fundId);

            SetViewBagRoles(ActionEnum.ConvergenceReadPermission);

            SetViewBagRoleCoordinator(RoleEnum.FundCoordinator, operationNumber);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(viewModel);

            SetCanEditViewBag(
                (bool)ViewBag.IsCoordinator,
                !viewModel.IsRejected && !viewModel.IsPendingFunding &&
                    viewModel.AwardFundEligibility.IsCommitEnabled);

            return View(viewModel);
        }

        public virtual ActionResult ESCDecisionEdit(
            string operationNumber,
            int trustFundId,
            int singleWindowsMeetingId,
            string accessType)
        {
            SetViewBagRoles(ActionEnum.ConvergenceReadPermission);

            SetViewBagRoleCoordinator(RoleEnum.FundCoordinator, operationNumber);

            var errorMessage = SynchronizationHelper.AccessToResources(
                accessType,
                URLConstants.URL_ESC_DECISION,
                operationNumber,
                IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
                return Json(new { ErrorMessage = errorMessage });

            var model = GetEscDecisionViewModel(operationNumber, trustFundId);

            if (ViewBag.IsValid == false && !string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
                return Json(new { ErrorMessage = ViewBag.ErrorMessage });

            SetCanEditViewBag(
                (bool)ViewBag.IsCoordinator,
                !model.IsRejected && !model.IsPendingFunding &&
                    model.AwardFundEligibility.IsCommitEnabled);

            if ((SynchronizationHelper.EDIT_MODE == accessType) && !ViewBag.CanEdit)
                return Json(
                    new
                    {
                        ErrorMessage = Localization.GetText(TCGlobalValues.NO_WRITE_PERMISSION)
                    });

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            var contentHTML = this.RenderRazorViewToString("Partials/ESCDecision", model);

            return Json(new { ViewBag.ErrorMessage, ViewBag.IsValid, ContentHTML = contentHTML });
        }

        public virtual ActionResult GetAwardFundEscDecision(
            string operationNumber,
            int trustFundId,
            int singleWindowsMeetingId)
        {
            var response = _escDecisionService.GetESCDecisionAwardFundEligibility(
                operationNumber,
                trustFundId,
                singleWindowsMeetingId);

            SetViewBagRoles(operationNumber, ActionEnum.ConvergenceReadPermission);

            ViewBag.SerializedViewModel = PageSerializationHelper
                .SerializeObject(response.AwardFundEligibilityViewModel);

            SetViewBagAwardFundEligibility(response.AwardFundEligibilityViewModel);

            return PartialView(
                "Partials/AwardFundEligibility/AwardFundEligibilityPartial",
                response.AwardFundEligibilityViewModel);
        }

        #endregion

        #region RequestIncrease UI-FP-016

        public virtual ActionResult RequestIncrease(string operationNumber)
        {
            var viewModel = GetRequestIncreaseViewModel(operationNumber);

            SetViewBagRoles(operationNumber, ActionEnum.ConvergenceReadPermission);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(viewModel);

            return View(viewModel);
        }

        public virtual ActionResult RequestIncreaseEdit(string operationNumber, string accessType)
        {
            SetViewBagRoles(operationNumber, ActionEnum.ConvergenceReadPermission);

            var errorMessage = SynchronizationHelper.AccessToResources(
                accessType,
                URLConstants.URL_REQUEST_INCREASE,
                operationNumber,
                IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
                return Json(new { ErrorMessage = errorMessage });

            var model = GetRequestIncreaseViewModel(operationNumber);

            if (!string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
                return Json(new { ErrorMessage = ViewBag.ErrorMessage });

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return PartialView("Partials/RequestIncrease", model);
        }

        public virtual FileResult RequestIncreaseExportToWord(string operationNumber)
        {
            var response = _requestIncreaseService
                .ExportRequestIncreaseToFile(operationNumber, OutputFormatEnum.Word);

            if (!response.IsValid)
                return null;

            return File(response.File, "application/doc", "RequestIncrease.doc");
        }

        public virtual FileResult RequestIncreaseExportToPDF(string operationNumber)
        {
            var response = _requestIncreaseService
                .ExportRequestIncreaseToFile(operationNumber, OutputFormatEnum.PDF);

            if (!response.IsValid)
                return null;

            return File(response.File, "application/doc", "RequestIncrease.pdf");
        }

        #endregion

        #region ReviewNotifyFC UI-FP-017

        public virtual ActionResult ReviewNotifyFC(string operationNumber)
        {
            var model = GetTCReviewNotifyFCViewModel(operationNumber);

            SetViewBagRoles(
                operationNumber,
                ActionEnum.ConvergenceReadPermission,
                ActionEnum.SWCoordinatorWritePermission);

            SetViewBagRoleCoordinator(RoleEnum.SingleWindowCoordinator);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return View(model);
        }

        public virtual ActionResult ReviewNotifyFCEdit(string operationNumber, string accessType)
        {
            SetViewBagRoles(
                operationNumber,
                ActionEnum.ConvergenceReadPermission,
                ActionEnum.SWCoordinatorWritePermission);

            SetViewBagRoleCoordinator(RoleEnum.SingleWindowCoordinator);

            var errorMessage = SynchronizationHelper.AccessToResources(
                accessType,
                URLConstants.URL_REVIEW_NOTIFY_FC,
                operationNumber,
                IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
                return Json(new { ErrorMessage = errorMessage });

            var model = GetTCReviewNotifyFCViewModel(operationNumber, true);

            if (!string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
                return Json(new { ErrorMessage = ViewBag.ErrorMessage });

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return PartialView("Partials/NotifyFC/ReviewNotifyFCPartial", model);
        }

        #endregion

        #region RevertFundingProcess

        public virtual JsonResult ReturnToProjectTeam()
        {
            var response = _revertFundingProcessService
                .ReturnToProjectTeam(IDBContext.Current.Operation);

            string statusMessage = string.Empty;

            if (response.IsValid)
                statusMessage = response.OperationStatus;

            if (!string.IsNullOrEmpty(response.MilestonesStatus))
                statusMessage += response.MilestonesStatus;

            return Json(
                new
                {
                    data = new
                    {
                        isValid = response.IsValid,
                        message = statusMessage,
                        errorMessage = response.ErrorMessage,
                    }
                });
        }

        #endregion

        #region Filter Funds

        public virtual ActionResult FilterFunds(string operationNumber)
        {
            var response = _filterFundsService.GetFilterFunds();

            SetViewBagRoles(ActionEnum.ConvergenceReadPermission);
            SetViewBagRoleCoordinator(RoleEnum.FundCoordinator);
            SetViewBagFilterFunds();

            SetViewBagErrorMessageInvalidResponse(response);

            return View(response.FilterFundsViewModel);
        }

        #endregion

        #region Fund Information Get Data

        public virtual ActionResult FundInformation(string operationNumber, int fundId)
        {
            var model = GetFundInformationViewModel(fundId);

            SetViewBagGlobalRolesList(
                ActionEnum.ConvergenceReadPermission,
                ActionEnum.GCMReadPermission,
                ActionEnum.FCWritePermission,
                ActionEnum.SWCoordinatorWritePermission,
                ActionEnum.GCMChiefWrite);

            SetViewBagRoleFundCoordinator(fundId);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return View(model);
        }

        public virtual ActionResult FundInformationEditBasicData(
            string operationNumber,
            int fundId,
            string accessType)
        {
            SetViewBagGlobalRolesList(
                ActionEnum.ConvergenceReadPermission,
                ActionEnum.GCMReadPermission,
                ActionEnum.FCWritePermission,
                ActionEnum.SWCoordinatorWritePermission,
                ActionEnum.GCMChiefWrite);

            var errorMessage = SynchronizationHelper.AccessToResources(
                accessType,
                URLConstants.URL_FUND_INFORMATION,
                fundId.ToString(),
                IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
                return Json(new { ErrorMessage = errorMessage });

            var model = GetFundInformationBasicDataViewModel(fundId);

            if (!ViewBag.IsValid && !string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
                return Json(new { ErrorMessage = ViewBag.ErrorMessage });

            SetViewBagRoleFundCoordinator(fundId);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            var contentHTML = this.RenderRazorViewToString(
                "Partials/FundInformation/FIData", model);

            return Json(new { ViewBag.ErrorMessage, ViewBag.IsValid, ContentHTML = contentHTML });
        }

        public virtual ActionResult FundInformationEditFundedOperations(
            string operationNumber,
            int fundId,
            string accessType)
        {
            SetViewBagGlobalRolesList(
                ActionEnum.ConvergenceReadPermission,
                ActionEnum.GCMReadPermission,
                ActionEnum.FCWritePermission,
                ActionEnum.SWCoordinatorWritePermission,
                ActionEnum.GCMChiefWrite);

            var errorMessage = SynchronizationHelper.AccessToResources(
                accessType,
                URLConstants.URL_FUND_INFORMATION,
                fundId.ToString(),
                IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
                return Json(new { ErrorMessage = errorMessage });

            var model = GetFundInformationFundedOperationsViewModel(fundId);

            if (!ViewBag.IsValid && !string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
                return Json(new { ErrorMessage = ViewBag.ErrorMessage });

            SetViewBagRoleFundCoordinator(fundId);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            var contentHTML = this.RenderRazorViewToString(
                "Partials/FundInformation/FIFundedOperations", model);

            return Json(new { ViewBag.ErrorMessage, ViewBag.IsValid, ContentHTML = contentHTML });
        }

        public virtual ActionResult FundInformationEditDocuments(
            string operationNumber,
            int fundId,
            string accessType)
        {
            SetViewBagGlobalRolesList(
                ActionEnum.ConvergenceReadPermission,
                ActionEnum.GCMReadPermission,
                ActionEnum.FCWritePermission,
                ActionEnum.SWCoordinatorWritePermission,
                ActionEnum.GCMChiefWrite);

            var errorMessage = SynchronizationHelper.AccessToResources(
                accessType,
                URLConstants.URL_FUND_INFORMATION,
                fundId.ToString(),
                IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
                return Json(new { ErrorMessage = errorMessage });

            var model = GetFundInformationDocumentsViewModel(fundId);

            if (!ViewBag.IsValid && !string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
                return Json(new { ErrorMessage = ViewBag.ErrorMessage });

            SetViewBagRoleFundCoordinator(fundId);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            var contentHTML = this.RenderRazorViewToString(
                "Partials/FundInformation/FIDocumentsTable", model);

            return Json(new { ViewBag.ErrorMessage, ViewBag.IsValid, ContentHTML = contentHTML });
        }

        public virtual ActionResult DonorContactDetails(string donorName, int fundId)
        {
            var model = GetDonorContactDetailsEdit(donorName, fundId);

            SetViewBagGlobalRolesList(
                ActionEnum.ConvergenceReadPermission,
                ActionEnum.GCMReadPermission,
                ActionEnum.FCWritePermission,
                ActionEnum.SWCoordinatorWritePermission,
                ActionEnum.GCMChiefWrite);

            SetViewBagRoleFundCoordinator(fundId);

            ViewBag.SerializedDataContactViewModel = PageSerializationHelper
                .SerializeObject(model);

            return PartialView("Partials/FundInformation/FIDonorsContactDetailsModal", model);
        }

        public virtual ActionResult DonorContactDetailsEdit(string donorName, int fundId)
        {
            var model = GetDonorContactDetailsEdit(donorName, fundId);

            SetViewBagGlobalRolesList(
                ActionEnum.ConvergenceReadPermission,
                ActionEnum.GCMReadPermission,
                ActionEnum.FCWritePermission,
                ActionEnum.SWCoordinatorWritePermission,
                ActionEnum.GCMChiefWrite);

            SetViewBagRoleFundCoordinator(fundId);

            if (!string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
                return Json(new { ErrorMessage = ViewBag.ErrorMessage });

            ViewBag.SerializedDataContactViewModel = PageSerializationHelper
                .SerializeObject(model);

            return PartialView("Partials/FundInformation/FIDonorsContactDetailsModalForm", model);
        }

        #endregion

        #region Fund Information Export

        public virtual FileResult FundInformationExportEFundedContractualsToExcel(int fundId)
        {
            var response = _fundInformationService.ExportEFundedContractualsToExcel(fundId);

            if (!response.IsValid)
                return null;

            return File(
                response.File,
                "application/doc",
                response.CodeFund + " - Externally Funded Contractuals.xls");
        }

        public virtual FileResult FundInformationExportOperationsToExcel(int fundId)
        {
            var response = _fundInformationService.ExportOperationsToExcel(fundId);

            if (!response.IsValid)
                return null;

            return File(response.File, "application/doc", response.CodeFund + " - Operations.xls");
        }

        public virtual FileResult FundInformationExportOApprovedConvergenceToExcel(int fundId)
        {
            var response = _fundInformationService.ExportOApprovedConvergenceToExcel(fundId);

            if (!response.IsValid)
                return null;

            return File(
                response.File,
                "application/doc", response.CodeFund +
                    " - Operations Approved in Convergence.xls");
        }

        #endregion

        #region Fund Information Actions

        public virtual JsonResult ValidateAction(string valueForValidate)
        {
            var reponse = _fundInformationService.ValidateIdentityService(valueForValidate);
            return Json(reponse);
        }

        public virtual JsonResult PagePendingPledgesHandler(
            int fundId,
            [ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            var request = requestModel.ConvertToDataTableRequestViewModel();
            var response = _fundInformationService.GetPendingPledgesListItems(fundId, request);

            var formatIntegerDolar = ConfigurationServiceFactory.Current.GetApplicationSettings()
                .FormatIntegerDolarWithoutSpace;

            var formatDate = ConfigurationServiceFactory.Current.GetApplicationSettings()
                .FormatDate;

            if (!response.IsValid)
            {
                List<object[]> emptyList = new List<object[]>();

                return Json(
                    new DataTablesResponse(requestModel.Draw, emptyList, 0, 0),
                    JsonRequestBehavior.AllowGet);
            }

            var result = from c in response.PendingPledgesViewModels
                         select new object[]
                         {
                             c.Donor,
                             DisplayAmount(c.PendingPledgeAmount, formatIntegerDolar),
                             c.PledgeCurrency,
                             DisplayAmount(c.USDValue, formatIntegerDolar),
                             FormatHelper.Format(c.DatePledged, formatDate),
                             FormatHelper.Format(c.ExpectedDate, formatDate)
                         };

            var jsonResponse = new DataTablesResponse(
                requestModel.Draw, result, response.TotalElements, response.TotalElements);

            return Json(jsonResponse, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult OperationsHandler(
            int fundId,
            [ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            var request = requestModel.ConvertToDataTableRequestViewModel();
            var response = _fundInformationService.GetOperationListItems(fundId, request);
            var formatIntegerDolar = ConfigurationServiceFactory.Current.GetApplicationSettings()
                .FormatIntegerDolarWithoutSpace;
            var formatDate = ConfigurationServiceFactory.Current.GetApplicationSettings()
                .FormatDate;

            if (!response.IsValid)
            {
                List<object[]> emptyList = new List<object[]>();
                return Json(
                    new DataTablesResponse(requestModel.Draw, emptyList, 0, 0),
                    JsonRequestBehavior.AllowGet);
            }

            var result = from c in response.OperationsViewModels
                         select new object[]
                         {
                             c.OperationNumber,
                             c.BeneficiaryCountry,
                             c.OperationName,
                             DisplayAmount(c.CurrentApprovedAmount, formatIntegerDolar),
                             c.ApprovalYear,
                             c.Waiver.HasValue ?
                                (c.Waiver.Value ?
                                    Localization.GetText(TCGlobalValues.YES) :
                                    Localization.GetText(TCGlobalValues.NO)) :
                                string.Empty,
                             c.WaiverComments,
                             c.State
                         };

            var jsonResponse = new DataTablesResponse(
                requestModel.Draw, result, response.TotalElements, response.TotalElements);

            return Json(jsonResponse, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult OperationsApprovedHandler(
            int fundId,
            [ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            var request = requestModel.ConvertToDataTableRequestViewModel();
            var response = _fundInformationService.GetOperationsApprovedListItems(fundId, request);

            if (!response.IsValid)
            {
                List<object[]> emptyList = new List<object[]>();

                return Json(
                    new DataTablesResponse(requestModel.Draw, emptyList, 0, 0),
                    JsonRequestBehavior.AllowGet);
            }

            var formatIntegerDolar = ConfigurationServiceFactory.Current.GetApplicationSettings()
                .FormatIntegerDolarWithoutSpace;

            var result = from c in response.OperationApprovedViewModels
                         select new object[]
                         {
                             c.OperationNumber,
                             c.ContractNumber,
                             c.OperationName,
                             c.OperationType,
                             DisplayAmount(c.ApprovedAmount, formatIntegerDolar),
                         };

            var jsonResponse = new DataTablesResponse(
                requestModel.Draw, result, response.TotalElements, response.TotalElements);

            return Json(jsonResponse, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult DonorsHandler(
            int fundId,
            [ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            var request = requestModel.ConvertToDataTableRequestViewModel();
            var response = _fundInformationService.GetDonorsListItems(fundId, request);

            var urlDonorsNameLink =
                "<a data-donor-link=\"true\" data-id=\"{1}\"" +
                    " data-fundId=\"{0}\" href=\"#\">{1}</span>";

            if (!response.IsValid)
            {
                List<object[]> emptyList = new List<object[]>();

                return Json(
                    new DataTablesResponse(requestModel.Draw, emptyList, 0, 0),
                    JsonRequestBehavior.AllowGet);
            }

            var globalRoles = GetGlobalRoleList(
                ActionEnum.GCMReadPermission, ActionEnum.GCMChiefWrite);

            bool hasPermissions = MvcHelpers.CheckRole(globalRoles, ActionEnum.GCMReadPermission);

            var result = from c in response.DonorsViewModels
                         select new object[]
                         {
                             hasPermissions ?
                                string.Format(urlDonorsNameLink, c.FundId, c.DonorsName) :
                                c.DonorsName,
                             c.Type,
                             c.Country
                         };

            var jsonResponse = new DataTablesResponse(
                requestModel.Draw, result, response.TotalElements, response.TotalElements);

            return Json(jsonResponse, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult SearchEconomicSubSectorsSectors(List<int> parentValue)
        {
            List<SelectListItem> subSectorsList = null;

            var response = _catalogService.GetMasterDataListByTypeCode(
                true,
                typeCodes: ConvergenceMasterDataTypeEnum
                    .EconomicSubSectorsAssigned.GetEnumDescription());

            if (response.IsValid)
            {
                var subSectors = response.MasterDataCollection;

                if (parentValue != null)
                    subSectors = subSectors.Where(x =>
                        parentValue.Any(y => y == x.ParentMasterId)).ToList();

                subSectorsList = ControllerHelper.ConvertToSelectListItems(subSectors);

                ViewBag.EconomicSubSectorsAssignedList = subSectorsList;
            }

            return Json(new { IsValid = subSectorsList != null, Data = subSectorsList });
        }

        public virtual JsonResult FindUserAD(string filter)
        {
            var adMode = ConfigurationServiceFactory.Current.GetApplicationSettings().IsADMode;
            var response = new FindUserADResponse();

            if (adMode)
            {
                response = _fundInformationService.FindUserAD(filter);
            }
            else
            {
                var list = new List<ListItemViewModel>();

                for (var i = 0; i < 5; i++)
                {
                    list.Add(new ListItemViewModel()
                    {
                        Text = string.Format("Text {0} {1}", filter, i),
                        Value = string.Format("Value {0} {1}", filter, i)
                    });
                }

                response.ListResponse = list;
                response.IsValid = true;
                response.ErrorMessage = string.Empty;
            }

            return new JsonResult
            {
                Data = response,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        #endregion

        #region Common Actions

        public virtual ActionResult TCAbstractHeader(string operationNumber)
        {
            SetViewBagRoles(operationNumber, ActionEnum.ConvergenceReadPermission);

            var tcAbstractResponse = _tcAbstractService.GetHeader(operationNumber);

            if (tcAbstractResponse.Header == null)
                tcAbstractResponse.Header = new HeaderViewModel();

            return PartialView("_TCHeader", tcAbstractResponse.Header);
        }

        public virtual ActionResult GetFundFullName(string fundCode)
        {
            var response = new SingleModelResponse<FundUserInfoDTO>();

            if (string.IsNullOrEmpty(fundCode))
            {
                response.IsValid = true;

                return Json(response);
            }

            try
            {
                response.Model = _fundsSharepointService.GetFundCoordinatorByCode(fundCode);

                response.IsValid = true;
            }
            catch (Exception ex)
            {
                Logger.GetLogger()
                    .WriteError("TCViewController-GetFundFullName", ex.Message, ex);

                response.ErrorMessage = Localization.GetText(TCGlobalValues.GENERAL_ERROR);
            }

            return Json(response);
        }

        public virtual ActionResult GetRegionalTeamLeader(string countryDepartment)
        {
            var response = new SingleModelResponse<FundUserInfoDTO>();

            if (string.IsNullOrEmpty(countryDepartment))
            {
                response.IsValid = true;

                return Json(response);
            }

            try
            {
                response.Model = _fundsSharepointService
                    .GetRegionalTeamLeaderByCode(countryDepartment);

                response.IsValid = true;
            }
            catch (Exception ex)
            {
                Logger.GetLogger()
                    .WriteError("TCViewController-GetRegionalTeamLeader", ex.Message, ex);

                response.ErrorMessage = Localization.GetText(TCGlobalValues.GENERAL_ERROR);
            }

            return Json(response);
        }

        public virtual FileResult DownloadWordDonorAndESCDecision(
            string operationNumber,
            int trustFundId)
        {
            var response = _tcCommonService.DownloadWordDraftEmail(
                operationNumber,
                trustFundId);

            if (!response.IsValid)
                return null;

            var fileFormat = GetApplicationAndNameReport(
                OutputFormatEnum.Word,
                response.FileName);

            return File(response.File, fileFormat[0], fileFormat[1]);
        }

        public virtual JsonResult AjaxHandler(
            [ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            var request = requestModel.ConvertToDataTableRequestViewModel();
            var response = _dataTableMockService.GetListItems(request);

            if (!response.IsValid)
                return null;

            var result = from c in response.ViewModels
                         select new[] { c.Name, c.Status, c.ObjetiveEn, c.RequestReference };

            var jsonResponse = new DataTablesResponse(
                requestModel.Draw, result, response.TotalElements, response.TotalElements);

            return Json(jsonResponse, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult PaginationExample()
        {
            var listDropDown = new List<SelectListItem>();
            var listDropDownHijos = new List<SelectListItem>();

            for (int i = 0; i < 10; i++)
            {
                listDropDown.Add(
                    new SelectListItem
                    {
                        Text = string.Format("Text {0}", i),
                        Value = i.ToString()
                    });

                for (int j = 0; j < 10; j++)
                {
                    listDropDownHijos.Add(
                        new SelectListItem
                        {
                            Text = string.Format("Text {0} {1}", i, j),
                            Value = j.ToString(),
                            Group = new SelectListGroup { Name = i.ToString() }
                        });
                }
            }

            ViewBag.DropdownExampleList = listDropDown;
            ViewBag.DropdownExampleHijoList = listDropDownHijos;

            return View();
        }

        public virtual JsonResult GetCascadaKendo()
        {
            var listDropDown = new List<SelectListItem>();

            for (int i = 0; i < 10; i++)
            {
                listDropDown.Add(
                    new SelectListItem
                    {
                        Text = string.Format("Text {0}", i),
                        Value = i.ToString()
                    });
            }

            return Json(listDropDown, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult GetCascadaHijoKendo(string parentId, FormCollection form)
        {
            var listDropDown = new List<SelectListItem>();

            for (int i = 0; i < 10; i++)
            {
                listDropDown.Add(
                    new SelectListItem
                    {
                        Text = string.Format("Text {0} {1}", parentId, i),
                        Value = i.ToString()
                    });
            }

            return Json(listDropDown, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Testing

        public virtual JsonResult CreateTC(string operationNumber)
        {
            var response = _tcMockService.CreateTCAbstract(operationNumber);

            if (response.IsValid)
                response.ErrorMessage = "Operation completed successfully.";

            return Json(response);
        }

        public virtual JsonResult ChangeRol(string operationNumber, ActionEnum rol)
        {
            _cacheStorageService.Remove("AuthorizationInfo");

            var auth = new AuthorizationOperationInfo
            {
                ActionList = new List<ActionEnum>(),
                RoleList = new List<RoleEnum>
                {
                    RoleEnum.PCRProjectTeamLeader
                },
                UserName = IDBContext.Current.UserLoginName
            };

            auth.ActionList.Add(ActionEnum.ConvergenceReadPermission);
            auth.ActionList.Add(ActionEnum.MyMeetingsReadPermission);
            auth.ActionList.Add(ActionEnum.SingleWindowOperationsReadPermission);

            if (!auth.ActionList.Any(x => x == rol))
                auth.ActionList.Add(rol);

            var authPermission = new AuthorizationInfo
            {
                Authorization = new Dictionary<string, AuthorizationOperationInfo>()
            };

            authPermission.Authorization.Add(operationNumber, auth);

            _cacheStorageService.Add("AuthorizationInfo", authPermission);

            return Json(new { IsValid = true, ErrorMessage = "Permissions was changed" });
        }

        public virtual JsonResult RemovePermission()
        {
            var authorizationInfo = _cacheStorageService
                .Get<AuthorizationInfo>("AuthorizationInfo");

            if (authorizationInfo == null)
                return Json(
                    new { IsValid = false, ErrorMessage = "No permission data to remove." });

            _cacheStorageService.Remove("AuthorizationInfo");
            authorizationInfo = _cacheStorageService.Get<AuthorizationInfo>("AuthorizationInfo");

            if (authorizationInfo == null)
                return Json(
                    new { IsValid = true, ErrorMessage = "Permission data removed correctly." });

            return Json(
                new { IsValid = false, ErrorMessage = "Permission data could not be removed." });
        }

        public virtual JsonResult ChangeWorspaceActionOrRole(
            string workspace,
            RoleEnum? role,
            ActionEnum? action)
        {
            _cacheStorageService.Remove("AuthorizationInfo");

            var auth = new AuthorizationOperationInfo()
            {
                ActionList = new List<ActionEnum>(),
                RoleList = new List<RoleEnum>(),
                UserName = IDBContext.Current.UserLoginName
            };

            if (role.HasValue)
                auth.RoleList.Add(role.Value);

            auth.ActionList.Add(ActionEnum.ConvergenceReadPermission);

            if (!auth.ActionList.Any(x => x == action))
            {
                if (action.HasValue)
                    auth.ActionList.Add(action.Value);
            }

            var authPermission = new AuthorizationInfo()
            {
                Authorization = new Dictionary<string, AuthorizationOperationInfo>()
            };

            authPermission.WorkspaceAuthorization.Add(workspace, auth);

            _cacheStorageService.Add("AuthorizationInfo", authPermission);

            return Json(new { IsValid = true, ErrorMessage = "Permissions was changed" });
        }

        public virtual JsonResult ChangeGlobalActionOrRole(
            string workspace,
            RoleEnum? role,
            ActionEnum? actionEnum)
        {
            _cacheStorageService.Remove("AuthorizationInfo");

            var auth = new AuthorizationOperationInfo()
            {
                ActionList = new List<ActionEnum>(),
                RoleList = new List<RoleEnum>(),
                UserName = IDBContext.Current.UserLoginName
            };

            if (role.HasValue)
                auth.RoleList.Add(role.Value);

            auth.ActionList.Add(ActionEnum.ConvergenceReadPermission);
            auth.ActionList.Add(ActionEnum.MyMeetingsReadPermission);
            auth.ActionList.Add(ActionEnum.SingleWindowOperationsReadPermission);

            if (!auth.ActionList.Any(x => x == actionEnum))
            {
                if (actionEnum.HasValue)
                    auth.ActionList.Add(actionEnum.Value);
            }

            var authPermission = new AuthorizationInfo();

            authPermission.GlobalAuthorization = auth;

            _cacheStorageService.Add("AuthorizationInfo", authPermission);

            return Json(new { IsValid = true, ErrorMessage = "Permissions was changed" });
        }

        public virtual JsonResult ChangeGlobalActionOrRoles(
            string workspace,
            RoleEnum? role,
            ActionEnum[] actionEnum)
        {
            _cacheStorageService.Remove("AuthorizationInfo");

            var auth = new AuthorizationOperationInfo()
            {
                ActionList = new List<ActionEnum>(),
                RoleList = new List<RoleEnum>(),
                UserName = IDBContext.Current.UserLoginName
            };

            if (role.HasValue)
                auth.RoleList.Add(role.Value);

            auth.ActionList.Add(ActionEnum.ConvergenceReadPermission);
            auth.ActionList.Add(ActionEnum.MyMeetingsReadPermission);
            auth.ActionList.Add(ActionEnum.SingleWindowOperationsReadPermission);

            for (var i = 0; i < actionEnum.Count(); i++)
            {
                if (!auth.ActionList.Any(x => x == actionEnum[i]))
                    auth.ActionList.Add(actionEnum[i]);
            }

            var authPermission = new AuthorizationInfo();

            authPermission.GlobalAuthorization = auth;

            _cacheStorageService.Add("AuthorizationInfo", authPermission);

            return Json(new { IsValid = true, ErrorMessage = "Permissions was changed" });
        }

        public virtual ActionResult RedirectToCustomAction(
            string operationNumber,
            string actionName,
            int trustFundId)
        {
            var swResponse = _tcMockService.GetSingleWindowMeetingId(operationNumber);

            return RedirectToAction(
                actionName,
                new { trustFundId, singleWindowsMeetingId = swResponse.SingleWindowMeetingId });
        }

        #endregion

        #region Private Methods

        private FundInformationViewModel GetFundInformationViewModel(int fundId)
        {
            Logger.GetLogger().WriteDebug("GetFunInformation", "Start. Get Id ..." + fundId);
            var response = _fundInformationService.GetFundInformation(fundId);

            var viewModel = response.FundInformationViewModel ??
                ViewModelInitializerFactory.InitializeFundInformationViewModel();

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewBagFundInformation(viewModel.BasicData, fundId);

            Logger.GetLogger().WriteDebug("GetFunInformation", "Finished. Returning ...");
            return viewModel;
        }

        SingleModelResponse<TCAbstractViewModel> GetTCAbstractViewModel(string operationNumber)
        {
            var response = _tcAbstractService.GetTCAbstract(operationNumber);

            if (!response.IsValid)
                return response;

            SetViewBagTCAbstract(response.Model);

            return response;
        }

        void SetViewBagTCAbstract(TCAbstractViewModel viewModel)
        {
            var basicDataViewModel = viewModel.BasicData;

            this.SetViewBagListFromCatalog(
                _catalogService,
                new Dictionary<ConvergenceMasterDataTypeEnum, string>
                {
                    { ConvergenceMasterDataTypeEnum.Country, "CountryList" },
                    { ConvergenceMasterDataTypeEnum.Consultants, "TypesOfConsultantsList" },
                    { ConvergenceMasterDataTypeEnum.SectorPriority, "GCISectorPriorityList" },
                    { ConvergenceMasterDataTypeEnum.OperationTheme, "OperationThemeList" },
                    { ConvergenceMasterDataTypeEnum.Status, "StatusList" },
                    { ConvergenceMasterDataTypeEnum.RiskType, "TypeList" },
                });

            var countryDepartments = _catalogService.GetCountryDepartment();

            ViewBag.CountryDepartmentList = ControllerHelper
               .ConvertToSelectListItems(countryDepartments, true)
               .OrderBy(x => x.Text)
               .ToList();

            var operationTheme = (List<SelectListItem>)ViewBag.OperationThemeList;
            var objUndefined = operationTheme.First(x => x.Text == Localization.GetText(
                "TC.TCAbstract.BasicData.OperationThemes.Undefined"));
            operationTheme.Remove(objUndefined);
            operationTheme.Insert(0, objUndefined);

            ViewBag.OperationThemeList = operationTheme;
            ViewBag.OrganizationalUnits = _catalogService.GetOrganizationalUnits();

            ViewBag.Country = GetSelectedItemText(ViewBag.CountryList, basicDataViewModel.Country);

            ViewBag.OperationTheme1 = GetSelectedItemText(
                ViewBag.OperationThemeList, basicDataViewModel.OperationTheme1Id);
            ViewBag.OperationTheme2 = GetSelectedItemText(
                ViewBag.OperationThemeList, basicDataViewModel.OperationTheme2Id);
            ViewBag.OperationTheme3 = GetSelectedItemText(
                ViewBag.OperationThemeList, basicDataViewModel.OperationTheme3Id);
        }

        ValidationTCAbstractViewModel GetValidationTCAbstractViewModel(string operationNumber)
        {
            var response = _validationTcAbstractService.GetTCAbstractValidation(operationNumber);

            var viewModel = response.ValidationTCAbstractViewModel ??
                ViewModelInitializerFactory.InitializeValidationTcAbstractViewModel();

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewBagValidationTCAbstract(viewModel);

            return viewModel;
        }

        void SetViewBagValidationTCAbstract(ValidationTCAbstractViewModel viewModel)
        {
            SetViewBagEnumMapping<ValidationTypeEnum>();

            SetViewBagTCAbstract(viewModel.TCAbstract);
        }

        TCAbstractReviewTLResponse GetReviewTCAbstractViewModel(string operationNumber)
        {
            var response = _reviewTcAbstractService.GetTCAbstractReviewTL(operationNumber);

            if (!response.IsValid)
                return response;

            var viewModel = response.ReviewTCAbstractViewModel ??
                ViewModelInitializerFactory.InitializeReviewTcAbstractViewModel();

            try
            {
                var users = new GetUserFullNamesRequest();

                foreach (var comment in viewModel.Comments)
                {
                    users.UserNames.Add(comment.User);
                }

                var userFullNames = UserIdentityManager
                    .SearchFullUserNamesByUser(users);
                for (int i = 0; i < userFullNames.FullNames.Count; i++)
                {
                    viewModel.Comments[i].UserFullName = string.IsNullOrEmpty(userFullNames.FullNames[i]) ?
                        viewModel.Comments[i].User : userFullNames.FullNames[i];
                    SetViewBagReviewTCAbstract(viewModel);
                }

                response.IsValid = true;

                return response;
            }
            catch (Exception ex)
            {
                Logger.GetLogger()
                    .WriteError("TCViewController-GetReviewTCAbstractViewModel", ex.Message, ex);
                response.ErrorMessage = Localization
                    .GetText(TCGlobalValues.ERROR_TC_ABSTRACT_SERVICE);
                response.IsValid = false;

                return response;
            }
        }

        void SetViewBagReviewTCAbstract(ReviewTCAbstractViewModel viewModel)
        {
            SetViewBagTCAbstract(viewModel.TCAbstract);
        }

        SingleModelResponse<TCNotifyFCViewModel> GetReviewersToNotify(string operationNumber)
        {
            var response = new SingleModelResponse<TCNotifyFCViewModel>();

            var responseService = _notifyFundingService.GetTCNotifyFC(operationNumber);

            if (!responseService.IsValid)
            {
                response.IsValid = responseService.IsValid;
                response.ErrorMessage = responseService.ErrorMessage;

                return response;
            }

            try
            {
                response.Model = responseService.TCNotifyFCViewModel;

                ViewBag.RegionalTLSelected = SelectListItemHelpers.BuildSelectItemList(
                    response.Model.TCNotifyFCRegionalTeamLeaderViewModel,
                    rtl => _fundsSharepointService
                        .GetRegionalTeamLeaderByCode(rtl.CountryLiaisonCode).FullName,
                    rtl => rtl.CountryLiaisonId.ToString());

                ViewBag.CountryDepartmentList = GetCountryLiaisons();

                var fundsSelectionID = response.Model.FundCoordinatorSelection
                    .Select(fc => fc.TrustFundId);

                var responseSet = SetSelectableFCReviewers(fundsSelectionID);

                if (!responseSet.IsValid)
                {
                    response.IsValid = false;
                    response.ErrorMessage = responseSet.ErrorMessage;
                }

                response.IsValid = true;

                return response;
            }
            catch (Exception ex)
            {
                Logger.GetLogger()
                    .WriteError("TCViewController-GetReviewersToNotify", ex.Message, ex);

                return new SingleModelResponse<TCNotifyFCViewModel>
                {
                    IsValid = false,
                    ErrorMessage = ex.Message,
                };
            }
        }

        ResponseBase SetSelectableFCReviewers(IEnumerable<int> trustFundIDSelection)
        {
            var trustFundResponse = _notifyFundingService.GetAllTrustFunding();

            if (!trustFundResponse.IsValid)
            {
                return new ResponseBase
                {
                    IsValid = false,
                    ErrorMessage = trustFundResponse.ErrorMessage
                };
            }

            try
            {
                ViewBag.ApplicableFunds = trustFundResponse.TrustFundingViewModels
                    .OrderBy(x => x.FundCodeFundName);

                ViewBag.ApplicableFundsSelected = SelectListItemHelpers.BuildSelectItemList(
                    trustFundResponse.TrustFundingViewModels
                        .Where(tf => trustFundIDSelection.Contains(tf.TrustFundingId)),
                    f => _fundsSharepointService.GetFundCoordinatorByCode(f.Code).FullName,
                    f => f.TrustFundingId.ToString());

                return new ResponseBase { IsValid = true };
            }
            catch (Exception ex)
            {
                Logger.GetLogger()
                    .WriteError("TCViewController-SetSelectableReviewers", ex.Message, ex);

                return new ResponseBase { IsValid = false, ErrorMessage = ex.Message };
            }
        }

        void SetViewPriorityTCAbstract(PriorityTCAbstractViewModel viewModel)
        {
            SetViewBagTCAbstract(viewModel.TCAbstract);

            this.SetViewBagListFromCatalog(
                _catalogService,
                ConvergenceMasterDataTypeEnum.PriorityLevel,
                "PriorityList");

            ViewBag.PriorityDesc = GetSelectedItemText(
                ViewBag.PriorityList, Convert.ToInt32(viewModel.Priority));

            SetViewBagEnumMapping<CountryPriorityTCAbstractEnum>();
        }

        FundCoordinatorTCAbstractViewModel GetFundCoordinatorTCAbstractViewModel(
            string operationNumber,
            int trustFundId)
        {
            var response = _reviewFundingCoordinatorService
                .GetReviewFundingCoordinator(operationNumber, trustFundId);

            var viewModel = response.FundCoordinatorTCAbstractViewModel ??
                ViewModelInitializerFactory.InitializeFundCoordinatorTCAbstractViewModel();

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewBagFundCoordinatorTCAbstract(viewModel, operationNumber, trustFundId);

            return viewModel;
        }

        void SetViewBagFundCoordinatorTCAbstract(
            FundCoordinatorTCAbstractViewModel viewModel,
            string operationNumber,
            int trustFundId)
        {
            SetViewBagTCAbstract(viewModel.TCAbstract);

            ViewBag.HiddenTrustFundId = trustFundId;

            this.SetViewBagListFromCatalog(
                _catalogService,
                new Dictionary<ConvergenceMasterDataTypeEnum, string>
                {
                    { ConvergenceMasterDataTypeEnum.FundCoordinatorTypeElegibility,
                        "TypeEligibilityList" },
                    { ConvergenceMasterDataTypeEnum.FundCoordinatorTCauseOfNoFunding,
                        "CauseOfNoFundingList" }
                });

            SetViewBagTrustFundTesting(operationNumber);

            SetViewBagEnumMapping<FundCoordinatorCauseNoFundingEnum>();
        }

        private FileResult SingleWindowMeetingExportAllFormat(
            OutputFormatEnum outputFormat,
            string operationNumber,
            string fromDate,
            string toDate)
        {
            DateTime? fromDateParsed = null;
            DateTime? toDateParsed = null;

            if (!string.IsNullOrWhiteSpace(fromDate))
                fromDateParsed = DateTime.ParseExact(
                    fromDate,
                    ClientFieldDataMappers.DATETIME_PARSE_FORMAT,
                    CultureInfo.InvariantCulture);

            if (!string.IsNullOrWhiteSpace(toDate))
                toDateParsed = DateTime.ParseExact(
                    toDate,
                    ClientFieldDataMappers.DATETIME_PARSE_FORMAT,
                    CultureInfo.InvariantCulture);

            var response = _singleWindowMeetingService.ExportSingleWindowMeeting(
                outputFormat,
                fromDateParsed,
                toDateParsed);

            if (!response.IsValid)
                return null;

            var fileFormat = GetApplicationAndNameReport(
                outputFormat, TCGlobalValues.SWM_NAME_REPORT);

            return File(response.File, fileFormat.ElementAt(0), fileFormat.ElementAt(1));
        }

        SingleWindowMeetingViewModel GetSingleWindoMeetingViewModel(
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var response = _singleWindowMeetingService.GetSingleWindowMeeting(
                IDBContext.Current.CurrentLanguage, fromDate, toDate);

            var viewModel = response.SingleWindowMeetingViewModel ??
                ViewModelInitializerFactory.InitializeSingleWindowMeetingViewModel();

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewBagSingleWindowMeeting();

            return viewModel;
        }

        void SetViewBagSingleWindowMeeting()
        {
            this.SetViewBagListFromCatalog(
                _catalogService,
                new Dictionary<ConvergenceMasterDataTypeEnum, string>
                {
                    { ConvergenceMasterDataTypeEnum.SingleWindowStandarizedComment,
                        "StandardizedCommentsList" },
                    { ConvergenceMasterDataTypeEnum.SingleWindowESCDecision,
                        "ESCDecisionList" }
                });

            SetViewBagEnumMapping<SingleWindowDonorEscDecisionEnum>();
        }

        GetMeetingGeneralCommentsResponse GetMeetingGeneralComments(int tcAbstractId)
        {
            var response = _singleWindowMeetingService
                .GetSingleWindowMeetingGeneralComments(tcAbstractId);

            if (response.MeetingListGeneralComments == null)
                response.MeetingListGeneralComments = ViewModelInitializerFactory
                    .InitializeMeetingListGeneralCommentsViewModel(tcAbstractId);

            return response;
        }

        GetMeetingHistoricalFundResponse GetMeetingHistoricalFunds(int tcAbstractId)
        {
            var response = _singleWindowMeetingService
                .GetSingleWindowMeetingHistoricalFunds(tcAbstractId);
            return response;
        }

        SingleWindowOperationsViewModel GetSingleWindowOperationsViewModel(
            SWTypeDecisionFilterEnum typeDecision = SWTypeDecisionFilterEnum.AllOperations,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            if (typeDecision != SWTypeDecisionFilterEnum.AllOperations)
                typeDecision = _enumMappingService
                    .GetMappedEnum<SWTypeDecisionFilterEnum>((int)typeDecision);

            var response = _singleWindowOperationService.GetSingleWindowOperationFilter(
                IDBContext.Current.CurrentLanguage, typeDecision, fromDate, toDate);

            var viewModel = response.SingleWindowOperationsViewModel ??
                ViewModelInitializerFactory.InitializeSingleWindowOperationsViewModel();

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewBagSingleWindowOperations();

            return viewModel;
        }

        GetSingleWindowOperationGeneralCommentsResponse GetOperationGeneralComments(
            int tcAbstractId)
        {
            var response = _singleWindowOperationService
                .GetSingleWindowMeetingOperationGeneralComments(tcAbstractId);

            if (response.MeetingGeneralCommentsViewModel == null)
                response.MeetingGeneralCommentsViewModel = ViewModelInitializerFactory
                    .InitializeMeetingListGeneralCommentsViewModel(tcAbstractId);

            return response;
        }

        void SetViewBagSingleWindowOperations()
        {
            var catalogResponse = _catalogService.GetMasterDataListByTypeCode(
                typeCodes: ConvergenceMasterDataTypeEnum.TCAbstractVersion.GetEnumDescription());
            var listOfCodes = _enumMappingService
                .GetMappingCodeCollection<SWTypeDecisionFilterEnum>();

            var filterDataList = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Selected = true,
                    Text = SWTypeDecisionFilterEnum.AllOperations.GetEnumDescription(),
                    Value = Convert.ToInt32(SWTypeDecisionFilterEnum.AllOperations)
                        .ToString(CultureInfo.InvariantCulture)
                }
            };

            var itemsFromCode = from f in listOfCodes.InternalDictionary
                                select new SelectListItem
                                {
                                    Selected = false,
                                    Text = f.Key.GetEnumDescription(),
                                    Value = f.Value.ToString()
                                };

            filterDataList.AddRange(itemsFromCode);

            ViewBag.TypeOfDecisionFilterList = filterDataList;

            SetViewBagSingleWindowMeeting();
            SetViewBagEnumMapping<SWTypeOperationEnum>();
        }

        private FileResult SingleWindowOperationsExportAllFormat(
            OutputFormatEnum outputFormat,
            SWTypeDecisionFilterEnum typeDecision = SWTypeDecisionFilterEnum.AllOperations,
            string fromDate = null,
            string toDate = null)
        {
            DateTime? fromDateParsed = null;
            DateTime? toDateParsed = null;

            if (fromDate != null && !string.IsNullOrWhiteSpace(fromDate))
                fromDateParsed = DateTime.ParseExact(
                    fromDate,
                    ClientFieldDataMappers.DATETIME_PARSE_FORMAT,
                    CultureInfo.InvariantCulture);

            if (toDate != null && !string.IsNullOrWhiteSpace(toDate))
                toDateParsed = DateTime.ParseExact(
                    toDate,
                    ClientFieldDataMappers.DATETIME_PARSE_FORMAT,
                    CultureInfo.InvariantCulture);

            if (typeDecision != SWTypeDecisionFilterEnum.AllOperations)
                typeDecision = _enumMappingService.GetMappedEnum<SWTypeDecisionFilterEnum>(
                    (int)typeDecision);

            var response = _singleWindowOperationService.ExportSingleWindowOperations(
                outputFormat,
                typeDecision,
                fromDateParsed,
                toDateParsed);

            if (!response.IsValid)
                return null;

            var fileFormat = GetApplicationAndNameReport(
                outputFormat, TCGlobalValues.SWO_NAME_REPORT);

            return File(response.File, fileFormat.ElementAt(0), fileFormat.ElementAt(1));
        }

        DecisionSWOperationViewModel GetDecisionSingleWindowOperation(string operationNumber)
        {
            var response = _singleWindowOperationDecisionService
                .GetDecisionSingleWindowOperation(operationNumber);

            var viewModel = response.DecisionSWOperationViewModel ??
                ViewModelInitializerFactory.InitializeDecisionSWOperationViewModel();

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewBagDecisionSWOperation(viewModel);

            return viewModel;
        }

        void SetViewBagDecisionSWOperation(DecisionSWOperationViewModel viewModel)
        {
            SetViewBagEnumMapping<OperationTypeEnum>();
            SetViewBagEnumMapping<DecisionOperationTypeEnum>();
        }

        SingleWindowDecisionViewModel GetSingleWindowDecisionViewModel(string operationNumber)
        {
            var response = _singleWindowDecisionService.GetSingleWindowDecision(operationNumber);

            var viewModel = response.SingleWindowDecisionViewModel ??
                ViewModelInitializerFactory.InitializeSingleWindowDecisionViewModel();

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewBagSingleWindowDecision();

            return viewModel;
        }

        void SetViewBagSingleWindowDecision()
        {
            this.SetViewBagListFromCatalog(
                _catalogService,
                new Dictionary<ConvergenceMasterDataTypeEnum, string>
                {
                    { ConvergenceMasterDataTypeEnum.SingleWindowReasonHoldStatus,
                        "ReasonHoldOperation" },
                    { ConvergenceMasterDataTypeEnum.SingleWindowStandarizedComment,
                        "StandardizedCommentsList" },
                    { ConvergenceMasterDataTypeEnum.SingleWindowESCDecision, "ESCDecisionList" },
                    { ConvergenceMasterDataTypeEnum.SWDecisionCoordinatorDecision,
                        "NotifyFundCoordinatorDecisionList" },
                    { ConvergenceMasterDataTypeEnum.FundCoordinatorTCauseOfNoFunding,
                        "FundCoordinatorTCauseOfNoFundingList" }
                });

            var trustFundResponse = _singleWindowDecisionService.GetAllTrustFunding();
            var fundCoordinatorList = new List<TrustFundingViewModel>();

            if (trustFundResponse.IsValid)
                fundCoordinatorList = trustFundResponse
                    .TrustFundingViewModels.OrderBy(x => x.FundCodeFundName).ToList();

            ViewBag.NotifyFundCoordinatorFundList = fundCoordinatorList;

            SetViewBagEnumMapping<DecisionSWFCDecisionEnum>();
            SetViewBagEnumMapping<SingleWindowDecisionTypeEnum>();
            SetViewBagEnumMapping<WithdrawalTypeEnum>();
            SetViewBagEnumMapping<ReasonHoldOperationEnum>();
        }

        EligibilityDecisionViewModel GetEligibilityDecisionViewModel(
            string operationNumber,
            int trustFundId)
        {
            var response = _eligibilityDecisionService
                .GetEligibilityDecision(operationNumber, trustFundId);

            var viewModel = response.EligibilityDecisionViewModel ??
                ViewModelInitializerFactory.InitializeEligibilityDecisionViewModel();

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewBagEligibilityDecision(viewModel);
            SetViewBagTrustFundsFromLastSingleWindowMeeting(operationNumber);

            return viewModel;
        }

        void SetViewBagEligibilityDecision(EligibilityDecisionViewModel viewModel)
        {
            this.SetViewBagListFromCatalog(
                _catalogService,
                new Dictionary<ConvergenceMasterDataTypeEnum, string>
                {
                    { ConvergenceMasterDataTypeEnum.EligibilityDecision,
                        "EligibilityDecisionList" },
                    { ConvergenceMasterDataTypeEnum.SingleWindowReasonHoldStatus,
                        "ReasonForOnHoldStatusList" }
                });

            SetViewBagEnumMapping<EligibilityDecisionTypeEnum>();
            SetViewBagEnumMapping<ReasonHoldOperationEnum>();

            if (viewModel != null && viewModel.AwardFundEligibility != null)
                SetViewBagAwardFundEligibility(viewModel.AwardFundEligibility);
        }

        ReviewTCAbstractPostValidationResponse GetReviewPostValidationViewModel(
            string operationNumber,
            int trustFundId)
        {
            var response = new ReviewTCAbstractPostValidationResponse();

            var taskDataModel = K2Helper.RetrieveK2Data(operationNumber, trustFundId);

            var responseTag = _tcCommonService.GetTypeTaskTag(taskDataModel.TaskTag);

            if (!responseTag.IsValid)
            {
                response.ErrorMessage = responseTag.ErrorMessage;
                return response;
            }

            response = _reviewTCAbstractPostValidationService.GetReviewTCAbstractPostValidation(
                operationNumber, trustFundId, responseTag.ActionMethod);

            if (!response.IsValid)
            {
                response.ErrorMessage = responseTag.ErrorMessage;
                return response;
            }

            var viewModel = response.ReviewTCAbstractPostValidationViewModel ??
                ViewModelInitializerFactory.InitializeReviewPostValidationViewModel();

            try
            {
                SetViewBageviewPostValidation(viewModel);
                SetViewBagTrustFundsFromLastSingleWindowMeeting(operationNumber);

                var users = new GetUserFullNamesRequest();

                users.UserNames.Add(viewModel.UserName);

                var fullUserName = UserIdentityManager
                    .SearchFullUserNamesByUser(users)
                    .FullNames
                    .FirstOrDefault();

                viewModel.UserFullName = string.IsNullOrEmpty(fullUserName) ?
                    viewModel.UserName : fullUserName;

                response.IsValid = true;
                return response;
            }
            catch (Exception ex)
            {
                Logger.GetLogger().WriteError(
                    "TCViewController-GetReviewPostValidationViewModel", ex.Message, ex);
                response.ErrorMessage = Localization
                    .GetText(TCGlobalValues.ERROR_TC_ABSTRACT_SERVICE);
                response.IsValid = false;
                return response;
            }
        }

        void SetViewBageviewPostValidation(ReviewTCAbstractPostValidationViewModel viewModel)
        {
            SetViewBagTCAbstract(viewModel.TCAbstract);
        }

        DonorDecisionViewModel GetDonorDecisionViewModel(string operationNumber, int trustFundId)
        {
            var response = _donorDecisionService.GetDonorDecision(operationNumber, trustFundId);

            var viewModel = response.DonorDecisionViewModel ??
                ViewModelInitializerFactory.InitializeDonorWindowDecisionViewModel();

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewBagDonorDecision(viewModel);
            SetViewBagTrustFundsFromLastSingleWindowMeeting(operationNumber);

            return viewModel;
        }

        void SetViewBagDonorDecision(DonorDecisionViewModel viewModel)
        {
            this.SetViewBagListFromCatalog(
                _catalogService,
                new Dictionary<ConvergenceMasterDataTypeEnum, string>
                {
                    { ConvergenceMasterDataTypeEnum.DonorDecision, "DonorDecisionList" },
                    { ConvergenceMasterDataTypeEnum.CauseOfNoFunding, "CauseOfNoFundingList" },
                    { ConvergenceMasterDataTypeEnum.SingleWindowReasonHoldStatus,
                        "ReasonForOnHoldStatusList" }
                });

            SetViewBagEnumMapping<DonorDecisionTypeEnum>();
            SetViewBagEnumMapping<ReasonHoldOperationEnum>();
            SetViewBagEnumMapping<NotFundingCauseEnum>();

            SetViewBagAwardFundEligibility(viewModel.AwardFundEligibility);
        }

        ESCDecisionViewModel GetEscDecisionViewModel(string operationNumber, int trustFundId)
        {
            var response = _escDecisionService.GetESCDecision(operationNumber, trustFundId);

            var viewModel = response.ESCDecisionViewModel
                ?? ViewModelInitializerFactory.InitializeEscWindowDecisionViewModel();

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewBagEscDecision(viewModel);
            SetViewBagTrustFundsFromLastSingleWindowMeeting(operationNumber);

            return viewModel;
        }

        void SetViewBagEscDecision(ESCDecisionViewModel viewModel)
        {
            this.SetViewBagListFromCatalog(
                _catalogService,
                new Dictionary<ConvergenceMasterDataTypeEnum, string>
                {
                    { ConvergenceMasterDataTypeEnum.SingleWindowESCDecision, "EscDecisionList" },
                    { ConvergenceMasterDataTypeEnum.CauseOfNoFunding, "CauseOfNoFundingList" },
                    { ConvergenceMasterDataTypeEnum.EscDecisionOperationType,
                        "OperationTypeList" },
                    { ConvergenceMasterDataTypeEnum.SingleWindowReasonHoldStatus,
                        "ReasonForOnHoldStatusList" }
                });

            SetViewBagEnumMapping<ESCDecisionTypeEnum>();
            SetViewBagEnumMapping<EscDecisionOperationTypeEnum>();
            SetViewBagEnumMapping<NotFundingCauseEnum>();
            SetViewBagEnumMapping<ReasonHoldOperationEnum>();

            SetViewBagAwardFundEligibility(viewModel.AwardFundEligibility);
        }

        void SetViewBagAwardFundEligibility(AwardFundEligibilityViewModel viewModel)
        {
            if (viewModel != null)
            {
                var showAmount = viewModel.ShowAmountMultidonorFund;
                ViewBag.ShowAmountMultidonorFund = showAmount;
            }
        }

        RequestIncreaseViewModel GetRequestIncreaseViewModel(string operationNumber)
        {
            var response = _requestIncreaseService.GetRequestIncrease(operationNumber);

            var viewModel = response.RequestIncreaseViewModel ??
                ViewModelInitializerFactory.InitializeRequestIncreaseViewModel();

            SetViewBagErrorMessageInvalidResponse(response);

            return viewModel;
        }

        TCReviewNotifyFCViewModel GetTCReviewNotifyFCViewModel(
           string operationNumber,
           bool loadFunds = false)
        {
            var response = _notifyFundingService.GetTCReviewTCNotifyFC(operationNumber);

            var viewModel = response.TCReviewNotifyFCViewModel ??
                ViewModelInitializerFactory.InitializeTCReviewNotifyFCViewModel();

            SetViewBagErrorMessageInvalidResponse(response);

            if (loadFunds)
            {
                var fundsSelectID = response.TCReviewNotifyFCViewModel.TCReviewFCSelectedViewModel
                    .Select(fc => fc.TrustFundId);

                SetSelectableFCReviewers(fundsSelectID);
            }

            return viewModel;
        }

        void SetViewBagFilterFunds()
        {
            ViewBag.FundsList = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = ((int)FundTypeEnum.AllFunds).ToString(),
                    Text = EnumHelper.GetEnumDescription(FundTypeEnum.AllFunds)
                },
                new SelectListItem
                {
                    Value = ((int)FundTypeEnum.MyFunds).ToString(),
                    Text = EnumHelper.GetEnumDescription(FundTypeEnum.MyFunds)
                }
            };
        }

        void SetViewBagTrustFundTesting(string operationNumber)
        {
            var operationTrustFunding = GetReviewersToNotify(operationNumber)
                .Model
                .FundCoordinatorSelection;

            var trustFundingList = from t in _notifyFundingService.GetAllTrustFunding()
                                       .TrustFundingViewModels
                                   where operationTrustFunding.Any(x =>
                                       x.TrustFundId == t.TrustFundingId)
                                   select new SelectListItem
                                   {
                                       Selected = false,
                                       Text = t.FundName,
                                       Value = t.TrustFundingId
                                        .ToString(CultureInfo.InvariantCulture)
                                   };

            ViewBag.TrustFundingList = trustFundingList.ToList();
        }

        void SetViewBagTrustFundsFromLastSingleWindowMeeting(string operationNumber)
        {
            var response = _tcMockService.GetTrustFundingList(operationNumber);

            if (response.IsValid)
            {
                ViewBag.TrustFundList = response.PreassignedFunds
                    .Select(x =>
                        new SelectListItem
                        {
                            Value = x.Key.ToString(CultureInfo.InvariantCulture),
                            Text = x.Value
                        })
                    .ToList();
            }
        }

        private FIBasicDataViewModel GetFundInformationBasicDataViewModel(int fundId)
        {
            var response = _fundInformationService.GetFundInformationBasicInformation(fundId);

            var viewModel = response.FIBasicDataViewModel ??
                ViewModelInitializerFactory.InitializeFundInformationViewModel().BasicData;

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewBagFundInformation(viewModel, fundId);

            return viewModel;
        }

        private List<FIRowExtFundWorkforceViewModel> GetFundInformationFundedOperationsViewModel(
            int fundId)
        {
            var response = _fundInformationService.GetFundInformationFundedOperations(fundId);

            var viewModel = response.ExtFundWorkforceList ??
                ViewModelInitializerFactory.InitializeFundInformationViewModel().ExtFundWorkforce;

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewBagFundInformationFundedOperations(fundId);

            return viewModel;
        }

        private List<DocumentViewModel> GetFundInformationDocumentsViewModel(int fundId)
        {
            var response = _fundInformationService.GetFundInformationDocuments(fundId);

            var viewModel = response.Documents ??
                ViewModelInitializerFactory.InitializeFundInformationViewModel().Documents;

            SetViewBagErrorMessageInvalidResponse(response);

            return viewModel;
        }

        private FIDonorContactDetailViewModel GetDonorContactDetailsEdit(
            string donorName,
            int fundId)
        {
            var response = _fundInformationService.GetFundDonorDetailsResponse(donorName, fundId);

            var viewModel = response.FIDonorContactDetailViewModel ??
                ViewModelInitializerFactory.InitializeDonorContactDetailsPartialViewModel();

            SetViewBagErrorMessageInvalidResponse(response);

            return viewModel;
        }

        private void SetViewBagFundInformation(FIBasicDataViewModel viewModel, int fundId)
        {
            ViewBag.HiddenFundId = fundId;

            this.SetViewBagListFromCatalog(
                _catalogService,
                new Dictionary<ConvergenceMasterDataTypeEnum, string>
                {
                    { ConvergenceMasterDataTypeEnum.Country, "CountriesAssignedList" },
                    { ConvergenceMasterDataTypeEnum.DonorType, "DonorTypeList" },
                    { ConvergenceMasterDataTypeEnum.State, "StateList" },
                    { ConvergenceMasterDataTypeEnum.FundType, "FundType" }
                });

            ViewBag.TyingCondition = new Dictionary<TyingConditionEnum, string>()
            {
                { TyingConditionEnum.Tied, "Tied" },
                { TyingConditionEnum.Untied, "Untied" }
            };

            SetViewBagDisplayabled();

            ViewBag.EconomicSectorsAssignedList = GetMultiSelectListItemByType(
                ConvergenceMasterDataTypeEnum.EconomicSectorAssigned);

            var childList = _catalogService.GetMasterDataListByTypeCode(
                hideExpired: true,
                typeCodes: ConvergenceMasterDataTypeEnum.EconomicSubSectorsAssigned
                    .GetEnumDescription());

            childList.MasterDataCollection = childList.MasterDataCollection.Where(x =>
                x.ParentMasterId != null).ToList();

            childList.MasterDataCollection = childList.MasterDataCollection.Where(x =>
                viewModel.EconomicSectorsAssigned.Any(y => y == x.ParentMasterId)).ToList();

            List<MultiSelectListItem> subsectors = new List<MultiSelectListItem>();

            foreach (var item in childList.MasterDataCollection)
            {
                var newItem = new MultiSelectListItem
                {
                    Text = item.GetLocalizedName(),
                    Value = item.MasterId.ToString()
                };

                subsectors.Add(newItem);
            }

            ViewBag.EconomicSubSectorsAssignedList = subsectors;

            SetViewBagBeneficiaryCountries();

            ViewBag.GCI9SectorPrioritiesList = GetMultiSelectListItemByType(
                ConvergenceMasterDataTypeEnum.GCI9SectorPriorities);

            ViewBag.IChallengesCrossCuttingThemesList = GetMultiSelectListItemByType(
                ConvergenceMasterDataTypeEnum.IChallengesCrossCuttingThemes);

            ViewBag.TypeInstrumentsFinancedList = GetMultiSelectListItemByType(
                ConvergenceMasterDataTypeEnum.TypeInstrumentsFinancedByFund);

            ViewBag.UsersAndOwnersOfTheFund = GetMultiSelectListItemByType(
                ConvergenceMasterDataTypeEnum.FundUser);

            ViewBag.ThemeList = GetMultiSelectListItemByType(ConvergenceMasterDataTypeEnum.Theme);

            ViewBag.TemplateA = _enumMappingService
                .GetMappingCode<EligibilityDecisionEmailTemplateEnum>(
                    EligibilityDecisionEmailTemplateEnum.TemplateA);
            ViewBag.TemplateB = _enumMappingService
                .GetMappingCode<EligibilityDecisionEmailTemplateEnum>(
                    EligibilityDecisionEmailTemplateEnum.TemplateB);
            ViewBag.TemplateC = _enumMappingService
                .GetMappingCode<EligibilityDecisionEmailTemplateEnum>(
                    EligibilityDecisionEmailTemplateEnum.TemplateC);

            ViewBag.EscTeamList = InitializeESCommitteeEmail();
        }

        private void SetViewBagFundInformationFundedOperations(int fundId)
        {
            ViewBag.HiddenFundId = fundId;

            this.SetViewBagListFromCatalog(
                _catalogService,
                new Dictionary<ConvergenceMasterDataTypeEnum, string>
                {
                    {
                        ConvergenceMasterDataTypeEnum.Country, "CountriesAssignedList"
                    }
                });

            SetViewBagDisplayabled();
        }

        List<SelectListItem> InitializeESCommitteeEmail()
        {
            var escTeamresponse = _fundInformationService.GetESCTeam();
            var response = new List<SelectListItem>();

            if (escTeamresponse.IsValid)
            {
                escTeamresponse.EscTeamList.ForEach(x =>
                    response.Add(new SelectListItem()
                    {
                        Value = x.EscTeamId.ToString(),
                        Text = x.EscEmail
                    }));
            }

            return response;
        }

        private List<string> GetApplicationAndNameReport(
            OutputFormatEnum outputFormat,
            string nameReport)
        {
            var response = new List<string>();
            string application = string.Empty;

            if (outputFormat == OutputFormatEnum.PDF)
            {
                response.Add(TCGlobalValues.APPLICATION + TCGlobalValues.DOC);
                response.Add(nameReport += TCGlobalValues.PDF);
                return response;
            }

            if (outputFormat == OutputFormatEnum.Word)
            {
                response.Add(TCGlobalValues.APPLICATION + TCGlobalValues.DOC);
                response.Add(nameReport += TCGlobalValues.DOC);
                return response;
            }

            if (outputFormat == OutputFormatEnum.WORDOPENXML)
            {
                response.Add(TCGlobalValues.APPLICATION + TCGlobalValues.DOCX);
                response.Add(nameReport += TCGlobalValues.DOCX);
                return response;
            }

            response.Add(TCGlobalValues.APPLICATION + TCGlobalValues.XLS);
            response.Add(nameReport += TCGlobalValues.XLS);
            return response;
        }

        private CustomEnumDictionary<int> SetViewBagEnumMapping<T>()
        {
            var result = _enumMappingService.GetMappingCodeCollection<T>();

            if (ViewBag.EnumMapping == null)
            {
                ViewBag.EnumMapping = result;
                return result;
            }

            ((CustomEnumDictionary<int>)ViewBag.EnumMapping).AddRange(result);
            return ViewBag.EnumMapping;
        }

        private string GetSelectedItemText(List<SelectListItem> listOption, int? value)
        {
            if ((listOption == null) || !listOption.Any() || !value.HasValue)
                return string.Empty;

            var text = string.Empty;
            foreach (var option in listOption)
            {
                if (Convert.ToInt32(option.Value) == value.Value)
                    return option.Text;
            }

            return text;
        }

        void SetViewBagRoleCoordinator(RoleEnum role, string operationNumber = null)
        {
            ViewBag.IsCoordinator = false;

            if (ViewBag.IsValid == false)
                return;

            if (!string.IsNullOrEmpty(operationNumber))
            {
                var responseIsInactive = _tcCommonService.IsInactiveOperation(operationNumber);

                if (!responseIsInactive.IsValid)
                {
                    SetViewBagErrorMessageInvalidResponse(responseIsInactive);

                    return;
                }

                if (!responseIsInactive.NullableValue.HasValue ||
                    responseIsInactive.NullableValue.Value)
                    return;
            }

            ViewBag.IsCoordinator = _authorizationService
                .HasGlobalRole(IDBContext.Current.UserLoginName, role);
        }

        private void SetViewBagRoleFundCoordinator(int fundId)
        {
            ViewBag.IsMyFund = false;

            if (ViewBag.IsValid ?? false)
            {
                var isFundCoordinator = _authorizationService
                    .HasGlobalRole(IDBContext.Current.UserLoginName, RoleEnum.FundCoordinator);

                var isMyFund = _filterFundsService
                    .IsFundCoordinator(IDBContext.Current.UserLoginName, fundId).IsFundCoordinator;

                ViewBag.IsMyFund = isFundCoordinator && isMyFund;
            }
        }

        void SetViewBagRoles(
            string operationNumber,
            ActionEnum readPermission,
            ActionEnum? writePermission = null,
            ActionEnum? writeTLPermission = null)
        {
            ViewBag.ReadRole = false;
            ViewBag.WriteRole = false;
            ViewBag.WriteTLRole = false;

            ViewBag.UserName = IDBContext.Current.UserName;

            if (ViewBag.IsValid == false)
                return;

            ViewBag.ReadRole = _authorizationService.IsAuthorized(
                    IDBContext.Current.UserLoginName,
                    operationNumber,
                    readPermission,
                    true);

            if (!writePermission.HasValue)
                return;

            var responseIsInactive = _tcCommonService.IsInactiveOperation(operationNumber);

            if (!responseIsInactive.IsValid)
            {
                SetViewBagErrorMessageInvalidResponse(responseIsInactive);

                return;
            }

            if (!responseIsInactive.NullableValue.HasValue ||
                responseIsInactive.NullableValue.Value)
                return;

            ViewBag.WriteRole = _authorizationService.IsAuthorized(
                IDBContext.Current.UserLoginName,
                operationNumber,
                writePermission.Value,
                true);

            if (writeTLPermission.HasValue)
                ViewBag.WriteTLRole = _authorizationService.IsAuthorized(
                    IDBContext.Current.UserLoginName,
                    operationNumber,
                    writeTLPermission.Value,
                    true);
        }

        void SetViewBagRoles(
            ActionEnum readPermission,
            ActionEnum? writePermission = null,
            string operationNumber = null)
        {
            ViewBag.ReadRole = false;
            ViewBag.WriteRole = false;

            ViewBag.UserName = IDBContext.Current.UserName;

            if (ViewBag.IsValid == false)
                return;

            ViewBag.ReadRole = _authorizationService.IsGlobalAuthorized(
                    IDBContext.Current.UserLoginName,
                    readPermission);

            if (!writePermission.HasValue)
                return;

            if (!string.IsNullOrEmpty(operationNumber))
            {
                var responseIsInactive = _tcCommonService.IsInactiveOperation(operationNumber);

                if (!responseIsInactive.IsValid)
                {
                    SetViewBagErrorMessageInvalidResponse(responseIsInactive);

                    return;
                }

                if (!responseIsInactive.NullableValue.HasValue ||
                    responseIsInactive.NullableValue.Value)
                    return;
            }

            ViewBag.WriteRole = _authorizationService.IsGlobalAuthorized(
                IDBContext.Current.UserLoginName,
                writePermission.Value);
        }

        private void SetViewBagGlobalRolesList(params ActionEnum[] roleList)
        {
            ViewBag.GLobalRoleList = new Dictionary<ActionEnum, bool>();

            if (ViewBag.IsValid ?? true)
                ViewBag.GLobalRoleList = GetGlobalRoleList(roleList);
        }

        private IDictionary<ActionEnum, bool> GetGlobalRoleList(params ActionEnum[] roleList)
        {
            IDictionary<ActionEnum, bool> roles = new Dictionary<ActionEnum, bool>();

            if ((roleList != null) && roleList.Any())
            {
                var hasReadPermission = roleList.Any(x => x == ActionEnum.ConvergenceReadPermission);

                var roleListCpy = new List<ActionEnum>(roleList);
                roleListCpy.RemoveAll(x => x == ActionEnum.ConvergenceReadPermission);

                //ToDo: R4 Permission - Pending Migrations
                roles = _authorizationService
                    .IsGlobalAuthorized(IDBContext.Current.UserLoginName, roleListCpy.ToArray());

                //ToDo: Verify in IDBContext R5 Permission 
                if (!roles[ActionEnum.GCMReadPermission])
                    roles[ActionEnum.GCMReadPermission] = IDBContext
                        .Current.HasPermission(Permission.GCM_READ);
                if (!roles[ActionEnum.GCMReadPermission])
                {
                    roles[ActionEnum.GCMReadPermission] = IDBContext.Current.HasPermission(Permission.GCM_READ);
                }

                if (!roles[ActionEnum.GCMChiefWrite])
                    roles[ActionEnum.GCMChiefWrite] = IDBContext
                        .Current.HasPermission(Permission.GCM_CHIEF_WRITE);

                if (hasReadPermission)
                    roles.Add(ActionEnum.ConvergenceReadPermission, true);
            }

            return roles;
        }

        private bool SetViewBagErrorMessageInvalidResponse(ResponseBase response)
        {
            if (!string.IsNullOrEmpty(response.ErrorMessage))
                ViewBag.ErrorMessage = response.ErrorMessage;

            ViewBag.IsValid = ViewBag.IsValid ?? true && response.IsValid;

            return response.IsValid;
        }

        void SetCanEditViewBag(bool hasWritePermission, bool canEdit = true)
        {
            ViewBag.CanEdit = hasWritePermission && canEdit;
        }

        private void SetViewBagDisplayabled()
        {
            ViewBag.Displayed = new List<SelectListItem>();

            for (int i = 1; i < 5; i++)
            {
                var listItem = new SelectListItem
                {
                    Value = (i * 5).ToString(),
                    Text = (i * 5).ToString() + " Displayed"
                };

                ViewBag.Displayed.Add(listItem);
            }

            ViewBag.Displayed.Add(new SelectListItem
            {
                Value = "-1",
                Text = "Show All"
            });
        }

        private void SetViewBagBeneficiaryCountries()
        {
            var beneficiaryCountries =
                GetMultiSelectListItemByType(ConvergenceMasterDataTypeEnum.Country);

            var borrowingCountries =
                GetMultiSelectListItemByType(ConvergenceMasterDataTypeEnum.BorrowingCountry);

            beneficiaryCountries.AddRange(borrowingCountries);

            ViewBag.BeneficiaryCountries = beneficiaryCountries;
        }

        List<MultiSelectListItem> GetMultiSelectListItemByType(
            ConvergenceMasterDataTypeEnum type)
        {
            List<MultiSelectListItem> elements = new List<MultiSelectListItem>();

            var listRepository = _catalogService.GetMasterDataListByTypeCode(
                hideExpired: true, typeCodes: type.GetEnumDescription());

            if (listRepository.IsValid && listRepository.MasterDataCollection.Any())
            {
                elements = listRepository.MasterDataCollection.Select(x =>
                    new MultiSelectListItem
                    {
                        Value = x.MasterId.ToString(),
                        Text = x.GetLocalizedName(),
                    })
                    .ToList();
            }

            return elements;
        }

        private string DisplayAmount(decimal amount, string format)
        {
            return "<div class=\"amount-alignment\">" +
                FormatHelper.Format(amount, format) + "</div>";
        }

        ResponseBase RetrieveTaskDataToFund(
            string operationNumber,
            string workflowTaskCode,
            out TaskDataModel taskModel)
        {
            var response = new ResponseBase();
            taskModel = null;

            try
            {
                taskModel = _k2IntegrationHelper
                    .GetTaskData(operationNumber, workflowTaskCode, string.Empty);

                response.IsValid = true;
                return response;
            }
            catch (Exception ex)
            {
                Logger.GetLogger()
                    .WriteError("TCViewController-RetrieveTaskDataToFund", ex.Message, ex);
                response.ErrorMessage = Localization
                    .GetText(TCGlobalValues.GET_DECISION_ERROR_MESSAGE);
                return response;
            }
        }

        ResponseBase StoreTaskData(string operationNumber, TaskDataModel taskModel)
        {
            var response = new ResponseBase();

            try
            {
                var fundID = 0;
                var responseFundID = TryParseFundID(operationNumber, taskModel, out fundID);

                if (!responseFundID.IsValid)
                    return responseFundID;

                if (fundID > 0)
                    K2Helper.StoreK2Data(taskModel, taskModel.OperationNumber, fundID);
                else
                    K2Helper.StoreK2Data(taskModel, taskModel.OperationNumber, null);

                response.IsValid = true;
                return response;
            }
            catch (Exception ex)
            {
                Logger.GetLogger().WriteError("TCViewController-StoreTaskData", ex.Message, ex);
                response.ErrorMessage = Localization
                    .GetText(TCGlobalValues.ERROR_TC_ABSTRACT_SERVICE);
                return response;
            }
        }

        ResponseBase TryParseFundID(string operationNumber, TaskDataModel model, out int result)
        {
            var response = new ResponseBase();
            result = 0;

            try
            {
                if (model.Parameters != null &&
                    model.Parameters.ContainsKey(TCGlobalValues.FUND_ID))
                {
                    int.TryParse(model.Parameters[TCGlobalValues.FUND_ID].ToString(), out result);
                    response.IsValid = true;
                    return response;
                }

                if (Request.Params != null && !string.IsNullOrEmpty(Request.Params[TRUST_FUND_ID]))
                {
                    int.TryParse(Request.Params[TRUST_FUND_ID].ToString(), out result);
                    response.IsValid = true;
                    return response;
                }

                response.IsValid = true;
                return response;
            }
            catch (Exception ex)
            {
                Logger.GetLogger().WriteError("TCViewController-GetFundID", ex.Message, ex);

                response.ErrorMessage = Localization
                    .GetText(TCGlobalValues.GET_DECISION_ERROR_MESSAGE);

                return response;
            }
        }

        bool HasRevertFundingProcessPermission()
        {
            var responseIsInactive = _tcCommonService
                .IsInactiveOperation(IDBContext.Current.Operation);

            if (!responseIsInactive.IsValid)
            {
                SetViewBagErrorMessageInvalidResponse(responseIsInactive);

                return false;
            }

            if (!responseIsInactive.NullableValue.HasValue ||
                responseIsInactive.NullableValue.Value)
                return false;

            try
            {
                return _authorizationService.IsGlobalAuthorized(
                    IDBContext.Current.UserLoginName,
                    ActionEnum.SWCoordinatorWritePermission);
            }
            catch (Exception ex)
            {
                Logger.GetLogger().WriteError(
                    "TCViewController-HasRevertFundingProcessPermission", ex.Message, ex);

                return false;
            }
        }

        IEnumerable<SelectListItem> GetCountryLiaisons()
        {
            var countryLiaisons = _catalogService.GetCountryDepartment()
                .Where(x => x.Code != CountryCode.IDB);

            return ControllerHelper.ConvertToSelectListItems(countryLiaisons, true)
                .OrderBy(x => x.Text)
                .ToList();
        }

        #endregion
    }
}