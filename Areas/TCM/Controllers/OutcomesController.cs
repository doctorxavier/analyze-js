using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;

using Newtonsoft.Json;

using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.IndicatorsModuleNew.Services.LinkPredefinedIndicator;
using IDB.MW.Application.IndicatorsModuleNew.ViewModels.LinkPredefinedIndicator;
using IDB.MW.Application.TCM.Messages.ResultsMatrix;
using IDB.MW.Application.TCM.Services.ResultsMatrixService;
using IDB.MW.Application.TCM.Services.TcmUniverseService;
using IDB.MW.Application.TCM.ViewModels.ResultsMatrix.Common;
using IDB.MW.Application.TCM.ViewModels.ResultsMatrix.Outcomes;
using IDB.MW.Business.TCM.Contracts;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.Presentation.MVC4.Areas.TCM.Models;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;
using IResultsMatrixRepository = IDB.MW.DomainModel.Contracts.Repositories.ResultsMatrixModule.IResultsMatrixRepository;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.MW.Domain.Entities;

namespace IDB.Presentation.MVC4.Areas.TCM.Controllers
{
    public partial class OutcomesController : BaseController
    {
        #region Constants

        public const string PAGE_CHART = "UI-OC-001-Outcome,UI-OC-002Partial-OutcomeDetails";

        #endregion

        private readonly IOutcomesService _outcomesService;
        private readonly ITcmUniverseService _tcmUniverseService;
        private readonly ILinkPredefinedIndicatorService _linkPredefinedIndicatorService;
        private readonly ICommonTCMService _commonTCMService;
        private readonly IResultsMatrixRepository _resultsMatrixRepository;
        private readonly IGenericRepository<Operation> _operationRepository;
        private readonly ViewModelMapperHelper _viewModelMapperHelper;

        public OutcomesController(IOutcomesService outcomesService,
            ITcmUniverseService tcmUniverseService,
            ILinkPredefinedIndicatorService linkPredefinedIndicatorService,
            ICommonTCMService commonTCMService,
            ICatalogService catalogService,
            IResultsMatrixRepository resultsMatrixRepository,
            IGenericRepository<Operation> opRep)
        {
            _outcomesService = outcomesService;
            _tcmUniverseService = tcmUniverseService;
            _linkPredefinedIndicatorService = linkPredefinedIndicatorService;
            _commonTCMService = commonTCMService;
            _resultsMatrixRepository = resultsMatrixRepository;
            _viewModelMapperHelper = new ViewModelMapperHelper(catalogService, null);
            _operationRepository = opRep;
        }

        public virtual ActionResult Index(string operationNumber)
        {
            var operationResponse = _tcmUniverseService.GetParentOperation(operationNumber);

            if (!operationResponse.IsValid)
                throw new Exception(operationResponse.ErrorMessage);

            var operation = _operationRepository.GetOne(o =>
                o.OperationNumber == operationResponse.OperationNumber);

            var outcomesResponse = _outcomesService.GetOutcomesData(operation);

            TempData["ResultMatrix" + outcomesResponse.ResultsMatrix.ResultsMatrixId] =
                outcomesResponse.ResultsMatrix;

            outcomesResponse.OutcomesData.FieldAccessList =
                _viewModelMapperHelper.GetFieldAccessModelList(operationNumber, PAGE_CHART);

            return View(outcomesResponse.OutcomesData);
        }

        public virtual ActionResult Edit(string operationNumber)
        {
            var predefModel = _linkPredefinedIndicatorService
                .GetLinkIndicatorModel(LinkIndicatorTypeEnum.Outcomes, 
                false, 
                "Outcomes", 
                true).Filter;
            predefModel.FieldAccessList = _viewModelMapperHelper.GetFieldAccessModelList(operationNumber, PAGE_CHART);
            ViewBag.LinkIndicatorModel = predefModel;
            LockRegister(operationNumber, TCMGlobalValues.TCM_OUTCOMES_EDIT);

            return View();
        }

        public virtual JsonResult GetResultsMatrix(string operationNumber, bool isBlock = false)
        {
            var rspnse = _tcmUniverseService.GetParentOperation(operationNumber);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);

            if (isBlock)
            {
                UnlockRegister(operationNumber, TCMGlobalValues.TCM_OUTCOMES_EDIT);
            }

            var outcomesResponse = _outcomesService.GetOutcomesDataNotRM(rspnse.OperationNumber, null);

