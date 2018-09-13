using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.Logging;
using IDB.Architecture.Language;
using IDB.MW.Domain.Contracts.ModelRepositories.Reports.Generic;
using IDB.MW.Domain.Models.Architecture.MasterDefinitions;
using IDB.MW.Domain.Models.Architecture.Reports.PMRExtraModels.Visualization;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Report;
using IDB.Presentation.MVC4.Areas.Reports.BusinessLogic;

namespace IDB.Presentation.MVC4.Areas.Reports.Controllers
{
    public partial class VisualizationController : BaseController
    {
        private const string PDF_FORMAT = "PDF";
        private const string EXCEL_FORMAT = "EXCELOPENXML";
        private const string TypeReportMapOperation = "VisualizationReport.cboReportType.MapOperation";
        private const string TypeReportVisualOutput = "VisualizationReport.cboReportType.VisualOutputs";
        private const string TypeReportAggregated = "VisualizationReport.cboReportType.Aggregated";
        private const string TitleMapOperation = "VisualizationReport.cboReportTypeTitle.MapOperation";
        private const string TitleVisualOutput = "VisualizationReport.cboReportTypeTitle.VisualOutputs";
        private const string TitleAggregated = "VisualizationReport.cboReportTypeTitle.Aggregated";

        private VisualizationBusinessLogic _filterBusinesslogic;

        #region ENPOINTS

        private IReportsGenericRepository _clientForGenericReports = null;
        public IReportsGenericRepository ClientForGenericReports
        {
            get { return _clientForGenericReports; }
            set { _clientForGenericReports = value; }
        }
        #endregion

        // GET: Reports/VisualizationReport
        public virtual ActionResult VisualizationReport()
        {
            VisualizationFilterModel filterModel = new VisualizationFilterModel
            {
                Language = IDBContext.Current.CurrentLanguage
            };

            VisualizationReportModel model = CreateModel(filterModel);
            TempData["Model"] = model;
            return View(model);
        }

        public virtual JsonResult VisualizationFilterCountries(VisualizationReportModel model)
        {
            _filterBusinesslogic = new VisualizationBusinessLogic();
            model.Countries = _filterBusinesslogic.FilterCountries(ClientForGenericReports
           .GetCountriesFilter(IDBContext.Current.CurrentLanguage, model.SelectedCountryDepartments));

            return new JsonResult() { Data = model.Countries };
        }

        public virtual JsonResult VisualizationFilterDivisions(VisualizationReportModel model)
        {
            if (model.SelectedSectorDepartments.Count > 0)
            {
                model.Divisions = FilterDivisionsForSectorDepartments(model.SelectedSectorDepartments);

                return new JsonResult() { Data = model.Divisions };
            }

            LoadSectorsAndDivisions(model);

            return new JsonResult() { Data = model.Divisions };
        }

