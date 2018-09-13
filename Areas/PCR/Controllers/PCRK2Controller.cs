using System.Web.Mvc;

using IDB.MW.Application.PCRModule.Services.ChecklistService;
using IDB.MW.Application.PCRModule.Services.WorkflowIntegrationService;
using IDB.MW.Application.PCRModule.ViewModels.WorkflowIntegrationService;
using IDB.MW.DomainModel.Entities.Core.K2;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVC4.Areas.PCR.Models;
using IDB.Presentation.MVC4.Controllers;
using IDB.MW.Domain.Session;
using IDB.MW.Application.PCRModule.Messages.ChecklistService;

namespace IDB.Presentation.MVC4.Areas.PCR.Controllers
{
    public partial class PCRK2Controller : BaseController
    {
        #region Fields
        private readonly IK2UIIntegrationService _k2UIIntegrationService;
        private readonly IPCRChecklistService _pcrChecklistService;
        #endregion

        #region Contructors
        public PCRK2Controller(IK2UIIntegrationService k2UIIntegrationService, IPCRChecklistService pcrChecklistService)
        {
            _k2UIIntegrationService = k2UIIntegrationService;
            _pcrChecklistService = pcrChecklistService;
        }
        #endregion

        public virtual ActionResult Index(TaskDataModel model)
        {
            var response = _k2UIIntegrationService.GetTaskData(model);

            if (response.PCRWorkflowTaskViewModel == null)
            {
                response.PCRWorkflowTaskViewModel = new PCRWorkflowTaskViewModel();
            }

            response.PCRWorkflowTaskViewModel.Instructions = "PCR.K2.Instructions.Task" + model.TaskTypeId;
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(response.PCRWorkflowTaskViewModel);

            if (IDBContext.Current.HasRole(Role.SPD_COORDINATOR) || IDBContext.Current.HasRole(Role.SPD_SPECIALIST))
            {
                response.PCRWorkflowTaskViewModel.IsSPDPermission = true;
            }

            return View("TaskDetails", response.PCRWorkflowTaskViewModel);
        }

        public virtual JsonResult SaveWorkflowComment()
        {
            var result = new JsonResult();

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);

            var viewModel = PageSerializationHelper.DeserializeObject<PCRWorkflowTaskViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateWorkflowTask(jsonDataRequest.ClientFieldData);

            var response = _k2UIIntegrationService.CreateWorkflowComment(viewModel);

            result.Data = new
            {
                response.IsValid,
                response.ErrorMessage
            };
            return result;
        }

        public virtual JsonResult SubmitTask()
        {
            var result = new JsonResult();

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<PCRWorkflowTaskViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateWorkflowTask(jsonDataRequest.ClientFieldData);

            var responseComment = _k2UIIntegrationService.CreateWorkflowComment(viewModel);

            if (!responseComment.IsValid)
            {
                result.Data = new
                {
                    responseComment.IsValid,
                    responseComment.ErrorMessage
                };
                return result;
            }

            var response = _pcrChecklistService.ChangeStage(viewModel.OperationNumber, viewModel.WorkflowInstanceTaskId, viewModel.TaskTypeId, viewModel.FolioId);

            result.Data = new
            {
                response.IsValid,
                response.ErrorMessage
            };

            return result;
        }

        public virtual JsonResult RejectChecklist()
        {
            var result = new JsonResult();

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<PCRWorkflowTaskViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateWorkflowTask(jsonDataRequest.ClientFieldData);

            var responseComment = _k2UIIntegrationService.CreateWorkflowComment(viewModel);

            if (!responseComment.IsValid)
            {
                result.Data = new
                {
                    responseComment.IsValid,
                    responseComment.ErrorMessage
                };
                return result;
            }

            var response = _pcrChecklistService.RejectChecklist(
                viewModel.OperationNumber,
                viewModel.WorkflowInstanceTaskId,
                viewModel.TaskTypeId,
                viewModel.FolioId,
                viewModel.CommentReject);

            result.Data = new
            {
                response.IsValid,
                response.ErrorMessage
            };

            return result;
        }

        public virtual JsonResult RedoChecklist(
            string operationNumber, int idPCRPoint, int idStageRedo, int numberTaskRedo)
        {
            var result = new JsonResult();
            var responseRedo = new RedoChecklistResponse()
            {
                IdPCRPoint = idPCRPoint,
                IdStageRedo = idStageRedo,
                NumberTaskRedo = numberTaskRedo
            };

            var response = _pcrChecklistService.RedoChecklist(operationNumber, responseRedo);

            result.Data = new
            {
                response.IsValid,
                response.ErrorMessage
            };

            return result;
        }

        public virtual JsonResult ValidateTask(string operationNumber, int workflowTaskId, int taskTypeId, string folioId)
        {
            var result = new JsonResult();
            var response = _pcrChecklistService.ChangeStage(operationNumber, workflowTaskId, taskTypeId, folioId, true);

            result.Data = new
            {
                response.IsValid,
                response.ErrorMessage,
                response.PCRId
            };

            return result;
        }
    }
}