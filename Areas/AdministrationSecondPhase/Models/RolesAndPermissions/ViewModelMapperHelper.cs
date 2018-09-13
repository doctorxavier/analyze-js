using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.MW.Application.AdministrationModule.Services.RolesAndPermissionsService;
using IDB.MW.Application.AdministrationModule.Messages.RolesAndPermissionsService;
using IDB.MW.Application.OPUSModule.Messages.OperationDataService;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.RolesAndPermissions
{
    public class ViewModelMapperHelper
    {
        #region fields
        private readonly IRolesAndPermissionsService _rolesAndPermissionsService;
        
        #endregion

        public ViewModelMapperHelper(IRolesAndPermissionsService rolesAndPermissionsService)
        {
            _rolesAndPermissionsService = rolesAndPermissionsService;
        }

        public GetSelectListItemResponse GetGroupList(string name)
        {
            var response = new GetSelectListItemResponse();

            var rolesAndPermissionsRepository = _rolesAndPermissionsService.GetGroupNameList(name);

            if (rolesAndPermissionsRepository.GetListItemGroupAssigned != null && rolesAndPermissionsRepository.GetListItemGroupAssigned.Count > 0)
            {
                response.ListResponse = rolesAndPermissionsRepository.GetListItemGroupAssigned.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.Value,
                    Text = o.Text
                }).ToList();
            }

            return response;
        }

        public List<SelectListItem> GetListRole(int idRole)
        {
            var response = new List<SelectListItem>();
            var rolesList = _rolesAndPermissionsService.GetRoleList(idRole);

            if (rolesList.GetListItemRoles != null && rolesList.GetListItemRoles.Any())
            {
                response = rolesList.GetListItemRoles.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.Value,
                    Text = o.Text
                }).ToList();
            }
            
            return response;
        }

        public List<SelectListItem> GetListRoleWithoutRoleID()
        {
            var response = new List<SelectListItem>();
            var rolesList = _rolesAndPermissionsService.GetRoleListWithoutRoleId();

            if (rolesList.GetListItemRoles != null && rolesList.GetListItemRoles.Any())
            {
                response = rolesList.GetListItemRoles.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.Value,
                    Text = o.Text
                }).ToList();
            }

            return response;
        }

        public List<SelectListItem> GetListPermission()
        {
            var response = new List<SelectListItem>();
            var permisionList = _rolesAndPermissionsService.GetPermissionList();

            if (permisionList.ListResponse != null && permisionList.ListResponse.Any())
            {
                response = permisionList.ListResponse.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.Value,
                    Text = o.Text
                }).ToList();
            }

            return response;
        }

        public GetListItemPermissionResponse GetListPermissionByFilter(string name)
        {
           return _rolesAndPermissionsService.GetPermissionListByFilter(name);
        }
    }
}