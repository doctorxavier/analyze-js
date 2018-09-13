using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;

using IDB.Architecture.Language;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.IndicatorsModuleNew.Services.Core;
using IDB.MW.Application.IndicatorsModuleNew.ViewModels.Core;
using IDB.MW.Business.Core.SharepointSecurityService;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.MW.Infrastructure.Caching;
using IDB.MW.Infrastructure.Caching.Contracts;
using IDB.MW.Infrastructure.SecurityService;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.MW.Infrastructure.SecurityService.Enums;
using IDB.Presentation.MVC4.Areas.Indicators.Models;
using IDB.Presentation.MVC4.General;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Domain.Session;
using IDB.MW.Infrastructure.Configuration;

namespace IDB.Presentation.MVC4.Areas.Indicators.Controllers
{
    public abstract partial class BaseController : MVC4.Controllers.ConfluenceController
    {
        protected const string FRIENDLY_PERMISSION_ERROR = "IM.CommonMesssage.NoPermission";
        protected const string STATUS_MODIFIED = "modified";
        protected const string STATUS_CREATED = "created";

        protected readonly string ControllerName;
        protected readonly string FilterCacheKey;
        protected readonly List<SelectListItem> activeOptions = null;

        private const string FILTER_CACHE_KEY = "Indicator-Filter-{0}";

        private readonly IAuthorizationService _authorizationService;
        private readonly IEnumMappingService _enumMappingService;
        private readonly ICacheStorageService _cacheStorageService;
        private readonly string FormatDate;

        #region Constructor

        protected BaseController(IAuthorizationService authorizationService,
                                 IEnumMappingService enumMappingService,
                                 string controllerName)
        {
            _authorizationService = AuthorizationServiceFactory.Current;
            _enumMappingService = enumMappingService;
            ControllerName = controllerName;
            FilterCacheKey = string.Format(FILTER_CACHE_KEY, controllerName);
            _cacheStorageService = CacheStorageFactory.Current;
            activeOptions = new List<SelectListItem>()
                {
                    new SelectListItem()
                    { 
                        Text = Localization.GetText("COMMON.All"),
                        Value = string.Empty
                    },
                    new SelectListItem()
                    { 
                        Text = Localization.GetText("COMMON.Active"),
                        Value = "Active"
                    },
                    new SelectListItem()
                    { 
                        Text = Localization.GetText("COMMON.Inactive"),
                        Value = "Inactive"
                    }
                };

            FormatDate = ConfigurationServiceFactory.Current.GetApplicationSettings().FormatDate;
        }

        #endregion

        public virtual ActionResult SetMockGlobalPermission(ActionEnum[] actions = null)
        {
            _cacheStorageService.Remove("AuthorizationInfo");

            var auth = new AuthorizationOperationInfo
            {
                ActionList = new List<ActionEnum>(),
                RoleList = new List<RoleEnum>(),
                UserName = IDBContext.Current.UserLoginName
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

        public virtual ActionResult SetIndicatosFilter(IndicatorsFilterViewModel filter, bool isClear = false)
        {
            if (isClear)
            {
                _cacheStorageService.Remove(FilterCacheKey);
            }
            else
            {
                filter.IsActive = filter.IsActive ?? string.Empty;
                _cacheStorageService.Add(FilterCacheKey, filter);
            }

            return Json(new { IsValid = true });
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

        protected string DisplayIsActiveIndicator(RowIndicatorViewModel indicator, bool isEdit = false)
        {
            var isActive = (DateTime.ParseExact(indicator.ExpirationDate, FormatDate, CultureInfo.InvariantCulture) >= DateTime.Today) ? indicator.IsActive : false;

            var result = this.RenderRazorViewToString("Display/Active", new ActiveViewModel()
            {
                IsEditMode = isEdit,
                IsActive = isActive,
                Id = indicator.IndicatorId
            });
            return result;
        }

        protected string GetUrlForIndicator(RowIndicatorViewModel indicator)
        {
            var result = this.RenderRazorViewToString("Display/Link", new LinkViewModel()
            {
                Action = "Read",
                Controller = ControllerName,
                Area = "Indicators",
                Text = indicator.IndicatorNameEn,
                Parameters = new Dictionary<string, object>() { { "indicatorId", indicator.IndicatorId } },
                EffectiveDate = DateTime.ParseExact(indicator.EffectiveDate, FormatDate, CultureInfo.InvariantCulture),
                isActive = indicator.IsActive
            });
            return result;
        }

        protected string DisplayEffectiveDate(string date, bool isActive)
        {
            string result = date;
            var dateCurrent = DateTime.ParseExact(date, FormatDate, CultureInfo.InvariantCulture);

            if (isActive && dateCurrent > DateTime.Today)
            {
                result = this.RenderRazorViewToString("Display/EffectiveDate", new EffectiveDateViewModel()
                {
                    date = dateCurrent
                });
            }

            return result;
        }

        protected IDictionary<ActionEnum, bool> SetViewBagGlobalPermission(params ActionEnum[] action)
        {
            var permissions = _authorizationService
                .IsGlobalAuthorized(IDBContext.Current.UserLoginName, "ControlPanel", action);
            ViewBag.Permissions = permissions;
            return permissions;
        }

        protected bool SetViewBagGlobalPermissionAndCheckAny(params ActionEnum[] action)
        {
            var permissions = SetViewBagGlobalPermission(action);
            var someTrue = MvcHelpers.CheckSomeRoles(permissions, action);
            return someTrue;
        }

        protected List<SelectListItem> GetTemplatesCombo(ICommonIndicatorService service)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            var templatesResponse = service.GetTemplates();
            if (templatesResponse.IsValid && templatesResponse.Templates != null)
            {
                list = templatesResponse.Templates.Select(x => new SelectListItem()
                {
                    Text = x.Version,
                    Value = x.Id.ToString()
                }).ToList();
            }

            return list;
        }

        protected ReloadHtmlContentResponse GetTechnicalFieldPartial(ICommonIndicatorService service, int templateId, string targetSelector)
        {
            var result = new ReloadHtmlContentResponse() { IsValid = true };

            var response = service.GetTechnicalFieldForTemplate(templateId);
            result.IsValid = response.IsValid;
            result.Message = response.ErrorMessage;

            var html = this.RenderRazorViewToString("EditIndicators/EditIndTechnicalFields", response.TechnicalFields);

            result.ContentToReplace.Add(targetSelector, html);

            return result;
        }
    }
}