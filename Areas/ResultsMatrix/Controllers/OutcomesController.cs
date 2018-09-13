using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.Cache;
using IDB.Architecture.Diagnostics;
using IDB.Architecture.Language;
using IDB.Architecture.Logging;
using IDB.Domain.Attributes;
using IDB.MW.Application.DEMModule.Services.Core.Interfaces;
using IDB.MW.Application.IndicatorsModuleNew.Services.LinkPredefinedIndicator;
using IDB.MW.Application.IndicatorsModuleNew.ViewModels.LinkPredefinedIndicator;
using IDB.MW.Application.PCRModule.Helpers;
using IDB.MW.Application.RMModule.Services;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.ResultMatrix.Outcomes;
using IDB.MW.Domain.Contracts.ModelRepositories.Reports.Generic;
using IDB.MW.Domain.Contracts.Specifications;
using IDB.MW.Domain.Models.Architecture.Enumerations;
using IDB.MW.Domain.Models.Architecture.ResultMatrix.Common;
using IDB.MW.Domain.Models.Architecture.ResultMatrix.Outcomes;
using IDB.MW.Domain.Models.Common;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.DomainModel.Contracts.Repositories.PCRModule;
using IDB.MW.Infrastructure.Data.Optima.Repositories.Models.LifeCycle;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVC4.Models.Outcomes;
using IDB.Presentation.MVCExtensions;

namespace IDB.Presentation.MVC4.Areas.ResultsMatrix.Controllers
{
    public partial class OutcomesController : BaseController
    {
        private readonly ILinkPredefinedIndicatorService _linkPredefinedIndicatorService;
        private readonly IReportsService _reportsService;
        readonly IDEMLockModulesService _demLockModulesService;
        private IResultsMatrixModelRepository _clientResultsMatrix = null;
        private ICacheManagement _cacheData = null;
        private ICacheManagement _cacheDataResultMatrixEop = null;
        private IPCRWorkflowStatusRepository _pcrWorkflowStatusRepository = null;
        private IReportsGenericRepository _ClientGenericRepository = null;
        private int _timeCachingVal =
            int.Parse(ConfigurationManager.AppSettings["CacheLongTime"].ToString());
        private string _outcomesCacheName = string.Empty;
        private string _resultMatrixEOPCacheName;

        public OutcomesController(
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
            _outcomesCacheName = string.Format(CacheNames.OUTCOMES, IDBContext.Current.Operation);
            _resultMatrixEOPCacheName = string.Format(
                  CacheNames.OUTCOMES_EOP, IDBContext.Current.Operation);
            _demLockModulesService = demLockModulesService;
        }

        public virtual IPCRWorkflowStatusRepository PCRWorkflowStatusRepository
        {
            get { return _pcrWorkflowStatusRepository; }
            set { _pcrWorkflowStatusRepository = value; }
        }

        public IReportsGenericRepository ClientGenericRepository
        {
            get { return _ClientGenericRepository; }
            set { _ClientGenericRepository = value; }
        }

        public virtual IResultsMatrixModelRepository ClientResultsMatrix
        {
            get { return _clientResultsMatrix; }
            set { _clientResultsMatrix = value; }
        }

