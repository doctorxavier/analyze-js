using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using IDB.Architecture;
using IDB.Architecture.Language;
using IDB.MW.Application.Components;
using IDB.MW.Application.VMRModule.ViewModels;
using IDB.Presentation.MVC4.Controllers;
using IDB.MW.Application.VMRModule.Services;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Areas.VMR.Models;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.VERModule.Handlers;
using IDB.MW.Application.VERModule.ViewModels;
using IDB.MW.Application.VMRModule.Messages.Request;
using IDB.MW.Application.VMRModule.Services.GenericService;
using IDB.MW.API365;
using IDB.MW.API365.Messages;
using IDB.MW.API365.Models;
using IDB.MW.Business.VMRModule.Contracts;
using IDB.MW.Business.VMRModule.DTOs.AdditionalFieldsModels;
using IDB.MW.Business.VMRModule.Messages.Request;
using IDB.MW.Domain.Values;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Domain.EntityHelpers;
using IDB.MW.Domain.Values.Vmr;
using IDB.Presentation.MVC4.Models.ClientFieldData;
using IDB.MW.Application.VERModule.Messages;
using IDB.MW.Application.VMRModule.Services.Permissions;
using IDB.MW.Application.VMRModule.Services.Steps;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.MW.Application.VMRModule.Services.Remarks;
using IDB.MW.Application.VERModule.Services.Documents;
using IDB.MW.Application.VERModule.Messages.Request;
using IDB.MW.Application.VMRModule.Services.Comment;
using IDB.MW.Application.VERModule.Services.GenericService;
using IDB.MW.Business.DocumentManagement.Contracts;
using IDB.MW.Business.DocumentManagement.Enums;
using IDB.MW.Business.DocumentManagement.Messages;
using IDB.Presentation.MVC4.Areas.VER.Models;
using ClientFieldDataMapper = IDB.Presentation.MVC4.Areas.VMR.Models.ClientFieldDataMapper;
using ViewModelMapperHelper = IDB.Presentation.MVC4.Areas.VMR.Models.ViewModelMapperHelper;
using IDB.Architecture.Security;
using IDB.MW.Application.Core.Mappers;
using IDB.MW.Application.Core.Messages;
using IDB.Architecture.Security.Messages.UserIdentity;

namespace IDB.Presentation.MVC4.Areas.VMR.Controllers
{
    public partial class MeetingRoomController : BaseController
    {
        #region Fields
        private readonly ViewModelMapperHelper _viewModelMapperHelper;
        private readonly IVmrService _vmrService;
        private readonly ICatalogService _catalogService;
        private readonly IVmrGenericService _vmrGenericService;
        private readonly IAdditionalFieldsService _additionalFieldsService;
        private readonly IVmrStepsService _vmrStepsServices;
        private readonly IVmrRemarksService _vmrRemarksService;
        private readonly IVmrPermissionsService _vmrPermissionsService;
        private readonly IVerDocumentService _verDocumentService;
        private readonly IVerGenericService _verGenericService;
        private readonly IDocumentManagementService _documentManagementService;
        private readonly IVmrNotificationsService _vmrNotificationsService;

        #endregion

        #region Constructor

        public MeetingRoomController(
            ICatalogService catalogService,
            IVmrService vmrService,
            IVmrGenericService vmrGenericService,
            IAdditionalFieldsService additionalFieldsService,
            IVmrStepsService vmrStepsServices,
            IVmrRemarksService vmrRemarksService,
            IVmrPermissionsService vmrPermissionsService,
            IVerDocumentService verDocumentService,
            ICommentService commentService,
            IVerGenericService verGenericService,
            IDocumentManagementService documentManagementService,
            ICommentRulesService commentRulesService,
            IVmrNotificationsService vmrNotificationsService)
        {
            _catalogService = catalogService;
            _vmrService = vmrService;
            _vmrGenericService = vmrGenericService;
            _additionalFieldsService = additionalFieldsService;
            _vmrStepsServices = vmrStepsServices;
            _vmrRemarksService = vmrRemarksService;
            _vmrPermissionsService = vmrPermissionsService;
            _verGenericService = verGenericService;
            _documentManagementService = documentManagementService;
            _viewModelMapperHelper = new ViewModelMapperHelper(
                ViewBag,
                _vmrService,
                _catalogService,
                _vmrGenericService,
                commentService,
                commentRulesService);
            _verDocumentService = verDocumentService;
            _vmrNotificationsService = vmrNotificationsService;
        }
        #endregion

