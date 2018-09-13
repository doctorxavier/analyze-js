using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

using IDB.Architecture;
using IDB.Architecture.Extensions;
using IDB.Architecture.Language;
using IDB.MW.API365;
using IDB.MW.API365.Messages;
using IDB.MW.API365.Models;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.MrBlueModule.Services.DisclosureESGDocuments;
using IDB.MW.Application.VERModule.Messages;
using IDB.MW.Application.VERModule.Messages.Request;
using IDB.MW.Application.VERModule.Services;
using IDB.MW.Application.VERModule.Services.Documents;
using IDB.MW.Application.VERModule.Services.GenericService;
using IDB.MW.Application.VERModule.Services.Permissions;
using IDB.MW.Application.VERModule.Services.PolicyWaiver;
using IDB.MW.Application.VERModule.Services.Steps;
using IDB.MW.Application.VERModule.Services.Tasks;
using IDB.MW.Application.VERModule.Services.VersionHistory;
using IDB.MW.Application.VERModule.ViewModels;
using IDB.MW.Application.ConvergenceValidationsModule.ViewModels;
using IDB.MW.Application.VERModule.Services.VerifyContent;
using IDB.MW.Business.DocumentManagement.Contracts;
using IDB.MW.Business.DocumentManagement.Enums;
using IDB.MW.Business.DocumentManagement.Messages;
using IDB.MW.Business.VERModule.Contracts;
using IDB.MW.Business.VERModule.Messages.Request;
using IDB.MW.Business.VMRModule.DTOs;
using IDB.MW.Domain.EntityHelpers;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.Domain.Values.Ver;
using IDB.MW.Domain.Values.Vmr;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.Presentation.MVC4.Models.ClientFieldData;
using IDB.Presentation.MVC4.Areas.VER.Models;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Application.VERModule.Messages.Response;
using IDB.MW.Application.VMRModule.Services.GenericService;
using IDB.Architecture.Security;
using IDB.MW.Application.Core.Mappers;
using IDB.MW.Application.Core.Messages;
using IDB.Architecture.Security.Messages.UserIdentity;

namespace IDB.Presentation.MVC4.Areas.VER.Controllers
{
    public partial class VirtualEditingRoomController : BaseController
    {
        #region Fields
        private readonly ViewModelMapperHelper _viewModelMapperHelper;
        private readonly IVerService _verService;
        private readonly ICatalogService _catalogService;
        private readonly IVerGenericService _verGenericService;
        private readonly IVmrGenericService _vmrGenericService;
        private readonly IVerStepsService _verStepsService;
        private readonly IVersionHistoryService _versionHistoryService;
        private readonly IVerTasksService _verTasksService;
        private readonly IVerDocumentService _verDocumentService;
        private readonly IVerPermissionsService _verPermissionsService;
        private readonly IDocumentManagementService _documentManagementService;
        private readonly IConcurrenceRepository _concurrenceRepository;
        private readonly IAdUsersRepository _adUsersRepository;
        private readonly IDisclosureESGDocumentBusinessLogic _disclosureESGDocumentBusinessLogic;
        private readonly IVerVerifyContentService _verVerifyContentService;
        private readonly IPolicyWaiverService _policyWaiverService;
        private readonly IStatusPolicyWaiverService _statusPolicyWaiverService;
        private readonly IDocumentRepository _documentRepository;

        #endregion

        #region Constructors

        public VirtualEditingRoomController(
            IVerService verService,
            ICatalogService catalogService,
            IVerGenericService verGenericService,
            IVmrGenericService vmrGenericService,
            IVerStepsService verStepsService,
            IVersionHistoryService versionHistoryService,
            IVerTasksService verTasksService,
            IVerDocumentService verDocumentService,
            IVerPermissionsService verPermissionsService,
            IDocumentManagementService documentManagementService,
            IConcurrenceRepository concurrenceRepository,
            IAdUsersRepository adUsersRepository,
            IDisclosureESGDocumentBusinessLogic disclosureESGDocumentBusinessLogic,
            IVerVerifyContentService verVerifyContentService,
            IPolicyWaiverService policyWaiverService,
            IStatusPolicyWaiverService statusPolicyWaiverService,
            IDocumentRepository documentRepository)
        {
            _catalogService = catalogService;
            _verService = verService;
            _verGenericService = verGenericService;
            _vmrGenericService = vmrGenericService;
            _verStepsService = verStepsService;
            _versionHistoryService = versionHistoryService;
            _verTasksService = verTasksService;
            _verDocumentService = verDocumentService;
            _verPermissionsService = verPermissionsService;
            _concurrenceRepository = concurrenceRepository;
            _adUsersRepository = adUsersRepository;
            _policyWaiverService = policyWaiverService;
            _viewModelMapperHelper = new ViewModelMapperHelper(
                ViewBag,
                _verService,
                _catalogService,
                _verTasksService,
                _verPermissionsService,
                _concurrenceRepository,
                _adUsersRepository);
            _documentManagementService = documentManagementService;
            _disclosureESGDocumentBusinessLogic = disclosureESGDocumentBusinessLogic;
            _verVerifyContentService = verVerifyContentService;
            _statusPolicyWaiverService = statusPolicyWaiverService;
            _documentRepository = documentRepository;
        }

