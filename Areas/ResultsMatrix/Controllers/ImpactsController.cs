using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.Cache;
using IDB.Architecture.Diagnostics;
using IDB.Architecture.Language;
using IDB.Domain.Attributes;
using IDB.MW.Application.DEMModule.Services.Core.Interfaces;
using IDB.MW.Application.IndicatorsModuleNew.Services.LinkPredefinedIndicator;
using IDB.MW.Application.IndicatorsModuleNew.ViewModels.LinkPredefinedIndicator;
using IDB.MW.Application.PCRModule.Helpers;
using IDB.MW.Application.RMModule.Services;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.ResultMatrix.Impacts;
using IDB.MW.Domain.Contracts.ModelRepositories.Reports.Generic;
using IDB.MW.Domain.Contracts.Specifications;
using IDB.MW.Domain.Models.Architecture.Enumerations;
using IDB.MW.Domain.Models.Architecture.ResultMatrix.Common;
using IDB.MW.Domain.Models.Architecture.ResultMatrix.Impacts;
using IDB.MW.Domain.Models.Common;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.DomainModel.Contracts.Repositories.PCRModule;
using IDB.MW.Infrastructure.Data.Optima.Repositories.Models.LifeCycle;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVC4.Models.Impacts;
using IDB.Presentation.MVCExtensions;

namespace IDB.Presentation.MVC4.Areas.ResultsMatrix.Controllers
{
    public partial class ImpactsController : BaseController
    {
        private readonly ILinkPredefinedIndicatorService _linkPredefinedIndicatorService;
        private readonly IReportsService _reportsService;
        public virtual Common.Logging.ILog Logger { get; set; }
        private IResultsMatrixModelRepository _clientResultsMatrix = null;
        private ICacheManagement _cacheData = null;
        private ICacheManagement _cacheDataResultMatrixEop = null;
        private readonly IDEMLockModulesService _demLockModulesService;
        private string _impactsCacheName = string.Empty;
        private string _resultMatrixEOPCacheName;
        private int _timeCachingVal = int.Parse(ConfigurationManager.AppSettings["CacheLongTime"].ToString());

        #region WCF  REPOSITORY SERVICES        

        public ImpactsController(
            ILinkPredefinedIndicatorService linkPredefinedIndicatorService,
            IReportsService reportsService,
            IResultsMatrixModelRepository clientResultsMatrix,
            ICacheManagement cacheData,
            ICacheManagement cacheDataResultMatrixEop,
            IDEMLockModulesService demLockModulesService)
        {
            _linkPredefinedIndicatorService = linkPredefinedIndicatorService;
            _reportsService = reportsService;
            _clientResultsMatrix = clientResultsMatrix;
            _cacheData = cacheData;
            _cacheDataResultMatrixEop = cacheDataResultMatrixEop;
            _resultMatrixEOPCacheName = string.Format(
                CacheNames.IMPACTS_EOP, IDBContext.Current.Operation);
            _impactsCacheName = string.Format(CacheNames.IMPACTS, IDBContext.Current.Operation);
            _demLockModulesService = demLockModulesService;
        }

        private IPCRWorkflowStatusRepository _pcrWorkflowStatusRepository;
        public virtual IPCRWorkflowStatusRepository PCRWorkflowStatusRepository
        {
            get { return _pcrWorkflowStatusRepository; }
            set { _pcrWorkflowStatusRepository = value; }
        }

        private IReportsGenericRepository _ClientGenericRepository = null;
        public IReportsGenericRepository ClientGenericRepository
        {
            get { return _ClientGenericRepository; }
            set { _ClientGenericRepository = value; }
        }

        #endregion

