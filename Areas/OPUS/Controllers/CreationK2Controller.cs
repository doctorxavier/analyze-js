using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.OPUSModule.Enums.K2;
using IDB.MW.Application.OPUSModule.K2Integration;
using IDB.MW.Application.OPUSModule.Messages.CreationFormService;
using IDB.MW.Application.OPUSModule.Messages.OpusWorkflows;
using IDB.MW.Application.OPUSModule.Services.CreationFormService;
using IDB.MW.Application.OPUSModule.Services.K2DataService;
using IDB.MW.Application.OPUSModule.ViewModels.CreationFormService;
using IDB.MW.DomainModel.Entities.Core.K2;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.Presentation.MVC4.Areas.OPUS.Models;
using IDB.Presentation.MVC4.Helpers;
using IDB.Architecture.Logging;
using IDB.MW.Domain.Entities;
using IDB.MW.Domain.Session;
using IDB.MW.DomainModel.Contracts.Repositories.Core;

namespace IDB.Presentation.MVC4.Areas.OPUS.Controllers
{
    public partial class CreationK2Controller : MVC4.Controllers.ConfluenceController
    {
        #region Fields
        private readonly IK2IntegrationOpus _k2IntegrationOpus;
        private readonly IK2DataService _k2DataService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICatalogService _catalogService;
        private readonly ICreationFormService _creationFormService;
        private readonly IWorkflowTaskRegisterRepository _workflowTaskRegisterRepository;
        private readonly IWorkflowInstanceTaskRepository _workflowInstanceTaskRepository;
        private readonly IWorkflowInstanceRepository _workflowInstanceRepository;
        #endregion

        #region Constructor
        public CreationK2Controller(
            IAuthorizationService authorizationService,
            IK2DataService k2DataService,
            ICatalogService catalogService,
            IK2IntegrationOpus k2IntegrationOpus,
            ICreationFormService creationFormService,
            IWorkflowTaskRegisterRepository workflowTaskRegisterRepository,
            IWorkflowInstanceTaskRepository workflowInstanceTaskRepository,
            IWorkflowInstanceRepository workflowInstanceRepository)
        {
            _authorizationService = authorizationService;
            _k2IntegrationOpus = k2IntegrationOpus;
            _k2DataService = k2DataService;
            _catalogService = catalogService;
            _creationFormService = creationFormService;
            _workflowTaskRegisterRepository = workflowTaskRegisterRepository;
            _workflowInstanceTaskRepository = workflowInstanceTaskRepository;
            _workflowInstanceRepository = workflowInstanceRepository;
        }
        #endregion

        #region Views Block

        public virtual ActionResult TaskDetail(TaskDataModel model)
        {
            var operation = _creationFormService.GetOperation(model.OperationNumber);

            var validators = _k2IntegrationOpus
                .GetRoleByOperationTypeVM(
                    model.UserProfile,
                    model.TaskTypeCode.Substring(0, 9),
                    operation.OperationData.OperationType.Code,
                    model.OperationNumber);

            if (!validators.IsValid)
            {
                return View(new CreationK2TaskViewModel
                {
                    Validators = validators.Models.ToList(),
                    Instructions = string.Empty,
                    OperationNumber = model.OperationNumber
                });
            }

            var commentsResponse = _k2DataService.GetCommentsCreation(model.OperationNumber);

            var modelTaskView = new CreationK2TaskViewModel
            {
                UserComments = commentsResponse.IsValid ? commentsResponse.CommentList : null,
                Instructions = Localization.GetText("OP.CR.K2.ApproveTaskWorkflow.Instructions"),
                TaskDataModel = model,
                UserName = IDBContext.Current.UserName,
                Validators = validators.Models.ToList(),
                UserRoleCreation = validators.Models.Where(v => v.Order > model.TaskTypeId)
                    .Select(v => v.Role).FirstOrDefault(),
                OperationNumber = model.OperationNumber
            };

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(modelTaskView);

            return View(modelTaskView);
        }

        public virtual ActionResult CreateWorkflow(string operationNumber)
        {
            var workflowType = _k2IntegrationOpus.GetWorkflowType(operationNumber);

            var operation = _creationFormService.GetOperation(operationNumber);

            var validators = _k2IntegrationOpus
                .GetRoleByOperationTypeVM(
                    string.Empty,
                    workflowType,
                    operation.OperationData.OperationType.Code,
                    operationNumber);

            if (!validators.IsValid)
            {
                return View(new CreationK2TaskViewModel
                {
                    Validators = validators.Models.ToList(),
                    Instructions = string.Empty,
                    OperationNumber = operationNumber
                });
            }

            var commentsCreation = _k2DataService.GetCommentsCreation(operationNumber);

            var modelTaskView = new CreationK2TaskViewModel
            {
                UserComments = commentsCreation.IsValid ? commentsCreation.CommentList : null,
                Instructions = Localization.GetText("OP.CR.K2.StartWorkflow.Instructions"),
                UserName = IDBContext.Current.UserName,
                Validators = validators.Models.ToList(),
                OperationNumber = operationNumber
            };

            var existsWorkflowTaskRegister =
                _workflowTaskRegisterRepository.Any(o => o.OperationNumber.Equals(operationNumber));

            if (!existsWorkflowTaskRegister)
            {
                foreach (var validator in validators.Models)
                {
                    var newWorkflowTaskRegister = new WorkflowTaskRegister
                    {
                        OperationNumber = operationNumber,
                        StatusName = validator.Status,
                        UserProfile = validator.Role
                    };

                    _workflowTaskRegisterRepository.Create(newWorkflowTaskRegister);
                }
            }

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(modelTaskView);

            return View(modelTaskView);
        }

