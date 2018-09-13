using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.DataTables.DataTable.ServerSide;
using IDB.Architecture.Language;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Application.IndicatorsModuleNew.Enums;
using IDB.MW.Application.IndicatorsModuleNew.Mappers;
using IDB.MW.Application.IndicatorsModuleNew.Messages;
using IDB.MW.Application.IndicatorsModuleNew.Service.StandardizedOutput;
using IDB.MW.Application.IndicatorsModuleNew.ViewModels.Core;
using IDB.MW.Application.IndicatorsModuleNew.ViewModels.Core.EditIndicators;
using IDB.MW.Application.IndicatorsModuleNew.ViewModels.Core.EditTemplates;
using IDB.MW.Application.IndicatorsModuleNew.ViewModels.StandardizedOutput;
using IDB.MW.Application.TCAbstractModule.Enums;
using IDB.MW.Business.TCM.Contracts;
using IDB.MW.Infrastructure.Caching;
using IDB.MW.Infrastructure.Caching.Contracts;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.MW.Infrastructure.SecurityService.Enums;
using IDB.Presentation.MVC4.Areas.Indicators.Mappers;
using IDB.Presentation.MVC4.Areas.Indicators.Models;
using IDB.Presentation.MVC4.General;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.Indicators.Controllers
{
    public partial class StandardizedController : BaseController
    {
        #region Constants
        private const string CONTROLLER_NAME = "Standardized";
        #endregion

        #region Fields
        private readonly IAuthorizationService _authorizationService;
        private readonly IEnumMappingService _enumMappingService;
        private readonly IStandardizedOutputService _standardizedOutputService;
        private readonly ICacheStorageService _cacheService;
        private readonly ICatalogService _catalogService;
        private readonly ICommonTCMService _commonTCMService;
        #endregion

        #region Contructors

        public StandardizedController(IAuthorizationService authorizationService,
            IStandardizedOutputService standardizedOutputService,
            ICatalogService catalogService,
            IEnumMappingService enumMappingService,
            ICommonTCMService commonTCMService)
            : base(authorizationService, enumMappingService, CONTROLLER_NAME)
        {
            _authorizationService = authorizationService;
            _enumMappingService = enumMappingService;
            _standardizedOutputService = standardizedOutputService;
            _cacheService = CacheStorageFactory.Current;
            _catalogService = catalogService;
            _commonTCMService = commonTCMService;
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

            var response = _standardizedOutputService.GetIndicators(request);

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

            var response = _standardizedOutputService.SaveIndicators(viewModel.Rows);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            var url = Url.Action("RedirectToIndicatorsPage", base.ControllerName, new { area = "Indicators" });
            responseView.UrlRedirect = url;

            return Json(responseView);
        }

        public virtual FileResult IndicatorsPageExportToPDF()
        {
            var filter = _cacheService.Get<IndicatorsFilterViewModel>(base.FilterCacheKey);
            var response = _standardizedOutputService.ExportIndicatorsToPDF(filter.ConvertToDataTableRequest());

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
            var response = _standardizedOutputService.ExportIndicatorsToExcel(request);

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
            var response = _standardizedOutputService.CheckIndicatorValues(indicatorNameEn,
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
            var model = GetIndicator(indicatorId, true);

            return View(model);
        }

        public virtual ActionResult Edit(int indicatorId = 0)
        {
            var model = GetIndicator(indicatorId, false);
            if (model != null && model.Indicator.Common.ExpirationDate == null)
            {
                model.Indicator.Common.ExpirationDate = DateTime.Parse("12/31/9999");
            }

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            return View(model);
        }

        public virtual ActionResult SaveIndicator(string saveMode)
        {
            var responseView = new SaveResponse() { IsValid = true };

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<EditIndicatorPageViewModel<StandardizedBasicDataViewModel>>(jsonDataRequest.SerializedData);
            var formData = jsonDataRequest.ClientFieldData;

            viewModel.UpdateEditIndicatorPageViewModelStandardized(formData);

            var response = _standardizedOutputService.SaveIndicator(viewModel.Indicator);
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

        public virtual ActionResult ReloadDataByTemplate(int templateId = 0)
        {
            var contentResponse = base.GetTechnicalFieldPartial(_standardizedOutputService, templateId, "[data-section=\"technicalFields\"]");

            var attributesResponse = _standardizedOutputService.GetAttributesForTemplate(templateId);
            if (attributesResponse.IsValid)
            {
                contentResponse.IsValid = contentResponse.IsValid && attributesResponse.IsValid;
                if (!string.IsNullOrWhiteSpace(attributesResponse.ErrorMessage))
                {
                    contentResponse.Message = attributesResponse.ErrorMessage;
                }

                List<AttributeCategoryViewModel> simpleLevelList = null;

                var emptyList = new List<SelectListItem>().Cast<object>().ToList();

                var dropdownModel = new DropDownModel((string)null, name: "businessLineId", optionList: emptyList, showEmptyOption: true, required: true);
                if (attributesResponse.SimpleLevelAttributes.ContainsKey(IndicatorAttributeTypeEnum.BusinessLine))
                {
                    simpleLevelList = attributesResponse.SimpleLevelAttributes[IndicatorAttributeTypeEnum.BusinessLine];
                    var list = simpleLevelList.ConvertToSelectListItem().Cast<object>().ToList();
                    dropdownModel = new DropDownModel((string)null, name: "businessLineId", optionList: list, showEmptyOption: true, required: true);
                }

                var html = this.RenderRazorViewToString("Display/Dropdown", dropdownModel);
                contentResponse.ContentToReplace.Add("[data-section=\"businessLine\"]", html);

                dropdownModel = new DropDownModel((string)null, name: "outputGroupId", optionList: emptyList, showEmptyOption: true, required: true);
                if (attributesResponse.SimpleLevelAttributes.ContainsKey(IndicatorAttributeTypeEnum.OutputGroup))
                {
                    simpleLevelList = attributesResponse.SimpleLevelAttributes[IndicatorAttributeTypeEnum.OutputGroup];
                    var list = simpleLevelList.ConvertToSelectListItem().Cast<object>().ToList();
                    dropdownModel = new DropDownModel((string)null, name: "outputGroupId", optionList: list, showEmptyOption: true, required: true);
                }

                html = this.RenderRazorViewToString("Display/Dropdown", dropdownModel);
                contentResponse.ContentToReplace.Add("[data-section=\"outputGroup\"]", html);
            }

            return Json(contentResponse);
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

            var response = _standardizedOutputService.SaveTemplates(viewModel.Templates);
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
            var viewModel = PageSerializationHelper.DeserializeObject<EditTemplatePageViewModel<EditTemplateStandardizedViewModel>>(jsonDataRequest.SerializedData);
            var formData = jsonDataRequest.ClientFieldData;

            viewModel.UpdateTemplate(formData);

            var response = _standardizedOutputService.SaveTemplate(viewModel.Template);
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

        #region private
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
                    var response = _standardizedOutputService.GetIndicators(request);

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

                var response = _standardizedOutputService.GetTemplates();
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

            EditTemplatePageViewModel<EditTemplateStandardizedViewModel> result = null;
            if (havePermission)
            {
                result = new EditTemplatePageViewModel<EditTemplateStandardizedViewModel>()
                {
                    ControllerName = base.ControllerName
                };

                var response = _standardizedOutputService.GetTemplate(templateId);

                if (response.IsValid)
                {
                    result.Template = (EditTemplateViewModel<EditTemplateStandardizedViewModel>)response.Template;
                }

                SetViewBagErrorMessageInvalidResponse(response);
            }
            else
            {
                ViewBag.ErrorMessage = Localization.GetText(FRIENDLY_PERMISSION_ERROR);
            }

            return result;
        }

        private EditIndicatorPageViewModel<StandardizedBasicDataViewModel> GetIndicator(int indicatorId, bool isRead)
        {
            var havePermission = SetViewBagGlobalPermissionAndCheckAny(ActionEnum.GCMIndicatorWritePermission, ActionEnum.GCMIndicatorTemplateWritePermission);

            EditIndicatorPageViewModel<StandardizedBasicDataViewModel> result = null;
            if (havePermission)
            {
                result = new EditIndicatorPageViewModel<StandardizedBasicDataViewModel>()
                {
                    ControllerName = base.ControllerName
                };

                var response = (GetIndicatorResponse<StandardizedBasicDataViewModel>)_standardizedOutputService.GetIndicator(indicatorId, isRead);
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

        private void SetViewBagEditIndicator(
            EditIndicatorPageViewModel<StandardizedBasicDataViewModel> model)
        {
            ViewBag.indicatorTemplateList = base.GetTemplatesCombo(_standardizedOutputService);
            ViewBag.UnitMeasureList = new List<SelectListItem>();
            ViewBag.BusinessLineList = new List<SelectListItem>();
            ViewBag.OutputGroupList = new List<SelectListItem>();
            ViewBag.ThemeList = new List<MultiDropDownItem>();
            ViewBag.FundsIndByTheme = new Dictionary<int, List<Tuple<string, List<MultiDropDownItem>>>>();

            var attributesResponse = _standardizedOutputService
                .GetAttributesForTemplate(model.Indicator.Common.TemplateVersionId);

            if (attributesResponse.IsValid)
            {
                List<AttributeCategoryViewModel> simpleLevelList = null;
                if (attributesResponse.SimpleLevelAttributes.ContainsKey(IndicatorAttributeTypeEnum.BusinessLine))
                {
                    simpleLevelList = attributesResponse.SimpleLevelAttributes[IndicatorAttributeTypeEnum.BusinessLine];
                    ViewBag.BusinessLineList = simpleLevelList.ConvertToSelectListItem();
                }

                if (attributesResponse.SimpleLevelAttributes.ContainsKey(IndicatorAttributeTypeEnum.OutputGroup))
                {
                    simpleLevelList = attributesResponse.SimpleLevelAttributes[IndicatorAttributeTypeEnum.OutputGroup];
                    ViewBag.OutputGroupList = simpleLevelList.ConvertToSelectListItem();
                }
            }

            this.SetViewBagListFromCatalog(
                _catalogService,
                new Dictionary<ConvergenceMasterDataTypeEnum, string>
                {
                    { ConvergenceMasterDataTypeEnum.UnitMeasureCGM, "UnitMeasureList" },
                });

            var responseThemes = _commonTCMService.GetOutputThemes(model.Indicator.Specific.ThemeIds)
                .Select(x => new MasterDataViewModel
                {
                    MasterId = x.ConvergenceMasterDataId,
                    NameEn = x.NameEn,
                    NameEs = x.NameEs,
                    NameFr = x.NameFr,
                    NamePt = x.NamePt
                })
                .ToList();

            ViewBag.ThemeList = responseThemes.ConvertToMultiDropDownItems();

            var themesIds = responseThemes.Select(x => x.MasterId);
            var fundByThemeResponse = _standardizedOutputService
                .GetIndicatorsAndFundByTheme(themesIds.ToArray());

            if (fundByThemeResponse.IsValid)
            {
                ViewBag.FundsIndByTheme = ConvertToMultiDropDownItem(fundByThemeResponse.FundsByTheme);
            }
        }

        private Dictionary<int, List<Tuple<string, List<MultiDropDownItem>>>> ConvertToMultiDropDownItem(Dictionary<int, List<Tuple<string, List<SelectListItem>>>> models)
        {
            var result = new Dictionary<int, List<Tuple<string, List<MultiDropDownItem>>>>();

            foreach (var model in models)
            {
                var list = new List<Tuple<string, List<MultiDropDownItem>>>();
                result.Add(model.Key, list);

                foreach (var tuple in model.Value)
                {
                    var transform = tuple.Item2.ConvertToMultiDropDownItems();
                    var newTuple = new Tuple<string, List<MultiDropDownItem>>(tuple.Item1, transform);
                    list.Add(newTuple);
                }
            }

            return result;
        }

        private void SetViewBagIndicatorsPage()
        {
            ViewBag.ActiveOptions = activeOptions;
            ViewBag.TemplateVersionList = base.GetTemplatesCombo(_standardizedOutputService);
        }
        #endregion
    }
}