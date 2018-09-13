using System;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.MW.Application.Core.Enums;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.MrBlueModule.Services.HighRisk;
using IDB.MW.Application.MrBlueModule.ViewModels.HighRisk;
using IDB.MW.Infrastructure.Helpers;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.Presentation.MVC4.Areas.MrBlue.Models;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.MrBlue.Controllers
{
    public partial class HighRiskController : BaseController
    {
        #region Constants

        private const string NO_WRITE_PERMISSION = "TC.TCAbstractService.NoWritePermission";
        private const string URL_HIGH_RISK_FORM = "/MrBlue/HighRisk/HighRiskForm";

        #endregion

        #region Fields

        private readonly IHighRiskService _highRiskService;
        private readonly IEnumMappingService _enumMappingService;
        private readonly IAuthorizationService _authorizationService;

        #endregion

        #region Constructors

        public HighRiskController(
            IHighRiskService highRiskService,
            IEnumMappingService enumMappingService,
            IAuthorizationService authorizationService)
            : base(authorizationService, enumMappingService)
        {
            _highRiskService = highRiskService;
            _enumMappingService = enumMappingService;
            _authorizationService = authorizationService;
        }

        #endregion

        #region View

        public virtual ActionResult HighRiskDashboard(string operationNumber)
        {
            base.SetViewBagErrorByTempData();

            var model = GetHighRiskDashboardViewModel(operationNumber);
            SetViewBagRoles(operationNumber);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return View(model);
        }

        public virtual ActionResult HighRiskForm(string operationNumber, bool isEditable, int versionId = 0, bool mostRecent = false)
        {
            SetViewBagRoles(operationNumber);

            var jsonResult = TryAccessToResources(URL_HIGH_RISK_FORM, operationNumber, versionId);
            if (jsonResult != null)
            {
                TempData["ErrorMessage"] = ((dynamic)jsonResult).ErrorMessage;
                return RedirectToAction("HighRiskDashboard", new { operationNumber });
            }

            var model = GetHighRiskViewModel(operationNumber, isEditable, versionId, mostRecent);

            if (!string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
            {
                TempData["ErrorMessage"] = Localization.GetText(ViewBag.ErrorMessage);
                return RedirectToAction("HighRiskDashboard", new { operationNumber });
            }

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            if (!mostRecent && versionId != 0)
                ViewBag.WriteRole = false;

            return View(model);
        }

        public virtual RedirectResult HighRiskDownload(string operationNumber,
            int versionId,
            string fecha)
        {
            var response = _highRiskService.ExportHighRiskToFile(operationNumber, versionId);

            if (!response.IsValid || !Uri.IsWellFormedUriString(response.FileName,
                UriKind.RelativeOrAbsolute))
            {
                return null;
            }

            return Redirect(response.FileName);
        }

        #endregion

        #region Save
        public virtual JsonResult SaveHighRiskForm(string operationNumber)
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<HighRiskViewModel>(jsonDataRequest.SerializedData);
            var givenAnswers = viewModel.HighRiskDynamicQuestionnaire.ProcessAnswers(jsonDataRequest.ClientFieldData);
            var value = jsonDataRequest.ClientFieldData;

            var response = _highRiskService.SaveHighRisk(operationNumber, viewModel, givenAnswers);
            return Json(response);
        }

        public virtual JsonResult HightRiskDeleteReport(string operationNumber, int versionId)
        {
            var response = _highRiskService.DeleteHighRiskReport(operationNumber, versionId);
            SetViewBagErrorMessageInvalidResponse(response);
            if (!string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
            {
                return Json(new { ErrorMessage = ViewBag.ErrorMessage });
            }

            return Json(response);
        }
        #endregion

        #region Private Methods

        private HighRiskDashboardViewModel GetHighRiskDashboardViewModel(string operationNumber)
        {
            var response = _highRiskService.GetHighRiskDashboard(operationNumber);
            var viewModel = response.HighRiskDashboard ?? ViewModelInitializerFactory.InitializeHighRiskDashboardViewModel();

            base.SetViewBagErrorMessageInvalidResponse(response);
            SetViewBagHighRiskDashboard(operationNumber);

            return viewModel;
        }

        private void SetViewBagHighRiskDashboard(string operationNumber)
        {
            //this.SetViewBagListFromCatalog(_catalogService, new Dictionary<ConvergenceMasterDataTypeEnum, string>
            //{
            //    {ConvergenceMasterDataTypeEnum.EsgClassification, "ImpactCategoryList"},

            //});

            //SetViewBagEnumMapping<ESGRoleEnum>();
        }

        private void SetViewBagHighRisk(string operationNumber)
        {
            //this.SetViewBagListFromCatalog( _catalogService , new Dictionary<ConvergenceMasterDataTypeEnum , string>
            //{
            //    {ConvergenceMasterDataTypeEnum.EsgClassification, "ImpactCategoryList"},

            //} );

            //SetViewBagEnumMapping<ESGRoleEnum>();
        }

        private HighRiskViewModel GetHighRiskViewModel(string operationNumber, bool isEditable, int versionId = 0, bool mostRecent = false)
        {
            var response = _highRiskService.GetHighRiskOrLastVersion(operationNumber, isEditable, versionId);

            if (mostRecent)
            {
                response.HighRisk.HighRiskId = 0;
                versionId = 0;
            }

            var viewModel = response.HighRisk ?? ViewModelInitializerFactory.InitializeHighRiskViewModel();

            base.SetViewBagErrorMessageInvalidResponse(response);
            SetViewBagHighRisk(operationNumber);

            return viewModel;
        }

        #endregion
    }
}