using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using IDB.Architecture.DataTables.Common;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.CPDModule.Enums;
using IDB.MW.Application.CPDModule.Mappers;
using IDB.MW.Application.CPDModule.Services.Annexes;
using IDB.MW.Application.CPDModule.ViewModels.Annexes;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.Presentation.MVC4.General;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVC4.Areas.CPD.Mappers;
using IDB.Presentation.MVC4.Areas.CPD.Models;
using IDB.MW.Infrastructure.ReportManager.Enums;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Contracts.ModelRepositories.Security;
using IDB.MW.Domain.Values.CPD;
using IDB.MW.Application.CPDModule.Services.NSGIntegrationCPD;

namespace IDB.Presentation.MVC4.Areas.CPD.Controllers
{
    public partial class AnnexesController : BaseController
    {
        #region Constants
        private static int NUMBER_YEAR_COMBO = 5;
        private static string CPD_ANNEXES_RESULT_MATRIX_URL = "/CPD/Annexes/ResultMatrix";
        private static string CPD_ANNEXES_INDICATIVE_PIPELINE = "/CPD/Annexes/IndicativePipeline";
        #endregion

        #region Fields
        private readonly IAuthorizationService _authorizationService;
        private readonly IAnnexesService _annexesService;
        private readonly IEnumMappingService _enumMappingService;
        private readonly ISecurityModelRepository _securityModelRepository;
        private readonly INsgIntegrationCpdService _cpdIntegrationNSGService;
        #endregion

        #region Contructors
        public AnnexesController(
            IAuthorizationService authorizationService,
            IAnnexesService annexesService,
            IEnumMappingService enumMappingService,
            ISecurityModelRepository securityModelRepository,
            INsgIntegrationCpdService cpdIntegrationNSGService)
            : base(authorizationService)
        {
            _authorizationService = authorizationService;
            _annexesService = annexesService;
            _enumMappingService = enumMappingService;
            _securityModelRepository = securityModelRepository;
            _cpdIntegrationNSGService = cpdIntegrationNSGService;
        }
        #endregion

        #region Action Methodss
        public virtual ActionResult Read(string operationNumber, string tabName = null, string errorMessage = null)
        {
            ViewBag.ActiveTab = tabName ?? NavigationAttributes.TAB_NAME_RESULTS_MATRIX;

            if (!string.IsNullOrEmpty(errorMessage))
            {
                ViewBag.ErrorMessage = errorMessage;
            }

            var model = GetAnnexes(operationNumber);
            SetViewBags(operationNumber, model.ResultMatrixTab);
            return View("Read", model);
        }

        public virtual ActionResult Cancel(string operationNumber, string tabName = null)
        {
            ViewBag.ActiveTab = tabName ?? NavigationAttributes.TAB_NAME_RESULTS_MATRIX;

            if (tabName == null || tabName == NavigationAttributes.TAB_NAME_RESULTS_MATRIX)
            {
                SynchronizationHelper.TryReleaseLock(CPD_ANNEXES_RESULT_MATRIX_URL, operationNumber, IDBContext.Current.UserLoginName);
            }
            else
            {
                SynchronizationHelper.TryReleaseLock(CPD_ANNEXES_INDICATIVE_PIPELINE, operationNumber, IDBContext.Current.UserLoginName);
            }

            var model = GetAnnexes(operationNumber);
            SetViewBags(operationNumber, model.ResultMatrixTab);
            return View("Read", model);
        }

        public virtual ActionResult EditResultMatrix(string operationNumber)
        {
            ViewBag.ActiveTab = NavigationAttributes.TAB_NAME_RESULTS_MATRIX;
            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, CPD_ANNEXES_RESULT_MATRIX_URL, operationNumber, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                return RedirectToAction("Read", new { tabName = NavigationAttributes.TAB_NAME_RESULTS_MATRIX, errorMessage = errorMessage });
            }

            var model = GetResultMatrixTab(operationNumber);
            ViewBag.SerializedModel = PageSerializationHelper.SerializeObject(model.ResultMatrixTab);
            SetViewBags(operationNumber, model.ResultMatrixTab);
            return View("Edit", model);
        }

        public virtual ActionResult EditIndicativePipeline(string operationNumber)
        {
            ViewBag.ActiveTab = NavigationAttributes.TAB_NAME_INDICATIVE_PIPELINE;
            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, CPD_ANNEXES_INDICATIVE_PIPELINE, operationNumber, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                return RedirectToAction("Read", new { tabName = NavigationAttributes.TAB_NAME_INDICATIVE_PIPELINE, errorMessage = errorMessage });
            }

