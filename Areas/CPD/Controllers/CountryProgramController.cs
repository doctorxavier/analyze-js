using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using IDB.Architecture.DataTables.Common;
using IDB.MW.Application.CPDModule.Services.CountryProgram;
using IDB.MW.Application.CPDModule.ViewModels.CountryProgram;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.MW.Application.CPDModule.Enums;
using IDB.Presentation.MVC4.General;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.CPDModule.Mappers;
using IDB.Presentation.MVC4.Areas.CPD.Models;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVC4.Areas.CPD.Mappers;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Contracts.ModelRepositories.Security;
using IDB.MW.Domain.Values.CPD;

namespace IDB.Presentation.MVC4.Areas.CPD.Controllers
{
    public partial class CountryProgramController : BaseController
    {
        #region Constants

        private static int NUMBER_YEAR_COMBO = 5;

        private static string CPD_COUNTRY_PROGRAM_CP_URL = "/CPD/CountryProgram/CountryProgram";
        private static string CPD_COUNTRY_PROGRAM_UCS_URL = "/CPD/CountryProgram/UseOfCountrySystem";
        private static string CPD_COUNTRY_PROGRAM_SA_URL = "/CPD/CountryProgram/StrategicAlignment";

        #endregion

        #region Fields

        private readonly IAuthorizationService _authorizationService;
        private readonly ICountryProgramService _countryProgramService;
        private readonly IEnumMappingService _enumMappingService;
        private readonly ISecurityModelRepository _securityModelRepository;

        #endregion

        #region Contructors

        public CountryProgramController(
            IAuthorizationService authorizationService,
            ICountryProgramService countryProgramService,
            IEnumMappingService enumMappingService,
            ISecurityModelRepository securityModelRepository)
            : base(authorizationService)
        {
            _authorizationService = authorizationService;
            _countryProgramService = countryProgramService;
            _enumMappingService = enumMappingService;
            _securityModelRepository = securityModelRepository;
        }

        #endregion

        #region Action Methods

        public virtual ActionResult Read(string operationNumber, string tabName = null, string errorMessage = null)
        {
            ViewBag.ActiveTab = tabName ?? NavigationAttributes.TAB_NAME_COUNTRYPROGRAM;

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                ViewBag.ErrorMessage = errorMessage;
            }

            var model = GetCountryProgram(operationNumber);
            return View("Read", model);
        }

        public virtual ActionResult Cancel(string operationNumber, string tabName = null)
        {
            ViewBag.ActiveTab = tabName ?? NavigationAttributes.TAB_NAME_COUNTRYPROGRAM;

            if (tabName == null || tabName == NavigationAttributes.TAB_NAME_COUNTRYPROGRAM)
            {
                SynchronizationHelper.TryReleaseLock(CPD_COUNTRY_PROGRAM_CP_URL, operationNumber, IDBContext.Current.UserLoginName);
            }

            if (tabName == NavigationAttributes.TAB_NAME_USEOFCS)
            {
                SynchronizationHelper.TryReleaseLock(CPD_COUNTRY_PROGRAM_UCS_URL, operationNumber, IDBContext.Current.UserLoginName);
            }

            if (tabName == NavigationAttributes.TAB_NAME_STRATEGICALIGNMENT)
            {
                SynchronizationHelper.TryReleaseLock(CPD_COUNTRY_PROGRAM_SA_URL, operationNumber, IDBContext.Current.UserLoginName);
            }

            var model = GetCountryProgram(operationNumber);
            return View("Read", model);
        }

        public virtual ActionResult EditCountryProgram(string operationNumber)
        {
            ViewBag.ActiveTab = NavigationAttributes.TAB_NAME_COUNTRYPROGRAM;
            SetViewBagsCountryProgram(operationNumber);

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, CPD_COUNTRY_PROGRAM_CP_URL, operationNumber, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return RedirectToAction("Read", new { tabName = NavigationAttributes.TAB_NAME_COUNTRYPROGRAM, errorMessage = errorMessage });
            }

