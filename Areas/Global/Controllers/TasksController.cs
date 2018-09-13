using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;

using IDB.Architecture.Extensions;
using IDB.Architecture.Helpers;
using IDB.Architecture.Language;
using IDB.Architecture.Logging;
using IDB.Architecture.Security.Models.UserIdentity;
using IDB.MVCControls.General.Helpers;
using IDB.MW.Application.Agreements.Services;
using IDB.MW.Application.Agreements.ViewModel;
using IDB.MW.Application.ClausesAndContracts.Services.RulesEngine;
using IDB.MW.Application.ClausesAndContractsModule.Services.Clauses;
using IDB.MW.Application.Components;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Application.GlobalModule.Helpers;
using IDB.MW.Application.GlobalModule.Services.WorkflowsService;
using IDB.MW.Application.GlobalModule.Workflow;
using IDB.MW.Application.NoK2Workflows.Enums;
using IDB.MW.Application.NoK2Workflows.Services;
using IDB.MW.Application.OPUSModule.Services.DocumentService;
using IDB.MW.Application.OPUSModule.Services.K2DataService;
using IDB.MW.Application.SupervisionPlanModule.ViewModels;
using IDB.MW.Business.Core.K2Manager.Contracts;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.Clauses;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.MasterDefinitions;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.SupervisionPlans.PMI;
using IDB.MW.Domain.Contracts.ModelRepositories.Global;
using IDB.MW.Domain.Contracts.ModelRepositories.K2Service;
using IDB.MW.Domain.Contracts.ModelRepositories.OperationProfile.BasicData;
using IDB.MW.Domain.Contracts.ModelRepositories.Supervision.PMI;
using IDB.MW.Domain.Contracts.ModelRepositories.Supervision.SupervisionPlan;
using IDB.MW.Domain.Entities;
using IDB.MW.Domain.Models.Architecture.Enumerations;
using IDB.MW.Domain.Models.Clauses;
using IDB.MW.Domain.Models.Common;
using IDB.MW.Domain.Models.Global;
using IDB.MW.Domain.Models.K2Service;
using IDB.MW.Domain.Models.Pagination;
using IDB.MW.Domain.Models.Supervision.SupervisionPlan;
using IDB.MW.Domain.Models.Supervision.Workflow;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.Domain.Values.Workflows.General;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.MW.DomainModel.Contracts.Repositories.VERModule;
using IDB.MW.DomainModel.Entities.Core.K2;
using IDB.MW.GenericWorkflow.Helpers;
using IDB.MW.GenericWorkflow.Services;
using IDB.MW.Infrastructure.Data.Optima.Repositories.Models.Global;
using IDB.Presentation.MVC4.Areas.Clauses.Controllers;
using IDB.Presentation.MVC4.Areas.Global.Models;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVC4.MVCCommon;

using DelegatingTaskSearchModel = IDB.MW.Domain.Models.Global.Tasks.DelegatingTaskSearchModel;
using GlobalReferences = IDB.MW.Domain.Models.Global;

namespace IDB.Presentation.MVC4.Areas.Global.Controllers
{
    public enum SupervisionPlanView
    {
        CriticalProducts = 0,
        Activities = 1,
        Budget = 2,
        Comments = 3
    }

    public partial class TasksController : BaseController
    {
        public const string CONVERGENCE_ROLES = "CONVERGENCE ROLES";

        private const string K2PMIFinishState = "Apply Modifications";
        private const string NOT_APPLICABLE_CLASSIFICATION = "N/A";

        public virtual IGlobalModelRepository GlobalModelRepository { get; set; }
        public virtual IK2ServiceProxy K2ServiceProxy { get; set; }
        public virtual IOperationModelRepository OperationClauseClient { get; set; }

        public ISupervisionPlanModelRepository ClientSupervisionPlanModelRepository { get; set; }

        private readonly ICatalogService _catalogService;
        private readonly IK2DataService _k2DataService;
        private readonly IWorkflowInstanceAdditionalValidatorRepository
            _workflowInstanceAdditionalValidatorRepository;
        private readonly IClauseBusinessLogicRuleService _businessRuleService;
        private readonly IWorkflowsService _workflowsService;
        private readonly IDocumentService _documentService;
        private readonly IAgreementAndConditionService _agreementAndConditionService;
        private readonly IClauseService _clauseService;
        private readonly IVerTaskParticipantRepository _verTaskParticipantRepository;
        private readonly INoK2WorkflowService _noK2WorkflowsService;
        private readonly IValidatorService _validatorService;

        private IK2Manager _k2Manager;
        private IK2IntegrationWorkflow _k2integration;
        private IWorkflowInstanceTaskRepository _workflowInstanceTaskRepository;
        private IConvergenceMasterDataModelRepository _masterDataModel;
        private IGlobalModelRepository _globalModelRepository { get; set; }
        private IK2ServiceProxy _k2ServiceProxy { get; set; }
        private IOperationModelRepository _operationClause { get; set; }
        private ISupervisionPlanModelRepository _supervisionPlanModelRepository { get; set; }
        private IPMIDetailsModelRepository _validationPMIDetails { get; set; }
        private IBasicDataModelRepository _basicDataModelRepository { get; set; }
        private IResultsMatrixModelRepository _resultsMatrixModelRepository { get; set; }
        private IRuleEvaluatorService _ruleService { get; set; }

        private readonly IWorkflowManagerService _workflowManagerService;

        public TasksController(
            IConvergenceMasterDataModelRepository masterDataModel,
            IGlobalModelRepository globalModelRepositoryClient,
            IK2ServiceProxy k2ServiceProxy,
            IOperationModelRepository operationClause,
            ISupervisionPlanModelRepository supervisionPlanModelRepository,
            IPMIDetailsModelRepository validationPMIDetails,
            IBasicDataModelRepository basicDataModelRepository,
            IResultsMatrixModelRepository resultsMatrixModelRepository,
            IRuleEvaluatorService ruleService,
            IClauseBusinessLogicRuleService businessRuleService,
            IK2Manager k2Manager,
            ICatalogService catalogService,
            IK2DataService k2DataService,
            IWorkflowsService workflowsService,
            IDocumentService documentService,
            IWorkflowInstanceAdditionalValidatorRepository workflowInstanceAdditionalValidatorRepository,
            IAgreementAndConditionService agreementAndConditionService,
            IK2IntegrationWorkflow k2IntegrationWorkflow,
            IWorkflowInstanceTaskRepository workflowInstanceTaskRepository,
            IVerTaskParticipantRepository verTaskParticipantRepository,
            IClauseService clauseServ,
            IWorkflowManagerService workflowManagerService,
            INoK2WorkflowService noK2WorkflowsService,
            IValidatorService validatorService)
        {
            _masterDataModel = masterDataModel;
            _globalModelRepository = globalModelRepositoryClient;
            _k2ServiceProxy = k2ServiceProxy;
            _supervisionPlanModelRepository = supervisionPlanModelRepository;
            _validationPMIDetails = validationPMIDetails;
            _basicDataModelRepository = basicDataModelRepository;
            _resultsMatrixModelRepository = resultsMatrixModelRepository;
            _ruleService = ruleService;
            _operationClause = operationClause;
            _businessRuleService = businessRuleService;
            _k2Manager = k2Manager;
            _k2integration = k2IntegrationWorkflow;
            _workflowInstanceTaskRepository = workflowInstanceTaskRepository;
            _catalogService = catalogService;
            _k2DataService = k2DataService;
            _workflowsService = workflowsService;
            _documentService = documentService;
            _workflowInstanceAdditionalValidatorRepository = workflowInstanceAdditionalValidatorRepository;
            _agreementAndConditionService = agreementAndConditionService;
            _verTaskParticipantRepository = verTaskParticipantRepository;
            _clauseService = clauseServ;
            _workflowManagerService = workflowManagerService;
            _noK2WorkflowsService = noK2WorkflowsService;
            _validatorService = validatorService;
        }

        public virtual ActionResult Index(
            string operationNumber = null,
            int state = 0,
            string status = null)
        {
            ViewBag.opNumber = operationNumber;
            ViewBag.OpDetailURL = GlobalCommonLogic.GetOperationDetailURL();

            @ViewBag.OperationNumber = operationNumber;

            if (state != 0)
            {
                var message = MessageHandler.setMessageTasks(state, false, 2, status);
                ViewData["message"] = message;
            }

            ViewBag.TaskType = _globalModelRepository.GetTasksTypes();

            return View();
        }

        public virtual ActionResult DetailWG(int id, bool edit = true, int state = 0, string status = null)
        {
            var taskModel = GlobalModelRepository.GetSpecificTask(id, IDBContext.Current.CurrentLanguage);
            taskModel.Documents = new List<DocumentGWViewModel>();
            var taskDataModel = taskModel.ConvertToTaskDataModel();
            var workflowInstanceTask = _workflowInstanceTaskRepository.GetOne(x => x.WorkflowInstanceTaskId.Equals(id));
            int taskTypeID = workflowInstanceTask.WorkflowTaskTypeId;
            var roles = GetListMasterData(CONVERGENCE_ROLES);
            var currentTaskValidators = _workflowInstanceAdditionalValidatorRepository.GetByCriteria(a => a.WorkflowTaskTypeId == taskTypeID && a.WorkflowInstanceId == taskDataModel.WorkflowInstanceId).Select(a => a.UserProfile).ToList();
            var validators = _k2integration.GetHistoricAndHappyRoadValidators(workflowInstanceTask.WorkflowInstanceId);
            var firstValidatorHappy = validators.Any(x => x.Order == -1) ? validators.Where(x => x.Order == -1).FirstOrDefault() : null;
            if (firstValidatorHappy != null)
            {
                validators.Remove(firstValidatorHappy);
            }

            ViewBag.validators = validators;
            ViewBag.principalTaskName = firstValidatorHappy != null ? firstValidatorHappy.TaskName : string.Empty; //validators.First(x => x.Status == string.Empty).TaskName;
            ViewBag.UserName = IDBContext.Current.UserName;
            ViewBag.TaskActions = GlobalModelRepository.GetActionsByInstanceTaskCode(taskModel.TaskTypeCode);
            ViewBag.UserComments = _k2integration.GetCommentsByWorkTaskID(taskDataModel.WorkflowInstanceId);
            ViewBag.SerializedViewModel = IDB.Presentation.MVC4.Helpers.PageSerializationHelper.SerializeObject(taskDataModel);
            ViewBag.HasComments = workflowInstanceTask.WorkflowTaskType.WorkflowTaskTypeConfiguration != null ?
                    workflowInstanceTask.WorkflowTaskType.WorkflowTaskTypeConfiguration.HasComment : true; // TODO - Cambiar por False
            ViewBag.HasDocuments = workflowInstanceTask.WorkflowTaskType.WorkflowTaskTypeConfiguration != null ?
                    workflowInstanceTask.WorkflowTaskType.WorkflowTaskTypeConfiguration.HasDocument : true; // TODO - Cambiar por False

            if (currentTaskValidators.Any())
            {
                ViewBag.RetrieveListRoles = RemoveUsedRole(currentTaskValidators, roles);
            }
            else
            {
                ViewBag.RetrieveListRoles = RemoveUsedRole(ViewBag.validators, roles);
            }

            if (taskModel.Code.StartsWith("WF-G-"))
            {
                ViewBag.instructionsFT = Localization.GetText(workflowInstanceTask.WorkflowTaskType.WorkflowTaskTypeConfiguration.Instructions.ToString());
                ViewBag.elementId = taskDataModel.EntityId;
            }

            var result = _workflowsService.GetDocumentByTask(id).DocumentGWInstanceTaskResponse.ToList();
            taskModel.Documents.AddRange(result);
            ViewBag.showLoader = true;
            return View("DetailWG", taskModel);
        }

        public virtual JsonResult AddNewDocument(string documentNumber)
        {
            var document = new
            {
                DocumentNumber = documentNumber,
                Date = FormatHelper.Format(DateTime.Now.Date),
                User = IDBContext.Current.UserName
            };

            var result = Json(new { data = document }, JsonRequestBehavior.AllowGet);

            return result;
        }

        public virtual ActionResult DocumentsView(int taskId, int? ispartial)
        {
            var task = _workflowInstanceTaskRepository.GetOne(x => x.WorkflowInstanceTaskId.Equals(taskId));
            var taskModel = GlobalModelRepository.GetSpecificTask(taskId, IDBContext.Current.CurrentLanguage);
            if (task == null)
            {
                throw new Exception("The task cannot be null.");
            }

            var response = _workflowsService.GetDocumentByTask(taskId);

            return PartialView("Partials/DataListViews/WorkflowDocuments", taskModel);
        }

        public virtual JsonResult SaveWG(string operationNumber)
        {
            IDB.MW.Infrastructure.ApplicationBase.Messages.ResponseBase response = new IDB.MW.Infrastructure.ApplicationBase.Messages.ResponseBase();
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<IDB.MW.DomainModel.Entities.Core.K2.TaskDataModel>(jsonDataRequest.SerializedData);
            response.IsValid = true;

            try
            {
                var newComments = jsonDataRequest.ClientFieldData.Where(o => o.Name.Equals("newComment"));

                if (newComments != null)
                {
                    for (int i = 0; i < newComments.Count(); i++)
                    {
                        var comment = new IDB.MW.Domain.Models.Global.UserCommentModel();
                        comment.Text = newComments.ElementAt(i).Value.Trim();
                        comment.WorkflowInstanceTaskId = viewModel.TaskId;
                        comment.CreatedBy = IDBContext.Current.UserName;
                        comment.ModifiedBy = IDBContext.Current.UserName;
                        response.IsValid = GlobalModelRepository.UpdateCommentsByTaskWG(comment, null);
                    }
                }

                var additionalValidators = jsonDataRequest.ClientFieldData.Where(o => o.Name.Equals("newUserProfile"));

                if (viewModel.Validators == null)
                {
                    viewModel.Validators = new List<ValidatorsAddViewModel>();
                }

                for (int i = 0; i < additionalValidators.Count(); i++)
                {
                    var validator = new ValidatorsAddViewModel();
                    validator.Role = additionalValidators.ElementAt(i).Value;
                    validator.Status = "Pending";
                    validator.Mandatory = false;
                    viewModel.Validators.Add(validator);
                }

                var deleteValidators = jsonDataRequest.ClientFieldData.FirstOrDefault(o => o.Name.Equals("deleteValidators"));

                if (deleteValidators != null)
                {
                    string[] deleteV = deleteValidators.Value.ToString().Split('|').Where(x => !string.IsNullOrEmpty(x)).ToArray();

                    if (viewModel.DeleteValidators == null)
                    {
                        viewModel.DeleteValidators = new List<int>();
                    }

                    foreach (string s in deleteV)
                    {
                        viewModel.DeleteValidators.Add(Convert.ToInt32(s));
                        viewModel.Validators.RemoveAll(d => d.Order.Equals(Convert.ToInt32(s)));
                    }
                }

                var listAdditionalValidators = viewModel.Validators.Where(a => a.Mandatory == false).Select(x => new { x.Role });
                var listValidators = new List<string>();

                foreach (var validator in listAdditionalValidators)
                {
                    listValidators.Add(validator.Role);
                }

                var listAdditionalValidatorsPresent = listAdditionalValidators.Count() == 0 ? false : true;
                var arrayTag = new List<string>();
                arrayTag.Add("\"" + GenericWorkflowHelper.AddValidators + "\":" + (listAdditionalValidatorsPresent ? "\"" + string.Join("|", listValidators) + "\"" : "\"\""));

                var taskModel = GlobalModelRepository.GetSpecificTask(viewModel.TaskId, IDBContext.Current.CurrentLanguage);
                var changes = jsonDataRequest.ClientFieldData.FirstOrDefault(o => o.Name.Equals("changeStatus"));

                if (changes.Value != "none")
                {
                    var workflow = new WorkflowEntity
                    {
                        Status = changes.Value,
                        UserName = IDBContext.Current.UserName,
                    };
                    var modalTag  = jsonDataRequest.ClientFieldData.FirstOrDefault(o => o.Name.Equals("modalTag"));
                    var modalTagString = modalTag != null ? modalTag.Value : string.Empty;

                    var parameters = new Dictionary<string, object>();
                    string userName = string.IsNullOrEmpty(viewModel.UserName) ? IDBContext.Current.UserName : viewModel.UserName;
                    userName = !string.IsNullOrWhiteSpace(userName) ? userName : "K2TESTUSER";
                    Logger.GetLogger().WriteDebug("SaveController", "Method: MissionsWorkflow : UserName: " + userName);

                    parameters.Add("CurrentEntityStatus", workflow.Status);
                    parameters.Add("TaskUser", userName);
                    if (listValidators.Count() > 0)
                    {
                        parameters.Add("AddValidators", string.Join("|", listValidators));
                        parameters.Add("tag", "{" + string.Join(",", arrayTag) + "," + modalTagString + "}");
                    }
                    else if (!string.IsNullOrWhiteSpace(modalTagString))
                    {
                        parameters.Add("tag", "{" + modalTagString + "}");
                    }

                    _k2integration.AdvanceWorkflow(taskModel.Code, taskModel.TaskFolio, taskModel.TaskOperation, taskModel.IdentityId, workflow.Status, parameters, taskModel.EntityType, taskModel.TaskId, workflow.Status, taskModel.TaskTypeCode);
                }

                try
                {
                    taskModel.Documents = new List<DocumentGWViewModel>();
                    var documents = jsonDataRequest.ClientFieldData.Where(o => o.Name.Equals("documentNumber")).ToList();
                    DocumentGWIntanceTaskViewModel DocumentTask = new DocumentGWIntanceTaskViewModel();
                    DocumentTask.Documents = new List<DocumentGWViewModel>();
                    DocumentTask.TaskId = taskModel.TaskId;

                    foreach (var item in documents)
                    {
                        var documentResponse = _workflowsService.GetDocumentsByDocNumber(item.Value);
                        DocumentGWViewModel newDocument = new DocumentGWViewModel();

                        newDocument.DocumentId = documentResponse.DocumentGWInstanceTaskResponse.DocumentId;
                        newDocument.DocNumber = documentResponse.DocumentGWInstanceTaskResponse.DocNumber;
                        newDocument.Date = DateTime.Now;
                        newDocument.User = IDBContext.Current.UserName.Substring(0, 4) == @"IDB\" ? IDBContext.Current.UserName.Substring(3) : IDBContext.Current.UserName;
                        newDocument.Description = documentResponse.DocumentGWInstanceTaskResponse.Description;
                        newDocument.FileName = documentResponse.DocumentGWInstanceTaskResponse.FileName;
                        DocumentTask.Documents.Add(newDocument);
                    }

                    response = _workflowsService.AddDocumentsTask(DocumentTask);
                }
                catch (Exception e)
                {
                    response.IsValid = false;
                    response.ErrorMessage = e.ToString();
                }
            }
            catch (Exception e)
            {
                response.IsValid = false;
                response.ErrorMessage = e.ToString();
            }

            return Json(response);
        }