        #endregion

        #region ViewSection

        public virtual ActionResult Test(string operationNumber)
        {
            ViewBag.InitialActionList = _catalogService.GetListMasterData("VER_ACTION_TYPE", true).Where(
                                                                    o => o.Text.StartsWith("INIT"));
            ViewBag.InitialActionId = _catalogService.GetConvergenceMasterDataIdByCode(
                                    VerActionTypeCode.INITATE_TC_PACK_PREP, "VER_ACTION_TYPE").Id;
            return View();
        }

        public virtual ActionResult Index(int? instanceId)
        {
            var model = _viewModelMapperHelper.GetVerGeneral(instanceId.GetValueOrDefault());
            return View(model);
        }

        public virtual ActionResult BasicDataContent(int instanceId)
        {
            var basicData = _verService.GetBasicData(instanceId, null);
            _viewModelMapperHelper.EditableScreen(true);
            _viewModelMapperHelper.SecurityBasicData(basicData.Security);
            return PartialView("Partials/BasicData", basicData.BasicData);
        }

        public virtual ActionResult VerContentReload(int instanceId)
        {
            var model = _viewModelMapperHelper.GetVerGeneral(instanceId);

            return PartialView("Partials/VerContent", model);
        }

        public virtual ActionResult VersionHistoryContent(int instanceId)
        {
            var versionHistoryRequest = new VersionHistoryRequest
            {
                InstanceId = instanceId
            };
            var model = _versionHistoryService.GetVersionHistory(versionHistoryRequest).VersionHistory;
            _viewModelMapperHelper.GetHistoryList();
            _viewModelMapperHelper.EditableScreen(true);
            ViewBag.SerializedDocumentViewModel = PageSerializationHelper.SerializeObject(model);
            return PartialView("Partials/Tabs/VersionHistoryPartial", model);
        }

        public virtual ActionResult ParticipantContent(int instanceId)
        {
            var requestGetParticipant = new GetParticipantRequest
            {
                InstanceId = instanceId
            };

            var participantData = _verPermissionsService.GetParticipant(requestGetParticipant);
            var model = participantData.Participant;
            var security = participantData.Security;
            _viewModelMapperHelper.GetParticipantList(false);
            _viewModelMapperHelper.EditableScreen(true);
            _viewModelMapperHelper.SecurityParticipant(security);
            ViewBag.SerializedParticipantViewModel = PageSerializationHelper.SerializeObject(model);
            return PartialView("Partials/Tabs/ParticipantsPartial", model);
        }

        public virtual ActionResult TaskContent(int instanceId)
        {
            var taskRequest = new VerTaskRequest
            {
                InstanceId = instanceId
            };

            var taskData = _verTasksService.GetTask(taskRequest);
            var model = taskData.Task;
            var security = taskData.Security;
            _viewModelMapperHelper.GetTaskList(instanceId);
            _viewModelMapperHelper.EditableScreen(true);
            _viewModelMapperHelper.SecurityTask(security);
            ViewBag.SerializedTaskViewModel = PageSerializationHelper.SerializeObject(model);
            return PartialView("Partials/Tabs/TasksPartial", model);
        }

        public virtual ActionResult DocumentContent(int instanceId)
        {
            var documentRequest = new VerDocumentTabRequest
            {
                InstanceId = instanceId
            };
            var documentData = _verDocumentService.GetDocumentTab(documentRequest);
            var model = documentData.Document;
            var security = documentData.Security;

            ViewBag.SerializedDocumentViewModel = PageSerializationHelper.SerializeObject(model);

            _viewModelMapperHelper.GetDocumentList();
            _viewModelMapperHelper.EditableScreen(true);
            _viewModelMapperHelper.SecurityDocument(security);
            return PartialView("Partials/Tabs/DocumentsPartial", model);
        }

