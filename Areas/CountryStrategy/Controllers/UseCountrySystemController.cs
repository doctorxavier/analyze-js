using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;

using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.CountryStrategyModule.Enums;
using IDB.MW.Application.CountryStrategyModule.Services.UseCountry;
using IDB.MW.Application.CountryStrategyModule.ViewModels.UseCountry;
using IDB.MW.Infrastructure.Caching;
using IDB.MW.Infrastructure.Caching.Contracts;
using IDB.MW.Infrastructure.ReportManager.Enums;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.Presentation.MVC4.General;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Application.TCAbstractModule.Enums;
using IDB.Presentation.MVC4.Areas.CountryStrategy.Mappers;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.MW.Application.CountryStrategyModule.Messages.UseCountry;
using IDB.Presentation.MVC4.Areas.CountryStrategy.Models;
using IDB.MW.Domain.Session;
using IDB.MW.Application.IndicatorsModuleNew.Services.LinkPredefinedIndicator;
using IDB.MW.Domain.Contracts.ModelRepositories.Security;

namespace IDB.Presentation.MVC4.Areas.CountryStrategy.Controllers
{
    public partial class UseCountrySystemController : BaseController
    {
        #region Constants
        public static string SAVE_MODE = "SaveMode";
        private static string CHART_USE_COUNTRY_SYS = "UI-CS-007-UseOfCountrySystems";
        private static string CS_USE_COUNTRY_SYSTEM_URL = "/CountryStrategy/UseCountrySystem/Edit";
        #endregion

        #region Fields
        private readonly IAuthorizationService _authorizationService;
        private readonly IEnumMappingService _enumMappingService;
        private readonly ICacheStorageService _cacheService;
        private readonly ICatalogService _catalogService;
        private readonly IUseCountrySystemService _useCountrySystemService;
        private readonly ILinkPredefinedIndicatorService _linkPredefinedIndicatorService;
        private readonly ISecurityModelRepository _securityModelRepository;
        #endregion

        #region Contructors

        public UseCountrySystemController(
            IAuthorizationService authorizationService,
            ICatalogService catalogService,
            IEnumMappingService enumMappingService,
            ILinkPredefinedIndicatorService linkPredefinedIndicatorService,
            IUseCountrySystemService useCountrySystemService,
            ISecurityModelRepository securityModelRepository)
            : base(authorizationService)
        {
            _authorizationService = authorizationService;
            _enumMappingService = enumMappingService;
            _catalogService = catalogService;
            _linkPredefinedIndicatorService = linkPredefinedIndicatorService;
            _cacheService = CacheStorageFactory.Current;
            _useCountrySystemService = useCountrySystemService;
            _securityModelRepository = securityModelRepository;
        }

        #endregion

        #region Action Methods

        public virtual ActionResult Read(string operationNumber, string errorMessage = null)
        {
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                ViewBag.ErrorMessage = errorMessage;
            }

            var model = GetUseCountrySystem(operationNumber, CHART_USE_COUNTRY_SYS);
            SetViewBag(operationNumber, model);
            return View(model);
        }

        public virtual ActionResult Cancel(string operationNumber)
        {
            SynchronizationHelper.TryReleaseLock(CS_USE_COUNTRY_SYSTEM_URL, operationNumber, IDBContext.Current.UserLoginName);

            var model = GetUseCountrySystem(operationNumber, CHART_USE_COUNTRY_SYS);
            SetViewBag(operationNumber, model);
            return View("Read", model);
        }

        public virtual ActionResult Edit(string operationNumber)
        {
            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, CS_USE_COUNTRY_SYSTEM_URL, operationNumber, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return RedirectToAction("Read", new { errorMessage = errorMessage });
            }

