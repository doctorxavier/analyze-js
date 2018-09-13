using System.Configuration;
using System.Web.Mvc;

using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.Reports;
using IDB.MW.Domain.Contracts.ModelRepositories.Reports.Generic;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Report;
using IDB.MW.Domain.Models.Architecture.Reports;

namespace IDB.Presentation.MVC4.Areas.Reports.Controllers
{
    public partial class TCMReportController : BaseController
    {
        private const string OPERATION = "OPERATION";
        private const string CYCLE = "CYCLE";
        private const string LANG = "LANG";
        private const string EXCEL_TYPE = "application/vnd.ms-excel";
        private const string EXCEL_EXTENSION = ".xlsx";
        private const string FORMAT = "EXCELOPENXML";

        private IPmrCycleModelRepository _ClientPMRForCycleModel = null;
        private IReportsGenericRepository _ClientForGenericReports = null;

        private string _lang = IDBContext.Current.CurrentLanguage.ToUpper();

        public TCMReportController(
            IPmrCycleModelRepository ClientPMRForCycleModel,
            IReportsGenericRepository ClientForGenericReports)
        {
            _ClientPMRForCycleModel = ClientPMRForCycleModel;
            _ClientForGenericReports = ClientForGenericReports;
        }

        public virtual ActionResult GetTCMReport(
            string operationNumber, int resultsMatrixId, string type)
        {
            PmrCycleModel tcmCycleModel = _ClientPMRForCycleModel.GetPMRCycle(_lang, resultsMatrixId);

            string parameters = SetReportParameters(operationNumber, tcmCycleModel, resultsMatrixId);

            string reportServer = ConfigurationManager.AppSettings["ParamReportServer"];
            string report = ConfigurationManager.AppSettings["ParamForTCMReport"];

            string url = reportServer + report;

            url += "&" + parameters + "&rs:Format=" + FORMAT + "&rs:ClearSession=true";

            return Redirect(url);
        }

        private string GetReportFileName(string operationNumber, PmrCycleModel tcmCycleModel)
        {
            return string.Format(
                "{0} {1}{2}", operationNumber, tcmCycleModel.PmrCycleName, EXCEL_EXTENSION);
        }

        private string SetReportParameters(
            string operationNumber, PmrCycleModel tcmCycleModel, int resultsMatrixId)
        {
            int operationID = _ClientForGenericReports
                .GetOperationIDParentForOperationNumber(operationNumber);

            string parameters = OPERATION + "=" + operationID;
            parameters += "&" + CYCLE + "=" + tcmCycleModel.PmrCycleId;
            parameters += "&" + LANG + "=" + _lang;

            return parameters;
        }
    }
}