        public virtual ActionResult PolicyWaiverContent(int instanceId, bool isEdit = false)
        {
            var policyRequest = new PolicyWaiverRequest
            {
                InstanceId = instanceId,
                IsEditMode = isEdit
            };

            var policyResponse = _policyWaiverService.GetPolicyWaiverTab(policyRequest);
            var model = policyResponse.PolicyWaiver;

            _viewModelMapperHelper.GetPagination();
            _viewModelMapperHelper.EditableScreen(true);
            ViewBag.SerializedPolicyWaiverViewModel = PageSerializationHelper.SerializeObject(model);

            return PartialView("Partials/Tabs/PolicyWaiverPartial", model);
        }

        #endregion

        #region  GetSection
        public virtual ActionResult GetPolicyWaiverRow()
        {
            var model = GetPolicyWaiverRowModel();
            return PartialView("Partials/DataTables/TemplateRows/PolicyWaivedRow", model);
        }

        public virtual JsonResult FilterPolicyStatus(string option)
        {
            var request = new SetStatusRequest
            {
                StatusCode = option,
                OpcCode = option
            };

            return Json(_statusPolicyWaiverService.SetStatusPolicyWaiver(request));
        }

        public virtual JsonResult UploadNewDocument(
            int instanceId, string documentNumber, string documentName, string docWebUrl, string documentNameTemp)
        {
            return Json(GetDocumentData(instanceId, documentNumber, documentName, docWebUrl, documentNameTemp));
        }

        public virtual ActionResult AddNewDocument(
            int instanceId, string documentNumber, string documentName, string docWebUrl, string docNameTemp, bool isGeneratedAnnex)
        {
            var model = GetNewDocumentData(instanceId, documentNumber, documentName, docWebUrl, docNameTemp, isGeneratedAnnex);

            _viewModelMapperHelper.GetDocumentList();
            return PartialView("Partials/DataTables/TemplateRows/DocumentsNewRow", model);
        }

        public virtual ActionResult GetDataVersionHistory(int packageVersion, int instanceId)
        {
            var model = new VersionHistoryDataViewModel();

            var dataPackageRequest = new DataVersionHistoryRequest
            {
                InstanceId = instanceId,
                PackageVersion = packageVersion
            };

            var data = _versionHistoryService.GetDataPackageVersion(dataPackageRequest);

            if (data.IsValid)
            {
                model = data.VersionHistoryData;
            }

            return PartialView("Partials/Tabs/Modals/ModalVersionHistoryPartial", model);
        }

        public virtual JsonResult SubmitInstance(int instanceId, int actionType, ClientFieldData[] additionalData)
        {
            var additionalFields = additionalData.SetAdditionalFieldForm();
            var submitResponse = new VerSubmitInstanceRequest
            {
                InstanceId = instanceId,
                ActionType = actionType,
                AdditionalData = additionalFields,
                Mode = SubmitType.MANUAL
            };

            var response = _verService.SubmitInstance(submitResponse);
            return Json(response);
        }

        public virtual JsonResult DeleteFolderSharepoint(string operationNumber, string folderName)
        {
            var request = new DeleteFolderRequest
            {
                Connection = new DataConnection(null, IDBContext.Current.UserName, operationNumber),
                NameFolder = folderName,
                DeleteAllFolder = true
            };
            var apiShp = new ApiOperations();
            var response = apiShp.DeleteFolder(request);
            return Json(response);
        }

        public virtual ActionResult GetMailTemplate(int instanceId, List<int> externalUserId, int taskId)
        {
            var request = new GetMailDataRequest
            {
                InstanceId = instanceId,
                ExternalUserId = externalUserId,
                TaskId = taskId
            };

            var response = _verService.GetMailData(request);

            return Json(response);
        }

        public virtual ActionResult GetTasksNewRow(int instanceId)
        {
            _viewModelMapperHelper.GetTaskList(instanceId);
            return PartialView("Partials/DataTables/TemplateRows/TaskNewRow");
        }

