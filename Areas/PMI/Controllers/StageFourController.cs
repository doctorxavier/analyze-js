using System;
using System.Configuration;
using System.Web.Mvc;
using System.Web.Caching;

using IDB.Architecture.Diagnostics;
using IDB.Architecture.Logging;
using IDB.Architecture.Extensions;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.Clauses;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.SupervisionPlans.PMI;
using IDB.MW.Domain.Contracts.ModelRepositories.Global;
using IDB.MW.Domain.Contracts.ModelRepositories.K2Service;
using IDB.MW.Domain.Contracts.ModelRepositories.OperationProfile.BasicData;
using IDB.MW.Domain.Contracts.ModelRepositories.Supervision.PMI;
using IDB.MW.Domain.Contracts.ModelRepositories.VerifyContent;
using IDB.MW.Domain.Models.Architecture.Enumerations;
using IDB.MW.Domain.Models.K2Service;
using IDB.MW.Domain.Models.OperationProfile.BasicData;
using IDB.MW.Domain.Models.Supervision.PMI;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Controllers.Helpers.Enumerators;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVCExtensions;
using IDB.Architecture.Cache;
using IDB.Presentation.MVC4.Areas.PMI.Helpers;

namespace IDB.Presentation.MVC4.Areas.PMI.Controllers
{
    public partial class StageFourController : BaseController
    {
        private int _timeCachingVal = int.Parse(ConfigurationManager.AppSettings["CacheMediumTime"]);
        private ICacheManagement _cacheData = null;

        #region wcf services repositories

        private IBasicDataModelRepository _basicDataModelRepository = null;
        private IK2ServiceProxy _k2ServiceProxy = null;
        private IOperationModelRepository _operationClauseClient = null;
        private IResultsMatrixModelRepository _resultsMatrixRepository = null;
        private IRuleEvaluatorService _ruleService = null;
        private IGlobalModelRepository _globalRepository = null;
        private IPMIDetailsModelRepository _clientPMIDetails = null;
        private IVerifyContentModelRepositoryService _clientVerifyContent = null;

        #endregion

        public StageFourController(IBasicDataModelRepository basicDataModelRepository,
            IK2ServiceProxy k2ServiceProxy,
            IOperationModelRepository operationClauseClient,
            IGlobalModelRepository globalRepository,
            IResultsMatrixModelRepository resultsMatrixRepository,
            IVerifyContentModelRepositoryService clientVerifyContent,
            IPMIDetailsModelRepository clientPMIDetails,
            IRuleEvaluatorService ruleService,
            ICacheManagement cacheData)
        {
            _basicDataModelRepository = basicDataModelRepository;
            _k2ServiceProxy = k2ServiceProxy;
            _operationClauseClient = operationClauseClient;
            _globalRepository = globalRepository;
            _resultsMatrixRepository = resultsMatrixRepository;
            _clientVerifyContent = clientVerifyContent;
            _clientPMIDetails = clientPMIDetails;
            _ruleService = ruleService;
            _cacheData = cacheData;
        }

        public virtual ActionResult Index(string operationNumber, bool ErrorMessage = false)
        {
            Logger.GetLogger().WriteDebug("PMI", "StageFourController Index");

            ViewBag.NumberOperation = operationNumber;
            ViewBag.OperationNUmber = operationNumber;
            ViewBag.Tab = "S4G";

            string cacheName = CacheHelper.GetCacheName(false, IDBContext.Current.Operation);

            var pmiOperation = _cacheData.Get<PMIOperation>(cacheName) ??
                _cacheData.Add(
                    cacheName,
                    _clientPMIDetails.LoadOperationContext(operationNumber, true, false),
                    _timeCachingVal);

            var areTLCompletedButtonsDisabled = false;

            bool.TryParse(
                ConfigurationManager.AppSettings.Get("PMI.PmrCycle.TLCompletedButtons.Disabled"),
                out areTLCompletedButtonsDisabled);

            var validationModel = _clientPMIDetails.GetPMIDetailsModelByStage(
                4, operationNumber, pmiOperation, areTLCompletedButtonsDisabled);

            if (validationModel.ValidationProcess != null)
            {
                validationModel.IsLiveMode = false;

                if (!validationModel.ValidationProcess.HasApprovalDate)
                    return View("WarningMessage");
            }

            if (ErrorMessage)
            {
                MessageConfiguration message = MessageHandler.SetMessageSendRequest(
                    MessageSendRequestCode.ErrorAndMessage,
                    false,
                    2,
                    "There is a current workflow creation in progress");

                ViewData["message"] = message;
            }

            return View(validationModel);
        }