        #region ViewSection
        public virtual ActionResult Index(int? instanceId, string derivedBy)
        {
            var model = _viewModelMapperHelper.GetVmrGeneral(instanceId.GetValueOrDefault(), derivedBy);

            return View(model);
        }

        public virtual ActionResult VmrContentReload(int instanceId, string derivedBy)
        {
            var model = _viewModelMapperHelper.GetVmrGeneral(instanceId, derivedBy);

            return PartialView("Partials/VmrContent", model);
        }

        public virtual ActionResult AccessVmr(string key)
        {
            var verifySecurity = _vmrGenericService.VerifyAccess(key);

            return !verifySecurity.IsValid
                ? RedirectToAction("Index", "MyMeetings")
                : RedirectToAction("Index", new
                {
                    instanceId = verifySecurity.InstanceId,
                    derivedBy = verifySecurity.DerivedBy
                });
        }

        public virtual ActionResult Test(string operationNumber)
        {
            ViewBag.MeetingType = _catalogService.GetListMasterData("VMR_MEETING_TYPE");
            var database = new OptimaDatabase();
            var instancesOp = database.VmrInstance
                .Where(o => o.Operation.OperationNumber == operationNumber && o.IsOpen)
                .Select(x => new SelectListItem
                {
                    Value = x.VmrInstanceId.ToString(),
                    Text = x.VmrTemplate.MeetingType.Code + " instanceid = " + x.VmrInstanceId
                }).ToList();
            ViewBag.InstancesList = instancesOp;
            return View();
        }

        public virtual JsonResult AutomaticSubmitTest()
        {
            _vmrStepsServices.VmrAutomaticSubmit();
            return Json(true);
        }

        public virtual JsonResult SyncVmrParticipantsWithAd()
        {
            var response = new ResponseBase
            {
                IsValid = true
            };

            var request = new SyncVmrParticipantsWithAdRequest
            {
                OperationNumber = IDBContext.Current.Operation
            };

            _vmrStepsServices.SyncVmrParticipantsWithAd(request);

            return Json(response);
        }

        public virtual JsonResult MinuteBatch()
        {
            SgHandlers.ValidateMinute();
            return Json(true);
        }

        public virtual JsonResult SetExecutionDate(int instanceId, DateTime executionDate)
        {
            var response = new ResponseBase
            {
                IsValid = true
            };

            var instance = _vmrGenericService.GetInstance(new VmrInstanceRequest
            {
                InstanceId = instanceId
            });

            if (instance.IsValid)
            {
                var currentAction = _vmrGenericService
                    .GetActualProcessFlow(instance.Instance.VmrProcessFlow);

                var saveAdditionalField = new SaveAdditionalFieldsRequest
                {
                    VmrInstance = instance.Instance,
                    CurrentActionType = currentAction.VmrProcessFlow.VmrActionTypeId,
                    IsSave = true,
                    Fields = new List<AdditionalFieldsSaveModel>
                    {
                        new AdditionalFieldsSaveModel
                        {
                           Name = VmrGlobalValues.END_DATE_PROCESS_FLOW,
                           Value = executionDate.ToShortDateString(),
                           Type = VmrGlobalValues.DATETIME_FIELD
                        }
                    }
                };

                _additionalFieldsService.SaveAdditionalFields(saveAdditionalField);

                response = _vmrStepsServices.SetAutomaticSubmit(new VmrSetAutomaticSubmitRequest
                {
                    Instance = instance.Instance,
                    ExecutionTime = executionDate
                });
            }

            return Json(response);
        }

        public virtual ActionResult FilterComment(int instanceId, FilterCommentViewModel filter = null)
        {
            var response = _viewModelMapperHelper.FilterComment(instanceId, filter);
            ViewBag.CountRows = response.CountRows;

            return PartialView("Partials/CommentPartial/CommentList", response.CommentList);
        }

        public virtual PartialViewResult GetCommentTree(int commentId, int instanceId, int level, bool isOpcAgreement, bool isExpand = false)
        {
            var viewmodelComment = _viewModelMapperHelper.GetCommentTree(commentId, instanceId, level, isExpand, isOpcAgreement);
            return PartialView("Partials/CommentPartial/Comment", viewmodelComment);
        }

        public virtual JsonResult GenerateEncriptedKey(string inputValue)
        {
            var response = _vmrNotificationsService.GetEncriptedKey(inputValue);

            return Json(response);
        }

