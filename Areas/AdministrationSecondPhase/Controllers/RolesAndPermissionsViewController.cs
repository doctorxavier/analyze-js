using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Newtonsoft.Json;

using IDB.MW.Application.AdministrationModule.Messages.RolesAndPermissionsService;
using IDB.MW.Application.AdministrationModule.Services.RolesAndPermissionsService;
using IDB.MW.Application.AdministrationModule.ViewModels.RolesAndPermissions;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Business.AdministrationSecondPhase.Contracts;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.RolesAndPermissions;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Controllers
{
    public partial class RolesAndPermissionsViewController : BaseController
    {
        #region fields

        private readonly ICatalogService _catalogService;
        private readonly IRolesAndPermissionsService _rolesAndPermissionsService;
        private readonly ViewModelMapperHelper _viewModelMapperHelper;
        private readonly IEquivalentTypeRoleService _equivalentTypeRoleService;

        #endregion

        public RolesAndPermissionsViewController(ICatalogService catalogService,
            IRolesAndPermissionsService rolesAndPermissionsService,
            IEquivalentTypeRoleService equivalentTypeRoleService)
        {
            _catalogService = catalogService;
            _rolesAndPermissionsService = rolesAndPermissionsService;
            _equivalentTypeRoleService = equivalentTypeRoleService;
            _viewModelMapperHelper = new ViewModelMapperHelper(_rolesAndPermissionsService);
        }

        #region Roles & Permission
        public virtual ActionResult RolesAndPermissions()
        {
            var model = new RoleAndPermissionViewModel
            {
                ResultSearchList = _rolesAndPermissionsService
                    .GetRolesAndPermissionsList(new SearchRolesAndPermissionViewModel
                    {
                        SearchRoleType = 0,
                        SearchRoles = string.Empty,
                        SearchPermission = string.Empty
                    }).GetRolesAndPermissionsList
            };

            RolesAndPermissionsPermission();
            model.HasPermissionWrite = IDBContext.Current
                .HasPermission(Permission.ADMIN_ROLES_AND_PERMISSIONS_WRITE);
            model.RoleTypeItems = _catalogService.GetListMasterData(MasterType.ROLE_TYPE);
            model.PermissionsItems = _viewModelMapperHelper.GetListPermission();
            model.RolesItems = _viewModelMapperHelper.GetListRoleWithoutRoleID();

            return View(model);
        }

        public virtual ActionResult RolesAndPermissionsCreateAndEdit(int? idRolePermission)
        {
            return View(GetDataRoleInformation(idRolePermission));
        }

        public virtual ActionResult RolesAndPermissionsCreateAndEditPartial(int? idRolePermission)
        {
            return PartialView("Partial/RolesAndPermissionsCreateAndEditPartial", GetDataRoleInformation(idRolePermission));
        }

        public virtual FileResult DownloadReport(SearchRolesAndPermissionViewModel filters)
        {
            var response = _rolesAndPermissionsService.GetReport(filters);

            if (!response.IsValid)
                return null;

            return File(
                response.File,
                RolesAndPermissionsValues.APPLICATION_REPORT,
                RolesAndPermissionsValues.FILENAME_REPORT);
        }

        #endregion

        public virtual ActionResult AddRowTablePermissionView()
        {
            ViewBag.ListType = _rolesAndPermissionsService.GetTypeList().ListResponse.Select(o => new SelectListItem { Value = o.Value, Text = o.Text }).ToList();
            ViewBag.ListCountryMulti = _catalogService.GetListMultiMasterData(MasterType.ORGANIZATION_UNIT, excludeByCode: new List<string> { CountryCode.UND });
            var includeCode = OrgUnitCode.INCLUDEBYCODE.ToList();
            ViewBag.ListDivisionMulti = _catalogService.GetListMultiMasterData(
                MasterType.ORGANIZATION_UNIT,
                true,
                includeByCode: includeCode);
            ViewBag.ListOperationTypeMulti = _catalogService.GetListMultiMasterData(MasterType.OPERATION_TYPE);
            return PartialView("Partial/TableAsignedPermissionNewRow");
        }

        public virtual JsonResult LoadCountryGroup(string filter)
        {
            return Json(_viewModelMapperHelper.GetGroupList(filter), JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult GetRoles(int type)
        {
            var response = _viewModelMapperHelper.GetListRole(type);
            return Json(new { IsValid = response.Any(), Data = response });
        }

        public virtual ActionResult SearchRolesAndPermissionsFilter(
            SearchRolesAndPermissionViewModel filters)
        {
            var response = _rolesAndPermissionsService.GetRolesAndPermissionsList(filters);

            return PartialView(
                "Partial/TableRoleAndPermissions",
                response.GetRolesAndPermissionsList);
        }

        #region Control Information

        public virtual ActionResult ControlInformation()
        {
            var model = _rolesAndPermissionsService.GetControlinformation();
            ViewBag.Country = _catalogService.GetListMultiMasterData(MasterType.COUNTRY, excludeByCode: new List<string> { CountryCode.UND });
            ViewBag.Division = _catalogService.GetListMultiMasterData(MasterType.ORGANIZATION_UNIT, true);
            ViewBag.OperationType = _catalogService.GetListMultiMasterData(MasterType.OPERATION_TYPE);
            ViewBag.OperationStatus = _catalogService.GetListMultiMasterData(MasterType.OVERALL_STAGE);
            ViewBag.SerializedViewModel = string.Empty;

            return View(model);
        }

        public virtual JsonResult GetPermissionControlInformation(string filter)
        {
            return Json(_viewModelMapperHelper.GetListPermissionByFilter(filter), JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult FilterByPermission(int permission)
        {
            var response = _rolesAndPermissionsService.GetPageListByPermission(permission).ListResponse;
            return Json(new { IsValid = response.Any(), Data = response });
        }

        public virtual ActionResult SearchControlInformationFilter(
            int searchPermissionId,
            int searchPageId,
            string searchCountryId,
            string searchDivisionId,
            string searchOperationType,
            string searchOperationStatus)
        {
            var response = _rolesAndPermissionsService.GetFieldTable(searchPermissionId, searchPageId, searchCountryId, searchDivisionId, searchOperationType, searchOperationStatus);
            ViewBag.SearchPermission = searchPermissionId.ToString();
            ViewBag.SearchPageId = searchPageId.ToString();
            ViewBag.PageTable = _rolesAndPermissionsService.GetPageList().ListResponse.Select(o => new SelectListItem { Value = o.Value, Text = o.Text }).ToList();
            ViewBag.Field = _rolesAndPermissionsService.GetFieldList(response.ControlInformation.TableField.Select(o => o.Field)).ListResponse.Select(o => new SelectListItem { Value = o.Value, Text = o.Text }).ToList();
            ViewBag.Country = _catalogService.GetListMultiMasterData(MasterType.COUNTRY, excludeByCode: new List<string> { CountryCode.UND });
            ViewBag.Division = _catalogService.GetListMultiMasterData(MasterType.ORGANIZATION_UNIT, true);
            ViewBag.OperationType = _catalogService.GetListMultiMasterData(MasterType.OPERATION_TYPE);
            ViewBag.OperationStatus = _catalogService.GetListMultiMasterData(MasterType.OVERALL_STAGE);
            ViewBag.SerializedViewModelControl = PageSerializationHelper.SerializeObject(response.ControlInformation);
            return PartialView("Partial/TableControlInformation", response.ControlInformation.TableField);
        }
        #endregion

        #region Permission

        public virtual JsonResult GetDescriptionPermission(int idPermission)
        {
            var response = _rolesAndPermissionsService.GetDescripcionPermission(idPermission).DescPermission;
            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult GetGroupAssigned(int roleType, int roleId)
        {
            var model = _rolesAndPermissionsService.GetAsignedGroupsList(roleType, roleId);
            ViewBag.Rtype = model.NameType;
            ViewBag.IsAd = model.IsAd;
            ViewBag.ListGroup = _catalogService.GetListMasterData(model.NameType, true);
            return PartialView("Partial/TableAsignedGroup", model.GetListAssignedGroupsList);
        }

        public virtual ActionResult GetNewRowGroupAssigned(int roleType)
        {
            var model = _equivalentTypeRoleService.GetEquivalentTypeRole(roleType);
            ViewBag.ListGroup = _catalogService.GetListMasterData(model.EquivalentType, true);
            return PartialView("Partial/TableAssignedGroupRow");
        }

        public virtual ActionResult NewRuleModal()
        {
            var model = _rolesAndPermissionsService.GetControlinformation();
            ViewBag.Page = _rolesAndPermissionsService.GetPageList().ListResponse.Select(o => new SelectListItem { Value = o.Value, Text = o.Text }).ToList();
            ViewBag.Country = _catalogService.GetListMultiMasterData(MasterType.COUNTRY);
            ViewBag.Division = _catalogService.GetListMultiMasterData(MasterType.ORGANIZATION_UNIT, true);
            ViewBag.OperationType = _catalogService.GetListMultiMasterData(MasterType.OPERATION_TYPE);
            ViewBag.OperationStatus = _catalogService.GetListMultiMasterData(MasterType.OVERALL_STAGE);
            ViewBag.SerializedModalModel = PageSerializationHelper.SerializeObject(model);
            return PartialView("Partial/CreateNewRoleControlInformation", model.TableField);
        }

        public virtual ActionResult AddRowTableNewRoleControlInformation(int page = 0)
        {
            ViewBag.Field =
                _rolesAndPermissionsService.GetControlByPage(Convert.ToInt32(page))
                    .ListResponse.Select(o => new SelectListItem
                    {
                        Value = o.Value,
                        Text = o.Text
                    })
                    .ToList();
            return PartialView("Partial/TableAsignedNewRuleRow");
        }

        public virtual JsonResult FilterControlsByPage(string page)
        {
            var response = _rolesAndPermissionsService.GetControlByPage(Convert.ToInt32(page)).ListResponse;
            return Json(new { IsValid = response.Any(), Data = response });
        }

        public virtual JsonResult GetPagesByPermission(int permission)
        {
            var response = _rolesAndPermissionsService.GetPagesByPermissions(permission);
            return new JsonResult { Data = response };
        }

        public virtual JsonResult RemoveControlInformation(string delrow)
        {
            var response = _rolesAndPermissionsService.RemoveControlInformation(Convert.ToInt32(delrow));
            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        #endregion

        public virtual JsonResult SearchDetailPermission(string query, int? page, string name)
        {
            var pageSize = OPUSGlobalValues.NUMBER_PAGE;

            var response = _rolesAndPermissionsService.GetDetailPermission(new DetailPermissionRequest
            {
                SearchTerm = query,
                Page = page,
                PageSize = pageSize,
                Items = name
            });

            if (!response.IsValid)
            {
                throw new JsonException(response.ErrorMessage);
            }

            var detailITems = response.Result.Select(o => new DetailPermissionList
            {
                id = o.ItemsId,
                codes = o.ItemsCode,
                text = name == OPUSGlobalValues.DIVISION_ITEMS ? o.ItemsFullName : o.ItemsName,
            }).ToList();

            return Json(new
            {
                total = detailITems.Count,
                results = detailITems,
                pagination = new { more = detailITems.Count == pageSize }
            }, JsonRequestBehavior.AllowGet);
        }

        private RoleInformationViewModel GetDataRoleInformation(int? idRolePermission)
        {
            idRolePermission = idRolePermission ?? 0;
            ViewBag.IDRolePermission = idRolePermission;
            var model = _rolesAndPermissionsService.GetRoleAndPermissionCreateAndEdit(idRolePermission);
            ViewBag.Create = idRolePermission == 0;
            model.GlobalPermissionList = new List<RowAssignedPermissionsViewModel>();

            if (idRolePermission != 0)
            {
                var listAssigned = _equivalentTypeRoleService
                    .GetEquivalentTypeRole(model.RoleType.GetValueOrDefault());
                ViewBag.ListGroup = _catalogService.GetListMasterData(listAssigned.EquivalentType, true);
                ViewBag.IsAd = listAssigned.EquivalentType == OPUSGlobalValues.AD_CODE;
                ViewBag.Rtype = listAssigned.NameTypeView;
            }

            model.GlobalPermissionList = _rolesAndPermissionsService
                .GetAssignedPermissionsTable(Convert.ToInt32(idRolePermission))
                .GetAssignedPermissionsViewModel;

            if (model.EfectiveDate == null)
            {
                model.EfectiveDate = DateTime.Now;
            }

            ViewBag.RoleByType = _viewModelMapperHelper.GetListRole(model.RoleType.GetValueOrDefault());
            ViewBag.RoleType = _catalogService.GetListMasterData(MasterType.ROLE_TYPE);
            ViewBag.ListPermission = _viewModelMapperHelper.GetListPermission();
            ViewBag.ListCountryMulti = _catalogService.GetListMultiMasterData(
                MasterType.ORGANIZATION_UNIT, excludeByCode: new List<string> { CountryCode.UND });
            var includeCode = OrgUnitCode.INCLUDEBYCODE.ToList();
            ViewBag.ListDivisionMulti = _catalogService.GetListMultiMasterData(
                MasterType.ORGANIZATION_UNIT,
                true,
                includeByCode: includeCode);
            ViewBag.ListOperationTypeMulti = _catalogService.GetListMultiMasterData(MasterType.OPERATION_TYPE);
            ViewBag.ListType = _rolesAndPermissionsService.GetTypeList().ListResponse
                .Select(o => new SelectListItem
                {
                    Value = o.Value,
                    Text = o.Text
                })
                .ToList();
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            model.HasPermissionWrite = IDBContext.Current
                .HasPermission(Permission.ADMIN_ROLES_AND_PERMISSIONS_WRITE);

            return model;
        }

        private void RolesAndPermissionsPermission()
        {
            ViewBag.Permission = IDBContext.Current.HasPermission(Permission.SYSTEM_ADMINISTRATOR);
        }
    }
}