        [ExceptionHandling]
        public virtual ActionResult Index(
            string operationNumber,
            int state = 0,
            int filter = 0,
            bool showInactive = false)
        {
            try
            {
                SynchronizationHelper.AccessToResources(
                    RMIndicatorValues.CANCEL,
                    RMIndicatorValues.URL_RM_OUTCOMES,
                    operationNumber,
                    IDBContext.Current.UserLoginName);

                new IDB.Presentation.MVC4.Controllers.CommonDocument().Log(
                    LogType.Debug,
                    "MVC4(OUTC_1S) - CALL WCF MODELS EVERNEXT",
                    "FindOneOperationModel -> BEFORE = OPERATION NUMBER:{0} ",
                    operationNumber);

                var operationModel = _clientResultsMatrix.FindOneOperationModel(
                    new OperationSpecification()
                    {
                        OperationNumber = operationNumber
                    });

                new IDB.Presentation.MVC4.Controllers.CommonDocument().Log(
                    LogType.Debug,
                    "MVC4(OUTC_1E) - CALL WCF MODELS EVERNEXT",
                    "FindOneOperationModel -> AFTER = RESPONSE: ",
                    operationModel);

                ResultsMatrixModel resultMatrixModel = null;

                if (operationModel != null)
                {
                    operationModel.AccessedByAdministrator =
                        IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);

                    new IDB.Presentation.MVC4.Controllers.CommonDocument().Log(
                        LogType.Debug,
                        "MVC4(OUTC_2S) - CALL WCF MODELS SERVICE",
                        "GetResultsMatrixModel -> BEFORE = OPERATION NUMBER:{0} ",
                        operationModel);

                    if (!showInactive)
                    {
                        resultMatrixModel = _cacheData.Get<ResultsMatrixModel>(_outcomesCacheName) ??
                            _cacheData.Add<ResultsMatrixModel>(_outcomesCacheName,
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

                    PutResultMatrixEopInCache(_resultMatrixEOPCacheName, resultMatrixModel);

                    new IDB.Presentation.MVC4.Controllers.CommonDocument().Log(
                        LogType.Debug,
                        "MVC4(OUTC_2E) - CALL WCF MODELS SERVICE",
                        "GetResultsMatrixModel -> AFTER = RESPONSE: ",
                        resultMatrixModel);

                    resultMatrixModel.showInactiveOutcomes = showInactive;
                }

                ViewData["filter"] = filter;

                if (state != 0)
                {
                    MessageConfiguration message = MessageHandler.setMessageRMOutcomes(state, false, 1);
                    ViewData["message"] = message;
                }

                ViewBag.isLessTaskFive = PCRHelpers.IsCurrentTaskLessThanFive(
                    ClientGenericRepository, PCRWorkflowStatusRepository);

                if (resultMatrixModel != null)
                {
                    resultMatrixModel.DEMLockReviewProcessData = _demLockModulesService
                        .BuildLockReviewProcessDEMDataModel(operationNumber);
                }

                return View(resultMatrixModel);
            }
            catch (Exception)
            {
                throw;
            }
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

            var accessedByAdministrator = 
                IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);

            Logger.GetLogger().WriteMessage(
                "Outcomes", "I've just read permissions and, accessAdmin = " + accessedByAdministrator);

            operationModel.AccessedByAdministrator = accessedByAdministrator;

            if (code != 0)
            {
                _cacheData.Remove(
                    _outcomesCacheName, System.Web.Caching.CacheItemRemovedReason.Removed);
            }

            var resultModel = _cacheData.Get<ResultsMatrixModel>(_outcomesCacheName) ??
                _cacheData.Add<ResultsMatrixModel>(_outcomesCacheName,
                _clientResultsMatrix.GetResultsMatrixModel(operationModel),
                _timeCachingVal);
            resultModel.AccessedByAdministrator = accessedByAdministrator;

            Logger.GetLogger().WriteMessage(
                "Outcomes", "After reading the cache, accessAdmin = " + resultModel.AccessedByAdministrator);

            ViewData["listBoxYearsPlan"] = GetYearPlans(resultModel.Operation.OperationId);
            ViewData["filter"] = filter;
            ViewData["newIndicator"] = newIndicator;

            ViewData["currentYearsPlan"] = resultModel.Outcomes.Count() > 0 ?
                resultModel.Outcomes.FirstOrDefault().GetMaxIndicatorYearPlanRows() : null;

            if (code != 0)
            {
                MessageConfiguration message = MessageHandler.setMessageRMOutcomes(code, false, 1);
                ViewData["message"] = message;
            }

            var modelIndicators = _linkPredefinedIndicatorService
                .GetLinkIndicatorModel(
                   LinkIndicatorTypeEnum.Outcomes,
                   false,
                   LinkIndicatorTypeEnum.Outcomes.ToString(),
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
                new OperationSpecification { OperationId = resultModel.OperationId });
            List<OutcomeModel> impacts = resultModel.Outcomes;
            resultModel.PmrCycle = pmrModel;
            resultModel.ValidationStage = _clientResultsMatrix.FindOneConvergenceMasterDataModel(
                new ConvergenceMasterDataSpecification
                {
                    ConvergenceMasterDataId = resultModel.ValidationStageId
                });

            try
            {
                _clientResultsMatrix.Save(resultModel, IDBContext.Current.UserName);
                code = 501;

                _cacheData.Remove(_outcomesCacheName, System.Web.Caching.CacheItemRemovedReason.Removed);
            }
            catch (Exception)
            {
                code = 499;
            }

            return RedirectToAction(
                "Index",
                "Outcomes",
                new
                {
                    operationNumber = resultModel.Operation.OperationNumber,
                    state = code,
                    filter = resultModel.filter
                });
        }