            Dictionary<string, string> paths = new Dictionary<string, string>();
            paths.Add("Edit", Url.Action("Edit", "Outcomes", new { area = "TCM" }));
            paths.Add("Index", Url.Action("Index", "Outcomes", new { area = "TCM" }));
            paths.Add("DeleteOutcomeIndicator", Url.Action("DeleteOutcomeIndicator", "Outcomes", new { area = "TCM" }));
            paths.Add("DeleteOutcomeDisaggregation", Url.Action("DeleteOutcomeDisaggregation", "Outcomes", new { area = "TCM" }));
            paths.Add("IsBlockedPlan", Url.Action("IsBlockedPlan", "Outcomes", new { area = "TCM" }));
            paths.Add("ReassignOutcomeIndicator", Url.Action("ReassignOutcomeIndicator", "Outcomes", new { area = "TCM" }));
            paths.Add("ReassignOutcomeIndicatorSave", Url.Action("ReassignOutcomeIndicatorSave", "Outcomes", new { area = "TCM" }));
            paths.Add("DeleteOutcomeStatement", Url.Action("DeleteOutcomeStatement", "Outcomes", new { area = "TCM" }));
            paths.Add("DeleteOutcomeStatementSave", Url.Action("DeleteOutcomeStatementSave", "Outcomes", new { area = "TCM" }));
            paths.Add("DeleteYearPlan", Url.Action("DeleteYearPlan", "Outcomes", new { area = "TCM" }));
            paths.Add("DeleteYearPlans", Url.Action("DeleteYearPlans", "Outcomes", new { area = "TCM" }));
            paths.Add("UnlockRegister", Url.Action("UnlockRegister", "Outcomes", new { area = "TCM" }));

            ViewBag.ApprovalYears = outcomesResponse.OutcomesData.ApprovalYears;

            ViewBag.ExistingPlannedYears = outcomesResponse.OutcomesData.ExistingPlannedYears;

            outcomesResponse.Paths = paths;

            return Json(outcomesResponse, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult Modal()
        {
            return PartialView();
        }

        public virtual ActionResult ReassignOutcomeIndicator(ReassignIndicatorViewModel reassignIndicatorModel)
        {
            return PartialView(reassignIndicatorModel);
        }

        public virtual JsonResult ReassignOutcomeIndicatorSave(ReassignIndicatorViewModel reassignIndicatorModel)
        {
            var response = _outcomesService.ReassignOutcomeIndicator(reassignIndicatorModel.CurrentOutcome.OutcomeId,
               reassignIndicatorModel.SelectedOutcomeToReassign.OutcomeId,
               reassignIndicatorModel.CurrentOutcomeIndicator.OutcomeIndicatorId,
               reassignIndicatorModel.Interval);

            return Json(response);
        }

        public virtual ActionResult DeleteOutcomeStatement(DeleteOutcomeStatementViewModel deleteOutcomeModel)
        {
            return PartialView(deleteOutcomeModel);
        }

        public virtual JsonResult DeleteOutcomeStatementSave(DeleteOutcomeStatementViewModel deleteOutcomeModel)
        {
            var response = _outcomesService.DeleteOutcomeStatement(deleteOutcomeModel);

            return Json(response);
        }

        public virtual JsonResult DeleteOutcomeIndicator(int resultsMatrixId, int outcomeIndicatorId, int interval)
        {
            var response = _outcomesService.DeleteOutcomeIndicator(resultsMatrixId, outcomeIndicatorId, interval);

            return Json(response);
        }

        public virtual JsonResult DeleteOutcomeDisaggregation(int resultsMatrixId, int outcomeDisaggregation, int interval)
        {
            var response = _outcomesService.DeleteOutcomeDisaggregation(resultsMatrixId, outcomeDisaggregation, interval);

            return Json(response);
        }

        public virtual JsonResult DeleteYearPlan(int resultsMatrixId, int year, int interval)
        {
            var response = _outcomesService.DeleteYearPlan(resultsMatrixId, year, interval);

            return Json(response);
        }

        public virtual JsonResult DeleteYearPlans(DeleteYearPlanViewModel deleteYearPlanModel)
        {
            var response = _outcomesService.DeleteYearPlan(deleteYearPlanModel);

            return Json(response);
        }

        public virtual JsonResult IsBlockedPlan(int interval, int year, int planId)
        {
            var response = new IDB.MW.Application.TCM.Messages.ResultsMatrix.BlockedResponse();
            response = _outcomesService.IsBlockedPlan(interval, year, planId, DateTime.Now.Year);
            return Json(response);
        }

        #region Save Outcomes
        public virtual JsonResult SaveOutcomes(OutcomesDataViewModel outcomesDataViewModel)
        {
            var username = IDBContext.Current.UserLoginName;
            var response = _outcomesService.SaveOutcomes(outcomesDataViewModel, outcomesDataViewModel.Interval, username);

            if (response.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(TCMGlobalValues.TCM_OUTCOMES_EDIT, response.OutcomesData.OperationNumber, username);
            }

            return Json(response);
        }
        #endregion

        #region IndicatorDetail

        public virtual ActionResult OutcomeDetail(string operationNumber, string paramsDetail)
        {
            var paramsDetails = JsonConvert.DeserializeObject<ParamsDetailViewModel>(paramsDetail);
            var model = _outcomesService.GetOutcomeIndicatorDetail(paramsDetails);
            model.OutcomeIndicatorDetailViewModel.FieldAccessList = _viewModelMapperHelper.GetFieldAccessModelList(operationNumber, PAGE_CHART);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model.OutcomeIndicatorDetailViewModel);
            ViewBag.TCMWritePermission = IDBContext.Current.HasPermission(Permission.TC_RESULT_MATRIX_WRITE_PERMISSIONS);

            return View(model.OutcomeIndicatorDetailViewModel);
        }

