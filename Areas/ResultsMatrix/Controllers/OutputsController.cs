using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Caching;
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
using IDB.MW.Application.TCM.Services.TcmUniverseService;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.ResultMatrix.Outputs;
using IDB.MW.Domain.Contracts.ModelRepositories.Reports.Generic;
using IDB.MW.Domain.EntityHelpers;
using IDB.MW.Domain.Models.Architecture.Enumerations;
using IDB.MW.Domain.Models.Architecture.ResultMatrix.Common;
using IDB.MW.Domain.Models.Architecture.ResultMatrix.Outputs;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.DomainModel.Contracts.Repositories.PCRModule;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVC4.Models;
using IDB.Presentation.MVC4.Report;
using IDB.Presentation.MVCExtensions;
using IDB.Architecture;
using IDB.MW.Application.PEPModule.Services.Notification;
using IDB.MW.Domain.Models.ResultMatrix.DTOs;
using IDB.MW.Application.RiskMatrix.Contracts;
using IDB.MW.Application.RiskMatrix.Messages;

namespace IDB.Presentation.MVC4.Areas.ResultsMatrix.Controllers
{
    public partial class OutputsController : BaseController
    {
        private readonly ILinkPredefinedIndicatorService _linkPredefinedIndicatorService;
        private readonly IReportsService _reportsService;
        private readonly ITcmUniverseService _tcmUniverseService;
        private readonly IDEMLockModulesService _demLockModulesService;
        private readonly IRiskMatrixService _riskMastrixService;
        private int _timeCachingVal = int.Parse(ConfigurationManager.AppSettings["CacheLongTime"].ToString());
        public virtual Common.Logging.ILog Logger { get; set; }
        private string _outputsPhysicalCacheName = string.Empty;
        private string _outputsFinancialCacheName = string.Empty;
        private string _resultMatrixEOPCacheName;

        #region wcf services repositories

        private IResultsMatrixModelRepository _resultMatrixOutputs = null;
        private ICacheManagement _cacheData = null;
        private ICacheManagement _cacheDataResultMatrixEop = null;

        public OutputsController(
            ILinkPredefinedIndicatorService linkPredefinedIndicatorService,
            IReportsService reportsService,
            ITcmUniverseService tcmUniverseService,
            IResultsMatrixModelRepository resultMatrixOutputs,
            ICacheManagement cacheData,
            ICacheManagement cacheDataResultMatrixEop,
            IDEMLockModulesService demLockModulesService,
            IPepServiceNotification pepServiceNotification,
            IRiskMatrixService riskMastrixService)
        {
            _linkPredefinedIndicatorService = linkPredefinedIndicatorService;
            _reportsService = reportsService;
            _tcmUniverseService = tcmUniverseService;
            _resultMatrixOutputs = resultMatrixOutputs;
            _cacheData = cacheData;
            _outputsPhysicalCacheName = string.Format(
                CacheNames.OUTPUTS_PHYSICAL, IDBContext.Current.Operation);
            _outputsFinancialCacheName = string.Format(
                 CacheNames.OUTPUTS_FINANCIAL, IDBContext.Current.Operation);
            _cacheDataResultMatrixEop = cacheDataResultMatrixEop;
            _resultMatrixEOPCacheName = string.Format(
                CacheNames.OUTPUTS_EOP, IDBContext.Current.Operation);
            _demLockModulesService = demLockModulesService;
            _riskMastrixService = riskMastrixService;
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

        public static bool IsSpecialOperation(string operationNumber)
        {
            var operationType = OperationTypeHelper.GetOperationTypes(operationNumber);
            return operationType.Contains(OperationType.PDL) ||
                   operationType.Contains(OperationType.PBL) ||
                   operationType.Contains(OperationType.PBP);
        }

        #region PhysicalProgress

        [ExceptionHandling]
        public virtual ActionResult IndexPhysicalProgress(
            string operationNumber = "",
            bool showInactiveOutputs = false,
            bool showMilestones = false,
            bool showDisaggregation = false,
            MessageNotificationCodes messageStatus = MessageNotificationCodes.VoidMessage)
        {
            SynchronizationHelper.AccessToResources(
                RMIndicatorValues.CANCEL,
                RMIndicatorValues.URL_RM_PHYSICAL_PROGRESS,
                operationNumber,
                IDBContext.Current.UserLoginName);

            var accessedByAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);

            new IDB.Presentation.MVC4.Controllers.CommonDocument().Log(
                LogType.Debug,
                "MVC4(OUTP_1S) - CALL WCF MODELS SERVICE",
                string.Format("getResultsMatrixOutputs -> BEFORE = OPERATION NUMBER: {0}", operationNumber));

            ResultsMatrixModel model = null;

            if (!accessedByAdministrator && !showInactiveOutputs && !showMilestones && !showDisaggregation)
            {
                model = _cacheData.Get<ResultsMatrixModel>(_outputsPhysicalCacheName) ??
                        _cacheData.Add<ResultsMatrixModel>(_outputsPhysicalCacheName,
                            _resultMatrixOutputs.getResultsMatrixOutputs(
                                operationNumber,
                                accessedByAdministrator,
                                showInactiveOutputs,
                                showMilestones,
                                showDisaggregation),
                            _timeCachingVal);
                model.AccessedByAdministrator = accessedByAdministrator;
            }
            else
            {
                model = _resultMatrixOutputs.getResultsMatrixOutputs(
                    operationNumber,
                    accessedByAdministrator,
                    showInactiveOutputs,
                    showMilestones,
                    showDisaggregation);
            }

            PutResultMatrixEopInCache(_resultMatrixEOPCacheName, model);

            new IDB.Presentation.MVC4.Controllers.CommonDocument().Log(
                LogType.Debug,
                "MVC4(OUTP_1E) - CALL WCF MODELS SERVICE",
                "getResultsMatrixOutputs -> AFTER = RESPONSE: ",
                model);

            model.Components.Sort((x1, x2) => x1.OrderNumber.CompareTo(x2.OrderNumber));

            model.showInactiveOutputs = showInactiveOutputs;
            model.showMilestones = showMilestones;
            model.showDisaggregation = showDisaggregation;
            model.AccessedByAdministrator = accessedByAdministrator;

            if (messageStatus != MessageNotificationCodes.VoidMessage)
            {
                MessageConfiguration message = MessageHandler.setMessage(messageStatus, true, 5);
                ViewData["message"] = message;
            }

            model.redirectToEdit = false;
            ViewBag.isLessTaskFive = PCRHelpers.IsCurrentTaskLessThanFive(
                ClientGenericRepository,
                PCRWorkflowStatusRepository);

            model.DEMLockReviewProcessData = _demLockModulesService.BuildLockReviewProcessDEMDataModel(
                operationNumber);

            return View(model);
        }

