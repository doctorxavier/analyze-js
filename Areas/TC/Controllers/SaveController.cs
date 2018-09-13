using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.Architecture.Logging;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.TCAbstractModule.Email;
using IDB.MW.Application.TCAbstractModule.Email.Enumerators;
using IDB.MW.Application.TCAbstractModule.Helpers;
using IDB.MW.Application.TCAbstractModule.Messages.DonorDecision;
using IDB.MW.Application.TCAbstractModule.Messages.EligibilityDecision;
using IDB.MW.Application.TCAbstractModule.Messages.ESCDecision;
using IDB.MW.Application.TCAbstractModule.Messages.FundInformation;
using IDB.MW.Application.TCAbstractModule.Messages.ReviewTCAbstractPostValidation;
using IDB.MW.Application.TCAbstractModule.Messages.SingleWindowDecision;
using IDB.MW.Application.TCAbstractModule.Messages.SingleWindowMeeting;
using IDB.MW.Application.TCAbstractModule.Messages.SingleWindowOperationDecision;
using IDB.MW.Application.TCAbstractModule.Messages.TCAbstractService;
using IDB.MW.Application.TCAbstractModule.Services.DonorDecision;
using IDB.MW.Application.TCAbstractModule.Services.EligibilityDecision;
using IDB.MW.Application.TCAbstractModule.Services.ESCDecision;
using IDB.MW.Application.TCAbstractModule.Services.FundInformation;
using IDB.MW.Application.TCAbstractModule.Services.NotifyFunding;
using IDB.MW.Application.TCAbstractModule.Services.RequestIncrease;
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
using IDB.MW.Application.TCAbstractModule.ViewModels.Shared;
using IDB.MW.Application.TCAbstractModule.ViewModels.SingleWindowOperationDecision;
using IDB.MW.Application.TCAbstractModule.ViewModels.TCAbstractService;
using IDB.MW.Application.TCAbstractModule.ViewModels.ValidationTCAbstract;
using IDB.MW.Business.Core.K2Manager.Contracts;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.MW.DomainModel.Entities.Core.K2;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.MW.Infrastructure.Configuration;
using IDB.Presentation.MVC4.Areas.TC.Enums;
using IDB.Presentation.MVC4.Areas.TC.Models;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.General;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.TC.Controllers
{
    public partial class SaveController : BaseController
    {
        #region Constants

        public const string SAVE_CONTROLLER_SUBMITTYPE = "SubmitType";
        public const string TC_TASK_STATUS_MODAL = "_TCTaskStatusModal";
        public const string SUBMIT_EMAIL_STAGE = "SubmitEmailStage";

        #endregion

        #region Fields

        readonly ITCAbstractService _tcAbstractService;
        readonly INotifyFundingService _notifyFundingService;
        readonly IReviewTCAbstractService _reviewTcAbstractService;
        readonly IValidationTCAbstractService _validationTcAbstractService;
        readonly IReviewFundingCoordinatorService _reviewFundingCoordinatorService;
        readonly IReviewRegionalTLService _reviewRegionalTLService;
        readonly ISingleWindowOperationDecisionService _singleWindowOperationDecisionService;
        readonly ISingleWindowDecisionService _singleWindowDecisionService;
        readonly ISingleWindowMeetingService _singleWindowMeetingService;
        readonly IDonorDecisionService _donorDecisionService;
        readonly IESCDecisionService _escDecisionService;
        readonly IRequestIncreaseService _requestIncreaseService;
        readonly IReviewTCAbstractPostValidationService _reviewTCAbstractPostValidationService;
        readonly IEligibilityDecisionService _eligibilityDecisionService;
        readonly IFundInformationService _fundInformationService;
        readonly IEnumMappingService _enumMappingService;
        readonly IK2Manager _k2Manager;
        readonly K2IntegrationHelper _k2IntegrationHelper;
        readonly ISingleWindowOperationService _singleWindowOperationService;
        readonly ITCAbstractModuleEmailManager _tcAbstractModuleEmailManager;

        #endregion

        #region Constructors

        public SaveController(
            ITCAbstractService tcAbstractService,
            INotifyFundingService notifyFundingService,
            IReviewTCAbstractService reviewTcAbstractService,
            IValidationTCAbstractService validationTcAbstractService,
            IReviewFundingCoordinatorService reviewFundingCoordinatorService,
            IReviewRegionalTLService reviewRegionalTLService,
            ISingleWindowOperationDecisionService singleWindowOperationDecisionService,
            ISingleWindowDecisionService singleWindowDecisionService,
            ISingleWindowMeetingService singleWindowMeetingService,
            IDonorDecisionService donorDecisionService,
            IESCDecisionService escDecisionService,
            IRequestIncreaseService requestIncreaseService,
            IReviewTCAbstractPostValidationService reviewTCAbstractPostValidationService,
            IEligibilityDecisionService eligibilityDecisionService,
            IFundInformationService fundInformationService,
            IEnumMappingService enumMappingService,
            IK2Manager k2Manager,
            ISingleWindowOperationService singleWindowOperationService,
            IWorkflowInstanceTaskRepository workflowInstanceTaskRepository,
            IWorkflowInstanceRepository workflowInstanceRepository,
            ITCAbstractModuleEmailManager tcAbstractModuleEmailManager)
        {
            _tcAbstractService = tcAbstractService;
            _notifyFundingService = notifyFundingService;
            _reviewTcAbstractService = reviewTcAbstractService;
            _validationTcAbstractService = validationTcAbstractService;
            _reviewFundingCoordinatorService = reviewFundingCoordinatorService;
            _reviewRegionalTLService = reviewRegionalTLService;
            _singleWindowOperationDecisionService = singleWindowOperationDecisionService;
            _singleWindowDecisionService = singleWindowDecisionService;
            _singleWindowMeetingService = singleWindowMeetingService;
            _donorDecisionService = donorDecisionService;
            _escDecisionService = escDecisionService;
            _requestIncreaseService = requestIncreaseService;
            _reviewTCAbstractPostValidationService = reviewTCAbstractPostValidationService;
            _eligibilityDecisionService = eligibilityDecisionService;
            _fundInformationService = fundInformationService;
            _enumMappingService = enumMappingService;
            _k2Manager = k2Manager;
            _k2IntegrationHelper = new K2IntegrationHelper(
                k2Manager, 
                workflowInstanceTaskRepository,
                workflowInstanceRepository);
            _singleWindowOperationService = singleWindowOperationService;
            _tcAbstractModuleEmailManager = tcAbstractModuleEmailManager;
        }

        #endregion

        #region TCAbstract UI-FP-001

        public virtual ActionResult SaveTCAbstract(string operationNumber)
        {
            var responseView = new SaveResponse();
            ResponseBase response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<TCAbstractViewModel>(jsonDataRequest.SerializedData);

            var taskDataModel = K2Helper.RetrieveK2Data(operationNumber);

            if (taskDataModel == null)
            {
                taskDataModel = _k2IntegrationHelper.GetTaskData(
                    operationNumber, K2IntegrationHelper.CreateTCTaskCode);
            }

            if (_k2IntegrationHelper.IsTaskRunning(taskDataModel.TaskId, operationNumber))
            {
                K2Helper.StoreK2Data(taskDataModel, operationNumber, null);
            }
            else
            {
                taskDataModel = null;
            }

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(
                SynchronizationHelper.EDIT_MODE,
                URLConstants.URL_TCABSTRACT,
                operationNumber,
                userName);

            TCTaskStatusViewModel statusViewModel = null;

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            var submitType = jsonDataRequest.ClientFieldData.FirstOrDefault(
                x => x.Name == SAVE_CONTROLLER_SUBMITTYPE);

            viewModel.UpdateTCAbstractViewModel(jsonDataRequest.ClientFieldData);

            if (submitType == null || submitType.Value == SaveControllerTypeEnum.Save.ToString())
            {
                response = _tcAbstractService.SaveTCAbstract(operationNumber, viewModel);

                responseView.IsValid = response.IsValid;
                responseView.ErrorMessage = response.ErrorMessage;
            }
            else
            {
                response = _tcAbstractService
                    .SaveAndSendTCAbstract(operationNumber, viewModel, taskDataModel);

                responseView.IsValid = response.IsValid;
                responseView.ErrorMessage = response.ErrorMessage;

                if (responseView.IsValid)
                    statusViewModel = ((SaveAndSendTCAbstractResponse)response).TCTaskStatus;
            }

            if (responseView.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(
                    URLConstants.URL_TCABSTRACT, operationNumber, userName);

                CleanK2Data(operationNumber);
            }

            if (CheckTCTaskStatus(statusViewModel))
                responseView.ContentHTML = this
                    .RenderRazorViewToString(TC_TASK_STATUS_MODAL, statusViewModel);

            if (response is ResponseRedirectBase)
            {
                var responseRedirect = (ResponseRedirectBase)response;
                if (responseRedirect.Redirect)
                    responseView.UrlRedirect = Url.Action(
                        responseRedirect.Action,
                        responseRedirect.Controller,
                        new { area = responseRedirect.Area, operationNumber = operationNumber });
            }

            return Json(responseView);
        }

        #endregion

        #region ValidationTCAbstract UI-FP-002

        public virtual JsonResult ValidationTCAbstract(string operationNumber)
        {
            var responseView = new SaveResponse();

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<ValidationTCAbstractViewModel>(jsonDataRequest.SerializedData);

            var taskDataModel = GetTaskData(operationNumber, K2IntegrationHelper.ReviewTCAbstract);

            viewModel.UpdateValidationTCAbstractViewModel(
                jsonDataRequest.ClientFieldData,
                _enumMappingService);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(
                SynchronizationHelper.EDIT_MODE,
                URLConstants.URL_VALIDATION_TC_ABSTRACT,
                operationNumber,
                userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            TCTaskStatusViewModel statusViewModel = null;

            var submitType = jsonDataRequest.ClientFieldData
                .FirstOrDefault(x => x.Name == SAVE_CONTROLLER_SUBMITTYPE);

            if (submitType == null || submitType.Value == SaveControllerTypeEnum.Save.ToString())
            {
                var responseSave = _validationTcAbstractService
                    .SaveTCAbstractValidation(operationNumber, viewModel);

                responseView.IsValid = responseSave.IsValid;
                responseView.ErrorMessage = responseSave.ErrorMessage;
            }
            else
            {
                var responseSend = _validationTcAbstractService
                    .SaveAndSendTCAbstractValidation(operationNumber, viewModel, taskDataModel);

                responseView.IsValid = responseSend.IsValid;
                responseView.ErrorMessage = responseSend.ErrorMessage;

                if (responseView.IsValid)
                    statusViewModel = responseSend.TCTaskStatus;

                if (responseSend.Redirect)
                    responseView.UrlRedirect = Url.Action(
                        responseSend.Action,
                        responseSend.Controller,
                        new { area = responseSend.Area, operationNumber = operationNumber });
            }

            if (responseView.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(
                    URLConstants.URL_VALIDATION_TC_ABSTRACT,
                    operationNumber,
                    userName);

                CleanK2Data(operationNumber);
            }

            if (CheckTCTaskStatus(statusViewModel))
                responseView.ContentHTML = this
                    .RenderRazorViewToString(TC_TASK_STATUS_MODAL, statusViewModel);

            return Json(responseView);
        }

        #endregion

        #region ReviewTCAbstract UI-FP-003

        public virtual JsonResult ReviewTCAbstract(string operationNumber)
        {
            var responseView = new SaveResponse();

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<ReviewTCAbstractViewModel>(jsonDataRequest.SerializedData);

            var taskDataModel = K2Helper.RetrieveK2Data(operationNumber);

            viewModel.UpdateValidationTCAbstractViewModel(
                jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(
                SynchronizationHelper.EDIT_MODE,
                URLConstants.URL_REVIEW_TC_ABSTRACT,
                operationNumber,
                userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            TCTaskStatusViewModel statusViewModel = null;

            var submitType = jsonDataRequest.ClientFieldData
                .FirstOrDefault(x => x.Name == SAVE_CONTROLLER_SUBMITTYPE);

            if (submitType == null || submitType.Value == SaveControllerTypeEnum.Save.ToString())
            {
                var responseSave = _reviewTcAbstractService.SaveTCAbstractReviewTL(viewModel);

                responseView.IsValid = responseSave.IsValid;
                responseView.ErrorMessage = responseSave.ErrorMessage;
            }
            else
            {
                var responseSend = _reviewTcAbstractService.SaveAndSendTCAbstractValidation(
                    operationNumber,
                    viewModel,
                    taskDataModel);

                responseView.IsValid = responseSend.IsValid;
                responseView.ErrorMessage = responseSend.ErrorMessage;

                if (responseView.IsValid)
                    statusViewModel = responseSend.TCTaskStatus;

                if (responseSend.Redirect)
                    responseView.UrlRedirect = Url.Action(
                        responseSend.Action,
                        responseSend.Controller,
                        new { area = responseSend.Area, operationNumber = operationNumber });
            }

            if (responseView.IsValid)
                SynchronizationHelper.TryReleaseLock(
                    URLConstants.URL_REVIEW_TC_ABSTRACT,
                    operationNumber,
                    userName);

            if (CheckTCTaskStatus(statusViewModel))
                responseView.ContentHTML = this
                    .RenderRazorViewToString(TC_TASK_STATUS_MODAL, statusViewModel);

            return Json(responseView);
        }

        #endregion

        #region NotifyFC UI-FP-004

        public virtual JsonResult NotifyFC(string operationNumber)
        {
            var responseView = new SaveResponse();

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<TCNotifyFCViewModel>(jsonDataRequest.SerializedData);

            var taskDataModel = GetTaskData(operationNumber, K2IntegrationHelper.SelectFCsAndRTLs);

            viewModel.UpdateTCNotifyFCViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(
                SynchronizationHelper.EDIT_MODE,
                URLConstants.URL_NOTIFY_FC,
                operationNumber,
                userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            TCTaskStatusViewModel statusViewModel = null;

            var submitType = jsonDataRequest.ClientFieldData
                .FirstOrDefault(x => x.Name == SAVE_CONTROLLER_SUBMITTYPE);

            if (submitType == null || submitType.Value == SaveControllerTypeEnum.Save.ToString())
            {
                var responseSave = _notifyFundingService
                    .SaveTCNotifyFC(operationNumber, viewModel);

                responseView.IsValid = responseSave.IsValid;
                responseView.ErrorMessage = responseSave.ErrorMessage;
            }
            else
            {
                var responseSend = _notifyFundingService
                    .SaveAndSendTCNotifyFC(operationNumber, viewModel, taskDataModel);

                responseView.IsValid = responseSend.IsValid;
                responseView.ErrorMessage = responseSend.ErrorMessage;

                if (responseView.IsValid)
                    statusViewModel = responseSend.TCTaskStatus;

                if (responseSend.Redirect)
                    responseView.UrlRedirect = Url.Action(
                        responseSend.Action,
                        responseSend.Controller,
                        new { area = responseSend.Area });
            }

            if (responseView.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(
                    URLConstants.URL_NOTIFY_FC, operationNumber, userName);

                CleanK2Data(operationNumber);
            }

            if (CheckTCTaskStatus(statusViewModel))
                responseView.ContentHTML = this
                    .RenderRazorViewToString(TC_TASK_STATUS_MODAL, statusViewModel);

            return Json(responseView);
        }

        #endregion

        #region PriorityTCAbstract UI-FP-005
        public virtual JsonResult PriorityTCAbstract(string operationNumber, int countryLiaisonId)
        {
            var responseView = new SaveResponse();
            ResponseBase response;

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<PriorityTCAbstractViewModel>(jsonDataRequest.SerializedData);
            var taskDataModel = K2Helper.RetrieveK2Data(operationNumber);

            viewModel.UpdatePriorityTCAbstractViewModel(
                jsonDataRequest.ClientFieldData, _enumMappingService);

            string userLoginName = IDBContext.Current.UserLoginName;

            var errorMessage = SynchronizationHelper.AccessToResources(
                SynchronizationHelper.EDIT_MODE,
                URLConstants.URL_PRIORITY_TC_ABSTRACT,
                operationNumber,
                userLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
            }
            else
            {
                var submitType = jsonDataRequest.ClientFieldData
                    .FirstOrDefault(x => x.Name == SAVE_CONTROLLER_SUBMITTYPE);

                if (submitType == null || submitType.Value == SaveControllerTypeEnum.Save.ToString())
                {
                    response = _reviewRegionalTLService.SavePriorityTCAbstract(
                        operationNumber, countryLiaisonId, viewModel);
                    responseView.IsValid = response.IsValid;
                    responseView.ErrorMessage = response.ErrorMessage;
                }
                else
                {
                    response = _reviewRegionalTLService.SaveAndSendPriorityTCAbstract(
                        operationNumber, countryLiaisonId, viewModel, taskDataModel);
                    responseView.IsValid = response.IsValid;
                    responseView.ErrorMessage = response.ErrorMessage;
                }

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(
                        URLConstants.URL_PRIORITY_TC_ABSTRACT, operationNumber, userLoginName);
                }

                if (response is ResponseRedirectBase)
                {
                    var responseRedirect = (ResponseRedirectBase)response;
                    if (responseRedirect.Redirect)
                    {
                        responseView.UrlRedirect = Url.Action(
                            responseRedirect.Action,
                            responseRedirect.Controller,
                            new { area = responseRedirect.Area });
                    }
                }
            }

            return Json(responseView);
        }

        #endregion

        #region FundCoordinatorTCAbstract UI-FP-006
        public virtual JsonResult FundCoordinatorTCAbstract(string operationNumber, int trustFundId)
        {
            var responseView = new SaveResponse();
            ResponseBase response;

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<FundCoordinatorTCAbstractViewModel>(jsonDataRequest.SerializedData);
            var taskDataModel = K2Helper.RetrieveK2Data(operationNumber, trustFundId);

            viewModel.UpdateFundCoordinatorTCAbstractViewModel(jsonDataRequest.ClientFieldData, _enumMappingService);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(
                SynchronizationHelper.EDIT_MODE,
                URLConstants.URL_FUND_COORDINATOR_TC_ABSTRACT,
                operationNumber,
                userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
            }
            else
            {
                var submitType = jsonDataRequest.ClientFieldData.FirstOrDefault(x => x.Name == SAVE_CONTROLLER_SUBMITTYPE);
                if (submitType == null || submitType.Value == SaveControllerTypeEnum.Save.ToString())
                {
                    response = _reviewFundingCoordinatorService.SaveReviewFundingCoordinator(operationNumber, trustFundId, viewModel);
                    responseView.IsValid = response.IsValid;
                    responseView.ErrorMessage = response.ErrorMessage;
                }
                else
                {
                    response = _reviewFundingCoordinatorService.SaveAndSendReviewFundingCoordinator(operationNumber, trustFundId, viewModel, taskDataModel);

                    responseView.IsValid = response.IsValid;
                    responseView.ErrorMessage = response.ErrorMessage;
                }

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(
                        URLConstants.URL_FUND_COORDINATOR_TC_ABSTRACT, operationNumber, userName);
                }

                if (response is ResponseRedirectBase)
                {
                    var responseRedirect = (ResponseRedirectBase)response;
                    if (responseRedirect.Redirect)
                    {
                        responseView.UrlRedirect = Url.Action(
                            responseRedirect.Action,
                            responseRedirect.Controller,
                            new { area = responseRedirect.Area });
                    }
                }
            }

            return Json(responseView);
        }
        #endregion

        #region SingleWindowMeeting UI-FP-007
        public virtual JsonResult SWMeetingStdComment()
        {
            ResponseBase response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<SingleWindowMeetingViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateSingleWindowMeetingViewModel(jsonDataRequest.ClientFieldData);

            response = _singleWindowMeetingService.SaveWindowMeeting(viewModel);

            return Json(response);
        }

        public virtual JsonResult SWMeetingGeneralComment()
        {
            SaveMeetingGeneralCommentsResponse response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<MeetingGeneralCommentsViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateMeetingGeneralCommentsViewModel(jsonDataRequest.ClientFieldData);

            response = _singleWindowMeetingService.SaveMeetingGeneralComments(viewModel);

            return Json(response);
        }
        #endregion

        #region DecisionSWOperation UI-FP-009

        public virtual ActionResult DecisionSWOperation(string operationNumber)
        {
            var responseView = new SaveResponse();

            ResponseBase response;

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);

            var viewModel = PageSerializationHelper
                .DeserializeObject<DecisionSWOperationViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateDecisionSWOpertationViewModel(
                jsonDataRequest.ClientFieldData,
                _enumMappingService);

            var userName = IDBContext.Current.UserLoginName;

            var errorMessage = SynchronizationHelper.AccessToResources(
                SynchronizationHelper.EDIT_MODE,
                URLConstants.URL_DECISION_SW_OPERATION,
                operationNumber,
                userName);

            OperationStatusViewModel statusViewModel = null;

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
            }
            else
            {
                var submitType = jsonDataRequest.ClientFieldData
                    .FirstOrDefault(x => x.Name == SAVE_CONTROLLER_SUBMITTYPE);

                if (submitType == null || submitType.Value == SaveControllerTypeEnum.Save.ToString())
                {
                    response = _singleWindowOperationDecisionService
                        .SaveSingleWindowOperationDecision(operationNumber, viewModel);

                    responseView.IsValid = response.IsValid;
                    responseView.ErrorMessage = response.ErrorMessage;
                }
                else
                {
                    response = _singleWindowOperationDecisionService
                        .SaveAndSendSingleWindowOperationDecision(operationNumber, viewModel);

                    responseView.IsValid = response.IsValid;
                    responseView.ErrorMessage = response.ErrorMessage;

                    if (responseView.IsValid)
                        statusViewModel = ((SaveAndSendSingleWindowOperationDecisionResponse)response)
                            .OperationStatusViewModel;
                }

                if (responseView.IsValid)
                    SynchronizationHelper.TryReleaseLock(
                        URLConstants.URL_DECISION_SW_OPERATION,
                        operationNumber,
                        userName);

                if (statusViewModel != null)
                    responseView.ContentHTML = this
                        .RenderRazorViewToString("_OperationStatusModal", statusViewModel);

                if (response is ResponseRedirectBase)
                {
                    var responseRedirect = (ResponseRedirectBase)response;

                    if (responseRedirect.Redirect)
                    {
                        var SWDateFrom = jsonDataRequest.ClientFieldData
                            .First(x => x.Name == "SWDateFrom").Value;

                        var SWDateTo = jsonDataRequest.ClientFieldData
                            .First(x => x.Name == "SWDateTo").Value;

                        var typeDecision = int.Parse(
                            jsonDataRequest.ClientFieldData.First(x => x.Name == "TypeDecision").Value);

                        responseView.UrlRedirect = Url.Action(
                            responseRedirect.Action,
                            responseRedirect.Controller,
                            new
                            {
                                area = responseRedirect.Area,
                                operationNumber = operationNumber,
                                typeDecision = typeDecision,
                                fromDate = SWDateFrom,
                                toDate = SWDateTo
                            });
                    }
                }
            }

            return Json(responseView);
        }

        #endregion

        #region SingleWindowDecision UI-FP-010

        public virtual ActionResult SingleWindowDecision(string operationNumber)
        {
            var responseView = new SaveResponse();
            ResponseBase response;

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<SingleWindowDecisionViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateSingleWindowDecisionViewModel(
                jsonDataRequest.ClientFieldData,
                _enumMappingService);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(
                SynchronizationHelper.EDIT_MODE,
                URLConstants.URL_SINGLE_WINDOW_DECISION,
                operationNumber,
                userName);

            TCTaskStatusViewModel statusViewModel = null;

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
            }
            else
            {
                var submitType = jsonDataRequest.ClientFieldData
                    .FirstOrDefault(x => x.Name == SAVE_CONTROLLER_SUBMITTYPE);

                if (submitType == null ||
                    submitType.Value == SaveControllerTypeEnum.Save.ToString())
                {
                    response = _singleWindowDecisionService
                        .SaveSingleWindowDecision(operationNumber, viewModel);

                    responseView.IsValid = response.IsValid;
                    responseView.ErrorMessage = response.ErrorMessage;

                    if (response.IsValid)
                    {
                        statusViewModel = new TCTaskStatusViewModel
                        {
                            OperationStatus = ((SaveSingleWindowDecisionResponse)response)
                            .OperationStatusViewModel
                        };
                    }
                }
                else
                {
                    response = _singleWindowDecisionService
                        .SaveAndSendSingleWindowDecision(operationNumber, viewModel);

                    responseView.IsValid = response.IsValid;
                    responseView.ErrorMessage = response.ErrorMessage;

                    if (response.IsValid)
                        statusViewModel = ((SaveAndSendSingleWindowDecisionResponse)response)
                            .TCTaskStatus;
                }

                if (responseView.IsValid)
                    SynchronizationHelper.TryReleaseLock(
                        URLConstants.URL_SINGLE_WINDOW_DECISION, operationNumber, userName);

                if (CheckTCTaskStatus(statusViewModel))
                    responseView.ContentHTML = this
                        .RenderRazorViewToString(TC_TASK_STATUS_MODAL, statusViewModel);

                if (response is ResponseRedirectBase)
                {
                    var responseRedirect = (ResponseRedirectBase)response;

                    if (responseRedirect.Redirect)
                    {
                        var SWMDateFrom = jsonDataRequest.ClientFieldData
                            .First(x => x.Name == "SWMDateFrom").Value;
                        var SWMDateTo = jsonDataRequest.ClientFieldData
                            .First(x => x.Name == "SWMDateTo").Value;

                        responseView.UrlRedirect = Url.Action(
                            responseRedirect.Action,
                            responseRedirect.Controller,
                            new
                            {
                                area = responseRedirect.Area,
                                operationNumber = operationNumber,
                                fromDate = SWMDateFrom,
                                toDate = SWMDateTo
                            });
                    }
                }
            }

            return Json(responseView);
        }

        #endregion

        #region EligibilityDecision UI-FP-011

        public virtual ActionResult EligibilityDecision(string operationNumber)
        {
            var responseView = new SaveResponse();
            ResponseBase response;

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<EligibilityDecisionViewModel>(jsonDataRequest.SerializedData);

            var taskDataModel = GetTaskData(
                operationNumber, K2IntegrationHelper.EligibilityTaskCode, viewModel.TrustFundId);

            viewModel.UpdateEligibilityDecisionViewModel(
                jsonDataRequest.ClientFieldData, _enumMappingService);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(
                SynchronizationHelper.EDIT_MODE,
                URLConstants.URL_ELIGIBILITY_DECISION,
                operationNumber,
                userName);

            TCTaskStatusViewModel statusViewModel = null;

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
            }
            else
            {
                var submitType = jsonDataRequest.ClientFieldData
                    .FirstOrDefault(x => x.Name == SAVE_CONTROLLER_SUBMITTYPE);

                if (submitType == null ||
                    submitType.Value == SaveControllerTypeEnum.Save.ToString())
                {
                    response = _eligibilityDecisionService.SaveEligibilityDecision(viewModel);

                    responseView.IsValid = response.IsValid;
                    responseView.ErrorMessage = response.ErrorMessage;
                }
                else
                {
                    response = _eligibilityDecisionService
                        .SaveAndSendEligibilityDecision(viewModel, taskDataModel);

                    responseView.IsValid = response.IsValid;
                    responseView.ErrorMessage = response.ErrorMessage;

                    if (responseView.IsValid)
                        statusViewModel = ((SaveAndSendEligibilityDecisionResponse)response)
                            .TCTaskStatus;
                }

                if (responseView.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(
                        URLConstants.URL_ELIGIBILITY_DECISION, operationNumber, userName);

                    CleanK2Data(operationNumber, viewModel.TrustFundId);
                }

                if (CheckTCTaskStatus(statusViewModel))
                    responseView.ContentHTML = this
                        .RenderRazorViewToString(TC_TASK_STATUS_MODAL, statusViewModel);

                if (response is ResponseRedirectBase)
                {
                    var responseRedirect = (ResponseRedirectBase)response;

                    if (responseRedirect.Redirect)
                        responseView.UrlRedirect = Url.Action(
                            responseRedirect.Action,
                            responseRedirect.Controller,
                            new
                            {
                                area = responseRedirect.Area,
                                operationNumber = operationNumber
                            });
                }
            }

            return Json(responseView);
        }

        #endregion

        #region ReviewPostValidation UI-FP-012

        public virtual ActionResult ReviewPostValidation(string operationNumber)
        {
            var responseView = new SaveResponse();
            ResponseBase response;

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<ReviewTCAbstractPostValidationViewModel>(
                    jsonDataRequest.SerializedData);

            var taskDataModel = K2Helper.RetrieveK2Data(operationNumber, viewModel.TrustFundId);

            viewModel.UpdateReviewTCAbstractPostValidationViewModel(
                jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(
                SynchronizationHelper.EDIT_MODE,
                URLConstants.URL_REVIEW_POST_VALIDATION,
                operationNumber,
                userName);

            TCTaskStatusViewModel statusViewModel = null;

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
            }
            else
            {
                var submitType = jsonDataRequest.ClientFieldData
                    .FirstOrDefault(x => x.Name == SAVE_CONTROLLER_SUBMITTYPE);

                if (submitType == null ||
                    submitType.Value == SaveControllerTypeEnum.Save.ToString())
                {
                    response = _reviewTCAbstractPostValidationService
                        .SaveReviewTCAbstractPostValidation(viewModel);

                    responseView.IsValid = response.IsValid;
                    responseView.ErrorMessage = response.ErrorMessage;
                }
                else
                {
                    response = _reviewTCAbstractPostValidationService
                        .SendFundCoordinator(viewModel, taskDataModel);

                    responseView.IsValid = response.IsValid;
                    responseView.ErrorMessage = response.ErrorMessage;

                    if (responseView.IsValid)
                        statusViewModel = ((SendFundCoordinatorResponse)response).TCTaskStatus;
                }

                if (responseView.IsValid)
                    SynchronizationHelper.TryReleaseLock(
                        URLConstants.URL_REVIEW_POST_VALIDATION, operationNumber, userName);

                if (CheckTCTaskStatus(statusViewModel))
                    responseView.ContentHTML = this
                        .RenderRazorViewToString(TC_TASK_STATUS_MODAL, statusViewModel);

                if (response is ResponseRedirectBase)
                {
                    var responseRedirect = (ResponseRedirectBase)response;

                    if (responseRedirect.Redirect)
                        responseView.UrlRedirect = Url.Action(
                            responseRedirect.Action,
                            responseRedirect.Controller,
                            new
                            {
                                area = responseRedirect.Area,
                                operationNumber = operationNumber
                            });
                }
            }

            return Json(responseView);
        }

        #endregion

        #region DonorDecision UI-FP-013

        public virtual ActionResult DonorDecision(string operationNumber)
        {
            var responseView = new SaveResponse();
            ResponseBase response;

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<DonorDecisionViewModel>(jsonDataRequest.SerializedData);

            var taskDataModel = GetTaskData(
                operationNumber, K2IntegrationHelper.DonorTaskCode, viewModel.TrustFundId);

            viewModel.UpdateDonorDecisionViewModel(
                jsonDataRequest.ClientFieldData, _enumMappingService);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(
                SynchronizationHelper.EDIT_MODE,
                URLConstants.URL_DONOR_DECISION,
                operationNumber,
                userName);

            TCTaskStatusViewModel statusViewModel = null;

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
            }
            else
            {
                var submitType = jsonDataRequest.ClientFieldData.FirstOrDefault(x =>
                    x.Name == SAVE_CONTROLLER_SUBMITTYPE);

                if (submitType == null ||
                    submitType.Value == SaveControllerTypeEnum.Save.ToString())
                {
                    response = _donorDecisionService.SaveDonorDecision(viewModel);

                    responseView.IsValid = response.IsValid;
                    responseView.ErrorMessage = response.ErrorMessage;
                }
                else
                {
                    response = _donorDecisionService
                        .SaveAndSendDonorDecision(viewModel, taskDataModel);

                    responseView.IsValid = response.IsValid;
                    responseView.ErrorMessage = response.ErrorMessage;

                    if (responseView.IsValid)
                        statusViewModel = ((SaveAndSendDonorDecisionResponse)response)
                            .TCTaskStatus;
                }

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(
                        URLConstants.URL_DONOR_DECISION, operationNumber, userName);

                    CleanK2Data(operationNumber, viewModel.TrustFundId);
                }

                if (CheckTCTaskStatus(statusViewModel))
                    responseView.ContentHTML = this
                        .RenderRazorViewToString(TC_TASK_STATUS_MODAL, statusViewModel);

                if (response is ResponseRedirectBase)
                {
                    var responseRedirect = (ResponseRedirectBase)response;
                    if (responseRedirect.Redirect)
                        responseView.UrlRedirect = Url.Action(
                            responseRedirect.Action,
                            responseRedirect.Controller,
                            new { area = responseRedirect.Area });
                }
            }

            return Json(responseView);
        }

        #endregion

        #region EscDecision UI-FP-014

        public virtual ActionResult EscDecision(string operationNumber)
        {
            var responseView = new SaveResponse();
            ResponseBase response;

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<ESCDecisionViewModel>(jsonDataRequest.SerializedData);

            var taskDataModel = GetTaskData(
                operationNumber, K2IntegrationHelper.EscTaskCode, viewModel.TrustFundId);

            viewModel.UpdateEscDecisionViewModel(
                jsonDataRequest.ClientFieldData, _enumMappingService);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(
                SynchronizationHelper.EDIT_MODE,
                URLConstants.URL_ESC_DECISION,
                operationNumber,
                userName);

            TCTaskStatusViewModel statusViewModel = null;

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
            }
            else
            {
                var submitType = jsonDataRequest.ClientFieldData.FirstOrDefault(x =>
                    x.Name == SAVE_CONTROLLER_SUBMITTYPE);

                if (submitType == null ||
                    submitType.Value == SaveControllerTypeEnum.Save.ToString())
                {
                    response = _escDecisionService.SaveESCDecision(viewModel);

                    responseView.IsValid = response.IsValid;
                    responseView.ErrorMessage = response.ErrorMessage;
                }
                else
                {
                    response = _escDecisionService
                        .SaveAndSendESCDecision(viewModel, taskDataModel);

                    responseView.IsValid = response.IsValid;
                    responseView.ErrorMessage = response.ErrorMessage;

                    if (responseView.IsValid)
                        statusViewModel = ((SaveAndSendESCDecisionResponse)response)
                            .TCTaskStatus;
                }

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(
                        URLConstants.URL_ESC_DECISION, operationNumber, userName);

                    CleanK2Data(operationNumber, viewModel.TrustFundId);
                }

                if (CheckTCTaskStatus(statusViewModel))
                    responseView.ContentHTML = this
                        .RenderRazorViewToString(TC_TASK_STATUS_MODAL, statusViewModel);

                if (response is ResponseRedirectBase)
                {
                    var responseRedirect = (ResponseRedirectBase)response;

                    if (responseRedirect.Redirect)
                        responseView.UrlRedirect = Url.Action(
                            responseRedirect.Action,
                            responseRedirect.Controller,
                            new { area = responseRedirect.Area });
                }
            }

            return Json(responseView);
        }

        #endregion

        #region RequestIncrease UI-FP-016
        public virtual ActionResult RequestIncrease(string operationNumber)
        {
            var responseView = new SaveResponse();
            ResponseBase response = new ResponseBase();

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<RequestIncreaseViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateRequestIncreaseViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(
                SynchronizationHelper.EDIT_MODE,
                URLConstants.URL_REQUEST_INCREASE,
                operationNumber,
                userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
            }
            else
            {
                var submitType = jsonDataRequest.ClientFieldData.FirstOrDefault(x => x.Name == SAVE_CONTROLLER_SUBMITTYPE);
                if (submitType == null || submitType.Value == SaveControllerTypeEnum.Save.ToString())
                {
                    response = _requestIncreaseService.SaveRequestIncrease(operationNumber, viewModel);
                    responseView.IsValid = response.IsValid;
                    responseView.ErrorMessage = response.ErrorMessage;
                }

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(
                        URLConstants.URL_REQUEST_INCREASE, operationNumber, userName);
                }

                var responseCastRedirect = response as ResponseRedirectBase;

                if (responseCastRedirect != null && responseCastRedirect.Redirect)
                {
                    responseView.UrlRedirect = Url.Action(responseCastRedirect.Action, responseCastRedirect.Controller, new { area = responseCastRedirect.Area, operationNumber = operationNumber });
                }
            }

            return Json(responseView);
        }
        #endregion

        #region ReviewNotifyFC UI-FP-017

        public virtual JsonResult ReviewNotifyFC(string operationNumber)
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<TCReviewNotifyFCViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateTCReviewNotifyFCViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(
                SynchronizationHelper.EDIT_MODE,
                URLConstants.URL_REVIEW_NOTIFY_FC,
                operationNumber,
                userName);

            var responseView = new SaveResponse();
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            ResponseBase response;

            response = _notifyFundingService
                .SaveAndSendTCReviewNotifyFC(operationNumber, viewModel);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if (response.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(
                    URLConstants.URL_NOTIFY_FC, operationNumber, userName);
            }

            if (response is ResponseRedirectBase)
            {
                var responseRedirect = (ResponseRedirectBase)response;
                if (responseRedirect.Redirect)
                {
                    responseView.UrlRedirect = Url.Action(
                        responseRedirect.Action,
                        responseRedirect.Controller,
                        new { area = responseRedirect.Area, operationNumber = operationNumber });
                }
            }

            return Json(responseView);
        }

        #endregion

        #region SingleWindowOperations

        public virtual JsonResult ReturnToSWM(int tcAbstractId)
        {
            ResponseBase response;

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(
                SynchronizationHelper.EDIT_MODE,
                URLConstants.URL_SINGLE_WINDOW_OPERATIONS,
                tcAbstractId.ToString(),
                userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new ResponseBase
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };

                Logger.GetLogger().WriteError(
                    "SingleWindowOperations",
                    "Error when blocking resources during returning to SWM",
                    new Exception(response.ErrorMessage));

                return Json(response);
            }

            response = _singleWindowOperationService
                .ReturnTCAbstractToSingleWindowMeeting(tcAbstractId);

            if (response.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(
                    URLConstants.URL_FUND_INFORMATION, tcAbstractId.ToString(), userName);
            }

            return Json(response);
        }

        #endregion

        #region Fund Information

        public virtual JsonResult FundInformationBasicData(string operationNumber, int fundId)
        {
            ResponseBase response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<FIBasicDataViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateFundInformationBasicInformationViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(
                SynchronizationHelper.EDIT_MODE,
                URLConstants.URL_FUND_INFORMATION,
                fundId.ToString(),
                userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new SaveFundInformationResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _fundInformationService.SaveFundInformationBasicInformation(fundId, viewModel);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(
                        URLConstants.URL_FUND_INFORMATION, fundId.ToString(), userName);
                }
            }
        
            return Json(response);
        }

        public virtual JsonResult FundInformationFundedOperations(string operationNumber, int fundId)
        {
            ResponseBase response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<List<FIRowExtFundWorkforceViewModel>>(jsonDataRequest.SerializedData);

            viewModel.UpdateFundInformationFundedOperationsViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(
                SynchronizationHelper.EDIT_MODE,
                URLConstants.URL_FUND_INFORMATION,
                fundId.ToString(),
                userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new SaveFundInformationResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _fundInformationService.SaveFundInformationFundedOperations(fundId, viewModel);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(
                        URLConstants.URL_FUND_INFORMATION, fundId.ToString(), userName);
                }
            }

            return Json(response);
        }

        public virtual JsonResult FundInformationDocuments(string operationNumber, int fundId)
        {
            ResponseBase response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<List<DocumentViewModel>>(jsonDataRequest.SerializedData);

            viewModel.UpdateFundInformationDocumentsViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(
                SynchronizationHelper.EDIT_MODE,
                URLConstants.URL_FUND_INFORMATION,
                fundId.ToString(),
                userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new SaveFundInformationResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _fundInformationService.SaveFundInformationDocuments(fundId, viewModel);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(
                        URLConstants.URL_FUND_INFORMATION, fundId.ToString(), userName);
                }
            }

            return Json(response);
        }

        public virtual JsonResult DonorContactDetails()
        {
            ResponseBase response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<FIDonorContactDetailViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateDonorContactDetails(jsonDataRequest.ClientFieldData);

            response = _fundInformationService.SaveFundDonorDetails(viewModel);

            return Json(response);
        }

        #endregion

        #region Common Actions

        public virtual JsonResult SendEmailEligibility(string operationNumber, int trustFundId)
        {
            var response = _eligibilityDecisionService
                .SendEmailEligibilityDecision(operationNumber, trustFundId);

            return Json(response);
        }

        public virtual JsonResult RetrySendEmail(string operationNumber)
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);

            var emailStage = jsonDataRequest.ClientFieldData
                .FirstOrDefault(x => x.Name == SUBMIT_EMAIL_STAGE);

            if (emailStage == null || string.IsNullOrEmpty(emailStage.Value))
                return Json(
                    new ResponseBase
                    {
                        ErrorMessage = Localization.GetText(TCGlobalValues.GENERAL_ERROR)
                    });

            var response = _tcAbstractModuleEmailManager
                .SendFPEmail(operationNumber, GetTCEmailStageTypeEnum(emailStage.Value));

            return Json(response);
        }

        #endregion

        #region Private Methods

        TaskDataModel GetTaskData(
            string operationNumber,
            string workflowTaskType,
            int? fundId = null)
        {
            var taskDataModel = K2Helper.RetrieveK2Data(operationNumber, fundId);

            if (!ConfigurationServiceFactory.Current.GetApplicationSettings().IsK2Mode &&
                taskDataModel == null)
            {
                taskDataModel = _k2IntegrationHelper.GetTaskData(
                    operationNumber, workflowTaskType);
            }

            return taskDataModel;
        }

        void CleanK2Data(string operationNumber, int? fundId = null)
        {
            if (!ConfigurationServiceFactory.Current.GetApplicationSettings().IsK2Mode)
                K2Helper.CleanK2Data(operationNumber, fundId);
        }

        TCEmailStageTypeEnum GetTCEmailStageTypeEnum(string emailStage)
        {
            if (TCEmailStageTypeEnum.SubmitTCAbstract.ToString() == emailStage)
                return TCEmailStageTypeEnum.SubmitTCAbstract;

            if (TCEmailStageTypeEnum.ReviewTCAbstractDetermineElibility.ToString() == emailStage)
                return TCEmailStageTypeEnum.ReviewTCAbstractDetermineElibility;

            if (TCEmailStageTypeEnum.ReviewTCabstractCheckElegibility.ToString() == emailStage)
                return TCEmailStageTypeEnum.ReviewTCabstractCheckElegibility;

            if (TCEmailStageTypeEnum.CommentsSWCReceivedTCAbstract.ToString() == emailStage)
                return TCEmailStageTypeEnum.CommentsSWCReceivedTCAbstract;

            if (TCEmailStageTypeEnum.CommentsReceivedToFC.ToString() == emailStage)
                return TCEmailStageTypeEnum.CommentsReceivedToFC;

            if (TCEmailStageTypeEnum.CommentsFCReceivedTCAbstract.ToString() == emailStage)
                return TCEmailStageTypeEnum.CommentsFCReceivedTCAbstract;

            return TCEmailStageTypeEnum.NA;
        }

        bool CheckTCTaskStatus(TCTaskStatusViewModel tcTaskStatus)
        {
            if (tcTaskStatus == null)
                return false;

            if (tcTaskStatus.OperationStatus != null)
                return true;

            if (!string.IsNullOrEmpty(tcTaskStatus.StatusMilestone))
                return true;

            if (tcTaskStatus.EmailNotificationStatus != null)
                return true;

            return false;
        }

        #endregion
    }
}