        // GET: /ResultsMatrix/Impacts/Index/
        [ExceptionHandling]
        public virtual ActionResult Index(
            string operationNumber,
            int state = 0,
            int filter = 0,
            bool showInactive = false)
        {
            SynchronizationHelper.AccessToResources(
                RMIndicatorValues.CANCEL,
                RMIndicatorValues.URL_RM_IMPACTS,
                operationNumber,
                IDBContext.Current.UserLoginName);

            new IDB.Presentation.MVC4.Controllers.CommonDocument().Log(LogType.Debug,
                "MVC4(IMP_1S) - CALL WCF MODELS EVERNEXT",
                "FindOneOperationModel -> BEFORE = OPERATION NUMBER:{0} ", 
                operationNumber);

            // Retrieve OperationModel
            var operationModel = _clientResultsMatrix
                .FindOneOperationModel(new OperationSpecification() { OperationNumber = operationNumber });
            new IDB.Presentation.MVC4.Controllers.CommonDocument().Log(LogType.Debug,
                "MVC4(IMP_1E) - CALL WCF MODELS EVERNEXT",
                "FindOneOperationModel -> AFTER = RESPONSE ",
                operationModel);

            // Retrieve ResultsMatrix associated to last PMR Cycle
            ResultsMatrixModel resultMatrixModel = null;

            // Get ResultsMatrix associated to last PMR Cycle
            if (operationModel != null)
            {
                operationModel.AccessedByAdministrator =
                    IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);
                ViewBag.isLessTaskFive = PCRHelpers.IsCurrentTaskLessThanFive(
                    ClientGenericRepository, PCRWorkflowStatusRepository);

                new IDB.Presentation.MVC4.Controllers.CommonDocument().Log(LogType.Debug,
                    "MVC4(IMP_2S) - CALL WCF MODELS SERVICE",
                    "GetResultsMatrixModel -> BEFORE = OPERATION NUMBER:{0} ",
                    operationModel);

                if (!showInactive)
                {
                    resultMatrixModel = _cacheData.Get<ResultsMatrixModel>(_impactsCacheName) ??
                        _cacheData.Add(_impactsCacheName,
                        _clientResultsMatrix.GetResultsMatrixModel(operationModel, showInactive),
                        _timeCachingVal);
                    resultMatrixModel.AccessedByAdministrator =
                        operationModel.AccessedByAdministrator;
                }
                else
                {
                    resultMatrixModel = _clientResultsMatrix
                        .GetResultsMatrixModel(operationModel, showInactive);
                }

                resultMatrixModel.showInactiveImpacts = showInactive;
                PutResultMatrixEopInCache(_resultMatrixEOPCacheName, resultMatrixModel);

                new IDB.Presentation.MVC4.Controllers.CommonDocument().Log(LogType.Debug,
                    "MVC4(IMP_2E) - CALL WCF MODELS SERVICE",
                    "GetResultsMatrixModel -> AFTER = RESPONSE: ",
                    resultMatrixModel);
            }

            ViewData["filter"] = filter;

            if (state != 0)
            {
                MessageConfiguration message = MessageHandler.setMessageRMImpacts(state, false, 1);
                ViewData["message"] = message;
            }

            if (resultMatrixModel != null)
            {
                resultMatrixModel.DEMLockReviewProcessData = _demLockModulesService
               .BuildLockReviewProcessDEMDataModel(operationNumber);
            }
           
            return View(resultMatrixModel);
        }

        [HttpGet]
        [HasPermission(Permissions = "Results Matrix Write")]
        public virtual ActionResult Edit(
            int operationId,
            int resultsMatrixId,
            int code = 0,
            int filter = 0,
            int newIndicator = -1)
        {
            var operationModel = _clientResultsMatrix
                .FindOneOperationModel(new OperationSpecification() { OperationId = operationId });

            operationModel.AccessedByAdministrator = 
                IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);

            if (code != 0)
            {
                _cacheData.Remove(
                    _impactsCacheName, System.Web.Caching.CacheItemRemovedReason.Removed);
            }

            var resultModel = _cacheData.Get<ResultsMatrixModel>(_impactsCacheName) ??
                        _cacheData.Add(_impactsCacheName,
                        _clientResultsMatrix.GetResultsMatrixModel(operationModel),
                        _timeCachingVal);
            resultModel.AccessedByAdministrator = operationModel.AccessedByAdministrator;

            PutResultMatrixEopInCache(_resultMatrixEOPCacheName, resultModel);