        public virtual ActionResult Detail(int id, bool edit = true, int state = 0, string status = null)
        {
            var taskModel = GlobalModelRepository
                .GetSpecificTask(id, IDBContext.Current.CurrentLanguage);

            var request = new DataSourceRequest();

            var userTask = _globalModelRepository
                .GetTaskByUser(IDBContext.Current.UserName,
                    request,
                    GlobalCommonLogic.GetOperationDetailURL(),
                    GlobalCommonLogic.GetOperationDraftDetailURL(),
                    taskModel.OperationNumber);

            var hasTask = userTask.Data.Where(lt => (lt.WorkflowInstanceTaskId == id &&
               lt.OperationNumber == IDBContext.Current.Operation));

            if (!hasTask.Any() && string.IsNullOrEmpty(taskModel.TaskStatus))
            {
                var routing = new
                {
                    operationNumber = taskModel.OperationNumber,
                    Operation = taskModel.OperationNumber,
                    Status = string.Empty,
                    State = 1002
                };

                return RedirectToAction("Index", routing);
            }

            // Redirect to Detalis for generic workflow
            if (!taskModel.WithK2)
            {
                var routing = new
                {
                    area = "Workflow",
                    operationNumber = taskModel.OperationNumber,
                    taskId = id,
                    edit = edit,
                    State = state,
                    Status = status
                };
                return RedirectToAction("Detail", "Task", routing);
            }

            if (taskModel.Code.StartsWith("WF-G-"))
            {
                var routing = new
                {
                    id = id,
                    edit = edit,
                    State = state,
                    Status = status
                };
                return RedirectToAction("DetailWG", routing);
            }

            var taskDataModel = taskModel.ConvertToTaskDataModel();

            _globalModelRepository.UpdateTaskStatus(id, string.Empty, 0, string.Empty, true);

            ViewData["operationNumber"] = taskModel.OperationNumber;
            ViewBag.CancelButtonAction = false;
            ViewBag.IsPCRWorkFlow = false;
            ViewBag.IsVisualizationFlow = false;

            if (!string.IsNullOrWhiteSpace(taskModel.TaskStatus))
            {
                var routing = new
                {
                    operationNumber = taskModel.OperationNumber,
                    Operation = taskModel.OperationNumber,
                    Status = string.Empty,
                    State = 1001
                };
                return RedirectToAction("Index", routing);
            }

            ViewData["operationNumber"] = taskModel.OperationNumber;
            ViewBag.CancelButtonAction = false;
            ViewBag.IsPCRWorkFlow = false;
            ViewBag.IsVisualizationFlow = false;
            ViewBag.PMIFinishTask = GlobalModelRepository.TaskGetTypeTask(id) == K2PMIFinishState;

            var entityValues = GlobalModelRepository
                .GetValuesWorkFlowTasksType(taskModel.IdentityId, taskModel.EntityType);

            if (taskModel.Code == WorkflowCodes.WF_PCR_001.GetStringValue())
            {
                ViewBag.IsPCRWorkFlow = true;
                ViewBag.IsVisualizationFlow = false;
            }

            if (taskModel.Code == WorkflowCodes.WF_VER_001.GetStringValue())
            {
                var participantVer = _verTaskParticipantRepository.GetByCriteria(x => x.WorkflowInstanceTaskId == taskDataModel.TaskId).FirstOrDefault();
                ViewBag.instructionsFT = participantVer != null ? participantVer.Instruction : string.Empty;

                ViewBag.elementId = taskDataModel.EntityId;
            }

            if (taskModel.Code == WorkflowCodes.WF_MIS_001.GetStringValue() ||
                taskModel.Code == WorkflowCodes.WF_MIS_002.GetStringValue() ||
                taskModel.Code == WorkflowCodes.WF_MIS_003.GetStringValue())
            {
                ViewBag.instructionsFT =
                    string.Format(Localization.GetText("OP.MS.K2.ApproveMission.Instructions"),
                        "<a href=\"" + string.Format(IDB.Architecture.Globals.GetSetting("BasePath") +
                            taskModel.SplitViewUrl,
                        taskModel.OperationNumber,
                        taskDataModel.Parameters["MissionID"]) + "\" target=\"splitFrame\">" +
                            Localization.GetText("OP.MS.MissionsWorkflow.MissionElectronic") + "</a>");

                ViewBag.elementId = taskDataModel.Parameters["MissionID"];
            }

            if (taskModel.Code == WorkflowCodes.WF_SP_001.GetStringValue())
            {
                ViewBag.elementId = taskDataModel.EntityId;
            }

            if (taskModel.Code == WorkflowCodes.WF_G_SGP_001.GetStringValue())
            {
                ViewBag.elementId = taskDataModel.Parameters["ProcurementId"];
            }

            if (taskModel.Code == WorkflowCodes.WF_INS_001.GetStringValue())
            {
                ViewBag.instructionsFT =
                    Localization.GetText("OP.IN.K2.ApproveTaskWorkflow.Instructions");

                ViewBag.elementId = taskDataModel.Parameters["Institution_ID"];
            }

            if (taskModel.Code == WorkflowCodes.WF_CA_001.GetStringValue() ||
                taskModel.Code == WorkflowCodes.WF_CA_002.GetStringValue() ||
                taskModel.Code == WorkflowCodes.WF_CA_003.GetStringValue())
            {
                ViewBag.instructionsFT =
                    Localization.GetText("OP.CR.K2.ApproveTaskWorkflow.Instructions");
                ViewBag.elementId = string.Empty;
            }

            if (taskModel.Code == WorkflowCodes.WF_TCM_001.GetStringValue())
            {
                string FinalR = taskDataModel.Parameters["IsFinalReport"].ToString();
                ViewBag.instructionsFT = FinalR.Equals("True")
                    ? Localization.GetText("TCM.TR.TaskAndRequestFoundCoordinator.ContentInstructions") +
                        "<br>" +
                        Localization.GetText(
                                "TCM.TR.TaskAndRequestFoundCoordinator.ContentInstructions.IsFinalReport")
                    : Localization
                        .GetText("TCM.TR.TaskAndRequestFoundCoordinator.ContentInstructions");
                ViewBag.elementId = taskDataModel.EntityId;
            }

            if (taskModel.Code == WorkflowCodes.WF_ECO_001.GetStringValue())
            {
                switch (taskDataModel.TaskTypeCode)
                {
                    case IDB.MW.Application.ESW_CIPModule.Helpers.K2ECOHelpers.WF_ECO_001_T1:
                        ViewBag.instructionsFT =
                            Localization.GetText("R6.Proposal.K2.Instructions.Create");
                        break;

                    case IDB.MW.Application.ESW_CIPModule.Helpers.K2ECOHelpers.WF_ECO_001_T2:
                    case IDB.MW.Application.ESW_CIPModule.Helpers.K2ECOHelpers.WF_ECO_001_T4:
                    case IDB.MW.Application.ESW_CIPModule.Helpers.K2ECOHelpers.WF_ECO_001_T5:
                    case IDB.MW.Application.ESW_CIPModule.Helpers.K2ECOHelpers.WF_ECO_001_T3:
                        ViewBag.instructionsFT = string.Format(
                            Localization.GetText("R6.Proposal.K2.Instructions.SaveReturn"),
                            "ESW/CIP");
                        break;

                    case IDB.MW.Application.ESW_CIPModule.Helpers.K2ECOHelpers.WF_ECO_001_T6:
                        ViewBag.instructionsFT = string.Format(
                            Localization.GetText("R6.Proposal.K2.Instructions.Update"),
                            "ESW/CIP",
                            taskDataModel
                                .Parameters[IDB.MW.Application.ESW_CIPModule.Helpers.K2ECOHelpers.TagRole]);
                        break;
                    default:
                        ViewBag.instructionsFT = string.Empty;
                        break;
                }

                ViewBag.elementId = taskDataModel
                    .Parameters[IDB.MW.Application.ESW_CIPModule.Helpers.K2ECOHelpers.TagProposalId]
                    + "&" + IDB.MW.Application.ESW_CIPModule.Helpers.K2ECOHelpers.TagProposalType + "="
                    + taskDataModel
                    .Parameters[IDB.MW.Application.ESW_CIPModule.Helpers.K2ECOHelpers.TagProposalType]
                    + "&" + IDB.MW.Application.ESW_CIPModule.Helpers.K2ECOHelpers.TagProposalYear + "="
                    + taskDataModel
                    .Parameters[IDB.MW.Application.ESW_CIPModule.Helpers.K2ECOHelpers.TagProposalYear];
            }

            if (taskModel.Code == WorkflowCodes.WF_CL_003.GetStringValue())
            {
                if (taskDataModel.EntityType == AgreementsAndConditionsConstants.WF_ENTITY_TYPE_CONDITION_EXTENSION)
                {
                    var response = _agreementAndConditionService.GetTaskModel(taskModel);

                    ViewBag.ConvarIdbRequest = response.Model.ConditionIndividuals[0]
                        .ConditionExtensions[0].RequestedExtensionMonths;
                    ViewBag.ConvarRequestedDate = response.Model.ConditionIndividuals[0]
                        .ConditionExtensions[0].RequestedExtensionDate;
                }
                else
                {
                    var clauseId = _globalModelRepository.GetClauseIdByClauseIndividual(taskModel.IdentityId);

                    var clauseIndividualIdClauseExtension = _globalModelRepository
                        .GetClauseIndividualByClauseExtension(taskModel.IdentityId);

                    var RequestAndExecutor = _globalModelRepository
                        .GetRequestAndExecutor(taskModel.IdentityId);
                    ViewBag.ConvarIdbRequest = RequestAndExecutor.ElementAt(0);
                    ViewBag.ConvarExecutorRequest = RequestAndExecutor.ElementAt(1);
                    ViewBag.ConvarRequestedDate = _globalModelRepository
                        .GetRequestedDateByIndividualId(clauseIndividualIdClauseExtension);
                }
            }

            ViewBag.IsPMITask = taskModel.Code == WorkflowCodes.WF_PMI_001.GetStringValue();

            if (taskModel.Code == WorkflowCodes.WF_VI_002.GetStringValue())
            {
                ViewBag.PMIFinishTask = false;
                ViewBag.IsPCRWorkFlow = false;
                ViewBag.IsVisualizationFlow = true;
            }

            if (taskModel.Code == WorkflowCodes.WF_LC_001.GetStringValue())
            {
                ViewBag.elementId = taskDataModel.EntityId;
            }

            switch (taskModel.EntityType)
            {
                case "ClauseExtension":

                    taskModel.TaskName = Localization.GetText(taskModel.TaskName) +
                        " " + Localization.GetText("for contract") + " " + entityValues["Contract"] +
                        " " + Localization.GetText("and clause") + " " + entityValues["Clause"];
                    ViewBag.ClauseNumber = entityValues["Clause"];
                    ViewBag.Description = entityValues["Description"];
                    break;

                case "ClauseIndividual":

                    taskModel.TaskName = Localization.GetText(taskModel.TaskName) +
                        " " + Localization.GetText("for contract") + " " + entityValues["Contract"] +
                        " " + Localization.GetText("and clause") + " " + entityValues["Clause"];
                    ViewBag.ClauseNumber = entityValues["Clause"];
                    ViewBag.Description = entityValues["Description"];
                    ViewBag.Category = entityValues["Category"];
                    taskModel.RequestNameType = OperationClauseClient
                        .GetClauseFinalStatusCodeById(taskModel.IdentityId, Localization.CurrentLanguage);
                    break;

                case AgreementsAndConditionsConstants.WF_ENTITY_TYPE_CONDITION_EXTENSION:

                    taskModel.TaskName = string.Format("{0} {1} {2} {3} {4}",
                        Localization.GetText(taskModel.TaskName),
                        Localization.GetText("for agreement"),
                        entityValues["AgreementNumber"],
                        Localization.GetText("and condition"),
                        entityValues["ConditionNumber"]);
                    ViewBag.ClauseNumber = entityValues["ConditionNumber"];
                    ViewBag.Description = entityValues["Description"];
                    break;

                case AgreementsAndConditionsConstants.WF_ENTITY_TYPE_CONDITION_INDIVIVDUAL:

                    taskModel.TaskName = string.Format("{0} {1} {2} {3} {4}",
                        Localization.GetText(taskModel.TaskName),
                        Localization.GetText("for agreement"),
                        entityValues["AgreementNumber"],
                        Localization.GetText("and condition"),
                        entityValues["ConditionNumber"]);
                    ViewBag.ClauseNumber = entityValues["ConditionNumber"];
                    ViewBag.Description = entityValues["Description"];
                    ViewBag.Category = entityValues["Category"];

                    var response = _agreementAndConditionService.GetTaskModel(taskModel);
                    taskModel.RequestNameType = response.Model.ConditionIndividuals[0].FinalStatusName ??
                        Localization.GetText("CL.FINALSTATUS.START.K2SCREEN.NOTAVAILABLE");
                    break;

                case "RevolvingFund":

                    taskModel.TaskName = Localization.GetText(taskModel.TaskName) +
                        " " + Localization.GetText("for contract") + " " + entityValues["Contract"];
                    break;

                case "Contract":

                    taskModel.TaskName = Localization.GetText(taskModel.TaskName) +
                        " " + Localization.GetText("for contract") + " " + entityValues["Contract"];
                    break;

                default:

                    taskModel.TaskName = Localization.GetText(taskModel.TaskName);
                    break;
            }

            if (!edit)
            {
                if (state != 0)
                {
                    var message = MessageHandler.setMessageTasks(state, false, 2, status);
                    ViewData["message"] = message;
                }

                return View(taskModel);
            }

            ViewBag.Role = taskModel.UserProfile;

            if (taskModel.UserProfile == Role.DIVISION_CHIEF &&
                IDBContext.Current.Roles.Contains(Role.DIVISION_CHIEF))
                ViewBag.Role = "DCRole";

            if (taskModel.Code != "WF-VI-001" && taskModel.Code != "WF-PCR-001")
            {
                ViewBag.CancelButtonAction = true;
            }

            return View("DetailEdit", taskModel);
        }