        public virtual ActionResult OutcomeDetailReload(string operationNumber, string paramsDetail)
        {
            var paramsDetails = JsonConvert.DeserializeObject<ParamsDetailViewModel>(paramsDetail);
            var model = _outcomesService.GetOutcomeIndicatorDetail(paramsDetails);
            model.OutcomeIndicatorDetailViewModel.FieldAccessList = _viewModelMapperHelper.GetFieldAccessModelList(operationNumber, PAGE_CHART);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model.OutcomeIndicatorDetailViewModel);
            ViewBag.TCMWritePermission = IDBContext.Current.HasPermission(Permission.TC_RESULT_MATRIX_WRITE_PERMISSIONS);

            return PartialView("Partials/OutcomeDetailPartial", model.OutcomeIndicatorDetailViewModel);
        }

        public virtual JsonResult SaveOutcomeDetail(string operationNumber)
        {
            var response = new SaveOutcomeIndicatorDetailResponse();
            var userName = IDBContext.Current.UserName;
            var loginName = IDBContext.Current.UserLoginName;
            try
            {
                var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
                var viewModel = PageSerializationHelper.DeserializeObject<OutcomeIndicatorDetailViewModel>(jsonDataRequest.SerializedData);

                viewModel.UpdateOutcomeIndicatorDetail(jsonDataRequest.ClientFieldData);
                response = _outcomesService.SaveOutcomeIndicatorDetail(viewModel, userName);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(TCMGlobalValues.TCM_OUTCOMES_EDIT, operationNumber, loginName);
                }
            }
            catch (Exception ex)
            {
                response.IsValid = false;
                response.ErrorMessage = ex.Message;
            }

            return Json(response);
        }

        #endregion

        #region downloadOutput
        public virtual FileResult DownloadFileOutcomes(string operationNumber, string formatType, string disaggregation)
        {
            var response = _outcomesService.GetOutcomesReport(operationNumber, formatType, disaggregation);
            var fileName = string.Format("{0}_Outcomes", operationNumber.Replace("-", "_"));

            if (!response.IsValid)
            {
                return null;
            }

            var application = "application/";

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
                case "pdf":
                    application = application + formatType;
                    fileName = fileName + ".pdf";
                    break;
                default:
                    application = application + "pdf";
                    fileName = fileName + ".pdf";
                    break;
            }

            return File(response.File, application, fileName);
        }
        #endregion

        #region PredefinedIndicator
        public virtual JsonResult LinkToPredefinedIndicator(LinkToPredefinedIndicatorRequest PredefinedIndicatorRequest)
        {
            var response = _commonTCMService.LinkToPredefinedIndicator(PredefinedIndicatorRequest.TcmElementToLink,
                PredefinedIndicatorRequest.TcmElementToLinkId,
                PredefinedIndicatorRequest.PredefinedIndicatorId);

            return Json(response);
        }

        public virtual JsonResult UnlinkPredefinedIndicators(UnlinkPredefinedIndicatorsRequest unlinkPredefinedIndicatorsRequest)
        {
            var response = _commonTCMService.UnlinkPredefinedIndicators(unlinkPredefinedIndicatorsRequest.TcmElementToUnlink,
                unlinkPredefinedIndicatorsRequest.TcmElementToUnlinkId,
                unlinkPredefinedIndicatorsRequest.PredefinedIndicatorIds);

            return Json(response);
        }

        public virtual JsonResult GetCRFLinkedIndicators(LinkToPredefinedIndicatorRequest GetCRFLinkedIndicators)
        {
            var response = _commonTCMService
                .GetCRFLinkedIndicators(GetCRFLinkedIndicators.TcmElementToLink,
                GetCRFLinkedIndicators.TcmElementToLinkId);

            return Json(response);
        }
        #endregion
    }
}