        [HttpGet]
        public virtual ActionResult ConfirmationMessage(
            string operationNumber,
            string confirmationMessageWF,
            bool prevVersion = true)
        {
            var isMatrixChangesValid = _clientVerifyContent.AllMatrixChangesAreExplained(
                new MW.Domain.Models.Architecture.ResultMatrix.Common.OperationModel()
                {
                    OperationNumber = operationNumber
                },
                IDBContext.Current.CurrentLanguage);

            string cacheName = CacheHelper.GetCacheName(false, IDBContext.Current.Operation);
            var pmiOperation = _cacheData.Get<PMIOperation>(cacheName) ??
                _cacheData.Add(cacheName,
                _clientPMIDetails.LoadOperationContext(operationNumber, prevVersion, false),
                _timeCachingVal);

            ViewData["isMatrixChangesValid"] = isMatrixChangesValid;
            ViewData["WFStart"] = _clientPMIDetails.GetConfirmationMessageStartWorkflow(
                confirmationMessageWF,
                operationNumber,
                isMatrixChangesValid,
                pmiOperation);

            return PartialView(pmiOperation.CycleTypeCode.ToUpper() == "S");
        }

        [HasPermission(Permissions = "PMI Workflow")]
        public virtual ActionResult BeginTLcompleted(
            string operationNumber,
            string classification,
            bool editable = false,
            bool prevVersion = true)
        {
            string cacheName = CacheHelper.GetCacheName(false, IDBContext.Current.Operation);

            var pmiOperation = _cacheData.Get<PMIOperation>(cacheName) ??
                _cacheData.Add(
                cacheName,
                _clientPMIDetails.LoadOperationContext(operationNumber, prevVersion, false),
                _timeCachingVal);

            var validationModel = _clientPMIDetails
               .GetPMIDetailsModelByStage(4, operationNumber, pmiOperation);

            if (validationModel.IsCycleMarch)
            {
                Session["OperationContext"] = pmiOperation;
                ViewBag.PartialAction = "March";
                ViewBag.Workflow = K2CallType.March;
                var listValidatorsMarch = _globalRepository.WorkFlowTaskValidatorList("WF-PMI-001", null);
                ViewBag.AdditionalValidators = listValidatorsMarch;
            }
            else
            {
                ViewBag.PartialAction = "September";
                ViewBag.Workflow = K2CallType.September;
                var listValidatorsStartSept = _globalRepository.WorkFlowTaskValidatorList("WF-PMI-003", null);
                ViewBag.AdditionalValidators = listValidatorsStartSept;
            }

            ViewBag.classification = classification == null ? GetClassification(operationNumber) : classification;
            var operation = _basicDataModelRepository
                  .FindOneBasicDataModel(new BasicDataModel
                  {
                      OperationNumber = operationNumber
                  });

            var details = _clientPMIDetails.GetBasicPartialsData(operationNumber, IDBContext.Current.UserName);
            operation.UserComments.AddRange(details.UserComments);
            operation.BasicPMIWorkflow.UserComments.AddRange(details.UserComments);
            operation.BasicPMIWorkflow.OperationNumber = operationNumber;
            operation.BasicPMIWorkflow.PMRCycleId = details.PMRCycleId;
            operation.BasicPMIWorkflow.ResultsMatrixId = details.ResultsMatrixId;

            if (editable)
            {
                return View("SendRequestEdit", operation);
            }
            else
            {
                return View("SendRequest", operation);
            }
        }

        [HasPermission(Permissions = "PMI Workflow")]
        public virtual ActionResult BeginPMRCycleStartup(bool? editable)
        {
            var operation = _basicDataModelRepository
                  .FindOneBasicDataModel(new BasicDataModel
                  {
                      OperationNumber = IDBContext.Current.Operation
                  });

            var listValidators = _globalRepository.WorkFlowTaskValidatorList("WF-PMI-004", null);
            ViewBag.AdditionalValidators = listValidators;
            ViewBag.Workflow = K2CallType.PMRCycleStartup;
            ViewBag.classification = GetClassification(operation.OperationNumber);

            if (ViewBag.classification == null)
            {
                ViewBag.classification = "N/A";
            }

            var details = _clientPMIDetails
                .GetBasicPartialsData(IDBContext.Current.Operation, IDBContext.Current.UserName);
            var pmrCycleCode = _globalRepository.GetPMRCycleCode(details.PMRCycleId);

            if (pmrCycleCode.ToUpper().Equals("M"))
            {
                ViewBag.PartialAction = "March";
            }
            else if (pmrCycleCode.ToUpper().Equals("S"))
            {
                ViewBag.PartialAction = "Startup";
            }

            operation.UserComments.AddRange(details.UserComments);
            operation.BasicPMIWorkflow.UserComments.AddRange(details.UserComments);
            operation.BasicPMIWorkflow.OperationNumber = IDBContext.Current.Operation;
            operation.BasicPMIWorkflow.PMRCycleId = details.PMRCycleId;
            operation.BasicPMIWorkflow.ResultsMatrixId = details.ResultsMatrixId;

            if (editable.IsTrue())
            {
                return View("SendRequestEdit", operation);
            }
            else
            {
                return View("SendRequest", operation);
            }
        }