        public virtual ActionResult ValidateTask(
            string taskStatus,
            int taskId,
            string classification,
            string additionalValidators)
        {
            if (_noK2WorkflowsService.AdvanceNoK2WorkflowIfApplies(
                taskId, IDBContext.Current.UserName, NoK2WorkflowTaskActionEnum.Validate))
            {
                var routing = new
                {
                    operationNumber = _globalModelRepository.GetOperationNumberByTaskId(taskId),
                    State = 600,
                    Status = string.Empty
                };

                return RedirectToAction("Index", routing);
            }

            AdditionalValidatorsSave(additionalValidators, taskId);

            Logger.GetLogger()
                .WriteDebug("TasksController",
                    string.Format(
                        "GOING TO VALIDATE" + Environment.NewLine +
                        "TASK= {0}," + Environment.NewLine +
                        "STATUS= {1}," + Environment.NewLine +
                        "CLASSIF.= {2}," + Environment.NewLine +
                        "ADDIT.VALIDAT.= {3}",
                        taskId,
                        taskStatus,
                        classification,
                        additionalValidators));

            var workflow =
                new WorkflowEntity
                {
                    Status = K2TaskStatus.Approved.GetStringValue(),
                    UserName = IDBContext.Current.UserName,
                    Classification = classification ?? NOT_APPLICABLE_CLASSIFICATION,
                    PmrValidationStage = taskStatus,
                    ElegibilityDateField = DateTime.UtcNow
                };

            Logger.GetLogger()
                .WriteDebug("MFH - TasksController",
                    string.Format(
                        "'ValidateTask' method: sending K2 UpdateWorkflow request with params:" +
                        Environment.NewLine +
                        "workflow.Status= '{0}'," + Environment.NewLine +
                        "workflow.UserName= '{1}'," + Environment.NewLine +
                        "workflow.Classification= '{2}'," + Environment.NewLine +
                        "workflow.PmrValidationStage= '{3}'," + Environment.NewLine +
                        "workflow.ElegibilityDateField= '{4}'",
                        workflow.Status,
                        workflow.UserName,
                        workflow.Classification,
                        workflow.PmrValidationStage,
                        workflow.ElegibilityDateField));

            var response = _k2ServiceProxy.UpdateWorkflow(workflow, taskId, K2TaskStatus.Approved);

            Logger.GetLogger()
                .WriteDebug("MFH - TasksController",
                    string.Format(
                        "'ValidateTask' method: returning from K2 UpdateWorkflow with reponse= '{0}'",
                        response));

            if (!response.StartsWith(K2Response.UpdateWorkflow.GetStringValue()))
            {
                var routing = new
                {
                    id = taskId,
                    State = 601,
                    Status = response
                };

                return RedirectToAction("Detail", routing);
            }

            if (response == K2Response.StartWorkFlow_InProgress.GetStringValue())
            {
                var routing = new
                {
                    id = taskId,
                    State = 555,
                    Status = response
                };

                return RedirectToAction("Detail", routing);
            }
            else
            {
                var routing = new
                {
                    operationNumber = _globalModelRepository.GetOperationNumberByTaskId(taskId),
                    State = 600,
                    Status = string.Empty
                };

                return RedirectToAction("Index", routing);
            }
        }

        public virtual ActionResult RejectTask(
            string taskStatus,
            int taskId,
            string classification)
        {
            if (_noK2WorkflowsService.AdvanceNoK2WorkflowIfApplies(
                taskId, IDBContext.Current.UserName, NoK2WorkflowTaskActionEnum.Reject))
            {
                var routing = new
                {
                    operationNumber = _globalModelRepository.GetOperationNumberByTaskId(taskId),
                    State = 600,
                    Status = string.Empty
                };

                return RedirectToAction("Index", routing);
            }

            var workflow = new WorkflowEntity
            {
                Status = K2TaskStatus.Rejected.GetStringValue(),
                UserName = IDBContext.Current.UserName,
                Classification = classification ?? NOT_APPLICABLE_CLASSIFICATION,
                PmrValidationStage = taskStatus
            };
            var response = _k2ServiceProxy.UpdateWorkflow(workflow, taskId, K2TaskStatus.Rejected);

            if (!response.StartsWith(K2Response.UpdateWorkflow.GetStringValue()))
            {
                var routing = new
                {
                    id = taskId,
                    State = 601,
                    Status = response
                };

                return RedirectToAction("Detail", routing);
            }

            if (response == K2Response.StartWorkFlow_InProgress.GetStringValue())
            {
                var routing = new
                {
                    id = taskId,
                    State = 555,
                    Status = response
                };

                return RedirectToAction("Detail", routing);
            }
            else
            {
                var routing = new
                {
                    operationNumber = _globalModelRepository.GetOperationNumberByTaskId(taskId),
                    State = 600,
                    Status = response
                };

                return RedirectToAction("Index", routing);
            }
        }

        public virtual ActionResult FinishPMITask(int taskId)
        {
            if (ClientGlobalModelRepository.FinishPMITask(taskId, IDBContext.Current.UserName))
            {
                var routing = new
                {
                    operationNumber = _globalModelRepository.GetOperationNumberByTaskId(taskId),
                    State = 604,
                    Status = string.Empty
                };

                return RedirectToAction("Index", routing);
            }
            else
            {
                var routing = new
                {
                    id = taskId,
                    State = 101,
                    Status = string.Empty
                };

                return RedirectToAction("Detail", routing);
            }
        }

        public virtual string FinishPMITaskAndSave(
            string taskId,
            BasicPMIWorkflowIds model,
            List<int> itemDeletedList = null)
        {
            model.UserName = IDBContext.Current.UserName;

            if (itemDeletedList != null)
            {
                model.itemDeletedList = itemDeletedList;
            }

            _resultsMatrixModelRepository.SaveCommentsPMIWithTask(model);

            if (ClientGlobalModelRepository
                .FinishPMITask(model.CompleteTaskView.TaskId, IDBContext.Current.UserName))
            {
                return Url
                    .Action("Index",
                        new
                        {
                            operationNumber = _globalModelRepository
                                .GetOperationNumberByTaskId(model.CompleteTaskView.TaskId),
                            State = 604,
                            Status = string.Empty
                        });
            }

            return Url
                .Action("Detail",
                    new
                    {
                        id = model.CompleteTaskView.TaskId,
                        State = 101,
                        Status = string.Empty
                    });
        }

        public virtual ActionResult AdditionalValidators(
            string code,
            bool editable,
            int taskId = 0,
            OperationRelatedModel modelValidation = null,
            int entityId = 0,
            bool isRequest = true)
        {
            var requiredValidators = new List<WorkFlowTaskTypeValidatorModel>();
            var validators = new List<Tuple<int, string, string, string, string>>();
            var validatorResults = new List<Tuple<int, string, string, string, string>>();
            var currentRoles = new List<string>();

            currentRoles.AddRange(IDBContext.Current.Roles);

            if (code.ContainsAny(
                    ClauseConstants.ClauseTypeCL01,
                    ClauseConstants.ClauseTypeCL02,
                    ClauseConstants.EXTENSION_WORKFLOW_TYPE,
                    ClauseConstants.FINAL_STATUS_WORKFLOW_TYPE))
            {
                currentRoles.AddRange(
                    IDBContext.Current.GetGlobalRoles()
                        .Where(rol => !currentRoles.Contains(rol))
                        .ToList());
            }

            if (!currentRoles.Any())
            {
                Exception e = new Exception(
                    string.Format("The user {0} doesn't have roles in the operation {1}",
                    IDBContext.Current.UserName,
                    IDBContext.Current.Operation));

                Logger.GetLogger()
                    .WriteError(
                        "TasksController",
                        "Error when processing Additional Validators",
                        e);

                throw e;
            }

            if (code.Contains("CL"))
            {
                string extensionId;

                try
                {
                    extensionId = string.IsNullOrEmpty(modelValidation.Contracts.First()
                        .Clauses.First().ClauseIndividuals.First().ClauseExtension.First()
                        .ClauseExtensionId.ToString()) ?
                            string.Empty :
                            modelValidation.Contracts.First()
                            .Clauses.First().ClauseIndividuals.First().ClauseExtension.First()
                            .ClauseExtensionId.ToString();
                }
                catch
                {
                    extensionId = string.Empty;
                }

                var currentAmount = modelValidation.Contracts.First().CurrentAprovedAmount;

                ClauseBusinessLogic logic =
                    new ClauseBusinessLogic(_businessRuleService, _clauseService);

                string validatorCandidates = logic.GetUserProfileValidators(
                    _ruleService,
                    modelValidation,
                    code.Replace("WF-CL-00", string.Empty),
                    modelValidation.OperationId,
                    modelValidation.Contracts.First().ContractId,
                    modelValidation.mainOperationNumber,
                    modelValidation.Contracts.First().Clauses.First().ClauseId,
                    modelValidation.Contracts.First().Clauses
                        .First().ClauseIndividuals.First().ClauseIndividualId,
                    extensionId,
                    currentAmount);

                if (!validatorCandidates.Contains("Error"))
                {
                    var ruleEvaluatorServiceResultsarray = validatorCandidates.Split('|');
                    string workflowType;
                    switch (code)
                    {
                        case ClauseConstants.ClauseTypeCL01:
                            workflowType = ClauseConstants.VALIDATION_REVOLVING_FUND;
                            break;
                        case ClauseConstants.ClauseTypeCL02:
                            workflowType = ClauseConstants.ELIGIBILITY_DATE_APPROVAL;
                            break;
                        case ClauseConstants.EXTENSION_WORKFLOW_TYPE:
                            workflowType = ClauseConstants.CLAUSE_EXTENSION_APPROVAL;
                            break;
                        case ClauseConstants.FINAL_STATUS_WORKFLOW_TYPE:
                            workflowType = ClauseConstants.CLAUSE_FINAL_STATUS_VALIDATION;
                            break;
                        default:
                            workflowType = string.Empty;
                            break;
                    }

                    var codeList = ValidatorHelpers
                        .ConvertProfileToCode(ruleEvaluatorServiceResultsarray, workflowType);
                    var validatorsResponse = _validatorService
                        .GetValidatorsByCode(codeList, workflowType);
                    if (validatorsResponse.IsValid && validatorsResponse.Models.Any())
                    {
                        ruleEvaluatorServiceResultsarray = validatorsResponse.Models.ToArray();
                    }

                    var indexVAlidator = 0;

                    foreach (var ruleEvaluatorServiceResult in ruleEvaluatorServiceResultsarray)
                    {
                        indexVAlidator++;

                        var validador =
                            new WorkFlowTaskTypeValidatorModel
                            {
                                WorkflowTaskTypeValidatorId = 0,
                                WorkflowTaskTypeId = 0,
                                WorkFlowTaskValidatorOrder = indexVAlidator,
                                WorkFlowtaskValidatorRole =
                                    ruleEvaluatorServiceResult.Replace("\"", string.Empty),
                                WorkFlowtaskValidatorName = string.Empty,
                                WorkFlowtaskValidatorDescription = "Mandatory Validator",
                                IsValidator = true
                            };

                        requiredValidators.Add(validador);
                    }
                }
            }
            else
            {
                requiredValidators = _globalModelRepository.WorkFlowTaskValidatorList(code, modelValidation).ToList();
            }

            ViewBag.IsRequiredValidator = !requiredValidators.Any();

            var originalRoles = MasterDefinitions.CONVERGENCE_ROLES;
            var additionalValidators = new List<WorkflowInstanceAdditionalValidator>();
            var tasks = new List<WorkflowInstanceTask>();
            var workflowId = 0;

            if (entityId != 0)
            {
                var callTypes = new ReflectionHelper<K2CallType>()
                    .GetFields<K2CallTypeDefinitionAttribute>();

                var entityType = callTypes
                    .FirstOrDefault(ct => ct.Value.Code == code);

                if (entityType.Value != null)
                {
                    var workFlowInstance = _globalModelRepository
                        .WorkflowGetByEntityId(entityType.Value.Description, entityId, WorkflowStatus.Started);

                    if (workFlowInstance != null)
                    {
                        workflowId = workFlowInstance.WorkflowInstanceId;
                    }
                }
            }

            if (workflowId != 0)
            {
                editable = false;
            }

            if (taskId != 0 || workflowId != 0)
            {
                additionalValidators = _globalModelRepository
                    .AdditionalValidatorGet(taskId, workflowId).ToList();

                if (taskId != 0)
                {
                    workflowId = _globalModelRepository.GetDetailByTask(taskId).WorkflowId;
                }

                tasks = _globalModelRepository.TaskGetByWfid(workflowId);
                tasks = tasks.OrderBy(t => t.WorkflowInstanceTaskId).ToList();

                if (tasks.Any())
                {
                    tasks.RemoveAt(0);
                }
            }

            var index = 1;

            foreach (var task in tasks)
            {
                var taskOriginalValidator = _globalModelRepository.GetTaskOriginalValidator(task.WorkflowInstanceTaskId);

                var isAdditional = additionalValidators
                    .Any(req =>
                        req.UserProfile == task.UserProfile);

                var validatorDescription = !isAdditional ?
                    Localization.GetText("Mandatory Validator") :
                    Localization.GetText("Additional Validator");

                var taskStatus = task.Status;

                if (currentRoles.Contains(task.UserProfile) &&
                    string.IsNullOrEmpty(taskStatus))
                {
                    taskStatus = "insert-validators";
                }

                validators.Add(
                    new Tuple<int, string, string, string, string>(
                        index,
                        task.UserProfile,
                        task.UserName,
                        validatorDescription,
                        taskStatus));

                additionalValidators.RemoveAll(add => add.UserProfile == task.UserProfile);

                requiredValidators
                    .RemoveAll(req => req.WorkFlowtaskValidatorRole == task.UserProfile);

                if (taskOriginalValidator != null)
                    requiredValidators.RemoveAll(
                        req => req.WorkFlowtaskValidatorRole == taskOriginalValidator.OriginalUserProfile);

                if (string.IsNullOrEmpty(task.Status))
                {
                    additionalValidators
                        .ForEach(addVal =>
                            {
                                index++;
                                validators.Add(
                                    new Tuple<int, string, string, string, string>(
                                        index,
                                        addVal.UserProfile,
                                        string.Empty,
                                        Localization.GetText("Additional Validator"),
                                        string.Empty));
                            });

                    additionalValidators.Clear();
                }

                index++;
            }

            if (index == 1)
            {
                index--;
            }

            requiredValidators
                .ForEach(
                    addVal =>
                    {
                        index++;
                        validators.Add(
                            new Tuple<int, string, string, string, string>(
                                index,
                                addVal.WorkFlowtaskValidatorRole,
                                string.Empty,
                                Localization.GetText("Required Validator"),
                                string.Empty));
                    });

            for (index = 0; index < validators.Count; index++)
            {
                validatorResults.Add(
                    new Tuple<int, string, string, string, string>(
                        index + 1,
                        validators[index].Item2,
                        validators[index].Item3,
                        validators[index].Item4,
                        validators[index].Item5));
            }

            ViewBag.Validators = validatorResults;
            ViewBag.AvailableRoles = originalRoles
                .Where(role => validators.All(val => val.Item2 != role))
                .ToList();
            ViewBag.Editable = editable;
            ViewBag.CurrentRoles = currentRoles.Aggregate((current, next) => current + ", " + next);

            if (!isRequest && taskId > 0)
            {
                var isLast = validatorResults
                    .Count(v => currentRoles.Contains(v.Item2) &&
                        v.Item4.Equals(Localization.GetText("Mandatory Validator")));

                if (isLast > 0)
                {
                    ViewBag.Editable = false;
                }

                foreach (var validator in validatorResults)
                {
                    if (string.IsNullOrEmpty(validator.Item5))
                    {
                        if (validator.Item4 == "Additional Validator")
                        {
                            ViewBag.Editable = false;
                        }

                        break;
                    }
                }
            }

            return PartialView();
        }