            var model = GetUseCountrySystem(operationNumber, CHART_USE_COUNTRY_SYS);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            SetViewBag(operationNumber, model);
            return View(model);
        }

        public virtual ActionResult Save(string operationNumber)
        {
            ResponseBase response = new SaveUseCountrySystemResponse() { IsValid = true };
            var responseView = new SaveResponse() { IsValid = true, ErrorMessage = string.Empty };

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<UseCountrySystemViewModel>(jsonDataRequest.SerializedData);
            var formData = jsonDataRequest.ClientFieldData;

            viewModel.UpdateUseCountrySystemViewModel(formData);

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, CS_USE_COUNTRY_SYSTEM_URL, operationNumber, IDBContext.Current.UserLoginName);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            var submitMode = formData.First(x => x.Name == "submitMode").Value;
            if (submitMode == SAVE_MODE)
            {
                response = _useCountrySystemService.SaveUseCountrySystem(operationNumber, viewModel);
            }

            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;
            responseView.UrlRedirect = Url.Action("Read", "UseCountrySystem", new { area = "CountryStrategy" });

            if (responseView.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(CS_USE_COUNTRY_SYSTEM_URL, operationNumber, IDBContext.Current.UserLoginName);
                responseView.UrlRedirect = Url.Action("Read", "UseCountrySystem", new { area = "CountryStrategy" });
            }

            return Json(responseView);
        }

        public virtual ActionResult RedirectToUseCountrySystemPage(bool isEdit = false)
        {
            var targetAction = "Read";
            if (isEdit)
            {
                targetAction = "Edit";
            }

            return RedirectToAction(targetAction);
        }

        public virtual FileResult UseCountrySystemExportToPDF(string operationNumber)
        {
            var response = _useCountrySystemService.ExportUseCountrySystemsResultsMatrixFile(operationNumber, OutputFormatEnum.PDF);

            if (!response.IsValid)
            {
                return null;
            }

            return File(response.File, "application/doc", "UseCountrySystemsResultsMatrix.pdf");
        }

        public virtual FileResult UseCountrySystemExportToExcel(string operationNumber)
        {
            var response = _useCountrySystemService.ExportUseCountrySystemsResultsMatrixFile(operationNumber, OutputFormatEnum.Excel);

            if (!response.IsValid)
            {
                return null;
            }

            return File(response.File, "application/doc", "UseCountrySystemsResultsMatrix.xls");
        }

        public virtual FileResult UseCountrySystemExportToWord(string operationNumber)
        {
            var response = _useCountrySystemService.ExportUseCountrySystemsResultsMatrixFile(operationNumber, OutputFormatEnum.Word);

            if (!response.IsValid)
            {
                return null;
            }

            return File(response.File, "application/doc", "UseCountrySystemsResultsMatrix.doc");
        }

        #endregion

        #region Private Methods

        private UseCountrySystemViewModel GetUseCountrySystem(string operationNumber,  string pageChart = null)
        {
            var response = _useCountrySystemService.GetUseCountrySystemResultsMatrix(operationNumber);
            var viewModel = response.UseCountrySystemViewModel;

            if (pageChart != null)
            {
                response.UseCountrySystemViewModel.FieldAccessList = _securityModelRepository.GetFieldBehaviorByPermissions(
                    IDBContext.Current.Operation,
                    pageChart,
                    IDBContext.Current.Permissions,
                    0,
                    0)
                    .ToList();
            }

            SetViewBagErrorMessageInvalidResponse(response);
            return viewModel;
        }

        private void SetViewBag(string operationNumber, UseCountrySystemViewModel viewModel)
        {
            var outcomeResponse = _linkPredefinedIndicatorService.GetLinkIndicatorModelForCSResultMatrix(false, "LinkIndicator");
            ViewBag.LinkIndicatorModel = outcomeResponse.Filter;

            ViewBag.CRFIndicators = new List<SelectListItem>();
            if (viewModel != null)
            {
                var allCRFIndicatorIdUsed = viewModel.UCSStrategicObjective.SelectMany(x => x.ExpectedOutcomeIndicators).SelectMany(y => y.LinkedIndicators).Distinct();
                var crfIndicatorResponse = _useCountrySystemService.GetCRFIndicatorsById(allCRFIndicatorIdUsed);
                ViewBag.CRFIndicators = crfIndicatorResponse.CRFIndicators;
            }

            this.SetViewBagListFromCatalog(_catalogService, new Dictionary<ConvergenceMasterDataTypeEnum, string>
            {
                { ConvergenceMasterDataTypeEnum.StrategicObjectiveType, "StrategicObjectiveList" },
                { ConvergenceMasterDataTypeEnum.TimmingType, "TimmingList" },
                { ConvergenceMasterDataTypeEnum.ForeseenActionType, "ForeseenActionsList" },
                { ConvergenceMasterDataTypeEnum.UseCountrySystemType, "UseCountryList" },
            });

            var forseenActionAux = (List<SelectListItem>)ViewBag.ForeseenActionsList;
            var foreseenActionsListAux = forseenActionAux.ConvertToMultiDropDownItems();

            var forseenList = new List<MultiDropDownItem>();

            forseenList.Add(foreseenActionsListAux.SingleOrDefault(x => x.Value == _enumMappingService.GetMappingCode(ForeseenActionTypeEnum.NoForeseenAction).ToString()));
            forseenList.Add(foreseenActionsListAux.SingleOrDefault(x => x.Value == _enumMappingService.GetMappingCode(ForeseenActionTypeEnum.FiduciaryAssessment).ToString()));
            forseenList.Add(foreseenActionsListAux.SingleOrDefault(x => x.Value == _enumMappingService.GetMappingCode(ForeseenActionTypeEnum.CloseGap).ToString()));
            forseenList.Add(foreseenActionsListAux.SingleOrDefault(x => x.Value == _enumMappingService.GetMappingCode(ForeseenActionTypeEnum.StrNationalSystem).ToString()));
            forseenList.Add(foreseenActionsListAux.SingleOrDefault(x => x.Value == _enumMappingService.GetMappingCode(ForeseenActionTypeEnum.OpenDialogue).ToString()));
            forseenList.Add(foreseenActionsListAux.SingleOrDefault(x => x.Value == _enumMappingService.GetMappingCode(ForeseenActionTypeEnum.ValidationSystem).ToString()));
            forseenList.Add(foreseenActionsListAux.SingleOrDefault(x => x.Value == _enumMappingService.GetMappingCode(ForeseenActionTypeEnum.AgreementImplement).ToString()));
            forseenList.Add(foreseenActionsListAux.SingleOrDefault(x => x.Value == _enumMappingService.GetMappingCode(ForeseenActionTypeEnum.StrengtheningIntervention).ToString()));
            forseenList.Add(foreseenActionsListAux.SingleOrDefault(x => x.Value == _enumMappingService.GetMappingCode(ForeseenActionTypeEnum.UseSystemSuspended).ToString()));

            ViewBag.ForeseenActionsList = forseenList;
            ViewBag.CloseGapId = _enumMappingService.GetMappingCode<ForeseenActionTypeEnum>(ForeseenActionTypeEnum.CloseGap);
        }

        #endregion
    }
}