        [HasPermission(Permissions = "PMI Workflow")]
        public virtual ActionResult BeginStartupPlan(
            string operationNumber,
            bool editable = false)
        {
            var operation = _basicDataModelRepository
                  .FindOneBasicDataModel(new BasicDataModel
                  {
                      OperationNumber = operationNumber
                  });

            var ListValidators = _globalRepository.WorkFlowTaskValidatorList("WF-PMI-002", null);
            ViewBag.AdditionalValidators = ListValidators;
            ViewBag.PartialAction = "Startup";
            ViewBag.Workflow = K2CallType.StartUpPlanValidation;

            var Details = _clientPMIDetails.GetBasicPartialsData(operationNumber, IDBContext.Current.UserName);
            operation.UserComments.AddRange(Details.UserComments);
            operation.BasicPMIWorkflow.UserComments.AddRange(Details.UserComments);
            operation.BasicPMIWorkflow.OperationNumber = operationNumber;
            operation.BasicPMIWorkflow.PMRCycleId = Details.PMRCycleId;
            operation.BasicPMIWorkflow.ResultsMatrixId = Details.ResultsMatrixId;

            if (editable)
            {
                return View("SendRequestEdit", operation);
            }
            else
            {
                return View("SendRequest", operation);
            }
        }

        [HttpPost()]
        public virtual ActionResult SaveCommentsPMI(
          BasicDataModel modelPMI,
          int operationId,
          int callType,
          string classification,
          string additional_validators,
          bool Request = false)
        {
            var model = modelPMI.BasicPMIWorkflow;
            model.UserComments = modelPMI.UserComments;
            model.PMRCycleId = modelPMI.BasicPMIWorkflow.PMRCycleId;
            model.ResultsMatrixId = modelPMI.BasicPMIWorkflow.ResultsMatrixId;
            model.UserName = IDBContext.Current.UserName;
            _resultsMatrixRepository.SaveCommentsPMI(model);

            var callTypeEnum = (K2CallType)callType;
            if (Request)
            {
                return RedirectToAction("HandleRequest",
                    new
                    {
                        operationId = operationId,
                        callType = callType,
                        operationNumber = model.OperationNumber,
                        classification = classification,
                        additionalValidator = additional_validators
                    });
            }
            else
            {
                if (callTypeEnum == K2CallType.StartUpPlanValidation)
                {
                    return RedirectToAction("BeginStartupPlan",
                       new { operationNumber = model.OperationNumber });
                }
                else if (callTypeEnum == K2CallType.PMRCycleStartup)
                {
                    return RedirectToAction("BeginPMRCycleStartup",
                       new { operationNumber = model.OperationNumber, classification });
                }
                else
                {
                    return RedirectToAction("BeginTLCompleted",
                       new { operationNumber = model.OperationNumber, classification });
                }
            }
        }

        [ChildActionOnly]
        public virtual ActionResult March(string operationNumber, bool prevVersion = true)
        {
            string cacheName = CacheHelper.GetCacheName(false, IDBContext.Current.Operation);
            Session["OperationContext"] = _cacheData.Get<PMIOperation>(cacheName) ??
                _cacheData.Add(
                    cacheName,
                    _clientPMIDetails.LoadOperationContext(operationNumber, true, false),
                    _timeCachingVal);

            var model = _clientPMIDetails.GetBasicPartialsData(operationNumber, IDBContext.Current.UserName);
            return View("_March", model);
        }

        [ChildActionOnly]
        public virtual ActionResult September(string operationNumber)
        {
            var model = _clientPMIDetails.GetBasicPartialsData(operationNumber, IDBContext.Current.UserName);
            return View("_September", model);
        }