        public virtual ActionResult SupervisionPlanAdditionalValidators(
            string code,
            bool editable,
            int taskId = 0,
            OperationRelatedModel modelValidation = null,
            int entityId = 0,
            bool isRequest = true)
        {
            var list = new List<WorkFlowTaskTypeValidatorModel>();
            var validators = new List<Tuple<int, string, string, string, string>>();
            var validatorResults = new List<Tuple<int, string, string, string, string>>();

            if (code.Contains("CL"))
            {
                string extensionId;

                try
                {
                    extensionId = string.IsNullOrEmpty(modelValidation.Contracts.First()
                        .Clauses.First().ClauseIndividuals.First().ClauseExtension.First()
                        .ClauseExtensionId.ToString()) ?
                            string.Empty :
                            modelValidation.Contracts.First()
                            .Clauses.First().ClauseIndividuals.First().ClauseExtension.First()
                            .ClauseExtensionId.ToString();
                }
                catch
                {
                    extensionId = string.Empty;
                }

                var currentAmount = code.Contains("CL") ?
                    modelValidation.Contracts.First().CurrentAprovedAmount :
                    OperationClauseClient.CalculateCurrentAmount(
                        modelValidation.OperationId, 1);

                ClauseBusinessLogic logic =
                    new ClauseBusinessLogic(_businessRuleService, _clauseService);

                string validatorCandidates = logic.GetUserProfileValidators(
                    _ruleService,
                    modelValidation,
                    code.Replace("WF-CL-00", string.Empty),
                    modelValidation.OperationId,
                    modelValidation.Contracts.First().ContractId,
                    modelValidation.mainOperationNumber,
                    modelValidation.Contracts.First().Clauses.First().ClauseId,
                    modelValidation.Contracts.First().Clauses
                        .First().ClauseIndividuals.First().ClauseIndividualId,
                    extensionId,
                    currentAmount);

                if (!validatorCandidates.Contains("Error"))
                {
                    var ruleEvaluatorServiceResultsarray = validatorCandidates.Split('|');
                    var indexVAlidator = 0;

                    foreach (var ruleEvaluatorServiceResult in ruleEvaluatorServiceResultsarray)
                    {
                        indexVAlidator++;

                        var validador =
                            new WorkFlowTaskTypeValidatorModel
                            {
                                WorkflowTaskTypeValidatorId = 0,
                                WorkflowTaskTypeId = 0,
                                WorkFlowTaskValidatorOrder = indexVAlidator,
                                WorkFlowtaskValidatorRole =
                                    ruleEvaluatorServiceResult.Replace("\"", string.Empty),
                                WorkFlowtaskValidatorName = string.Empty,
                                WorkFlowtaskValidatorDescription = "Mandatory Validator",
                                IsValidator = true
                            };

                        list.Add(validador);
                    }
                }
            }
            else
            {
                list = GlobalModelRepository.WorkFlowTaskValidatorList(code, modelValidation).ToList();
            }

            ViewBag.IsRequiredValidator = !list.Any();

            var requiredValidators = list;
            var originalRoles = MasterDefinitions.CONVERGENCE_ROLES;
            var additionalValidators = new List<WorkflowInstanceAdditionalValidator>();
            var tasks = new List<WorkflowInstanceTask>();
            var workflowId = 0;

            if (entityId != 0)
            {
                var callTypes = new ReflectionHelper<K2CallType>()
                    .GetFields<K2CallTypeDefinitionAttribute>();
                var entityType = callTypes
                    .First(ct => ct.Value.Code == code);
                var workFlowInstance = GlobalModelRepository
                    .WorkflowGetByEntityId(entityType.Value.Description, entityId, "Started");

                if (workFlowInstance != null)
                {
                    workflowId = workFlowInstance.WorkflowInstanceId;
                }
            }

            if (workflowId != 0)
            {
                editable = false;
            }

            if (taskId != 0 || workflowId != 0)
            {
                additionalValidators = GlobalModelRepository
                    .AdditionalValidatorGet(taskId, workflowId).ToList();

                if (taskId != 0)
                {
                    workflowId = GlobalModelRepository.GetDetailByTask(taskId).WorkflowId;
                }

                tasks = GlobalModelRepository.TaskGetByWfid(workflowId);
                tasks = tasks.OrderBy(t => t.WorkflowInstanceTaskId).ToList();

                if (tasks.Any())
                {
                    tasks.RemoveAt(0);
                }
            }

            var index = 1;

            foreach (var task in tasks)
            {
                var isAdditional = additionalValidators
                    .Any(req =>
                        req.UserProfile == task.UserProfile);

                var validatorDescription = !isAdditional ?
                    Localization.GetText("Mandatory Validator") :
                    Localization.GetText("Additional Validator");

                var taskStatus = task.Status;

                if (IDBContext.Current.Roles.Contains(task.UserProfile) &&
                    string.IsNullOrEmpty(taskStatus))
                {
                    taskStatus = "insert-validators";
                }

                validators.Add(
                    new Tuple<int, string, string, string, string>(
                        index,
                        task.UserProfile,
                        task.UserName,
                        validatorDescription,
                        taskStatus));

                additionalValidators.RemoveAll(add => add.UserProfile == task.UserProfile);

                requiredValidators
                    .RemoveAll(req => req.WorkFlowtaskValidatorRole == task.UserProfile);

                if (string.IsNullOrEmpty(task.Status))
                {
                    additionalValidators
                        .ForEach(addVal =>
                        {
                            index++;
                            validators.Add(
                                new Tuple<int, string, string, string, string>(
                                    index,
                                    addVal.UserProfile,
                                    string.Empty,
                                    Localization.GetText("Additional Validator"),
                                    string.Empty));
                        });

                    additionalValidators.Clear();
                }

                index++;
            }

            if (index == 1)
            {
                index--;
            }

            requiredValidators
                .ForEach(
                    addVal =>
                    {
                        index++;
                        validators.Add(
                            new Tuple<int, string, string, string, string>(
                                index,
                                addVal.WorkFlowtaskValidatorRole,
                                string.Empty,
                                Localization.GetText("Required Validator"),
                                string.Empty));
                    });

            for (index = 0; index < validators.Count; index++)
            {
                validatorResults.Add(
                    new Tuple<int, string, string, string, string>(
                        index + 1,
                        validators[index].Item2,
                        validators[index].Item3,
                        validators[index].Item4,
                        validators[index].Item5));
            }

            ViewBag.Validators = validatorResults;

            var availableRoles = new List<SelectListItem>();
            foreach (var availableRole in originalRoles
                .Where(role => validators.All(val => val.Item2 != role))
                .ToList())
            {
                availableRoles.Add(new SelectListItem()
                {
                    Selected = false,
                    Text = availableRole,
                    Value = availableRole,
                    Disabled = false
                });
            }

            ViewBag.AvailableRoles = availableRoles;
            ViewBag.Editable = editable;

            var currentRoles = IDBContext.Current.Roles;

            if (IDBContext.Current.IsLocalEnvironment)
            {
                currentRoles.Add(Role.RM_ADMINISTRATOR);
                currentRoles.Add(Role.TEAM_LEADER);
                currentRoles.Add(Role.DIVISION_CHIEF);
                currentRoles.Add(Role.CHIEF_OF_OPERATIONS);
                currentRoles.Add(Role.REPRESENTATIVE);
            }

            if (!currentRoles.Any())
            {
                Exception e = new Exception(
                    string.Format("The user {0} doesn't have roles in the operation {1}",
                    IDBContext.Current.UserName,
                    IDBContext.Current.Operation));

                Logger.GetLogger()
                    .WriteError(
                        "TasksController",
                        "Error when processing Additional Validators",
                        e);

                throw e;
            }

            ViewBag.CurrentRoles = currentRoles.Aggregate((current, next) => current + ", " + next);

            if (!isRequest && taskId > 0)
            {
                var isLast = validatorResults
                    .Count(v => currentRoles.Contains(v.Item2) &&
                        v.Item4.Equals(Localization.GetText("Mandatory Validator")));

                if (isLast > 0)
                {
                    ViewBag.Editable = false;
                }

                foreach (var validator in validatorResults)
                {
                    if (string.IsNullOrEmpty(validator.Item5))
                    {
                        if (validator.Item4 == "Additional Validator")
                        {
                            ViewBag.Editable = false;
                        }

                        break;
                    }
                }
            }

            return PartialView();
        }

