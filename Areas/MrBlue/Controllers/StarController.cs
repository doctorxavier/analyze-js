using System;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.MW.Application.Core.Enums;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.MrBlueModule.Enums;
using IDB.MW.Application.MrBlueModule.Services.Star;
using IDB.MW.Application.MrBlueModule.ViewModels.Star;
using IDB.MW.Infrastructure.Helpers;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.Presentation.MVC4.Areas.MrBlue.Models;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.MrBlue.Controllers
{
    public partial class StarController : BaseController
    {
        #region Constants

        private const string URL_STAR_FORM = "/MrBlue/StarForm";

        #endregion

        #region Fields

        private readonly IStarService _starService;
        private readonly IEnumMappingService _enumMappingService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICatalogService _catalogService;

        #endregion

        #region Constructors

        public StarController(
            IStarService starService,
            IEnumMappingService enumMappingService,
            IAuthorizationService authorizationService,
            ICatalogService catalogService)
            : base(authorizationService, enumMappingService)
        {
            _starService = starService;
            _enumMappingService = enumMappingService;
            _authorizationService = authorizationService;
            _catalogService = catalogService;
        }

        #endregion

        #region View

        public virtual ActionResult StarDashboard(string operationNumber)
        {
            base.SetViewBagErrorByTempData();

            var model = GetStartDashboard(operationNumber);
            base.SetViewBagRoles(operationNumber);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            return View(model);
        }

        public virtual ActionResult StarForm(string operationNumber, bool isEditable, int versionId = 0, bool isNew = false)
        {
            base.SetViewBagRoles(operationNumber);

            var jsonResult = base.TryAccessToResources(URL_STAR_FORM, operationNumber, versionId);
            if (jsonResult != null)
            {
                TempData["ErrorMessage"] = ((dynamic)jsonResult).ErrorMessage;
                return RedirectToAction("StarDashboard", new { operationNumber });
            }

            var model = GetStarViewModel(operationNumber, isEditable, versionId);
            model.IsNew = isNew;

            if (!string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
            {
                TempData["ErrorMessage"] = Localization.GetText(ViewBag.ErrorMessage);
                return RedirectToAction("StarDashboard", new { operationNumber });
            }

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return View(model);
        }

        public virtual RedirectResult StarDownload(string operationNumber, int versionId, string fecha)
        {
            var response = _starService.ExportStarToFile(operationNumber, versionId);
                  
            if (!response.IsValid || !Uri.IsWellFormedUriString(response.FileName,
                UriKind.RelativeOrAbsolute))
            {
                return null;
            }
            
            return Redirect(response.FileName);
        }

        #endregion

        #region Save
        public virtual JsonResult SaveStarForm(string operationNumber)
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<StarViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateStar(jsonDataRequest.ClientFieldData, _enumMappingService);

            var givenAnswers = viewModel.StarDynamicQuestionnaire.ProcessAnswers(jsonDataRequest.ClientFieldData);
            var value = jsonDataRequest.ClientFieldData.FirstOrDefault(x => x.Name == "isFinalize");

            bool isFinalize = value != null ? bool.Parse(value.Value) : false;
            var response = _starService.SaveStar(operationNumber, viewModel, givenAnswers, isFinalize);

            return Json(response);
        }

        public virtual JsonResult StarDeleteReport(string operationNumber, int versionId)
        {
            var response = _starService.DeleteStarReport(operationNumber, versionId);
            SetViewBagErrorMessageInvalidResponse(response);
            if (!string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
            {
                return Json(new { ErrorMessage = ViewBag.ErrorMessage });
            }

            return Json(response);
        }
        #endregion

        #region Private Methods

        private StarDashboardViewModel GetStartDashboard(string operationNumber)
        {
            var response = _starService.GetStarDashboard(operationNumber);
            var viewModel = response.StarDashboard ?? ViewModelInitializerFactory.InitializeStarDashboardViewModel();

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewBagStartDashboard(operationNumber);

            return viewModel;
        }

        private void SetViewBagStartDashboard(string operationNumber)
        {
        }

        private StarViewModel GetStarViewModel(string operationNumber, bool isEditable, int versionId = 0)
        {
            var response = _starService.GetStarOrLastVersion(operationNumber, isEditable, versionId);
            var viewModel = response.Star ?? ViewModelInitializerFactory.InitializeStarViewModel();

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewBagStartForm(operationNumber);

            return viewModel;
        }

        private void SetViewBagStartForm(string operationNumber)
        {
            var policiesTriggered = _enumMappingService.GetMasterDataCollectionByCodesFiltered<ESGValuesTypeEnum>(ESGValuesTypeEnumFilter.FILTER_STAR_POLICIES_TRIGGERED);

            ViewBag.PoliciesTriggeredList = from p in policiesTriggered
                                            select new SelectListItem
                                            {
                                                Text = p.NameEn,
                                                Value = p.MasterDataId.ToString()
                                            };
        }

        #endregion
    }
}