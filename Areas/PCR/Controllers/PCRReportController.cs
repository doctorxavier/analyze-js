using System.Web.Mvc;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.PCRModule.Services.ChecklistService;
using IDB.MW.Application.PCRModule.Services.FollowUpService;
using IDB.MW.Infrastructure.ReportManager.Enums;
using IDB.MW.Infrastructure.SecurityService;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.Presentation.MVC4.Areas.PCR.Models;
using IDB.Presentation.MVC4.Controllers;

namespace IDB.Presentation.MVC4.Areas.PCR.Controllers
{
    public partial class PCRReportController : BaseController
    {
        #region Contructors
        public PCRReportController(
            IPCRChecklistService pcrChecklistService,
            ICatalogService catalogService, 
            IPCRFollowUpService pcrFollowUpService, 
            IAuthorizationService authorizationService)
        {
            _pcrFollowUpService = pcrFollowUpService;
            _viewModelMapperHelper = new ViewModelMapperHelper(ViewBag, pcrChecklistService, catalogService, authorizationService, _pcrFollowUpService);
        }

        #endregion

        #region Fields

        private readonly ViewModelMapperHelper _viewModelMapperHelper;
        private readonly IPCRFollowUpService _pcrFollowUpService;

        #endregion

        public virtual ActionResult PCRMonitoringReport()
        {
            _viewModelMapperHelper.GetMonitoringReport();

            return View();
        }

        public virtual FileResult DownloadExcelReport(string monitoringStatus, string exerciseYear, string divisionGrouping, string countryDepartment, string sectorDepartment, string operations, string country, string division, bool allCountries, bool allMonitoringStatus, bool allCountryDepartment, bool allSectorDepartment, bool allDivision)
        {
            var response = _viewModelMapperHelper.FileReport(monitoringStatus, exerciseYear, divisionGrouping, countryDepartment, sectorDepartment, operations, country, division, allCountries, allMonitoringStatus, allCountryDepartment, allSectorDepartment, allDivision, OutputFormatEnum.Excel);

            return (!response.IsValid || response.File == null || response.File.Length == 0) ?
                null : File(response.File, "application/vnd.ms-excel");
        }

        public virtual FileResult DownloadPdfReport(string monitoringStatus, string exerciseYear, string divisionGrouping, string countryDepartment, string sectorDepartment, string operations, string country, string division, bool allCountries, bool allMonitoringStatus, bool allCountryDepartment, bool allSectorDepartment, bool allDivision)
        {
            var response = _viewModelMapperHelper.FileReport(monitoringStatus, exerciseYear, divisionGrouping, countryDepartment, sectorDepartment, operations, country, division, allCountries, allMonitoringStatus, allCountryDepartment, allSectorDepartment, allDivision, OutputFormatEnum.PDF);

            return (!response.IsValid || response.File == null || response.File.Length == 0) ?
                null : File(response.File, "application/pdf");
        }

        public virtual ActionResult PCRMonitoringReportFilterSubmit(string monitoringStatus, string exerciseYear, string divisionGrouping, string countryDepartment, string sectorDepartment, string operations, string country, string division, bool allCountries, bool allMonitoringStatus, bool allCountryDepartment, bool allSectorDepartment, bool allDivision)
        {
            var paramsReport = _viewModelMapperHelper.SetFilterOptions(monitoringStatus, exerciseYear, divisionGrouping, countryDepartment, sectorDepartment, operations, country, division, allCountries, allMonitoringStatus, allCountryDepartment, allSectorDepartment, allDivision);

            var submitMonitoringReportResponse = _pcrFollowUpService.SubmitMonitoringReport(paramsReport, OutputFormatEnum.PDF);
            if (submitMonitoringReportResponse.IsValid)
            {
                ViewBag.ReportDataSource = submitMonitoringReportResponse.MonitoringReportList;
                ViewBag.ReportDefinition = submitMonitoringReportResponse.ReportDefinition;
            }

            return PartialView("Partials/Reports/PCRMonitoringReportViewer");
        }
    }
}