        public virtual ActionResult TaskInnerDetail(
            CompleteTaskViewModel model,
            bool edit,
            bool prevVersion = true)
        {
            ViewBag.CurrentUser = IDBContext.Current.UserName;

            if (model.Code == WorkflowCodes.WF_PMI_002.GetStringValue() ||
                model.Code == WorkflowCodes.WF_PMI_003.GetStringValue())
            {
                BasicPMIWorkflowIds ModelStarUp = null;

                ModelStarUp = _validationPMIDetails
                    .GetBasicPartialsDataPMI(model.OperationNumber, model.TaskId);

                ModelStarUp.UserName = IDBContext.Current.UserName;

                if (_globalModelRepository.TaskGetTypeTask(model.TaskId) == K2PMIFinishState)
                {
                    if (model.WorkflowId == (int)K2CallType.September)
                    {
                        ViewBag.Description = Localization.GetText(
                            "PMI.Message.SeptemberCycle.ApplyModificationsAndRequestAgain");

                        ViewBag.Workflow = K2CallType.September;
                    }
                    else
                    {
                        ViewBag.Description =
                            Localization.GetText(@"Please apply modifications on the Results Matrix" +
                                " or the Startup Plan as required by the Chief of Operations, " +
                                "the Division Chief or the Country Representative. " +
                                "Once the information has been corrected, send again your request " +
                                "for the Startup Plan validation.");

                        ViewBag.Workflow = K2CallType.StartUpPlanValidation;
                    }
                }
                else
                {
                    if (model.UserProfile == "Chief of Operations")
                    {
                        if (model.WorkflowId == (int)K2CallType.September)
                        {
                            ViewBag.Description =
                                Localization.GetText(@"Please validate or reject the " +
                                    "PMR September cycle, the operation’s overall risk and any " +
                                    "mitigation measures proposed. Also note that where necessary," +
                                    " you may also add comments to your validation or rejection.");

                            ViewBag.Workflow = K2CallType.September;
                        }
                        else
                        {
                            ViewBag.Description =
                                Localization.GetText(@"Please review the structure of the " +
                                    "Results Matrix and progress reported.  " +
                                    "Where applicable, please also review the Plan at Startup.  " +
                                    "Validate if the information is correct and reject if some " +
                                    "data needs to be corrected by the Team Leader.  " +
                                    "In the case that you reject, comments must be provided " +
                                    "to explain what information should be revised.");

                            ViewBag.Workflow = K2CallType.StartUpPlanValidation;
                        }
                    }
                    else if (model.UserProfile == "Division Chief")
                    {
                        if (model.WorkflowId == (int)K2CallType.September)
                        {
                            ViewBag.Description =
                                Localization.GetText(@"Please validate or reject the PMR September" +
                                    "cycle, the operation’s overall risk and any mitigation " +
                                    "measures proposed. Also note that where necessary, " +
                                    "you may also add comments to your validation or rejection.");

                            ViewBag.Workflow = K2CallType.September;
                        }
                        else
                        {
                            ViewBag.Description =
                                Localization.GetText(@"Please review the structure of the " +
                                    "Results Matrix and the Plan at Startup. " +
                                    "Validate if the information is correct and reject " +
                                    "if some data needs to be corrected by the Team ");

                            ViewBag.Workflow = K2CallType.StartUpPlanValidation;
                        }
                    }
                    else if (model.UserProfile == "Representative")
                    {
                        if (model.WorkflowId == (int)K2CallType.September)
                        {
                            ViewBag.Description =
                                Localization.GetText(@"Please validate or reject the " +
                                    "PMR September cycle, the operation’s overall risk and any " +
                                    "mitigation measures proposed. Also note that where necessary," +
                                    " you may also add comments to your validation or rejection.");

                            ViewBag.Workflow = K2CallType.September;
                        }
                        else
                        {
                            ViewBag.Description =
                                Localization.GetText(@"Please review the structure of the " +
                                    "Results Matrix and progress reported.  " +
                                    "Where applicable, please also review the Plan at Startup.  " +
                                    "Validate if the information is correct and reject if " +
                                    "some data needs to be corrected by the Team Leader.  " +
                                    "In the case that you reject, comments must be provided " +
                                    "to explain what information should be revised.");

                            ViewBag.Workflow = K2CallType.StartUpPlanValidation;
                        }
                    }
                    else
                    {
                        if (model.WorkflowId == (int)K2CallType.September)
                        {
                            ViewBag.Description = string.Empty;
                            ViewBag.Workflow = K2CallType.September;
                        }
                        else
                        {
                            ViewBag.Description = string.Empty;
                            ViewBag.Workflow = K2CallType.StartUpPlanValidation;
                        }
                    }
                }

                ModelStarUp.CompleteTaskView = model;

                if (edit == false)
                {
                    return View("PMI002PMI003ContentStarupOrSeptember", ModelStarUp);
                }

                return View("PMI002PMI003ContentStarupOrSeptemberEdit", ModelStarUp);
            }

            if (model.Code == WorkflowCodes.WF_PMI_001.GetStringValue())
            {
                var taskMarch = _globalModelRepository.GetTasks(model.WorkflowInstanceId);

                if (taskMarch.First().clasification != NOT_APPLICABLE_CLASSIFICATION)
                {
                    var ListClass = _masterDataModel
                        .GetMasterDataModels("PMI_CLASSIFICATION");
                    int lstClasificationIndex = ListClass.FindIndex(cl => cl.NameEn == NOT_APPLICABLE_CLASSIFICATION);

                    if (lstClasificationIndex > -1)
                    {
                        ListClass.RemoveAt(lstClasificationIndex);
                    }

                    ViewBag.ClassificationList = ListClass;
                }
                else
                {
                    ViewBag.ClassificationList = _masterDataModel
                        .GetMasterDataModels("PMI_CLASSIFICATION");
                }

                Session["OperationContext"] = _validationPMIDetails
                    .LoadOperationContext(model.OperationNumber, prevVersion, false);
                var _modelMarch = _validationPMIDetails
                    .GetBasicPartialsDataPMI(model.OperationNumber, model.TaskId);

                ViewBag.tasks = taskMarch;

                ViewBag.taskViewModel = model;

                var previousTaskViewModel = _globalModelRepository
                    .GetPreviousWorkflowInstanceTask(model.TaskId);

                _modelMarch.UserName = IDBContext.Current.UserName;

                if (_globalModelRepository.TaskGetTypeTask(model.TaskId) == K2PMIFinishState)
                {
                    ViewBag.Description =
                        Localization.GetText(@"Please apply modifications as required by the " +
                            "Chief of Operations, the Division Chief or the Country Representative." +
                            " Once the information has been corrected, " +
                            "send again your request for the Classification validation.");
                    ViewBag.Role = string.Empty;
                    _modelMarch.PreviousClasification = previousTaskViewModel.clasification;
                }
                else
                {
                    if (model.UserProfile == "Representative")
                    {
                        ViewBag.Description =
                            Localization.GetText(@"Please review the structure of the " +
                                "Results Matrix and progress reported.  " +
                                "Where applicable, please also review the Plan at Startup.  " +
                                "Validate if the information is correct and reject if " +
                                "some data needs to be corrected by the Team Leader.  " +
                                "In the case that you reject, comments must be provided to " +
                                "explain what information should be revised.");
                        ViewBag.Role = "Representative";
                        _modelMarch.PreviousClasification = previousTaskViewModel.clasification;
                    }
                    else if (model.UserProfile == "Chief of Operations")
                    {
                        ViewBag.Description =
                            Localization.GetText(@"Please review the structure of the " +
                                "Results Matrix and progress reported.  " +
                                "Where applicable, please also review the Plan at Startup.  " +
                                "Validate if the information is correct and reject if " +
                                "some data needs to be corrected by the Team Leader.  " +
                                "In the case that you reject, comments must be provided " +
                                "to explain what information should be revised.");
                        ViewBag.Role = "Chief of Operations";
                        _modelMarch.PreviousClasification = previousTaskViewModel.clasification;
                    }
                    else if (IDBContext.Current.Roles.Contains("Division Chief"))
                    {
                        ViewBag.Description =
                            Localization.GetText(@"Please review the Progress Monitoring Indicators," +
                                "the risk’s mitigation measures proposed and the overall operation " +
                                "risk. Validate if the information is correct and reject " +
                                "if some data needs to be corrected by the Team Leader. " +
                                "In case you validate, please explain if you agree/disagree " +
                                "with the Performance Classification proposed by the " +
                                "Chief of Operations. In case you reject, comments must be " +
                                "provided to clarify what information should be revised.");
                        ViewBag.Role = string.Empty;
                        _modelMarch.PreviousClasification = previousTaskViewModel.clasification;
                    }
                    else
                    {
                        ViewBag.Description = string.Empty;
                        ViewBag.Role = string.Empty;
                        _modelMarch.PreviousClasification = previousTaskViewModel.clasification;
                    }
                }

                ViewBag.Workflow = K2CallType.March;

                _modelMarch.CompleteTaskView = model;
                _modelMarch.Clasification = previousTaskViewModel.currentClasification;

                if (edit == false)
                {
                    return View("PMI001ContentMarch", _modelMarch);
                }

                return View("PMI001ContentMarchEdit", _modelMarch);
            }

            if (model.Code == WorkflowCodes.WF_CL_004.GetStringValue())
            {
                if (model.EntityType == AgreementsAndConditionsConstants.WF_ENTITY_TYPE_CONDITION_INDIVIVDUAL)
                {
                    var taskModel = _agreementAndConditionService.GetTaskModel(model);

                    ViewBag.languages = _masterDataModel.GetMasterDataModels("Language");

                    ViewBag.ListValidationStage = _masterDataModel
                        .GetMasterDataModels(ClauseConstants.VALIDATION_STAGE);

                    taskModel.Model.CompleteTaskViewModel = model;

                    if (!edit)
                        return View("DetailsConditionApprovalFinalView", taskModel.Model);

                    taskModel.Model.IsModeEditTask = true;

                    return View("DetailsConditionApprovalFinalViewEdit", taskModel.Model);
                }

                var clauseId = _globalModelRepository
                    .GetClauseIdByClauseIndividual(model.IdentityId);
                var operationId = _globalModelRepository
                    .GetOperationIdByOperationNumber(model.OperationNumber);
                var mainOperationNumber = _globalModelRepository
                    .GetParentOperation(model.OperationNumber);
                var clauseIndividualId = model.IdentityId;
                var contractId = _globalModelRepository
                    .GetFirstOrDefaultContractByClauseId(clauseId);

                ViewBag.languages = _masterDataModel
                    .GetMasterDataModels("Language");

                var operationModel = _operationClause
                    .GetClauseContractByOperation(
                        operationId,
                        contractId,
                        clauseId,
                        clauseIndividualId,
                        mainOperationNumber,
                        false);

                ViewBag.ListValidationStage = _masterDataModel
                    .GetMasterDataModels("VALIDATION_STAGE");

                var ModelOperation = operationModel;

                operationModel.CompleteTaskView = model;

                if (!edit)
                {
                    return View("DetailsClauseApprovalFinalView", operationModel);
                }

                return View("DetailsClauseApprovalFinalViewEdit", operationModel);
            }

            if (model.Code == WorkflowCodes.WF_CL_003.GetStringValue())
            {
                if (model.EntityType == AgreementsAndConditionsConstants.WF_ENTITY_TYPE_CONDITION_EXTENSION)
                {
                    var taskModel = _agreementAndConditionService.GetTaskModel(model);

                    taskModel.Model.CompleteTaskViewModel = model;

                    if (!edit)
                        return View("DetailsConditionExtensionApprovalFinalView", taskModel.Model);

                    taskModel.Model.IsModeEditTask = true;

                    return View("DetailsConditionExtensionApprovalFinalViewEdit", taskModel.Model);
                }

                var operationIdClauseExtension = _globalModelRepository
                    .GetOperationIdByOperationNumber(model.OperationNumber);

                var mainOperationNumberClauseExtension = _globalModelRepository
                    .GetParentOperation(model.OperationNumber);

                var clauseExtensionId = model.IdentityId;

                var clauseIndividualIdClauseExtension = _globalModelRepository
                    .GetClauseIndividualByClauseExtension(clauseExtensionId);

                var clauseIdClauseExtension = _globalModelRepository
                    .GetClauseIdByClauseIndividual(clauseIndividualIdClauseExtension);

                var contractIdClauseExtension = _globalModelRepository
                    .GetFirstOrDefaultContractByClauseId(clauseIdClauseExtension);

                var operationModelClauseExtension = _operationClause
                    .GetClauseExtensionContractByOperation(operationIdClauseExtension,
                        contractIdClauseExtension,
                        clauseIdClauseExtension,
                        clauseIndividualIdClauseExtension,
                        clauseExtensionId,
                        mainOperationNumberClauseExtension);

                ViewBag.ListValidationStage = _masterDataModel
                    .GetMasterDataModels("VALIDATION_STAGE");

                var ModelOperationExt = operationModelClauseExtension;

                operationModelClauseExtension.CompleteTaskView = model;

                if (!edit)
                {
                    return
                        View(
                            "DetailsClauseExtensionApprovalFinalView",
                            operationModelClauseExtension);
                }

                return
                    View(
                        "DetailsClauseExtensionApprovalFinalViewEdit",
                        operationModelClauseExtension);
            }

            if (model.Code == WorkflowCodes.WF_CL_002.GetStringValue())
            {
                Logger.GetLogger()
                    .WriteMessage("MFH - TasksController",
                        string.Format(
                            "'TaskInnerDetails' method: entering switch for WF-CL-002, " +
                            "parameter model.OperationNumber= {0}, contractIdContract= {1}",
                            model.OperationNumber,
                            model.IdentityId));

                var operationIdContract = _globalModelRepository
                    .GetOperationIdByOperationNumber(model.OperationNumber);

                var mainOperationNumberContract = _globalModelRepository
                    .GetParentOperation(model.OperationNumber);

                var contractIdContract = model.IdentityId;

                var operationModelContract = _operationClause
                    .GetContractByOperation(
                        operationIdContract,
                        contractIdContract,
                        mainOperationNumberContract);

                var ModelOperationEle = operationModelContract;

                operationModelContract.CompleteTaskView = model;

                if (!edit)
                {
                    return View("DetailsEligibilityApprovalFinalView", operationModelContract);
                }

                return View("DetailsEligibilityApprovalFinalViewEdit", operationModelContract);
            }

            if (model.Code == WorkflowCodes.WF_CL_001.GetStringValue())
            {
                var operationIdRevolvingFund = _globalModelRepository
                    .GetOperationIdByOperationNumber(model.OperationNumber);

                var mainOperationNumberRevolvingFund = _globalModelRepository
                    .GetParentOperation(model.OperationNumber);

                var ContractIdRevolvingFund = _globalModelRepository
                    .GetContractIdByRevolvingFount(model.IdentityId);

                var operationModelRevolvingFund = _operationClause
                    .GetContractByOperation(
                        operationIdRevolvingFund,
                        ContractIdRevolvingFund,
                        mainOperationNumberRevolvingFund);

                var revolvingFundModel = _operationClause
                    .GetRevolvingFundById(model.IdentityId);

                operationModelRevolvingFund.Contracts[0].RevolvingFund.Add(revolvingFundModel);

                ViewBag.ListValidationStage = _masterDataModel
                    .GetMasterDataModels("VALIDATION_STAGE");

                var ModelOperationRev = operationModelRevolvingFund;

                ViewBag.ListEntityValidators = _globalModelRepository
                    .WorkFlowTaskValidatorList("WF-CL-001", ModelOperationRev);

                operationModelRevolvingFund.CompleteTaskView = model;

                if (!edit)
                {
                    return
                        View("DetailsRevolvingFundApprovalFinalView", operationModelRevolvingFund);
                }

                return
                    View("DetailsRevolvingFundApprovalFinalViewEdit", operationModelRevolvingFund);
            }

            if (model.Code == WorkflowCodes.WF_SP_001.GetStringValue())
            {
                var UserCommentsModel = _supervisionPlanModelRepository
                    .GetSupervisionPlanVersionComments(model.IdentityId);

                ViewBag.ActualSP = model.IdentityId;
                ViewBag.ActualTask = model.TaskId;
                ViewBag.TaskStatus = model.TaskStatus;
                ViewBag.CurrentUser = IDBContext.Current.UserName;
                ViewBag.ActualTask = model.TaskId;

                var taskModelSupervisionPlan01 = model.ConvertToTaskDataModel();
                var routingDataSupervisionPlan01 = K2Helper.GetRoutingDataForWorkflow(taskModelSupervisionPlan01);
                taskModelSupervisionPlan01.AreaName = routingDataSupervisionPlan01.Item1;
                taskModelSupervisionPlan01.Controller = routingDataSupervisionPlan01.Item2;
                taskModelSupervisionPlan01.ActionMethod = routingDataSupervisionPlan01.Item3;

                return View("K2Redirect", taskModelSupervisionPlan01);
            }

            if (model.Code == WorkflowCodes.WF_PMI_004.GetStringValue())
            {
                var details = _validationPMIDetails.GetBasicPartialsData(
                    IDBContext.Current.Operation, IDBContext.Current.UserName);

                var pmrCycleCode = _globalModelRepository.GetPMRCycleCode(details.PMRCycleId);

                var task = _globalModelRepository.GetTasks(model.WorkflowInstanceId);

                if (pmrCycleCode.ToUpper().Equals("M") && task.First().clasification != NOT_APPLICABLE_CLASSIFICATION)
                {
                    var listClass = _masterDataModel
                        .GetMasterDataModels("PMI_CLASSIFICATION");

                    listClass.RemoveAt(3);

                    ViewBag.ClassificationList = listClass;
                }
                else
                {
                    ViewBag.ClassificationList = _masterDataModel
                        .GetMasterDataModels("PMI_CLASSIFICATION");
                }

                Session["OperationContext"] = _validationPMIDetails
                    .LoadOperationContext(model.OperationNumber, prevVersion, false);

                var _modelPMR = _validationPMIDetails
                    .GetBasicPartialsDataPMI(model.OperationNumber, model.TaskId);

                ViewBag.tasks = task;

                ViewBag.taskViewModel = model;

                var previousTask = _globalModelRepository
                    .GetPreviousWorkflowInstanceTask(model.TaskId);

                _modelPMR.UserName = IDBContext.Current.UserName;

                if (_globalModelRepository.TaskGetTypeTask(model.TaskId) == K2PMIFinishState)
                {
                    ViewBag.Description =
                        Localization.GetText(
                            @"Please apply modifications as required by the Chief of Operations, " +
                            "the Division Chief or the Country Representative. Once the " +
                            "information has been corrected, send again your request " +
                            "for the Classification validation.");
                    ViewBag.Role = string.Empty;
                    _modelPMR.PreviousClasification = previousTask.clasification;
                }
                else
                {
                    _modelPMR.PreviousClasification = previousTask.clasification;

                    if (model.UserProfile == "Representative")
                    {
                        ViewBag.Description =
                            Localization.GetText(@"Please review the structure of the " +
                                "Results Matrix and progress reported.  Where applicable, " +
                                "please also review the Plan at Startup.  " +
                                "Validate if the information is correct and reject " +
                                "if some data needs to be corrected by the Team Leader.  " +
                                "In the case that you reject, comments must be provided to explain" +
                                " what information should be revised.");
                        ViewBag.Role = "Representative";
                    }
                    else if (model.UserProfile == "Chief of Operations")
                    {
                        ViewBag.Description =
                            Localization.GetText(@"Please review the structure of the " +
                                "Results Matrix and progress reported.  Where applicable, " +
                                "please also review the Plan at Startup.  " +
                                "Validate if the information is correct and reject " +
                                "if some data needs to be corrected by the Team Leader.  " +
                                "In the case that you reject, comments must be provided to explain" +
                                " what information should be revised.");
                        ViewBag.Role = "Chief of Operations";
                    }
                    else if (IDBContext.Current.Roles.Contains("Division Chief"))
                    {
                        ViewBag.Description =
                            Localization.GetText(@"Please review the Progress Monitoring " +
                                "Indicators, the risk’s mitigation measures proposed and the " +
                                "overall operation risk. Validate if the information is correct" +
                                " and reject if some data needs to be corrected by the Team Leader." +
                                " In case you validate, please explain if you agree/disagree " +
                                "with the Performance Classification proposed by the " +
                                "Chief of Operations. In case you reject, comments must be " +
                                "provided to clarify what information should be revised.");
                        ViewBag.Role = string.Empty;
                    }
                    else
                    {
                        ViewBag.Description = string.Empty;
                        ViewBag.Role = string.Empty;
                    }
                }

                ViewBag.Workflow = K2CallType.PMRCycleStartup;
                _modelPMR.CompleteTaskView = model;
                _modelPMR.Clasification = previousTask.currentClasification;

                if (pmrCycleCode.ToUpper().Equals("M"))
                {
                    if (edit == false)
                    {
                        return View("PMI001ContentMarch", _modelPMR);
                    }

                    return View("PMI001ContentMarchEdit", _modelPMR);
                }

                if (edit == false)
                {
                    return View("PMI004", _modelPMR);
                }

                return View("PMI004Edit", _modelPMR);
            }

            if (model.Code == WorkflowCodes.WF_CA_001.GetStringValue())
            {
                var taskModelCreation001 = model.ConvertToTaskDataModel();
                var routingDataCreation001 = K2Helper.GetRoutingDataForWorkflow(taskModelCreation001);
                taskModelCreation001.AreaName = routingDataCreation001.Item1;
                taskModelCreation001.Controller = routingDataCreation001.Item2;
                taskModelCreation001.ActionMethod = routingDataCreation001.Item3;

                return View("K2Redirect", taskModelCreation001);
            }

            if (model.Code == WorkflowCodes.WF_CA_002.GetStringValue())
            {
                var taskModelCreation002 = model.ConvertToTaskDataModel();
                var routingDataCreation002 = K2Helper.GetRoutingDataForWorkflow(taskModelCreation002);
                taskModelCreation002.AreaName = routingDataCreation002.Item1;
                taskModelCreation002.Controller = routingDataCreation002.Item2;
                taskModelCreation002.ActionMethod = routingDataCreation002.Item3;

                return View("K2Redirect", taskModelCreation002);
            }

            if (model.Code == WorkflowCodes.WF_LC_001.GetStringValue())
            {
                var taskModelLC001 = model.ConvertToTaskDataModel();
                var routingDataLC001 = K2Helper.GetRoutingDataForWorkflow(taskModelLC001);
                taskModelLC001.AreaName = routingDataLC001.Item1;
                taskModelLC001.Controller = routingDataLC001.Item2;
                taskModelLC001.ActionMethod = routingDataLC001.Item3;

                return View("K2Redirect", taskModelLC001);
            }

            if (model.Code == WorkflowCodes.WF_CA_003.GetStringValue())
            {
                var taskModelCreation003 = model.ConvertToTaskDataModel();
                var routingDataCreation003 = K2Helper.GetRoutingDataForWorkflow(taskModelCreation003);
                taskModelCreation003.AreaName = routingDataCreation003.Item1;
                taskModelCreation003.Controller = routingDataCreation003.Item2;
                taskModelCreation003.ActionMethod = routingDataCreation003.Item3;

                return View("K2Redirect", taskModelCreation003);
            }

            if (model.Code == WorkflowCodes.WF_VER_001.GetStringValue())
            {
                var taskModelVER001 = model.ConvertToTaskDataModel();
                var routingDataVER001 = K2Helper.GetRoutingDataForWorkflow(taskModelVER001);
                taskModelVER001.AreaName = routingDataVER001.Item1;
                taskModelVER001.Controller = routingDataVER001.Item2;
                taskModelVER001.ActionMethod = routingDataVER001.Item3;

                return View("K2Redirect", taskModelVER001);
            }

            if (model.Code == WorkflowCodes.WF_INS_001.GetStringValue())
            {
                var taskModelInstitution001 = model.ConvertToTaskDataModel();
                var routingDataInstitution001 = K2Helper.GetRoutingDataForWorkflow(taskModelInstitution001);
                taskModelInstitution001.AreaName = routingDataInstitution001.Item1;
                taskModelInstitution001.Controller = routingDataInstitution001.Item2;
                taskModelInstitution001.ActionMethod = routingDataInstitution001.Item3;

                return View("K2Redirect", taskModelInstitution001);
            }

            if (model.Code == WorkflowCodes.WF_MIS_001.GetStringValue())
            {
                var taskModelMission001 = model.ConvertToTaskDataModel();
                var routingDataMission001 = K2Helper.GetRoutingDataForWorkflow(taskModelMission001);
                taskModelMission001.AreaName = routingDataMission001.Item1;
                taskModelMission001.Controller = routingDataMission001.Item2;
                taskModelMission001.ActionMethod = routingDataMission001.Item3;

                return View("K2Redirect", taskModelMission001);
            }

            if (model.Code == WorkflowCodes.WF_MIS_002.GetStringValue())
            {
                var taskModelMission002 = model.ConvertToTaskDataModel();
                var routingDataMission002 = K2Helper.GetRoutingDataForWorkflow(taskModelMission002);
                taskModelMission002.AreaName = routingDataMission002.Item1;
                taskModelMission002.Controller = routingDataMission002.Item2;
                taskModelMission002.ActionMethod = routingDataMission002.Item3;

                return View("K2Redirect", taskModelMission002);
            }

            if (model.Code == WorkflowCodes.WF_MIS_003.GetStringValue())
            {
                var taskModelMission003 = model.ConvertToTaskDataModel();
                var routingDataMission003 = K2Helper.GetRoutingDataForWorkflow(taskModelMission003);
                taskModelMission003.AreaName = routingDataMission003.Item1;
                taskModelMission003.Controller = routingDataMission003.Item2;
                taskModelMission003.ActionMethod = routingDataMission003.Item3;

                return View("K2Redirect", taskModelMission003);
            }

            if (model.Code == WorkflowCodes.WF_ECO_001.GetStringValue())
            {
                var taskModelECO001 = model.ConvertToTaskDataModel();
                var routingDataECO001 = K2Helper.GetRoutingDataForWorkflow(taskModelECO001);
                taskModelECO001.AreaName = routingDataECO001.Item1;
                taskModelECO001.Controller = routingDataECO001.Item2;
                taskModelECO001.ActionMethod = routingDataECO001.Item3;

                return View("K2Redirect", taskModelECO001);
            }

            return GetDefaultView(model);
        }

