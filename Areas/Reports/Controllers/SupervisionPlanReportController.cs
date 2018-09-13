using System.Configuration;
using System.Linq;
using System.Web.Mvc;

using IDB.MW.Domain.Models.Architecture.Reports.PMRExtraModels.SupervisionPlan;
using IDB.MW.Domain.Contracts.ModelRepositories.Reports.Generic;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.SupervisionPlans;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Report;

namespace IDB.Presentation.MVC4.Areas.Reports.Controllers
{
    public partial class SupervisionPlanReportController : BaseController
    {
        #region Repositories

        IReportsGenericRepository _ClientGenericRepositoty = null;
        public IReportsGenericRepository ClientGenericRepositoty
        {
            get { return _ClientGenericRepositoty; }
            set { _ClientGenericRepositoty = value; }
        }

        ISupervisionPlanModelRepository _ClienSupervisionPlan = null;
        public ISupervisionPlanModelRepository ClienSupervisionPlan
        {
            get { return _ClienSupervisionPlan; }
            set { _ClienSupervisionPlan = value; }
        }

        #endregion

        #region Variables

        private string Lang = "EN";

        #endregion

        public virtual ActionResult Create(string OperationNumber)
        {
            var OperationId = ClientGenericRepositoty.GetOperationIDForOperationNumber(OperationNumber);
            var ListSupervisionPlanOperation = ClienSupervisionPlan.GetYearsForSupervicionPlan(OperationId);
            SupervisionPlanOperationReport ModelSupervisionPlan;
            if (ListSupervisionPlanOperation.Any())
            {
                ModelSupervisionPlan = ClientGenericRepositoty.GetSupervisionPlanVersionByYear(OperationId, ListSupervisionPlanOperation[0].Year);
            }
            else
            {
                ModelSupervisionPlan = new SupervisionPlanOperationReport();
            }

            ViewBag.ListSupervisionPlanOperation = new SelectList(ListSupervisionPlanOperation, "Year", "Year");

            return View(ModelSupervisionPlan);
        }

        public virtual JsonResult GetSupervisionVersionByYear(int OperationId, int Year)
        {
            var ListSupervisionPlanYear = ClientGenericRepositoty.GetSupervisionPlanVersionByYear(OperationId, Year);
            return new JsonResult() { Data = ListSupervisionPlanYear };
        }

        [HttpPost()]
        public virtual ActionResult GenerateReport(SupervisionPlanOperationReport ModelSupervisionPlan)
        {
            string Header = ReportBuilder.GetReportHeader();

            string LinkReportSupervicionPlan = ReportBuilder.GetReportPreffix("ParamForSupervisionPlan");

            if (Lang != null)
            {
                LinkReportSupervicionPlan += "&LANG=" + Lang;
            }
            else
            {
                LinkReportSupervicionPlan += "&LANG=EN";
            }

            if (ModelSupervisionPlan.OperationID != 0)
            {
                LinkReportSupervicionPlan += "&OPERATION_ID=" + ModelSupervisionPlan.OperationID;
            }

            if (ModelSupervisionPlan.SupervisionVersion.Count > 0)
            {
                foreach (var item in ModelSupervisionPlan.SupervisionVersion)
                {
                    if (item.Value)
                    {
                        LinkReportSupervicionPlan += "&SUPERVISION_PLAN_VERSION_ID=" + item.Key;
                    }
                }
            }
            else
            {
                LinkReportSupervicionPlan = "You Haven't supervision plan version";
            }

            return Content(LinkReportSupervicionPlan + Header);
        }

        public virtual ActionResult DownloadSupervisionReport(SupervisionPlanOperationReport ModelSupervisionPlan)
        {
            string LinkReportSupervicionPlan = string.Empty;

            LinkReportSupervicionPlan += ReportBuilder.GetReportPreffix("ParamForSupervisionPlan");

            if (Lang != null)
            {
                LinkReportSupervicionPlan += "&LANG=" + Lang;
            }
            else
            {
                LinkReportSupervicionPlan += "&LANG=EN";
            }

            if (ModelSupervisionPlan.OperationID != 0)
            {
                LinkReportSupervicionPlan += "&OPERATION_ID=" + ModelSupervisionPlan.OperationID;
            }

            if (ModelSupervisionPlan.SupervisionVersion.Count > 0)
            {
                foreach (var item in ModelSupervisionPlan.SupervisionVersion)
                {
                    if (item.Value)
                    {
                        LinkReportSupervicionPlan += "&SUPERVISION_PLAN_VERSION_ID=" + item.Key;
                    }
                }
            }
            else
            {
                LinkReportSupervicionPlan = "You Haven't supervision plan version";
            }

            LinkReportSupervicionPlan += "&rs:Format=" + ModelSupervisionPlan.ExportType;

            return Content(LinkReportSupervicionPlan);
        }
    }
}
