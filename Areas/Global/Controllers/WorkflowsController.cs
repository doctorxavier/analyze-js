using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using System.Web.Configuration;
using System.Web.Mvc;

using IDB.Architecture;
using IDB.Architecture.Language;
using IDB.Architecture.Logging;
using IDB.MW.Application.AdministrationModule.Messages.RolesAndPermissionsService;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Application.GlobalModule.Enums;
using IDB.MW.Application.GlobalModule.Helpers;
using IDB.MW.Application.GlobalModule.Messages.WorkflowsService;
using IDB.MW.Application.GlobalModule.Services.WorkflowsService;
using IDB.MW.Application.GlobalModule.ViewModels;
using IDB.MW.Application.GlobalModule.Workflow;
using IDB.MW.Application.OPUSModule.Services.K2DataService;
using IDB.MW.Domain.Contracts.ModelRepositories.Global;
using IDB.Architecture.Security.Models.UserIdentity;
using IDB.MW.Domain.Contracts.ModelRepositories.Workflows;
using IDB.MW.Domain.Models.Workflows;
using IDB.MW.Domain.Session;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.MW.GenericWorkflow.Services;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.Presentation.MVC4.Areas.Global.Models;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Domain.Entities;
using IDB.Presentation.MVC4.Areas.Global.Helpers;

namespace IDB.Presentation.MVC4.Areas.Global.Controllers
{
    public partial class WorkflowsController : BaseController
    {
        public const string StrMachine = "Machine";
        public const string StrK2Server = "K2Server";
        public const string StrHostServerName = "HostServerName";
        public const string StrHostServerPort = "HostServerPort";
        public const string StrProcessId = "ProcessID";
        public const string CONVERGENCE_ROLES = "CONVERGENCE ROLES";

        private readonly IWorkflowsService _workflowsService;
        private readonly ICatalogService _catalogService;
        private readonly IK2DataService _k2DataService;
        private readonly ViewModelMapperHelper _viewModelMapperHelper;
        private readonly IWorkflowTypeRepository _workflowTypeRepository;
        private readonly IWorkflowManagerService _workflowManager;
        private readonly IValidatorService _validatorService;
        private readonly IWorkflowInstanceTaskRepository _workflowInstanceTaskRepository;

        public virtual IGlobalModelRepository _globalModelRepository { get; set; }

        private readonly IWorkflowModelRepository _workflowModelRepository;

        public static string GetK2WorkflowViewDetailURL()
        {
            var sb = new StringBuilder();
            Configuration rootWebConfig = WebConfigurationManager.OpenWebConfiguration("~/");

            if (rootWebConfig.AppSettings.Settings.Count > 0)
            {
                string machine = null;
                string k2Server = null;
                string hostServerName = null;
                string hostServerPort = null;

                KeyValueConfigurationElement strMachineValue = rootWebConfig.AppSettings.Settings[StrMachine];
                KeyValueConfigurationElement strK2ServerValue = rootWebConfig.AppSettings.Settings[StrK2Server];
                KeyValueConfigurationElement strHostServerNameValue = rootWebConfig.AppSettings.Settings[StrHostServerName];
                KeyValueConfigurationElement strHostServerPortValue = rootWebConfig.AppSettings.Settings[StrHostServerPort];

                machine = strMachineValue != null ? strMachineValue.Value : string.Empty;
                k2Server = strK2ServerValue != null ? strK2ServerValue.Value : string.Empty;
                hostServerName = strHostServerNameValue != null ? strHostServerNameValue.Value : string.Empty;
                hostServerPort = strHostServerPortValue != null ? strHostServerPortValue.Value : string.Empty;

                sb.Append("https://" + machine);
                sb.Append("/ViewFlow/ViewFlow.aspx?ViewTypeName=ProcessView&");
                sb.Append(StrK2Server + "=" + k2Server + "&");
                sb.Append(StrHostServerName + "=" + hostServerName + "&");
                sb.Append(StrHostServerPort + "=" + hostServerPort + "&");
                sb.Append(StrProcessId + "=");
            }

            return sb.ToString();
        }

        public WorkflowsController(IWorkflowsService workflowsService,
            ICatalogService catalogService,
            IK2DataService k2DataService,
            IGlobalModelRepository GlobalModelRepository,
            IWorkflowTypeRepository WorkflowTypeRepository,
            IWorkflowModelRepository workflowModelRepository,
            IWorkflowManagerService workflowManager,
            IValidatorService validatorService,
            IWorkflowInstanceTaskRepository workflowInstanceTaskRepository)
        {
            _workflowsService = workflowsService;
            _catalogService = catalogService;
            _viewModelMapperHelper = new ViewModelMapperHelper(_catalogService);
            _k2DataService = k2DataService;
            _globalModelRepository = GlobalModelRepository;
            _workflowTypeRepository = WorkflowTypeRepository;
            _workflowModelRepository = workflowModelRepository;
            _workflowManager = workflowManager;
            _validatorService = validatorService;
            _workflowInstanceTaskRepository = workflowInstanceTaskRepository;
        }

