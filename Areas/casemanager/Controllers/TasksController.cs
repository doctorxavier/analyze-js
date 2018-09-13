using System;
using System.Linq;
using System.Web.Mvc;

using IDB.Presentation.MVC4.Controllers;
using IDB.MW.DomainModel.Entities.Core.K2;
using IDB.MW.Application.CaseManager.ViewModels;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.MW.Domain.Values;
using IDB.MW.Application.CaseManager.Services.Tasks;
using IDB.MW.Infrastructure.Data.Optima.Context;

namespace IDB.Presentation.MVC4.Areas.casemanager.Controllers
{
    public partial class TasksController : BaseController
    {
        #region Fields
        private ILCTaskService _lcTaskService;

        #endregion

        #region Constructors

        public TasksController(ILCTaskService lcTaskService)
        {
            _lcTaskService = lcTaskService;
        }
        #endregion

        public virtual ActionResult LCTask(string operationNumber, TaskDataModel model)
        {
            var database = new OptimaContainer();
            var splittedFolio = model.TaskFolio.Split('-');
            var taskId = splittedFolio[splittedFolio.Length - 1];
            var taskName = _lcTaskService.GetTaskName(taskId).Name;

            var TaskViewModel = new TasksViewModel
            {
                TaskName = taskName,
                Instructions = LifeCycleGlobalValues.INSTRUCTIONS_LC,
                TaskDataModel = model,
                UserName = IDBContext.Current.UserName
            };

            ViewBag.SerializedViewModel = IDB.Presentation.MVC4.Helpers.PageSerializationHelper.SerializeObject(TaskViewModel);
            return View(TaskViewModel);
        }

        public virtual JsonResult LCUpdateTask(string operationNumber)
        {
            ResponseBase response = new ResponseBase();
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<TasksViewModel>(jsonDataRequest.SerializedData);
            var splittedFolio = viewModel.TaskDataModel.TaskFolio.Split('-');
            var milestoneId = splittedFolio[splittedFolio.Length - 1];
            try
            {
                var workFlow = jsonDataRequest.ClientFieldData.FirstOrDefault(o => o.Name.Equals("changeStatus"));

                switch (workFlow.Value)
                {
                    case LifeCycleGlobalValues.COMPLETE_ACTION_TASK:
                        _lcTaskService.CompleteTask(viewModel.TaskDataModel.TaskId, viewModel.TaskDataModel.WorkflowInstanceId, LifeCycleGlobalValues.COMPLETE_TASK, LifeCycleGlobalValues.COMPLETE_ACTION_TASK, viewModel.TaskDataModel.UserName, milestoneId);
                        break;

                    case LifeCycleGlobalValues.REJECT_ACTION_TASK:
                        _lcTaskService.CompleteTask(viewModel.TaskDataModel.TaskId, viewModel.TaskDataModel.WorkflowInstanceId, LifeCycleGlobalValues.REJECT_TASK, LifeCycleGlobalValues.REJECT_ACTION_TASK, viewModel.TaskDataModel.UserName, milestoneId);
                        break;
                }

                response.IsValid = true;
            }
            catch (Exception e)
            {
                response.IsValid = false;
                response.ErrorMessage = e.ToString();
            }

            return Json(response);
        }
    }
}