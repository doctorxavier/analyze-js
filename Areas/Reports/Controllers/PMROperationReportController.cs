using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.Extensions;
using IDB.Architecture.Language;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.Reports;
using IDB.MW.Domain.Contracts.ModelRepositories.Reports.Generic;
using IDB.MW.Domain.Models.Architecture.Reports;
using IDB.MW.Domain.Models.Architecture.Reports.PMRExtraModels.PMROperation;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.Infrastructure.Configuration;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Report;
using IDB.Presentation.MVC4.Areas.PMI.Controllers;
using IDB.Architecture.Logging;

namespace IDB.Presentation.MVC4.Areas.Reports.Controllers
{
    public partial class PMROperationReportController : BaseController
    {
        #region Interface Definitions

        private IPmrCycleModelRepository _ClientPMRForCycleModel = null;
        public IPmrCycleModelRepository ClientPMRForCycleModel
        {
            get { return _ClientPMRForCycleModel; }
            set { _ClientPMRForCycleModel = value; }
        }

        private IReportsGenericRepository _ClientForGenericReports = null;
        public IReportsGenericRepository ClientForForGenericReports
        {
            get { return _ClientForGenericReports; }
            set { _ClientForGenericReports = value; }
        }

        #endregion

        #region Variables

        private int? idOperation = 0;
        private string Lang = IDBContext.Current.CurrentLanguage.ToUpper();

        #endregion

        public virtual ActionResult PMROperationReport()
        {
            return View();
        }

        public virtual ActionResult PMRCycle()
        {
            return View();
        }

        public virtual ActionResult ReportType()
        {
            return View();
        }

        [HttpGet()]
        public virtual ActionResult PMROperationReportCreate(string OperationNumber)
        {
            ViewBag.OperationNumber = OperationNumber;
            this.idOperation = ClientForForGenericReports.GetOperationIDParentForOperationNumber(OperationNumber);

            if (idOperation != null && idOperation != 0)
            {
                int OperationID;
                OperationID = Convert.ToInt32(this.idOperation);
                PMROperationReportModel ModelOperationReport = new PMROperationReportModel() { OperationId = OperationID };

                try
                {
                    ModelOperationReport.PMRCycles = ClientPMRForCycleModel.GetCyclesForPMR(Lang, OperationID).OrderByDescending(x => x.PmrCycleId).ToList();

                    ViewBag.ListPMRCycle = new MultiSelectList(ModelOperationReport.PMRCycles, "PmrCycleId", "PmrCycleName");
                    bool ParentOperation = ClientPMRForCycleModel.ValidateOperationParent(OperationID);
                    ModelOperationReport.FinancialDataAggregation = ParentOperation;

                    ModelOperationReport.ReportTypes.Add("Complete", Localization.GetText("Complete"));
                    ModelOperationReport.ReportTypes.Add("Custom", Localization.GetText("Custom"));
                    ModelOperationReport.ReportTypes.Add("Results Matrix", Localization.GetText("Results Matrix"));

                    ViewBag.ListReportsType = new SelectList(ModelOperationReport.ReportTypes, "Key", "Value");

                    return View(ModelOperationReport);
                }
                catch (Exception e)
                {
                    return HttpNotFound(e.Message);
                }
            }

            return HttpNotFound();
        }

        #region Ajax Call Generate URL Download