        public virtual ActionResult GetDefaultView(CompleteTaskViewModel model)
        {
            var taskModel = model.ConvertToTaskDataModel();
            var routingData = K2Helper.GetRoutingDataForWorkflow(taskModel);
            taskModel.AreaName = routingData.Item1;
            taskModel.Controller = routingData.Item2;
            taskModel.ActionMethod = routingData.Item3;

            int fundId;

            if (taskModel.Parameters != null &&
                taskModel.Parameters.ContainsKey("FundId") &&
                int.TryParse(taskModel.Parameters["FundId"].ToString(), out fundId))
            {
                K2Helper.StoreK2Data(taskModel, taskModel.OperationNumber, fundId);
            }
            else
            {
                K2Helper.StoreK2Data(taskModel, taskModel.OperationNumber, null);
            }

            return View("K2Redirect", taskModel);
        }

        public virtual ActionResult PartialSupervisionPlanDetail(
            int TaskEntityId,
            SupervisionPlanView view = SupervisionPlanView.CriticalProducts)
        {
            var year = _globalModelRepository.GetYearByTaskEntityId(TaskEntityId);

            ViewBag.Year = year;

            var operationNumber = _globalModelRepository
                .GetSupervisionPlanOperationNumberByTaskEntityId(TaskEntityId);

            ViewBag.EditMode = false;
            ViewBag.EditableView = view;

            if (!year.HasValue)
            {
                year = DateTime.Now.Year;
            }

            var supervisionPlans = PrepareSummaryView(null, operationNumber, year.Value);

            if (view == SupervisionPlanView.Budget)
            {
                LocalizeAndCompleteBudgetMatrix(supervisionPlans);
            }

            return View("DetailsSupervisionPlanPartial", supervisionPlans);
        }

        public virtual ActionResult SuperVisionPlanTaskDetails(TaskDataModel model)
        {
            var year = GlobalModelRepository.GetYearByTaskEntityId(model.EntityId);
            var UserCommentsModel = _supervisionPlanModelRepository.GetSupervisionPlanVersionComments(model.EntityId);

            var modelTaskView = new SupervisionPlanWorkflowViewModel
            {
                UserComments = UserCommentsModel,
                TaskDataModel = model,
                UserName = IDBContext.Current.UserName,
            };

            ViewBag.Year = year;

            var stages = new List<string> { "SP_DRAFT", "SP_MOD" };
            ViewBag.IsInDraftOrModified = true;

            var operationNumber = GlobalModelRepository
                .GetSupervisionPlanOperationNumberByTaskEntityId(model.EntityId);

            ViewBag.EditMode = true;
            ViewBag.OperationNumber = operationNumber;
            ViewBag.Year = year;
            ViewBag.SupervisionPlanId = model.EntityId;
            ViewBag.ActualTask = model.TaskId;

            if (!year.HasValue)
            {
                year = DateTime.Now.Year;
            }

            return View("~/Areas/SupervisionPlan/Views/SupervisionPlan/SupervisionPlanWorkflow.cshtml", modelTaskView);
        }

        [HttpPost]
        public virtual ActionResult SaveSupervisionPlanVersionComments(
            string ModelUserComment,
            int CurrentSP,
            int Task,
            string ActionType = "",
            string taskStatus = "",
            string itemDeletedList = null,
            string additionalValidators = "",
            string newComments = "")
        {
            List<IDB.MW.Domain.Models.Architecture.Clauses.UserCommentModel> model = new
                JavaScriptSerializer().Deserialize<List<IDB.MW.Domain.Models.Architecture.Clauses.UserCommentModel>>(ModelUserComment);

            foreach (var newComment in newComments.Split('|').ToList())
            {
                if (!string.IsNullOrEmpty(newComment))
                    model.Add(new IDB.MW.Domain.Models.Architecture.Clauses.UserCommentModel()
                    {
                        UserCommentId = 0,
                        Text = newComment,
                        CreatedBy = IDBContext.Current.UserName,
                        ModifiedBy = IDBContext.Current.UserName,
                        Created = DateTime.Now,
                        Modified = DateTime.Now
                    });
            }

            var itemsToBeDeleted = new List<int>();

            foreach (var itemToBeDeleted in itemDeletedList.Split('|').ToList())
            {
                if (!string.IsNullOrEmpty(itemToBeDeleted))
                {
                    itemsToBeDeleted.Add(Convert.ToInt32(itemToBeDeleted));
                }
            }

            if (model != null)
            {
                _supervisionPlanModelRepository.SaveSupervisionPlanVersionComments(
                    model,
                    CurrentSP,
                    IDBContext.Current.UserName,
                    itemsToBeDeleted);
            }

            if (ActionType == "RejectTask")
            {
                var routing = new
                {
                    taskStatus,
                    taskId = Task,
                    additionalValidators
                };

                return RedirectToAction("RejectTask", routing);
            }

            if (ActionType == "ValidateTask")
            {
                var routing = new
                {
                    taskStatus,
                    taskId = Task,
                    additionalValidators
                };

                return RedirectToAction("ValidateTask", routing);
            }
            else
            {
                AdditionalValidatorsSave(additionalValidators, Task);

                var opNumber = IDBContext.Current.Operation;

                var rout =
                    new
                    {
                        operationNumber = opNumber
                    };

                return RedirectToAction("Index", "Tasks", rout);
            }
        }

        [HttpPost]
        public virtual ActionResult SaveExtensionClauseTask(
            OperationRelatedModel ModelOperationRelate)
        {
            var modelClauseIndividualExtension =
                _operationClause
                .GetExtensionById(
                    ModelOperationRelate.Contracts[0].Clauses[0]
                    .ClauseIndividuals[0].ClauseExtension[0].ClauseExtensionId);

            modelClauseIndividualExtension.UserComments.Clear();

            var additionalValidators = Request["validator_list_additional_list"];

            foreach (var Comments in
                ModelOperationRelate.Contracts[0].Clauses[0].ClauseIndividuals[0]
                .ClauseExtension[0].UserComments.Where(x => x.UserCommentId == -1))
            {
                Comments.ModifiedBy = IDBContext.Current.UserName;
            }

            modelClauseIndividualExtension.UserComments
                .AddRange(
                    ModelOperationRelate.Contracts[0].Clauses[0].ClauseIndividuals[0]
                    .ClauseExtension[0].UserComments);

            _operationClause
                .SaveOrUpdateExtensionModel(
                    modelClauseIndividualExtension,
                    IDBContext.Current.UserName);

            ModelOperationRelate.Contracts[0].UpdateLMSStatus = 0;

            if (ModelOperationRelate.ActionType == "RejectTask")
            {
                var routing =
                    new
                    {
                        taskStatus = ModelOperationRelate.CompleteTaskView.TaskStatus,
                        taskId = ModelOperationRelate.CompleteTaskView.TaskId,
                        classification = string.Empty,
                        additionalValidators
                    };

                return RedirectToAction("RejectTask", routing);
            }

            if (ModelOperationRelate.ActionType == "ValidateTask")
            {
                var routing =
                    new
                    {
                        taskStatus = ModelOperationRelate.CompleteTaskView.TaskStatus,
                        taskId = ModelOperationRelate.CompleteTaskView.TaskId,
                        classification = string.Empty,
                        additionalValidators
                    };

                return RedirectToAction("ValidateTask", routing);
            }
            else
            {
                var rout =
                    new
                    {
                        operationNumber = ModelOperationRelate.OperationNumber
                    };

                return RedirectToAction("Index", "Tasks", rout);
            }
        }

        [HttpPost]
        public virtual ActionResult SaveFinalAprovalClauseTask(
            OperationRelatedModel ModelOperationRelate)
        {
            var modelClauseIndividual =
                _operationClause
                .GetClauseIndividualsById(
                    ModelOperationRelate.Contracts[0].Clauses[0]
                    .ClauseIndividuals[0].ClauseIndividualId);

            modelClauseIndividual.UserComments.Clear();

            var additionalValidators = Request["validator_list_additional_list"];

            foreach (var Comments in
                ModelOperationRelate.Contracts[0]
                .Clauses[0].ClauseIndividuals[0].UserComments.Where(x => x.UserCommentId == -1))
            {
                Comments.ModifiedBy = IDBContext.Current.UserName;
            }

            modelClauseIndividual.UserComments
                .AddRange(
                    ModelOperationRelate.Contracts[0].Clauses[0].ClauseIndividuals[0].UserComments);

            _operationClause
                .SaveOrUpdateClauseIndividual(modelClauseIndividual, IDBContext.Current.UserName);

            if (ModelOperationRelate.ActionType == "RejectTask")
            {
                var routing =
                    new
                    {
                        taskStatus = ModelOperationRelate.CompleteTaskView.TaskStatus,
                        taskId = ModelOperationRelate.CompleteTaskView.TaskId,
                        classification = string.Empty
                    };

                return RedirectToAction("RejectTask", routing);
            }

            if (ModelOperationRelate.ActionType == "ValidateTask")
            {
                var routing =
                    new
                    {
                        taskStatus = ModelOperationRelate.CompleteTaskView.TaskStatus,
                        taskId = ModelOperationRelate.CompleteTaskView.TaskId,
                        classification = string.Empty,
                        additionalValidators
                    };

                return RedirectToAction("ValidateTask", routing);
            }
            else
            {
                var rout =
                    new
                    {
                        operationNumber = ModelOperationRelate.OperationNumber
                    };

                return RedirectToAction("Index", "Tasks", rout);
            }
        }

        [HttpPost]
        public virtual ActionResult SavePostEligibilityClause(
            OperationRelatedModel ModelOperationRelate)
        {
            var modelEligibility = _operationClause
                .GetContractById(ModelOperationRelate.Contracts[0].ContractId);

            modelEligibility.UserComments.Clear();

            var additionalValidators = Request["validator_list_additional_list"];

            foreach (var Comments in ModelOperationRelate.Contracts[0].UserComments)
            {
                if (Comments.UserCommentId == -1)
                {
                    Comments.ModifiedBy = IDBContext.Current.UserName;
                }
            }

            modelEligibility.UserComments
                .AddRange(ModelOperationRelate.Contracts[0].UserComments);
            _operationClause
                .SaveContractStatus(modelEligibility, IDBContext.Current.UserName);

            if (ModelOperationRelate.ActionType == "RejectTask")
            {
                var routing =
                    new
                    {
                        taskStatus = ModelOperationRelate.CompleteTaskView.TaskStatus,
                        taskId = ModelOperationRelate.CompleteTaskView.TaskId,
                        classification = string.Empty
                    };

                return RedirectToAction("RejectTask", routing);
            }

            if (ModelOperationRelate.ActionType == "ValidateTask")
            {
                var routing =
                    new
                    {
                        taskStatus = ModelOperationRelate.CompleteTaskView.TaskStatus,
                        taskId = ModelOperationRelate.CompleteTaskView.TaskId,
                        classification = string.Empty,
                        additionalValidators
                    };

                return RedirectToAction("ValidateTask", routing);
            }
            else
            {
                var rout =
                    new
                    {
                        operationNumber = ModelOperationRelate.OperationNumber
                    };

                return RedirectToAction("Index", "Tasks", rout);
            }
        }

        [HttpPost]
        public virtual ActionResult SavePostRevolvingClause(
            OperationRelatedModel ModelOperationRelate)
        {
            var modelRevolvingFund = _operationClause
                .GetRevolvingFundById(
                    ModelOperationRelate.Contracts[0].RevolvingFund[0].RevolvingFundId);

            modelRevolvingFund.UserComments.Clear();

            var additionalValidators = Request["validator_list_additional_list"];

            foreach (var Comments in
                ModelOperationRelate.Contracts[0]
                .RevolvingFund[0].UserComments.Where(Comments => Comments.UserCommentId == -1))
            {
                Comments.ModifiedBy = IDBContext.Current.UserName;
            }

            modelRevolvingFund.UserComments
                .AddRange(ModelOperationRelate.Contracts[0].RevolvingFund[0].UserComments);

            _operationClause
                .SaveOrUpdateRevolvingFund(modelRevolvingFund, IDBContext.Current.UserName);

            if (ModelOperationRelate.ActionType == "RejectTask")
            {
                var routing =
                    new
                    {
                        taskStatus = ModelOperationRelate.CompleteTaskView.TaskStatus,
                        taskId = ModelOperationRelate.CompleteTaskView.TaskId,
                        classification = string.Empty
                    };

                return RedirectToAction("RejectTask", routing);
            }

            if (ModelOperationRelate.ActionType == "ValidateTask")
            {
                var routing =
                    new
                    {
                        taskStatus = ModelOperationRelate.CompleteTaskView.TaskStatus,
                        taskId = ModelOperationRelate.CompleteTaskView.TaskId,
                        classification = string.Empty,
                        additionalValidators
                    };

                return RedirectToAction("ValidateTask", routing);
            }
            else
            {
                var rout =
                    new
                    {
                        operationNumber = ModelOperationRelate.OperationNumber
                    };

                return RedirectToAction("Index", "Tasks", rout);
            }
        }