            ViewData["listBoxYearsPlan"] = GetYearPlans(resultModel.Operation.OperationId);
            ViewData["filter"] = filter;
            ViewData["currentYearsPlan"] = resultModel.Impacts.Count() > 0 ?
                resultModel.Impacts.FirstOrDefault().GetMaxIndicatorYearPlanRows() : null;

            if (code != 0)
            {
                MessageConfiguration message = MessageHandler.setMessageRMImpacts(code, false, 1);
                ViewData["message"] = message;
            }

            var modelIndicators = _linkPredefinedIndicatorService
              .GetLinkIndicatorModel(
                 LinkIndicatorTypeEnum.Impacts,
                 false,
                 LinkIndicatorTypeEnum.Impacts.ToString(),
                 true);
            ViewBag.ModelIndicators = modelIndicators.Filter;

            return View(resultModel);
        }

        [HttpPost]
        [HasPermission(Permissions = "Results Matrix Write")]
        public virtual ActionResult Edit(ResultsMatrixModel resultModel)
        {
            int code = 0;
            PmrCycleModel pmrModel = _clientResultsMatrix.GetPmrCycle(resultModel);
            resultModel.Operation = _clientResultsMatrix.FindOneOperationModel(
                new OperationSpecification() { OperationId = resultModel.OperationId });

            List<ImpactModel> impacts = resultModel.Impacts;
            resultModel.PmrCycle = pmrModel;
            resultModel.ValidationStage = _clientResultsMatrix.FindOneConvergenceMasterDataModel(
                new ConvergenceMasterDataSpecification() { ConvergenceMasterDataId = resultModel.ValidationStageId });

            try
            {
                _clientResultsMatrix.Save(resultModel, IDBContext.Current.UserName);
                code = 501;

                _cacheData.Remove(_impactsCacheName, System.Web.Caching.CacheItemRemovedReason.Removed);
            }
            catch (Exception)
            {
                code = 499;
            }

            return RedirectToAction(
                "Index",
                "Impacts",
                new
                {
                    operationNumber = resultModel.Operation.OperationNumber,
                    state = code,
                    filter = resultModel.filter
                });
        }

        [HttpGet]
        public virtual ActionResult Warning()
        {
            return PartialView();
        }

        [HttpGet]
        public virtual ActionResult Delete(
            int impactId, int intervalId, bool accessedByAdmin, bool isThirInterval)
        {
            CustomDeleteImpactModel deleteImpactModel = new CustomDeleteImpactModel();

            int totalRows = 0;

            ImpactModel currentImpact = _clientResultsMatrix.FindOneImpactModel(
                new ImpactSpecification() { ImpactId = impactId });

            var existingImpacts = _clientResultsMatrix.FindAllImpactModelsBySpecification(
                totalRows,
                new ImpactSpecification() { ResultsMatrixId = currentImpact.ResultsMatrixId },
                int.MaxValue, 
                0,
                null).ToList<ImpactModel>();

            existingImpacts.Where(x => x.ImpactId == currentImpact.ImpactId).ForEach(x =>
               x.Statement = "Delete");
            existingImpacts.Sort((x, y) => x.Statement.CompareTo(y.Statement));

            deleteImpactModel.currentImpact = currentImpact;

            deleteImpactModel.existingImpacts = existingImpacts.ToList();

            deleteImpactModel.intervalId = intervalId;

            deleteImpactModel.resultsMatrixId = currentImpact.ResultsMatrixId;

            deleteImpactModel.AccessedByAdministrator = accessedByAdmin;

            if (intervalId == ResultsMatrixCodes.ThirdInterval || (accessedByAdmin && isThirInterval))
            {
                deleteImpactModel.IsThirdInterval = isThirInterval;
                ViewData["message"] = Localization.GetText(
                    "This change will be registered on the 'Changes to the Matrix' " +
                    "section and you will be required to explain it. Would you like to proceed?");
            }
            else
            {               
                deleteImpactModel.IsThirdInterval = false;
            }

            deleteImpactModel.IsValidated = _clientResultsMatrix.IsValidateImpactByImpactIndicatorYearPlans(impactId);
            return PartialView(deleteImpactModel);
        }

        [ExceptionHandling]
        public virtual ActionResult DeleteImpact(CustomDeleteImpactModel deleteModel)
        {
            int code = 0;
            var impact = _clientResultsMatrix.FindOneImpactModel(
                new ImpactSpecification() { ImpactId = deleteModel.currentImpact.ImpactId });

            try
            {
                List<ImpactIndicatorModel> outcomes = new List<ImpactIndicatorModel>();

                foreach (var outcomeIndicator in deleteModel.currentImpact.ImpactIndicators)
                {
                    var currentOutcome = this._clientResultsMatrix.FindOneImpactIndicatorModel(
                        new ImpactIndicatorSpecification
                        {
                            ImpactIndicatorId = outcomeIndicator.ImpactIndicatorId
                        });

                    if (currentOutcome != null)
                    {
                        currentOutcome.ReassignToImpact = outcomeIndicator.ReassignToImpact;
                        outcomes.Add(currentOutcome);
                    }
                }

                deleteModel.currentImpact.ImpactIndicators = outcomes;

                _clientResultsMatrix.DeleteImpact(
                    new ResultsMatrixModel
                    {
                        ResultsMatrixId = deleteModel.resultsMatrixId,
                        AccessedByAdministrator = deleteModel.AccessedByAdministrator,
                        IsThirdInterval = deleteModel.IsThirdInterval
                    },
                    deleteModel.currentImpact,
                    new IntervalModel() { IntervalId = deleteModel.intervalId },
                    IDBContext.Current.UserName);

                code = 506;
            }
            catch (Exception)
            {
                code = 404;
                throw;
            }

            var resultsMatrix = _clientResultsMatrix.FindOneModel(
                new ResultsMatrixSpecification() { ResultsMatrixId = impact.ResultsMatrixId });

            _cacheData.Remove(_impactsCacheName, System.Web.Caching.CacheItemRemovedReason.Removed);

            return RedirectToAction(
                "Edit", 
                "Impacts",
                new
                {
                    operationId = resultsMatrix.Operation.OperationId,
                    resultsMatrixId = resultsMatrix.ResultsMatrixId,
                    code = code
                });
        }

        [HttpGet]
        public virtual ActionResult SaveChanges(int changesCount = 0)
        {
            ViewData["changesCount"] = changesCount;

            return PartialView();
        }

        public virtual FileResult DownloadReportImpacts(
          string operationNumber,
          string formatType,
          string disaggregation)
        {
            var response = _reportsService.GetImpactsReport(
                operationNumber,
                formatType,
                disaggregation);

            if (!response.IsValid)
                return null;

            return File(response.File, response.Application, response.FileName);
        }

        public void PutResultMatrixEopInCache(string _resultMatrixEOPCacheName, ResultsMatrixModel model)
        {
            ResultMatrixEOPModel resultMatrixEopInCache = _cacheDataResultMatrixEop
                .Get<ResultMatrixEOPModel>(_resultMatrixEOPCacheName);

            if (resultMatrixEopInCache != null && model.EndProjectYear != resultMatrixEopInCache.EopYear)
            {
                _cacheDataResultMatrixEop.Add<ResultMatrixEOPModel>(
                _resultMatrixEOPCacheName,
                _clientResultsMatrix.GetResultMatrixEOP(model.EndProjectYear, resultMatrixEopInCache),
                _timeCachingVal);
            }
            else
            {
                _cacheDataResultMatrixEop.Add<ResultMatrixEOPModel>(
                    _resultMatrixEOPCacheName,
                    _clientResultsMatrix.GetNewResultMatrixEOP(model.EndProjectYear, model.ResultsMatrixId),
                    _timeCachingVal);
            }
        }

        List<int> GetYearPlans(int operationId)
        {
            LifeCycleEventsResolver lcResolver = new LifeCycleEventsResolver(operationId);
            DateTime onPipelineDate = lcResolver.ResolvePipelineDate() ?? DateTime.MinValue;

            int startYearPlan = onPipelineDate.Year - 10;

            return Enumerable.Range(startYearPlan, 40).ToList();
    }
}
}