        #endregion

        #region Get

        public virtual FileResult SingleWindowMeetingExportMinuteToWord(int instanceId)
        {
            var response = _vmrService.GenerateMinute(instanceId);
            if (!response.IsValid)
                return null;
            return File(response.File, "application/docx", response.FileName + ".docx");
        }

        public virtual FileResult VmrDownloadDocument(string documentId)
        {
            var user = ConfigurationManager.AppSettings["UserDownload_API365"];

            DownloadDocumentApi365Request request = new DownloadDocumentApi365Request
            {
                Connection = new DataConnection(null, user, IDBContext.Current.Operation),
                DocumentId = documentId
            };

            var apiShp = new ApiOperations();
            var response = apiShp.DownloadDocumentNew(request);

            return !response.IsValid
                ? null
                : File(response.File, System.Net.Mime.MediaTypeNames.Application.Octet, response.FileName);
        }

        public virtual FileResult VmrDownloadZipDocument(int instanceId, string documents)
        {
            if (string.IsNullOrEmpty(documents) == false)
            {
                var list = documents.Split('|').ToList();
                var requestInstance = new VmrInstanceRequest { InstanceId = instanceId };
                var instanceReponse = _vmrGenericService.GetInstance(requestInstance);

                if (instanceReponse.IsValid &&
                    instanceReponse.Instance.VerInstance != null &&
                    instanceReponse.Instance.VerInstance.VerTemplate.PackageType != null)
                {
                    var requestFolder = new PackageFolderRequest { Instance = instanceReponse.Instance.VerInstance };
                    var folderResponse = _verGenericService.GetFolderName(requestFolder);

                    if (folderResponse.IsValid)
                    {
                        var request = new DownloadZipDocumentRequest
                        {
                            BussinesAreaCode = BusinessAreaCodeEnum.BA_VER.Value,
                            OperationNumber = instanceReponse.Instance.Operation.OperationNumber,
                            FileNameZip = instanceReponse.Instance.VerInstance.VerTemplate.PackageType
                            .GetNameLanguage(Localization.CurrentLanguage),
                            DocumentList = list
                        };

                        var response = _documentManagementService.DownloadZipDocument(request);

                        return response.IsValid
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

        public virtual ActionResult BasicDataPartial(int instanceId)
        {
            var basicDataRequest = new BasicDataRequest
            {
                InstanceId = instanceId
            };
            var basicData = _vmrService.GetBasicData(basicDataRequest);
            var model = basicData.BasicData;
            var security = basicData.Security;
            _viewModelMapperHelper.SecurityBasicData(security);
            _viewModelMapperHelper.IsEditableScreen(false);
            ViewBag.SerializedBasicDataViewModel = PageSerializationHelper.SerializeObject(model);

            return PartialView("Partials/BasicData", model);
        }

        public virtual ActionResult ParticipantContent(int instanceId)
        {
            var participantData = _vmrService.GetParticipants(instanceId, null);
            var model = participantData.Participant;
            var security = participantData.Security;
            _viewModelMapperHelper.IsEditableScreen(true);
            _viewModelMapperHelper.GetParticipantList();
            _viewModelMapperHelper.RoleList();
            _viewModelMapperHelper.SecurityParticipant(security);
            ViewBag.SerializedParticipantViewModel = PageSerializationHelper.SerializeObject(model);
            return PartialView("Partials/Tabs/TabParticipants", model);
        }

        public virtual ActionResult RemarksContent(int instanceId)
        {
            var remarksRequest = new CommentsRequest
            {
                InstanceId = instanceId
            };
            _viewModelMapperHelper.GetPagination();
            var remarksData = _vmrRemarksService.GetVmrRemarks(remarksRequest);
            var model = remarksData.VmrCommentTab;

            return PartialView("Partials/Tabs/TabRemarks", model);
        }

        public virtual ActionResult DocumentContent(int instanceId)
        {
            var documentData = _vmrService.GetDocuments(instanceId, null);
            var model = documentData.Document;
            var security = documentData.Security;

            var basicDataRequest = new BasicDataRequest
            {
                InstanceId = instanceId
            };

            var modelBasicData = _vmrService.GetBasicData(basicDataRequest).BasicData;

            ViewBag.FolderShp = modelBasicData.FolderShp;
            ViewBag.OperationNumber = modelBasicData.OperationNumber;
            ViewBag.Site = modelBasicData.SiteShp;

            _viewModelMapperHelper.SecurityDocument(security);
            _viewModelMapperHelper.IsEditableScreen(true);
            _viewModelMapperHelper.GetDocumentList();
            ViewBag.SerializedDocumentViewModel = PageSerializationHelper.SerializeObject(model);

            return PartialView("Partials/Tabs/TabDocuments", model);
        }

        public virtual ActionResult GetParticipantsNewRow(
            string searchBy,
            int participantTypeId,
            int roleId,
            string userName,
            int organizationalUnitId)
        {
            var modelParticipant = new VmrParticipantRowViewModel();
            var roleAccess = _catalogService
                .GetConvergenceMasterDataIdByCode(
                    ModuleAccessLevelCode.ROLE,
                    MasterType.MODULE_ACCESS_LEVEL);

            var userAccess = _catalogService
                .GetConvergenceMasterDataIdByCode(
                    ModuleAccessLevelCode.USER,
                    MasterType.MODULE_ACCESS_LEVEL);

            var permissionService =
                _vmrPermissionsService
                    .GetParticipantName(
                        roleId,
                        organizationalUnitId);

            var userIdentityService = UserIdentityManager
                .SearchFullNameByUserName(new GetUsersRequest { UserName = userName });

            var getParticipantName =
                _vmrPermissionsService
                    .GetParticipantName(
                        roleId,
                        organizationalUnitId);

            switch (searchBy)
            {
                case ModuleAccessLevelCode.ROLE:
                    modelParticipant.ParticipantTypeId = participantTypeId;
                    modelParticipant.Role = roleId;
                    modelParticipant.Username = getParticipantName.IsValid
                        ? getParticipantName.UserName
                        : string.Empty;
                    modelParticipant.OrganizationalUnitId = organizationalUnitId;
                    modelParticipant.AccessLevelId = roleAccess.IsValid ? roleAccess.Id : 0;
                    modelParticipant.FullName = permissionService.IsValid
                        ? permissionService.FullName
                        : string.Empty;
                    modelParticipant.AccessLevelCode = searchBy;
                    break;
                case ModuleAccessLevelCode.USER:
                    modelParticipant.ParticipantTypeId = participantTypeId;
                    modelParticipant.Role = roleId == -1 ? (int?)null : roleId;
                    modelParticipant.OrganizationalUnitId = organizationalUnitId == -1
                        ? (int?)null
                        : organizationalUnitId;
                    modelParticipant.Username = userName;
                    modelParticipant.AccessLevelId = userAccess.IsValid ? userAccess.Id : 0;
                    modelParticipant.FullName = userIdentityService.IsValid
                        ? userIdentityService
                        .FullName
                        : string.Empty;
                    modelParticipant.AccessLevelCode = searchBy;
                    break;
            }

            _viewModelMapperHelper.GetParticipantList();
            return PartialView("Partials/Tabs/TemplateRows/ParticipantsNewRow", modelParticipant);
        }

        public virtual ActionResult AddNewDocument(
            int instanceId, string documentNumber, string documentName, string docWebUrl, string docNameTemp)
        {
            var model = GetNewDocumentData(instanceId, documentNumber, documentName, docWebUrl, docNameTemp);

            _viewModelMapperHelper.GetDocumentList();
            return PartialView("Partials/Tabs/TemplateRows/DocumentsNewRow", model);
        }

        public virtual JsonResult UploadNewDocument(
            int instanceId, string documentNumber, string documentName, string docWebUrl, string documentNameTemp)
        {
            return Json(GetDocumentData(instanceId, documentNumber, documentName, docWebUrl, documentNameTemp));
        }

        public virtual JsonResult GetMeetingDays(DateTime startDate, DateTime endDate)
        {
            var request = new AvailableDaysRequest
            {
                StartDate = startDate,
                EndDate = endDate
            };
            return Json(_additionalFieldsService.GetAvailableDays(request));
        }

        public virtual ActionResult GetParticipantSearch()
        {
            _viewModelMapperHelper.GetParticipantList();
            return PartialView("_ParticpantSearch");
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

        public virtual JsonResult ValidationParticipant(
            string accessLevelCode,
            int roleId,
            int orgUnitId,
            int participantTypeId,
            string userName,
            List<VmrParticipantRowViewModel> participants)
        {
            var model = new VmrValidationParticipantRequest
            {
                AccessLevelCode = accessLevelCode,
                RoleId = roleId,
                OrgUnitId = orgUnitId,
                ParticipantTypeId = participantTypeId,
                UserName = userName,
                Participants = participants
            };

            var response = _vmrPermissionsService.ValidationParticipant(model);
            return Json(response);
        }

        public virtual JsonResult GetOrganizationUnitListByRol(int roleId)
        {
            var orgUnitRequest = new OrgUnitByRoleRequest
            {
                RoleId = roleId
            };
            var response = _vmrService.GetOrganizationUnitListByRole(orgUnitRequest);
            return Json(new { selectedId = response.Id, Data = response.OrganizationUnitList });
        }

        public virtual JsonResult GetDataUserName(string userName)
        {
            var response = _vmrService.GetDataByUserName(userName);
            return Json(new { Data = response.RolesUser });
        }
        #endregion

        #region Save
        public virtual JsonResult SaveDocuments(string operationNumber)
        {
            ResponseBase response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<VerDocumentViewModel>(jsonDataRequest.SerializedData);
            ClientFieldDataMapper.UpdateDocumentViewModel(viewModel, jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(
                                "edit", VmrGlobalValues.URL_DOCUMENT, operationNumber, userName);

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
                response = _vmrService.SaveDocument(viewModel);
                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(
                        VmrGlobalValues.URL_DOCUMENT, operationNumber, userName);
                }
            }

            return Json(response);
        }

        public virtual JsonResult SaveParticipant(string operationNumber)
        {
            SaveParticipantResponse response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<VmrParticipantViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateParticipantViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(
                                    "edit", VmrGlobalValues.URL_PARTICIPANT, operationNumber, userName);

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
                response = _vmrService.SaveParticipant(viewModel);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(
                                    VmrGlobalValues.URL_PARTICIPANT, operationNumber, userName);
                }
            }

            return Json(response);
        }

        public virtual JsonResult SubmitInstance(
            int instanceId, int actionType, ClientFieldData[] additionalData)
        {
            var additionalFields = ClientFieldDataMapper.SetAdditionalFieldForm(additionalData);
            var requestSubmit = new VmrSubmitRequest
            {
                InstanceId = instanceId,
                ActionType = actionType,
                Mode = SubmitType.MANUAL,
                AdditionalData = additionalFields
            };
            var response = _vmrGenericService.SubmitInstance(requestSubmit);
            return Json(response);
        }

        public virtual JsonResult SaveBasicData(int instanceId, ClientFieldData[] additionalData)
        {
            var additionalFields = ClientFieldDataMapper.SetAdditionalFieldForm(additionalData);
            var request = new SaveBasicDataRequest
            {
                InstanceId = instanceId,
                AdditionalData = additionalFields
            };

            var response = _vmrService.SaveBasicData(request);
            return Json(response);
        }

        #endregion

        public virtual JsonResult AllowSubmit(
            int instanceId, IList<VmrDocumentRowViewModel> documents, int actionType)
        {
            return Json(_vmrGenericService.AllowSubmit(instanceId, documents, actionType));
        }

        public virtual JsonResult CheckMakePublicMinute(int instanceId)
        {
            var request = new ValidateMinuteRequest
            {
                InstanceId = instanceId
            };

            var response = _viewModelMapperHelper.MakePublicAnswer(request);

            return Json(response);
        }

        public virtual ActionResult UploadFile(
            HttpPostedFileBase file,
            string module,
            string operationNumber,
            string folder,
            string nameDocument,
            bool newDocumentForSave,
            string documentList)
        {
            var oldName = Path.GetFileNameWithoutExtension(file.FileName);

            var documentToList = documentList.Split(',').ToList();
            if (string.IsNullOrEmpty(nameDocument) && documentToList.Contains(oldName))
            {
                return new JsonResult
                {
                    Data = new ResponseApi365
                    {
                        IsValid = false,
                        ErrorMessage = Localization.GetText("VER.Homepage.TabDocuments.AlreadyExist")
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            var formatDate = string.Format("{0:yyyyMMddHHmmff}", DateTime.Now);
            var prefix = "_" + formatDate;

            var nameDoc = new StringBuilder();
            nameDoc.Append(string.IsNullOrEmpty(nameDocument)
                ? oldName
                : SanitizeFileName(nameDocument));
            nameDoc.Append(prefix);
            nameDoc.Append(Path.GetExtension(file.FileName));

            var stream = new MemoryStream();
            file.InputStream.CopyTo(stream);

            var request = new UploadDocumentApi365Request
            {
                Connection = new DataConnection(string.IsNullOrEmpty(folder) ? Globals.GetSetting("Temporary_Documents_VER") : folder,
                    IDBContext.Current.UserName,
                    operationNumber,
                    string.IsNullOrEmpty(module) ? null : module),
                Name = nameDoc.ToString(),
                DocumentFile = stream.ToArray(),
                OptionVersion = OptionVersion.NoVersion
            };
            var apiShp = new ApiOperations();
            var response = string.IsNullOrEmpty(module) ?
                apiShp.AddDocumentTempVer(request) :
                apiShp.AddDocument(request);

            response.DocumentNameTemp = nameDoc.ToString();
            response.DocumentName = string.IsNullOrEmpty(nameDocument) ? oldName : nameDocument;

            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        #region Coment Tab Code

        public virtual ActionResult SaveComment(VmrSaveCommentViewModel model)
        {
            var response = _viewModelMapperHelper.SaveComment(model);

            return PartialView("Partials/CommentPartial/Comment", response.Comment);
        }

        public virtual PartialViewResult GetNewLevel(int instanceId, int parentCommentId, int treeLevel)
        {
            var model = _viewModelMapperHelper.GetNewLevel(instanceId, parentCommentId, treeLevel);

            return PartialView("Partials/CommentPartial/Comment", model);
        }

        public virtual JsonResult TransferComment(int commentId, int instanceId, bool isCheckTransfer)
        {
            var response = _viewModelMapperHelper.TransferComment(commentId, instanceId, isCheckTransfer);

            return Json(response);
        }

        public virtual JsonResult DeleteComment(int commentId, int instanceId)
        {
            var response = _viewModelMapperHelper.DeleteComment(commentId, instanceId);

            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual JsonResult ValidateGenerateMinute(int instanceId)
        {
            var request = new ValidateMinuteRequest
            {
                InstanceId = instanceId
            };

            var response = _viewModelMapperHelper.ValidateGenerateMinute(request);

            return Json(response);
        }

        public virtual JsonResult GetCommentType(int instanceId)
        {
            var response = _viewModelMapperHelper.GetTypeComment(instanceId);
            return new JsonResult
            {
                Data = response,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual PartialViewResult GetFiltersComment(int instanceId,
            bool isOpcAgreement,
            FilterCommentViewModel filter)
        {
            var response = _viewModelMapperHelper.GetFiltersComment(instanceId, isOpcAgreement, filter);
            return PartialView("Partials/CommentPartial/CommentFilterPartial",
                response.VmrCommentTab.VmrFilterComments);
        }
        #endregion

        #region MethodsPrivate

        private VerDocumentRowViewModel GetNewDocumentData(
           int instanceId, string documentNumber, string documentName, string docWebUrl, string documentNameTemp)
        {
            var model = _viewModelMapperHelper.GetDataNewDocument(
                instanceId,
                documentNumber,
                documentName,
                docWebUrl,
                documentNameTemp);

            return model;
        }

        private VerDocumentRowViewModel GetDocumentData(
            int instanceId, string documentNumber, string documentName, string docWebUrl, string documentNameTemp)
        {
            var requestInstance = new VmrInstanceRequest
            {
                InstanceId = instanceId
            };
            var vmrInstance = _vmrGenericService.GetInstance(requestInstance).Instance;
            var model = new VerDocumentRowViewModel
            {
                SLno = 0,
                DocumentType = 0,
                DocumentName = documentName,
                PackageVersion = vmrInstance.VerInstance.Version.GetNameLanguage(Localization.CurrentLanguage),
                LastUpdate = DateTime.Now,
                UserName = UserIdentityManager
                .SearchFullNameByUserName(new GetUsersRequest { UserName = IDBContext.Current.UserName })
                .FullName,
                IsPrimary = false,
                IsRequired = false,
                DocumentNumber = documentNumber,
                ShpUrlDocument = docWebUrl,
                IsVer = false,
                LastUpdateFormat = string.Format("{0:dd MMM yyyy}", DateTime.Now),
                DocumentNameTemp = documentNameTemp
            };
            return model;
        }

        private string SanitizeFileName(string documentName)
        {
            string charsToRemove = new string(Path.GetInvalidFileNameChars()) +
                                   new string(Path.GetInvalidPathChars());
            string pattern = string.Format("[{0}]", Regex.Escape(charsToRemove));
            return Regex.Replace(documentName, pattern, "-");
        }
        #endregion
    }
}