        public virtual ActionResult IsRedirectToEDWReport(PMROperationReportModel modelPMROperation)
        {
            List<PmrCycleModel> pmrCycles = GetListCycles();

            bool isRedirectToEDWReport = IsRedirectToEDWReport(
                modelPMROperation, pmrCycles);

            if (isRedirectToEDWReport)
            {
                string edwUrl = ConfigurationServiceFactory.Current.GetApplicationSettings().UrlPMREDWReport;
                edwUrl += "&p_pCycle=" + modelPMROperation.PMRCycleId.Max().ToString();
                edwUrl += "&p_pOperNumber=" + IDBContext.Current.Operation;
                edwUrl += "&p_pSection=";

                List<string> selectedSections = new List<string>();

                if (modelPMROperation.ReportParts["EnvironmentSocialImpact"])
                    selectedSections.Add("IS_ENVIRONMENT_SECTION_DISPLAY");

                if (modelPMROperation.ReportParts["Reformulation"])
                    selectedSections.Add("IS_REFORMULATED_SECTION_DISPLAY");

                if (modelPMROperation.ReportParts["Impacts"])
                    selectedSections.Add("IS_IMPACT_SECTION_DISPLAY");

                if (modelPMROperation.ReportParts["ImpactIndicators"])
                    selectedSections.Add("IS_IMPIND_DETAILS_SECTION_DISPLAY");

                if (modelPMROperation.ReportParts["Outcomes"])
                    selectedSections.Add("IS_OUTCOME_DISPLAY");

                if (modelPMROperation.ReportParts["OutputsPhysicalProgress"])
                    selectedSections.Add("IS_OUTPUT_PHYSICAL_DISPLAY");

                if (modelPMROperation.ReportParts["OutputsFinancialProgress"])
                    selectedSections.Add("IS_OUTPUT_FINANCIAL_DISPLAY");

                if (modelPMROperation.ReportParts["OutputsAnnualProgress"])
                    selectedSections.Add("IS_OUTPUT_ANNUAL_DISPLAY");

                if (modelPMROperation.ReportParts["OutcomesIndicators"])
                    selectedSections.Add("IS_OUTPUT_INDICATOR_DISPLAY");

                if (modelPMROperation.ReportParts["Risk"])
                    selectedSections.Add("IS_RISKS_DISPLAY");

                if (modelPMROperation.ReportParts["OutcomesIndicators"])
                    selectedSections.Add("IS_OUT_INDICATORS_DETAILS_DISPLAY");

                if (modelPMROperation.ReportParts["MatrixChanges"])
                    selectedSections.Add("IS_CHANGES_MATRIX_DISPLAY");

                if (modelPMROperation.ReportParts["Events"])
                    selectedSections.Add("IS_EVENTS_DATA");

                if (modelPMROperation.ReportParts["TotalCostSource"])
                    selectedSections.Add("IS_TOTAL_COST_DISPLAY");

                if (modelPMROperation.ReportParts["AvailableFunds"])
                    selectedSections.Add("IS_AVAILABLE_FUNDS_DISPLAY");

                if (modelPMROperation.ReportParts["DisaggregationCategories"])
                    selectedSections.Add("IS_DISAGGREGATIONS_DISPLAY");

                if (modelPMROperation.ReportParts["OperationBasicData"])
                    selectedSections.Add("IS_OPERATION_BASIC_DATA_DISPLAY");

                if (modelPMROperation.ReportParts["PMIGraphics"])
                    selectedSections.Add("IS_PMI_GRAPHICS");

                if (modelPMROperation.ReportParts["ClassificationValidation"])
                    selectedSections.Add("IS_CLASSIFICATION_VALIDATION");

                if (modelPMROperation.ReportParts["DELAY_SOLVED"])
                    selectedSections.Add("IS_FR_RESOLVED_DISPLAY");

                if (modelPMROperation.ReportParts["DELAY_UNSOLVED"])
                    selectedSections.Add("IS_FR_UNRESOLVED_DISPLAY");

                if (modelPMROperation.ReportParts["Disbursements"])
                    selectedSections.Add("IS_DISBURSEMENT_DISPLAY");

                if (modelPMROperation.ReportParts["DEMInformation"])
                    selectedSections.Add("IS_DEM_DISPLAY");

                if (modelPMROperation.ReportParts["ExpenseCategories"])
                    selectedSections.Add("IS_EXPENDITURE_CATEGORIES_DISPLAY");

                if (modelPMROperation.ReportParts["FindingRecommendation"])
                    selectedSections.Add("IS_FR_DISPLAY");

                edwUrl += string.Join(",", selectedSections);

                return new JsonResult() { Data = edwUrl };
            }
            else
            {
                return new JsonResult() { Data = null };
            }
        }

