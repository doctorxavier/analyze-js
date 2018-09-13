using System;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;

using IDB.Presentation.MVC4.General;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Application.Core.Container;
using IDB.MW.Application.SGPModule.Messages;
using IDB.MW.Application.SGPModule.Services;
using IDB.MW.Application.SGPModule.Constants;
using IDB.MW.Application.SGPModule.ViewModels;
using IDB.Presentation.MVC4.Areas.SGP.Mappers;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.MW.Infrastructure.Caching.Contracts;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Infrastructure.Caching;
using IDB.MW.Domain.Session;
using IDB.Architecture.Language;
using IDB.MW.Application.GlobalModule.Messages.WorkflowsService;

namespace IDB.Presentation.MVC4.Areas.SGP.Controllers
{
    public partial class ProcurementPlanController : Controller
    {
        #region Constants
        private const string FILTER_CACHE_KEY = "Agency-Filter";
        private static string SGP_PROCUREMENT_PLAN_URL = "/SGP/ProcurementPlan/Edit";
        #endregion

        #region Fields

        private readonly IProcurementPlanService _procurementPlanService;
        private readonly ICacheStorageService _cacheService;

        #endregion

        #region Contructors

        public ProcurementPlanController(IProcurementPlanService procurementPlanService,
                                         ICatalogService catalogService)
        {
            _procurementPlanService = procurementPlanService;
            _cacheService = CacheStorageFactory.Current;
        }

        #endregion

        #region Action Methods

        public virtual ActionResult Read(string operationNumber, string tabName = null, string errorMessage = null)
        {
            _cacheService.Remove(FILTER_CACHE_KEY);

            var model = GetReadData(operationNumber);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                model.ViewContainer["IsValid"] = false;
                model.ViewContainer["ErrorMessage"] = errorMessage;
            }

            var defaultTab = ProcurementPlanNavigation.TAB_NAME_OPERATION_LEVEL;

            if (model.ViewContainer.GetValue<bool>("isOperationApproved"))
            {
                defaultTab = ProcurementPlanNavigation.TAB_NAME_PROCUREMENT_LEVEL;
            }

            model.ViewContainer["ActiveTab"] = tabName ?? defaultTab;

            return View(model);
        }

        public virtual ActionResult Cancel(string operationNumber, string tabName = null)
        {
            SynchronizationHelper.TryReleaseLock(SGP_PROCUREMENT_PLAN_URL, operationNumber, IDBContext.Current.UserLoginName);

            var model = GetReadData(operationNumber);
            model.ViewContainer["ActiveTab"] = tabName ?? ProcurementPlanNavigation.TAB_NAME_OPERATION_LEVEL;
            return View("Read", model);
        }

        public virtual ActionResult Edit(string operationNumber, int planId = 0, int taskBucketId = 0, string tabName = null)
        {
            ProcurementPlanViewModel model = null;

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, SGP_PROCUREMENT_PLAN_URL, operationNumber, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return RedirectToAction("Read", new { operationNumber = operationNumber, tabName = tabName, errorMessage = errorMessage });
            }

            switch (tabName)
            {
                case ProcurementPlanNavigation.TAB_NAME_PROCUREMENT_PARAMETRIZATION:
                    model = GetParameterizationData(operationNumber, planId);
                    model.ViewContainer["ActiveTab"] = ProcurementPlanNavigation.TAB_NAME_PROCUREMENT_PARAMETRIZATION;
                    break;
                case ProcurementPlanNavigation.TAB_NAME_OPERATION_LEVEL:
                    model = GetOperationLevelData(operationNumber, planId, taskBucketId);
                    model.ViewContainer["ActiveTab"] = ProcurementPlanNavigation.TAB_NAME_OPERATION_LEVEL;
                    break;
                default:
                    model = GetPlanData(operationNumber, planId, taskBucketId);
                    model.ViewContainer["ActiveTab"] = ProcurementPlanNavigation.TAB_NAME_PROCUREMENT_LEVEL;
                    break;
            }