        [ExceptionHandling]
        [HasPermission(Permissions = Permission.RESULTS_MATRIX_WRITE)]
        public virtual ActionResult EditPhysicalProgress(
            string operationNumber = "",
            bool isAddNewOutputActivate = false,
            bool editMilestones = false,
            bool editDisaggregation = false,
            bool importDataFromPep = false,
            MessageNotificationCodes messageStatus = MessageNotificationCodes.VoidMessage)
        {
            bool accessedByAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);

            var model = _resultMatrixOutputs.getResultsMatrixOutputs(
                operationNumber,
                accessedByAdministrator,
                false,
                editMilestones,
                !editMilestones,
                false,
                importDataFromPep);

            model.Components.Sort((x1, x2) => x1.OrderNumber.CompareTo(x2.OrderNumber));
            model.showMilestones = editMilestones;
            model.showDisaggregation = !editMilestones;
            model.AccessedByAdministrator = accessedByAdministrator;

            ViewBag.ShowImportPEPBtn = model.IsIntegratedWithPep &&
                                       IsOperationTypeValid(model.Operation.OperationType,
                                           new[] { OperationType.GUA, OperationType.IGR, OperationType.LON }) &&
                                       model.Operation.IsStartUpPlanValidated &&
                                       IDBContext.Current.HasPermission(Permission.TC_RESULT_MATRIX_WRITE_PERMISSIONS)
                ? true
                : false;

            ViewBag.OperationNumber = model.OperationNumber;

            if (messageStatus != MessageNotificationCodes.VoidMessage)
            {
                MessageConfiguration message = MessageHandler.setMessage(messageStatus, true, 5);
                ViewData["message"] = message;
            }

            ViewBag.isAddNewOutputActivate = isAddNewOutputActivate;

            var modelIndicators = _linkPredefinedIndicatorService
                .GetLinkIndicatorModel(
                    LinkIndicatorTypeEnum.Outputs,
                    false,
                    LinkIndicatorTypeEnum.Outputs.ToString(),
                    true);
            ViewBag.ModelIndicators = modelIndicators.Filter;

            model.DEMLockReviewProcessData = _demLockModulesService.BuildLockReviewProcessDEMDataModel(
                operationNumber);

            return View(model);
        }

        [ExceptionHandling]
        [HttpPost]
        [HasPermission(Permissions = Permission.RESULTS_MATRIX_WRITE)]
        public virtual ActionResult UpdatePhysicalProgress(ResultsMatrixModel model)
        {
            var messageStatus = MessageNotificationCodes.SaveDataSucessfully;

            try
            {
                model.AccessedByAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);

                Dictionary<string, int> createdPepTaskIds = null;
                bool teamLiderNotification = false;
                _resultMatrixOutputs.updatePhysicalProgress(model, IDBContext.Current.UserName, ref createdPepTaskIds, ref teamLiderNotification);

                if (teamLiderNotification)
                {
                    var pepServiceNotificationResponse =
                        Globals.Resolve<IPepServiceNotification>()
                            .SendNotificationModifiedTasksOnlyTeamLider(model.OperationNumber);

                    if (!pepServiceNotificationResponse.IsValid)
                    {
                        throw new Exception();
                    }
                }

                _cacheData.Remove(_outputsPhysicalCacheName, CacheItemRemovedReason.Removed);
            }
            catch (Exception)
            {
                messageStatus = MessageNotificationCodes.SaveDataFail;
            }

