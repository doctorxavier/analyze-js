using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture;
using IDB.Architecture.Extensions;
using IDB.Architecture.Language;
using IDB.MVCControls.General.Helpers;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.GlobalModule.Workflow;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.MW.GenericWorkflow.Mappers;
using IDB.MW.GenericWorkflow.Models;
using IDB.MW.GenericWorkflow.Services;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.Workflow.Controllers
{
    public partial class TaskController : BaseController
    {
        private readonly IWorkflowInstanceTaskRepository _workflowInstanceTaskRepository;
        private readonly IWorkflowManagerService _workflowManagerService;
        private readonly ITaskService _taskService;
        private readonly ICommentService _commentService;
        private readonly ICatalogService _catalogService;
        private readonly ITaskDocumentsService _taskDocumentsService;
        private readonly IValidatorService _validatorService;

        public TaskController(
            IWorkflowInstanceTaskRepository workflowInstanceTaskRepository,
            IWorkflowManagerService workflowManagerService,
            ICommentService commentService,
            ICatalogService catalogService,
            ITaskService taskService,
            ITaskDocumentsService taskDocumentsService,
            IValidatorService validatorService)
        {
            _workflowInstanceTaskRepository = workflowInstanceTaskRepository;
            _workflowManagerService = workflowManagerService;
            _commentService = commentService;
            _catalogService = catalogService;
            _taskService = taskService;
            _taskDocumentsService = taskDocumentsService;
            _validatorService = validatorService;
        }

        public virtual ActionResult Detail(int taskId, bool edit = true, int state = 0, string status = null)
        {
            var workflowInstanceTask = _workflowInstanceTaskRepository
                .GetOne(x => x.WorkflowInstanceTaskId.Equals(taskId));

            if (workflowInstanceTask == null)
            {
                return View("Detail", new TaskDetailViewModel());
            }

            var taskModel = workflowInstanceTask.ConvertToTaskDetailViewModel(IDBContext.Current.CurrentLanguage);
            taskModel.UserLogin = IDBContext.Current.UserName;

            var taskDataModel = taskModel.ConvertToTaskDataModel();

            var roles = GetListMasterData(MasterType.CONVERGENCE_ROLES);

            var responseValidators =
                _workflowManagerService
                .GetHistoricAndHappyRoadValidators(workflowInstanceTask.WorkflowInstanceId);

            if (!responseValidators.IsValid)
            {
                throw new ArgumentException("Parameter no is valid, Detail method");
            }

            var validators = responseValidators.Models;

            var firstValidatorHappy = validators.FirstOrDefault(x => x.Order == -1);
            if (firstValidatorHappy != null)
            {
                validators.Remove(firstValidatorHappy);
            }

            taskModel.Validators = validators.ToList();
            taskModel.TaskActions = _taskService.GetActionsByInstanceTaskCode(taskModel.TaskTypeCode);
            var commentResponse = _commentService.GetCommentsByWorkflow(taskDataModel.WorkflowInstanceId);

            if (commentResponse.IsValid)
            {
                taskModel.Comments = commentResponse.Models.ToList();
            }

            taskModel.SerializedViewModel = PageSerializationHelper.SerializeObject(taskDataModel);

            var getBusinessAreaResult = _taskService.GetBusinessArea(workflowInstanceTask);
            if (!getBusinessAreaResult.IsValid)
            {
                return View("Detail", new TaskDetailViewModel());
            }

            taskModel.BusinessArea = getBusinessAreaResult.Data;

            var rolesToBeLoaded = taskModel.Validators.Select(x => x.Role).ToList();

            if (rolesToBeLoaded.Any())
            {
                ViewBag.RetrieveListRoles = RemoveUsedRole(rolesToBeLoaded, roles);
            }
            else
            {
                ViewBag.RetrieveListRoles = RemoveUsedRole(ViewBag.validators, roles);
            }

            if (taskModel.Code.StartsWith("WF-G-"))
            {
                taskModel.InstructionsFT = Localization.GetText(workflowInstanceTask.WorkflowTaskType.WorkflowTaskTypeConfiguration.Instructions.ToString());
            }

            var result = _taskDocumentsService.GetDocumentByTask(workflowInstanceTask);

            if (!result.IsValid)
            {
                return View("Detail", new TaskDetailViewModel());
            }

            taskModel.Documents.AddRange(result.Models);

            return View("Detail", taskModel);
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

        public virtual JsonResult Save(WorkflowSaveViewModel model)
        {
            var response = new ResponseBase { IsValid = true };
            try
            {
                if (model == null)
                {
                    throw new ArgumentNullException("model");
                }

                if (model.DocumentNumbers != null)
                {
                    var resultInsertDocuments = _taskDocumentsService.InsertDocuments(
                        model.DocumentNumbers, model.TaskId);

                    if (!resultInsertDocuments.IsValid)
                    {
                        response.ErrorMessage = resultInsertDocuments.ErrorMessage;
                        response.IsValid = false;

                        return Json(response);
                    }
                }

                if (model.Comments != null)
                {
                    var responseInsert = _commentService
                        .InsertComments(model.Comments);
                    if (!responseInsert.IsValid)
                    {
                        response.ErrorMessage = responseInsert.ErrorMessage;
                        response.IsValid = false;

                        return Json(response);
                    }
                }

                if (model.DeleteComments != null)
                {
                    var responseDelete = _commentService
                        .DeleteComments(model.DeleteComments);
                    if (!responseDelete.IsValid)
                    {
                        response.ErrorMessage = responseDelete.ErrorMessage;
                        response.IsValid = false;

                        return Json(response);
                    }
                }

                response = ValidatorModel(model);

                if (!response.IsValid)
                {
                    return Json(response);
                }

                if (model.ChangeStatus != "none")
                {
                    _workflowManagerService.AdvanceWorkflow(
                        model.TaskId,
                        IDBContext.Current.UserName,
                        model.ChangeStatus,
                        model.AdditionalValidators);
                }
            }
            catch (Exception e)
            {
                response.IsValid = false;
                response.ErrorMessage = e.ToString();
            }

            return Json(response);
        }

        private List<SelectListItem> RemoveUsedRole(List<string> validators, List<SelectListItem> roles)
        {
            if (validators.HasAny())
            {
                foreach (var validator in validators)
                {
                    roles.RemoveAll(x => x.Text == validator);
                }
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

        private ResponseBase ValidatorModel(WorkflowSaveViewModel model)
        {
            var response = new ResponseBase { IsValid = true };

            if (model.ValidatorViewModel != null && model.WorkflowInstanceId != 0)
            {
                var responseDeleteValidators =
                    _validatorService.DeleteValidators(model.ValidatorViewModel, model.WorkflowInstanceId);

                if (!responseDeleteValidators.IsValid)
                {
                    response.IsValid = false;
                    response.ErrorMessage = responseDeleteValidators.ErrorMessage;

                    return response;
                }
            }

            if (model.AdditionalValidators != null && model.WorkflowInstanceId != 0)
            {
                var workflowInstance = _workflowInstanceTaskRepository
                    .GetOne(wit => wit.WorkflowInstanceId == model.WorkflowInstanceId);

                var responseCreateValidators =
                    _validatorService.AdditionalValidators(model.WorkflowInstanceId,
                        workflowInstance.WorkflowTaskType.WorkflowTaskTypeId,
                        model.AdditionalValidators);

                if (!responseCreateValidators.IsValid)
                {
                    response.IsValid = false;
                    response.ErrorMessage = responseCreateValidators.ErrorMessage;

                    return response;
                }
            }

            return response;
        }
    }
}