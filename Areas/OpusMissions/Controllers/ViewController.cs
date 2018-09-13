using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.Architecture.Logging;
using IDB.MVCControls.General.Helpers;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.Core.ViewModels.Common;
using IDB.MW.Application.MissionsModule.ViewModels;
using IDB.MW.Application.MissionsModule.ViewModels.Workflows;
using IDB.MW.Application.OpusMissionsModule.Services.MissionService;
using IDB.MW.Application.OpusMissionsModule.Services.ReportsService;
using IDB.MW.Application.OPUSModule.K2Integration;
using IDB.MW.Application.OPUSModule.Services.K2DataService;
using IDB.MW.Application.OpusMissionsModule.Enums;
using IDB.MW.Application.MrBlueModule.Services.DisclosureESGDocuments;
using IDB.MW.Application.CaseManager.Business;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.DomainModel.Entities.Core.K2;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.MW.Infrastructure.SecurityService;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.Presentation.MVC4.Areas.OpusMissions.Models;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVC4.Controllers.Helpers.Enumerators;
using IDB.MW.Domain.EntityHelpers;

namespace IDB.Presentation.MVC4.Areas.OpusMissions.Controllers
{
    public partial class ViewController : MVC4.Controllers.ConfluenceController
    {
        #region Fields
        private const string CONVERGENCE_ROLES = "CONVERGENCE ROLES";
        private const string MISSION_STATUS = "MISSION_STATUS";
        private const string UrlMission = "/OpusMissions/Missions";
        private readonly ViewModelMapperHelper _viewModelMapperHelper;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICatalogService _catalogService;
        private readonly IMissionService _missionService;
        private readonly IEnumMappingService _enumMappingService;
        private readonly IMissionReportService _missionReportService;
        private readonly IK2DataService _k2DataService;
        private readonly IK2IntegrationOpus _k2IntegrationOpus;
        private readonly IDisclosureESGDocumentBusinessLogic _disclosureESGDocumentBusinessLogic;
        #endregion

        #region Constructors
        public ViewController(
            ICatalogService catalogService,
            IEnumMappingService enumMappingService,
            IMissionService missionService,
            IMissionReportService missionReportService,
            IK2DataService k2DataService,
            IK2IntegrationOpus k2IntegrationOpus,
            IDisclosureESGDocumentBusinessLogic disclosureESGDocumentBusinessLogic)
        {
            _authorizationService = AuthorizationServiceFactory.Current;
            _missionService = missionService;
            _catalogService = catalogService;
            _missionReportService = missionReportService;
            _enumMappingService = enumMappingService;
            _k2DataService = k2DataService;
            _k2IntegrationOpus = k2IntegrationOpus;
            _viewModelMapperHelper = new ViewModelMapperHelper(ViewBag, _catalogService, _authorizationService, _missionService, missionReportService);
            _disclosureESGDocumentBusinessLogic = disclosureESGDocumentBusinessLogic;
        }
        #endregion

        public virtual ActionResult GetMailTemplate(
            string operationNumber,
            int missionId,
            string documentNumber,
            string urlDocument,
            bool isTOR)
        {
            string fullPath = DownloadDocumentHelper.GetDocumentLink(documentNumber);
            var response = _missionService
                .GetMailData(operationNumber, missionId, documentNumber, fullPath, isTOR);
            return Json(response);
        }

        public virtual JsonResult AddNewDocument(string documentNumber)
        {
            var documentData = _viewModelMapperHelper.GetDocumentData(documentNumber);

            var document = new
            {
                DocumentNumber = documentNumber,
                Date = documentData != null
                    ? FormatHelper.Format(documentData.Created.Value)
                    : FormatHelper.Format(DateTime.Now.Date),
                User = IDBContext.Current.UserName,
                Url = MW.Domain.EntityHelpers.DownloadDocumentHelper.GetDocumentLink(documentNumber)
            };

            var result = Json(new { data = document }, JsonRequestBehavior.AllowGet);

            return result;
        }

