using System;
using System.Collections.Generic;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.MrBlueModule.Services.Indicators;
using IDB.MW.Application.MrBlueModule.ViewModels.Indicators;
using IDB.MW.Application.TCAbstractModule.Enums;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.Presentation.MVC4.Areas.MrBlue.Models;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.MrBlue.Controllers
{
    public partial class IndicatorsController : BaseController
    {
        #region Constants

        private const string NO_WRITE_PERMISSION = "TC.TCAbstractService.NoWritePermission";
        private const string URL_INDICATORS_FORM = "/MrBlue/IndicatorsForm";

        #endregion

        #region Fields

        private readonly IIndicatorsService _indicatorsService;

        private readonly IEnumMappingService _enumMappingService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICatalogService _catalogService;

        #endregion

        #region Constructors

        public IndicatorsController(
            IIndicatorsService indicatorsService,
            IEnumMappingService enumMappingService,
            IAuthorizationService authorizationService,
            ICatalogService catalogService)
            : base(authorizationService, enumMappingService)
        {
            _indicatorsService = indicatorsService;
            _enumMappingService = enumMappingService;
            _authorizationService = authorizationService;
            _catalogService = catalogService;
        }

        #endregion

        #region View

        public virtual ActionResult IndicatorsDashboard(string operationNumber)
        {
            base.SetViewBagErrorByTempData();

            var model = GetIndicatorsDashboardViewModel(operationNumber);
            base.SetViewBagRoles(operationNumber);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return View(model);
        }

        public virtual ActionResult IndicatorsForm(string operationNumber, bool isEditable, int versionId = 0)
        {
            SetViewBagRoles(operationNumber);

            var jsonResult = TryAccessToResources(URL_INDICATORS_FORM, operationNumber, versionId);
            if (jsonResult != null)
            {
                TempData["ErrorMessage"] = ((dynamic)jsonResult).ErrorMessage;
                return RedirectToAction("IndicatorsDashboard", new { operationNumber });
            }

            var model = GetIndicatorsViewModel(operationNumber, isEditable, versionId);

            if (!string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
            {
                TempData["ErrorMessage"] = Localization.GetText(ViewBag.ErrorMessage);
                return RedirectToAction("IndicatorsDashboard", new { operationNumber });
            }

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return View(model);
        }

        #endregion

        #region Save

        public virtual JsonResult SaveIndicators(string operationNumber)
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<IndicatorsViewModel>(jsonDataRequest.SerializedData);
            viewModel.IndicatorId = 0;
            var givenAnswers = viewModel.IndicatorsDynamicQuestionnaire.ProcessAnswers(jsonDataRequest.ClientFieldData);
            var value = jsonDataRequest.ClientFieldData;

            var response = _indicatorsService.SaveIndicators(operationNumber, viewModel, givenAnswers);

            return Json(response);
        }

        public virtual JsonResult IndicatorsDeleteReport(string operationNumber, int versionId)
        {
            var response = _indicatorsService.DeleteIndicatorReport(operationNumber, versionId);
            SetViewBagErrorMessageInvalidResponse(response);
            if (!string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
            {
                return Json(new { ErrorMessage = ViewBag.ErrorMessage });
            }

            return Json(response);
        }

        #endregion

        #region Private Methods

        private IndicatorsDashboardViewModel GetIndicatorsDashboardViewModel(string operationNumber)
        {
            var response = _indicatorsService.GetIndicatorsDashboard(operationNumber);
            var viewModel = response.IndicatorsViewModel ?? ViewModelInitializerFactory.InitializeIndicatorsDashboardViewModel();

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewBagIndicatorsDashboard(operationNumber);

            return viewModel;
        }

        private void SetViewBagIndicatorsDashboard(string operationNumber)
        {
            this.SetViewBagListFromCatalog(_catalogService, new Dictionary<ConvergenceMasterDataTypeEnum, string>
            {
            });
        }

        private IndicatorsViewModel GetIndicatorsViewModel(string operationNumber, bool isEditable, int versionId = 0)
        {
            var response = _indicatorsService.GetIndicatorsOrLastVersion(operationNumber, isEditable, versionId);
            var viewModel = response.Indicators ?? ViewModelInitializerFactory.InitializeIndicatorsViewModel();

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewBagIndicatorsForms(operationNumber);

            return viewModel;
        }

        private void SetViewBagIndicatorsForms(string operationNumber)
        {
            this.SetViewBagListFromCatalog(_catalogService, new Dictionary<ConvergenceMasterDataTypeEnum, string>
            {
                //{ConvergenceMasterDataTypeEnum.EsgClassification, "ImpactCategoryList"},
            });

            //ViewBag.FundList = notifyFundCoordinatorFundList;

            //SetViewBagEnumMapping<ESGRoleEnum>();
        }

        #endregion
    }
}