        [HttpPost]
        public virtual ActionResult SaveTask(
            BasicPMIWorkflowIds model,
            List<int> itemDeletedList = null)
        {
            model.UserName = IDBContext.Current.UserName;

            if (itemDeletedList != null)
            {
                model.itemDeletedList = itemDeletedList;
            }

            List<int> ListIdsPos = new List<int>();
            int IdPosition = 0;

            foreach (var commentsFor in model.UserComments)
            {
                if (string.IsNullOrEmpty(commentsFor.Text))
                {
                    ListIdsPos.Add(IdPosition);
                }

                IdPosition++;
            }

            foreach (int Icomments in ListIdsPos)
            {
                model.UserComments.RemoveAt(Icomments);
            }

            var additionalValidators = Request["validator_list_additional_list"];
            var ModelWorkFlow =
                _resultsMatrixModelRepository.SaveCommentsPMIWithTask(model);

            if (model.ActionType == "RejectTask")
            {
                var routing =
                    new
                    {
                        taskStatus = model.CompleteTaskView.TaskStatus,
                        taskId = model.CompleteTaskView.TaskId,
                        classification = string.IsNullOrEmpty(model.Clasification)
                        ? model.PreviousClasification : model.Clasification
                    };

                return RedirectToAction("RejectTask", routing);
            }

            if (model.ActionType == "ValidateTask")
            {
                var routing =
                    new
                    {
                        taskStatus = model.CompleteTaskView.TaskStatus,
                        taskId = model.CompleteTaskView.TaskId,
                        classification = string.IsNullOrEmpty(model.Clasification) ?
                        model.PreviousClasification : model.Clasification,
                        additionalValidators
                    };

                return RedirectToAction("ValidateTask", routing);
            }

            if (_globalModelRepository.SaveClasificationTask(model))
            {
                AdditionalValidatorsSave(additionalValidators, model.CompleteTaskView.TaskId);

                var code = model.CompleteTaskView.Code;

                if (code != "WF-VI-001" && code != "WF-PCR-001")
                {
                    var rout =
                        new
                        {
                            operationNumber = model.OperationNumber
                        };

                    return RedirectToAction("Index", "Tasks", rout);
                }

                var routing =
                    new
                    {
                        id = model.CompleteTaskView.TaskId,
                        edit = false
                    };

                return RedirectToAction("Detail", routing);
            }

            return HttpNotFound();
        }

        [HttpPost]
        public virtual ActionResult SaveFinalAprovalConditionTask(
            DetailsConditionViewModel individual)
        {
            var response = _agreementAndConditionService.SaveFinalAprovalConditionTask(individual);

            var additionalValidators = Request["validator_list_additional_list"];

            if (!response.IsValid || string.IsNullOrEmpty(individual.ActionType))
                return RedirectToAction("Index", new { operationNumber = individual.MainOperationNumber });

            return RedirectToAction(individual.ActionType, new
            {
                taskStatus = individual.CompleteTaskViewModel.TaskStatus,
                taskId = individual.CompleteTaskViewModel.TaskId,
                classification = string.Empty,
                additionalValidators = additionalValidators
            });
        }

        [HttpPost]
        public virtual ActionResult SaveExtensionConditionTask(
            DetailsConditionViewModel extension)
        {
            var response = _agreementAndConditionService.SaveConditionExtensionTask(extension);
            var additionalValidators = Request["validator_list_additional_list"];

            if (!response.IsValid || string.IsNullOrEmpty(extension.ActionType))
                return RedirectToAction("Index", new { operationNumber = extension.MainOperationNumber });

            return RedirectToAction(extension.ActionType, new
            {
                taskStatus = extension.CompleteTaskViewModel.TaskStatus,
                taskId = extension.CompleteTaskViewModel.TaskId,
                classification = string.Empty,
                additionalValidators = additionalValidators
            });
        }

        [HttpPost]
        public virtual JsonResult GetData(
            DataSourceRequest request,
            string urlOperation,
            string urlOperationDraft,
            bool operationTasks = true,
            string operationNumber = null)
        {
            WorkflowType workflowType = null;

            var result = _globalModelRepository
                .GetTaskByUser(IDBContext.Current.UserName,
                    request,
                    GlobalCommonLogic.GetOperationDetailURL(),
                    GlobalCommonLogic.GetOperationDraftDetailURL(),
                    operationNumber);
            Dictionary<int, int> validator = new Dictionary<int, int>();
            List<MyTasksViewModel> result_ = new List<MyTasksViewModel>();

            foreach (var itemResultData in result.Data)
            {
                try
                {
                    var entityValues = _globalModelRepository
                        .GetValuesWorkFlowTasksType(
                            itemResultData.EntityId, itemResultData.EntityType);

                    var responseWorkflowType = _workflowsService
                        .GetWorkflowType(itemResultData.WorkflowTypeId);

                    if (responseWorkflowType.IsValid)
                    {
                        workflowType = responseWorkflowType.Model;
                    }

                    var workflowTaskType = _workflowsService
                        .GetWorkflowTaskType(itemResultData.TaskWorkflowTaskTypeId)
                        .Model;

                    switch (itemResultData.EntityType)
                    {
                        case "ClauseIndividual":
                        case "ClauseExtension":

                            itemResultData.TaskNameEn = ResolveNameForClause(
                                itemResultData,
                                workflowType,
                                workflowTaskType,
                                entityValues);
                            break;

                        case "RevolvingFund":
                        case "Contract":

                            itemResultData.TaskNameEn = ResolveNameForContract(
                                itemResultData,
                                workflowType,
                                workflowTaskType,
                                entityValues);
                            break;

                        case "Mission":

                            itemResultData.TaskNameEn =
                                Localization.GetText(itemResultData.TaskNameEn) + " - " +
                                entityValues["Mission_code"] + " - " + entityValues["Mission_obj"];
                            break;

                        case "TCM":

                            if (!string.IsNullOrEmpty(Localization.GetText(entityValues["IsFinalReport"])))
                            {
                                string fundCode = FundCode
                                    .GetFundCodeFromFundCoordinator(itemResultData.UserProfile);

                                itemResultData.TaskNameEn = string.Format(
                                    "{0} ({1})",
                                    Localization.GetText(entityValues["IsFinalReport"]),
                                    fundCode);
                            }
                                
                            break;

                        default:

                            itemResultData.TaskNameEn = Localization.GetText(itemResultData.TaskNameEn);
                            break;
                    }

                    var workflowInstanceTask = _workflowInstanceTaskRepository
                        .Find(itemResultData.WorkflowInstanceTaskId);

                    var workflowContract = workflowInstanceTask
                        .WorkflowInstance.Contract;
                    if (workflowContract != null)
                    {
                        itemResultData.LoansList = itemResultData.LoansList
                            .Where(loan => loan == workflowContract.ContractNumber);
                        itemResultData.Loans = string.Join(";", itemResultData.LoansList);
                    }

                    var actionsResponse = _workflowManagerService
                        .GetActions(workflowInstanceTask.WorkflowTaskType.TaskCode);
                    if (actionsResponse.IsValid)
                    {
                        itemResultData.Actions = actionsResponse.Models
                            .Select(a => new MW.Domain.Models.Global.Action
                                {
                                    Name = Localization.GetText(a.WorkflowActionName),
                                    Value = a.WorkflowActionCode,
                                });
                    }
                    
                    validator.Add(itemResultData.WorkflowInstanceTaskId, 0);
                    result_.Add(itemResultData);
                    itemResultData.StartDate =
                        itemResultData.StartDate == null ?
                            itemResultData.StartDate :
                            new DateTime(
                                itemResultData.StartDate.Value.Year,
                                itemResultData.StartDate.Value.Month,
                                itemResultData.StartDate.Value.Day);
                }
                catch
                {
                    if (itemResultData.UserRead)
                    {
                        result.Read = result.Read - 1;
                    }
                    else
                    {
                        result.New = result.New - 1;
                    }
                }
            }

            result.Data = result_;
            result.Total = result.New + result.Read;

            return Json(result);
        }

        [HttpPost]
        public virtual JsonResult GetCommentsByTask(
            int taskId,
            int type,
            string operationNumber,
            int identityId)
        {
            List<GlobalReferences.UserCommentModel> result =
                _globalModelRepository
                .GetCommentsByTask(taskId,
                    type,
                    operationNumber,
                    IDBContext.Current.UserName,
                    identityId).ToList();

            result.Reverse();

            return Json(result);
        }

        [HttpPost]
        public virtual JsonResult GetExpectedtextResult(
            int entityId,
            int type,
            string classification = null)
        {
            try
            {
                string template = Localization.GetText("WF-DETAIL-TABLE-00" + type);
                Dictionary<string, string> keys = new Dictionary<string, string>();
                string replacedTemplate = string.Empty;

                if ((type == (int)K2CallType.March) || (type == (int)K2CallType.September))
                {
                    keys.Add("[CLASSIFICATION]",
                        string.IsNullOrEmpty(classification) ?
                            EmailCodes.EmptyKey :
                            classification);
                }
                else if ((type == (int)K2CallType.ClauseExtension) ||
                    (type == (int)K2CallType.ClauseIndividual) ||
                    (type == (int)K2CallType.Contract) ||
                    (type == (int)K2CallType.RevolvingFund) ||
                    (type == (int)K2CallType.SupervisionPlanVersion))
                {
                    keys = _globalModelRepository.GetExpectedText(entityId, type);
                }
                else
                {
                    var response = _workflowsService.GetWorkflowType(type);
                    if (response.IsValid && !response.Model.WithK2)
                    {
                        var typeCode = response.Model.Code;
                        keys = _globalModelRepository.GetExpectedTextByCode(entityId, typeCode);
                        template = Localization.GetText("WF-DETAIL-TABLE-" + typeCode);
                    }
                    else
                    {
                        return Json(EmailCodes.EmptyKey);
                    }  
                }

                foreach (KeyValuePair<string, string> key in keys)
                {
                    if (key.Value != null)
                    {
                        replacedTemplate = replacedTemplate.Length <= 0 ?
                            template.Replace(key.Key, key.Value) :
                            replacedTemplate.Replace(key.Key, key.Value);
                    }
                    else
                    {
                        replacedTemplate = replacedTemplate.Length <= 0 ?
                            template.Replace(key.Key, EmailCodes.EmptyKey) :
                            replacedTemplate.Replace(key.Key, EmailCodes.EmptyKey);
                    }
                }

                replacedTemplate = replacedTemplate
                    .Replace(EmailCodes.ContractKey, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.CurrentExpirationDateKey, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.OperationKey, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.OperationDescripKey, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.OperationDescripKeySp, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.RequestKey, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.ActionKey, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.ClauseNumberKey, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.ClauseDescriptionKey, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.RequestExtensionDateKey, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.IdbRequestKey, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.FinalStatusKey, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.FinalStatusKeySp, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.CurrentRevolvingFundKey, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.NewRevolvingFundKey, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.RoleKey, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.LinkKey, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.TextLinkKey, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.TextLinkKeySp, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.SharePointSiteKey, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.NextTaskIdKey, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.UserKey, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.User2Key, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.Yearkey, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.ReasonKey, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.CommentKey, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.OwnerKey, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.OwnerFullNameKey, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.UserCommentKey, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.DateCommentKey, EmailCodes.EmptyKey)
                    .Replace(EmailCodes.CommentTextKey, EmailCodes.EmptyKey);

                if (template == ("WF-DETAIL-TABLE-00" + type) ||
                    string.IsNullOrEmpty(template) ||
                    string.IsNullOrEmpty(replacedTemplate))
                {
                    return Json(EmailCodes.EmptyKey);
                }

                return Json(replacedTemplate);
            }
            catch
            {
                return Json(EmailCodes.EmptyKey);
            }
        }

        [HttpPost]
        public virtual JsonResult GetJustificationAndRecommendation(int clauseExtensionId)
        {
            Dictionary<string, string> result =
                _globalModelRepository.GetJustificationAndRecommendation(clauseExtensionId);

            return Json(result);
        }

        [HttpPost]
        public virtual JsonResult GetClasification()
        {
            try
            {
                var lstClasification = _masterDataModel.GetMasterDataModels("PMI_CLASSIFICATION");
                int lstClasificationIndex = lstClasification.FindIndex(cl => cl.NameEn == NOT_APPLICABLE_CLASSIFICATION);

                if (lstClasificationIndex > -1)
                {
                    lstClasification.RemoveAt(lstClasificationIndex);
                }

                return Json(lstClasification);
            }
            catch
            {
                return Json(EmailCodes.EmptyKey);
            }
        }

        [HttpPost]
        public virtual JsonResult UpdateCommentsAndValidateTask(
            string taskId,
            string commentId,
            string date,
            string classification,
            string status,
            string type,
            string action,
            string typeId,
            string text,
            string classificationId,
            string option,
            string entityId,
            string itemsToDelete)
        {
           bool withK2 = true;

            switch (option)
            {
                case "Validate":

                    if (action != "null" || !string.IsNullOrEmpty(action))
                    {
                        var workflow = new WorkflowEntity
                        {
                            Status =
                                (action == "Reject") ?
                                    K2TaskStatus.Rejected.GetStringValue() :
                                    K2TaskStatus.Approved.GetStringValue(),
                            UserName = IDBContext.Current.UserName,
                            Classification = classification ?? NOT_APPLICABLE_CLASSIFICATION,
                            PmrValidationStage = status
                        };

                        switch (int.Parse(typeId))
                        {
                            case (int)K2CallType.September:
                            case (int)K2CallType.StartUpPlanValidation:
                            case (int)K2CallType.March:
                            case (int)K2CallType.PMRCycleStartup:
                                _k2ServiceProxy.UpdateWorkflow(
                                    workflow,
                                    int.Parse(taskId),
                                    action == "Reject"
                                        ? K2TaskStatus.Rejected
                                        : K2TaskStatus.Approved);
                                break;

                            case (int)K2CallType.ClauseIndividual:
                            case (int)K2CallType.ClauseExtension:
                            case (int)K2CallType.Contract:
                            case (int)K2CallType.RevolvingFund:
                            case (int)K2CallType.SupervisionPlanVersion:
                                if (_noK2WorkflowsService.AdvanceNoK2WorkflowIfApplies(
                                    Convert.ToInt32(taskId),
                                    IDBContext.Current.UserName,
                                    action == "Aproved" ?
                                        NoK2WorkflowTaskActionEnum.Validate :
                                        NoK2WorkflowTaskActionEnum.Reject))
                                {
                                    break;
                                }

                                _k2ServiceProxy.UpdateWorkflow(
                                    workflow,
                                    int.Parse(taskId),
                                    action == "Reject" ?
                                        K2TaskStatus.Rejected :
                                        K2TaskStatus.Approved);
                                break;
                            default:
                                _workflowManagerService
                                    .AdvanceWorkflow(int.Parse(taskId), IDBContext.Current.UserName, action, null);
                                    break;
                        }
                    }

                    break;
                case "Comments":

                    GlobalReferences.UserCommentModel model =
                        new GlobalReferences.UserCommentModel()
                        {
                            WorkflowInstanceTaskId = int.Parse(taskId),
                            Modified = DateTime.Now.ToString(),
                            ModifiedBy = IDBContext.Current.UserName,
                            Created = DateTime.Now.ToString(),
                            CreatedBy = IDBContext.Current.UserName,
                            Text = text,
                            UserCommentId = (type == "E") ? int.Parse(commentId) : 0,
                            IsEditable = true,
                        };

                    var intTaskId = int.Parse(taskId);
                    var taskRepository = _workflowInstanceTaskRepository
                        .GetOne(x => x.WorkflowInstanceTaskId == intTaskId);

                    withK2 = taskRepository.WorkflowInstance.WorkflowType.WithK2;

                    if (taskRepository != null)
                    {
                        if (!withK2)
                        {
                            var modelClauseIndividual = taskRepository.UserComments
                                .FirstOrDefault(x => x.UserCommentId == model.UserCommentId);

                            SaveUpdatedComment(modelClauseIndividual, model, taskRepository);
                        }
                        else
                        {
                            _globalModelRepository.UpdateCommentsByTask(
                          model, int.Parse(typeId), int.Parse(entityId), new List<int>());
                        }
                    }

                    break;

                case "Clasification":

                    BasicPMIWorkflowIds modelclasification =
                        new BasicPMIWorkflowIds()
                        {
                            Clasification = classification,
                            CompleteTaskView =
                                new CompleteTaskViewModel()
                                {
                                    TaskId = int.Parse(taskId)
                                }
                        };

                    _globalModelRepository.SaveClasificationTask(modelclasification);
                    break;
                case "ItemsDelete":
                    List<int> DeleteList = new List<int>();
                    if (!string.IsNullOrEmpty(itemsToDelete))
                    {
                        foreach (var item in itemsToDelete.Split('|'))
                        {
                            if (!string.IsNullOrEmpty(item))
                            {
                                DeleteList.Add(int.Parse(item));
                            }
                        }
                    }

                    _globalModelRepository.UpdateCommentsByTask(
                            null, int.Parse(typeId), int.Parse(entityId), DeleteList);
                    break;
            }

            return Json(true);
        }