        public virtual ActionResult MissionCreateWorkflow(
            string operationNumber,
            int missionId,
            bool? isVpsResend,
            string missionRoles,
            int? missionTypeId)
        {
            var missionModel = _viewModelMapperHelper.GetMissionbyId(missionId);

            var resendVpsFlow = isVpsResend ?? false;
            var groupCode = resendVpsFlow
                ? DivisionOpusGroup.VPS
                : _missionService.DetermineGroupCode(missionModel.Mission);

            var taskTypeCode = _missionService.DetermineWorkflowCode(groupCode);

            var validatorsResponse = _k2DataService.GetValidators(taskTypeCode, null);
            validatorsResponse.Validators = _viewModelMapperHelper
                .FilterMandatory(
                    validatorsResponse.Validators,
                    missionModel.MissionbyId.Header.Type,
                    missionModel.MissionbyId.IsHaitiCountry,
                    taskTypeCode,
                    missionModel.Mission.Operation.OperationData.OperationType.Code);

            Logger.GetLogger().WriteDebug(
                "ViewController",
                "Method: MissionCreateWorkflow : UserName: " + IDBContext.Current.UserName);

            var roles = _viewModelMapperHelper.GetValidatorsListMasterData(CONVERGENCE_ROLES);

            var workflowResponse =
                _missionService.HasWorkflowActive(operationNumber, missionId, taskTypeCode);

            var isGroupCodeVps = groupCode == DivisionOpusGroup.VPS;
            var currentStatus = missionModel.Mission.ConvergenceMasterDataStatus.Code;

            var modelTaskView = new MissionsWorkflowViewModels
            {
                StartProfileTM = resendVpsFlow
                    ? missionRoles
                    : GetListMembersInString(missionModel.MissionbyId),
                missionId = missionModel.MissionbyId.MissionId,
                missionStatusCode = missionModel.MissionbyId.Header.StatusCode,
                missionCode = missionModel.MissionbyId.Header.MissionCode,
                UserComments = null,
                Instructions = Localization.GetText(selectInstruction(taskTypeCode)),
                TaskDataModel = null,
                UserName = IDBContext.Current.UserName,
                Validators = validatorsResponse.Validators,
                Documents = _viewModelMapperHelper.GetWorkFlowMissionDocuments(missionModel.MissionbyId.MissionId),
                Objetive = missionModel.Mission.Objetive,
                IsGroupCodeVps = isGroupCodeVps,
                HasWorkflowActive = workflowResponse.HasCondition,
                IsReturnToViewMission = isGroupCodeVps &&
                    (currentStatus == MissionStatusCode.MISSION_STATUS_APPROVED ||
                     currentStatus == MissionStatusCode.MISSION_STATUS_APPROVED_MOD),
                IsVpsResend = resendVpsFlow
            };

            ViewBag.RetrieveListRoles = _viewModelMapperHelper.RemoveUsedRole(validatorsResponse.Validators, roles);
            ViewBag.TaskTypeCode = taskTypeCode;
            ViewBag.Type = resendVpsFlow
                ? _viewModelMapperHelper.GetMissionTypeName(missionTypeId)
                : missionModel.MissionbyId.Header.Type;
            ViewBag.IsHaiti = missionModel.MissionbyId.IsHaitiCountry.ToString();
            ViewBag.SerializedViewModel = IDB.Presentation.MVC4.Helpers.PageSerializationHelper.SerializeObject(modelTaskView);

            return View("MissionsWorkflow", modelTaskView);
        }

