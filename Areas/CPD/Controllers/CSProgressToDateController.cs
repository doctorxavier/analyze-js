using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.DataTables.Common;
using IDB.MW.Application.CPDModule.Services.CSProgressToDate;
using IDB.MW.Application.CPDModule.ViewModels.CSProgressToDate;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.CPDModule.Enums;
using IDB.MW.Application.CPDModule.Mappers;
using IDB.Presentation.MVC4.Areas.CPD.Models;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVC4.General;
using IDB.Presentation.MVC4.Areas.CPD.Mappers;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Contracts.ModelRepositories.Security;
using IDB.MW.Domain.Values.CPD;
using IDB.MW.Application.CountryStrategyModule.Enums;
using IDB.MW.Application.CPDModule.Services.NSGIntegrationCPD;

namespace IDB.Presentation.MVC4.Areas.CPD.Controllers
{
    public partial class CSProgressToDateController : BaseController
    {
        #region Constants
        private static int NUMBER_YEAR_COMBO = 5;

        private static string CPD_PROGRESS_DATE_PD_URL = "/CPD/CSProgressToDate/ProgressToDate";
        private static string CPD_PROGRESS_DATE_UCS_URL = "/CPD/CSProgressToDate/UseOfCountrySystems";

        #endregion

        #region Fields
        private readonly IAuthorizationService _authorizationService;
        private readonly ICSProgressToDateService _csProgressToDateService;
        private readonly IEnumMappingService _enumMappingService;
        private readonly ISecurityModelRepository _securityModelRepository;
        private readonly INsgIntegrationCpdService _nSGIntegrationCPDService;
        #endregion

        #region Contructors
        public CSProgressToDateController(
            IAuthorizationService authorizationService,
            ICSProgressToDateService csProgressToDateService,
            IEnumMappingService enumMappingService,
            ISecurityModelRepository securityModelRepository,
            INsgIntegrationCpdService nSGIntegrationCPDService)
            : base(authorizationService)
        {
            _authorizationService = authorizationService;
            _csProgressToDateService = csProgressToDateService;
            _enumMappingService = enumMappingService;
            _securityModelRepository = securityModelRepository;
            _nSGIntegrationCPDService = nSGIntegrationCPDService;
        }
        #endregion

        #region Action Methods
        public virtual ActionResult Read(string operationNumber, string tabName = null, string errorMessage = null)
        {
            ViewBag.ActiveTab = tabName ?? NavigationAttributes.TAB_NAME_PROGRESSTODATE;

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                ViewBag.ErrorMessage = errorMessage;
            }

            var model = GetCSProgressToDate(operationNumber);
            SetViewBagsProgressDate(operationNumber);
            return View("Read", model);
        }

        public virtual ActionResult DetailPortfolioApprovedDataRead(string operationNumber, int rowId, CSOperationsAprovedEnum typeInstrument)
        {
            ViewBag.ActiveTab = NavigationAttributes.TAB_NAME_PROGRESSTODATE;

            var model = GetDetailPortfolioApproved(operationNumber, rowId, typeInstrument);
            SetViewBagsProgressDate(operationNumber);

            return View("PortfolioDetailRead", model);
        }

        public virtual ActionResult DetailsPortfolioAlignmentDataRead(string operationNumber, int rowId, CpdTypeOperations typeOperationPortfolio, int csObjectiveId, bool isOtherArea)
        {
            ViewBag.ActiveTab = NavigationAttributes.TAB_NAME_PROGRESSTODATE;

            var model = GetDetailPortfolioAlignment(operationNumber, rowId, typeOperationPortfolio);
            SetViewBagsProgressDate(operationNumber);

            return View("PortfolioDetailRead", model);
        }

        public virtual ActionResult Cancel(string operationNumber, string tabName = null)
        {
            ViewBag.ActiveTab = tabName ?? NavigationAttributes.TAB_NAME_PROGRESSTODATE;

            if (tabName == null || tabName == NavigationAttributes.TAB_NAME_PROGRESSTODATE)
            {
                SynchronizationHelper.TryReleaseLock(CPD_PROGRESS_DATE_PD_URL, operationNumber, IDBContext.Current.UserLoginName);
            }

            if (tabName == NavigationAttributes.TAB_NAME_PD_USEOFCS)
            {
                SynchronizationHelper.TryReleaseLock(CPD_PROGRESS_DATE_UCS_URL, operationNumber, IDBContext.Current.UserLoginName);
            }

            var model = GetCSProgressToDate(operationNumber);
            SetViewBagsProgressDate(operationNumber);
            return View("Read", model);
        }

