using System;
using System.Linq;
using System.Web.Mvc;
using IDB.MW.Application.VERModule.Messages.Request;
using IDB.Presentation.MVC4.Areas.VER.Models;
using IDB.Presentation.MVC4.Controllers;
using IDB.MW.DomainModel.Entities.Core.K2;
using IDB.MW.Application.VERModule.Services.Tasks;
using IDB.MW.Application.VERModule.ViewModels;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values.Ver;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.VER.Controllers
{
    public partial class TasksController : BaseController
    {
        #region Fields
        private readonly IVerTasksService _verTaskService;

        #endregion

        #region Constructors

        public TasksController(IVerTasksService verTaskService)
        {
            _verTaskService = verTaskService;
        }
        #endregion

        public virtual ActionResult VERTask(string operationNumber, TaskDataModel model)
        {
            var commentRequest = new CommentsTaskRequest
            {
                TaskId = model.TaskId
            };

            var commentsResponse = _verTaskService.GetComments(commentRequest);
            var instructionRequest = new InstructionTaskRequest
            {
                TaskId = model.TaskId
            };

            var instructionResponse = _verTaskService.GetInstruction(instructionRequest);

            var verTaskViewModel = new TaskViewModel
            {
                UserComments = commentsResponse.IsValid ? commentsResponse.Comments : null,
                Instructions = instructionResponse.IsValid ? instructionResponse.Instruction : null,
                TaskDataModel = model,
                UserName = IDBContext.Current.UserName
            };

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(verTaskViewModel);
            return View(verTaskViewModel);
        }

        public virtual JsonResult VERUpdateTask(string operationNumber)
        {
            ResponseBase response = new ResponseBase();
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<TaskViewModel>(jsonDataRequest.SerializedData);

            try
            {
                viewModel.UpdateVerTaskViewModel(jsonDataRequest.ClientFieldData);
                var saveRequest = new SaveTaskRequest
                {
                    Model = viewModel
                };

                response = _verTaskService.SaveVerTask(saveRequest);

                var workFlow = jsonDataRequest.ClientFieldData.FirstOrDefault(o => o.Name.Equals("changeStatus"));

                var requestTask = new CompleteTaskRequest
                {
                    TaskId = viewModel.TaskDataModel.TaskId,
                    InstanceId = viewModel.TaskDataModel.WorkflowInstanceId,
                    Status = VerGlobalValues.COMPLETE_TASK,
                    Action = VerGlobalValues.COMPLETE_ACTION_TASK
                };

                if (workFlow != null)
                    switch (workFlow.Value)
                    {
                        case VerGlobalValues.COMPLETE_ACTION_TASK:
                            requestTask.Action = VerGlobalValues.COMPLETE_ACTION_TASK;
                            requestTask.Status = VerGlobalValues.COMPLETE_TASK;
                            break;
                        case VerGlobalValues.REJECT_ACTION_TASK:
                            requestTask.Action = VerGlobalValues.REJECT_ACTION_TASK;
                            requestTask.Status = VerGlobalValues.REJECT_TASK;
                            break;
                    }

                _verTaskService.CompleteTask(requestTask);

                response.IsValid = true;
            }
            catch (Exception e)
            {
                response.IsValid = false;
                response.ErrorMessage = e.ToString();
            }

            return Json(response);
        }

        public virtual ActionResult UsersByRolePartial(string operationNumber, string role)
        {
            var response = _verTaskService.GetUsersByRole(new UsersByRoleRequest
            {
                OperationNumber = operationNumber,
                Role = role
            });

            return PartialView("Partials/UsersByRolePartial", response.Model);
        }
    }
}