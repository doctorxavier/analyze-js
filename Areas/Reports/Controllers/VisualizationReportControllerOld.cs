using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.MW.Domain.Contracts.ModelRepositories.Reports.Generic;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.Reports;
using IDB.MW.Domain.Models.Architecture.Reports.PMRExtraModels.Visualization.VisualizationOperation;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.SupervisionPlans;
using IDB.Presentation.MVC4.Controllers;
using IDB.Architecture.Language;
using IDB.Presentation.MVC4.Report;

namespace IDB.Presentation.MVC4.Areas.Reports.Controllers
{
    public partial class VisualizationReportControllerOld : BaseController
    {
        #region Repositories

        IPmrCycleModelRepository _ClientPMRCycle = null;
        public IPmrCycleModelRepository ClientPMRCycle
        {
            get { return _ClientPMRCycle; }
            set { _ClientPMRCycle = value; }
        }

        ISupervisionPlanModelRepository _ClienSupervisionPlan = null;
        public ISupervisionPlanModelRepository ClienSupervisionPlan
        {
            get { return _ClienSupervisionPlan; }
            set { _ClienSupervisionPlan = value; }
        }

        IReportsGenericRepository _ClientGenericRepositoty = null;
        public IReportsGenericRepository ClientGenericRepositoty
        {
            get { return _ClientGenericRepositoty; }
            set { _ClientGenericRepositoty = value; }
        }

        #endregion

        #region Variables

        private string Lang = "EN";

        #endregion

        public virtual ActionResult Create(string OperationNumber)
        {
            VisualizationOperationReportModel ModelVisualizationOperationReport = new VisualizationOperationReportModel();
            ModelVisualizationOperationReport.OperationID = ClientGenericRepositoty.GetOperationIDForOperationNumber(OperationNumber);

            var ListYearForOperation = ClienSupervisionPlan.GetYearsForSupervicionPlan(ModelVisualizationOperationReport.OperationID).OrderBy(x => x.Year).ToList();
            var ListVisualOutputDeliveryStatus = ClientGenericRepositoty.GetVisualOutputDeliveryStatus(Lang).OrderBy(x => x.Name).ToList();
            var ListMap = new Dictionary<bool, string>();
            ListMap.Add(false, Localization.GetText("Internal Map"));
            ListMap.Add(true, Localization.GetText("External Map"));

            ViewBag.ListMap = new SelectList(ListMap, "key", "value");
            ViewBag.ListYearForOperation = new MultiSelectList(ListYearForOperation, "Year", "Year");
            ViewBag.ListVisualOutputDeliveryStatus = new MultiSelectList(ListVisualOutputDeliveryStatus, "convergencemasterdataid", "name");

            return View(ModelVisualizationOperationReport);
        }

        [HttpPost()]
        public virtual ActionResult Create(VisualizationOperationReportModel ModelVisualizationOperationReport)
        {
            string Header = ReportBuilder.GetReportHeader();

            string ReportVisualizationOperation = ReportBuilder.GetReportPreffix("ParamForVisualizationOperationReport");

            if (Lang != null)
            {
                ReportVisualizationOperation += "&LANG=" + Lang;
            }
            else
            {
                ReportVisualizationOperation += "&LANG=EN";
            }

            ReportVisualizationOperation += "&OPERATION_ID=" + ModelVisualizationOperationReport.OperationID;

            ReportVisualizationOperation += "&ValidatedRMStatus=" + ModelVisualizationOperationReport.ValidatedRMStatus;

            ReportVisualizationOperation += "&EXTERNAL_MAP=" + ModelVisualizationOperationReport.Map;

            if (ModelVisualizationOperationReport.VODeliveryStatus.Count > 0)
            {
                foreach (var ItemVODeliveryStatus in ModelVisualizationOperationReport.VODeliveryStatus)
                {
                    ReportVisualizationOperation += "&DELIVERY_STATUS=" + ItemVODeliveryStatus;
                }
            }
            else
            {
                ReportVisualizationOperation += "&DELIVERY_STATUS=-1";
            }

            if (ModelVisualizationOperationReport.Year.Count > 0)
            {
                foreach (var ItemYear in ModelVisualizationOperationReport.Year)
                {
                    ReportVisualizationOperation += "&YEAR=" + ItemYear;
                }
            }
            else
            {
                ReportVisualizationOperation += "&YEAR=-1";
            }

            ReportVisualizationOperation += "&PMR_CYCLE=" + ClientGenericRepositoty.GetLastPMRCycleByOperationId(ModelVisualizationOperationReport.OperationID);
            return Content(ReportVisualizationOperation + Header);
        }

        public virtual ActionResult ExportVisualizationOperationReport(VisualizationOperationReportModel ModelVisualizationOperationReport)
        {
            string ReportVisualizationOperation = string.Empty;

            ReportVisualizationOperation += ReportBuilder.GetReportPreffix("ParamForVisualizationOperationReport");

            if (Lang != null)
            {
                ReportVisualizationOperation += "&LANG=" + Lang;
            }
            else
            {
                ReportVisualizationOperation += "&LANG=EN";
            }

            ReportVisualizationOperation += "&OPERATION_ID=" + ModelVisualizationOperationReport.OperationID;

            ReportVisualizationOperation += "&ValidatedRMStatus=" + ModelVisualizationOperationReport.ValidatedRMStatus;

            ReportVisualizationOperation += "&EXTERNAL_MAP=" + ModelVisualizationOperationReport.Map;

            if (ModelVisualizationOperationReport.VODeliveryStatus.Count > 0)
            {
                foreach (var ItemVODeliveryStatus in ModelVisualizationOperationReport.VODeliveryStatus)
                {
                    ReportVisualizationOperation += "&DELIVERY_STATUS=" + ItemVODeliveryStatus;
                }
            }
            else
            {
                ReportVisualizationOperation += "&DELIVERY_STATUS=-1";
            }

            if (ModelVisualizationOperationReport.Year.Count > 0)
            {
                foreach (var ItemYear in ModelVisualizationOperationReport.Year)
                {
                    ReportVisualizationOperation += "&YEAR=" + ItemYear;
                }
            }
            else
            {
                ReportVisualizationOperation += "&YEAR=-1";
            }

            ReportVisualizationOperation += "&PMR_CYCLE=" + ClientGenericRepositoty.GetLastPMRCycleByOperationId(ModelVisualizationOperationReport.OperationID);

            ReportVisualizationOperation += "&rs:Format=" + ModelVisualizationOperationReport.ExportType;

            return Content(ReportVisualizationOperation);
        }
    }
}
