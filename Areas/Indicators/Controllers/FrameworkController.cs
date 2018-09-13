using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.DataTables.DataTable.ServerSide;
using IDB.Architecture.Language;
using IDB.Architecture.Logging;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.IndicatorsModuleNew.Enums;
using IDB.MW.Application.IndicatorsModuleNew.Mappers;
using IDB.MW.Application.IndicatorsModuleNew.Messages;
using IDB.MW.Application.IndicatorsModuleNew.Services.ResultFramework;
using IDB.MW.Application.IndicatorsModuleNew.ViewModels.Core;
using IDB.MW.Application.IndicatorsModuleNew.ViewModels.Core.EditIndicators;
using IDB.MW.Application.IndicatorsModuleNew.ViewModels.Core.EditTemplates;
using IDB.MW.Application.IndicatorsModuleNew.ViewModels.Framework;
using IDB.MW.Application.TCAbstractModule.Enums;
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
    public partial class FrameworkController : BaseController
    {
        #region Constants
        private const string CONTROLLER_NAME = "Framework";
        #endregion

        #region Fields
        private readonly IAuthorizationService _authorizationService;
        private readonly IEnumMappingService _enumMappingService;
        private readonly IResultFrameworkService _resultFrameworkService;
        private readonly ICacheStorageService _cacheService;
        private readonly ICatalogService _catalogService;
        #endregion

        #region Contructors

        public FrameworkController(IAuthorizationService authorizationService,
            IResultFrameworkService resultFrameworkService,
            ICatalogService catalogService,
            IEnumMappingService enumMappingService)
            : base(authorizationService, enumMappingService, CONTROLLER_NAME)
        {
            _authorizationService = authorizationService;
            _enumMappingService = enumMappingService;
            _resultFrameworkService = resultFrameworkService;
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

            var model = GetIndicatorsPageViewModel(filterCache);

            if (status == STATUS_CREATED)
            {
                ViewBag.GreenMessage = Localization.GetText("IM.EditReadIndicator.SaveAndClose.Message");
            }
            else if (status == STATUS_MODIFIED)
            {
                ViewBag.GreenMessage = Localization.GetText("IM.EditReadIndicator.SaveAndClose.ModifiedMessage");
            }

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

            var response = _resultFrameworkService.GetIndicators(request);

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

            var response = _resultFrameworkService.SaveIndicators(viewModel.Rows);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            var url = Url.Action("RedirectToIndicatorsPage", base.ControllerName, new { area = "Indicators" });
            responseView.UrlRedirect = url;

            return Json(responseView);
        }

        public virtual FileResult IndicatorsPageExportToPDF()
        {
            var filter = _cacheService.Get<IndicatorsFilterViewModel>(base.FilterCacheKey);
            var response = _resultFrameworkService.ExportIndicatorsToPDF(filter.ConvertToDataTableRequest());

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
            var response = _resultFrameworkService.ExportIndicatorsToExcel(request);

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
            var response = _resultFrameworkService.CheckIndicatorValues(indicatorNameEn,
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
            SetViewBag();
            return View(model);
        }

        public virtual ActionResult Edit(int indicatorId = 0)
        {
            var model = GetIndicator(indicatorId);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            SetViewBag();
            return View(model);
        }

        public virtual ActionResult SaveIndicator(string saveMode)
        {
            var responseView = new SaveResponse() { IsValid = true };

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<EditIndicatorPageViewModel<FWBasicDataViewModel>>(jsonDataRequest.SerializedData);
            var formData = jsonDataRequest.ClientFieldData;

            viewModel.UpdateEditIndicatorPageViewModelFramework(formData);

            var response = _resultFrameworkService.SaveIndicator(viewModel.Indicator);
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
            MultiDropDownModel multiSelect;
            var contentResponse = base.GetTechnicalFieldPartial(_resultFrameworkService, templateId, "[data-section=\"technicalFields\"]");

            var attributesResponse = _resultFrameworkService.GetAttributesForTemplate(templateId);
            if (attributesResponse.IsValid)
            {
                contentResponse.IsValid = contentResponse.IsValid && attributesResponse.IsValid;
                if (!string.IsNullOrWhiteSpace(attributesResponse.ErrorMessage))
                {
                    contentResponse.Message = attributesResponse.ErrorMessage;
                }

                List<AttributeCategoryViewModel> simpleLevelList = null;
                List<AttributeLevelViewModel> multiLevelList = null;
                var dropdownModel = new DropDownModel((string)null, name: "typeCRFId", optionList: new List<SelectListItem>().Cast<object>().ToList(), showEmptyOption: true, required: true);
                if (attributesResponse.SimpleLevelAttributes.ContainsKey(IndicatorAttributeTypeEnum.TypeCRF))
                {
                    simpleLevelList = attributesResponse.SimpleLevelAttributes[IndicatorAttributeTypeEnum.TypeCRF];
                    var list = simpleLevelList.ConvertToSelectListItem().Cast<object>().ToList();
                    dropdownModel = new DropDownModel((string)null, name: "typeCRFId", optionList: list, showEmptyOption: true, required: true);
                }

                var html = this.RenderRazorViewToString("Display/Dropdown", dropdownModel);
                contentResponse.ContentToReplace.Add("[data-section=\"typeCRF\"]", html);

                var type = GetUOMMasterTypeByTemplate(templateId);
                this.SetViewBagListFromCatalog(
                _catalogService, new Dictionary<ConvergenceMasterDataTypeEnum, string>
                {
                    { type, "UnitMeasureList" },
                });

                var elementos = ((IEnumerable<SelectListItem>)this.ViewBag.UnitMeasureList).Cast<object>().ToList();

                var dropdownUOM = new DropDownModel((string)null, name: "unitMeasure", optionList: elementos, showEmptyOption: true, required: true);

                var html2 = this.RenderRazorViewToString("Display/Dropdown", dropdownUOM);
                contentResponse.ContentToReplace.Add("[data-section=\"unitMeasure\"]", html2);

                multiLevelList = new List<AttributeLevelViewModel>();
                if (attributesResponse.MultiLevelAttributes.ContainsKey(IndicatorAttributeTypeEnum.PriorityArea))
                {
                    multiLevelList = attributesResponse.MultiLevelAttributes[IndicatorAttributeTypeEnum.PriorityArea];
                }

                multiSelect = new MultiDropDownModel().Name("priorityAreaId")
                            .Items(multiLevelList.ConvertToMultiDropDownItems())
                            .CanSelectGroup();
                contentResponse.ContentToReplace.Add("[data-section=\"priorityArea\"]", multiSelect.ToHtmlString());

                multiLevelList = new List<AttributeLevelViewModel>();
                if (attributesResponse.MultiLevelAttributes.ContainsKey(IndicatorAttributeTypeEnum.Disaggregation))
                {
                    multiLevelList = attributesResponse.MultiLevelAttributes[IndicatorAttributeTypeEnum.Disaggregation];
                }

                multiSelect = new MultiDropDownModel().Name("disaggregationId")
                            .Items(multiLevelList.ConvertToMultiDropDownItems())
                            .CanSelectGroup();
                contentResponse.ContentToReplace.Add("[data-section=\"disaggregation\"]", multiSelect.ToHtmlString());
            }

            return Json(contentResponse);
        }

        #endregion

        #region Template List
        public virtual ActionResult TemplatesPage(string status = null)
        {
            try
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
            catch (Exception e)
            {
                Logger.GetLogger().WriteError("FrameworkController", "Templates page error", e);
                return null;
            }
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

            var response = _resultFrameworkService.SaveTemplates(viewModel.Templates);
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
            var viewModel = PageSerializationHelper.DeserializeObject<EditTemplatePageViewModel<EditTemplateFrameworkViewModel>>(jsonDataRequest.SerializedData);
            var formData = jsonDataRequest.ClientFieldData;

            viewModel.UpdateTemplate(formData);

            var response = _resultFrameworkService.SaveTemplate(viewModel.Template);
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

        private ConvergenceMasterDataTypeEnum GetUOMMasterTypeByTemplate(int idTemplate)
        {
            var templateActual = _resultFrameworkService.GetTemplate(idTemplate);

            return
                templateActual.Template.Common.Description.IndexOf("2012") != -1 ?
                ConvergenceMasterDataTypeEnum.UnitMeasureCRF_2012_2015
                : ConvergenceMasterDataTypeEnum.UnitMeasureCRF;
        }

        #endregion

        #region Private Methods

        private void SetViewBag()
        {
            ViewBag.Equality = _enumMappingService.GetMappingCode<INDImageAssociatedEnum>(INDImageAssociatedEnum.Equality).ToString();
            ViewBag.ClimateChange = _enumMappingService.GetMappingCode<INDImageAssociatedEnum>(INDImageAssociatedEnum.ClimateChange).ToString();
            ViewBag.FullEconomic = _enumMappingService.GetMappingCode<INDImageAssociatedEnum>(INDImageAssociatedEnum.FullEconomic).ToString();
            ViewBag.HighProductivity = _enumMappingService.GetMappingCode<INDImageAssociatedEnum>(INDImageAssociatedEnum.HighProductivity).ToString();
            ViewBag.InstCapacity = _enumMappingService.GetMappingCode<INDImageAssociatedEnum>(INDImageAssociatedEnum.InstCapacity).ToString();
            ViewBag.SocialInclusion = _enumMappingService.GetMappingCode<INDImageAssociatedEnum>(INDImageAssociatedEnum.SocialInclusion).ToString();
        }

        private IndicatorsPageViewModel GetIndicatorsPageViewModel(IndicatorsFilterViewModel filter, bool isEdit = false)
        {
            var havePermission = SetViewBagGlobalPermissionAndCheckAny(ActionEnum.FWIndicatorWritePermission, ActionEnum.FWIndicatorTemplateWritePermission);

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
                    var response = _resultFrameworkService.GetIndicators(request);

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
            ViewBag.TemplateVersionList = base.GetTemplatesCombo(_resultFrameworkService);
        }

        private EditIndicatorPageViewModel<FWBasicDataViewModel> GetIndicator(int indicatorId)
        {
            var havePermission = SetViewBagGlobalPermissionAndCheckAny(ActionEnum.FWIndicatorWritePermission, ActionEnum.FWIndicatorTemplateWritePermission);

            EditIndicatorPageViewModel<FWBasicDataViewModel> result = null;
            if (havePermission)
            {
                result = new EditIndicatorPageViewModel<FWBasicDataViewModel>()
                {
                    ControllerName = base.ControllerName
                };

                var response = (GetIndicatorResponse<FWBasicDataViewModel>)_resultFrameworkService.GetIndicator(indicatorId);
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

        private void SetViewBagEditIndicator(EditIndicatorPageViewModel<FWBasicDataViewModel> model)
        {
            ViewBag.indicatorTemplateList = base.GetTemplatesCombo(_resultFrameworkService);
            ViewBag.PriorityAreaList = new List<MultiDropDownItem>();
            ViewBag.DisaggregationList = new List<MultiDropDownItem>();
            ViewBag.TypeCRFList = new List<SelectListItem>();
            ViewBag.UnitMeasureList = new List<SelectListItem>();

            var attributesResponse = _resultFrameworkService.GetAttributesForTemplate(model.Indicator.Common.TemplateVersionId);
            if (attributesResponse.IsValid)
            {
                List<AttributeCategoryViewModel> simpleLevelList = null;
                List<AttributeLevelViewModel> multiLevelList = null;
                if (attributesResponse.SimpleLevelAttributes.ContainsKey(IndicatorAttributeTypeEnum.TypeCRF))
                {
                    simpleLevelList = attributesResponse.SimpleLevelAttributes[IndicatorAttributeTypeEnum.TypeCRF];
                    ViewBag.TypeCRFList = simpleLevelList.ConvertToSelectListItem();
                }

                if (attributesResponse.MultiLevelAttributes.ContainsKey(IndicatorAttributeTypeEnum.PriorityArea))
                {
                    multiLevelList = attributesResponse.MultiLevelAttributes[IndicatorAttributeTypeEnum.PriorityArea];
                    ViewBag.PriorityAreaList = multiLevelList.ConvertToMultiDropDownItems();
                }

                if (attributesResponse.MultiLevelAttributes.ContainsKey(IndicatorAttributeTypeEnum.Disaggregation))
                {
                    multiLevelList = attributesResponse.MultiLevelAttributes[IndicatorAttributeTypeEnum.Disaggregation];
                    ViewBag.DisaggregationList = multiLevelList.ConvertToMultiDropDownItems();
                }
            }

            var masterTypeUOM = GetUOMMasterTypeByTemplate(model.Indicator.Common.TemplateVersionId);

            this.SetViewBagListFromCatalog(
            _catalogService, new Dictionary<ConvergenceMasterDataTypeEnum, string>
            {
                { masterTypeUOM, "UnitMeasureList" },
            });
        }

        private TemplatePageViewModel GetTemplatesPageViewModel()
        {
            var havePermission = SetViewBagGlobalPermissionAndCheckAny(ActionEnum.FWIndicatorTemplateWritePermission);

            TemplatePageViewModel result = null;
            if (havePermission)
            {
                result = new TemplatePageViewModel()
                {
                    ControllerName = base.ControllerName
                };

                var response = _resultFrameworkService.GetTemplates();
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
            var havePermission = SetViewBagGlobalPermissionAndCheckAny(ActionEnum.FWIndicatorTemplateWritePermission);

            EditTemplatePageViewModel<EditTemplateFrameworkViewModel> result = null;
            if (havePermission)
            {
                result = new EditTemplatePageViewModel<EditTemplateFrameworkViewModel>()
                {
                    ControllerName = base.ControllerName
                };

                var response = _resultFrameworkService.GetTemplate(templateId);

                if (response.IsValid)
                {
                    result.Template = (EditTemplateViewModel<EditTemplateFrameworkViewModel>)response.Template;
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