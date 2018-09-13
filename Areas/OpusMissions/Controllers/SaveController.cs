using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.Extensions;
using IDB.Architecture.Language;
using IDB.Architecture.Logging;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.Core.ViewModels.Common;
using IDB.MW.Application.MissionsModule.ViewModels;
using IDB.MW.Application.MissionsModule.ViewModels.Workflows;
using IDB.MW.Application.OpusMissionsModule.Email;
using IDB.MW.Application.OpusMissionsModule.Email.Enumerators;
using IDB.MW.Application.OpusMissionsModule.Enums;
using IDB.MW.Application.OpusMissionsModule.Messages.Mission.MissionService;
using IDB.MW.Application.OpusMissionsModule.Services.MissionService;
using IDB.MW.Application.OpusMissionsModule.Services.ReportsService;
using IDB.MW.Application.OPUSModule.Enums.K2;
using IDB.MW.Application.OPUSModule.K2Integration;
using IDB.MW.Domain.Entities;
using IDB.MW.Domain.Models.Architecture.Enumerations;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.Domain.Values.Mission;
using IDB.MW.Domain.Values.Workflows.General;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.MW.Infrastructure.Configuration;
using IDB.MW.Infrastructure.SecurityService;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.Presentation.MVC4.Areas.OpusMissions.Models;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.OpusMissions.Controllers
{
    public partial class SaveController : MVC4.Controllers.ConfluenceController
    {
        private const string UrlMission = "/OpusMissions/Missions";
        private const string UrlSaveDocuments = "/OpusMissions/SaveDocuments";
        private const string SLASH_CHAR = "/";

        private readonly IMissionService _missionService;
        private readonly IMissionReportService _missionReportService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IEnumMappingService _enumMappingService;
        private readonly IMissionEmailManager _missionEmailManager;
        private IK2IntegrationOpus _k2IntegrationOpus;

        public SaveController(
            IMissionService missionService,
            IEnumMappingService enumMappingService,
            IK2IntegrationOpus k2IntegrationOpus,
            IMissionReportService missionReportService,
            IMissionEmailManager missionEmailManager)
        {
            _authorizationService = AuthorizationServiceFactory.Current;
            _missionService = missionService;
            _missionReportService = missionReportService;
            _enumMappingService = enumMappingService;
            _k2IntegrationOpus = k2IntegrationOpus;
            _missionEmailManager = missionEmailManager;
        }

        public virtual JsonResult SaveDocuments(string operationNumber)
        {
            SaveMissionResponse response;

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<MissionViewModel>(jsonDataRequest.SerializedData);

            if (viewModel == null)
            {
                viewModel = new MissionViewModel();
            }

            viewModel.UpdateMissionDocuments(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserName;
            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, UrlSaveDocuments, viewModel.MissionId.ToString(), userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new SaveMissionResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _missionService.SaveMissionDocuments(viewModel);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(UrlSaveDocuments, operationNumber, userName);
                }
            }

            return Json(response);
        }

        public virtual JsonResult SaveHelpMemoryDocument(string operationNumber, int missionId, string documentNumber, string fileName)
        {
            var documents = new List<DocumentViewModel>
            {
                new DocumentViewModel
                {
                    DocNumber = documentNumber,
                    Date = DateTime.Now,
                    Description = string.Empty,
                    FileName = fileName,
                    User = IDBContext.Current.UserName
                }
            };
            var viewModel = new MissionViewModel
            {
                MissionId = missionId,
                Documents = documents
            };

            var response = _missionService.SaveMissionDocuments(viewModel);

            return Json(response);
        }

        public virtual JsonResult Mission(string operationNumber)
        {
            SaveMissionResponse response = new SaveMissionResponse();
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<MissionViewModel>(jsonDataRequest.SerializedData);

            if (viewModel == null)
                viewModel = new MissionViewModel();

            viewModel.UpdateMissionViewModel(jsonDataRequest.ClientFieldData);

            string userName = IDBContext.Current.UserName;
            string missionId = viewModel.MissionId.ToString();

            response = _missionService.SaveMission(viewModel, operationNumber);

            if (!viewModel.IsNewMission)
                SynchronizationHelper.TryReleaseLock(UrlMission, missionId, userName);

            return Json(response);
        }

        public virtual JsonResult CheckResend()
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = new MissionViewModel();

            viewModel.UpdateMissionViewModel(jsonDataRequest.ClientFieldData);

            var response = _missionService.CheckResendVpsWorkflow(viewModel);

            return Json(response);
        }

        public virtual JsonResult MissionWorkFlow(string operationNumber)
        {
            ResponseBase response = new ResponseBase();
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<MissionsWorkflowViewModels>(jsonDataRequest.SerializedData);

            try
            {
                viewModel.UpdateMissionWorkFlowViewModel(jsonDataRequest.ClientFieldData);

                var workFlow = jsonDataRequest.ClientFieldData.FirstOrDefault(o => o.Name.Equals("changeStatus"));

                var parameters = new Dictionary<string, object>();

                var userName = string.IsNullOrEmpty(viewModel.UserName) ? IDBContext.Current.UserName : viewModel.UserName;
                Logger.GetLogger().WriteDebug("SaveController", "Method: MissionsWorkflow : UserName: " + userName);

                parameters.Add(K2HelperOpus.TaskUserName, userName);

                response = _missionService.SaveMissionWorkFlow(viewModel);

                switch (workFlow.Value)
                {
                    case "StartWorkflow":

                        var workflowCode = jsonDataRequest.ClientFieldData.FirstOrDefault(o => o.Name.Equals("taskTypeCode")).Value;

                        var listAdditionalValidators = viewModel.Validators.Where(a => a.Mandatory == false).Select(x => new { x.Role });
                        var listValidators = new List<string>();

                        foreach (var validator in listAdditionalValidators)
                        {
                            listValidators.Add(validator.Role);
                        }

                        var listAdditionalValidatorsPresent = listAdditionalValidators.Count() == 0 ? false : true;
                        parameters.Add(K2HelperOpus.AdditionalValidatorsBool, listAdditionalValidatorsPresent);

                        parameters.Add(K2HelperOpus.MissionId, Convert.ToString(viewModel.missionId));

                        parameters.Add(K2HelperOpus.MissionCode, viewModel.missionCode);

                        parameters.Add(K2HelperOpus.StartProfileTM, viewModel.StartProfileTM);

                        var arrayTag = new List<string>();

                        arrayTag.Add("\"MissionID\":" + Convert.ToString(viewModel.missionId));

                        arrayTag.Add("\"AddValidators\":" + (listAdditionalValidators.Count() > 0 ? "\"" + string.Join(",", listValidators) + "\"" : "\"\""));

                        parameters.Add(K2HelperOpus.AddValidators, listAdditionalValidators.Count() > 0 ? string.Join(",", listValidators) : "\"\"");

                        parameters.Add(K2HelperOpus.GeneralTag, "{" + string.Join(",", arrayTag) + "}");

                        parameters.Add(K2HelperOpus.Economic, jsonDataRequest.ClientFieldData.FirstOrDefault(o => o.Name.Equals("Type")).Value.Equals("Economic"));

                        parameters.Add(K2HelperOpus.IsHaiti, jsonDataRequest.ClientFieldData.FirstOrDefault(o => o.Name.Equals("IsHaiti")).Value.Equals("True"));

                        var objetiveMission = jsonDataRequest.ClientFieldData.FirstOrDefault(o => o.Name.Equals("MissionObjetive")).Value;
                        var objetiveMissionNew = objetiveMission.Replace("\"", "\\\"");
                        parameters.Add(K2HelperOpus.MissionObjetive, objetiveMissionNew);

                        response.IsValid = _k2IntegrationOpus.StartAdvanceWorkflowOpus(workflowCode, "0", operationNumber, K2HelperOpus.entityTypeOpusMission, null, parameters, K2IntegrationEnumerator.GeneralActions.StartWorkflow, 0);

                        if (response.IsValid)
                        {
                            var nextMissionStatus =
                                GetNextMissionStatusByAction(viewModel.missionId, "StartWorkflow");

                            response = _missionService.ChangeMissionStatus(
                                viewModel.missionId, nextMissionStatus);
                        }

                        if (response.IsValid)
                            response = _missionReportService.GenerationTorMissionReport(viewModel.missionId);

                        break;

                    case "ApproveWorkflow":

                        if (viewModel.IsLastApproved.Equals("sendLastTor"))
                        {
                            parameters.Add(K2HelperOpus.UrlLastDocumentTor, viewModel.UrlDocumentTOR);
                        }

                        response.IsValid = _k2IntegrationOpus.StartAdvanceWorkflowOpus(viewModel.TaskDataModel.Code, viewModel.TaskDataModel.TaskFolio, operationNumber, K2HelperOpus.entityTypeOpusMission, viewModel.TaskDataModel.TaskTypeCode, parameters, K2IntegrationEnumerator.GeneralActions.ApproveWorkflow, viewModel.TaskDataModel.TaskId);

                        if (response.IsValid)
                        {
                            string recipients = Role.TEAM_LEADER + ";" + Role.PROJECT_ASSISTANT;

                            SendEmailToRolesApproveWorkflow(
                                viewModel.TaskDataModel.TaskFolio,
                                viewModel.TaskDataModel.Code,
                                recipients);

                            if (_missionService.IsAdditionalValidator(viewModel.TaskDataModel.UserProfile, viewModel.TaskDataModel.WorkflowInstanceId))
                            {
                                response = _missionService.ChanceAdditionalValidatorStatus(viewModel.TaskDataModel.UserProfile, viewModel.TaskDataModel.WorkflowInstanceId, "Approved");
                            }
                            else if (_missionService.IsLastValidator(viewModel.TaskDataModel.UserProfile, viewModel.TaskDataModel.Code))
                            {
                                var nextMissionStatus = GetNextMissionStatusByAction(
                                    viewModel.missionId, "ApproveWorkflow");

                                response = _missionService.ChangeMissionStatus(
                                    viewModel.missionId, nextMissionStatus);

                                _missionService.UpdateMilestoneDateAndStatus(
                                    operationNumber, viewModel.missionId);
                            }
                        }

                        break;

                    case "RejectWorkflow":
                        string missionComments = string.Empty;
                        int i = 1;
                        foreach (var ObjComment in viewModel.UserComments)
                        {
                            if (i < viewModel.UserComments.Count)
                                missionComments += ObjComment.Comment + "|";
                            else
                                missionComments += ObjComment.Comment;
                            i++;
                        }

                        if (missionComments.Equals(string.Empty))
                        {
                            missionComments = null;
                        }

                        parameters.Add(K2HelperOpus.MissionComment, missionComments);
                        response.IsValid = _k2IntegrationOpus.StartAdvanceWorkflowOpus(viewModel.TaskDataModel.Code, viewModel.TaskDataModel.TaskFolio, operationNumber, K2HelperOpus.entityTypeOpusMission, viewModel.TaskDataModel.TaskTypeCode, parameters, K2IntegrationEnumerator.GeneralActions.RejectWorkflow, viewModel.TaskDataModel.TaskId);

                        if (response.IsValid)
                        {
                            string recipients = Role.TEAM_LEADER + ";" + Role.PROJECT_ASSISTANT;

                            SendEmailToRolesRejectWorkflow(
                                viewModel.TaskDataModel.TaskFolio,
                                viewModel.TaskDataModel.Code,
                                recipients);

                            var nextMissionStatus = GetNextMissionStatusByAction(
                                viewModel.missionId, "RejectWorkflow");

                            response = _missionService.ChangeMissionStatus(
                                viewModel.missionId, nextMissionStatus);
                        }

                        break;
                }
            }
            catch (Exception e)
            {
                response.IsValid = false;
                response.ErrorMessage = e.ToString();
            }

            return Json(response);
        }

        public virtual JsonResult DeleteDocument(string operationNumber, string documentNumber)
        {
            var response = _missionService.DeleteMissionDocument(documentNumber);
            return Json(response);
        }

        public virtual ActionResult ChangeStatusCompletedPending(string operationNumber, int missionId)
        {
            var response = _missionService.ChangeStatusCompletedPending(missionId);
            return Json(response);
        }

        [HttpPost]
        public virtual JsonResult SendEmailToCountryRepresentative(
            int countryId, string operationNumber, int missionId, DateTime endDate, DateTime startDate)
        {
            var missionResponse = _missionService.GetMissionbyId(missionId);

            if (!missionResponse.IsValid)
                return Json(missionResponse);

            var missionUrlEn = BuildUrlKeyEmailMissionAnalysis(operationNumber, missionId, Language.EN);
            var missionUrlEs = BuildUrlKeyEmailMissionAnalysis(operationNumber, missionId, Language.ES);

            Logger.GetLogger().WriteDebug("SaveController mission", string.Format(
                "SendEmailToCountryRepresentative - MissionUrlEn is: {0} MissionUrlEs is: {1}",
                missionUrlEn,
                missionUrlEs));

            if (string.IsNullOrEmpty(missionUrlEn) && string.IsNullOrEmpty(missionUrlEs))
                return Json(new ResponseBase { IsValid = false, ErrorMessage = "No url found" });

            return Json(_missionEmailManager
                .SendEmail(
                    MissionEmailSendType.MissionAnalysisCreation,
                    countryId,
                    new Dictionary<string, string>
                    {
                        { EmailCodes.OperationKey, operationNumber },
                        { EmailCodes.MISSION_NUMBER_KEY, missionResponse.Mission.MissionCode },
                        { EmailCodes.MISSION_END_DATE, endDate.ToString("dd-MM-yyyy") },
                        { EmailCodes.MISSION_START_DATE, startDate.ToString("dd-MM-yyyy") },
                        { EmailCodes.URL_EN, missionUrlEn },
                        { EmailCodes.URL_ES, missionUrlEs }
                    }));
        }

        private string BuildUrlKeyEmailMissionAnalysis(
            string operationNumber, int missionId, string language)
        {
            var urlBasePath = ConfigurationServiceFactory.Current
                .GetApplicationSettings().BasePath;

            urlBasePath = urlBasePath.EndsWith(SLASH_CHAR) ? urlBasePath : urlBasePath + SLASH_CHAR;

            if (string.IsNullOrEmpty(urlBasePath))
                return null;

            var missionUrl = EmailCodes.MISSION_URL;

            missionUrl = missionUrl.Replace(
                EmailCodes.WILDCARD_OPERATION_NUMBER, operationNumber);

            missionUrl = missionUrl.Replace(
                EmailCodes.WILDCARD_MISSION_ID, Convert.ToString(missionId));

            switch (language)
            {
                case Language.EN:
                    return "<a href=\"" + urlBasePath + missionUrl + "\" >" +
                        EmailCodes.HERE_EN + "</a>";
                case Language.ES:
                    return "<a href=\"" + urlBasePath + missionUrl + "\" >" +
                        EmailCodes.HERE_ES + "</a>";
                default:
                    return "<a href=\"" + urlBasePath + missionUrl + "\" >" +
                        EmailCodes.HERE_EN + "</a>";
            }
        }

        private void SendEmailToRolesApproveWorkflow(
            string folioId, string workflowType, string recipients)
        {
            bool sendEmailByApprovalMission = false;
            bool.TryParse(
                ConfigurationManager.AppSettings.Get("IsEnabledSendEmailMissionNoK2"),
                out sendEmailByApprovalMission);

            if (!sendEmailByApprovalMission)
            {
                return;
            }

            string currentTemplateCode = GetNotificationCodeApprove(workflowType);

            _missionEmailManager.SendEmail(
                    MissionEmailSendType.MissionApproveWorkflow,
                    0,
                    parameters: new Dictionary<string, string>()
                                {
                                      { WorkflowValues.FOLIO_ID, folioId },
                                      { MissionGlobalValues.TEMPLATE_CODE, currentTemplateCode },
                                      { MissionGlobalValues.ROLE_LIST, recipients }
                                });
        }

        private string GetNotificationCodeApprove(string workflowType)
        {
            if (workflowType == WorkflowCodes.WF_MIS_001.GetStringValue())
            {
                return EmailCodes.WFMISVPSN002;
            }

            if (workflowType == WorkflowCodes.WF_MIS_002.GetStringValue())
            {
                return EmailCodes.WFMISVPCN002;
            }

            if (workflowType == WorkflowCodes.WF_MIS_003.GetStringValue())
            {
                return EmailCodes.WFMISESGN002;
            }

            return EmailCodes.WFMISEWPN005;
        }

        private void SendEmailToRolesRejectWorkflow(
            string folioId, string workflowType, string recipients)
        {
            bool sendEmailByApprovalMission = false;
            bool.TryParse(
                ConfigurationManager.AppSettings.Get("IsEnabledSendEmailMissionNoK2"),
                out sendEmailByApprovalMission);

            if (!sendEmailByApprovalMission)
            {
                return;
            }

            string currentTemplateCode = GetNotificationCodeReject(workflowType);

            _missionEmailManager.SendEmail(
                    MissionEmailSendType.MissionRejectWorkflow,
                    0,
                    parameters: new Dictionary<string, string>()
                                {
                                      { WorkflowValues.FOLIO_ID, folioId },
                                      { MissionGlobalValues.TEMPLATE_CODE, currentTemplateCode },
                                      { MissionGlobalValues.ROLE_LIST, recipients }
                                });
        }

        private string GetNotificationCodeReject(string workflowType)
        {
            if (workflowType == WorkflowCodes.WF_MIS_001.GetStringValue())
            {
                return EmailCodes.WFMISVPSN001;
            }

            if (workflowType == WorkflowCodes.WF_MIS_002.GetStringValue())
            {
                return EmailCodes.WFMISVPCN001;
            }

            if (workflowType == WorkflowCodes.WF_MIS_003.GetStringValue())
            {
                return EmailCodes.WFMISESGN001;
            }

            return EmailCodes.WFMISEWPN003;
        }

        private string GetNextMissionStatusByAction(int missionId, string action)
        {
            var missionResponse = _missionService.GetMissionbyId(missionId);

            var groupCode = _missionService.DetermineGroupCode(missionResponse.Mission);

            var currentMissionStatus = missionResponse.Mission.ConvergenceMasterDataStatus.Code;

            var nextMissionStatus = currentMissionStatus;

            switch (action)
            {
                case "StartWorkflow":

                    nextMissionStatus = (groupCode == DivisionOpusGroup.VPS &&
                        currentMissionStatus != MissionStatusCode.MISSION_STATUS_DRAFT)
                        ? MissionStatusCode.MISSION_STATUS_PEND_APPR_MODIFICATION
                        : MissionStatusCode.MISSION_STATUS_PENDING_APP;

                    break;

                case "ApproveWorkflow":

                    nextMissionStatus = (groupCode == DivisionOpusGroup.VPS &&
                        currentMissionStatus ==
                            MissionStatusCode.MISSION_STATUS_PEND_APPR_MODIFICATION)
                        ? MissionStatusCode.MISSION_STATUS_APPROVED_MOD
                        : MissionStatusCode.MISSION_STATUS_APPROVED;

                    break;

                case "RejectWorkflow":

                    nextMissionStatus = (groupCode == DivisionOpusGroup.VPS &&
                        currentMissionStatus ==
                            MissionStatusCode.MISSION_STATUS_PEND_APPR_MODIFICATION)
                        ? MissionStatusCode.MISSION_STATUS_REJECTED_MODIFICATION
                        : MissionStatusCode.MISSION_STATUS_DRAFT;

                    break;

                default:
                    break;
            }

            return nextMissionStatus;
        }
    }
}