        [ChildActionOnly]
        public virtual ActionResult Startup(string operationNumber)
        {
            var pmiDetailsModelRepository = Architecture.Globals.Resolve<IPMIDetailsModelRepository>();
            var model = pmiDetailsModelRepository.GetBasicPartialsData(operationNumber, IDBContext.Current.UserName);
            return View("_StartUp", model);
        }

        public virtual ActionResult HandleRequest(
            int operationId,
            int callType,
            string operationNumber,
            string classification,
            string additionalValidator)
        {
            Logger.GetLogger().WriteDebug(
                "PMI",
                string.Format(
                    "Going to create PMI workflow. OPERATIONID: {0}; OPERATIONNUMBER: {1}",
                    operationId,
                    operationNumber));

            int entityId = _clientPMIDetails.GetCurrentResultMatrix(operationId);
            int pmrCycleId = _clientPMIDetails.GetPRMCycle(entityId);
            K2CallType callTypeEnum = (K2CallType)callType;
            DateTime currentDate = DateTime.Now;

            bool TaskApplyMod = _clientPMIDetails.UpdateWFAppPMIdraft(entityId);

            PMIWorkflow workflow = new PMIWorkflow
            {
                OperationNumber = operationNumber,
                Classification = classification,
                EntityId = entityId,
                FolioID = "0",
                ValueDate = DateTime.Now,
                PMRCycleId = pmrCycleId,
                PmrValidationStage = "PMI_TL",
                EntityType = K2CallType.ResultMatrix.GetStringValue(),  //??? Wrong, it should be callTypeEnum
                UserProfileAdditional = additionalValidator,
                UserName = IDBContext.Current.UserName,
                UserProfile = IDBContext.Current.FirstRole
            };

            var currentAmount = _operationClauseClient.CalculateCurrentAmount(operationId, 1);
            string validators =
                PMIWorkflowManager.GetValidators(_ruleService, currentAmount, callTypeEnum);
            PMIWorkflowManager.SetValidatorsToStartWorkflow(
                validators, additionalValidator, workflow);

            string workflowName = callTypeEnum.GetStringValue().ToUpper();

            new CommonDocument().Log(
                LogType.Debug,
                "K2 EXECUTION",
                string.Format("{0}-BEFORE OPERATIONID:{1}",
                    workflowName,
                    IDBContext.Current.Operation),
                workflow);

            var result = PMIWorkflowManager.BeginK2Workflow(
                callTypeEnum, workflow, _k2ServiceProxy);

            new CommonDocument().Log(
                LogType.Debug,
                "K2 EXECUTION",
                string.Format("{0}-AFTER OPERATIONID:{1} K2RESPONSE:{2}",
                    workflowName,
                    IDBContext.Current.Operation,
                    result),
                workflow);

            if (result == K2Response.StartWorkFlow_InProgress.GetStringValue())
            {
                IDBContext.Current.ErrorMessage(result);

                return RedirectToAction(
                    "Index",
                    "StageFour",
                    new
                    {
                        area = "PMI",
                        operationNumber,
                        ErrorMessage = true
                    });
            }
            else if (result != K2Response.StartWorkFlow_PMI.GetStringValue())
            {
                IDBContext.Current.ErrorMessage(result);
                return RedirectToAction(
                    "Index",
                    "StageFour",
                    new
                    {
                        area = "PMI",
                        operationNumber,
                        ErrorMessage = true
                    });
            }

            Logger.GetLogger().WriteDebug(
                "PMI",
                string.Format("Workflow created: {0}. Going to update comments", result));

            _resultsMatrixRepository
                .UpdateCommentsPMIAfterWorkFlowCreation(operationNumber, currentDate);

            Logger.GetLogger().WriteDebug("PMI", "Comments updated");

            return RedirectToAction("Index", "StageFour", new { area = "PMI", operationNumber });
        }

        //This action method is useful during tests and UAT, to force fresh data
        public virtual ActionResult CleanCache(string operationNumber)
        {
            string cacheName = CacheHelper.GetCacheName(false, operationNumber);
            _cacheData.Remove(cacheName, CacheItemRemovedReason.Expired);

            return null;
        }

        string GetClassification(string operationNumber, bool prevVersion = true)
        {
            string cacheName = CacheHelper.GetCacheName(false, IDBContext.Current.Operation);

            var pmiOperation = _cacheData.Get<PMIOperation>(cacheName) ??
                _cacheData.Add(
                    cacheName,
                    _clientPMIDetails.LoadOperationContext(operationNumber, prevVersion, false),
                    _timeCachingVal);

            var state = pmiOperation.SyntethicIndicatorState;

            return state;
        }
    }
}