        #endregion

        #region processWorkFlow

        public virtual JsonResult ProcessWorkFlow(string operationNumber)
        {
            Logger.GetLogger().WriteDebug("CreationK2Controller - ProcessWorkflow",
                "Going to process workflow with operation:" + operationNumber);

            UpdateStateOperationResponse response = new UpdateStateOperationResponse();

            var responseCheck = _creationFormService.CheckFundAmountsByOperationType(operationNumber);

            if (!responseCheck.IsValid)
                return Json(responseCheck);

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<CreationK2TaskViewModel>(jsonDataRequest.SerializedData);

            response.IsRegistred = false;

            try
            {
                Logger.GetLogger().WriteDebug("CreationK2Controller - ProcessWorkflow",
                    "Going to UpdateCreationWorkFlowViewModel ...");

                viewModel.UpdateCreationWorkFlowViewModel(jsonDataRequest.ClientFieldData);

                Logger.GetLogger().WriteDebug("CreationK2Controller - ProcessWorkflow",
                    "Going to UpdateCreationWorkFlowViewModel ... Done!");

                Logger.GetLogger().WriteDebug("CreationK2Controller - ProcessWorkflow",
                    "Going to UpdateCreationK2 ...");

                response.IsValid = _creationFormService.UpdateCreationK2(viewModel).IsValid;

                Logger.GetLogger().WriteDebug("CreationK2Controller - ProcessWorkflow",
                    "Going to UpdateCreationK2 ... Done!");

                var workFlow = jsonDataRequest.ClientFieldData
                    .FirstOrDefault(o => o.Name.Equals("changeStatus"));

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add(K2HelperOpus.TaskUserName, viewModel.UserName);

                StartAdvanceWorkflowResponse responseWF = new StartAdvanceWorkflowResponse();

                Logger.GetLogger().WriteDebug("CreationK2Controller - ProcessWorkflow",
                    string.Format(
                        "Okay, now going to {0}; User name: {1}",
                        workFlow.Value,
                        viewModel.UserName));

                switch (workFlow.Value)
                {
                    case "StartWorkflow":

                        var workflowCode =
                            _k2IntegrationOpus.GetWorkflowType(viewModel.OperationNumber);

                        Logger.GetLogger().WriteDebug("CreationK2Controller - ProcessWorkflow",
                            "Going to SendWorkflowAction ...");

                        responseWF = SendWorkflowAction(
                            workflowCode,
                            "0",
                            viewModel.OperationNumber,
                            null,
                            parameters,
                            K2IntegrationEnumerator.GeneralActions.StartWorkflow,
                            0);

                        Logger.GetLogger().WriteDebug("CreationK2Controller - ProcessWorkflow",
                            "Going to SendWorkflowAction ... Done! Response: " + responseWF.IsValid);

                        if (!responseWF.IsValid)
                        {
                            throw new ApplicationException(responseWF.ErrorMessage);
                        }

                        response.IsValid = true;
                        return Json(response);

                    case "ApproveWorkflow":

                        Logger.GetLogger().WriteDebug("CreationK2Controller - ProcessWorkflow",
                            "Going to SendWorkflowAction ...");

                        responseWF = SendWorkflowAction(
                            viewModel.TaskDataModel.Code,
                            viewModel.TaskDataModel.TaskFolio,
                            viewModel.OperationNumber,
                            viewModel.TaskDataModel.TaskTypeCode,
                            parameters,
                            K2IntegrationEnumerator.GeneralActions.ApproveWorkflow,
                            viewModel.TaskDataModel.TaskId);

                        Logger.GetLogger().WriteDebug("CreationK2Controller - ProcessWorkflow",
                            "Going to SendWorkflowAction ... Done! Response: " + responseWF.IsValid);

                        if (!responseWF.IsValid)
                            throw new ApplicationException(responseWF.ErrorMessage);

                        var stateOperation = _creationFormService.StateOpertion(
                            operationNumber,
                            true,
                            viewModel.TaskDataModel.TaskTypeCode);

                        if (!stateOperation.IsValid)
                            throw new ApplicationException(stateOperation.ErrorMessage);

                        response.IsRegistred = stateOperation.IsRegistred;
                        response.OperationNumber = stateOperation.OperationNumber;
                        response.IsValid = true;
                        response.IsValidSap = stateOperation.IsValidSap;
                        response.MessageCreation = stateOperation.MessageCreation;

                        if (string.IsNullOrEmpty(response.OperationNumber))
                            return Json(response);

                        Logger.GetLogger().WriteDebug(
                            "CreationK2Controller - ProcessWorkflow", string.Format(
                                "Going to SendWorkflowAction in approval, 2nd time, operation: {0} ...",
                                    response.OperationNumber));

                        _k2IntegrationOpus.FinishAsyncServerEvent(
                            viewModel.TaskDataModel.TaskFolio,
                            stateOperation.OperationNumber);

                        Logger.GetLogger().WriteDebug("CreationK2Controller - ProcessWorkflow",
                            "Going to SendWorkflowAction ... Done! Response: " + responseWF.IsValid);

                        return Json(response);

                    case "RejectWorkflow":

                        Logger.GetLogger().WriteDebug("CreationK2Controller - ProcessWorkflow",
                            "Going to SendWorkflowAction ...");

                        responseWF = SendWorkflowAction(
                            viewModel.TaskDataModel.Code,
                            viewModel.TaskDataModel.TaskFolio,
                            viewModel.OperationNumber,
                            viewModel.TaskDataModel.TaskTypeCode,
                            parameters,
                            K2IntegrationEnumerator.GeneralActions.RejectWorkflow,
                            viewModel.TaskDataModel.TaskId);

                        Logger.GetLogger().WriteDebug("CreationK2Controller - ProcessWorkflow",
                            "Going to SendWorkflowAction ... Done! Response: " + responseWF.IsValid);

                        if (!responseWF.IsValid)
                            throw new ApplicationException(responseWF.ErrorMessage);

                        response.IsValid = true;
                        return Json(response);
                }
            }
            catch (Exception e)
            {
                response.IsValid = false;
                response.ErrorMessage = e.ToString();
                return Json(response);
            }

            Logger.GetLogger().WriteDebug("CreationK2Controller - ProcessWorkflow",
                "Exiting with response: " + response.IsValid);

            return Json(response);
        }