        public virtual ActionResult MissionsWorkflow(string operationNumber, TaskDataModel model)
        {
            string documentReference = string.Empty;
            string host = string.Empty;
            string port = string.Empty;
            string fullPath = string.Empty;

            Logger.GetLogger().WriteDebug("ViewController", "Method: MissionsWorkflow : UserName: " + IDBContext.Current.UserName);
            var mission = _viewModelMapperHelper.GetMissionbyId(Convert.ToInt32(model.Parameters["MissionID"]));
            var missionModel = mission.MissionbyId;
            var validatorsResponse = _k2DataService.GetValidators(model.Code, model.TaskFolio);
            validatorsResponse.Validators = _viewModelMapperHelper
                .FilterMandatory(
                    validatorsResponse.Validators,
                    missionModel.Header.Type,
                    missionModel.IsHaitiCountry,
                    model.Code,
                    mission.Mission.Operation.OperationData.OperationType.Code);
            var commentsResponse = _missionService.GetComments(Convert.ToInt32(model.Parameters["MissionID"]));

            bool isLastApproved = _viewModelMapperHelper.IsLastEmptyState(validatorsResponse.Validators);

            if (isLastApproved)
            {
                documentReference = _viewModelMapperHelper.GetLastDocumentTOR(Convert.ToInt32(model.Parameters["MissionID"]));
                host = "https://" + Request.Url.Host;
                port = Request.Url.Port.ToString();
                fullPath = host + ":" + port + Url.RouteUrl("Document_default", new { action = "DownloadDocument", documentNumber = documentReference });
            }

            var groupCode = _missionService.DetermineGroupCode(mission.Mission);
            var taskTypeCode = _missionService.DetermineWorkflowCode(groupCode);

            var workflowResponse = _missionService.HasWorkflowActive(
                operationNumber, mission.Mission.MissionId, taskTypeCode);

            var modelTaskView = new MissionsWorkflowViewModels
            {
                StartProfileTM = GetListMembersInString(missionModel),
                missionId = missionModel.MissionId,
                missionStatusCode = missionModel.Header.StatusCode,
                missionCode = missionModel.Header.MissionCode,
                UserComments = commentsResponse,
                Instructions = Localization.GetText("OP.MS.K2.ApproveMission.Instructions"),
                TaskDataModel = model,
                UserName = IDBContext.Current.UserName,
                Validators = validatorsResponse.Validators,
                Documents = _viewModelMapperHelper.GetWorkFlowMissionDocuments(Convert.ToInt32(model.Parameters["MissionID"])),
                IsLastApproved = isLastApproved ? "sendLastTor" : string.Empty,
                DocNumber = documentReference,
                UrlDocumentTOR = fullPath,
                IsGroupCodeVps = groupCode == DivisionOpusGroup.VPS,
                HasWorkflowActive = workflowResponse.HasCondition
            };

            if (model.Parameters.ContainsKey("AddValidators"))
                modelTaskView.Validators = _missionService.saveExistingValidators(modelTaskView.Validators, modelTaskView.TaskDataModel.WorkflowInstanceId, Convert.ToString(model.Parameters["AddValidators"]));

            ViewBag.SerializedViewModel = IDB.Presentation.MVC4.Helpers.PageSerializationHelper.SerializeObject(modelTaskView);
            return View(modelTaskView);
        }

        public virtual ActionResult Missions(string operationNumber, MessageSendRequestCode? returnCode)
        {
            ViewBag.Status = _viewModelMapperHelper.GetListMasterData(MISSION_STATUS);
            ViewBag.Type = _viewModelMapperHelper.GetMissionTypeListFilteredNoFase(operationNumber);
            var isACTOperation = _viewModelMapperHelper.IsOperationType(operationNumber, OperationType.ACT);
            ViewBag.visible = "hide";
            if (isACTOperation == true)
            {
                _viewModelMapperHelper.GetCountryAndDepartment();
                ViewBag.visible = "visible";
            }

            ViewBag.MembersTeam = _viewModelMapperHelper.GetMembersMission(operationNumber);
            _viewModelMapperHelper.GetMemberRole();

            var MissionsList = _viewModelMapperHelper.GetListMissionAll(operationNumber);
            ViewBag.TypeSPV = _viewModelMapperHelper.GetTypeSupervisionPlan(operationNumber);
            var model = new MissionsViewModel
            {
                Missions = MissionsList ?? new List<MissionViewModel>(),
                Header = new MissionHeaderViewModel { operationNumber = operationNumber }
            };

            SetMissionPermission(isACTOperation);

            if (returnCode.HasValue)
            {
                int durationSecs = 5;
                var message = MessageHandler.SetMessageSendRequest(returnCode.Value, false, durationSecs, string.Empty);
                ViewData["message"] = message;
            }

            return View(model);
        }

