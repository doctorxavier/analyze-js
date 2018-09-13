using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.MW.Infrastructure.Caching.Contracts;
using IDB.MW.Infrastructure.SecurityService;
using IDB.MW.Infrastructure.Caching;
using IDB.MW.Infrastructure.ApplicationBase.Messages;

namespace IDB.Presentation.MVC4.Areas.CPD.Controllers
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
    }
}