        [HttpPost]
        public virtual JsonResult Update([Bind(Prefix = "models")] List<MyTasksViewModel> models)
        {
            var result = new GlobalModelRepository().updateTask(models);
            return Json(result ? models : new List<MyTasksViewModel>());
        }

        public virtual ActionResult DelegatingTaskModal(int taskId, string username = null)
        {
            ViewBag.Username = username;

            var delegatingTaskSearchModel =
                new Models.DelegatingTaskSearchModel
                {
                    TaskId = taskId,
                    Roles = _masterDataModel
                        .GetMasterDataModels("MEMBER ROLE")
                        .OrderBy(c => c.NameEn).ToList(),
                    Countries = _masterDataModel
                        .GetMasterDataModels("COUNTRY")
                        .OrderBy(c => c.NameEn).ToList(),
                    Divisions = _masterDataModel
                        .GetMasterDataModels("SECTOR")
                        .OrderBy(c => c.NameEn).ToList(),
                    SharepointGroups = _globalModelRepository.GetSharepointGroups(),
                    SearchOption = -1
                };

            return PartialView(delegatingTaskSearchModel);
        }

        public virtual ActionResult DelegatingTaskSearch(int taskId)
        {
            var model =
                new Models.DelegatingTaskSearchModel
                {
                    TaskId = taskId,
                    Roles = _masterDataModel
                        .GetMasterDataModels("MEMBER ROLE")
                        .OrderBy(c => c.NameEn).ToList(),
                    Countries = _masterDataModel
                        .GetMasterDataModels("COUNTRY")
                        .OrderBy(c => c.NameEn).ToList(),
                    Divisions = _masterDataModel
                        .GetMasterDataModels("SECTOR")
                        .OrderBy(c => c.NameEn).ToList(),
                    SharepointGroups = _globalModelRepository.GetSharepointGroups(),
                    SearchOption = -1
                };

            return PartialView(model);
        }

        public virtual ActionResult DelegatingTaskResults(DelegatingTaskSearchModel modelSearch)
        {
            var fullTeamMembers = new List<UserIdentityModel>();

            if (modelSearch.SearchOption > -1)
            {
                fullTeamMembers = ClientGlobalModelRepository
                    .SearchUsersToDelegateTask(modelSearch);
            }
            else
            {
                if (!string.IsNullOrEmpty(modelSearch.Username) ||
                    !string.IsNullOrEmpty(modelSearch.FullName))
                {
                    fullTeamMembers = ClientGlobalModelRepository
                        .SearchUsersToDelegateTask(modelSearch);
                }
            }

            var i = 0;
            var query = from s in fullTeamMembers
                        let num = i++
                        group s by num / 3
                            into g
                        select g.ToArray();

            var results = query.ToArray();

            var modelResults =
                new DelegatingTaskResultsModel
                {
                    FilterApplied = string.Empty,
                    TaskId = modelSearch.TaskId,
                    Users = fullTeamMembers,
                    SplittedUsers = results
                };

            return PartialView(modelResults);
        }

        [HttpGet]
        public virtual ActionResult DelegatingTaskSave(
            string operationNumber,
            int taskId,
            string username,
            string currentUserName)
        {
            var status = 0;

            try
            {
                var success = false;

                success = _globalModelRepository.RemoveDelegation(username, taskId);
                success = _globalModelRepository.DelegateTask(username, taskId, currentUserName);

                var modelDelegatedTask = _globalModelRepository
                    .AdditionalNotifications(taskId, IDBContext.Current.UserName, username);

                if (modelDelegatedTask.taskId != 0)
                {
                    var callObject = new DelegateNotification
                    {
                        operationNumber = modelDelegatedTask.operationNumber,
                        originalTaskRole = modelDelegatedTask.originalTaskRole,
                        taskId = modelDelegatedTask.taskId,
                        userNameActual = modelDelegatedTask.userNameActual,
                        userNameDelegated = modelDelegatedTask.userNameDelegated,
                        workflowInstanceId = modelDelegatedTask.workflowInstanceId,
                        workflowTaskType = modelDelegatedTask.workflowTaskType,
                        initialWorkflowRol = modelDelegatedTask.initialWorkflowRol,
                        workflowType = modelDelegatedTask.workflowType,
                        IdNotification = modelDelegatedTask.IdNotification
                    };

                    var respose = _k2ServiceProxy
                        .BeginWorkflow(K2CallType.AdditionalNotifications, callObject);

                    success = respose
                        .StartsWith(K2Response.AdditionalNotifications.GetStringValue());
                }

                status = success ? 603 : 103;
            }
            catch (Exception e)
            {
                status = 103;

                Logger.GetLogger()
                .WriteError("TasksController", "Error when delegating task save", e);
            }

            var routing = new
            {
                operationNumber,
                State = status,
                Status = string.Empty
            };

            return RedirectToAction("Index", "Tasks", routing);
        }

        [HttpGet]
        public virtual ActionResult RemoveDelegation(
            string operationNumber,
            int taskId,
            string username)
        {
            var status = 0;

            try
            {
                status =
                    ClientGlobalModelRepository.RemoveDelegation(username, taskId) ?
                        602 :
                        102;
            }
            catch (Exception)
            {
                status = 102;
            }

            var routing = new
            {
                operationNumber,
                State = status,
                Status = string.Empty
            };
            return RedirectToAction("Index", "Tasks", routing);
        }

        public virtual ActionResult DelegatingTaskAssigned(
            string operationNumber,
            int taskId,
            string userName)
        {
            var delegatedTask = ClientGlobalModelRepository
                .GetDelegationsTaskByUser(taskId, userName);

            var viewModel =
                new DelegatedTaskUserModel
                {
                    FilterApplied = string.Empty,
                    TaskId = taskId,
                    UserData = delegatedTask,
                    OperationNumber = operationNumber
                };

            return PartialView(viewModel);
        }

        public void AdditionalValidatorsSave(string additionalValidators, int taskId)
        {
            if (string.IsNullOrEmpty(additionalValidators))
            {
                return;
            }

            var addVal = additionalValidators
                .Split('|')
                .ToList();
            var roles = MasterDefinitions.CONVERGENCE_ROLES;

            if (addVal.Count() == 0)
            {
                return;
            }

            int index = 0;

            while (index < addVal.Count())
            {
                if (!roles.Contains(addVal[index]))
                {
                    addVal.RemoveAt(index);
                }
                else
                {
                    index++;
                }
            }

            GlobalModelRepository.AdditionalValidatorSaveByTId(addVal.ToArray(), taskId);
        }

        [HttpGet]
        public virtual ActionResult Message()
        {
            return PartialView();
        }

        [HttpGet]
        public virtual ActionResult ValidationMessage(int messageCode, string id = "")
        {
            ViewData["messageCode"] = messageCode;
            ViewBag.id = id;
            return PartialView();
        }

        [HttpGet]
        public virtual ActionResult ModalDemo()
        {
            return PartialView();
        }

        #region Private Methods

        private List<SupervisionPlanModel> PrepareSummaryView(
            SupervisionPlanModel plan,
            string operationNumber,
            int year)
        {
            var availableYears =
                new List<int>
                {
                    DateTime.Today.Year, DateTime.Today.Year + 1
                };

            if (plan != null)
            {
                availableYears.Remove(year);
            }

            var supervisionPlans = _supervisionPlanModelRepository
                .GetByOperation(operationNumber);

            if (plan != null)
            {
                supervisionPlans.Add(plan);
            }

            supervisionPlans.Sort((s1, s2) => s1.Year.CompareTo(s2.Year));

            if (supervisionPlans.All(s => s.Year != year))
            {
                year = availableYears.First();
            }

            foreach (var sp in supervisionPlans)
            {
                sp.ActualVersionIsNew = sp.ActualVersion == null;
                sp.ActualVersionIsApproved = sp.ActualVersion != null &&
                    sp.ActualVersion.ValidationStage.Code
                    .Equals("SP_APPR", StringComparison.InvariantCultureIgnoreCase);
                sp.ActualVersionEditable = !(sp.ActualVersion != null &&
                    sp.ActualVersion.ValidationStage.Code
                    .Equals("SP_REV", StringComparison.InvariantCultureIgnoreCase));
                sp.ActualVersionCanModify = sp.ActualVersion != null &&
                    sp.ActualVersion.ValidationStage.Code
                    .Equals("SP_APPR", StringComparison.InvariantCultureIgnoreCase);

                var stages =
                    new List<string>
                    {
                        "SP_DRAFT", "SP_MOD"
                    };

                sp.ActualVersionIsInDraftOrModified = sp.ActualVersion == null ||
                    stages.Any(s =>
                        s.Equals(
                            sp.ActualVersion.ValidationStage.Code,
                            StringComparison.InvariantCultureIgnoreCase));

                sp.ActualVersionIsInModified = sp.ActualVersion != null &&
                    sp.ActualVersion.ValidationStage.Code
                    .Equals("SP_MOD", StringComparison.InvariantCultureIgnoreCase);

                sp.SupervisionPlanId = sp.ActualVersion == null ? -1 : sp.SupervisionPlanId;
            }

            SetLocalizedValues();

            ViewBag.OperationNumber = operationNumber;
            ViewBag.AvailableYears = availableYears;
            ViewBag.SelectedYear = year;

            if (supervisionPlans.Count > 0)
            {
                ViewBag.OperationId = supervisionPlans.First(p => p.Year == year).OperationId;
                SetLocalizedModel(supervisionPlans);
                TempData["SupervisionPlan"] = supervisionPlans;
            }

            return supervisionPlans;
        }

        private void LocalizeAndCompleteBudgetMatrix(
            IEnumerable<SupervisionPlanModel> supervisionPlans)
        {
            var bCategory = _masterDataModel
                .GetMasterDataModels("BUDGET_CATEGORY");
            var bFundingSource = _masterDataModel
                .GetMasterDataModels("FUNDING_SOURCE");

            ViewBag.LocalizedBudgetMatrixRows = bCategory
                .ToDictionary(c => c.ConvergenceMasterDataId,
                    BusinessLogic.MasterDataGetLocaleName);

            ViewBag.LocalizedBudgetMatrixCols = bFundingSource
                .ToDictionary(fs => fs.ConvergenceMasterDataId,
                    BusinessLogic.MasterDataGetLocaleName);

            ViewBag.EmptyCosts = (from md2 in bCategory
                                  from md1 in bFundingSource
                                  select new CostModel
                                  {
                                      BudgetCategory = md2,
                                      BudgetCategoryId = md2.ConvergenceMasterDataId,
                                      FundingSource = md1,
                                      FundingSourceId = md1.ConvergenceMasterDataId,
                                      Budget = 0,
                                      CostId = -1
                                  }).ToList();

            foreach (var sp in supervisionPlans.Where(p => p.ActualVersion != null))
            {
                var costs = (from md2 in bCategory
                             from md1 in bFundingSource
                             let cost = sp.ActualVersion.Costs
                                 .Find(c => c.BudgetCategoryId == md2.ConvergenceMasterDataId &&
                                     c.FundingSourceId == md1.ConvergenceMasterDataId)
                             select new CostModel
                             {
                                 BudgetCategory = md2,
                                 BudgetCategoryId = md2.ConvergenceMasterDataId,
                                 FundingSource = md1,
                                 FundingSourceId = md1.ConvergenceMasterDataId,
                                 Budget = cost != null ? cost.Budget : 0,
                                 CostId = cost != null ? cost.CostId : -1
                             }).ToList();

                sp.ActualVersion.Costs = costs;
            }
        }

        private void SetLocalizedValues()
        {
            var dQuarter = _masterDataModel.GetMasterDataModels("QUARTER");
            var dActivities = _masterDataModel.GetMasterDataModels("MAIN_ACTIVITY");

            ViewBag.LocalizedQuarterValue = dQuarter
                .ToDictionary(d => d.ConvergenceMasterDataId, BusinessLogic.MasterDataGetLocaleName);
            ViewBag.LocalizedMainActivities = dActivities
                .ToDictionary(d => d.ConvergenceMasterDataId, BusinessLogic.MasterDataGetLocaleName);
        }

        private void SetLocalizedModel(IEnumerable<SupervisionPlanModel> supervisionPlan)
        {
            foreach (var sp in supervisionPlan.Where(s => s.ActualVersion != null))
            {
                sp.ActualVersion.ValidationStage.Name = BusinessLogic
                    .MasterDataGetLocaleName(sp.ActualVersion.ValidationStage);

                foreach (var spv in sp.SupervisionPlanVersions)
                {
                    spv.ValidationStage.Name =
                        BusinessLogic.MasterDataGetLocaleName(spv.ValidationStage);
                }
            }
        }

        private List<SelectListItem> RemoveUsedRole(List<ValidatorViewModel> validators, List<SelectListItem> roles)
        {
            foreach (var validator in validators)
            {
                roles.RemoveAll(x => x.Text == validator.Role);
            }

            return roles;
        }

        private List<SelectListItem> RemoveUsedRole(List<string> validators, List<SelectListItem> roles)
        {
            foreach (var validator in validators)
            {
                roles.RemoveAll(x => x.Text == validator);
            }

            return roles;
        }

        private List<SelectListItem> GetListMasterData(string type)
        {
            var list = new List<SelectListItem>();
            var listRepository = _catalogService.GetMasterDataListByTypeCode(true, typeCodes: type);

            if (listRepository != null && listRepository.MasterDataCollection != null &&
                listRepository.MasterDataCollection.Count > 0)
            {
                list = listRepository.MasterDataCollection.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.MasterId.ToString(),
                    Text = MvcHelpers.GetItemName(o, Localization.CurrentLanguage),
                }).ToList();
            }

            return list;
        }

        private string ResolveNameForContract(MyTasksViewModel itemResultData, 
            WorkflowType workflowType,
            WorkflowTaskType workflowTaskType,
            IDictionary<string, string> entityValues)
        {
            var taskNameLocalized = GetLocalizedTaskName(
                itemResultData,
                workflowType,
                workflowTaskType);

            if (workflowType.WithK2)
            {
                return taskNameLocalized +
                    " " + Localization.GetText("for contract") +
                    " " + entityValues["Contract"];
            }
            else
            {
                return TaskHelpers.TaskNameAndParametersProcess(
                    taskNameLocalized,
                    itemResultData.JsonTag);
            }
        }

        private string ResolveNameForClause(MyTasksViewModel taskModel,
            WorkflowType workflowType,
            WorkflowTaskType workflowTaskType,
            IDictionary<string, string> entityValues)
        {
            var taskNameLocalized = GetLocalizedTaskName(
                taskModel,
                workflowType,
                workflowTaskType);

            if (workflowType.WithK2)
            {
                return taskNameLocalized +
                    " " + Localization.GetText("for contract") +
                    " " + entityValues["Contract"] +
                    " " + Localization.GetText("and clause") +
                    " " + entityValues["Clause"];
            }
            else
            {
                return TaskHelpers.TaskNameAndParametersProcess(
                    taskNameLocalized,
                    taskModel.JsonTag);
            }
        }

        private string GetLocalizedTaskName(MyTasksViewModel taskModel,
            WorkflowType workflowType,
            WorkflowTaskType workflowTaskType)
        {
            if (workflowTaskType == null)
            {
                return Localization.GetText(taskModel.TaskNameEn);
            }

            if (workflowType.WithK2)
            {
                return Localization.GetText(workflowTaskType.NameEn);
            }

            return Localization.GetText(workflowTaskType.TaskCode + "-NAME");
        }

        private void SaveUpdatedComment(
            UserComment modelClauseIndividual,
            UserCommentModel model,
            WorkflowInstanceTask taskRepository)
        {
            if (modelClauseIndividual != null)
            {
                modelClauseIndividual.Text = model.Text;
                modelClauseIndividual.Modified = DateTime.Now;
                modelClauseIndividual.ModifiedBy = model.ModifiedBy;
                _workflowInstanceTaskRepository.SaveChanges();
            }
            else
            {
                var UserCommentModel_ = new UserComment();
                UserCommentModel_.Text = model.Text;
                UserCommentModel_.Modified = DateTime.Now;
                UserCommentModel_.ModifiedBy = model.ModifiedBy;
                UserCommentModel_.Created = DateTime.Now;
                UserCommentModel_.CreatedBy = model.CreatedBy;
                taskRepository.UserComments.Add(UserCommentModel_);
                _workflowInstanceTaskRepository.SaveChanges();
            }
        }
        #endregion
    }
}