            return View(model);
        }

        public virtual JsonResult Save(string operationNumber)
        {
            var responseView = new SaveResponse() { IsValid = true };

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var model = PageSerializationHelper.DeserializeObject<ProcurementPlanViewModel>(jsonDataRequest.SerializedData);

            model.Update(jsonDataRequest.ClientFieldData);

            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, SGP_PROCUREMENT_PLAN_URL, operationNumber, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
                return Json(responseView);
            }

            var response = _procurementPlanService.Save(model);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if (responseView.IsValid)
            {
                var tab = ProcurementPlanNavigation.TAB_NAME_PROCUREMENT_LEVEL;

                if (model.Parametrization != null)
                {
                    tab = ProcurementPlanNavigation.TAB_NAME_PROCUREMENT_PARAMETRIZATION;
                }
                else if (model.OperationLevel != null)
                {
                    tab = ProcurementPlanNavigation.TAB_NAME_OPERATION_LEVEL;
                }

                var url = Url.Action("Read", "ProcurementPlan", new { area = "SGP", tabName = tab });
                responseView.UrlRedirect = url;
            }

            return Json(responseView);
        }

        public virtual ActionResult InitGeneralNoticeWorkflow(string operationNumber, int planId, int entityId, string entityType)
        {
            var arrayWorkflowInfo = entityType.Split(';');

            WorkflowRequest workflowRequest = new WorkflowRequest()
            {
                OperationNumber = operationNumber,
                WorkflowCode = arrayWorkflowInfo[0],
                EntityType = string.Format("{0}-{1}-{2}", ProcurementPlanNavigation.WORKFLOW_TYPE_GENERAL_NOTICE, planId, DateTime.Now.Ticks.ToString()),
                EntityId = entityId,
                ModuleName = "Submit for Non Objection",
                ReturnURL = Url.Action("SaveGeneralNoticeWF", "ProcurementPlan", new { area = "SGP", tabName = ProcurementPlanNavigation.TAB_NAME_OPERATION_LEVEL, planId = planId }),
                ReturnURLCancel = Url.Action("Read", "ProcurementPlan", new { area = "SGP", tabName = ProcurementPlanNavigation.TAB_NAME_OPERATION_LEVEL, planId = planId }),
                InstructionsIncluded = false,
                CanAddValidator = false,
            };

            TempData["workflowRequest"] = workflowRequest;

            return RedirectToAction("New", "Workflows", new { area = "Global", operationNumber = operationNumber });
        }

        public virtual ActionResult SaveGeneralNoticeWF(string operationNumber, int planId)
        {
            var response = _procurementPlanService.ChangeStatusGeneralNotice(planId);
            return RedirectToAction("Read", "ProcurementPlan", new { area = "SGP", tabName = ProcurementPlanNavigation.TAB_NAME_OPERATION_LEVEL, planId = planId, errorMessage = response.ErrorMessage });
        }
        #endregion

        #region Calls Ajax
        public virtual ActionResult GetProcurementsByAgency(int taskBucketId, bool isEditMode = false)
        {
            var responseView = new SaveResponse { IsValid = true };
            var html = string.Empty;

            var response = _procurementPlanService.GetFilterProcurementLevel(taskBucketId, null);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if (isEditMode)
            {
                html = this.RenderRazorViewToString("EditPartial/PlanForAgencyDataTable", response.Model.ProcurementLevel);
            }
            else
            {
                html = this.RenderRazorViewToString("ReadPartial/PlanForAgencyDataTable", response.Model.ProcurementLevel);
            }

            if (responseView.IsValid)
            {
                responseView.ContentHTML = html;
            }

            return Json(responseView);
        }

        public virtual ActionResult GetProcurementsProcLevelByFilter(int taskBucketId, ModelFilterRequest filter, bool isEditMode = false)
        {
            var responseView = new SaveResponse { IsValid = true };
            var html = string.Empty;

            _cacheService.Add(FILTER_CACHE_KEY, filter);

            var response = _procurementPlanService.GetFilterProcurementLevel(taskBucketId, filter);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if (isEditMode)
            {
                html = this.RenderRazorViewToString("EditPartial/PlanForAgencyDataTable", response.Model.ProcurementLevel);
            }
            else
            {
                html = this.RenderRazorViewToString("ReadPartial/PlanForAgencyDataTable", response.Model.ProcurementLevel);
            }

            if (responseView.IsValid)
            {
                responseView.ContentHTML = html;
            }

            return Json(responseView);
        }

        public virtual ActionResult GetProcurementsOperationLevelByFilter(ModelFilterRequest filter, bool isEditMode = false)
        {
            var responseView = new SaveResponse { IsValid = true };
            var html = string.Empty;

            _cacheService.Add(FILTER_CACHE_KEY, filter);

            var response = _procurementPlanService.GetFilterOperationLevel(filter);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            html = this.RenderRazorViewToString("ReadPartial/ProcurementActivitiesDataTable", response.Model.OperationLevel);
          
            if (responseView.IsValid)
            {
                responseView.ContentHTML = html;
            }

            return Json(responseView);
        }
           
        public virtual ActionResult PublishGeneralNoticeDoc(int planId)
        {
            var responseView = new SaveResponse { IsValid = true };

            var response = _procurementPlanService.PublishGeneralNotice(planId);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if (responseView.IsValid)
            {
                responseView.UrlRedirect = Url.Action("Read", "ProcurementPlan", new { area = "SGP" });
            }

            return Json(responseView);
        }

        public virtual ActionResult PublishProcPlanDoc(int planId)
        {
            var responseView = new SaveResponse { IsValid = true };

            var response = _procurementPlanService.PublishProcPlanDocument(planId);
            responseView.IsValid = response.IsValid;
            responseView.ErrorMessage = response.ErrorMessage;

            if (responseView.IsValid)
            {
                responseView.UrlRedirect = Url.Action("Read", "ProcurementPlan", new { area = "SGP" });
            }

            return Json(responseView);
        }

        public virtual FileResult GetReportPlanAgencyByFilter()
        {
            var response = _procurementPlanService.ExportProcurementPlanToExcel();

            if (!response.IsValid)
            {
                return null;
            }

            return File(response.File, "application/xls", "ProcurementsList.xls");
        }

        public virtual FileResult GetReportProcurementActivitiesByFilter()
        {
            var response = _procurementPlanService.ExportProcurementActivitiesToExcel();

            if (!response.IsValid)
            {
                return null;
            }

            return File(response.File, "application/xls", "ProcurementsList.xls");
        }

        public virtual JsonResult GetLocation(string literal)
        {
            var response = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(literal))
            {
                if (!response.ContainsKey(literal))
                {
                    response.Add(literal, Localization.GetText(literal));
                }
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }       
        #endregion

        #region Private Methods

        private ProcurementPlanViewModel GetReadData(string operationNumber)
        {
            var response = _procurementPlanService.GetProcurementPlan(operationNumber);
            response.Model.ViewContainer = SetViewBagErrorMessageInvalidResponse(response, response.Model.ViewContainer);
            return response.Model;
        }

        private ProcurementPlanViewModel GetPlanData(string operationNumber, int planId, int taskBucketId)
        {
            var response = _procurementPlanService.GetProcurementLevel(operationNumber, planId, taskBucketId);
            response.Model.ViewContainer = SetViewBagErrorMessageInvalidResponse(response, response.Model.ViewContainer);
            return response.Model;
        }

        private ProcurementPlanViewModel GetOperationLevelData(string operationNumber, int planId, int taskBucketId)
        {
            var response = _procurementPlanService.GetOperationLevel(operationNumber, planId);
            response.Model.ViewContainer = SetViewBagErrorMessageInvalidResponse(response, response.Model.ViewContainer);
            return response.Model;
        }
        
        private ProcurementPlanViewModel GetParameterizationData(string operationNumber, int planId)
        {
            var response = _procurementPlanService.GetParameterization(operationNumber, planId);
            response.Model.ViewContainer = SetViewBagErrorMessageInvalidResponse(response, response.Model.ViewContainer);
            return response.Model;
        }

        private GenericContainer SetViewBagErrorMessageInvalidResponse(ResponseBase response, GenericContainer container)
        {
            if (container == null)
            {
                container = new GenericContainer();
            }

            container.Add("IsValid", response.IsValid);

            if (!response.IsValid)
            {
                var urlDecode = HttpUtility.UrlDecode(response.ErrorMessage);
                if (urlDecode != null)
                {
                    var message = HttpUtility.HtmlEncode(urlDecode.Replace(Environment.NewLine, " "));
                    container.Add("ErrorMessage", message);
                }
            }

            return container;
        }

        #endregion
    }
}