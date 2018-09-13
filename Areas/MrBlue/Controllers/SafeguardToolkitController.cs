using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.MW.Application.Core.Enums;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.DEMModule.Services.Core.Interfaces;
using IDB.MW.Application.MrBlueModule.Enums;
using IDB.MW.Application.MrBlueModule.Services.SafeguardToolkit;
using IDB.MW.Application.MrBlueModule.ViewModels.SafeguardToolkit;
using IDB.MW.Application.TCAbstractModule.Enums;
using IDB.MW.Domain.Session;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.MW.Infrastructure.Helpers;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.Presentation.MVC4.Areas.MrBlue.Models;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.MrBlue.Controllers
{
    public partial class SafeguardToolkitController : BaseController
    {
        #region Constants

        private const string NO_WRITE_PERMISSION = "TC.TCAbstractService.NoWritePermission";
        private const string URL_SAFEGUARD_TOOLKIT_DASHBOARD = "/MrBlue/SafeguardToolkitDashboard";
        private const string URL_SAFEGUARD_TOOLKIT_2 = "/MrBlue/SafeguardToolkit2";
        private const string URL_SAFEGUARD_TOOLKIT_3 = "/MrBlue/SafeguardToolkit3";
        private const string URL_SAFEGUARD_TOOLKIT_4 = "/MrBlue/SafeguardToolkit4";
        private const string URL_SAFEGUARD_TOOLKIT_5 = "/MrBlue/SafeguardToolkit5";
        private const string URL_SAFEGUARD_TOOLKIT_6 = "/MrBlue/SafeguardToolkit6";

        #endregion

        #region Fields

        private readonly ISafeguardToolkitService _safeguardToolkitService;
        private readonly IEnumMappingService _enumMappingService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICatalogService _catalogService;
        private readonly IConvergenceMasterDataRepository _convergenceMasterDataRepository;
        private readonly IDEMLockModulesService _demLockModulesService;
        #endregion

        #region Constructors

        public SafeguardToolkitController(
            IConvergenceMasterDataRepository convergenceMasterDataRepository,
            ISafeguardToolkitService safeguardToolkitService,
            IEnumMappingService enumMappingService,
            IAuthorizationService authorizationService,
            ICatalogService catalogService,
            IDEMLockModulesService demLockModulesService)
            : base(authorizationService, enumMappingService)
        {
            _safeguardToolkitService = safeguardToolkitService;
            _enumMappingService = enumMappingService;
            _authorizationService = authorizationService;
            _catalogService = catalogService;
            _convergenceMasterDataRepository = convergenceMasterDataRepository;
            _demLockModulesService = demLockModulesService;
        }

        #endregion

        #region View

        public virtual ActionResult SafeguardToolkitDashboard(string operationNumber)
        {
            base.SetViewBagErrorByTempData();

            var model = GetSafeguardToolkitDashboardViewModel(operationNumber);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            ViewBag.DataAttrSPF = (int)ESGDocumentTypeEnum.SafeguardToolkitSPF;
            ViewBag.DataAttrSSF = (int)ESGDocumentTypeEnum.SafeguardToolkitSSF;

            model.DemLockReviewProcessDataViewModel = _demLockModulesService
                .BuildLockReviewProcessDEMDataSerializable(operationNumber);

            return View(model);
        }

        public virtual ActionResult SafeguardToolkitDocumentRedirect(string operationNumber, int versionId, ESGDocumentTypeEnum documentType, string fecha)
        {
            var response = _safeguardToolkitService.ExportSafeguardToolkitToFile(operationNumber, documentType, versionId);

            if (!response.IsValid)
            {
                return null;
            }

            return Redirect(response.DocumentUrl);
        }

        public virtual FileResult SafeguardToolkitDownload(string operationNumber, int versionId, ESGDocumentTypeEnum documentType, string fecha)
        {
            var response = _safeguardToolkitService.ExportSafeguardToolkitToFile(operationNumber, documentType, versionId);

            if (!response.IsValid)
            {
                return null;
            }

            DateTime Fecha = Convert.ToDateTime(fecha);

            string OnDate = Fecha.ToString("yyyyMMdd");
            string OnHour = Fecha.ToString("HHmm");

            var RepName = string.Empty;
            if (ESGDocumentTypeEnum.SafeguardToolkitSPF == documentType)
            {
                RepName = operationNumber + "_SPF_" + OnDate + "_" + OnHour + ".pdf";
            }

            if (ESGDocumentTypeEnum.SafeguardToolkitSSF == documentType)
            {
                RepName = operationNumber + "_SSF_" + OnDate + "_" + OnHour + ".pdf";
            }

            return File(response.File, FileContentTypeEnum.Pdf.GetEnumDescription(), RepName);
        }

        public virtual ActionResult SafeguardToolkitRedirect(string operationNumber, SafeguardToolkitStepEnum fromStep, StepNavigatorEnum navigator, int versionId = 0, bool isNew = false, SafeguardToolkitCreationModeEnum creationMode = SafeguardToolkitCreationModeEnum.NA, int? previousVersionId = null)
        {
            base.SetViewBagErrorByTempData();

            var responseNextStep = _safeguardToolkitService.GetSafeguardToolkitNextStep(operationNumber, versionId, fromStep: fromStep, navigator: navigator);

            ViewBag.CreationMode = creationMode;

            return RedirectToAction(responseNextStep.NextStep.GetEnumDescription(), new { operationNumber = operationNumber, versionId = versionId, isNew = isNew, creationMode = creationMode });
        }

        public virtual ActionResult SafeguardToolkit2(string operationNumber, int? simulateCountry, int versionId = 0, bool isNew = false, SafeguardToolkitCreationModeEnum creationMode = SafeguardToolkitCreationModeEnum.NA)
        {
            var jsonResult = TryAccessToResources(URL_SAFEGUARD_TOOLKIT_2, operationNumber, versionId);
            if (jsonResult != null)
            {
                TempData["ErrorMessage"] = ((dynamic)jsonResult).ErrorMessage;
                return RedirectToAction("SafeguardToolkitDashboard", new { operationNumber });
            }

            var model = GetSafeguardToolkitStep2ViewModel(operationNumber, versionId, simulateCountry);
            model.IsNew = isNew;
            if (!string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
            {
                TempData["ErrorMessage"] = ViewBag.ErrorMessage;
                return RedirectToAction("SafeguardToolkitDashboard", new { operationNumber });
            }

            ViewBag.isNew = isNew;

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            ViewBag.CreationMode = creationMode;

            return View(model);
        }

        public virtual ActionResult SafeguardToolkit3(string operationNumber, int versionId, bool isNew = false, SafeguardToolkitCreationModeEnum creationMode = SafeguardToolkitCreationModeEnum.NA)
        {
            var jsonResult = TryAccessToResources(URL_SAFEGUARD_TOOLKIT_3, operationNumber, versionId);
            if (jsonResult != null)
            {
                TempData["ErrorMessage"] = ((dynamic)jsonResult).ErrorMessage;
                return RedirectToAction("SafeguardToolkit2", new { operationNumber });
            }

            var model = GetSafeguardToolkitStep3ViewModel(operationNumber, creationMode, versionId, isNew);

            if (!string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
            {
                TempData["ErrorMessage"] = ViewBag.ErrorMessage;
                return RedirectToAction("SafeguardToolkit2", new { operationNumber, versionId });
            }

            ViewBag.isNew = isNew;

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            ViewBag.CreationMode = creationMode;

            return View(model);
        }

        public virtual ActionResult SafeguardToolkit4(string operationNumber, int versionId, bool isNew = false, SafeguardToolkitCreationModeEnum creationMode = SafeguardToolkitCreationModeEnum.NA)
        {
            var jsonResult = TryAccessToResources(URL_SAFEGUARD_TOOLKIT_4, operationNumber, versionId);
            if (jsonResult != null)
            {
                TempData["ErrorMessage"] = ((dynamic)jsonResult).ErrorMessage;
                return RedirectToAction("SafeguardToolkit3", new { operationNumber });
            }

            var model = GetSafeguardToolkitStep4ViewModel(operationNumber, creationMode, versionId, isNew);

            if (!string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
            {
                TempData["ErrorMessage"] = ViewBag.ErrorMessage;
                return RedirectToAction("SafeguardToolkit3", new { operationNumber, versionId });
            }

            ViewBag.isNew = isNew;

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            ViewBag.CreationMode = creationMode;

            return View(model);
        }

        public virtual ActionResult SafeguardToolkit5(string operationNumber, int versionId, SafeguardToolkitCreationModeEnum creationMode = SafeguardToolkitCreationModeEnum.NA)
        {
            var jsonResult = TryAccessToResources(URL_SAFEGUARD_TOOLKIT_5, operationNumber, versionId);
            if (jsonResult != null)
            {
                TempData["ErrorMessage"] = ((dynamic)jsonResult).ErrorMessage;
                return RedirectToAction("SafeguardToolkitDashboard", new { operationNumber });
            }

            if (!string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
            {
                TempData["ErrorMessage"] = ViewBag.ErrorMessage;
                return RedirectToAction("SafeguardToolkitDashboard", new { operationNumber });
            }

            var model = GetSafeguardToolkitStep5ViewModel(operationNumber, versionId);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            ViewBag.CreationMode = creationMode;

            return View(model);
        }

        public virtual ActionResult SafeguardToolkit6(string operationNumber, int versionId)
        {
            var jsonResult = TryAccessToResources(URL_SAFEGUARD_TOOLKIT_6, operationNumber, versionId);
            if (jsonResult != null)
            {
                TempData["ErrorMessage"] = ((dynamic)jsonResult).ErrorMessage;
                return RedirectToAction("SafeguardToolkitDashboard", new { operationNumber });
            }

            var model = GetSafeguardToolkitStep6ViewModel(operationNumber, versionId);

            if (!string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
            {
                TempData["ErrorMessage"] = ViewBag.ErrorMessage;
                return RedirectToAction("SafeguardToolkitDashboard", new { operationNumber, versionId });
            }

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            return View(model);
        }

        public virtual ActionResult GetNewTextRecomendation(int enumEnviromental)
        {
            string response = string.Empty;

            ESGClassificationEnum classificationEnum = _enumMappingService.GetMappedEnum<ESGClassificationEnum>(enumEnviromental);

            response = _safeguardToolkitService.GetEnviromentalCategoryText(classificationEnum);

            return Json(response);
        }

        #endregion

        #region Save

        public virtual JsonResult SaveSafeguardToolkit2(string operationNumber, SafeguardToolkitCreationModeEnum? creationMode = null, int? previousVersionId = null)
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<SafeguardToolkitStep2ViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateSafeguardToolkit2(jsonDataRequest.ClientFieldData, _enumMappingService);

            var givenAnswers = viewModel.SafeguardPolicyFilter.ProcessAnswers(jsonDataRequest.ClientFieldData);
            var value = jsonDataRequest.ClientFieldData.FirstOrDefault(x => x.Name == "isFinalize");

            var response = _safeguardToolkitService.SaveSafeguardToolkitStep2(operationNumber, viewModel, givenAnswers, creationMode: creationMode, previousVersionId: previousVersionId);

            return Json(response);
        }

        public virtual JsonResult SaveSafeguardToolkit3(string operationNumber)
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<SafeguardToolkitStep3ViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateSafeguardToolkit3(jsonDataRequest.ClientFieldData, _enumMappingService);

            var givenAnswers = viewModel.ImpactsChecklist.ProcessAnswers(jsonDataRequest.ClientFieldData);

            var response = _safeguardToolkitService.SaveSafeguardToolkitStep3(operationNumber, viewModel, givenAnswers);

            return Json(response);
        }

        public virtual JsonResult SaveSafeguardToolkit4(string operationNumber)
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<SafeguardToolkitStep4ViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateSafeguardToolkit4(jsonDataRequest.ClientFieldData, _enumMappingService);

            var givenAnswers = viewModel.SafeguardPolicyFilter.ProcessAnswers(jsonDataRequest.ClientFieldData);

            var response = _safeguardToolkitService.SaveSafeguardToolkitStep4(operationNumber, viewModel, givenAnswers);

            return Json(response);
        }

        public virtual JsonResult SaveSafeguardToolkit5(string operationNumber)
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<SafeguardToolkitStep5ViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateSafeguardToolkit5(jsonDataRequest.ClientFieldData, _enumMappingService);

            var value = jsonDataRequest.ClientFieldData.FirstOrDefault(x => x.Name == "isFinalize");

            bool isFinalize = value != null ? bool.Parse(value.Value) : false;
            var response = _safeguardToolkitService.SaveSafeguardToolkitStep5(operationNumber, viewModel, isFinalize);
            return Json(response);
        }

        public virtual JsonResult SaveSafeguardToolkit6(string operationNumber, SafeguardToolkitStep6ViewModel viewModel)
        {
            return Json(new { IsValid = true });
        }

        public virtual JsonResult SaveSafeguardToolkitDeleteReport(string operationNumber, int versionId)
        {
            var response = _safeguardToolkitService.DeleteSafeguardToolkitReport(operationNumber, versionId);
            SetViewBagErrorMessageInvalidResponse(response);
            if (!string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
            {
                return Json(new 
                { 
                    ErrorMessage = ViewBag.ErrorMessage 
                });
            }

            return Json(response);
        }

        #endregion

        #region Private Methods

        private SafeguardToolkitDashboardViewModel GetSafeguardToolkitDashboardViewModel(string operationNumber)
        {
            var response = _safeguardToolkitService.GetSafeguardToolkitDashboard(operationNumber);
            var viewModel = response.SafeguardToolkitDashboard ?? ViewModelInitializerFactory.InitializeSafeguardToolkitDashboardViewModel();

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewBagSafeguardToolkitDashboard(operationNumber);

            return viewModel;
        }

        private void SetViewBagSafeguardToolkitDashboard(string operationNumber)
        {
            this.SetViewBagListFromCatalog(_catalogService, new Dictionary<ConvergenceMasterDataTypeEnum, string>
            {
                { ConvergenceMasterDataTypeEnum.Country, "CountryList" },
            });
        }

        private SafeguardToolkitStep2ViewModel GetSafeguardToolkitStep2ViewModel(string operationNumber, int versionId, int? simulateCountry)
        {
            var response = _safeguardToolkitService.GetSafeguardToolkitStep2(operationNumber, simulateCountry, versionId);
            var viewModel = response.SafeguardToolkitStep2 ?? ViewModelInitializerFactory.InitializeSafeguardToolkitStep2ViewModel();

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewBagSafeguardToolkit2(operationNumber);

            return viewModel;
        }

        private void SetViewBagSafeguardToolkit2(string operationNumber)
        {
        }

        private SafeguardToolkitStep3ViewModel GetSafeguardToolkitStep3ViewModel(string operationNumber, SafeguardToolkitCreationModeEnum creationMode, int versionId, bool isNew = false)
        {
            var response = _safeguardToolkitService.GetSafeguardToolkitStep3(operationNumber, creationMode, versionId);
            var viewModel = response.SafeguardToolkitStep3 ?? ViewModelInitializerFactory.InitializeSafeguardToolkitStep3ViewModel();

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewBagSafeguardToolkit3(operationNumber);

            return viewModel;
        }

        private void SetViewBagSafeguardToolkit3(string operationNumber)
        {
        }

        private SafeguardToolkitStep4ViewModel GetSafeguardToolkitStep4ViewModel(string operationNumber, SafeguardToolkitCreationModeEnum creationMode, int versionId, bool isNew = false)
        {
            var response = _safeguardToolkitService.GetSafeguardToolkitStep4(operationNumber, creationMode, versionId);
            var viewModel = response.SafeguardToolkitStep4 ?? ViewModelInitializerFactory.InitializeSafeguardToolkitStep4ViewModel();

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewBagSafeguardToolkit4(operationNumber);

            return viewModel;
        }

        private void SetViewBagSafeguardToolkit4(string operationNumber)
        {
        }

        private SafeguardToolkitStep5ViewModel GetSafeguardToolkitStep5ViewModel(string operationNumber, int versionId)
        {
            var response = _safeguardToolkitService.GetSafeguardToolkitStep5(operationNumber, versionId);
            var viewModel = response.SafeguardToolkitStep5 ?? ViewModelInitializerFactory.InitializeSafeguardToolkitStep5ViewModel();

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewBagSafeguardToolkit5(operationNumber);

            return viewModel;
        }

        private void SetViewBagSafeguardToolkit5(string operationNumber)
        {
            this.SetViewBagListFromCatalog(_catalogService, new Dictionary<ConvergenceMasterDataTypeEnum, string>
            {
                { ConvergenceMasterDataTypeEnum.EsgClassification, "RevisedClassificationList" },
                { ConvergenceMasterDataTypeEnum.EsgJustificationOverriden, "EsgJustificationOverridenList" }
            });

            ViewBag.EsgClasification = new List<SelectListItem>();
            ViewBag.EsgClasificationOverride = new List<SelectListItem>();

            var itemA = ((List<SelectListItem>)ViewBag.RevisedClassificationList).FirstOrDefault(x => x.Value == _enumMappingService.GetMappingCode(ESGClassificationEnum.A).ToString());
            var itemB = ((List<SelectListItem>)ViewBag.RevisedClassificationList).FirstOrDefault(x => x.Value == _enumMappingService.GetMappingCode(ESGClassificationEnum.B).ToString());
            var itemC = ((List<SelectListItem>)ViewBag.RevisedClassificationList).FirstOrDefault(x => x.Value == _enumMappingService.GetMappingCode(ESGClassificationEnum.C).ToString());
            var itemB13 = ((List<SelectListItem>)ViewBag.RevisedClassificationList).FirstOrDefault(x => x.Value == _enumMappingService.GetMappingCode(ESGClassificationEnum.B13).ToString());

            ((List<SelectListItem>)ViewBag.EsgClasification).Add(itemA);
            ((List<SelectListItem>)ViewBag.EsgClasification).Add(itemB);
            ((List<SelectListItem>)ViewBag.EsgClasification).Add(itemC);
            ((List<SelectListItem>)ViewBag.EsgClasification).Add(itemB13);

            ((List<SelectListItem>)ViewBag.EsgClasificationOverride).Add(itemA);
            ((List<SelectListItem>)ViewBag.EsgClasificationOverride).Add(itemB);
            ((List<SelectListItem>)ViewBag.EsgClasificationOverride).Add(itemC);

            ViewBag.EnvironmentalCategoryList = new List<SelectListItem>
            {
                new SelectListItem { Text = "A", Value = "0" }
            };
            ViewBag.DisasterRiskCategoryList = new List<SelectListItem>
            {
                new SelectListItem { Text = "Low", Value = "0" }
            };
            ViewBag.TypeOfOperationList = new List<SelectListItem>
            {
                new SelectListItem { Text = "Other Lending or Financing Instrument", Value = "0" }
            };

            var itemDefault = ((List<SelectListItem>)ViewBag.EsgJustificationOverridenList).FirstOrDefault(x => x.Value == _enumMappingService.GetMappingCode(ESGJustificationOverridenEnum.ESGJustificationDefault).ToString());

            ((List<SelectListItem>)ViewBag.EsgJustificationOverridenList).Remove(itemDefault);
            ((List<SelectListItem>)ViewBag.EsgJustificationOverridenList).Insert(5, itemDefault);

            SetViewBagEnumMapping<ESGClassificationEnum>();
            SetViewBagEnumMapping<ESGJustificationOverridenEnum>();
        }

        private SafeguardToolkitStep6ViewModel GetSafeguardToolkitStep6ViewModel(string operationNumber, int versionId)
        {
            var response = _safeguardToolkitService.GetSafeguardToolkitStep6(operationNumber, versionId);
            var viewModel = response.SafeguardToolkitStep6 ?? ViewModelInitializerFactory.InitializeSafeguardToolkitStep6ViewModel();

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewBagSafeguardToolkit6(operationNumber);

            return viewModel;
        }

        private void SetViewBagSafeguardToolkit6(string operationNumber)
        {
            ViewBag.UserName = IDBContext.Current.UserLoginName;
        }
        #endregion
    }
}