            var model = GetIndicativePipelineTab(operationNumber);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model.IndicativePipelineTab);
            SetViewBags(operationNumber, null);
            return View("Edit", model);
        }

        public virtual JsonResult SaveResultMatrix(string operationNumber)
        {
            var responseView = new SaveResponse() { IsValid = true, ErrorMessage = string.Empty };

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var model = PageSerializationHelper.DeserializeObject<ResultMatrixTabViewModel>(jsonDataRequest.SerializedData);

            model.Update(jsonDataRequest.ClientFieldData);

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, CPD_ANNEXES_RESULT_MATRIX_URL, operationNumber, IDBContext.Current.UserLoginName);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            var response = _annexesService.SaveResultMatrixTab(operationNumber, model);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if (responseView.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(CPD_ANNEXES_RESULT_MATRIX_URL, operationNumber, IDBContext.Current.UserLoginName);
                responseView.UrlRedirect = Url.Action("Read", "Annexes", new { area = "CPD", tabName = NavigationAttributes.TAB_NAME_RESULTS_MATRIX });
            }

            return Json(responseView);
        }

        public virtual ActionResult SaveIndicativePipeline(string operationNumber)
        {
            var responseView = new SaveResponse() { IsValid = true, ErrorMessage = string.Empty };

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<IndicativePipelineTabViewModel>(jsonDataRequest.SerializedData);
            var formData = jsonDataRequest.ClientFieldData;

            viewModel.UpdateIndicativePipelineTabViewModel(formData);

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, CPD_ANNEXES_INDICATIVE_PIPELINE, operationNumber, IDBContext.Current.UserLoginName);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            var response = _annexesService.SaveIndicativePipelineTab(operationNumber, viewModel);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if (responseView.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(CPD_ANNEXES_INDICATIVE_PIPELINE, operationNumber, IDBContext.Current.UserLoginName);
                responseView.UrlRedirect = Url.Action("Read", "Annexes", new { area = "CPD", tabName = NavigationAttributes.TAB_NAME_INDICATIVE_PIPELINE });
            }

            return Json(responseView);
        }

        public virtual FileResult CPDResultsMatrixExportToPDF(string operationNumber)
        {
            var response = _annexesService.ExportCPDResultsMatrixFile(operationNumber, OutputFormatEnum.Word);

            if (!response.IsValid)
            {
                return null;
            }

            return File(response.File, "application/doc", "CPDResultsMatrix.doc");
        }
        #endregion

        #region Private Methods
        private AnnexesViewModel GetAnnexes(string operationNumber)
        {
            //ToDo Mock
            var model = new AnnexesViewModel();

            var response = _annexesService.GetAnnexes(operationNumber);

            model.ResultMatrixTab = response.ResultMatrixTab;
            model.IndicativePipelineTab = response.IndicativePipelineTab;

            model.IsOperationClosed = response.IsOperationClosed;

            SetViewBagErrorMessageInvalidResponse(response);

            model.FieldAccessList = _securityModelRepository.GetFieldBehaviorByPermissions(
                                       IDBContext.Current.Operation,
                                       AttributeCPD.PAGE_NAME_ANNEXES_RESULT_MATRIX +
                                                '|' + AttributeCPD.PAGE_NAME_ANNEXES_INDICATIVE,
                                       IDBContext.Current.Permissions,
                                       0,
                                       0)
                                       .ToList();

            return model;
        }

        private AnnexesViewModel GetResultMatrixTab(string operationNumber)
        {
            //ToDo Mock
            var model = new AnnexesViewModel();

            model.ResultMatrixTab = _annexesService.GetResultMatrixTab(operationNumber).ResultMatrixTab;

            return model;
        }

        private AnnexesViewModel GetIndicativePipelineTab(string operationNumber)
        {
            //ToDo Mock
            var model = new AnnexesViewModel();

            var response = _annexesService.GetIndicativePipelineTab(operationNumber);

            model.IndicativePipelineTab = response.IndicativePipelineTab;

            SetViewBagErrorMessageInvalidResponse(response);

            return model;
        }

        private void SetViewBags(string operationNumber, ResultMatrixTabViewModel resultMatrix)
        {
            var collectionCPDFinManag = _enumMappingService.GetMasterDataCollection<CPDFinancialManagementEnum>();
            var collectionCPDProcurementEnum = _enumMappingService.GetMasterDataCollection<CPDProcurementEnum>();
            var collectionCPDForseenAction = _enumMappingService.GetMasterDataCollection<CPDForseenActionsCSEnum>();

            var currentYear = DateTime.Now.Year;
            var yearList = new List<SelectListItem>();
            for (var i = 0; i <= NUMBER_YEAR_COMBO; i++)
            {
                yearList.Add(new SelectListItem { Value = (currentYear + i).ToString(), Text = (currentYear + i).ToString() });
            }

            ViewBag.financialManagmentList = collectionCPDFinManag.ConverToSelectListItems().ConvertToMultiDropDownItem();

            ViewBag.procurementList = collectionCPDProcurementEnum.ConverToSelectListItems().ConvertToMultiDropDownItem();

            ViewBag.FinancialManagementYearList = yearList;

            ViewBag.ProcurementYearList = yearList;

            ViewBag.ForeseenActionsList = collectionCPDForseenAction.ConverToSelectListItems().ConvertToMultiDropDownItem();

            ViewBag.CRFIndicators = new List<SelectListItem>();
            if (resultMatrix != null)
            {
                var allCRFIndicatorIdUsed = resultMatrix.StrategicObjectives.SelectMany(x => x.StrategicOutcomes).SelectMany(y => y.LinkedIndicators).Distinct();
                var crfIndicatorResponse = _annexesService.GetCRFIndicatorsById(allCRFIndicatorIdUsed);
                ViewBag.CRFIndicators = crfIndicatorResponse.CRFIndicators;
            }
        }
        #endregion
    }
}