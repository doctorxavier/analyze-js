using System;
using System.Web.Mvc;
using System.Web.Script.Serialization;

using IDB.MW.Application.FinancialPlanModule.Enums;
using IDB.MW.Application.FinancialPlanModule.Helpers;
using IDB.MW.Application.FinancialPlanModule.Services;
using IDB.MW.Application.FinancialPlanModule.ViewModels;
using IDB.MW.Application.GlobalModule.Messages.WorkflowsService;
using IDB.MW.Application.OPUSModule.Services.FinancialDataExecutionService;
using IDB.MW.Application.OPUSModule.Services.FinancialDataPreparationService;
using IDB.MW.Application.Reformulation.Services;
using IDB.MW.Domain.Session;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.FinancialPlan.Controllers
{
    public partial class FinancialPlanController : BaseController
    {
        private readonly IFinancialPlanService _financialPlanService;
        private readonly IFinancialDataPreparationService _financialDataPreparationService;
        private readonly IFinancialDataExecutionService _financialDataExecutionService;
        private readonly IReformulationService _reformulationService;
        private readonly IOperationHistoryService _operationHistoryService;
        private readonly IContractRepository _contractRepository;

        public FinancialPlanController(
            IFinancialPlanService financialPlanService,
            IFinancialDataPreparationService financialDataPreparationService,
            IReformulationService reformulationService,
            IOperationHistoryService operationHistoryService,
            IFinancialDataExecutionService financialDataExecutionService,
            IContractRepository contractRepository)
        {
            _financialPlanService = financialPlanService;
            _financialDataPreparationService = financialDataPreparationService;
            _reformulationService = reformulationService;
            _financialDataExecutionService = financialDataExecutionService;
            _contractRepository = contractRepository;
            _operationHistoryService = operationHistoryService;
        }

        // GET: FinancialPlan/FinancialPlan
        public virtual ActionResult Index(string operationNumber, string currentTabToBeLoaded)
        {
            var model = _financialPlanService.GetOperationLevel(
                operationNumber: operationNumber,
                currenTab: currentTabToBeLoaded,
                userName: IDBContext.Current.UserName,
                userExternalModel: IDBContext.Current.UserExternal);
            SetPermission(model);
            ViewBag.SerializedViewModelNew = PageSerializationHelper.SerializeObject(model);

            if (!string.IsNullOrWhiteSpace(model.ErrorMessage))
            {
                return View("FinancialError", model);
            }

            if (string.IsNullOrEmpty(currentTabToBeLoaded))
            {
                return View(model);
            }
            else
            {
                var data = new JsonResult
                {
                    Data = new
                    {
                        TabView = this.RenderRazorViewToString("Partial/FinancialPlanInformation", model),
                        Months = model.MonthTotal,
                        fpMode = model.Mode
                    }
                };

                return data;
            }
        }

        public virtual ActionResult FinancialInformationRead(string contractNumber, string operationNumber)
        {
            var model = _financialPlanService.GetOperationLevel(
                operationNumber: operationNumber,
                contractNumber: contractNumber.Trim(),
                userName: IDBContext.Current.UserName);
            SetPermission(model);

            return PartialView("Partial/FinancialPlanInformation", model);
        }

        public virtual JsonResult GetTotalAmountYear(
            string contractNumber,
            string operationNumber,
            int year,
            string modelFinancial,
            bool IsCurrentYear,
            string curretTab)
        {
            var result = Json(new { data = string.Empty },
                                JsonRequestBehavior.AllowGet);

            var response = _financialPlanService.GetTotalAmontYear(
                contractNumber,
                operationNumber,
                year,
                IsCurrentYear,
                curretTab);

            if (response.IsValid)
            {
                result.Data = new JavaScriptSerializer().Serialize(response);
            }
            else
            {
                result.Data = response.ErrorMessage;
            }

            return result;
        }

        public virtual ActionResult GetPeriod(
            string contractNumber,
            string operationNumber,
            string startMonthStr,
            string startYearStr,
            string endMonthStr,
            string endYearStr,
            string currentTab)
        {
            var Today = DateTime.Now;
            DateTime startDate = new DateTime(Today.Year, 1, 1);
            DateTime endDate = new DateTime(Today.Year, 12, 1);
            var startDateSrt = string.Format("{0}-{1}-01 12:00:00.0", startYearStr, startMonthStr);
            var endDateSrt = string.Format("{0}-{1}-01 12:00:00.0", endYearStr, endMonthStr);
            DateTime.TryParse(startDateSrt, out startDate);
            DateTime.TryParse(endDateSrt, out endDate);

            var model = _financialPlanService.GetOperationLevel(
                operationNumber: operationNumber,
                contractNumber: contractNumber,
                startDate: startDate,
                endDate: endDate,
                currenTab: currentTab,
                userName: IDBContext.Current.UserName,
                userExternalModel: IDBContext.Current.UserExternal);
            SetPermission(model);

            var data = new JsonResult
            {
                Data = new
                {
                    TabView = this.RenderRazorViewToString("Partial/FinancialPlanInformation", model),
                    Months = model.MonthTotal,
                    fpMode = model.Mode
                }
            };

            return data;
        }

        [HttpPost]
        public virtual ActionResult Save(FinancialPlanViewModel model)
        {
            var response = _financialPlanService.SaveFinancialPlan(model, userName: IDBContext.Current.UserName);

            return new JsonResult
            {
                Data = new
                {
                    isValid = response.IsValid,
                    message = response.ErrorMessage,
                }
            };
        }

        public virtual JsonResult WorkflowFinancialPlan(WorkflowRequest workflowRequest)
        {
            _financialPlanService.UpdateFPLastSubmitted(workflowRequest.EntityId);

            workflowRequest.ReturnURL = @Url.Action(
                "Index",
                "FinancialPlan",
                new { area = "FinancialPlan" });

            workflowRequest.WorkflowCode = WorkflowInformationConst.WorkflowCode;
            workflowRequest.ModuleName = WorkflowInformationConst.ModuleName;

            TempData["workflowRequest"] = workflowRequest;
            return this.Json(new ResponseBase()
            {
                IsValid = true,
                ErrorMessage = "Workflow was not executed"
            });
        }

        private void SetPermission(FinancialPlanViewModel model)
        {
            if (model.DisableActionButton)
            {
                model.HasPermissionEdit = false;
                model.HasPermissionSubmmit = false;
                return;
            }

            var isInExecution = FinancialPlanHelper.IsInExecution(model.Mode);
            model.HasPermissionEdit = (isInExecution
                && IDBContext.Current.HasPermission(Permission.FINANCIAL_PLAN_EXECUTION_WRITE))
                || (!isInExecution
                    && IDBContext.Current.HasPermission(Permission.FINANCIAL_PLAN_PREPARATION_WRITE));

            model.HasPermissionSubmmit = (isInExecution
                && IDBContext.Current.HasPermission(Permission.FINANCIAL_PLAN_EXECUTION_SUBMIT))
                || (!isInExecution
                    && IDBContext.Current.HasPermission(Permission.FINANCIAL_PLAN_PREPARATION_SUBMIT));
        }
    }
}