        public virtual JsonResult GetUsersList(string filter)
        {
            UsersByNameOrFullNameResponse resp = new UsersByNameOrFullNameResponse();
            var response = UserIdentityManager.SearchUsersByFullNameOrName(new GetUsersRequest { FullName = filter });
            if (response != null && response.Users != null)
            {
                resp = response.Users.UserIdentityModelToResponse();
            }

            return new JsonResult { Data = resp, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual JsonResult AllowSubmitForClearence(string operationNumber, string submitCode)
        {
            var response = _disclosureESGDocumentBusinessLogic
                .AllowSubmitForClearence(operationNumber, submitCode);

            return Json(response);
        }

        public virtual ActionResult GetParticipantsNewRow(
            string searchBy,
            int participantTypeId,
            int roleId,
            string userName,
            int organizationalUnitId,
            string email)
        {
            var modelParticipant = new VerParticipantRowViewModel();

            switch (searchBy)
            {
                case ModuleAccessLevelCode.ROLE:
                    modelParticipant.ParticipantTypeId = participantTypeId;
                    modelParticipant.Role = roleId;
                    modelParticipant.UserName = string.IsNullOrEmpty(userName)
                        ? _verPermissionsService
                            .GetParticipantName(roleId, organizationalUnitId)
                            .Participant.ParticipantsUserName
                        : userName;
                    modelParticipant.FullName = string.IsNullOrEmpty(userName)
                        ? _verPermissionsService.GetParticipantName(roleId, organizationalUnitId)
                            .Participant.ParticipantsFullName
                        : userName;
                    modelParticipant.OrganizationalUnitId = organizationalUnitId == -1
                        ? (int?)null
                        : organizationalUnitId;
                    modelParticipant.AccessLevelId = _catalogService
                        .GetConvergenceMasterDataIdByCode(
                            ModuleAccessLevelCode.ROLE, MasterType.MODULE_ACCESS_LEVEL).Id;
                    modelParticipant.AccessLevelCode = searchBy;
                    modelParticipant.Email = string.IsNullOrEmpty(email) ? null : email;
                    break;
                case ModuleAccessLevelCode.USER:
                    modelParticipant.ParticipantTypeId = participantTypeId;
                    modelParticipant.Role = roleId == -1 ? (int?)null : roleId;
                    modelParticipant.OrganizationalUnitId = organizationalUnitId == -1
                        ? (int?)null
                        : organizationalUnitId;
                    modelParticipant.UserName = userName;
                    modelParticipant.FullName = UserIdentityManager
                        .SearchFullNameByUserName(new GetUsersRequest { UserName = userName })
                        .FullName;
                    modelParticipant.AccessLevelId = _catalogService
                        .GetConvergenceMasterDataIdByCode(
                            ModuleAccessLevelCode.USER, MasterType.MODULE_ACCESS_LEVEL).Id;
                    modelParticipant.AccessLevelCode = searchBy;
                    modelParticipant.Email = string.IsNullOrEmpty(email) ? null : email;
                    break;
            }

            _viewModelMapperHelper.GetParticipantList(true);
            return PartialView("Partials/DataTables/TemplateRows/ParticipantsNewRow", modelParticipant);
        }

        //TODO: TEST METHODS
        public virtual JsonResult VerInstance(
            string operationNumber, int template, int initialAction)
        {
            var initialStatus =
               _catalogService.GetConvergenceMasterDataIdByCode(
                                    VerInstanceStatusCode.PREPARATION, "VER_INSTANCE_STATUS").Id;

            var initialVersion =
               _catalogService.GetConvergenceMasterDataIdByCode("PRE_QRR", "VER_PACKAGE_VERSION").Id;

            var generateInstanceRequest = new VerGenerateInstanceRequest
            {
                OperationNumber = operationNumber,
                TemplateId = template,
                InitialActionId = initialAction,
                InitialStatusId = initialStatus,
                InitialVersionId = initialVersion,
                MilestoneCodeList = new List<string> { "TCPP" }
            };

            var response = _verService.GenerateInstance(generateInstanceRequest);
            return Json(response);
        }

        public virtual JsonResult AutomaticSubmitTest()
        {
            _verStepsService.VerAutomaticSubmit();

            return Json(true);
        }

        public virtual ActionResult GetParticipantsTaks(string roleAndOrgUnitId, int instanceId)
        {
            var model = new List<VerUserViewModel>();

            if (roleAndOrgUnitId != string.Empty)
            {
                int? roleId = Convert.ToInt32(roleAndOrgUnitId.Split('$')[0]);
                int? orgUnitId = Convert.ToInt32(roleAndOrgUnitId.Split('$')[1]);

                model = _verTasksService
                    .GetNamePartipantListTask(
                    instanceId,
                        roleId == 0 ? null : roleId,
                        orgUnitId == 0 ? null : orgUnitId)
                    .ListUsers;
            }

            return PartialView("Partials/DataTables/TemplateRows/TaskParticipantList", model);
        }

        public virtual JsonResult VerLockUnlock(int instanceId, bool lockInstance)
        {
            bool isValid = true;
            string errorMessage = string.Empty;
            bool isEqualLock = false;

            try
            {
                var instanceRequest = new VerInstanceRequest
                {
                    InstanceId = instanceId
                };

                var instanceResponse = _verGenericService.GetInstance(instanceRequest);

                if (instanceResponse.IsValid == false)
                {
                    throw new Exception(instanceResponse.ErrorMessage);
                }

                if (lockInstance == instanceResponse.Instance.IsLocked)
                {
                    isEqualLock = true;
                    throw new Exception(Localization.GetText("VER.GLOBAL.MESSAGE.EQUAL.LOCK"));
                }

                var lockInstanceResponse = _verStepsService.VerEnableInstance(
                    new VerEnableInstanceRequest
                    {
                        Instance = instanceResponse.Instance,
                        EnableInstance = !lockInstance,
                        EnableEzShare = !lockInstance
                    });

                if (lockInstanceResponse.IsValid == false)
                {
                    throw new Exception(lockInstanceResponse.ErrorMessage);
                }
            }
            catch (Exception e)
            {
                isValid = false;
                errorMessage = e.Message;
            }

            return Json(new { IsValid = isValid, ErrorMessage = errorMessage, IsEqualLock = isEqualLock });
        }

        public virtual ActionResult VerGetButtons(int instanceId)
        {
            _viewModelMapperHelper.EditableScreen(true);

            var loadSecurityRequest = new VerLoadSecurityRequest
            {
                InstanceId = instanceId,
                Pages = string.Concat(
                    VerSecurityValues.SECURITY_VER_DOCUMENT_PAGE,
                    "|",
                    VerSecurityValues.SECURITY_VER_POLICY_WAIVER)
            };

            var model = _verService.GetButtonsSubmit(instanceId);

            var security = _verGenericService.LoadSecurity(loadSecurityRequest).SecurityList;

            ViewBag.Security = security.ToList();

            return PartialView("Partials/DataTables/Buttons/VerAllButtonsPartial", model.ListSubmit);
        }

        public virtual FileResult VerDownloadZipDocument(int instanceId, string documents)
        {
            if (string.IsNullOrEmpty(documents) == false)
            {
                var list = documents.Split('|').ToList();
                var requestInstance = new VerInstanceRequest { InstanceId = instanceId };
                var instanceReponse = _verGenericService.GetInstance(requestInstance);

                if (instanceReponse != null && instanceReponse.Instance != null &&
                    instanceReponse.IsValid)
                {
                    var requestFolder = new PackageFolderRequest { Instance = instanceReponse.Instance };
                    var folderResponse = _verGenericService.GetFolderName(requestFolder);

                    if (folderResponse != null && folderResponse.Folder != null &&
                        folderResponse.IsValid)
                    {
                        var request = new DownloadZipDocumentRequest
                        {
                            BussinesAreaCode = BusinessAreaCodeEnum.BA_VER.Value,
                            OperationNumber = instanceReponse.Instance.Operation.OperationNumber,
                            FileNameZip = instanceReponse.Instance.VerTemplate.PackageType
                            .GetNameLanguage(Localization.CurrentLanguage),
                            DocumentList = list
                        };

                        var response = _documentManagementService.DownloadZipDocument(request);

                        return response != null && response.IsValid
                            ? File(
                                response.Document.File,
                                System.Net.Mime.MediaTypeNames.Application.Octet,
                                response.Document.FileName)
                            : null;
                    }
                }
            }

            return null;
        }

        #endregion

        #region SaveSection

        public virtual JsonResult SendTask(int instanceId, List<int> tasksId)
        {
            var request = new SendTaskRequest
            {
                InstanceId = instanceId,
                TasksId = tasksId
            };

            return Json(_verTasksService.SendTask(request));
        }

        public virtual JsonResult SaveParticipant(string operationNumber)
        {
            SaveParticipantResponse response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<VerParticipantViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateParticipantViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(
                            "edit", VerGlobalValues.URL_PARTICIPANT, operationNumber, userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new SaveParticipantResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _verService.SaveParticipant(viewModel);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(VerGlobalValues.URL_PARTICIPANT, operationNumber, userName);
                }
            }

            return Json(response);
        }

        public virtual JsonResult SaveDocument(string operationNumber)
        {
            ResponseBase response;

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<VerDocumentViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateDocumentViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(
                                "edit", VerGlobalValues.URL_DOCUMENT, operationNumber, userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new ResponseBase
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                var documentRequest = new SaveDocumentTabRequest
                {
                    DocumentViewModel = viewModel
                };
                response = _verDocumentService.SaveDocumentTab(documentRequest);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(
                                        VerGlobalValues.URL_DOCUMENT, operationNumber, userName);
                }
            }

            return Json(response);
        }

