using System.Web.Mvc;

using IDB.MW.Application.AdministrationModule.Messages.RolesAndPermissionsService;
using IDB.MW.Application.AdministrationModule.Services.RolesAndPermissionsService;
using IDB.MW.Application.AdministrationModule.ViewModels.RolesAndPermissions;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.RolesAndPermissions;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Domain.Session;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Controllers
{
    public partial class RolesAndPermissionsSaveController : MVC4.Controllers.ConfluenceController
    {
        #region constants
        private const string UrlRolesAndPermissionsView = "/AdministrationSecondPhase/RolesAndPermissions";
        #endregion

        #region fields
        private readonly IRolesAndPermissionsService _rolesAndPermissionsService;
        private readonly IAuthorizationService _authorizationService;

        #endregion 

        public RolesAndPermissionsSaveController(IRolesAndPermissionsService roleAndPermissionService, IAuthorizationService authorizationService)
        {
            _rolesAndPermissionsService = roleAndPermissionService;
            _authorizationService = authorizationService;
        }

        public virtual JsonResult RolesAndPermissionsDataSaveController()
        {
           RolesAndPermissionsSaveResponse response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<RoleInformationViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateRoleInformationSaveViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources("edit", UrlRolesAndPermissionsView, viewModel.IdRolePermission.ToString(), userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new RolesAndPermissionsSaveResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _rolesAndPermissionsService.RolesAndPermissionsSave(viewModel);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(UrlRolesAndPermissionsView, viewModel.IdRolePermission.ToString(), userName);
                }
            }

            return Json(response);
        }

        public virtual JsonResult CreateNewRoleSave()
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel =
                PageSerializationHelper.DeserializeObject<ControlInformationViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateControlInformationViewModel(jsonDataRequest.ClientFieldData);
            var response = _rolesAndPermissionsService.ControlInformationEditSave(viewModel);

            return Json(response);
        }

        public virtual JsonResult ControlInformationSave()
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel =
                PageSerializationHelper.DeserializeObject<ControlInformationViewModel>(jsonDataRequest.SerializedData);
            viewModel.UpdateAllControlInformationViewModel(jsonDataRequest.ClientFieldData);

            var response = _rolesAndPermissionsService.ControlInformationEditSave(viewModel);
              
            return Json(response);
        }
    }
}