        public virtual ActionResult EditProgressToDate(string operationNumber)
        {
            ViewBag.ActiveTab = NavigationAttributes.TAB_NAME_PROGRESSTODATE;
            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, CPD_PROGRESS_DATE_PD_URL, operationNumber, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return RedirectToAction("Read", new { tabName = NavigationAttributes.TAB_NAME_PROGRESSTODATE, errorMessage = errorMessage });
            }

            var model = GetProgressToDate(operationNumber);
            SetViewBagsProgressDate(operationNumber);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model.ProgressToDateTab);
            return View("Edit", model);
        }

        public virtual ActionResult EditUseOfCountrySystems(string operationNumber)
        {
            ViewBag.ActiveTab = NavigationAttributes.TAB_NAME_PD_USEOFCS;
            SetViewBagsUseCountrySys(operationNumber);
            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, CPD_PROGRESS_DATE_UCS_URL, operationNumber, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return RedirectToAction("Read", new { tabName = NavigationAttributes.TAB_NAME_PD_USEOFCS, errorMessage = errorMessage });
            }

            var model = GetUseOfCS(operationNumber);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model.UseOfCountrySystemsTab);
            return View("Edit", model);
        }

        public virtual ActionResult SaveProgressToDate(string operationNumber)
        {
            var responseView = new SaveResponse() { IsValid = true, ErrorMessage = string.Empty };

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<ProgressToDateTabViewModel>(jsonDataRequest.SerializedData);
            var formData = jsonDataRequest.ClientFieldData;

            viewModel.UpdateProgressToDateTabViewModel(formData);

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, CPD_PROGRESS_DATE_PD_URL, operationNumber, IDBContext.Current.UserLoginName);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            var response = _csProgressToDateService.SaveProgressToDateTab(operationNumber, viewModel);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if (responseView.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(CPD_PROGRESS_DATE_PD_URL, operationNumber, IDBContext.Current.UserLoginName);
                responseView.UrlRedirect = Url.Action("Read", "CSProgressToDate", new { area = "CPD", tabName = NavigationAttributes.TAB_NAME_PROGRESSTODATE });
            }

            return Json(responseView);
        }

        public virtual ActionResult SaveUseOfCountrySystems(string operationNumber)
        {
            var responseView = new SaveResponse() { IsValid = true, ErrorMessage = string.Empty };

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<CSPDUseOfCountrySystemsTabViewModel>(jsonDataRequest.SerializedData);
            var formData = jsonDataRequest.ClientFieldData;

            viewModel.UpdateUseOfCountrySystemsTabCSViewModel(formData);

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, CPD_PROGRESS_DATE_UCS_URL, operationNumber, IDBContext.Current.UserLoginName);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            var response = _csProgressToDateService.SaveUseOfCSTab(operationNumber, viewModel);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if (responseView.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(CPD_PROGRESS_DATE_UCS_URL, operationNumber, IDBContext.Current.UserLoginName);
                responseView.UrlRedirect = Url.Action("Read", "CSProgressToDate", new { area = "CPD", tabName = NavigationAttributes.TAB_NAME_PD_USEOFCS });
            }

            return Json(responseView);
        }

        public virtual JsonResult GetCSObjectives(string operationNumber)
        {
            var response = _csProgressToDateService.SearchCSObjectives(operationNumber);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Private Methods
        private ProgressToDateViewModel GetCSProgressToDate(string operationNumber)
        {
            var model = new ProgressToDateViewModel();
            var response = _csProgressToDateService.GetProgressToDate(operationNumber);
            model.ProgressToDateTab = response.ProgressToDateTab;
            model.UseOfCountrySystemsTab = response.UseOfCountrySystemsTab;
            model.IsOperationClosed = response.IsOperationClosed;
            SetViewBagErrorMessageInvalidResponse(response);

            model.FieldAccessList = _securityModelRepository.GetFieldBehaviorByPermissions(
                           IDBContext.Current.Operation,
                           AttributeCPD.PAGE_NAME_PROGRESS_TO_DATE + 
                                    '|' + AttributeCPD.PAGE_NAME_PROGRESS_TO_DATE_COUNTRY,
                           IDBContext.Current.Permissions,
                           0,
                           0)
                           .ToList();

            return model;
        }

        private ProgressToDateViewModel GetProgressToDate(string operationNumber)
        {
            var model = new ProgressToDateViewModel()
            {
                ProgressToDateTab = new ProgressToDateTabViewModel()
            };

            var response = _csProgressToDateService.GetProgressToDateTab(operationNumber);
            model.ProgressToDateTab = response.ProgressToDate;
            SetViewBagErrorMessageInvalidResponse(response);
            return model;
        }

        private ProgressToDateViewModel GetUseOfCS(string operationNumber)
        {
            var model = new ProgressToDateViewModel()
            {
                UseOfCountrySystemsTab = new CSPDUseOfCountrySystemsTabViewModel()
            };

            var response = _csProgressToDateService.GetUseOfCSTab(operationNumber);
            model.UseOfCountrySystemsTab = response.UseOfCountrySystems;
            SetViewBagErrorMessageInvalidResponse(response);
            return model;
        }

        private PortfolioDetailViewModel GetDetailPortfolioApproved(string operationNumber, int rowId, CSOperationsAprovedEnum typeInstrument)
        {
            var response = _csProgressToDateService.GetPortfolioApprovedDetail(operationNumber, rowId, typeInstrument);
            var model = response.OperationDetails;
            return model;
        }

        private PortfolioDetailViewModel GetDetailPortfolioAlignment(string operationNumber, int rowId, CpdTypeOperations idTypePortfolio)
        {
            var response = _csProgressToDateService.GetPortfolioAlignmentDetail(operationNumber, rowId, idTypePortfolio);
            var model = response.OperationDetails;
            return model;
        }

        private void SetViewBagsProgressDate(string operationNumber)
        {
            var list = new List<MultiDropDownItem>();
            var response = _csProgressToDateService.SearchCSObjectives(operationNumber);
            if (response.IsValid)
            {
                list = response.ListResponse.Select(x => new MultiDropDownItem
                {
                    Value = x.Value,
                    Text = x.Text
                }).ToList();
            }

            ViewBag.CountryStrategyObjectivesList = list;
        }

        private void SetViewBagsUseCountrySys(string operationNumber)
        {
            var collectionCPDProcurementEnum = _enumMappingService.GetMasterDataCollection<CPDProcurementEnum>();
            var collectionCPDFinManag = _enumMappingService.GetMasterDataCollection<CPDFinancialManagementEnum>();
            var collectionCPDForseenAction = _enumMappingService.GetMasterDataCollection<CPDForseenActionsProgressToDate>();

            var response = _csProgressToDateService.SearchCSObjectives(operationNumber).ListResponse;
            var list = new List<MultiDropDownItem>();

            foreach (var item in response)
            {
                var dropDownItem = new MultiDropDownItem
                {
                    Value = item.Value,
                    Text = item.Text
                };

                list.Add(dropDownItem);
            }

            ViewBag.CountryStrategyObjectivesList = list;

            var currentYear = DateTime.Now.Year;
            var yearList = new List<SelectListItem>();
            for (var i = -5; i <= NUMBER_YEAR_COMBO; i++)
            {
                yearList.Add(new SelectListItem { Value = (currentYear + i).ToString(), Text = (currentYear + i).ToString() });
            }

            ViewBag.procurementList = collectionCPDProcurementEnum.ConverToSelectListItems().ConvertToMultiDropDownItem();

            ViewBag.financialManagmentList = collectionCPDFinManag.ConverToSelectListItems().ConvertToMultiDropDownItem();

            ViewBag.FinancialManagementYearList = yearList;

            ViewBag.ProcurementYearList = yearList;

            ViewBag.ForeseenActionsList = collectionCPDForseenAction.ConverToSelectListItems().ConvertToMultiDropDownItem();
        }
        #endregion
    }
}