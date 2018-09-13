using System;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture;
using IDB.Architecture.Language;
using IDB.MW.Application.CaseManager.Business;
using IDB.MW.Business.SupervisionPlan.Messages;
using IDB.MW.Business.SupervisionPlan.Services.OperationBudget;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values.SupervisionPlan;
using IDB.MW.Infrastructure.Configuration;
using IDB.Presentation.MVC4.Areas.SupervisionPlan.Models;

namespace IDB.Presentation.MVC4.Areas.SupervisionPlan.Controllers
{
    public partial class OperationBudgetController : MVC4.Controllers.ConfluenceController
    {
        #region Fields
        private readonly IOperationBudgetService _operationBudgetService;
        #endregion

        #region Constructor
        public OperationBudgetController(IOperationBudgetService operationBudgetService)
        {
            _operationBudgetService = operationBudgetService;
        }
        #endregion

        public virtual FileResult DownloadOperationBudgetProgress(
            string operationNumber, string formatType, string year)
        {
            var response = _operationBudgetService.GetOperationBudgetReport(
                operationNumber, formatType, year);
            if (!response.IsValid)
            {
                return null;
            }

            var application = "application/";
            var fileName = string.Format("{0}_Operation_Budget",
                operationNumber.Replace("-", "_"));

            switch (formatType)
            {
                case "xls":
                    application = application + "vnd.ms-excel";
                    fileName = fileName + ".xls";
                    break;
                default:
                    application = application + "pdf";
                    fileName = fileName + ".pdf";
                    break;
            }

            return File(response.File, application, fileName);
        }

        public virtual ActionResult IndexOperationBudget(string operationNumber, string year)
        {
            var stageCode = CMBusiness.Get(IDBContext.Current.Operation).Context.APPRMilestone
                .IsCompleted(false)
                ? OperationBudgetValues.OPERATION_PHASE_EXECUTION
                : OperationBudgetValues.OPERATION_PHASE_PREPARATION;

            string stage = stageCode == OperationBudgetValues.OPERATION_PHASE_EXECUTION
                ? Localization.GetText("TCP.FirmProcurement.Tabs.Execution")
                : Localization.GetText("TCP.FirmProcurement.Tabs.Preparation");

            var service = Globals.Resolve<ISPOperationBudget>();

            var responseYears = service.GetSpYearsByOperationNumber(
                new GetSPYearsByOperationNumberRequest
                {
                    OperationNumber = operationNumber
                });
            if (!responseYears.IsValid) throw new Exception(responseYears.ErrorMessage);
            var responseConvert = service.ConvertListItems(responseYears.Years);
            var selectYear = string.IsNullOrEmpty(year)
                ? responseYears.Years.FirstOrDefault()
                : Convert.ToInt32(year);
            var response = service.GetBudget(new PSPBudgetRequest
            {
                Year = selectYear,
                OperationNumber = operationNumber,
                Stage = stage,
                StageCode = stageCode
            });
            if (!response.IsValid) throw new Exception(response.ErrorMessage);
            var result = new SPOperationBudgetViewModel
            {
                SpOperationBudgetDto = response.PreparationOperationBudget,
                SpTotalValuesDto = response.SpTotalValuesDto,
                SelectedYear = selectYear.ToString(),
                Years = responseConvert,
                SupervisionPlanDashboardRoute = ConfigurationServiceFactory
                    .Current.GetApplicationSettings().SupervisionPlanDashboardRoute
            };

            return PartialView(result);
        }
    }
}