using System.Collections.Generic;
using System.Web.Mvc;

using IDB.MW.Business.Core.SharepointSecurityService;
using IDB.MW.Infrastructure.Caching.Contracts;
using IDB.MW.Infrastructure.SecurityService;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.MW.Infrastructure.SecurityService.Enums;
using IDB.Presentation.MVC4.Controllers;
using IDB.MW.Domain.Session;

namespace IDB.Presentation.MVC4.Areas.PCR.Controllers
{
    public partial class PCRMockController : BaseController
    {
        #region Fields
        private readonly ICacheStorageService _cacheStorageService;
        private readonly IAuthorizationService _authorizationService;
        #endregion

        public PCRMockController(ICacheStorageService cacheStorageService)
        {
            _authorizationService = AuthorizationServiceFactory.Current;
            _cacheStorageService = cacheStorageService;
        }

        public virtual JsonResult ChangeRole(string operationNumber, ActionEnum rol)
        {
            _cacheStorageService.Remove("AuthorizationInfo");

            var auth = new AuthorizationOperationInfo()
            {
                ActionList = new List<ActionEnum>(),
                RoleList = new List<RoleEnum>() 
                {
                   RoleEnum.Unrecognized
                },
                UserName = IDBContext.Current.UserLoginName
            };

            auth.ActionList.Add(ActionEnum.ConvergenceReadPermission);
            auth.ActionList.Add(rol);

            var authPermission = new AuthorizationInfo()
            {
                Authorization = new Dictionary<string, AuthorizationOperationInfo>()
            };
            authPermission.Authorization.Add(operationNumber, auth);

            _cacheStorageService.Add("AuthorizationInfo", authPermission);

            return Json(new { IsValid = true, ErrorMessage = "Permissions was changed" });
        }

        public virtual JsonResult RemovePermission()
        {
            var authorizationInfo = _cacheStorageService.Get<AuthorizationInfo>("AuthorizationInfo");

            if (authorizationInfo == null)
            {
                return Json(new { IsValid = false, ErrorMessage = "No permission data to remove." });
            }

            _cacheStorageService.Remove("AuthorizationInfo");
            authorizationInfo = _cacheStorageService.Get<AuthorizationInfo>("AuthorizationInfo");

            return Json(authorizationInfo == null ? new { IsValid = true, ErrorMessage = "Permission data removed correctly." } : new { IsValid = false, ErrorMessage = "Permission data could not be removed." });
        }
    }
}