        public virtual ActionResult Index(string operationNumber)
        {
            Logger.GetLogger().WriteDebug("WorkflowController",
                "Entering index method with operation number " + operationNumber);

            ViewBag.operationNumber = operationNumber;
            ViewBag.K2ViewDetailURL = GlobalCommonLogic.GetK2WorkflowViewDetailURL();
            Logger.GetLogger().WriteDebug("WorkflowController",
                "K2 Workflow view detail URL " + ViewBag.K2ViewDetailURL);

            ViewBag.OpDetailURL = GlobalCommonLogic.GetOperationDetailURL();
            Logger.GetLogger().WriteDebug("WorkflowController",
                "K2 operation detail URL " + ViewBag.OpDetailURL);

            ViewBag.lstStatus = new List<SelectListItem>();

            ViewBag.lstStatus.Add(new SelectListItem
            {
                Text = Localization.GetText("Select Status"),
                Value = string.Empty
            });

            Logger.GetLogger().WriteDebug("WorkflowController", "Going to get workflow status");

            foreach (string st in _workflowModelRepository.GetWorkFlowStatus())
            {
                ViewBag.lstStatus.Add(new SelectListItem { Text = st, Value = st });
            }

            ViewBag.lstTypes = new List<SelectListItem>();

            ViewBag.lstTypes.Add(new SelectListItem { Text = Localization.GetText("Select Workflow Type"), Value = string.Empty });

            Logger.GetLogger().WriteDebug("WorkflowController", "Going to get workflow types");
            foreach (var workflowType in _workflowModelRepository.GetWorkFlowTypes())
            {
                ViewBag.lstTypes.Add(new SelectListItem
                {
                    Text = workflowType.Name,
                    Value = workflowType.WorkflowTypeId.ToString()
                });
            }

            Logger.GetLogger().WriteDebug("WorkflowController", "Going to compare the user");
            ViewBag.userName = string.IsNullOrEmpty(operationNumber) ? IDBContext.Current.UserName : null;

            Logger.GetLogger().WriteDebug("WorkflowController", "Returning ...");
            return View();
        }