        public virtual ActionResult MissionCompleteView(int missionId, int? ispartial)
        {
            ViewBag.New = false;
            var missionResponse = _viewModelMapperHelper.GetMissionbyId(missionId);

            if (!missionResponse.IsValid)
                throw new Exception(missionResponse.ErrorMessage);

            var model = missionResponse.MissionbyId;
            ViewBag.OrganizationalUnit = model.Header.OrganizationalUnit;
            ViewBagMission(model.operationNumber);
            var isACTOperation = _viewModelMapperHelper.IsOperationType(model.operationNumber, OperationType.ACT);
            SetMissionPermission(isACTOperation);
            string codeStatusMission = missionResponse.Mission.ConvergenceMasterDataStatus.Code;

            if (codeStatusMission == MissionStatusCode.MISSION_STATUS_CANCELLED ||
                codeStatusMission == MissionStatusCode.MISSION_STATUS_POSTPONED)
            {
                model.IsRead = codeStatusMission == MissionStatusCode.MISSION_STATUS_CANCELLED ||
                    MissionPostponedAllowedToRead(codeStatusMission);
            }

            var activityItem = new CMBusiness(model.operationNumber)
                .GetActivityItem(CMConstants.DefaultActivityItems.ESD1);

            model.Header.HasActivityItemESD1 = activityItem != null;

            var groupCode = _missionService.DetermineGroupCode(missionResponse.Mission);
            model.Header.IsGroupCodeVps = groupCode == DivisionOpusGroup.VPS;

            var workflowTypeCode = _missionService.DetermineWorkflowCode(groupCode);

            var workflowActive = _missionService.HasWorkflowActive(
                model.operationNumber, missionId, workflowTypeCode);

            if (workflowActive.IsValid)
            {
                model.Header.HasWorkflowActive = workflowActive.HasCondition;
            }

            if (ispartial == null)
                return View(model);

            return PartialView("Partials/MissionPage", model);
        }

        public virtual ActionResult MissionDocuments(int missionId)
        {
            MissionCompleteViewModel model = new MissionCompleteViewModel();
            ViewBag.New = false;
            var missionResponse = _viewModelMapperHelper.GetMissionbyId(missionId);

            if (!missionResponse.IsValid)
                throw new Exception(missionResponse.ErrorMessage);

            var mission = missionResponse.MissionbyId;
            var isACTOperation = _viewModelMapperHelper.IsOperationType(mission.operationNumber, OperationType.ACT);
            SetMissionPermission(isACTOperation);
            model.DocumentMissionList = _viewModelMapperHelper.GetMissionDocuments(missionId);
            return PartialView("Partials/MissionDocuments", model);
        }

        public virtual ActionResult NewMission(string operationNumber)
        {
            ViewBag.New = true;
            ViewBag.OrganizationalUnit = string.Empty;
            if (string.IsNullOrEmpty(operationNumber))
            {
                operationNumber = IDBContext.Current.Operation;
            }

            var statusDraf = _viewModelMapperHelper.GetStatusDrafMasterData(
                MISSION_STATUS, MissionStatusCode.MISSION_STATUS_DRAFT);

            MissionCompleteViewModel model = new MissionCompleteViewModel()
            {
                Header = new MissionHeaderViewModel()
                {
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now,
                    StatusDraf = statusDraf
                },
                DocumentMissionList = new List<DocumentViewModel>(),
                MissionContactList = new List<MissionContactViewModel>(),
                MissionTeamMemberList = new List<MissionTeamMemberViewModel>(),
                IsBeforeApproved = true
            };

            model.Header.RegionalCountry = _missionService.GetRegionalCountry(
                operationNumber, Localization.CurrentLanguage).RegionalCountry;

            model.Header.operationNumber = operationNumber;
            _missionService.BuildMissionFieldsConfig(model.Header);
            _missionService.BuildMissionOptions(model.Header);

            ViewBagMission(operationNumber);

            model.ExecutionPhase = _viewModelMapperHelper.IsExecutionPhaseOperation(operationNumber);

            model.HasSupervisionPlan = _missionService.HasSupervisionPlan(operationNumber);

            if (model.ExecutionPhase && model.HasSupervisionPlan)
            {
                model.NumberActual = _viewModelMapperHelper.GetFieldCurrentActivity(operationNumber);
                ViewBag.MessageNumberActual = string.Format(
                    Localization.GetText("OP.MS.MissionCompleteView.Message.NumberActual"),
                    model.NumberActual);
            }

            var isACTOperation = _viewModelMapperHelper.IsOperationType(
                operationNumber, OperationType.ACT);

            SetMissionPermission(isACTOperation);

            return View("MissionCompleteView", model);
        }

