using System;
using System.Web.Mvc;

using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.IndicatorsModuleNew.Services.LinkPredefinedIndicator;
using IDB.MW.Application.IndicatorsModuleNew.ViewModels.LinkPredefinedIndicator;
using IDB.MW.Application.TCM.Messages.ResultsMatrix;
using IDB.MW.Application.TCM.Services.ResultsMatrixService;
using IDB.MW.Application.TCM.Services.TcmUniverseService;
using IDB.MW.Application.TCM.ViewModels.ResultsMatrix.Common.PredefinedIndicators;
using IDB.MW.Application.TCM.ViewModels.ResultsMatrix.Components;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.Architecture.Language;
using IDB.Presentation.MVC4.Areas.TCM.Models;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.TCM.Controllers
{
    public partial class ComponentsController : BaseController
    {
        public static string PAGE_CHART = "UI-COM-002Partial-TabPhysicalProgress,UI-COM-003Partial-TabFinancialProgress,UI-COM-004Partial-TabMappingProgress,UI-COM-005Partial-OutputIndicatorDetails";

        private readonly IComponentService _componentService;
        private readonly ICatalogService _catalogService;
        private readonly ViewModelMapperHelper _viewModelMapperHelper;
        private readonly ITcmUniverseService _tcmUniverseService;
        private readonly ILinkPredefinedIndicatorService _linkPredefinedIndicatorService;

        public ComponentsController(
            IComponentService componentService,
            ICatalogService catalogService,
            ITcmUniverseService tcmUniverseService,
            ILinkPredefinedIndicatorService linkPredefinedIndicatorService)
        {
            _componentService = componentService;
            _catalogService = catalogService;
            _viewModelMapperHelper = new ViewModelMapperHelper(_catalogService, null);
            _tcmUniverseService = tcmUniverseService;
            _linkPredefinedIndicatorService = linkPredefinedIndicatorService;
        }

        public virtual ActionResult Index(
            string operationNumber,
            bool isMilestonesEnabled = false,
            bool isDisaggregationsEnabled = false,
            bool unlock = false)
        {
            var predefModel = _linkPredefinedIndicatorService
                .GetLinkIndicatorModel(LinkIndicatorTypeEnum.Outputs, 
                false, 
                "Outputs", 
                true).Filter;

            ViewBag.LinkIndicatorModel = predefModel;

            var rspnse = _tcmUniverseService.GetParentOperation(operationNumber);

            if (unlock)
            {
                UnlockRegister(operationNumber, TCMGlobalValues.URL_PHYSICAL_PROGRESS);
            }

            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);

            var model = _componentService.GetResultsMatrix(
                rspnse.OperationNumber,
                Localization.CurrentLanguage,
                isMilestonesEnabled,
                isDisaggregationsEnabled,
                false).Components;

            model.FieldAccessList = _viewModelMapperHelper.GetFieldAccessModelList(operationNumber, PAGE_CHART);
            model.PhysicalProgress.FieldAccessList = model.FieldAccessList;
            model.MappingProgress.FieldAccessList = model.FieldAccessList;

            return View(model);
        }

        public virtual JsonResult IndexPhysicalProgress(GetPhysicalProgressRequest physicalProgressRequest)
        {
            var rspnse = _tcmUniverseService.GetParentOperation(physicalProgressRequest.OperationNumber);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);
            var response = _componentService.GetPhysicalProgress(rspnse.OperationNumber, Localization.CurrentLanguage);

            return Json(response);
        }

        public virtual ActionResult IndexFinancialProgressReload(string operationNumber, bool showInactiveOutputs = false)
        {
            var rspnse = _tcmUniverseService.GetParentOperation(operationNumber);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);
            var model = _componentService.GetResultsMatrix(rspnse.OperationNumber, string.Empty, showInactiveOutputs);
            return PartialView("Partial/FinancialProgress", model.Components.FinancialProgress);
        }

        public virtual ActionResult IndexMappingProgressReload(string operationNumber)
        {
            var rspnse = _tcmUniverseService.GetParentOperation(operationNumber);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);

            var model = _componentService.GetResultsMatrix(rspnse.OperationNumber, string.Empty);
            model.Components.MappingProgress.FieldAccessList = _viewModelMapperHelper.GetFieldAccessModelList(operationNumber, PAGE_CHART);

            return PartialView("Partial/MappingProgress", model.Components.MappingProgress);
        }

        public virtual JsonResult ToBlockByChangeInView(string operationNumber, string url)
        {
            var response = new ResponseBase { IsValid = true };
            var errorMessage = SynchronizationHelper.AccessToResources(
               "edit",
               url,
               operationNumber,
               IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response.IsValid = false;
                response.ErrorMessage = errorMessage;
            }

            return Json(response);
        }

        public virtual JsonResult UnlockChangeInView(string operationNumber, string url)
        {
            var response = new ResponseBase { IsValid = true };
            SynchronizationHelper.TryReleaseLock(url, operationNumber, IDBContext.Current.UserLoginName);
            return Json(response);
        }

        public virtual FileResult DownloadFileFinancialProgress(
            string operationNumber, 
            string formatType, 
            string inactiveOutput)
        {
            var rspnse = _tcmUniverseService.GetParentOperation(operationNumber);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);

            var response = _componentService.GetFinancialProgressReport(
                rspnse.OperationNumber, 
                formatType, 
                inactiveOutput, 
                Localization.CurrentLanguage);

            if (!response.IsValid)
            {
                return null;
            }

            var application = "application/";
            var fileName = 
                string.Format("{0}_Outputs_Fin_Progress", operationNumber.Replace("-", "_"));

            switch (formatType)
            {
                case "doc":
                    application = application + "msword";
                    fileName = fileName + ".doc";
                    break;
                case "xls":
                    application = application + "vnd.ms-excel";
                    fileName = fileName + ".xls";
                    break;
                default:
                    application = application + "pdf";
                    fileName = fileName + ".pdf";
                    break;
            }

            return File(response.File, application, fileName);
        }

        public virtual FileResult DownloadFilePhysicalProgress(
            string operationNumber,
            string formatType,
            string inactiveOutput,
            string milestones,
            string disaggregation)
        {
            var rspnse = _tcmUniverseService.GetParentOperation(operationNumber);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);

            var response = _componentService
                .GetPhysicalProgressReport(
                    rspnse.OperationNumber,
                    formatType,
                    inactiveOutput,
                    milestones,
                    disaggregation,
                    Localization.CurrentLanguage);

            var fileName = 
                string.Format("{0}_Outputs_Phy_Progress", operationNumber.Replace("-", "_"));

            if (!response.IsValid)
            {
                return null;
            }

            var application = "application/";

            switch (formatType)
            {
                case "doc":
                    application = application + "msword";
                    fileName = fileName + "." + formatType;
                    break;
                case "xls":
                    application = application + "vnd.ms-excel";
                    fileName = fileName + "." + formatType;
                    break;
                case "pdf":
                    application = application + formatType;
                    fileName = fileName + "." + formatType;
                    break;
                default:
                    application = application + "pdf";
                    fileName = fileName + ".pdf";
                    break;
            }

            return File(response.File, application, fileName);
        }

        public virtual JsonResult GetFundIndicatorsByTheme(
            string operationNumber, IndicatorThemeViewModel themeModel)
        {
            var rspnse = _tcmUniverseService.GetParentOperation(operationNumber);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);

            var response = _componentService
                .GetFundIndicatorsByTheme(rspnse.OperationNumber, themeModel);

            return Json(response);
        }

        public virtual JsonResult GetFundIndicatorsByThemeId(
            string operationNumber, int idTheme, int outputId)
        {
            var rspnse = _tcmUniverseService.GetParentOperation(operationNumber);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);
            var response = _componentService
                .GetFundIndicatorsByThemeId(rspnse.OperationNumber, idTheme, outputId);

            return Json(response);
        }

        public virtual JsonResult DeleteVisualOutputVersion(string operationNumber, string vovId)
        {
            var response = _componentService.DeleteVisualOutputVersion(vovId);
            return Json(response);
        }

        public virtual JsonResult DeleteOtherCost(string id, int interval)
        {
            var response = _componentService.DeleteOtherCost(id, interval);
            return Json(response);
        }

        public virtual ActionResult AddRowTableOtherCost(
            string interval, string year, string periodYear)
        {
            var model = _componentService.AddNewRowOtherCostYear(interval, year, periodYear);
            return PartialView("Partial/AddNewOtherCost", model);
        }

        public virtual JsonResult GetValidationElementToDelete(int componentId, int outputId)
        {
            var response = _componentService.ValidateElementToDelete(componentId, outputId);

            return Json(response);
        }

        public virtual JsonResult GetStandardOutputDetail(
            PredefinedIndicatorViewModel standardOutputRequest)
        {
            var response = _componentService.GetStandardOutputDetail(standardOutputRequest);

            return Json(response);
        }

        public virtual JsonResult GetLinkedProcurementProcesses(OutputViewModel output)
        {
            var response = _componentService.GetProcurementByOutput(output.OutputId);

            return Json(response);
        }

        public virtual ActionResult OutputMilestoneDetail(string operationNumber, int id)
        {
            var rspnse = _tcmUniverseService.GetParentOperation(operationNumber);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);
            var model = _componentService.GetMilestoneDetail(rspnse.OperationNumber, id);
            ViewBag.ApprovalYears = _componentService
                .GetApprovaListItems(model.OutputMilestoneDetail.OperationId);
            model.OutputMilestoneDetail.FieldAccessList = _viewModelMapperHelper
                .GetFieldAccessModelList(operationNumber, PAGE_CHART);

            return View(model.OutputMilestoneDetail);
        }

        public virtual ActionResult OutputMilestoneDetailReload(string operationNumber, int id)
        {
            var rspnse = _tcmUniverseService.GetParentOperation(operationNumber);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);

            var model = _componentService.GetMilestoneDetail(rspnse.OperationNumber, id);
            ViewBag.ApprovalYears = _componentService
                .GetApprovaListItems(model.OutputMilestoneDetail.OperationId);
            model.OutputMilestoneDetail.FieldAccessList = _viewModelMapperHelper
                .GetFieldAccessModelList(operationNumber, PAGE_CHART);

            return PartialView("Partial/OutputMilestoneDetailPartial", model.OutputMilestoneDetail);
        }

        public virtual ActionResult OutputIndicatorDetail(string operationNumber, int id)
        {
            var rspnse = _tcmUniverseService.GetParentOperation(operationNumber);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);
            var model = _componentService.GetOutputIndicatorDetail(
                rspnse.OperationNumber, id, Localization.CurrentLanguage);
            model.OutputIndicatorDetail.FieldAccessList = _viewModelMapperHelper
                .GetFieldAccessModelList(operationNumber, PAGE_CHART);

            return View(model.OutputIndicatorDetail);
        }

        public virtual ActionResult OutputIndicatorDetailReload(string operationNumber, int id)
        {
            var rspnse = _tcmUniverseService.GetParentOperation(operationNumber);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);
            var model = _componentService.GetOutputIndicatorDetail(
                rspnse.OperationNumber, id, Localization.CurrentLanguage);
            model.OutputIndicatorDetail.FieldAccessList = _viewModelMapperHelper
                .GetFieldAccessModelList(operationNumber, PAGE_CHART);

            return PartialView("Partial/OutputIndicatorDetailPartial", model.OutputIndicatorDetail);
        }
    }
}