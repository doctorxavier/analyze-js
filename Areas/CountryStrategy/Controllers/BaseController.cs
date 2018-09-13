using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.MW.Business.Core.SharepointSecurityService;
using IDB.MW.Domain.Session;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.MW.Infrastructure.Caching;
using IDB.MW.Infrastructure.Caching.Contracts;
using IDB.MW.Infrastructure.SecurityService;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.MW.Infrastructure.SecurityService.Enums;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.CountryStrategy.Controllers
{
    public abstract partial class BaseController : MVC4.Controllers.ConfluenceController
    {
        #region Constants
        protected const string FRIENDLY_PERMISSION_ERROR = "CS.CommonMesssage.NoPermission";
        #endregion

        #region Fields
        private readonly IAuthorizationService _authorizationService;
        private readonly ICacheStorageService _cacheStorageService;

        #endregion

        #region Constructor

        protected BaseController(IAuthorizationService authorizationService)
        {
            _authorizationService = AuthorizationServiceFactory.Current;
            _cacheStorageService = CacheStorageFactory.Current;
        }

        #endregion

        public virtual ActionResult SetMockPermission(ActionEnum[] actions = null)
        {
            _cacheStorageService.Remove("AuthorizationInfo");

            var auth = new AuthorizationOperationInfo
            {
                ActionList = new List<ActionEnum>(),
                RoleList = new List<RoleEnum>(),
                UserName = IDBContext.Current.UserName
            };

            if ((actions != null) && actions.Length > 0)
            {
                auth.ActionList.AddRange(actions);
            }

            var authPermission = new AuthorizationInfo
            {
                GlobalAuthorization = auth
            };

            _cacheStorageService.Add("AuthorizationInfo", authPermission);

            return Json(new { IsValid = true, ErrorMessage = "Permissions was changed" });
        }

        protected bool SetViewBagErrorMessageInvalidResponse(ResponseBase response)
        {
            ViewBag.IsValid = response.IsValid;
            if (!response.IsValid)
            {
                var urlDecode = HttpUtility.UrlDecode(response.ErrorMessage);
                if (urlDecode != null)
                {
                    ViewBag.ErrorMessage = HttpUtility.HtmlEncode(urlDecode.Replace(Environment.NewLine, " "));
                }
            }

            return response.IsValid;
        }

        protected IDictionary<ActionEnum, bool> SetViewBagPermission(string operationNumber, params ActionEnum[] action)
        {
            ////var permissions = _authorizationService.IsAuthorized(IDBContext.Current.UserName, operationNumber, action);

            var permissions = new Dictionary<ActionEnum, bool>();
            foreach (var item in action)
            {
                permissions.Add(item, true);
            }

            ViewBag.Permissions = permissions;
            return permissions;
        }

        protected void SetViewBagPermissionAndCheckAny(string operationNumber, params ActionEnum[] action)
        {
            var permissions = SetViewBagPermission(operationNumber, action);
            var someTrue = MvcHelpers.CheckSomeRoles(permissions, action);
            if (!someTrue)
            {
                throw new Exception(Localization.GetText(FRIENDLY_PERMISSION_ERROR));
            }
        }
    }
}