        [ExceptionHandling]
        [HttpGet]
        public virtual ActionResult Delete(
            int impactId, int intervalId, bool accessedByAdmin, bool isThirInterval)
        {
            string user = IDBContext.Current.UserLoginName;
            Logger.GetLogger().WriteMessage(
                "OutcomesController", string.Format("I am {0}, accessAdmin: {1}", user, accessedByAdmin));

            CustomDeleteOutcomeModel deleteImpactModel = new CustomDeleteOutcomeModel();

            int totalRows = 0;

            OutcomeModel currentImpact = _clientResultsMatrix.FindOneOutcomeModel(
                new OutcomeSpecification()
                {
                    OutcomeId = impactId
                });

            var existingImpacts = _clientResultsMatrix.FindAllOutcomeModelsBySpecification(
                totalRows,
                new OutcomeSpecification()
                {
                    ResultsMatrixId = currentImpact.ResultsMatrixId
                },
                int.MaxValue,
                0,
                null).ToList<OutcomeModel>();

            existingImpacts.Where(x => x.OutcomeId == currentImpact.OutcomeId).ForEach(x =>
             x.Statement = "Delete");

            existingImpacts.Sort((x, y) => x.Statement.CompareTo(y.Statement));

            deleteImpactModel.currentImpact = currentImpact;

            deleteImpactModel.existingImpacts = existingImpacts.ToList<OutcomeModel>();

            deleteImpactModel.intervalId = intervalId;

            deleteImpactModel.resultsMatrixId = currentImpact.ResultsMatrixId;

            if (intervalId == ResultsMatrixCodes.ThirdInterval || (accessedByAdmin && isThirInterval))
            {
                deleteImpactModel.AccessedByAdministrator = accessedByAdmin;
                deleteImpactModel.IsThirdInterval = isThirInterval;
                ViewData["message"] = Localization.GetText("TCM.RCMW.RegisterChangesMany.TextMessage");
            }
            else
            {
                deleteImpactModel.AccessedByAdministrator = accessedByAdmin;
                deleteImpactModel.IsThirdInterval = false;
            }

            deleteImpactModel.IsValidated = 
                ClientResultsMatrix.IsValidateOutcomeBYOutcomeIndicatorYearPlans(impactId);

            return PartialView(deleteImpactModel);
        }

        [ExceptionHandling]
        public virtual ActionResult DeleteOutcome(CustomDeleteOutcomeModel deleteModel)
        {
            var code = 0;
            var impact = _clientResultsMatrix.FindOneOutcomeModel(
                new OutcomeSpecification { OutcomeId = deleteModel.currentImpact.OutcomeId });

            var operation = _clientResultsMatrix.FindOneModel(
                new ResultsMatrixSpecification { ResultsMatrixId = impact.ResultsMatrixId });

            try
            {
                List<OutcomeIndicatorModel> outcomes = new List<OutcomeIndicatorModel>();

                foreach (var outcomeIndicator in deleteModel.currentImpact.OutcomeIndicators)
                {
                    var currentOutcome = this._clientResultsMatrix.FindOneOutcomeIndicatorModel(
                        new OutcomeIndicatorSpecification
                        {
                            OutcomeIndicatorId = outcomeIndicator.OutcomeIndicatorId
                        });

                    if (currentOutcome != null)
                    {
                        currentOutcome.ReassignToImpact = outcomeIndicator.ReassignToImpact;
                        outcomes.Add(currentOutcome);
                    }
                }

                deleteModel.currentImpact.OutcomeIndicators = outcomes;

                _clientResultsMatrix.DeleteImpact(
                    new ResultsMatrixModel
                    {
                        ResultsMatrixId = deleteModel.resultsMatrixId,
                        AccessedByAdministrator = deleteModel.AccessedByAdministrator,
                        IsThirdInterval = deleteModel.IsThirdInterval
                    },
                    deleteModel.currentImpact,
                    new IntervalModel { IntervalId = deleteModel.intervalId },
                    IDBContext.Current.UserName);

                _cacheData.Remove(_outcomesCacheName, System.Web.Caching.CacheItemRemovedReason.Removed);

                code = 505;
            }
            catch (Exception)
            {
                code = 495;
            }

            return RedirectToAction(
                "Index",
                "Outcomes",
                new
                {
                    operationNumber = operation.Operation.OperationNumber,
                    state = code,
                    filter = 0
                });
        }

        [HttpGet]
        public virtual ActionResult SaveChanges(int changesCount = 0)
        {
            ViewData["changesCount"] = changesCount;

            return PartialView();
        }

        public virtual FileResult DownloadReportOutcomes(
            string operationNumber,
            string formatType,
            string disaggregation)
        {
            var response = _reportsService.GetOutcomesReport(
                operationNumber,
                formatType,
                disaggregation);

            if (!response.IsValid)
                return null;

            return File(response.File, response.Application, response.FileName);
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

            if (resultMatrixEopInCache != null)
            {
                if (model.EndProjectYear != resultMatrixEopInCache.EopYear)
                {
                    _cacheDataResultMatrixEop.Add<ResultMatrixEOPModel>(
                        _resultMatrixEOPCacheName,
                         _clientResultsMatrix.GetResultMatrixEOP(model.EndProjectYear, resultMatrixEopInCache),
                        _timeCachingVal);
                }
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
            DateTime onPipelineDate = lcResolver.ResolvePipelineDate() ?? DateTime.Now;

            int startYearPlan = onPipelineDate.Year - 10;

            return Enumerable.Range(startYearPlan, 40).ToList();
        }
    }
}