        public virtual JsonResult CreateParticipantTask(
            int instanceId,
            TaskTableRowViewModel taskViewModel)
        {
            var saveRowRequest = new SaveRowParticipantTaskRequest
            {
                InstanceId = instanceId,
                Task = taskViewModel
            };

            var response = _verTasksService.SaveRowParticipantTask(saveRowRequest);
            return Json(response);
        }

        public virtual JsonResult GetOrgUnitByCode(string orgUnit)
        {
            var response = _catalogService
                        .GetConvergenceMasterDataIdByCode(orgUnit, MasterType.ORGANIZATION_UNIT);

            return Json(response);
        }

        public virtual JsonResult ValidationParticipant(
        string accessLevelCode,
        int roleId,
        int orgUnitId,
        int participantTypeId,
        string userName,
        List<VerParticipantRowViewModel> participants)
        {
            var model = new VerValidationParticipantRequest
            {
                AccessLevelCode = accessLevelCode,
                RoleId = roleId,
                OrgUnitId = orgUnitId,
                ParticipantTypeId = participantTypeId,
                UserName = userName,
                Participants = participants
            };

            var response = _verPermissionsService.ValidationParticipant(model);
            return Json(response);
        }

        public virtual JsonResult SaveTask(string operationNumber)
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<TaskTableViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateTaskViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(
                                    "edit", VerGlobalValues.URL_TASK, operationNumber, userName);

