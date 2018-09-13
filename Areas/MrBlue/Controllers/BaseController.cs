using System;
using System.Web;
using System.Web.Mvc;

using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Domain.Session;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.MW.Infrastructure.BaseClasses;
using IDB.MW.Infrastructure.SecurityService;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.MrBlue.Controllers
{
    public abstract partial class BaseController : MVC4.Controllers.ConfluenceController
    {
        #region Fields

        private readonly IAuthorizationService _authorizationService;
        private readonly IEnumMappingService _enumMappingService;

        #endregion

        #region Constructor

        protected BaseController(IAuthorizationService authorizationService, IEnumMappingService enumMappingService)
        {
            _authorizationService = AuthorizationServiceFactory.Current;
            _enumMappingService = enumMappingService;
        }

        #endregion

        protected bool SetViewBagErrorMessageInvalidResponse(ResponseBase response)
        {
            ViewBag.IsValid = response.IsValid;
            if ((ViewBag.ErrorMessage == null) && !string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                var urlDecode = HttpUtility.UrlDecode(response.ErrorMessage);
                if (urlDecode != null)
                {
                    ViewBag.ErrorMessage = HttpUtility.HtmlEncode(urlDecode.Replace(Environment.NewLine, " "));
                }
            }

            return response.IsValid;
        }

        protected void SetViewBagRoles(string operationNumber)
        {
            if (ViewBag.IsValid ?? true)
            {
                ViewBag.ReadRole = true;
                ViewBag.WriteRole = IDBContext.Current.HasPermission(Permission.ESG_SPECIALIST_WRITE_PERMISSIONS);
            }
            else
            {
                ViewBag.ReadRole = false;
                ViewBag.WriteRole = false;
            }
        }

        protected CustomEnumDictionary<int> SetViewBagEnumMapping<T>()
        {
            var result = _enumMappingService.GetMappingCodeCollection<T>();

            if (ViewBag.EnumMapping == null)
            {
                ViewBag.EnumMapping = result;
                return result;
            }

            ((CustomEnumDictionary<int>)ViewBag.EnumMapping).AddRange(result);
            return ViewBag.EnumMapping;
        }

        protected JsonResult TryAccessToResources(string url, string operationNumber, int? versionId)
        {
            if (versionId.HasValue)
            {
                var errorMessage = SynchronizationHelper.AccessToResources(
                    SynchronizationHelper.EDIT_MODE,
                    url,
                    string.Format("{0}-{1}", operationNumber, versionId.Value),
                    IDBContext.Current.UserLoginName);

                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    return Json(new { ErrorMessage = errorMessage });
                }
            }

            return null;
        }

        protected void SetViewBagErrorByTempData()
        {
            if (!string.IsNullOrWhiteSpace((string)TempData["ErrorMessage"]))
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
                TempData.Remove("ErrorMessage");
            }
        }

        #region Private Methods

        #endregion
    }
}