        public virtual ActionResult New(string operation, 
            string workflowTypeCode = "", 
            string workflowTypeName = "",
            string entityType = "",
            int entityId = 0)
        {
            Logger.GetLogger().WriteDebug("WorkflowController",
                "Method: New | operation number " + operation);
            WorkflowRequest workflowRequest = new WorkflowRequest();
            var validatorsHappyRoad = new List<DetailValidatorModel>();
            var Mapper = new ValidatorsViewModelMapper();
            var validatorTaskStart = new List<ValidatorViewModel>();
            var getHappyRoadResponse = new ResponseBase() { IsValid = false };
            var responseWokflowType = new WorkflowTypeResponse();
            responseWokflowType.WorkflowType = new WorkflowViewModel
            {
                InstructionsIncluded = true,
                CanAddValidator = true,
                Validators = new List<ValidatorViewModel>(),
                UserComments = new List<UserCommentViewModel>(),
            };

            if (!TempData.ContainsKey("workflowRequest"))
            {
                if (string.IsNullOrEmpty(workflowTypeCode) || 
                    string.IsNullOrEmpty(entityType) || 
                    entityId != 0)
                {
                    workflowRequest.WorkflowCode = workflowTypeCode;
                    workflowRequest.EntityType = entityType;
                    workflowRequest.EntityId = entityId;
                    workflowRequest.ModuleName = K2IntegrationWorkflowConst.External;
                    workflowRequest.Whit2k = false;
                    workflowRequest.OperationNumber = operation;
                    workflowRequest.ReturnURL = Request.UrlReferrer.ToString();
                    workflowRequest.ReturnURLCancel = Request.UrlReferrer.ToString();
                }
                else
                {
                    workflowRequest = new WorkflowRequest()
                    {
                        OperationNumber = operation,
                        WorkflowCode = string.Empty,
                        EntityType = string.Empty,
                        EntityId = 0,
                        ModuleName = string.Empty
                    };
                    ViewData["message"] = new MessageConfiguration()
                    {
                        Message = "workflowRequest is not Contains in TempData",
                        Type = "error",
                        AutoClose = false.ToString(),
                        Duration = "5",
                    };
                    Logger.GetLogger().WriteError("WorkflowController", "workflowRequest is not Contains in TempData", new Exception("workflowRequest is not Contains in TempData"));
                }
            }
            else
            {
                workflowRequest = (WorkflowRequest)TempData["workflowRequest"];
            }

            var workflowType = WorkflowHelper.GetWorkflowTypeByCode(workflowRequest.WorkflowCode);
            if (!workflowType.IsValid)
            {
                workflowRequest = new WorkflowRequest()
                {
                    OperationNumber = operation,
                    WorkflowCode = string.Empty,
                    EntityType = string.Empty,
                    EntityId = 0,
                    ModuleName = string.Empty
                };
                ViewData["message"] = new MessageConfiguration()
                {
                    Message = workflowType.ErrorMessage,
                    Type = "error",
                    AutoClose = false.ToString(),
                    Duration = "5",
                };
                Logger.GetLogger().WriteError("WorkflowController", workflowType.ErrorMessage, new Exception("Workflow Type no found"));
            }
            else
            {
                getHappyRoadResponse = WorkflowHelper.GetHappyRoad(null, workflowRequest.EntityType, operation, 1, workflowType.WorkflowType, ref validatorsHappyRoad);
            }

            if (getHappyRoadResponse.IsValid)
            {
                responseWokflowType = _workflowsService.GetWorkflowType(workflowRequest);
                responseWokflowType.WorkflowType.Validators = Mapper.ConvertHappyRoadValidatorsToViewModel(validatorsHappyRoad);
                var roles = _viewModelMapperHelper.GetListMasterData(CONVERGENCE_ROLES);
                validatorTaskStart.Add(responseWokflowType.WorkflowType.Validators.First());
                ViewBag.RetrieveListRoles = _viewModelMapperHelper.RemoveUsedRole(validatorTaskStart, roles);
                ViewBag.firstTaskName = responseWokflowType.WorkflowType.Validators.First().TaskName;
                ViewBag.SerializedViewModel = IDB.Presentation.MVC4.Helpers.PageSerializationHelper.SerializeObject(responseWokflowType.WorkflowType);
                responseWokflowType.WorkflowType.ContractNumber = workflowRequest.ContractNumber;
            }
            else
            {
                ViewBag.RetrieveListRoles = new List<SelectListItem>();
                ViewBag.firstTaskName = string.Empty;
                ViewBag.SerializedViewModel = string.Empty;

                ViewData["message"] = new MessageConfiguration()
                {
                    Message = getHappyRoadResponse.ErrorMessage,
                    Type = "error",
                    AutoClose = false.ToString(),
                    Duration = "5",
                };
                Logger.GetLogger().WriteError("WorkflowController", workflowType.ErrorMessage, new Exception("Error getting the happy road"));
            }

            return View("WorkflowNew", responseWokflowType.WorkflowType);
        }

        public virtual ActionResult Save(string operation)
        {
            ResponseBase response = new ResponseBase();
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<WorkflowViewModel>(jsonDataRequest.SerializedData);

            try
            {
                viewModel.UpdateWorkFlowViewModel(jsonDataRequest.ClientFieldData);
                var userCommentResponse = _workflowsService.UpdateUserComment(viewModel.UserComments);
                if (!userCommentResponse.IsValid)
                {
                    throw new Exception(userCommentResponse.ErrorMessage);
                }

                viewModel.UserComments = userCommentResponse.UserComments;

                // var workFlow = jsonDataRequest.ClientFieldData.FirstOrDefault(o => o.Name.Equals("changeStatus"));
                var parameters = new Dictionary<string, object>();

                var userName = string.IsNullOrEmpty(viewModel.UserName) ? IDBContext.Current.UserName : viewModel.UserName;
                var currentRole = "K2"; //IDBContext.Current.Roles.FirstOrDefault();
                Logger.GetLogger().WriteDebug("WorkflowController", "Method: Save | UserName: " + userName);

                var listAdditionalValidators = 
                    viewModel.Validators
                    .Where(a => a.Mandatory == false).Select(x => new { x.Role });

                var listValidators = new List<string>();

                foreach (var validator in listAdditionalValidators)
                {
                    listValidators.Add(validator.Role);
                }

                var listAdditionalValidatorsPresent = listAdditionalValidators.Count() == 0 ? false : true;
                var arrayTag = new List<string>();
                arrayTag.Add("\"" + GenericWorkflowHelper.AddCommnets + "\":" + (viewModel.UserComments.Count != 0 ? "\"" + string.Join(",", viewModel.UserComments.Select(uc => uc.CommentId.ToString())) + "\"" : "\"\""));
                arrayTag.Add("\"" + GenericWorkflowHelper.IsfirstTask + "\":" + "\"" + GenericWorkflowHelper.IsfirstTask + "\"");
                arrayTag.Add("\"" + GenericWorkflowHelper.AddValidators + "\":" + (listAdditionalValidatorsPresent ? "\"" + string.Join("|", listValidators) + "\"" : "\"\""));

                if (listAdditionalValidators.Any())
                {
                    parameters.Add("AddValidators", string.Join("|", listValidators));
                }

                parameters.Add("OriginatorUser", userName);
                parameters.Add("OriginatorRole", currentRole);

                parameters.Add("tag", "{" + string.Join(",", arrayTag) + "}");

                //start generic workflow
                if (!viewModel.WhitK2)
                {
                    response = InitialGenericWorkflow(viewModel);
                }
                else
                {
                    var k2Service = Globals.Resolve<IK2IntegrationWorkflow>();
                    response.IsValid = k2Service.AdvanceWorkflow(
                        workflowType: viewModel.WorkflowCode,
                        folioId: string.Empty,
                        operationNumber: viewModel.OperationNumber ?? operation,
                        entityId: viewModel.EntityId,
                        action: "StartWorkflow",
                        parameters: parameters,
                        EntityType: viewModel.EntityType);
                }
            }
            catch (Exception e)
            {
                response.IsValid = false;
                response.ErrorMessage = e.ToString();
            }

            return Json(response);
        }