        public virtual ActionResult GetURLPMROPeration(PMROperationReportModel modelPMROperation)
        {
            List<PmrCycleModel> pmrCycles = GetListCycles();

            bool isRedirectToEDWReport = IsRedirectToEDWReport(
                modelPMROperation, pmrCycles);

            if (isRedirectToEDWReport)
            {
                string edwUrl = ConfigurationServiceFactory.Current.GetApplicationSettings().UrlPMREDWReport;
                edwUrl += "&p_pCycle=" + modelPMROperation.PMRCycleId.Max().ToString();
                edwUrl += "&p_pOperNumber=" + IDBContext.Current.Operation;
                edwUrl += "&p_pSection=";

                List<string> selectedSections = new List<string>();

                if (modelPMROperation.ReportParts["EnvironmentSocialImpact"])
                    selectedSections.Add("IS_ENVIRONMENT_SECTION_DISPLAY");

                if (modelPMROperation.ReportParts["Reformulation"])
                    selectedSections.Add("IS_REFORMULATED_SECTION_DISPLAY");

                if (modelPMROperation.ReportParts["Impacts"])
                    selectedSections.Add("IS_IMPACT_SECTION_DISPLAY");

                if (modelPMROperation.ReportParts["ImpactIndicators"])
                    selectedSections.Add("IS_IMPIND_DETAILS_SECTION_DISPLAY");

                if (modelPMROperation.ReportParts["Outcomes"])
                    selectedSections.Add("IS_OUTCOME_DISPLAY");

                if (modelPMROperation.ReportParts["OutputsPhysicalProgress"])
                    selectedSections.Add("IS_OUTPUT_PHYSICAL_DISPLAY");

                if (modelPMROperation.ReportParts["OutputsFinancialProgress"])
                    selectedSections.Add("IS_OUTPUT_FINANCIAL_DISPLAY");

                if (modelPMROperation.ReportParts["OutputsAnnualProgress"])
                    selectedSections.Add("IS_OUTPUT_ANNUAL_DISPLAY");

                if (modelPMROperation.ReportParts["OutcomesIndicators"])
                    selectedSections.Add("IS_OUTPUT_INDICATOR_DISPLAY");

                if (modelPMROperation.ReportParts["Risk"])
                    selectedSections.Add("IS_RISKS_DISPLAY");

                if (modelPMROperation.ReportParts["OutcomesIndicators"])
                    selectedSections.Add("IS_OUT_INDICATORS_DETAILS_DISPLAY");

                if (modelPMROperation.ReportParts["MatrixChanges"])
                    selectedSections.Add("IS_CHANGES_MATRIX_DISPLAY");

                if (modelPMROperation.ReportParts["Events"])
                    selectedSections.Add("IS_EVENTS_DATA");

                if (modelPMROperation.ReportParts["TotalCostSource"])
                    selectedSections.Add("IS_TOTAL_COST_DISPLAY");

                if (modelPMROperation.ReportParts["AvailableFunds"])
                    selectedSections.Add("IS_AVAILABLE_FUNDS_DISPLAY");

                if (modelPMROperation.ReportParts["DisaggregationCategories"])
                    selectedSections.Add("IS_DISAGGREGATIONS_DISPLAY");

                if (modelPMROperation.ReportParts["OperationBasicData"])
                    selectedSections.Add("IS_OPERATION_BASIC_DATA_DISPLAY");

                if (modelPMROperation.ReportParts["PMIGraphics"])
                    selectedSections.Add("IS_PMI_GRAPHICS");

                if (modelPMROperation.ReportParts["ClassificationValidation"])
                    selectedSections.Add("IS_CLASSIFICATION_VALIDATION");

                if (modelPMROperation.ReportParts["DELAY_SOLVED"])
                    selectedSections.Add("IS_FR_RESOLVED_DISPLAY");

                if (modelPMROperation.ReportParts["DELAY_UNSOLVED"])
                    selectedSections.Add("IS_FR_UNRESOLVED_DISPLAY");

                if (modelPMROperation.ReportParts["Disbursements"])
                    selectedSections.Add("IS_DISBURSEMENT_DISPLAY");

                if (modelPMROperation.ReportParts["DEMInformation"])
                    selectedSections.Add("IS_DEM_DISPLAY");

                if (modelPMROperation.ReportParts["ExpenseCategories"])
                    selectedSections.Add("IS_EXPENDITURE_CATEGORIES_DISPLAY");

                if (modelPMROperation.ReportParts["FindingRecommendation"])
                    selectedSections.Add("IS_FR_DISPLAY");

                edwUrl += string.Join(",", selectedSections);

                return new JsonResult() { Data = edwUrl };
            }
            else
            {
                string mimeType = string.Empty;

                var fileName = string.Format("{0} ", IDBContext.Current.Operation);

                modelPMROperation.ResultMatrixID = ClientForForGenericReports
                    .GetResultMatrixIDForOperationNumber(modelPMROperation.OperationId);

                string reportServer = ConfigurationManager.AppSettings["ParamReportServer"];

                string report = modelPMROperation.TypeReport.Equals("Results Matrix") ?
                ConfigurationManager.AppSettings["ParamForPMROperationRMReport"] :
                report = ConfigurationManager.AppSettings["ParamForPMROperationReport"];

                string parameters = "&OPERATION=" + modelPMROperation.OperationId;

                if (modelPMROperation.PMRCycleId != null)
                {
                    var cycleId = modelPMROperation.PMRCycleId.OrderByDescending(x => x)
                    .FirstOrDefault();
                    fileName = fileName + pmrCycles
                    .Where(x => x.PmrCycleId == cycleId)
                    .Select(x => x.PmrCycleName)
                    .FirstOrDefault();

                    for (int x = 0; x < modelPMROperation.PMRCycleId.Count; x++)
                    {
                        parameters += "&CYCLE=" + modelPMROperation.PMRCycleId[x];
                    }
                }
                else
                {
                    fileName = fileName + pmrCycles
                    .OrderByDescending(x => x.PmrCycleId)
                    .Select(x => x.PmrCycleName)
                    .FirstOrDefault();

                    foreach (var itemCycles in pmrCycles)
                    {
                        parameters += "&CYCLE=" + itemCycles.PmrCycleId;
                    }
                }

                parameters += Lang != null ? "&LANG=" + Lang : "&LANG=EN";

                parameters += "&IS_OPERATION_OBJECTIVE=" +
                    modelPMROperation.ReportParts["OperationObjective"];

                parameters += "&IS_INACTIVE_OUTPUTS_DISPLAY=" +
                    modelPMROperation.ReportParts["IS_DEACTIVATED"];

                if (!modelPMROperation.TypeReport.Equals("Results Matrix"))
                {
                    if (modelPMROperation.ReportParts.ContainsKey("isFinancialDataAggregated"))
                    {
                        parameters += "&IS_FINANCIAL_DATA_AGGREGATED=" +
                            modelPMROperation.ReportParts["isFinancialDataAggregated"];
                    }
                    else
                    {
                        parameters += "&IS_FINANCIAL_DATA_AGGREGATED=" + true;
                    }

                    parameters += "&IS_ENVIRONMENT_SECTION_DISPLAY=" +
                            modelPMROperation.ReportParts["EnvironmentSocialImpact"];
                    parameters += "&IS_REFORMULATED_SECTION_DISPLAY=" +
                            modelPMROperation.ReportParts["Reformulation"];
                    parameters += "&IS_IMPACT_SECTION_DISPLAY=" +
                            modelPMROperation.ReportParts["Impacts"];
                    parameters += "&IS_IMPIND_DETAILS_SECTION_DISPLAY=" +
                            modelPMROperation.ReportParts["ImpactIndicators"];
                    parameters += "&IS_OUTCOME_DISPLAY=" +
                            modelPMROperation.ReportParts["Outcomes"];
                    parameters += "&IS_OUTPUT_PHYSICAL_DISPLAY=" +
                            modelPMROperation.ReportParts["OutputsPhysicalProgress"];
                    parameters += "&IS_OUTPUT_FINANCIAL_DISPLAY=" +
                            modelPMROperation.ReportParts["OutputsFinancialProgress"];
                    parameters += "&IS_OUTPUT_ANNUAL_DISPLAY=" +
                            modelPMROperation.ReportParts["OutputsAnnualProgress"];
                    parameters += "&IS_OUTPUT_INDICATOR_DISPLAY=" +
                            modelPMROperation.ReportParts["OutcomesIndicators"];
                    parameters += "&IS_RISKS_DISPLAY=" +
                            modelPMROperation.ReportParts["Risk"];
                    parameters += "&IS_OUT_INDICATORS_DETAILS_DISPLAY=" +
                            modelPMROperation.ReportParts["OutcomesIndicators"];
                    parameters += "&IS_CHANGES_MATRIX_DISPLAY=" +
                            modelPMROperation.ReportParts["MatrixChanges"];
                    parameters += "&IS_EVENTS_DATA=" +
                            modelPMROperation.ReportParts["Events"];
                    parameters += "&IS_TOTAL_COST_DISPLAY=" +
                            modelPMROperation.ReportParts["TotalCostSource"];
                    parameters += "&IS_AVAILABLE_FUNDS_DISPLAY=" +
                            modelPMROperation.ReportParts["AvailableFunds"];
                    parameters += "&IS_DISAGGREGATIONS_DISPLAY=" +
                            modelPMROperation.ReportParts["DisaggregationCategories"];
                    parameters += "&IS_OPERATION_BASIC_DATA_DISPLAY=" +
                            modelPMROperation.ReportParts["OperationBasicData"];
                    parameters += "&IS_PMI_GRAPHICS=" +
                            modelPMROperation.ReportParts["PMIGraphics"];
                    parameters += "&IS_CLASSIFICATION_VALIDATION=" +
                            modelPMROperation.ReportParts["ClassificationValidation"];
                    parameters += "&IS_FR_RESOLVED_DISPLAY=" +
                            modelPMROperation.ReportParts["DELAY_SOLVED"];
                    parameters += "&IS_FR_UNRESOLVED_DISPLAY=" +
                            modelPMROperation.ReportParts["DELAY_UNSOLVED"];
                    parameters += "&IS_DISBURSEMENT_DISPLAY=" +
                            modelPMROperation.ReportParts["Disbursements"];
                    parameters += "&IS_DEM_DISPLAY=" +
                        modelPMROperation.ReportParts["DEMInformation"];
                    parameters += "&IS_EXPENDITURE_CATEGORIES_DISPLAY=" +
                        modelPMROperation.ReportParts["ExpenseCategories"];
                    parameters += "&IS_FR_DISPLAY=" +
                        modelPMROperation.ReportParts["FindingRecommendation"];
                    parameters += "&HIDE_CLASSIFICATION=" +
                        StageController.IsSpecialOperationOrSUPWithoutRelation(modelPMROperation.OperationId);
                }

                string url = reportServer + report;

                url += parameters + "&rs:Format=" + modelPMROperation.ExportTypeReport["ExportReport"];

                return Redirect(url);
            }
        }