            var model = GetCountryProgramTab(operationNumber);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model.CountryProgramTab);
            return View("Edit", model);
        }

        public virtual ActionResult EditUseOfCountrySystem(string operationNumber)
        {
            ViewBag.ActiveTab = NavigationAttributes.TAB_NAME_USEOFCS;
            SetViewBagUseCountrySys(operationNumber);
            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, CPD_COUNTRY_PROGRAM_UCS_URL, operationNumber, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return RedirectToAction("Read", new { tabName = NavigationAttributes.TAB_NAME_USEOFCS, errorMessage = errorMessage });
            }

            var model = GetUseOfCSTab(operationNumber);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model.UseOfCountrySystemTab);
            return View("Edit", model);
        }

        public virtual ActionResult EditStrategicAlignment(string operationNumber)
        {
            ViewBag.ActiveTab = NavigationAttributes.TAB_NAME_STRATEGICALIGNMENT;
            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, CPD_COUNTRY_PROGRAM_SA_URL, operationNumber, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return RedirectToAction("Read", new { tabName = NavigationAttributes.TAB_NAME_STRATEGICALIGNMENT, errorMessage = errorMessage });
            }

            var model = GetStrategicAlignmentTab(operationNumber);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model.StrategicAlignmentTab);
            return View("Edit", model);
        }

        public virtual ActionResult SaveCountryProgram(string operationNumber)
        {
            var responseView = new SaveResponse() { IsValid = true, ErrorMessage = string.Empty };

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<CountryProgramTabViewModel>(jsonDataRequest.SerializedData);
            var formData = jsonDataRequest.ClientFieldData;

            viewModel.UpdateCountryProgramTabViewModel(formData);

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, CPD_COUNTRY_PROGRAM_CP_URL, operationNumber, IDBContext.Current.UserLoginName);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            var response = _countryProgramService.SaveCountryProgramTab(operationNumber, viewModel);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if (responseView.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(CPD_COUNTRY_PROGRAM_CP_URL, operationNumber, IDBContext.Current.UserLoginName);
                responseView.UrlRedirect = Url.Action("Read", "CountryProgram", new { area = "CPD", tabName = NavigationAttributes.TAB_NAME_COUNTRYPROGRAM });
            }

            return Json(responseView);
        }

        public virtual ActionResult SaveUseOfCountrySystem(string operationNumber)
        {
            var responseView = new SaveResponse() { IsValid = true, ErrorMessage = string.Empty };

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<UseOfCountrySystemsTabViewModel>(jsonDataRequest.SerializedData);
            var formData = jsonDataRequest.ClientFieldData;

            viewModel.UpdateUseOfCountrySystemsTabViewModel(formData);

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, CPD_COUNTRY_PROGRAM_UCS_URL, operationNumber, IDBContext.Current.UserLoginName);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            var response = _countryProgramService.SaveUseOfCountrySystemsTab(operationNumber, viewModel);

            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if (responseView.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(CPD_COUNTRY_PROGRAM_UCS_URL, operationNumber, IDBContext.Current.UserLoginName);
                responseView.UrlRedirect = Url.Action("Read", "CountryProgram", new { area = "CPD", tabName = NavigationAttributes.TAB_NAME_USEOFCS });
            }

            return Json(responseView);
        }

        public virtual ActionResult SaveStrategicAlignment(string operationNumber)
        {
            var responseView = new SaveResponse() { IsValid = true, ErrorMessage = string.Empty };

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<StrategicAlignmentTabViewModel>(jsonDataRequest.SerializedData);
            var formData = jsonDataRequest.ClientFieldData;

            viewModel.UpdateStrategicAlignmentTabViewModel(formData, operationNumber);

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, CPD_COUNTRY_PROGRAM_SA_URL, operationNumber, IDBContext.Current.UserLoginName);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            var response = _countryProgramService.SaveStrategicAligmentTab(operationNumber, viewModel);

            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if (responseView.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(CPD_COUNTRY_PROGRAM_SA_URL, operationNumber, IDBContext.Current.UserLoginName);
                responseView.UrlRedirect = Url.Action("Read", "CountryProgram", new { area = "CPD", tabName = NavigationAttributes.TAB_NAME_STRATEGICALIGNMENT });
            }

            return Json(responseView);
        }

        #endregion

        #region Private Methods

        private CountryProgramViewModel GetCountryProgram(string operationNumber)
        {
            var model = new CountryProgramViewModel();
            var response = _countryProgramService.GetCountryProgram(operationNumber);
            model.CountryProgramTab = response.CountryProgramTab;
            model.UseOfCountrySystemTab = response.UseOfCountrySystemsTab;
            model.StrategicAlignmentTab = response.StrategicAlignmentTab;
            model.IsOperationClosed = response.IsOperationClosed;
            SetViewBagErrorMessageInvalidResponse(response);

            model.FieldAccessList = _securityModelRepository.GetFieldBehaviorByPermissions(
                                       IDBContext.Current.Operation,
                                       AttributeCPD.PAGE_NAME_COUNTRY_PROGRAM +
                                                '|' + AttributeCPD.PAGE_NAME_COUNTRY_PROGRAM_COUNTRY +
                                                '|' + AttributeCPD.PAGE_NAME_COUNTRY_PROGRAM_STRATEGIC,
                                       IDBContext.Current.Permissions,
                                       0,
                                       0)
                                       .ToList();

            return model;
        }

        private CountryProgramViewModel GetCountryProgramTab(string operationNumber)
        {
            var model = new CountryProgramViewModel()
            {
                CountryProgramTab = new CountryProgramTabViewModel()
            };
            var response = _countryProgramService.GetCountryProgramTab(operationNumber);
            model.CountryProgramTab = response.CountryProgramTab;
            SetViewBagErrorMessageInvalidResponse(response);
            return model;
        }

        private CountryProgramViewModel GetUseOfCSTab(string operationNumber)
        {
            var model = new CountryProgramViewModel()
            {
                UseOfCountrySystemTab = new UseOfCountrySystemsTabViewModel()
            };
            var response = _countryProgramService.GetUseOfCountrySystemsTab(operationNumber);
            model.UseOfCountrySystemTab = response.UseOfCountrySystemsTab;
            SetViewBagErrorMessageInvalidResponse(response);
            return model;
        }

        private CountryProgramViewModel GetStrategicAlignmentTab(string operationNumber)
        {
            var model = new CountryProgramViewModel() 
            {
                StrategicAlignmentTab = new StrategicAlignmentTabViewModel()
            };

            var response = _countryProgramService.GetStrategicAligmentTab(operationNumber);
            model.StrategicAlignmentTab = response.StrategicAlignmentTab;
            SetViewBagErrorMessageInvalidResponse(response);
            return model;
        }

        private void SetViewBagsCountryProgram(string operationNumber)
        {
            var cancelAdjustmentTypeId = _enumMappingService.GetMappingCode<CPDAdjustementTypeEnum>(CPDAdjustementTypeEnum.Cancel);
            var reformulationAdjustmentTypeId = _enumMappingService.GetMappingCode<CPDAdjustementTypeEnum>(CPDAdjustementTypeEnum.Reform);

            ViewBag.CancelationAdjustmentTypeId = cancelAdjustmentTypeId;

            ViewBag.ReformulationAdjustmentTypeId = reformulationAdjustmentTypeId;
        }

        private void SetViewBagUseCountrySys(string operationNumber)
        {
            var collectionCPDFinManag = _enumMappingService.GetMasterDataCollection<CPDFinancialManagementEnum>();
            var collectionCPDProcurementEnum = _enumMappingService.GetMasterDataCollection<CPDProcurementEnum>();
            var collectionCPDForseenAction = _enumMappingService.GetMasterDataCollection<CPDForseenActionsCSEnum>();
            var collectionCPDForseenActionProgYear = _enumMappingService.GetMasterDataCollection<CPDForseenActionProgYearEnum>();

            var currentYear = DateTime.Now.Year;
            var yearList = new List<SelectListItem>();
            for (var i = -5; i <= NUMBER_YEAR_COMBO; i++)
            {
                yearList.Add(new SelectListItem { Value = (currentYear + i).ToString(), Text = (currentYear + i).ToString() });
            }

            ViewBag.financialManagmentList = collectionCPDFinManag.ConverToSelectListItems().ConvertToMultiDropDownItem();

            ViewBag.procurementList = collectionCPDProcurementEnum.ConverToSelectListItems().ConvertToMultiDropDownItem();

            ViewBag.FinancialManagementYearList = yearList;

            ViewBag.ProcurementYearList = yearList;

            ViewBag.ForeseenActionsList = collectionCPDForseenAction.ConverToSelectListItems().ConvertToMultiDropDownItem();

            ViewBag.ForeseenActionsListYear = collectionCPDForseenActionProgYear.ConverToSelectListItems().ConvertToMultiDropDownItem();
        }

        #endregion
    }
}