        [HttpPost]
        public virtual JsonResult Search(FilterModel filter)
        {
            var workflowInstances = _workflowModelRepository.SearchWorkFlows(filter);

            foreach (var workflowInstance in workflowInstances)
            {
                if (!string.IsNullOrWhiteSpace(workflowInstance.ContractNumber))
                {
                    workflowInstance.Loans = workflowInstance.Loans
                        .Where(loan => loan == workflowInstance.ContractNumber)
                        .ToList();
                }
            }

            return this.Json(workflowInstances);
        }

        [HttpPost]
        public virtual JsonResult GetTasksByWorkflowId(int workflowId)
        {
            var changeGroupAssignPermission = IDBContext.Current.HasPermission(Permission.CHANGE_GROUP_ASSIGN);
            return Json(_workflowModelRepository
                .GetTasksByWorkflowIdWithGroupAssignPermission(workflowId, changeGroupAssignPermission));
        }

        [HttpPost]
        public virtual JsonResult GetCommentsByWorkflowId(int workflowId)
        {
            return Json(_workflowModelRepository.GetCommentsByWorkflowId(workflowId));
        }

        [HttpPost]
        public virtual JsonResult GetClauseExtensionAndFinalStatusInfo(int workflowid)
        {
            var clauseInformation = _workflowModelRepository.GetClauseInformation(workflowid);

            if (string.IsNullOrEmpty(clauseInformation.ClauseDescription))
            {
                clauseInformation.ClauseDescription = string.Empty;
            }

            return Json(_workflowModelRepository.GetClauseInformation(workflowid));
        }

        public virtual JsonResult GetRoleList(string filter)
        {
            var response = new GetListItemPermissionResponse();
            var rolesList = _workflowsService.GetRoles(filter);

            response.ListResponse = rolesList.GetRoles.Select(o => new ListItemViewModel
            {
                Select = false,
                Text = o,
                Value = o
            }).ToList();

            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual JsonResult SaveChangeRoles(string newRole, int workflowInstanceTaskId)
        {
            var response = _workflowsService.ChangeRole(newRole, workflowInstanceTaskId);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult Test(string operationNumber)
        {
            ViewBag.operationNumber = operationNumber;

            ViewBag.lstTypes = _workflowTypeRepository.GetByCriteria(x => x.Code.Contains("WF-G-")).Select(o => new SelectListItem
            {
                Selected = false,
                Value = o.NameEn,
                Text = o.Code,
            }).ToList();
            return View();
        }

        [HttpPost]
        public virtual JsonResult TestRequest(WorkflowRequest workflowRequest)
        {
            workflowRequest.ReturnURL = Url.Action("Test", "Workflows", new { area = "Global" });
            workflowRequest.Whit2k = false;
            TempData["workflowRequest"] = workflowRequest;
            return this.Json(new ResponseBase() { IsValid = true });
        }

        private string GetOperationDetailURL()
        {
            string url = Globals.GetSetting("BasePath", null);
            url = url + "/operations/";
            return url;
        }

        private ResponseBase InitialGenericWorkflow(WorkflowViewModel workflowModel)
        {   
            var workflowRequest = WorkflowsHelper.LoadWorkflowRequest(workflowModel);
            var responseRequest = _workflowManager.InitiateWorkflow(workflowRequest);

            return new ResponseBase
            {
                IsValid = responseRequest.IsValid,
                ErrorMessage = responseRequest.ErrorMessage
            };
        }
    }
}
