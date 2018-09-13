using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

using Newtonsoft.Json;

using IDB.Architecture.Language;
using IDB.Architecture.Logging;
using IDB.MW.Application.OPUSModule.Services.K2DataService;
using IDB.MW.Application.TCM.Enums.K2IntegrationEnums;
using IDB.MW.Application.TCM.Helpers;
using IDB.MW.Application.TCM.Services.CommentsTaskService;
using IDB.MW.Application.TCM.Services.K2IntegrationService;
using IDB.MW.Application.TCM.Services.TcmUniverseService;
using IDB.MW.Application.TCM.ViewModels.K2Workflow;
using IDB.MW.Business.TCAbstractModule.Contracts;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.Domain.Values.Workflows.General;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.MW.DomainModel.Entities.Core.K2;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.Presentation.MVC4.Areas.TCM.Models;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.TCM.Controllers
{
    public partial class WorkflowK2Controller : BaseController
    {
        #region Fields
        private const string ACCEPT_WORKFLOW = "AcceptWorkflow";
        private const string REJECT_WORKFLOW = "RejectWorkflow";
        private const string TCM_REJECTED_ALL = "Rejected by ";
        private const string SEPARATOR = ",";

        private readonly IK2DataService _k2DataService;
        private readonly IK2IntegrationService _k2IntegrationService;
        private readonly ICommentsTaskService _commentsTaskService;
        private readonly IFundsSharepointService _fundsSharepointService;
        private readonly IOperationRepository _operationRepository;
        private readonly ITcmUniverseService _tcmService;
        private readonly IWorkflowInstanceEntityRepository _workflowInstanceEntityRepository;
        private readonly IWorkflowInstanceTaskRepository _workflowInstanceTaskRepository;

        #endregion

        #region Contructors
        public WorkflowK2Controller(
            IK2DataService k2DataService,
            IK2IntegrationService k2IntegrationService,
            ICommentsTaskService commentsTaskService,
            IFundsSharepointService fundsSharepointService,
            IOperationRepository opRep,
            ITcmUniverseService tcmServ,
            IWorkflowInstanceEntityRepository workflowInstanceEntityRepository,
            IWorkflowInstanceTaskRepository workflowInstanceTaskRepository)
        {
            _k2DataService = k2DataService;
            _k2IntegrationService = k2IntegrationService;
            _commentsTaskService = commentsTaskService;
            _fundsSharepointService = fundsSharepointService;
            _operationRepository = opRep;
            _tcmService = tcmServ;
            _workflowInstanceEntityRepository = workflowInstanceEntityRepository;
            _workflowInstanceTaskRepository = workflowInstanceTaskRepository;
        }
        #endregion

        public virtual ActionResult TaskDetailWorkflow(string operationNumber, TaskDataModel model)
        {
            Logger.GetLogger().WriteDebug(
                "WorkflowK2Controller",
                "Method: TaskDetailWorkflow - Parameters( operationNumber: " + operationNumber + ")");

            object serialNro;
            object isFinalReport;

            if (model.Parameters != null)
            {
                model.Parameters.TryGetValue(K2TCMHelpers.SerialNro, out serialNro);
                model.Parameters.TryGetValue(K2TCMHelpers.isFinalReport, out isFinalReport);
            }
            else
            {
                serialNro = "0";
                isFinalReport = false;
            }

            var commentsResponse = _commentsTaskService.GetComments(model.EntityId);
            var modelTaskView = new WorkflowViewModels
            {
                UserComments = commentsResponse.IsValid ? commentsResponse.Comments : null,
                CountComments =
                    commentsResponse.Comments.Count > 0 ? commentsResponse.Comments.Count : 0,
                Instructions = Convert.ToBoolean(isFinalReport) ?
                    Localization.GetText("TCM.TR.TaskAndRequestFoundCoordinator.ContentInstructions") +
                        "<br>" +
                        Localization.GetText("TCM.TR.TaskAndRequestFoundCoordinator.ContentInstructions.IsFinalReport") :
                    Localization.GetText("TCM.TR.TaskAndRequestFoundCoordinator.ContentInstructions"),
                TaskDataModel = model,
                UserName = IDBContext.Current.UserName,
                ResultMatrixId = model.EntityId,
                SerialNro = Convert.ToString(serialNro),
                IsFinalReport = Convert.ToBoolean(isFinalReport)
            };

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(modelTaskView);

            Logger.GetLogger().WriteDebug("WorkflowK2Controller", "Method: TaskDetailWorkflow - Final");

            return View(modelTaskView);
        }

        public virtual JsonResult SaveWorkFlowTCM()
        {
            Logger.GetLogger().WriteDebug(
                "WorkflowK2Controller", "Method: SaveWorkFlowTCM()  -  Start");

            ResponseBase response = new ResponseBase();
            string operationNumber = IDBContext.Current.Operation;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<WorkflowViewModels>(jsonDataRequest.SerializedData);

            try
            {
                viewModel.UpdateCreationWorkFlowViewModel(jsonDataRequest.ClientFieldData);
                response = _commentsTaskService.SaveCommentWorkFlowTCM(viewModel);

                if (viewModel.TaskDataModel != null)
                {
                    var parameters = new Dictionary<string, object>();
                    string userName = IDBContext.Current.UserName;

                    Logger.GetLogger().WriteDebug(
                        "WorkflowK2Controller", "Method: SaveWorkFlowTCM - userName: " + userName);

                    parameters.Add(K2TCMHelpers.TaskUserName, userName);

                    var resultMatrixId = jsonDataRequest.ClientFieldData
                        .FirstOrDefault(o => o.Name.Equals("ResultMatrixId"));
                    int entityId = Convert.ToInt32(resultMatrixId.Value);

                    Logger.GetLogger().WriteDebug(
                        "WorkflowK2Controller", "Method: SaveWorkFlowTCM - entityId: " + entityId);

                    string serialNro = viewModel.SerialNro;

                    Logger.GetLogger().WriteDebug(
                        "WorkflowK2Controller", "Method: SaveWorkFlowTCM - serialNro: " + serialNro);

                    parameters.Add(K2TCMHelpers.SerialNro, serialNro);

                    Logger.GetLogger().WriteDebug(
                        "WorkflowK2Controller", "Method: SaveWorkFlowTCM - TaskRol: " +
                            viewModel.TaskDataModel.UserProfile);

                    parameters.Add(K2TCMHelpers.TaskRole, viewModel.TaskDataModel.UserProfile);

                    var workFlow = jsonDataRequest.ClientFieldData
                        .FirstOrDefault(o => o.Name.Equals("changeStatus"));

                    parameters.Add(K2TCMHelpers.GeneralTag, viewModel.TaskDataModel.TaskTag);
                    Logger.GetLogger().WriteDebug(
                        "WorkflowK2Controller",
                        "Method: SaveWorkFlowTCM - TaskTag: " + viewModel.TaskDataModel.TaskTag);

                    Logger.GetLogger().WriteDebug(
                        "WorkflowK2Controller", "Method: SaveWorkFlowTCM - workFlow: " + workFlow.Value);

                    switch (workFlow.Value)
                    {
                        case ACCEPT_WORKFLOW:
                            response.IsValid = _k2IntegrationService.StartAdvanceWorkflowTCM(
                                viewModel.TaskDataModel.Code,
                                                    viewModel.TaskDataModel.TaskFolio,
                                                    operationNumber,
                                                    K2TCMHelpers.EntityType,
                                                    entityId,
                                                    viewModel.TaskDataModel.TaskTypeCode,
                                                    parameters,
                                                    K2IntegrationEnumerator.GeneralActions.AcceptWorkflow,
                                                    viewModel.TaskDataModel.TaskId);

                            if (response.IsValid)
                            {
                                var result = _k2IntegrationService
                                    .IsTaskAccepted(viewModel.TaskDataModel.WorkflowInstanceId);

                                Logger.GetLogger().WriteDebug(
                                    "WorkflowK2Controller",
                                    "Method: SaveWorkFlowTCM - isAllTasksAccepted: " +
                                        result.isAllTasksAccepted);

                                if (result.isAllTasksAccepted)
                                    response = _k2IntegrationService.ChangeStatusResultMatrix(
                                        entityId,
                                        TCMGlobalValues.TCM_STAGE_FUN_COOR,
                                        false,
                                        true,
                                        null);
                                }

                            break;

                        case REJECT_WORKFLOW:
                            response.IsValid = _k2IntegrationService.StartAdvanceWorkflowTCM(
                                viewModel.TaskDataModel.Code,
                                                    viewModel.TaskDataModel.TaskFolio,
                                                    operationNumber,
                                                    K2TCMHelpers.EntityType,
                                                    entityId,
                                                    viewModel.TaskDataModel.TaskTypeCode,
                                                    parameters,
                                                    K2IntegrationEnumerator.GeneralActions.RejectWorkflow,
                                                    viewModel.TaskDataModel.TaskId);

                            if (response.IsValid)
                                response = _k2IntegrationService.ChangeStatusOthersTasks(
                                    viewModel.TaskDataModel, TCM_REJECTED_ALL);

                            if (response.IsValid)
                                response = _k2IntegrationService.ChangeStatusResultMatrix(
                                    entityId, TCMGlobalValues.TCM_STAGE_FUN_DRAFT, false, false, null);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                response.IsValid = false;
                response.ErrorMessage = e.ToString();
                Logger.GetLogger().WriteError("WorkflowK2Controller", e.Message, e);
            }

            Logger.GetLogger().WriteDebug("WorkflowK2Controller", "Method: SaveWorkFlowTCM Final");

            return Json(response);
        }

        public virtual ActionResult SubmitWorkflow(string resultMatrixId, bool isFinalReport)
        {
            Logger.GetLogger().WriteDebug(
                "WorkflowK2Controller",
                "Method: SubmitWorkflow - Parameters( resultMatrixId: " + resultMatrixId + ")");

            var commentsResponse = _commentsTaskService.GetComments(Convert.ToInt32(resultMatrixId));
            var modelTaskView = new WorkflowViewModels
            {
                UserComments = commentsResponse.IsValid ? commentsResponse.Comments : null,
                CountComments = commentsResponse.Comments.Count > 0 ? commentsResponse.Comments.Count : 0,
                Instructions = Convert.ToBoolean(isFinalReport)
                ? Localization.GetText("TCM.TR.TaskAndRequestFoundCoordinator.SubmitInstructions.IsFinalReport")
                : Localization.GetText("TCM.TR.TaskAndRequestFoundCoordinator.SubmitInstructions"),
                TaskDataModel = null,
                UserName = IDBContext.Current.UserName,
                ResultMatrixId = Convert.ToInt32(resultMatrixId),
                IsFinalReport = Convert.ToBoolean(isFinalReport)
            };

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(modelTaskView);

            Logger.GetLogger().WriteDebug("WorkflowK2Controller", "Method: SubmitWorkflow - Final");

            return View("TaskDetailWorkflow", modelTaskView);
        }

        public virtual JsonResult StartWorkflowTCM(string resultMatrixId, bool isMap, bool isFinalReport)
        {
            Logger.GetLogger().WriteDebug(
                "WorkflowK2Controller",
                "Method: StartWorkflowTCM - Parameters( resultMatrixId: " + resultMatrixId + ")");

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<WorkflowViewModels>(jsonDataRequest.SerializedData);
            string TCMWF = K2TCMHelpers.WorkflowTypeTCM1;

            if (isMap)
                TCMWF = K2TCMHelpers.WorkflowTypeTCM2;

            var response = new ResponseBase();
            var parameters = new Dictionary<string, object>();
            var arrayTag = new List<string>();
            int entityId = 0;
            string operationNumber = IDBContext.Current.Operation;
            string userName = IDBContext.Current.UserName;

            Logger.GetLogger().WriteDebug(
                "WorkflowK2Controller", "Method: StartWorkflowTCM - userName: " + userName);

            parameters.Add(K2TCMHelpers.EntityId, Convert.ToString(resultMatrixId));

            Logger.GetLogger().WriteDebug(
                "WorkflowK2Controller",
                "Method: StartWorkflowTCM - entityId: " + Convert.ToString(resultMatrixId));

            entityId = Convert.ToInt32(resultMatrixId);
            arrayTag.Add("\"IsFinalReport\":\"" + Convert.ToString(isFinalReport) + "\"");
            parameters.Add(K2TCMHelpers.GeneralTag, "{" + string.Join(",", arrayTag) + "}");

            var sharepointUrl = ConfigurationManager.AppSettings["BasePath"];
            var linkOpera = string.Format(
                "{0}/operation/{1}/Pages/Default?idTask=nro",
                sharepointUrl,
                operationNumber);

            parameters.Add(K2TCMHelpers.LinkTask, linkOpera);

            string fundId = string.Empty;
            string fcUserName = string.Empty;
            string fundCode = string.Empty;

            var fundsResponse = _tcmService.CalculateFunds(operationNumber);

            if (!fundsResponse.IsValid)
            {
                Exception e = new Exception("Could not calculate funds");

                Logger.GetLogger()
                    .WriteError("WorkflowK2Controller - StartWorkflowTCM", e.Message, e);

                throw e;
            }

            var fundOperations = fundsResponse.Models;

            foreach (var fund in fundOperations)
            {
                var fundUserInfo = _fundsSharepointService
                    .GetFundCoordinatorByCode(fund.Fund.FundCode);

                if (!string.IsNullOrEmpty(fundId))
                {
                    fundId += SEPARATOR;
                    fundCode += SEPARATOR;
                    fcUserName += SEPARATOR;
                }

                fundId += fund.Fund.FundId.ToString();
                fundCode += fund.Fund.FundCode;
                fcUserName += fundUserInfo != null ? fundUserInfo.LoginName : string.Empty;
            }

            Logger.GetLogger().WriteDebug(
                "WorkflowK2Controller",
                "Method: StartWorkflowTCM - IdsFund: " + Convert.ToString(fundId));
            Logger.GetLogger().WriteDebug(
                "WorkflowK2Controller",
                "Method: StartWorkflowTCM - CodeFund: " + Convert.ToString(fundCode));
            Logger.GetLogger().WriteDebug(
                "WorkflowK2Controller",
                "Method: StartWorkflowTCM - FundUsers_Temp: " + Convert.ToString(fcUserName));

            var operationNameResponse = _k2IntegrationService.getOperationNamebyRM(entityId);

            parameters.Add(
                K2TCMHelpers.OperationName,
                operationNameResponse.IsValid ? operationNameResponse.operationName : string.Empty);
            parameters.Add(K2TCMHelpers.UserStart, userName);
            parameters.Add(K2TCMHelpers.IdsFund, fundId);
            parameters.Add(K2TCMHelpers.CodeFund, fundCode);
            parameters.Add(K2TCMHelpers.FundUsers_Temp, fcUserName);

            try
            {
                viewModel.UpdateCreationWorkFlowViewModel(jsonDataRequest.ClientFieldData);
                response = _commentsTaskService.SaveCommentWorkFlowTCM(viewModel);

                if (response.IsValid)
                {
                    response.IsValid = _k2IntegrationService.StartAdvanceWorkflowTCM(
                        TCMWF,
                                                        "0",
                                                        operationNumber,
                                                        K2TCMHelpers.EntityType,
                                                        entityId,
                                                        null,
                                                        parameters,
                                                        K2IntegrationEnumerator.GeneralActions.StartWorkflow,
                                                        0);

                    if (response.IsValid)
                        response = _k2IntegrationService.ChangeStatusResultMatrix(
                            entityId, TCMGlobalValues.TCM_STAGE_TL_SUBM, true, false, isFinalReport);
                    }
                }
            catch (Exception e)
            {
                response.IsValid = false;
                response.ErrorMessage = e.ToString();
                Logger.GetLogger().WriteError("WorkflowK2Controller", e.Message, e);
            }

            Logger.GetLogger().WriteDebug("WorkflowK2Controller", "Method: StartWorkflowTCM - Final");

            return Json(response);
        }

        [HttpPost]
        public virtual ActionResult SaveIsFinalReportCheckbox(
            int resultsMatrixId, bool isFinalReportChecked)
        {
            var response = new ResponseBase();

            try
            {
                response = _k2IntegrationService.ChangeStatusResultMatrix(
                    resultsMatrixId,
                    TCMGlobalValues.TCM_STAGE_TL_SUBM,
                    false,
                    false,
                    isFinalReportChecked);

                if (!response.IsValid)
                {
                    throw new Exception("Could not retrieve status of results matrix");
                }

                var workflowInstanceEntity = _workflowInstanceEntityRepository
                    .GetByCriteria(x =>
                        x.EntityId == resultsMatrixId &&
                        x.EntityType == K2TCMHelpers.EntityType &&
                        x.WorkflowInstance.WorkflowType.Code == K2TCMHelpers.WorkflowTypeTCM1);

                if (workflowInstanceEntity == null)
                    return Json(response);

                var workflowInstanceTask = workflowInstanceEntity
                    .Where(x => 
                        x.WorkflowInstance.Status != WorkflowStatus.Cancelled ||
                        x.WorkflowInstance.Status != WorkflowStatus.Rejected)
                    .SelectMany(x => x.WorkflowInstance.WorkflowInstanceTasks)
                    .Where(x =>
                        x.WorkflowTaskType.TaskCode == K2TCMHelpers.WF_TCM_001_T1 &&
                        x.Status != WorkflowStatus.Rejected)
                    .OrderByDescending(d => d.Created)
                    .FirstOrDefault();

                if (workflowInstanceTask == null || string.IsNullOrEmpty(workflowInstanceTask.Tag))
                    return Json(response);

                dynamic data = JsonConvert.DeserializeObject(workflowInstanceTask.Tag);
                data.IsFinalReport = isFinalReportChecked;
                workflowInstanceTask.Tag = JsonConvert.SerializeObject(data);
                _workflowInstanceTaskRepository.Update(workflowInstanceTask);

                return Json(response);
            }
            catch (Exception e)
            {
                response.IsValid = false;
                response.ErrorMessage = e.ToString();
                Logger.GetLogger().WriteError("WorkflowK2Controller", e.Message, e);

                return Json(response);
            }
        }
    }
}