            ResponseBase response;

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new ResponseBase
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                var saveRequest = new SaveTaskTabRequest
                {
                    Task = viewModel
                };
                response = _verTasksService.SaveTaskTab(saveRequest);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(VerGlobalValues.URL_TASK, operationNumber, userName);
                }
            }

            return Json(response);
        }

        public virtual JsonResult SavePolicyWaiver(string instanceId)
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<PolicyWaiverViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdatePolicyWaiverViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(
                                    "edit", VerGlobalValues.URL_POLICY_WAIVER, instanceId, userName);

            ResponseBase response;

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new ResponseBase
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                var saveRequest = new SavePolicyWaiverRequest
                {
                    PolicyWaiverViewModel = viewModel
                };
                response = _policyWaiverService.SavePolicyWaiver(saveRequest);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(VerGlobalValues.URL_POLICY_WAIVER, instanceId, userName);
                }
            }

            return Json(response);
        }

        public virtual JsonResult SaveClearanceDocument(
            int instanceId,
            bool isOpcRequired)
        {
            var request = new SaveDocumentsForClearanceRequest
            {
                InstanceId = instanceId,
                IsOpcRequired = isOpcRequired
            };

            var response = _verDocumentService.SaveOpcClearanceDocuments(request);

            return Json(response);
        }

        public virtual JsonResult RemoveDocumentSharepointTemp(
            int instanceId,
            string operationNumber,
            string documentNumber,
            string documentName,
            string documentNameTemp)
        {
            string folder = Globals.GetSetting("Temporary_Documents_VER");

            string docName;
            if (string.IsNullOrEmpty(documentNameTemp))
            {
                var docEntitie = _verDocumentService.GetDocumentByReference(documentNumber);
                docName = SanitizeFileName(docEntitie.VerDocumentEntitie.DocumentName) + Path.GetExtension(docEntitie.VerDocumentEntitie.Document.Description);
            }
            else
            {
                docName = documentNameTemp;
            }

            var request = new DeleteDocumentApi365Request
            {
                Connection = new DataConnection(folder, IDBContext.Current.UserName, operationNumber),
                DocumentId = documentNumber,
                DocumentName = docName
            };

            var apiShp = new ApiOperations();
            var responseApi = apiShp.DeleteDocument(request);

            return Json(responseApi);
        }

        #endregion

        #region MethodsSection

        public virtual JsonResult GenerateAnnex(int instanceId, List<int> rowIdList)
        {
            var request = new GetDocumentsForGenerateRequest
            {
                InstanceId = instanceId,
                RowIdList = rowIdList
            };

            return Json(_verDocumentService.GetDocumentsForGenerate(request));
        }

        public virtual JsonResult GenerateAnnexRefresh(int instanceId)
        {
            var request = new GetDocumentsForRefreshRequest
            {
                InstanceId = instanceId
            };

            return Json(_verDocumentService.GetDocumentsForRefresh(request));
        }

        public virtual ActionResult GetParticipantSearch()
        {
            _viewModelMapperHelper.GetParticipantList(true);
            return PartialView("_ParticpantSearch");
        }

        public virtual FileResult VerDownloadDocument(string documentId, string opDownNumber)
        {
            var opNumber = IDBContext.Current.Operation ?? opDownNumber;
            var user = ConfigurationManager.AppSettings["UserDownload_API365"];

            DownloadDocumentApi365Request request = new DownloadDocumentApi365Request
            {
                Connection = new DataConnection(null, user, opNumber),
                DocumentId = documentId
            };
            var apiShp = new ApiOperations();
            var response = apiShp.DownloadDocumentNew(request);

            return !response.IsValid ? null : File(
                    response.File, System.Net.Mime.MediaTypeNames.Application.Octet, response.FileName);
        }

        public virtual ActionResult VerVerifyContent(int instanceId)
        {
            var model = new VerifyContentViewModel
            {
                VerifyContent = new List<VerifyContentRowViewModel>()
            };

            var verifyContentRequest = new VerifyContentRequest
            {
                InstanceId = instanceId
            };

            var data = _verVerifyContentService.VerifyContent(verifyContentRequest);

            if (data.IsValid == false)
            {
                model = data.VerifyContentModel;
            }

            return PartialView("Controls/ConvergenceValidation/ModalVerifyContentTable", model);
        }

        public virtual JsonResult ValidateSubmit(int instanceId, string actionCode)
        {
            return Json(_verVerifyContentService.VerifyContent(new VerifyContentRequest
            {
                InstanceId = instanceId,
                SelectedActionCode = actionCode
            }));
        }

        public virtual JsonResult AllowSubmit(int instanceId, IList<VerDocumentRowViewModel> documents)
        {
            return Json(_verService.AllowSubmit(instanceId, documents));
        }

        public virtual ActionResult GetReviewedLoanModal(string operationNumber, int instanceId)
        {
            ReviewedLoanViewModel model = new ReviewedLoanViewModel();
            var loanOpsRequest = new OperationsReviewLoanRequest
            {
                OperationNumber = operationNumber,
                InstanceId = instanceId
            };

            var loanOpsResponse = _verService.GetOperationsForReviewInLoan(loanOpsRequest);

            if (loanOpsResponse.IsValid)
            {
                model = loanOpsResponse.ReviewedLoan;
            }

            return PartialView("Partials/Tabs/Modals/ModelReviewedLoan", model);
        }

        public virtual JsonResult SaveIsReviewedInLoan(
            int instanceId, bool isReviewedInLoan, string loanOperation)
        {
            var saveLoanRequest = new SaveReviewedLoanRequest
            {
                InstanceId = instanceId,
                IsReviewedInLoan = isReviewedInLoan,
                LoanOperation = loanOperation
            };

            var saveLoanResponse = _verService.SaveReviewedLoan(saveLoanRequest);

            return Json(saveLoanResponse);
        }

        public virtual JsonResult IsLockConcurrence(string operationNumber)
        {
            var lstTabsConcurrence = new List<string>()
            {
                { VerGlobalValues.URL_TASK },
                { VerGlobalValues.URL_DOCUMENT },
                { VerGlobalValues.URL_PARTICIPANT },
                { VerGlobalValues.URL_POLICY_WAIVER }
            };

            var response = _viewModelMapperHelper.IsLockConcurrence(
                lstTabsConcurrence, operationNumber, IDBContext.Current.UserLoginName);

            return Json(response);
        }

        public virtual JsonResult GeneratePdf(int instanceId, bool isOpcCheked)
        {
            var request = new GeneratePdfDocumentRequest
            {
                InstanceId = instanceId,
                IsOpcCheked = isOpcCheked
            };

            var response = _verDocumentService.GeneratePdfDocument(request);

            return new JsonResult()
            {
                Data = response,
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        #endregion

        #region PrivateMethods

        private VerDocumentRowViewModel GetDocumentData(
            int instanceId, string documentNumber, string documentName, string docWebUrl, string documentNameTemp)
        {
            var model = new VerDocumentRowViewModel();

            var instanceRequest = new VerInstanceRequest
            {
                InstanceId = instanceId
            };

            var instanceResponse = _verGenericService.GetInstance(instanceRequest);

            if (instanceResponse.IsValid)
            {
                model = new VerDocumentRowViewModel
                {
                    SLno = 0,
                    DocumentType = _catalogService.GetConvergenceMasterDataIdByCode(
                    VerDocumentTypeCode.OTHER, MasterType.VER_DOCUMENT_TYPE).Id,
                    DocumentName = SanitizeFileName(documentName),
                    PackageVersion = instanceResponse.Instance.Version.GetNameLanguage(Localization.CurrentLanguage),
                    LastUpdate = DateTime.Now,
                    UserName = UserIdentityManager
                    .SearchFullNameByUserName(new GetUsersRequest { UserName = IDBContext.Current.UserName })
                    .FullName,
                    IsPrimary = false,
                    IsRequired = false,
                    DocumentNumber = documentNumber,
                    ShpUrlDocument = docWebUrl,
                    IsVer = true,
                    IsVisibleChkPublish = false,
                    IsNewDocument = true,
                    DocumentNameTemp = documentNameTemp
                };
            }

            return model;
        }

        private VerDocumentRowViewModel GetNewDocumentData(
            int instanceId, string documentNumber, string documentName, string docWebUrl, string docNameTemp, bool isGenerated)
        {
            var model = new VerDocumentRowViewModel();
            var instanceRequest = new VerInstanceRequest
            {
                InstanceId = instanceId
            };

            var instanceResponse = _verGenericService.GetInstance(instanceRequest);

            if (instanceResponse.IsValid)
            {
                var loadSecurityRequest = new VerLoadSecurityRequest
                {
                    Instance = instanceResponse.Instance,
                    Pages = VerSecurityValues.SECURITY_VER_DOCUMENT_PAGE
                };

                var security = _verGenericService.LoadSecurity(loadSecurityRequest);

                _viewModelMapperHelper.SecurityDocument(security.SecurityList.ToList());

                model = new VerDocumentRowViewModel
                {
                    SLno = 0,
                    DocumentType = _catalogService.GetConvergenceMasterDataIdByCode(
                        VerDocumentTypeCode.OTHER, MasterType.VER_DOCUMENT_TYPE).Id,
                    DocumentName = documentName,
                    PackageVersion = instanceResponse.Instance.Version.GetNameLanguage(Localization.CurrentLanguage),
                    LastUpdate = DateTime.Now,
                    UserName = UserIdentityManager
                    .SearchFullNameByUserName(new GetUsersRequest { UserName = IDBContext.Current.UserName })
                    .FullName,
                    IsPrimary = false,
                    IsRequired = false,
                    DocumentNumber = documentNumber,
                    ShpUrlDocument = docWebUrl,
                    IsVer = true,
                    IncInPublish = false,
                    IsVisibleChkPublish = false,
                    IsVersionHistory = false,
                    IsGenerated = isGenerated,
                    IsNewDocument = true,
                    DocumentNameTemp = docNameTemp
                };
            }

            return model;
        }

        private string SanitizeFileName(string documentName)
        {
            string charsToRemove = new string(Path.GetInvalidFileNameChars()) +
                                   new string(Path.GetInvalidPathChars());
            string pattern = string.Format("[{0}]", Regex.Escape(charsToRemove));
            return Regex.Replace(documentName, pattern, "-");
        }

        private PolicyWaiverRowViewModel GetPolicyWaiverRowModel()
        {
            var categoryMasterData = _catalogService.GetMasterDataListByTypeCode(
                true, MasterType.POLICY_WAIVER_CATEGORY);

            var statusMasterDataId = _catalogService.GetConvergenceMasterDataIdByCode(
                PolicyWaiverStatusCode.UNSENT, MasterType.POLICY_WAIVER_STATUS);

            var statusMasterData = _catalogService.GetMasterDataListByTypeCode(
                true, MasterType.POLICY_WAIVER_STATUS);

            var categoryList = categoryMasterData.IsValid && categoryMasterData.MasterDataCollection.HasAny()
                ? categoryMasterData.MasterDataCollection.Select(
                    o =>
                        new Tuple<string, string>(o.MasterId.ToString(),
                            o.GetLocalizedName(Localization.CurrentLanguage))).ToList()
                : new List<Tuple<string, string>>();

            var statusList = statusMasterData.IsValid && statusMasterData.MasterDataCollection.HasAny()
               ? statusMasterData.MasterDataCollection.Select(
                   o =>
                       new Tuple<string, string>(o.MasterId.ToString(),
                           o.GetLocalizedName(Localization.CurrentLanguage))).ToList()
               : new List<Tuple<string, string>>();

            var model = new PolicyWaiverRowViewModel
            {
                PolicyWaiverCategory = new FieldModel
                {
                    FieldName = VerSecurityValues.POLICY_WAIVER_CATEGORY,
                    DropDownSource = categoryList
                },
                PolicyWaiver = new FieldModel
                {
                    FieldName = VerSecurityValues.POLICY_WAIVER,
                    Placeholder = Localization.GetText("POLICY.WAIVER.PLACEHOLDER")
                },
                PolicyWaiverJustification = new FieldModel
                {
                    FieldName = VerSecurityValues.POLICY_WAIVER_JUSTIFICATION,
                    Placeholder = Localization.GetText("POLICY.WAIVER.JUSTIFICATION.PLACEHOLDER")
                },
                PolicyWaiverStatus = new FieldModel
                {
                    FieldName = VerSecurityValues.POLICY_WAIVER_STATUS,
                    IntValue = statusMasterData.IsValid ? statusMasterDataId.Id : 0,
                    DropDownSource = statusList
                }
            };

            return model;
        }
        #endregion
    }
}