        public virtual JsonResult GetRoleMember(string OperationTeamDataId, string operationNumber)
        {
            var response = _viewModelMapperHelper.GetTeamRole(OperationTeamDataId, operationNumber);
            return Json(new { IsValid = response.IsValid, ErrorMessage = response.ErrorMessage, Data = response });
        }

        public virtual JsonResult AllowAnalysisMissionCreation(int missionType, string operationNumber)
        {
            bool allowAnalysisMission = _disclosureESGDocumentBusinessLogic
                .AllowAnalysisMissionCreation(missionType, operationNumber);

            return Json(
                new
                {
                    IsValid = allowAnalysisMission,
                    ErrorMessage = Localization.GetText(
                        "ESG.DD.Mission.Message.CantInitWorkflow")
                });
        }

        public virtual JsonResult GetInstitutionName(string filter)
        {
            var response = _viewModelMapperHelper.GetListInstitution(filter);
            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult FilterSearchMissions(string operationNumber, string status, string type, string countryDepartment, string country, string startDateMission, string endDateMission)
        {
            var isACTOperation = _viewModelMapperHelper.IsOperationType(operationNumber, OperationType.ACT);
            SetMissionPermission(isACTOperation);

            var missionsList = _viewModelMapperHelper.GetMissionFilterSearch(operationNumber, status, type, countryDepartment, country, startDateMission, endDateMission);
            return PartialView("Partials/DataListViews/MissionData", missionsList);
        }

        public virtual ActionResult DeleteMission(string operationNumber, string status, string type, string countryDepartment, string country, string startDateMission, string endDateMission, string missionId)
        {
            var missionIdInt = Convert.ToInt32(missionId);
            var isACTOperation = _viewModelMapperHelper.IsOperationType(operationNumber, OperationType.ACT);
            SetMissionPermission(isACTOperation);
            bool result = _viewModelMapperHelper.DeleteMissionCascade(missionIdInt);
            var missionsList = _viewModelMapperHelper.GetMissionFilterSearch(operationNumber, status, type, countryDepartment, country, startDateMission, endDateMission);
            return PartialView("Partials/DataListViews/MissionData", missionsList);
        }

        public virtual ActionResult CancelOrPosponedMission(
            string operationNumber,
            string status,
            string type,
            string countryDepartment,
            string country,
            string startDateMission,
            string endDateMission,
            string missionId,
            string missionStatus,
            string justification)
        {
            int missionIdInt = Convert.ToInt32(missionId);
            var missionModel = _viewModelMapperHelper.GetMissionbyId(missionIdInt);
            var isACTOperation =
                _viewModelMapperHelper.IsOperationType(operationNumber, OperationType.ACT);
            SetMissionPermission(isACTOperation);

            var serviceResponse = _missionService.CancelAndChangeStatus(
                missionModel.Mission, missionStatus, justification);

            if (!serviceResponse.IsValid)
            {
                return Json(new
                {
                    IsValid = false,
                    ErrorMsg = serviceResponse.ErrorMessage
                });
            }

            var missions = _viewModelMapperHelper.GetMissionFilterSearch(
                operationNumber,
                status,
                type,
                countryDepartment,
                country,
                startDateMission,
                endDateMission);

            return Json(new
            {
                IsValid = true,
                Partial = this.RenderRazorViewToString(
                    "Partials/DataListViews/MissionData", missions),
            });
        }

        public virtual JsonResult AccessToResource(string missionId)
        {
            var response = new ResponseBase();
            string userName = IDBContext.Current.UserName;

            var syncErrorMessage = SynchronizationHelper.AccessToResources("edit", UrlMission, missionId, userName);
            if (!string.IsNullOrWhiteSpace(syncErrorMessage))
            {
                response.IsValid = false;
                response.ErrorMessage = syncErrorMessage;
            }
            else
            {
                response.IsValid = true;
            }

            return Json(response);
        }

        public virtual ActionResult MissionCompletion()
        {
            return PartialView("~/Areas/OpusMissions/Views/View/Partials/MissionModalComplete.cshtml");
        }

        public virtual ActionResult AssociateActivitiesMissions(int idMission, int idActivity)
        {
            var respose = _missionService.SaveRelationMissionActivity(idMission, idActivity);
            return Json(respose);
        }

        public virtual ActionResult undoDetailed(int missionId)
        {
            var response = _missionService.UndoDetailed(missionId);
            return Json(response);
        }

        public virtual ActionResult UndoSimplified(string operationNumber, int missionId)
        {
            var response = _missionService.UndoSimplified(operationNumber, missionId);
            return Json(response);
        }

        public virtual ActionResult GetActivities(string operationNumber, int yearActivities)
        {
            var response = _viewModelMapperHelper.GetActivities(operationNumber, yearActivities);
            return Json(response);
        }

        public virtual ActionResult IsSpDetailed(string operationNumber, int year)
        {
            var response = 0;
            bool IsDetailed = _viewModelMapperHelper.IsSpDetailed(operationNumber, year);
            if (IsDetailed)
            {
                var activities = _viewModelMapperHelper.GetActivities(operationNumber, year);
                response = activities.Count;
            }

            return Json(response);
        }

        public virtual JsonResult FreeResource(string missionId)
        {
            var response = new ResponseBase();
            response.IsValid = true;
            string userName = IDBContext.Current.UserName;
            try
            {
                SynchronizationHelper.TryReleaseLock(UrlMission, missionId, userName);
            }
            catch (Exception ex)
            {
                response.IsValid = false;
                response.ErrorMessage = ex.Message;
            }

            return Json(response);
        }

        private static string GetListMembersInString(MissionCompleteViewModel missionModel)
        {
            if (missionModel.MissionTeamMemberList != null)
            {
                return string.Join(",", missionModel.MissionTeamMemberList.Select(x => x.TeamRole).Distinct().ToList());
            }
            else
            {
                return string.Empty;
            }
        }

        private string selectInstruction(string typeCode)
        {
            if (typeCode.Equals("WF-MIS-VPS"))
                return "OP.MS.K2.RequestMissionApproval.VPS.Instructions";
            if (typeCode.Equals("WF-MIS-VPC"))
                return "OP.MS.K2.RequestMissionApproval.VPC.Instructions";

            if (typeCode.Equals("WF-MIS-ESG"))
                return "OP.MS.K2.RequestMissionApproval.ESG.Instructions";

            return string.Empty;
        }

        private void ViewBagMission(string operationNumber)
        {
            ViewBag.Type = _viewModelMapperHelper.GetMissionTypeListFiltered(operationNumber);
            ViewBag.Countries = _missionService.GetCountries(operationNumber, Localization.CurrentLanguage);
            ViewBag.VpsEsg = _viewModelMapperHelper.GetOrganizationalUnitForCode("VPS/ESG");
            var boolOperationType = _viewModelMapperHelper.IsOperationType(operationNumber, OperationType.ACT);
            ViewBag.visible = "hide";
            if (boolOperationType == true)
            {
                _viewModelMapperHelper.GetCountryAndDepartment();
                ViewBag.visible = "visible";
            }

            _viewModelMapperHelper.GetMemberRole();
            ViewBag.MissionTeamMembers = _viewModelMapperHelper.GetMembersMission(operationNumber);
        }

        private void SetMissionPermission(bool isActOperation)
        {
            ViewBag.HasWritePermission = IDBContext.Current.HasPermission("Missions Write") || isActOperation;
        }

        bool MissionPostponedAllowedToRead(string codeStatusMission)
        {
            return codeStatusMission == MissionStatusCode.MISSION_STATUS_POSTPONED &&
                !IDBContext.Current.HasPermission(Permission.MISSION_PERMISION_WRITE);
        }
    }
}