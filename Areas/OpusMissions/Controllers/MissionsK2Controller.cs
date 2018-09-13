using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.MW.Domain.Session;
using IDB.Architecture.Language;
using IDB.Architecture.Logging;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Application.OPUSModule.Enums.K2;
using IDB.MW.Application.OPUSModule.K2Integration;
using IDB.MW.Application.OPUSModule.Messages.OpusWorkflows;
using IDB.MW.Application.OPUSModule.Services.K2DataService;
using IDB.MW.DomainModel.Entities.Core.K2;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.Presentation.MVC4.Areas.OpusMissions.Models;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.OpusMissions.Controllers
{
    public partial class MissionsK2Controller : MVC4.Controllers.ConfluenceController
    {        
        private readonly IK2IntegrationOpus _k2IntegrationOpus;
        private readonly IK2DataService _k2DataService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICatalogService _catalogService;

        public MissionsK2Controller(IAuthorizationService authorizationService, IK2DataService k2DataService, ICatalogService catalogService, IK2IntegrationOpus k2IntegrationOpus)
        {
            _authorizationService = authorizationService;
            _k2IntegrationOpus = k2IntegrationOpus;
            _k2DataService = k2DataService;
            _catalogService = catalogService;
        }

        public virtual ActionResult TaskDetail(TaskDataModel model)
        {
            var validatorsResponse = _k2DataService.GetValidators(model.Code, model.TaskFolio);
            var commentsResponse = _k2DataService.GetComments(model.WorkflowInstanceId);
            var additionalValidatorsResponse = _catalogService.GetMasterDataListByTypeCode(typeCodes: _k2DataService.AdditionalValidatorsRolesDataType);

            var modelTaskView = new MissionsK2TaskViewModel
            {
                UserComments = commentsResponse.IsValid ? commentsResponse.Comments : null,
                Instructions = Localization.GetText("Pending key multilanguage"),
                TaskDataModel = model,
                UserName = IDBContext.Current.UserName,
                Validators = validatorsResponse.IsValid ? validatorsResponse.Validators : null,
                UserRoleMissions = validatorsResponse.IsValid ? validatorsResponse.Validators.Where(v => v.Order > model.TaskTypeId).Select(v => v.Role).FirstOrDefault() : null
            };

            ViewBag.AdditionalValidators = additionalValidatorsResponse.IsValid ? additionalValidatorsResponse.MasterDataCollection : new List<MasterDataViewModel>();

            ViewBag.UserRole = validatorsResponse.IsValid ? validatorsResponse.Validators.Where(v => v.Order > model.TaskTypeId).Select(v => v.Role).FirstOrDefault() : null;

            ViewBag.UserName = IDBContext.Current.UserName;

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(modelTaskView);

            return View(modelTaskView);
        }

        public virtual ActionResult CreateWorkflow(string operationNumber)
        {
            var workflowType = _k2IntegrationOpus.GetWorkflowType(operationNumber);
            var validatorsResponse = _k2DataService.GetValidators(workflowType, null);

            var modelTaskView = new MissionsK2TaskViewModel
            {
                Instructions = Localization.GetText("Pending key multilanguage"),
                UserName = IDBContext.Current.UserName,
                Validators = validatorsResponse.IsValid ? validatorsResponse.Validators : null
            };

            ViewBag.OperationNumber = operationNumber;

            return View(modelTaskView);
        }

        public virtual JsonResult StartWorkflow(string operationNumber, string userName)
        {
            var response = new StartAdvanceWorkflowResponse();

            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add(K2HelperOpus.TaskUserName, userName);

                var workflowCode = _k2IntegrationOpus.GetWorkflowType(operationNumber);

                response = SendWorkflowAction(workflowCode, "0", operationNumber, null, parameters, K2IntegrationEnumerator.GeneralActions.StartWorkflow, 0);
            }
            catch (Exception e)
            {
                response.IsValid = false;
                response.ErrorMessage = e.ToString();
            }

            return Json(response);
        }

        //TODO: Need to implement logical function for save when flow is validated.
        public virtual JsonResult ValidateTask(
            string workflowType,
            string operationNumber,
            string taskTypeCode,
            string folioId,
            string userName,
            string userRole,
            string workflowComments,
            int taskId)
        {
            var result = new JsonResult();

            ResponseBase response;

            try
            {
                response = _k2DataService.SaveComments(workflowComments, taskId);

                response.IsValid = true;

                var data = new
                {
                    workflowType,
                    operationNumber,
                    taskTypeCode,
                    folioId,
                    userName,
                    userRole,
                    response.IsValid
                };

                result.Data = data;

                result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            }
            catch (Exception e)
            {
                Logger.GetLogger().WriteError("Missions", "Error when validating task", e);
            }

            return result;
        }

        //TODO: Need to implement logical function for save when flow is rejected
        public virtual JsonResult SaveRejectTask(
            string workflowType,
            string operationNumber,
            string taskTypeCode,
            string folioId,
            string userName,
            string userRole,
            string workflowComments,
            int taskId)
        {
            var result = new JsonResult();

            ResponseBase response;

            try
            {
                response = _k2DataService.SaveComments(workflowComments, taskId);

                response.IsValid = true;

                var data = new
                {
                    workflowType,
                    operationNumber,
                    taskTypeCode,
                    folioId,
                    userName,
                    userRole,
                    response.IsValid
                };

                result.Data = data;

                result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            }
            catch (Exception e)
            {
                e.ToString();
            }
            
            return result;
        }

        public virtual JsonResult ApproveTask(string operationNumber, string userRole)
        {
            var response = new StartAdvanceWorkflowResponse();

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<MissionsK2TaskViewModel>(jsonDataRequest.SerializedData);

            var parameters = new Dictionary<string, object>();
            parameters.Add(K2HelperOpus.TaskUserName, viewModel.UserName);
            parameters.Add(K2HelperOpus.UserRole, viewModel.UserRoleMissions);

            try
            {
                response = SendWorkflowAction(
                    viewModel.TaskDataModel.Code,
                    viewModel.TaskDataModel.TaskFolio,
                    operationNumber,
                    viewModel.TaskDataModel.TaskTypeCode,
                    parameters,
                    K2IntegrationEnumerator.GeneralActions.ApproveWorkflow,
                    viewModel.TaskDataModel.TaskId);
            }
            catch (Exception e)
            {
                response.IsValid = false;
                response.ErrorMessage = e.ToString();
            }

            return Json(response);
        }

        public virtual JsonResult RejectTask(string operationNumber, string userRole)
        {
            var response = new StartAdvanceWorkflowResponse();

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<MissionsK2TaskViewModel>(jsonDataRequest.SerializedData);

            var parameters = new Dictionary<string, object>();
            parameters.Add(K2HelperOpus.TaskUserName, viewModel.UserName);
            parameters.Add(K2HelperOpus.UserRole, viewModel.UserRoleMissions);

            try
            {
                response = SendWorkflowAction(
                    viewModel.TaskDataModel.Code,
                    viewModel.TaskDataModel.TaskFolio,
                    operationNumber,
                    viewModel.TaskDataModel.TaskTypeCode,
                    parameters,
                    K2IntegrationEnumerator.GeneralActions.RejectWorkflow,
                    viewModel.TaskDataModel.TaskId);
            }
            catch (Exception e)
            {
                response.IsValid = false;
                response.ErrorMessage = e.ToString();
            }

            return Json(response);
        }

        #region K2Methods

        private StartAdvanceWorkflowResponse SendWorkflowAction(
            string workflowType,
            string folioId,
            string operationNumber,
            string taskTypeCode,
            Dictionary<string, object> parameters,
            K2IntegrationEnumerator.GeneralActions action,
            int taskId)
        {
            var response = new StartAdvanceWorkflowResponse();

            string entityType = K2HelperOpus.entityTypeOpusMission;

            try
            {
                if (action == K2IntegrationEnumerator.GeneralActions.StartWorkflow)
                {
                    response.IsValid = _k2IntegrationOpus.StartAdvanceWorkflowOpus(workflowType, folioId, operationNumber, entityType, taskTypeCode, parameters, action, taskId);
                }
                else if (action == K2IntegrationEnumerator.GeneralActions.ApproveWorkflow)
                {
                    response.IsValid = _k2IntegrationOpus.StartAdvanceWorkflowOpus(workflowType, folioId, operationNumber, entityType, taskTypeCode, parameters, action, taskId);
                }
                else if (action == K2IntegrationEnumerator.GeneralActions.RejectWorkflow)
                {
                    response.IsValid = _k2IntegrationOpus.StartAdvanceWorkflowOpus(workflowType, folioId, operationNumber, entityType, taskTypeCode, parameters, action, taskId);
                }
            }
            catch (Exception e)
            {
                response.IsValid = false;
                response.ErrorMessage = e.ToString();
            }

            return response;
        }

        #endregion
    }
}