            return RedirectToAction(
                "IndexPhysicalProgress",
                new
                {
                    operationNumber = model.OperationNumber,
                    messageStatus = messageStatus,
                    showDisaggregation = model.showDisaggregation,
                    showMilestones = !model.showDisaggregation
                });
        }

        #endregion PhysicalProgress

        #region FinancialProgress

        [ExceptionHandling]
        public virtual ActionResult IndexFinancialProgress(string operationNumber = "",
            bool showInactiveOutputs = false,
            bool showMilestones = false,
            MessageNotificationCodes messageStatus = MessageNotificationCodes.VoidMessage)
        {
            var accessedByAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);

            SynchronizationHelper.AccessToResources(
                RMIndicatorValues.CANCEL,
                RMIndicatorValues.URL_RM_PHYSICAL_PROGRESS,
                operationNumber,
                IDBContext.Current.UserLoginName);

            ResultsMatrixModel model;

            if (!accessedByAdministrator && !showMilestones)
            {
                model = _cacheData.Get<ResultsMatrixModel>(_outputsFinancialCacheName) ?? 
                    _cacheData.Add<ResultsMatrixModel>(_outputsFinancialCacheName, 
                    _resultMatrixOutputs.getResultsMatrixOutputs(operationNumber, 
                    accessedByAdministrator, 
                    true, 
                    showMilestones, 
                    false, 
                    true), 
                    _timeCachingVal);
                model.AccessedByAdministrator = accessedByAdministrator;
            }
            else
            {
                model = _resultMatrixOutputs.getResultsMatrixOutputs(operationNumber, accessedByAdministrator, true, showMilestones, false, true);
            }

            model.AccessedByAdministrator = accessedByAdministrator;

            PutResultMatrixEopInCache(_resultMatrixEOPCacheName, model);

            if (IsSpecialOperation(model.OperationNumber))
            {
                return RedirectToAction("IndexPhysicalProgress",
                    new { operationNumber = operationNumber });
            }

            model.showInactiveOutputs = showInactiveOutputs;
            model.showMilestones = showMilestones;
            model.showDisaggregation = false;

            if (messageStatus != MessageNotificationCodes.VoidMessage)
            {
                // Set message in agreement with the code
                MessageConfiguration message = MessageHandler.setMessage(messageStatus, true, 5);

                // Pass message to the view
                ViewData["message"] = message;
            }

            ViewBag.isLessTaskFive = PCRHelpers.IsCurrentTaskLessThanFive(
                ClientGenericRepository, PCRWorkflowStatusRepository);

            model.DEMLockReviewProcessData = _demLockModulesService.BuildLockReviewProcessDEMDataModel(
                operationNumber);

            return View(model);
        }

        [ExceptionHandling]
        [HasPermission(Permissions = "Results Matrix Write")]
        public virtual ActionResult EditFinancialProgress(
            string operationNumber = "",
            bool editMilestones = false,
            bool importDataFromPep = false,
            MessageNotificationCodes messageStatus = MessageNotificationCodes.VoidMessage)
        {
            var accessedByAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);

            // For dissagregations FinancialProgress is always false
            var model = _resultMatrixOutputs.getResultsMatrixOutputs(operationNumber, 
                accessedByAdministrator, 
                false,
                editMilestones, 
                false, 
                true, 
                importDataFromPep);
            model.AccessedByAdministrator = accessedByAdministrator;

            var modelAll = _cacheData.Get<ResultsMatrixModel>(_outputsFinancialCacheName) ??
                           _cacheData.Add<ResultsMatrixModel>(_outputsFinancialCacheName,
                           _resultMatrixOutputs.getResultsMatrixOutputs(operationNumber, 
                               accessedByAdministrator, 
                               true, 
                               false, 
                               false, 
                               true, 
                               importDataFromPep),
                           _timeCachingVal);
            modelAll.AccessedByAdministrator = accessedByAdministrator;

            PutResultMatrixEopInCache(_resultMatrixEOPCacheName, model);

            decimal? sumEopPaOutputs =
                modelAll.Components.SelectMany(x => x.Outputs).SelectMany(y => y.OutputYearPlans)
                    .Where(z => z.IsCost == true && z.Year == -1).Select(i => i.AnnualPlan).Sum(j => j);
            decimal? sumEopPaOtherCost = modelAll.OtherCosts.SelectMany(x => x.OtherCostYearPlans)
                .Where(z => z.Year == -1).Select(i => i.AnnualPlan).Sum(j => j);
            decimal? allCostFull = sumEopPaOutputs + sumEopPaOtherCost;

            ViewBag.allCostFull = allCostFull;

            // If an operation  subtype is PerformanceDrivenLoan,PolicyBasedLoan or ProgramaticPolicyBaseLoan, it shouldn't show financial tab
            if (IsSpecialOperation(model.OperationNumber))
            {
                return RedirectToAction("IndexPhysicalProgress",
                    new { operationNumber = operationNumber });
            }

            model.showMilestones = editMilestones;

            ViewBag.OperationNumber = model.OperationNumber;

            ViewBag.ShowImportPEPBtn = model.IsIntegratedWithPep &&
                                       IsOperationTypeValid(model.Operation.OperationType,
                                           new[] { OperationType.GUA, OperationType.IGR, OperationType.LON }) &&
                                       model.Operation.IsStartUpPlanValidated &&
                                       IDBContext.Current.HasPermission(Permission.TC_RESULT_MATRIX_WRITE_PERMISSIONS)
                ? true
                : false;

            if (messageStatus != MessageNotificationCodes.VoidMessage)
            {
                // Set message in agreement with the code
                MessageConfiguration message = MessageHandler.setMessage(messageStatus, true, 5);

                // Pass message to the view
                ViewData["message"] = message;
            }

            model.DEMLockReviewProcessData = _demLockModulesService.BuildLockReviewProcessDEMDataModel(
               operationNumber);

            return View(model);
        }

        [ExceptionHandling]
        [HttpPost]
        [HasPermission(Permissions = "Results Matrix Write")]
        public virtual ActionResult UpdateFinancialProgress(ResultsMatrixModel model)
        {
            var messageStatus = MessageNotificationCodes.SaveDataSucessfully;

            try
            {
                Dictionary<string, int> createdPepTaskIds = null;
                bool teamLiderNotification = false;
                model.AccessedByAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);
                _resultMatrixOutputs.updateFinancialProgress(model, IDBContext.Current.UserName, ref createdPepTaskIds, ref teamLiderNotification);

                if (teamLiderNotification)
                {
                    var pepServiceNotificationResponse =
                        Globals.Resolve<IPepServiceNotification>()
                            .SendNotificationModifiedTasksOnlyTeamLider(model.OperationNumber);
                    if (!pepServiceNotificationResponse.IsValid)
                    {
                        throw new Exception();
                    }
                }

                _cacheData.Remove(_outputsFinancialCacheName, CacheItemRemovedReason.Removed);
            }
            catch (Exception)
            {
                messageStatus = MessageNotificationCodes.SaveDataFail;
            }

            return RedirectToAction(
                "IndexFinancialProgress",
                new
                {
                    operationNumber = model.OperationNumber,
                    messageStatus = messageStatus
                });
        }

        #endregion

        #region VisualizationProgress

        // GET: /ResultsMatrix/Outputs/IndexVisualizationProgress/
        [ExceptionHandling]
        public virtual ActionResult IndexVisualizationProgress(string operationNumber = "", 
            bool showInactiveOutputs = false, 
            bool showMilestones = false, 
            bool showVisualizations = false)
        {
            // Check if the administrator is accessing
            var accessedByAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);
            ResultsMatrixModel model = null;
            if (!accessedByAdministrator && !showInactiveOutputs && !showMilestones && !showVisualizations)
            {
                model = _cacheData.Get<ResultsMatrixModel>(_outputsPhysicalCacheName) ?? 
                    _cacheData.Add<ResultsMatrixModel>(_outputsPhysicalCacheName, 
                    _resultMatrixOutputs.getResultsMatrixOutputs(operationNumber, 
                    accessedByAdministrator, 
                    showInactiveOutputs, 
                    showMilestones, 
                    showVisualizations),
                    _timeCachingVal);
                model.AccessedByAdministrator = accessedByAdministrator;
            }
            else
            {
                model = _resultMatrixOutputs.getResultsMatrixOutputs(operationNumber, 
                    accessedByAdministrator,
                    showInactiveOutputs, 
                    showMilestones, 
                    showVisualizations);
            }

            PutResultMatrixEopInCache(_resultMatrixEOPCacheName, model);

            model.showInactiveOutputs = showInactiveOutputs;
            model.showMilestones = showMilestones;
            model.showVisualizations = showVisualizations;

            model.DEMLockReviewProcessData = _demLockModulesService.BuildLockReviewProcessDEMDataModel(
                operationNumber);

            return View(model);
        }

        #endregion

        #region Component

        [ExceptionHandling]
        public virtual ActionResult IndexDeleteComponent(int componentId, string operationNumber)
        {
            ResultsMatrixModel resultsModel = new ResultsMatrixModel()
            {
                Operation =
                    new IDB.MW.Domain.Models.Architecture.ResultMatrix.Outputs.OperationModel()
                    {
                        OperationNumber = operationNumber
                    }
            };

            var accessedByAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);
            var model = _resultMatrixOutputs.getResultsMatrixOutputs(operationNumber, 
                accessedByAdministrator, 
                true,
                false, 
                false);

            var customModel = new CustomDeleteComponentModel()
            {
                Components = model.Components,
                currentComponent = model.Components.Where(x => x.ComponentId == componentId).FirstOrDefault()
            };

            customModel.Components.Where(x => x.ComponentId == componentId).ForEach(x =>
                x.Statement = "Delete");
            customModel.currentComponent.OperationOperationNumber = model.Operation.OperationNumber;
            customModel.Components.Sort((x, y) => x.Statement.CompareTo(y.Statement));

            customModel.SelectedOptions = new List<DeleteComponentModelItem>();

            return PartialView(customModel);
        }

        [ExceptionHandling]
        public virtual ActionResult DeleteComponent(CustomDeleteComponentModel model)
        {
            var messageStatus = MessageNotificationCodes.SaveDataSucessfully;

            try
            {
                model.currentComponent.AccessedByAdministrator =
                    IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);
                bool teamLiderNotification = false;
                var reassignedPeptasks = new List<Tuple<int, int, int>>();

                if (model.SelectedOptions != null && model.SelectedOptions.Any())
                {
                    model.SelectedOptions.ForEach(x =>
                        reassignedPeptasks.Add(new Tuple<int, int, int>(x.ComponentId,
                            x.SubComponentId,
                            x.OutputId)));
                }

                _resultMatrixOutputs.deleteComponent(model.currentComponent, IDBContext.Current.UserName, ref teamLiderNotification, reassignedPeptasks);

                if (teamLiderNotification)
                {
                    var pepServiceNotificationResponse =
                        Globals.Resolve<IPepServiceNotification>()
                            .SendNotificationModifiedTasksOnlyTeamLider(model.currentComponent.OperationOperationNumber);

                    if (!pepServiceNotificationResponse.IsValid)
                    {
                        throw new Exception();
                    }
                }

                // Remove the Cache
                _cacheData.Remove(_outputsPhysicalCacheName, System.Web.Caching.CacheItemRemovedReason.Removed);
            }
            catch (TimeoutException)
            {
                messageStatus = MessageNotificationCodes.SaveDataFail;
            }
            catch (Exception)
            {
                messageStatus = MessageNotificationCodes.DeleteDataComponentFail;
            }

            return RedirectToAction(
                "EditPhysicalProgress",
                new
                {
                    operationNumber = model.currentComponent.OperationOperationNumber,
                    messageStatus = messageStatus
                });
        }

        #endregion

        #region Outputs

        [ExceptionHandling]
        [HasPermission(Permissions = "Results Matrix Write")]
        public virtual ActionResult UpdateOuputComponentParent(CustomReassingOutputModel model)
        {
            var statusCode = MessageNotificationCodes.ReassignOutputSucessfully;

            try
            {
                _resultMatrixOutputs.updateOutputComponent(
                    model.output.OutputId, model.SelectedComponentId, model.SelectedSubComponentId, IDBContext.Current.UserName);
                _cacheData.Remove(_outputsPhysicalCacheName, CacheItemRemovedReason.Removed);
            }
            catch (Exception)
            {
                statusCode = MessageNotificationCodes.ReassignOutputFail;
            }

            return RedirectToAction(
                "EditPhysicalProgress",
                new
                {
                    operationNumber = model.output.OperationOperationNumber,
                    messageStatus = statusCode
                });
        }

        [ExceptionHandling]
        [HasPermission(Permissions = "Results Matrix Write")]
        public virtual ActionResult EditReassingOutput(int outputID)
        {
            var outputModel = _resultMatrixOutputs.getOutputDetail(outputID);
            var componentList = _resultMatrixOutputs.getComponentByReassingOutput(outputID);

            var model = new CustomReassingOutputModel()
            {
                output = outputModel,
                componentList = componentList,
                SelectedComponentId = 0
            };

            model.componentList.Insert(0, new ComponentModel()
            {
                Statement = "Select Component",
                ComponentId = -1
            });

            return PartialView(model);
        }

        [ExceptionHandling]
        public virtual ActionResult IndexOutputDetail(int outputID,
            MessageNotificationCodes messageStatus = MessageNotificationCodes.VoidMessage)
        {
            var model = _resultMatrixOutputs.getOutputDetail(outputID);
            if (messageStatus != MessageNotificationCodes.VoidMessage)
            {
                // Set message in agreement with the code
                MessageConfiguration message = MessageHandler.setMessage(messageStatus, true, 5);

                // Pass message to the view
                ViewData["message"] = message;
            }

            return View(model);
        }

        [ExceptionHandling]
        [HasPermission(Permissions = "Results Matrix Write")]
        public virtual ActionResult EditOutputDetail(int outputID)
        {
            var model = _resultMatrixOutputs.getOutputDetail(outputID);
            int totalRows = 0;
            var modelCategories = _resultMatrixOutputs.getCategories(out totalRows, int.MaxValue, 0);
            ViewBag.Categories = modelCategories;
            model.AccessedByAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);
            return View(model);
        }

        [ExceptionHandling]
        [HasPermission(Permissions = "Results Matrix Write")]
        public virtual ActionResult UpdateOutputDetails(OutputModel model)
        {
            var messageStatus = MessageNotificationCodes.SaveDataSucessfully;
            try
            {
                model.AccessedByAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);
                var modelResult = _resultMatrixOutputs.updateOutputDetail(model, IDBContext.Current.UserName);

                if (model.IsDeactivated)
                {
                    DesactivateOutputRequest request = new DesactivateOutputRequest
                    {
                        OutputId = model.OutputId
                    };

                    _riskMastrixService.LogicDesactivateOutputResultMatrix(request);
                }

                // Remove the Cache
                _cacheData.Remove(_outputsPhysicalCacheName, System.Web.Caching.CacheItemRemovedReason.Removed);
                _cacheData.Remove(_outputsFinancialCacheName, System.Web.Caching.CacheItemRemovedReason.Removed);
            }
            catch (Exception)
            {
                messageStatus = MessageNotificationCodes.SaveDataFail;
            }

            return RedirectToAction("IndexOutputDetail", new { outputID = model.OutputId, messageStatus = messageStatus });
        }

        [ExceptionHandling]
        public virtual ActionResult IndexDeleteOutputYearColumnWarning(int outputYearPlanId, string OperationNumber)
        {
            var model = new OutputYearPlanModel()
            {
                OutputYearPlanId = outputYearPlanId,
                OperationNumber = OperationNumber
            };
            return PartialView(model);
        }

        [ExceptionHandling]
        [HttpPost]
        public virtual ActionResult DeleteOutputColumnYear(OutputYearPlanModel model)
        {
            model.AccessedByAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);
            var messageStatus = MessageNotificationCodes.DeleteDataYearPlanSucessfully;
            try
            {
                bool canBeDeleted = false;
                bool teamLiderNotification = false;
                _resultMatrixOutputs.deleteOutputColumnYear(model, out canBeDeleted, ref teamLiderNotification);

                if (teamLiderNotification)
                {
                    var pepServiceNotificationResponse =
                        Globals.Resolve<IPepServiceNotification>()
                            .SendNotificationModifiedTasksOnlyTeamLider(model.OperationNumber);

                    if (!pepServiceNotificationResponse.IsValid)
                    {
                        throw new Exception();
                    }
                }

                // Remove the Cache
                _cacheData.Remove(_outputsPhysicalCacheName, System.Web.Caching.CacheItemRemovedReason.Removed);

                if (!canBeDeleted)
                {
                    messageStatus = MessageNotificationCodes.DeleteDataOutputFail1;
                }
            }
            catch (Exception)
            {
                messageStatus = MessageNotificationCodes.DeleteDataYearPlanFail;
            }

            return RedirectToAction(
                "EditPhysicalProgress",
                new
                {
                    operationNumber = model.OperationNumber,
                    messageStatus = messageStatus
                });
        }

        [ExceptionHandling]
        public virtual ActionResult IndexDeleteOutput(int outputId, string OperationNumber)
        {
            var model = _resultMatrixOutputs.getReasignMilestones(outputId);

            model.relatedOutputs.Where(x => x.OutputId == outputId).ForEach(x =>
                x.Definition = "Delete");
            model.relatedOutputs.Sort((x, y) => x.Definition.CompareTo(y.Definition));

            var accessedByAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);

            model.AccessedByAdministrator = accessedByAdministrator;

            return PartialView(model);
        }

        [ExceptionHandling]
        [HttpPost]
        public virtual ActionResult DeleteOutput(OutputModel model)
        {
            var messageStatus = MessageNotificationCodes.SaveDataSucessfully;

            try
            {
                model.AccessedByAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);

                bool canBeDeleted = false;
                bool teamLiderNotification = false;
                _resultMatrixOutputs.deleteOutput(model, IDBContext.Current.UserName, out canBeDeleted, ref teamLiderNotification);

                if (teamLiderNotification)
                {
                    var pepServiceNotificationResponse =
                        Globals.Resolve<IPepServiceNotification>()
                            .SendNotificationModifiedTasksOnlyTeamLider(model.OperationOperationNumber);

                    if (!pepServiceNotificationResponse.IsValid)
                    {
                        throw new Exception();
                    }
                }

                // Remove the Cache
                _cacheData.Remove(_outputsPhysicalCacheName, System.Web.Caching.CacheItemRemovedReason.Removed);

                if (!canBeDeleted)
                    messageStatus = MessageNotificationCodes.DeleteDataOutputFail1;
            }
            catch (Exception)
            {
                messageStatus = MessageNotificationCodes.DeleteDataYearPlanFail;
            }

            return RedirectToAction(
                "EditPhysicalProgress",
                new
                {
                    operationNumber = model.OperationOperationNumber,
                    messageStatus = messageStatus,
                    editMilestones = true
                });
        }

        #endregion

        #region Milestones

        [ExceptionHandling]
        public virtual ActionResult IndexMilestoneDetails(int milestoneId)
        {
            var model = _resultMatrixOutputs.getMilestoneDetail(milestoneId);
            return View(model);
        }

        [ExceptionHandling]
        [HasPermission(Permissions = "Results Matrix Write")]
        public virtual ActionResult EditMilestoneDetails(int milestoneId)
        {
            var model = _resultMatrixOutputs.getMilestoneDetail(milestoneId);
            model.AccessedByAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);
            return View(model);
        }

        [ExceptionHandling]
        [HasPermission(Permissions = "Results Matrix Write")]
        public virtual ActionResult UpdateMilestoneDetails(MilestoneModel model)
        {
            _resultMatrixOutputs.updateMilestoneDetail(model, IDBContext.Current.UserName);
            _cacheData.Remove(_outputsPhysicalCacheName, CacheItemRemovedReason.Removed);

            return RedirectToAction("IndexMilestoneDetails", new { milestoneId = model.MilestoneId });
        }

        [ExceptionHandling]
        public virtual ActionResult IndexDeleteMilestoneWarning(int MilestoneId, string OperationNumber)
        {
            var model = new MilestoneModel()
            {
                MilestoneId = MilestoneId,
                OperationNumber = OperationNumber
            };
            return PartialView(model);
        }

        [ExceptionHandling]
        [HttpPost]
        public virtual ActionResult DeleteMilestone(MilestoneModel model)
        {
            var messageStatus = MessageNotificationCodes.SaveDataSucessfully;
            model.AccessedByAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);
            try
            {
                bool canBeDeleted = false;

                _resultMatrixOutputs.deleteMilestone(model, out canBeDeleted, IDBContext.Current.UserName);
                _cacheData.Remove(_outputsPhysicalCacheName, CacheItemRemovedReason.Removed);

                if (!canBeDeleted)
                    messageStatus = MessageNotificationCodes.DeleteDataMilestoneFail;
            }
            catch (Exception)
            {
                messageStatus = MessageNotificationCodes.DeleteDataYearPlanFail;
            }

            return RedirectToAction(
                "EditPhysicalProgress",
                new
                {
                    operationNumber = model.OperationNumber,
                    editMilestones = true,
                    messageStatus = messageStatus
                });
        }

        #endregion

        #region Disaggregation

        [ExceptionHandling]
        [HttpPost]
        public virtual ActionResult DeleteDisaggregation(OutputDisaggregationModel model)
        {
            _resultMatrixOutputs.deleteDisaggregation(model);

            // Remove the Cache
            _cacheData.Remove(_outputsPhysicalCacheName, System.Web.Caching.CacheItemRemovedReason.Removed);

            if (model.IsJsonResult == true)
            {
                return Json(new
                {
                    RedirectUrl = Url.Action(
                        "EditPhysicalProgress",
                        "Outputs",
                        new
                        {
                            area = "ResultsMatrix",
                            operationNumber = model.OperationOperationNumber,
                            editDisaggregation = true
                        }),
                    IsValid = true
                });
            }

            return RedirectToAction("EditPhysicalProgress", new { operationNumber = model.OperationOperationNumber, editDisaggregation = true });
        }

        [ExceptionHandling]
        public virtual ActionResult IndexDeleteDisaggregationWarning(int OutputDisaggregationId, string OperationNumber)
        {
            var model = new OutputDisaggregationModel()
            {
                OutputDisaggregationId = OutputDisaggregationId,
                OperationOperationNumber = OperationNumber
            };
            return PartialView(model);
        }

        #endregion

        #region OtherCost

        [ExceptionHandling]
        public virtual ActionResult IndexDeleteOtherCost(int otherCostId, string operationNumber)
        {
            OtherCostModel model = new OtherCostModel()
            {
                OperationNumber = operationNumber,
                OtherCostId = otherCostId
            };

            return PartialView(model);
        }

        [ExceptionHandling]
        [HttpPost]
        public virtual ActionResult DeleteOtherCost(OtherCostModel model)
        {
            var messageStatus = MessageNotificationCodes.SaveDataSucessfully;
            try
            {
                model.AccessedByAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);
                bool teamLiderNotification = false;
                _resultMatrixOutputs.deleteOtherCost(model, ref teamLiderNotification);

                if (teamLiderNotification)
                {
                    var pepServiceNotificationResponse =
                        Globals.Resolve<IPepServiceNotification>()
                            .SendNotificationModifiedTasksOnlyTeamLider(model.OperationNumber);

                    if (!pepServiceNotificationResponse.IsValid)
                    {
                        throw new Exception();
                    }
                }

                // Remove the Cache
                _cacheData.Remove(_outputsFinancialCacheName, System.Web.Caching.CacheItemRemovedReason.Removed);
            }
            catch (Exception)
            {
                messageStatus = MessageNotificationCodes.SaveDataFail;
            }

            return RedirectToAction("EditFinancialProgress",
                new { operationNumber = model.OperationNumber, messageStatus = messageStatus });
        }

        #endregion

        [ExceptionHandling]
        public virtual ActionResult IndexChangeMatrixWarningMessage(string formName)
        {
            ViewBag.FormName = formName;
            return PartialView();
        }

        [ExceptionHandling]
        public virtual ActionResult IndexChangeMatrixWarningMessageVisualOutput(string formName)
        {
            ViewBag.FormName = formName;
            return PartialView();
        }

        [ExceptionHandling]
        public virtual ActionResult IndexChangeEditPhysicalModeWarningMessage(bool showMilestone)
        {
            ViewBag.showMilestone = showMilestone;
            return PartialView();
        }

        public virtual FileResult DownloadReportFinancialProgress(
            string operationNumber,
            string formatType,
            string inactiveOutput)
        {
            if (IsSpecialOperation(operationNumber))
                return null;

            var parent = _tcmUniverseService.GetParentOperation(operationNumber);

            if (!parent.IsValid)
                throw new Exception(parent.ErrorMessage);

            var response = _reportsService.GetFinancialProgressReport(
                parent.OperationNumber,
                formatType,
                inactiveOutput);

            if (!response.IsValid)
                return null;

            return File(response.File, response.Application, response.FileName);
        }

        public virtual FileResult DownloadReportPhysicalProgress(
            string operationNumber,
            string formatType,
            string inactiveOutput,
            string milestones,
            string disaggregation)
        {
            var parent = _tcmUniverseService.GetParentOperation(operationNumber);

            if (!parent.IsValid)
                throw new Exception(parent.ErrorMessage);

            var response = _reportsService.GetPhysicalProgressReport(
                parent.OperationNumber,
                formatType,
                inactiveOutput,
                milestones,
                disaggregation);

            if (!response.IsValid)
                return null;

            return File(response.File, response.Application, response.FileName);
        }

        public virtual ActionResult DownloadReportVisualizationProgress(string OperationNumber, 
            string format = "",
            string Statement = "", 
            bool showInactiveOutputs = false,
            bool showMilestones = false,
            bool showDisaggregations = false, 
            bool showVisualizations = false)
        {
            if ((format == "pdf") || (format == "xls"))
            {
                try
                {
                    var accessedByAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);

                    var model = _resultMatrixOutputs.getResultsMatrixOutputs(OperationNumber, 
                        accessedByAdministrator,
                        showInactiveOutputs, 
                        showMilestones, 
                        showVisualizations);
                    model.showInactiveOutputs = showInactiveOutputs;
                    model.showMilestones = showMilestones;
                    model.showVisualizations = showVisualizations;

                    ReportBuilder RB = new ReportBuilder();
                    string Result = RB.Build_Outputs_VisualizationProgress(model, format, Statement);

                    string name = IDBContext.Current.Operation + "_" + Localization.GetText("Outputs") +
                                  Localization.GetText("VisualizationProgress");

                    string filename = name + "." + format;
                    Response.AppendHeader("Content-Disposition", "inline;filename=" + filename);

                    if (format == "pdf")
                    {
                        Response.ContentType = "application/vnd.pdf";
                    }
                    else
                    {
                        Response.ContentType = "application/vnd.ms-excel";
                        Response.Write(Result);
                    }

                    Response.End();
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return View();
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
                        _resultMatrixOutputs.GetResultMatrixEOP(model.EndProjectYear, resultMatrixEopInCache),
                        _timeCachingVal);
                }
            }
            else
            {
                _cacheDataResultMatrixEop.Add<ResultMatrixEOPModel>(
                    _resultMatrixEOPCacheName,
                    _resultMatrixOutputs.GetNewResultMatrixEOP(model.EndProjectYear, model.ResultsMatrixId),
                    _timeCachingVal);
            }
        }

        public virtual JsonResult GetSubComponentsByComponentId(int componentId)
        {
            var subComponents = _resultMatrixOutputs.GetSubComponentsByComponentId(componentId);
            subComponents.Insert(0, new PepTaskListItemDto() { PepTaskId = -1, PepTaskName = "Select SubComponent" });
            return Json(subComponents, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult ExistsOutputsLinkedToSubComponents(int componentId)
        {
            var response = _resultMatrixOutputs.ExistsOutputsLinkedToSubComponents(componentId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult HasPepUnconfirmedData(int operationId)
        {
            var response = _resultMatrixOutputs.HasPepUnconfirmedData(operationId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        private bool IsOperationTypeValid(string operationType, string[] rangeOfTypes)
        {
            return rangeOfTypes.Contains(operationType);
        }

        private bool SynchronizeModalWindowsWithPep(string operationType, bool isStartUpPlanValidated, bool isIntegratedWithPep)
        {
            var isValidOperation = IsOperationTypeValid(operationType, 
                new[] 
                {
                    OperationType.GUA,
                    OperationType.IGR,
                    OperationType.LON
                });

            var isValid = (isValidOperation && !isStartUpPlanValidated) ||
                (isValidOperation && isStartUpPlanValidated && isIntegratedWithPep);

            return isValid;
        }  
    }
}