        public virtual JsonResult GetFullUrlPMRReport(PMROperationReportModel model)
        {
            List<PmrCycleModel> listCycles = GetListCycles();

            bool isRedirectToEDWReport = IsRedirectToEDWReport(model, listCycles);

            string PMROperationURL = string.Empty;

            if (isRedirectToEDWReport)
            {
                PMROperationURL = ConfigurationServiceFactory.Current.GetApplicationSettings().UrlPMREDWReport;
                PMROperationURL += "&p_pCycle=" + model.PMRCycleId.Max();
                PMROperationURL += "&p_pOperNumber=" + IDBContext.Current.Operation;
                PMROperationURL += "&p_pSection=";
                PMROperationURL += "IS_OPERATION_OBJECTIVE,";
                PMROperationURL += "IS_INACTIVE_OUTPUTS_DISPLAY,";
                PMROperationURL += "IS_ENVIRONMENT_SECTION_DISPLAY,";
                PMROperationURL += "IS_REFORMULATED_SECTION_DISPLAY,";
                PMROperationURL += "IS_IMPACT_SECTION_DISPLAY,";
                PMROperationURL += "IS_OUTCOME_DISPLAY,";
                PMROperationURL += "IS_OUTPUT_PHYSICAL_DISPLAY,";
                PMROperationURL += "IS_OUTPUT_FINANCIAL_DISPLAY,";
                PMROperationURL += "IS_RISKS_DISPLAY,";
                PMROperationURL += "IS_OUT_INDICATORS_DETAILS_DISPLAY,";
                PMROperationURL += "IS_CHANGES_MATRIX_DISPLAY,";
                PMROperationURL += "IS_EVENTS_DATA,";
                PMROperationURL += "IS_TOTAL_COST_DISPLAY,";
                PMROperationURL += "IS_AVAILABLE_FUNDS_DISPLAY,";
                PMROperationURL += "IS_DISAGGREGATIONS_DISPLAY,";
                PMROperationURL += "IS_OPERATION_BASIC_DATA_DISPLAY,";
                PMROperationURL += "IS_PMI_GRAPHICS,";
                PMROperationURL += "IS_CLASSIFICATION_VALIDATION,";
                PMROperationURL += "IS_FR_RESOLVED_DISPLAY,";
                PMROperationURL += "IS_FR_UNRESOLVED_DISPLAY,";
                PMROperationURL += "IS_DISBURSEMENT_DISPLAY,";
                PMROperationURL += "IS_DEM_DISPLAY,";
                PMROperationURL += "IS_EXPENDITURE_CATEGORIES_DISPLAY,";
                PMROperationURL += "IS_FR_DISPLAY";
            }
            else
            {
                model.ResultMatrixID = ClientForForGenericReports.GetResultMatrixIDForOperationNumber(model.OperationId);

                if (ConfigurationManager.AppSettings["ParamDownloadReportServer"] == null ||
                    ConfigurationManager.AppSettings["ParamServer"] == null ||
                    ConfigurationManager.AppSettings["ParamForPMROperationReport"] == null)
                {
                    Logger.GetLogger().WriteWarning(
                        "ReportManager", "Missing configuration entries in web.config");

                    return Json(new { reportpath = string.Empty });
                }

                string reportServer = ConfigurationManager.AppSettings["ParamDownloadReportServer"].ToString();
                string server = ConfigurationManager.AppSettings["ParamServer"].ToString();
                string report = ConfigurationManager.AppSettings["ParamForPMROperationReport"].ToString();

                PMROperationURL = reportServer + server + report;

                if (Lang != null)
                {
                    PMROperationURL += "&LANG=" + Lang;
                }
                else
                {
                    PMROperationURL += "&LANG=EN";
                }

                PMROperationURL += "&OPERATION=" + model.OperationId;

                if (model.PMRCycleId != null)
                {
                    for (int x = 0; x < model.PMRCycleId.Count; x++)
                    {
                        PMROperationURL += "&CYCLE=" + model.PMRCycleId[x];
                    }
                }

                PMROperationURL += "&IS_OPERATION_OBJECTIVE=True";
                PMROperationURL += "&IS_INACTIVE_OUTPUTS_DISPLAY=True";
                PMROperationURL += "&IS_ENVIRONMENT_SECTION_DISPLAY=True";
                PMROperationURL += "&IS_REFORMULATED_SECTION_DISPLAY=True";
                PMROperationURL += "&IS_IMPACT_SECTION_DISPLAY=True";
                PMROperationURL += "&IS_IMPIND_DETAILS_SECTION_DISPLAY=False";
                PMROperationURL += "&IS_OUTCOME_DISPLAY=True";
                PMROperationURL += "&IS_OUTPUT_PHYSICAL_DISPLAY=True";
                PMROperationURL += "&IS_OUTPUT_FINANCIAL_DISPLAY=True";
                PMROperationURL += "&IS_OUTPUT_ANNUAL_DISPLAY=False";
                PMROperationURL += "&IS_OUTPUT_INDICATOR_DISPLAY=False";
                PMROperationURL += "&IS_RISKS_DISPLAY=True";
                PMROperationURL += "&IS_OUT_INDICATORS_DETAILS_DISPLAY=True";
                PMROperationURL += "&IS_CHANGES_MATRIX_DISPLAY=True";
                PMROperationURL += "&IS_EVENTS_DATA=True";
                PMROperationURL += "&IS_TOTAL_COST_DISPLAY=True";
                PMROperationURL += "&IS_AVAILABLE_FUNDS_DISPLAY=True";
                PMROperationURL += "&IS_DISAGGREGATIONS_DISPLAY=True";
                PMROperationURL += "&IS_OPERATION_BASIC_DATA_DISPLAY=True";
                PMROperationURL += "&IS_PMI_GRAPHICS=True";
                PMROperationURL += "&IS_CLASSIFICATION_VALIDATION=True";
                PMROperationURL += "&IS_FR_RESOLVED_DISPLAY=True";
                PMROperationURL += "&IS_FR_UNRESOLVED_DISPLAY=True";
                PMROperationURL += "&IS_DISBURSEMENT_DISPLAY=True";
                PMROperationURL += "&IS_DEM_DISPLAY=True";
                PMROperationURL += "&IS_EXPENDITURE_CATEGORIES_DISPLAY=True";
                PMROperationURL += "&IS_FR_DISPLAY=True";
                PMROperationURL += "&HIDE_CLASSIFICATION=" +
                StageController.IsSpecialOperationOrSUPWithoutRelation(model.OperationId);
            }

            PMROperationURL += "&rs:Format=" + model.TypeReport;

            return Json(new { reportpath = PMROperationURL });
        }

