using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.DataTables.DataTable.ServerSide;
using IDB.Architecture.Language;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Application.IndicatorsModuleNew.Messages;
using IDB.MW.Application.IndicatorsModuleNew.Service.Fund;
using IDB.MW.Application.IndicatorsModuleNew.ViewModels.Core;
using IDB.MW.Application.IndicatorsModuleNew.ViewModels.Core.EditIndicators;
using IDB.MW.Application.IndicatorsModuleNew.ViewModels.Core.EditTemplates;
using IDB.MW.Application.IndicatorsModuleNew.ViewModels.Fund;
using IDB.MW.Application.TCAbstractModule.Enums;
using IDB.MW.Infrastructure.Caching;
using IDB.MW.Infrastructure.Caching.Contracts;
using IDB.MW.Infrastructure.Helpers;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.MW.Infrastructure.SecurityService.Enums;
using IDB.Presentation.MVC4.Areas.Indicators.Mappers;
using IDB.Presentation.MVC4.Areas.Indicators.Models;
using IDB.Presentation.MVC4.General;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.Indicators.Controllers
{
    public partial class FundController : BaseController
    {
        #region Constants
        private const string CONTROLLER_NAME = "Fund";
        #endregion

        #region Fields
        private readonly IAuthorizationService _authorizationService;
        private readonly IEnumMappingService _enumMappingService;
        private readonly IFundService _fundService;
        private readonly ICacheStorageService _cacheService;
        private readonly ICatalogService _catalogService;
        #endregion

        #region Contructors
        public FundController(IAuthorizationService authorizationService,
            IFundService fundService,
            ICatalogService catalogService,
            IEnumMappingService enumMappingService)
            : base(authorizationService, enumMappingService, CONTROLLER_NAME)
        {
            _authorizationService = authorizationService;
            _enumMappingService = enumMappingService;
            _fundService = fundService;
            _cacheService = CacheStorageFactory.Current;
            _catalogService = catalogService;
        }
        #endregion

        #region Indicator List
        public virtual ActionResult IndicatorsPage(string status = null)
        {
            var filterCache = _cacheService.Get<IndicatorsFilterViewModel>(base.FilterCacheKey);
            if (filterCache == null)
            {
                filterCache = new IndicatorsFilterViewModel { IsActive = "Active" };
            }

            if (status == STATUS_CREATED)
            {
                ViewBag.GreenMessage = Localization.GetText("IM.EditReadIndicator.SaveAndClose.Message");
            }
            else if (status == STATUS_MODIFIED)
            {
                ViewBag.GreenMessage = Localization.GetText("IM.EditReadIndicator.SaveAndClose.ModifiedMessage");
            }

            var model = GetIndicatorsPageViewModel(filterCache);
            return View(model);
        }

        public virtual ActionResult IndicatorsPageEdit()
        {
            var filterCache = _cacheService.Get<IndicatorsFilterViewModel>(base.FilterCacheKey);
            if (filterCache == null)
            {
                filterCache = new IndicatorsFilterViewModel { IsActive = "Active" };
            }

            var model = GetIndicatorsPageViewModel(filterCache, isEdit: true);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            return View(model);
        }

        public virtual JsonResult IndicatorsPageSearch([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, bool isEdit = false)
        {
            var request = requestModel.ConvertToDataTableRequestViewModel();
            var filter = request.ConvertToIndicatorsFilterViewModel();
            _cacheService.Add(base.FilterCacheKey, filter);

            var response = _fundService.GetIndicators(request);

            if (!response.IsValid)
            {
                response.Indicators = new RowsFilteredViewModel<RowIndicatorViewModel>()
                {
                    TotalElements = 0,
                    Rows = new List<RowIndicatorViewModel>()
                };
            }

            var result = from c in response.Indicators.Rows
                         select new[] 
            { 
                c.IndicatorNumber,
                GetUrlForIndicator(c),
                c.TemplateVersion,
                DisplayEffectiveDate(c.EffectiveDate, c.IsActive),
                c.ExpirationDate,
                DisplayIsActiveIndicator(c, isEdit)
            };

            var jsonResponse = new DataTablesResponse(requestModel.Draw, result, response.Indicators.TotalElements, response.Indicators.TotalElements);
            return Json(jsonResponse, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult SaveIndicatorList()
        {
            var responseView = new SaveResponse() { IsValid = true };

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<IndicatorsPageViewModel>(jsonDataRequest.SerializedData);
            var formData = jsonDataRequest.ClientFieldData;

            viewModel.UpdateIndicatorsPageViewModel(formData);

            var response = _fundService.SaveIndicators(viewModel.Rows);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            var url = Url.Action("RedirectToIndicatorsPage", base.ControllerName, new { area = "Indicators" });
            responseView.UrlRedirect = url;

            return Json(responseView);
        }

        public virtual FileResult IndicatorsPageExportToPDF()
        {
            var filter = _cacheService.Get<IndicatorsFilterViewModel>(base.FilterCacheKey);
            var response = _fundService.ExportIndicatorsToPDF(filter.ConvertToDataTableRequest());

            if (!response.IsValid)
            {
                return null;
            }

            return File(response.File, "application/doc", "IndicatorsList.pdf");
        }

        public virtual FileResult IndicatorsPageExportToExcel()
        {
            var filter = _cacheService.Get<IndicatorsFilterViewModel>(base.FilterCacheKey);
            var request = filter.ConvertToDataTableRequest();
            var response = _fundService.ExportIndicatorsToExcel(request);

            if (!response.IsValid)
            {
                return null;
            }

            return File(response.File, "application/xls", "IndicatorsList.xls");
        }

        public virtual ActionResult RedirectToIndicatorsPage(bool isEdit = false, string status = null)
        {
            if (isEdit)
            {
                return RedirectToAction("IndicatorsPageEdit", base.ControllerName);
            }

            return RedirectToAction("IndicatorsPage", base.ControllerName, new { status = status });
        }

        public virtual JsonResult ValidateIndicatorNameNumber(string indicatorNameEn,
            string indicatorNameEs,
            string indicatorNameFr,
            string indicatorNamePt,
            string indicatorNumber,
            string indicatorId)
        {
            var response = _fundService.CheckIndicatorValues(indicatorNameEn,
                indicatorNameEs,
                indicatorNameFr,
                indicatorNamePt,
                indicatorNumber,
                indicatorId);

            return Json(response);
        }
        #endregion

        #region Indicator
        public virtual ActionResult Read(int indicatorId)
        {
            var model = GetIndicator(indicatorId);

            return View(model);
        }

        public virtual ActionResult Edit(int indicatorId = 0)
        {
            var model = GetIndicator(indicatorId);
            if (model != null && model.Indicator.Common.ExpirationDate == null)
            {
                model.Indicator.Common.ExpirationDate = DateTime.Parse("12/31/9999");
            }

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            return View(model);
        }

        public virtual ActionResult ReloadDataByTemplate(int templateId = 0)
        {
            var contentResponse = base.GetTechnicalFieldPartial(_fundService, templateId, "[data-section=\"technicalFields\"]");

            return Json(contentResponse);
        }

        public virtual ActionResult SaveIndicator(string saveMode)
        {
            var responseView = new SaveResponse() { IsValid = true };

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<EditIndicatorPageViewModel<FundBasicDataViewModel>>(jsonDataRequest.SerializedData);
            var formData = jsonDataRequest.ClientFieldData;

            viewModel.UpdateEditIndicatorPageViewModelFund(formData);

            var response = _fundService.SaveIndicator(viewModel.Indicator);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if ((saveMode == "save") && (response.IndicatorId != 0))
            {
                var url = Url.Action("Read", base.ControllerName, new { area = "Indicators", indicatorId = response.IndicatorId });
                responseView.UrlRedirect = url;
            }
            else if (response.IndicatorId != 0)
            {
                var status = STATUS_CREATED;
                if (viewModel.Indicator.Common.IndicatorId != 0)
                {
                    status = STATUS_MODIFIED;
                }

                var url = Url.Action("RedirectToIndicatorsPage", base.ControllerName, new { area = "Indicators", status = status });
                responseView.UrlRedirect = url;
            }

            return Json(responseView);
        }

        #endregion

        #region Template List
        public virtual ActionResult TemplatesPage(string status = null)
        {
            var model = GetTemplatesPageViewModel();

            if (status == STATUS_CREATED)
            {
                ViewBag.GreenMessage = Localization.GetText("IM.EditReadTemplates.SaveAndClose.Message");
            }
            else if (status == STATUS_MODIFIED)
            {
                ViewBag.GreenMessage = Localization.GetText("IM.EditReadTemplates.SaveAndClose.ModifiedMessage");
            }

            return View(model);
        }

        public virtual ActionResult TemplatesPageEdit()
        {
            var model = GetTemplatesPageViewModel();
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            return View(model);
        }

        public virtual ActionResult SaveTemplateList()
        {
            var responseView = new SaveResponse() { IsValid = true };

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<TemplatePageViewModel>(jsonDataRequest.SerializedData);
            var formData = jsonDataRequest.ClientFieldData;

            viewModel.UpdateTemplateListPage(formData);

            var response = _fundService.SaveTemplates(viewModel.Templates);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if (responseView.IsValid)
            {
                var url = Url.Action("TemplatesPage", base.ControllerName, new { area = "Indicators" });
                responseView.UrlRedirect = url;
            }

            return Json(responseView);
        }
        #endregion

        #region Edit Template
        public virtual ActionResult EditTemplate(int templateId = 0)
        {
            var model = GetTemplateViewModel(templateId);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            return View(model);
        }

        public virtual ActionResult ReadTemplate(int templateId)
        {
            var model = GetTemplateViewModel(templateId);

            return View(model);
        }

        public virtual ActionResult SaveTemplate(string saveMode)
        {
            var responseView = new SaveResponse() { IsValid = true };

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<EditTemplatePageViewModel>(jsonDataRequest.SerializedData);
            var formData = jsonDataRequest.ClientFieldData;

            viewModel.UpdateTemplate(formData);

            var response = _fundService.SaveTemplate(viewModel.Template);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if ((saveMode == "save") && (response.TemplateId != 0))
            {
                var url = Url.Action("ReadTemplate", base.ControllerName, new { area = "Indicators", templateId = response.TemplateId });
                responseView.UrlRedirect = url;
            }
            else if (response.TemplateId != 0)
            {
                var status = STATUS_CREATED;
                if (viewModel.Template.Common.TemplateId != 0)
                {
                    status = STATUS_MODIFIED;
                }

                var url = Url.Action("TemplatesPage", base.ControllerName, new { area = "Indicators", status = status });
                responseView.UrlRedirect = url;
            }

            return Json(responseView);
        }

        #endregion

        #region Private Methods
        
        private IndicatorsPageViewModel GetIndicatorsPageViewModel(IndicatorsFilterViewModel filter, bool isEdit = false)
        {
            var havePermission = SetViewBagGlobalPermissionAndCheckAny(ActionEnum.GCMIndicatorWritePermission, ActionEnum.GCMIndicatorTemplateWritePermission);

            IndicatorsPageViewModel result = null;
            if (havePermission)
            {
                result = new IndicatorsPageViewModel()
                {
                    ControllerName = base.ControllerName
                };

                if (filter != null)
                {
                    result.Filter = filter;
                }

                if (isEdit)
                {
                    var request = filter.ConvertToDataTableRequest();
                    var response = _fundService.GetIndicators(request);

                    SetViewBagErrorMessageInvalidResponse(response);
                    result.Rows = response.Indicators.Rows.ToList();
                }

                SetViewBagIndicatorsPage();
            }
            else
            {
                ViewBag.ErrorMessage = Localization.GetText(FRIENDLY_PERMISSION_ERROR);
            }

            return result;
        }

        private void SetViewBagIndicatorsPage()
        {
            ViewBag.ActiveOptions = activeOptions;
            ViewBag.TemplateVersionList = base.GetTemplatesCombo(_fundService);
        }

        private EditIndicatorPageViewModel<FundBasicDataViewModel> GetIndicator(int indicatorId)
        {
            var havePermission = SetViewBagGlobalPermissionAndCheckAny(ActionEnum.GCMIndicatorWritePermission, ActionEnum.GCMIndicatorTemplateWritePermission);

            EditIndicatorPageViewModel<FundBasicDataViewModel> result = null;

            if (havePermission)
            {
                result = new EditIndicatorPageViewModel<FundBasicDataViewModel>()
                {
                    ControllerName = base.ControllerName
                };

                var response = (GetIndicatorResponse<FundBasicDataViewModel>)_fundService.GetIndicator(indicatorId);
                if (response.IsValid)
                {
                    result.Indicator = response.Indicator;
                }

                SetViewBagErrorMessageInvalidResponse(response);
                SetViewBagEditIndicator(result);
            }
            else
            {
                ViewBag.ErrorMessage = Localization.GetText(FRIENDLY_PERMISSION_ERROR);
            }

            return result;
        }

        private void SetViewBagEditIndicator(EditIndicatorPageViewModel<FundBasicDataViewModel> model)
        {
            ViewBag.UnitOfMeasureList = new List<SelectListItem>();
            ViewBag.FundList = new List<SelectListItem>();
            ViewBag.ThemeList = new List<MultiDropDownItem>();

            this.SetViewBagListFromCatalog(_catalogService,
                new Dictionary<ConvergenceMasterDataTypeEnum, string>
                {
                    { ConvergenceMasterDataTypeEnum.UnitMeasureCGM, "UnitOfMeasureList" },
                });

            var responseThemes = _catalogService.GetMasterDataListByTypeCode(false, ConvergenceMasterDataTypeEnum.IndicatorTheme.GetEnumDescription());
            if (responseThemes.MasterDataCollection == null)
            {
                responseThemes.MasterDataCollection = new List<MasterDataViewModel>();
            }

            ViewBag.ThemeList = responseThemes.MasterDataCollection.ConvertToMultiDropDownItems();

            var responseFund = _fundService.GetFunds();
            if (responseFund.IsValid)
            {
                ViewBag.FundList = responseFund.Funds;
            }

            ViewBag.indicatorTemplateList = base.GetTemplatesCombo(_fundService);
        }

        private TemplatePageViewModel GetTemplatesPageViewModel()
        {
            var havePermission = SetViewBagGlobalPermissionAndCheckAny(ActionEnum.GCMIndicatorTemplateWritePermission);

            TemplatePageViewModel result = null;
            if (havePermission)
            {
                result = new TemplatePageViewModel()
                {
                    ControllerName = base.ControllerName
                };

                var response = _fundService.GetTemplates();
                SetViewBagErrorMessageInvalidResponse(response);
                result.Templates = response.Templates;
            }
            else
            {
                ViewBag.ErrorMessage = Localization.GetText(FRIENDLY_PERMISSION_ERROR);
            }

            return result;
        }

        private EditTemplatePageViewModel GetTemplateViewModel(int templateId)
        {
            var havePermission = SetViewBagGlobalPermissionAndCheckAny(ActionEnum.GCMIndicatorTemplateWritePermission);

            EditTemplatePageViewModel result = null;
            if (havePermission)
            {
                result = new EditTemplatePageViewModel()
                {
                    ControllerName = base.ControllerName
                };

                var response = _fundService.GetTemplate(templateId);

                if (response.IsValid)
                {
                    result.Template = response.Template;
                }

                SetViewBagErrorMessageInvalidResponse(response);
            }
            else
            {
                ViewBag.ErrorMessage = Localization.GetText(FRIENDLY_PERMISSION_ERROR);
            }

            return result;
        }
        #endregion
    }
}