        #endregion

        #region K2 private Methods

        private StartAdvanceWorkflowResponse SendWorkflowAction(
            string workflowType,
            string folioId,
            string operationNumber,
            string taskTypeCode,
            Dictionary<string, object> parameters,
            K2IntegrationEnumerator.GeneralActions action,
            int taskId)
        {
            Logger.GetLogger().WriteDebug("CreationK2Controller - SendWorkflowAction",
                string.Format("Enter method for operation: {0}", operationNumber));

            var response = new StartAdvanceWorkflowResponse();

            string entityType = K2HelperOpus.EntityTypeOpusCreation;

            try
            {
                if (action == K2IntegrationEnumerator.GeneralActions.StartWorkflow)
                {
                    Logger.GetLogger().WriteDebug("CreationK2Controller - SendWorkflowAction",
                        "Start workflow ...");

                    response.IsValid = _k2IntegrationOpus.StartAdvanceWorkflowOpus(
                        workflowType,
                        folioId,
                        operationNumber,
                        entityType,
                        taskTypeCode,
                        parameters,
                        action,
                        taskId);

                    Logger.GetLogger().WriteDebug("CreationK2Controller - SendWorkflowAction",
                        "Start workflow ... Done! Response: " + response.IsValid);

                    return response;
                }

                if (action == K2IntegrationEnumerator.GeneralActions.ApproveWorkflow)
                {
                    Logger.GetLogger().WriteDebug("CreationK2Controller - SendWorkflowAction",
                        "Approve workflow ...");

                    response.IsValid = _k2IntegrationOpus.StartAdvanceWorkflowOpus(
                        workflowType,
                        folioId,
                        operationNumber,
                        entityType,
                        taskTypeCode,
                        parameters,
                        action,
                        taskId);

                    Logger.GetLogger().WriteDebug("CreationK2Controller - SendWorkflowAction",
                        "Approve workflow ... Done! Response: " + response.IsValid);

                    return response;
                }

                Logger.GetLogger().WriteDebug("CreationK2Controller - SendWorkflowAction",
                        "Reject workflow ...");

                response.IsValid = _k2IntegrationOpus.StartAdvanceWorkflowOpus(
                    workflowType,
                    folioId,
                    operationNumber,
                    entityType,
                    taskTypeCode,
                    parameters,
                    action,
                    taskId);

                Logger.GetLogger().WriteDebug("CreationK2Controller - SendWorkflowAction",
                        "Reject workflow ... Done! Response: " + response.IsValid);

                return response;
            }
            catch (Exception e)
            {
                response.IsValid = false;
                response.ErrorMessage = e.ToString();

                return response;
            }
        }

        #endregion
    }
}