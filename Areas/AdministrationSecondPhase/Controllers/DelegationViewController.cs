using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.MW.Application.AdministrationModule.Services.DelegationService;
using IDB.MW.Application.AdministrationModule.ViewModels.Delegation;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.OPUSModule.Enums;
using IDB.MW.Business.AdministrationSecondPhase.Contracts;
using IDB.MW.Business.DocumentManagement.Contracts;
using IDB.MW.Domain.Contracts.DomainServices;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.Domain.Values.Delegation;
using IDB.MW.Infrastructure.ReportManager.Enums;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.AuthorizationAll;
using IDB.MW.Application.AdministrationModule.Messages.DelegationService.Request;
using IDB.MW.Business.AdministrationSecondPhase.Messages.Request;
using IDB.Architecture.Security;
using IDB.MW.Application.Core.Mappers;
using IDB.MW.Application.Core.Messages;
using IDB.Architecture.Security.Messages.UserIdentity;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Controllers
{
    public partial class DelegationViewController : BaseController
    {
        #region Fields

        private readonly ICatalogService _catalogService;
        private readonly IDelegationService _delegationService;
        private readonly IUserTypeService _userTypeService;
        private readonly Models.Delegation.ViewModelMapperHelper _viewModelMapperHelper;
        private readonly ViewModelMapperHelper _authorizeAllViewModelMapperHelper;
        private readonly IDocumentManagementService _documentManagementService;
        private readonly IAuthorizationManager _authorizationManager;

        #endregion

        #region Construct

        public DelegationViewController(ICatalogService catalogService,
            IDelegationService delegationService,
            IUserTypeService userTypeService,
            IDocumentManagementService documentManagementService,
            IAuthorizationManager authorizationManager)
        {
            _catalogService = catalogService;
            _delegationService = delegationService;
            _userTypeService = userTypeService;
            _viewModelMapperHelper =
                new Models.Delegation.ViewModelMapperHelper(ViewBag, _delegationService);
            _authorizeAllViewModelMapperHelper = new ViewModelMapperHelper();
            _documentManagementService = documentManagementService;
            _authorizationManager = authorizationManager;
        }

        #endregion

        #region Search

        public virtual ActionResult Search()
        {
            DelegationPermission();
            ViewBag.NeedLoader = true;

            var searchModel = new DelegationSearchViewModel
            {
                TableSearch = new List<TableDelegationSearchViewModel>(),
                DisplayOptions = _viewModelMapperHelper.GetDisplayedOptions()
            };

            ViewBag.UserName = string.Empty;

            if (ViewBag.DelegationAdminPermission == false)
            {
                var request = new FilterRequest
                {
                    StartDate = string.Empty,
                    EndDate = string.Empty,
                    UserTypeDelegation = "0",
                    UserDelegation = ViewBag.UserName,
                    InactiveDelegate = false
                };

                searchModel.TableSearch = _delegationService.GetDelegationMainList(request)
                    .GetListItemTableDelegator;
                var dataUser = _delegationService
                .GetDelegatorList(IDBContext.Current.UserName).ListResponse;
                if (dataUser != null)
                {
                    ViewBag.FullName = dataUser.Count > 0
                        ? dataUser.First().Text : string.Empty;
                    ViewBag.UserName = dataUser.Count > 0
                        ? dataUser.First().Value : string.Empty;
                }
                else
                {
                    SetFullname();
                }
            }

            ViewBag.listOfType = _catalogService.GetListMasterData(MasterType.DELEGATION_TYPE);

            return View(searchModel);
        }

        public virtual ActionResult FilterMainTableDelegator(string startDate,
            string endDate,
            string userTypeDelegation,
            string userDelegation,
            bool inactiveDelegate)
        {
            var request = new FilterRequest
            {
                StartDate = startDate,
                EndDate = endDate,
                UserTypeDelegation = userTypeDelegation,
                UserDelegation = userDelegation,
                InactiveDelegate = inactiveDelegate == true
            };
            var response = _delegationService.GetDelegationMainList(request);

            return PartialView("Partial/Search/Tables/TableFilterDelegation",
                response.GetListItemTableDelegator);
        }

        public virtual JsonResult GetUserType(int type = 0)
        {
            var response = _userTypeService.GetUserTypeFromDb(type);
            return Json(new { IsValid = true, Data = response });
        }
        #endregion

        #region Common Methods
        public virtual JsonResult SearchUser(string filter)
        {
            var response = UserIdentityManager.SearchUsersByNameOrFullName(new GetUserByPCmailOrNameRequest { Search = filter });

            return new JsonResult
            {
                Data = SearchUserByNameOrFullNameList(response),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        #endregion

        #region Delegation

        public virtual ActionResult Delegation(int delegationId = 0)
        {
            ViewBag.NeedLoader = true;
            return View(GetDelegationData(delegationId));
        }

        public virtual ActionResult DelegationContent(int delegationId = 0)
        {
            ViewBag.NeedLoader = true;
            return PartialView("Partial/Delegation/DelegationPartial",
                GetDelegationData(delegationId));
        }

        public virtual ActionResult SearchDelegatorFilter(
            string searchDelegatorName,
            string[] nameOperationNumber,
            int role)
        {
            var orgUnitId = new List<int>();
            var response = _delegationService.SeachDelegatorList(searchDelegatorName,
                nameOperationNumber,
                orgUnitId,
                role,
                false);
            ViewBag.Roles = _viewModelMapperHelper.GetRoleList();
            ViewBag.HasDelegation = false;

            return PartialView("Partial/Delegation/Tables/TableDelegator",
                response.GetListTableDelegator);
        }

        public virtual ActionResult SearchDelegatorFilterRol(string userName,
            string operationNumber = "")
        {
            var response = _delegationService.GetRoleListByUserNameAndOperationNumber(userName,
                operationNumber);

            return new JsonResult
            {
                Data = response,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual ActionResult RefreshAssignPermissionTaskDelegation(
            string userName,
            string[] operationNumbers,
            int delegationId,
            int? roleId,
            string[] countrys,
            string[] cDepartments,
            string[] departments,
            string[] divisions)
        {
            var model = _viewModelMapperHelper.GetRefreshDelegation(
                userName,
                operationNumbers,
                delegationId,
                roleId,
                countrys,
                cDepartments,
                departments,
                divisions);
            ViewBag.SerializedViewModelNew = PageSerializationHelper.SerializeObject(model);
            ViewBag.IsNewDelegation = model.DelegationId.Equals(0);
            var isTeamMember = _delegationService.IsTeamMember(roleId);
            ViewBag.AssignSubDelegation = isTeamMember ? null : model.AssignSubDelegation;
            ViewBag.SubDelegationAllSelected = !isTeamMember
                && model.AssignSubDelegation.SubDelegationAllSelected;

            return PartialView("Partial/Delegation/AssignPermissionsTask",
                model.AssignPermissionsTask);
        }

        public virtual ActionResult DelegablePermissionTask(
            string userName,
            string[] operationNumbers,
            int delegationId,
            int? roleId,
            string[] countrys,
            string[] cDepartments,
            string[] departments,
            string[] divisions,
            string delegateUsername)
        {
            var model = _viewModelMapperHelper
                .GetDelegablePermissionsTasks(
                    userName,
                    operationNumbers,
                    delegationId,
                    roleId,
                    countrys,
                    cDepartments,
                    departments,
                    divisions,
                    delegateUsername);
            ViewBag.SerializedViewModelNew = PageSerializationHelper.SerializeObject(model);
            ViewBag.IsNewDelegation = model.DelegationId.Equals(0);
            var isTeamMember = _delegationService.IsTeamMember(roleId);
            ViewBag.AssignSubDelegation = isTeamMember ? null : model.AssignSubDelegation;
            ViewBag.SubDelegationAllSelected =
                !isTeamMember && model.AssignSubDelegation.SubDelegationAllSelected;

            return PartialView("Partial/Delegation/AssignPermissionsTask",
                model.AssignPermissionsTask);
        }

        public virtual ActionResult RefreshAssignAttributesSubdelegation(string operationTypes,
            int delegationId)
        {
            var attributesModel = new List<AssignAttributeViewModel>();
            var operationType = operationTypes.Split('|').ToList();

            if (!(operationType.Count().Equals(1) && string.IsNullOrEmpty(operationType.First())))
            {
                attributesModel.AddRange(
                    _delegationService.GetAttributesEntitiesByOperationType(operationType)
                    .ToList());

                attributesModel = attributesModel.GroupBy(x => x.AttributeValue)
                    .Select(y => y.FirstOrDefault()).ToList();
            }

            return PartialView("Partial/Delegation/AssignAttributes", attributesModel);
        }

        public virtual ActionResult GetRowComment(string operationNumber)
        {
            return PartialView("Partial/Delegation/Tables/RowCommentDelegation");
        }

        public virtual ActionResult GetRowCommentAuthorizeAll(string operationNumber)
        {
            return PartialView("Partial/AuthorizeAll/Tables/RowCommentAuthorizeAll");
        }

        public virtual PartialViewResult AddNewDocument(string documentNumber, string fileName)
        {
            var user = IDBContext.Current.UserName;
            var date = DateTime.Now.Date;
            var model = new TableDocumentViewModel();
            model.User = user ?? string.Empty;
            model.Date = date;
            model.DocNumber = documentNumber ?? string.Empty;
            model.Description = fileName;
            return PartialView("Partial/Document/RowDataDocument", model);
        }

        public virtual JsonResult GetUsersList(string filter)
        {
            var response = UserIdentityManager.SearchUsersByNameOrFullName(new GetUserByPCmailOrNameRequest { Search = filter });
            return new JsonResult
            {
                Data = SearchUserByNameOrFullNameList(response),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual JsonResult GetUsersListNoSelf(string filter)
        {
            var userList = UserIdentityManager.SearchUsersByNameOrFullName(new GetUserByPCmailOrNameRequest { Search = filter });
            UsersByNameOrFullNameResponse response = new UsersByNameOrFullNameResponse();
            if (userList != null)
            {
                response = userList.Users.UserIdentityModelToResponse();

                if (response != null && response.ListResponse != null)
                {
                    var currentUser = response.ListResponse
                    .SingleOrDefault(o => o.Value.ToUpper() == IDBContext.Current.UserName.ToUpper());
                    response.ListResponse.Remove(currentUser);
                }
            }

            return new JsonResult
            {
                Data = response,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual ActionResult GetRowDocument()
        {
            return PartialView("Partial/Delegation/Tables/RowDocument");
        }

        public virtual FileResult DownloadDelegationReport()
        {
            var response = _delegationService.DownloadDelegationReport(OutputFormatEnum.Excel);
            return !response.IsValid
                ? null : File(response.File, "application/vnd.ms-excel", "DelegationsReport.xls");
        }

        public virtual FileResult DownloadDelegatorFilteredFileExport(
            string formatType,
            string userTypeDelegation,
            string userDelegation,
            string startDate,
            string endDate,
            bool inactiveDelegate)
        {
            var response = _delegationService.GetDelegatorFilteredToFileExport(
                formatType,
                userTypeDelegation,
                userDelegation,
                startDate,
                endDate,
                inactiveDelegate);

            if (!response.IsValid)
            {
                return null;
            }

            var fileName = DelegationGlobalValues.AUTHORIZATION_RECORDS_FILE_NAME;
            string application;
            switch (formatType)
            {
                case DelegationGlobalValues.EXCEL:
                    application = MimeTypeMap.GetMimeType(DelegationGlobalValues.EXCEL);
                    fileName = fileName + DelegationGlobalValues.DOTEXCEL;
                    break;
                default:
                    application = MimeTypeMap.GetMimeType(DelegationGlobalValues.PDF);
                    fileName = fileName + DelegationGlobalValues.DOTPDF;
                    break;
            }

            return File(response.File, application, fileName);
        }

        public virtual FileResult DownloadDelegatorFileExport(
            string formatType,
            string searchDelegatorName,
            string[] operationNumber,
            int role,
            int delegationId)
        {
            var response = _delegationService.GetDelegatorFoundToFileExport(
                formatType,
                searchDelegatorName,
                operationNumber,
                role,
                delegationId);

            if (!response.IsValid)
            {
                return null;
            }

            var fileName = DelegationGlobalValues.AUTHORIZATION_DATA_DETAIL;
            string application;
            switch (formatType)
            {
                case DelegationGlobalValues.EXCEL:
                    application = MimeTypeMap.GetMimeType(DelegationGlobalValues.EXCEL);
                    fileName = fileName + DelegationGlobalValues.DOTEXCEL;
                    break;
                default:
                    application = MimeTypeMap.GetMimeType(DelegationGlobalValues.PDF);
                    fileName = fileName + DelegationGlobalValues.DOTPDF;
                    break;
            }

            return File(response.File, application, fileName);
        }

        public virtual FileResult DownloadDelegationDocumentsFileExport(
            string formatType,
            string userName,
            string[] operationNumber,
            int delegationId,
            int? roleId,
            string[] country,
            string[] countryDepartment,
            string[] department,
            string[] division)
        {
            var response = _delegationService.GetDelegatorDocumentsToFileExport(
                formatType,
                userName,
                operationNumber,
                delegationId,
                roleId,
                country,
                countryDepartment,
                department,
                division);

            if (!response.IsValid)
            {
                return null;
            }

            var fileName = DelegationGlobalValues.DOCUMENTS_FILE_NAME;
            string application;
            switch (formatType)
            {
                case DelegationGlobalValues.EXCEL:
                    application = MimeTypeMap.GetMimeType(DelegationGlobalValues.EXCEL);
                    fileName = fileName + DelegationGlobalValues.DOTEXCEL;
                    break;
                default:
                    application = MimeTypeMap.GetMimeType(DelegationGlobalValues.PDF);
                    fileName = fileName + DelegationGlobalValues.DOTPDF;
                    break;
            }

            return File(response.File, application, fileName);
        }

        public virtual FileResult DownloadAuthorizeAllFileExport(
            string formatType,
            string searchDelegatorName)
        {
            var response = _delegationService.GetAuthorizeAllToFileExport(
                searchDelegatorName,
                formatType);

            if (!response.IsValid)
            {
                return null;
            }

            var fileName = DelegationGlobalValues.AUTHORIZATION_DATA_DETAIL;
            string application;
            switch (formatType)
            {
                case DelegationGlobalValues.EXCEL:
                    application = MimeTypeMap.GetMimeType(DelegationGlobalValues.EXCEL);
                    fileName = fileName + DelegationGlobalValues.DOTEXCEL;
                    break;
                default:
                    application = MimeTypeMap.GetMimeType(DelegationGlobalValues.PDF);
                    fileName = fileName + DelegationGlobalValues.DOTPDF;
                    break;
            }

            return File(response.File, application, fileName);
        }

        #endregion

        #region AuthorizeAll
        public virtual ActionResult AuthorizeAll(int delegationId = 0)
        {
            AuthorizationAllViewModel responseModel;
            if (delegationId == 0)
            {
                var response = new MW.Application.AdministrationModule.Messages.DelegationService
                    .GetListTableDelegatorResponse
                {
                    IsValid = true,
                    UnavailableDates = new List<DateTime>()
                };

                responseModel = _authorizeAllViewModelMapperHelper
                    .AuthorizeAllToViewModel(response);
                responseModel.OtherReasonId = _delegationService.GetOtherReason();
            }
            else
            {
                responseModel = AuthorizeAllData(delegationId);
            }

            responseModel.AuthorizationAllView.DisplayOptions =
                _viewModelMapperHelper.GetDisplayedOptions();

            return View(responseModel);
        }

        public virtual ActionResult AuthorizeAllFilter(string searchDelegatorName)
        {
            var response = _delegationService.SeachDelegatorAuthorizeAllList(
                searchDelegatorName, false);
            if (response.UnavailableDates == null)
                response.UnavailableDates = new List<DateTime>();
            var responseModel =
                _authorizeAllViewModelMapperHelper.AuthorizeAllToViewModel(response);

            return PartialView("Partial/AuthorizeAll/Tables/TableFilterAuthorizationAll",
                responseModel.AuthorizationAllView);
        }

        public virtual JsonResult ValidateDateAuthorizeAll(
            string user, DateTime begin, DateTime end)
        {
            var response = _delegationService.ValidateDateAuthorizeAll(user, begin, end);
            return new JsonResult
            {
                Data = response,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        #endregion

        public virtual ActionResult GetRoles()
        {
            return View();
        }

        #region Permission
        private void DelegationPermission()
        {
            ViewBag.DelegationAdminPermission =
                IDBContext.Current.HasPermission(Permission.DELEGATION_ADMINISTRATION)
                || IDBContext.Current.HasPermission(Permission.DELEGATION_WRITE);
        }
        #endregion

        private DelegationViewModel GetDelegationData(int delegationId = 0)
        {
            DelegationPermission();
            if (ViewBag.DelegationAdminPermission == false)
            {
                if (IDBContext.Current.UserName != string.Empty)
                {
                    var listUsers = UserIdentityManager
                    .SearchUsersByNameOrFullName(new GetUserByPCmailOrNameRequest { Search = IDBContext.Current.UserName });
                    if (listUsers != null)
                    {
                        var response = listUsers.Users.UserIdentityModelToResponse();
                        ViewBag.FullName = response.ListResponse.FirstOrDefault().Text;
                    }
                }
                else
                {
                    ViewBag.FullName = string.Empty;
                }

                ViewBag.Roles = _viewModelMapperHelper
                    .GetRoleListByUserName(IDBContext.Current.UserName);
            }
            else
            {
                ViewBag.Roles = _viewModelMapperHelper.GetRoleList();
            }

            var users = _authorizationManager
                .GetUsersByGroup(DelegationEnum.DelegationAdministrator, string.Empty)
                .Where(o => o.Name != null).Select(o => o.Name.Trim()).ToList();

            var model = _viewModelMapperHelper.GetDelegation(delegationId);
            if (model == null)
            {
                model = _viewModelMapperHelper.GetDelegation(0);
                ViewBag.ErrorMessage = string.Empty;
                _delegationService.ExpireDelegationNow(delegationId, -1);
            }

            model.OtherReason = _delegationService.GetOtherReason();
            model.DisplayOptions = _viewModelMapperHelper.GetDisplayedOptions();
            ViewBag.IsNewDelegation = model.DelegationId.Equals(0);
            ViewBag.IsDelegator = model.IsDelegator;
            ViewBag.IsExpired = model.UserToAssign.EndDate.HasValue
                ? (model.UserToAssign.EndDate.Value.Date < DateTime.Now.Date) : false;
            ViewBag.RoleCurrent = users.Count > 0
                ? users.Contains(IDBContext.Current.UserName) : false;
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            var isTeamMember = _delegationService.IsTeamMember(model.DelegationFilter.RoleId);
            ViewBag.AssignSubDelegation = isTeamMember ? null : model.AssignSubDelegation;
            ViewBag.SubDelegationAllSelected = model.AssignSubDelegation.SubDelegationAllSelected;

            ViewBag.AdminDoc = model.TableDocument.Count();
            ViewBag.HasDelegation = delegationId != 0;

            var operationsToSave = string.Empty;
            int count = 0;

            if (model.ListOfOperations != null)
            {
                foreach (var opN in model.ListOfOperations)
                {
                    if (count > 0)
                    {
                        operationsToSave = operationsToSave +
                            AgreementsAndConditionsConstants.PERMISSIONS_CONCAT_SEPARATOR + opN;
                    }
                    else
                    {
                        operationsToSave = operationsToSave + opN;
                    }

                    count++;
                }
            }

            ViewBag.OperationsToSave = operationsToSave;

            return model;
        }

        private AuthorizationAllViewModel AuthorizeAllData(int delegationId)
        {
            var request = new AuthorizeAllRequest
            {
                DelegationId = delegationId
            };
            var response = _delegationService.GetAuthorizeAll(request);
            var responseModel =
                _authorizeAllViewModelMapperHelper.AuthorizeAllToViewModel(response);
            return responseModel;
        }

        private ListItemModelResponse SearchUserByNameOrFullNameList(
                                        GetUserByPCmailOrNameResponse getUserByPcmailOrName)
        {
            var response = new ListItemModelResponse();
            response.IsValid = getUserByPcmailOrName.IsValid;
            response.ErrorMessage = getUserByPcmailOrName.ErrorMessage;
            response.ListResponse = getUserByPcmailOrName.Users.Select(lr => 
                new MW.Application.Core.ViewModels.ListItemViewModel
                {
                    Text = lr.FullName,
                    Value = lr.UserName,
                    AdditionalData = lr.OrganizationalUnit
                })
                .ToList();            

            return response;
        }

        private void SetFullname()
        {
            string fulllName = string.Empty;
            var listUserIdentity = UserIdentityManager
                .SearchUsersByNameOrFullName(new GetUserByPCmailOrNameRequest { Search = IDBContext.Current.UserName });

            if (listUserIdentity != null)
            {
                var response = listUserIdentity.Users.UserIdentityModelToResponse();
                if (response != null && response.ListResponse != null)
                {
                    var listItemViewModel = response.ListResponse.FirstOrDefault();
                    fulllName = listItemViewModel.Text;
                }
            }

            ViewBag.FullName = fulllName;
            ViewBag.UserName = IDBContext.Current.UserName;
        }
    }
}