        [HttpPost()]
        public virtual ActionResult ShowEmbeddedVisualizationReport(
            string reportParametersEmbedded, string reportParametersReportTypeEmbedded)
        {
            try
            {
                string urlReportEmbedded = string.Empty;
                List<string> reportName = GetReportNameFromReportType(reportParametersReportTypeEmbedded);
                string reportUrl = ReportBuilder.GetReportPreffix(reportName.ElementAt(0));
                string header = ReportBuilder.GetReportHeader();

                urlReportEmbedded += reportUrl;
                urlReportEmbedded += "&LANGUAGE=" + IDBContext.Current.CurrentLanguage;
                urlReportEmbedded += reportParametersEmbedded;
                urlReportEmbedded += "&TITLE_REPORT=" + reportName.ElementAt(1);
                urlReportEmbedded += header;

                return Json(new { data = urlReportEmbedded }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Logger.GetLogger().WriteError(
                    "Visualization", "Error when building custom Visualization report", ex);
                throw;
            }
        }

        [HttpPost]
        public virtual FileResult GeneratePDFCompositeReport(
            string reportParametersPDF, string reportParametersReportTypePDF)
        {
            try
            {
                List<string> reportName = GetReportNameFromReportType(reportParametersReportTypePDF);
                reportParametersPDF = reportParametersPDF.Replace("rp:", string.Empty);
                reportParametersPDF += "&LANGUAGE=" + IDBContext.Current.CurrentLanguage;
                reportParametersPDF += "&TITLE_REPORT=" + reportName.ElementAt(1);

                return File(ReportBuilder.DownloadReport(
                    ConfigurationManager.AppSettings[reportName.ElementAt(0)],
                    reportParametersPDF,
                    PDF_FORMAT),
                    "application/vnd.pdf",
                    string.Format(
                        "VisualizationReport-{0}.pdf",
                        reportParametersReportTypePDF));
            }
            catch (Exception ex)
            {
                Logger.GetLogger().WriteError(
                    "Visualization", "Error when building custom Visualization report PDF", ex);
                throw;
            }
        }

        [HttpPost]
        public virtual FileResult GenerateExcelCompositeReport(
            string reportParametersExcel, string reportParametersReportTypeExcel)
        {
            try
            {
                List<string> reportName = GetReportNameFromReportType(reportParametersReportTypeExcel);
                reportParametersExcel = reportParametersExcel.Replace("rp:", string.Empty);
                reportParametersExcel += "&LANGUAGE=" + IDBContext.Current.CurrentLanguage;
                reportParametersExcel += "&TITLE_REPORT=" + reportName.ElementAt(1);

                return File(ReportBuilder.DownloadReport(
                    ConfigurationManager.AppSettings[reportName.ElementAt(0)],
                    reportParametersExcel,
                    EXCEL_FORMAT),
                    "application/vnd.ms-excel",
                    "VisualizationReport.xls");
            }
            catch (Exception ex)
            {
                Logger.GetLogger().WriteError(
                    "Visualization", "Error when building custom Visualization report Excel", ex);
                throw;
            }
        }

        VisualizationReportModel CreateModel(VisualizationFilterModel filterModel)
        {
            VisualizationReportModel result = new VisualizationReportModel();
            string language = IDBContext.Current.CurrentLanguage;

            _filterBusinesslogic = new VisualizationBusinessLogic();

            result.CountryDepartments =
                _filterBusinesslogic.FilterCountryDepartments(
                ClientForGenericReports.GetCountryDepartments(language));

            result.Countries = _filterBusinesslogic.FilterCountries(
                ClientForGenericReports.GetCountries(language));

            result.PublicationStatus = _filterBusinesslogic.FilterPublicationsStatus(
                ClientForGenericReports.GetPublicationStatus(language));

            result.ReportTypes = _filterBusinesslogic.FilterReportTypes(
                ClientForGenericReports.GetReportTypes(language));

            LoadSectorsAndDivisions(result);

            return result;
        }

        IList<ConvergenceMasterDataModel> FilterDivisionsForSectorDepartments(
           List<int> sectorIds)
        {
            _filterBusinesslogic = new VisualizationBusinessLogic();

            return _filterBusinesslogic.FilterDivisions(
                ClientForGenericReports.GetDivisionFilter(
                IDBContext.Current.CurrentLanguage, sectorIds));
        }

        void LoadSectorsAndDivisions(VisualizationReportModel model)
        {
            _filterBusinesslogic = new VisualizationBusinessLogic();

            model.SectorDepartments =
                _filterBusinesslogic.FilterSectorDepartment(
                ClientForGenericReports
                .GetSectorDepartment(IDBContext.Current.CurrentLanguage));

            List<int> sectorIds =
                model.SectorDepartments.Select(x => x.ConvergenceMasterDataId).ToList();

            model.Divisions = FilterDivisionsForSectorDepartments(sectorIds);
        }

        VisualizationFilterModel CreateFilteredModel(VisualizationReportModel model)
        {
            VisualizationFilterModel filterModel = new VisualizationFilterModel
            {
                Language = IDBContext.Current.CurrentLanguage,
                CountryDepartment = model.SelectedCountryDepartments.Count == 0 ? null
                    : string.Join(",", model.SelectedCountryDepartments),
                SectorDepartment = model.SelectedSectorDepartments.Count == 0 ? null
                    : string.Join(",", model.SelectedSectorDepartments),
                Country = model.SelectedCountries.Count == 0 ? null
                    : string.Join(",", model.SelectedCountries),
                Division = model.SelectedDivisions.Count == 0 ? null
                    : string.Join(",", model.SelectedDivisions),
                PublicationStatus = model.SelectedPublicationStatus.First() == 0 ? null
                    : string.Join(",", model.SelectedPublicationStatus),
                ReportType = model.SelectedReportTypes.Count == 0 ? null
                : string.Join(",", model.SelectedReportTypes),
                OperationNumber = model.OperationNumber == null ? null
                    : string.Join(",", model.OperationNumber),
            };

            return filterModel;
        }

        List<string> GetReportNameFromReportType(string reportType)
        {
            List<string> parametersName = new List<string>();
            if (reportType == Localization.GetText(TypeReportMapOperation))
            {
                parametersName.Add("VisualizationMapOperation");
                parametersName.Add(Localization.GetText(TitleMapOperation));
                return parametersName;
            }

            if (reportType == Localization.GetText(TypeReportVisualOutput))
            {
                parametersName.Add("VisualOutputsDetailed");
                parametersName.Add(Localization.GetText(TitleVisualOutput));
                return parametersName;
            }

            parametersName.Add("AggregatedVisualization");
            parametersName.Add(Localization.GetText(TitleAggregated));
            return parametersName;
        }
    }
}