        private bool IsRedirectToEDWReport(
            PMROperationReportModel modelPMROperation,
            List<PmrCycleModel> pmrCycleModels)
        {
            if (modelPMROperation.PMRCycleId == null)
                return false;

            if (ConfigurationServiceFactory.Current.GetApplicationSettings()
                .IsPMREDWReportRedirectEnabled)
            {
                var startDateLastPmrCycleEdw = pmrCycleModels
                    .Where(a => a.Code == PMRCycleValues.EDW_REPORT_CODE)
                    .Select(a => a.StartDate)
                    .FirstOrDefault();

                var edwEnabledCycleIds = pmrCycleModels
                    .Where(a => (a.StartDate >= startDateLastPmrCycleEdw)
                        && (a.CycleType == PMRCycleValues.PMR_CYCLE_TYPE_SG))
                    .Select(a => a.PmrCycleId);

                return modelPMROperation.PMRCycleId.Intersect(edwEnabledCycleIds).HasAny();
            }

            return false;
        }

        private List<PmrCycleModel> GetListCycles()
        {
            idOperation = ClientForForGenericReports
                .GetOperationIDForOperationNumber(IDBContext.Current.Operation);

            return ClientPMRForCycleModel.GetCyclesForPMR(
                IDBContext.Current.CurrentLanguage, idOperation ?? 0)
                .OrderByDescending(x => x.PmrCycleId).ToList();
        }
        #endregion
    }
}
