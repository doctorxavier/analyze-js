using System.Collections.Generic;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.OpusMissionsModule.Services.MissionService;
using IDB.MW.Application.OpusMissionsModule.Services.ReportsService;
using IDB.MW.Infrastructure.ReportManager.Enums;
using IDB.MW.Infrastructure.SecurityService;
using IDB.Presentation.MVC4.Areas.OpusMissions.Models;
using IDB.Presentation.MVC4.Areas.OpusMissions.Models.Reports;

namespace IDB.Presentation.MVC4.Areas.OpusMissions.Controllers
{
    public partial class MissionsReportController : MVC4.Controllers.ConfluenceController
    {
        #region Fields
        private readonly ViewModelMapperHelper _viewModelMapperHelper;
        private readonly IMissionReportService _missionReportService;
        private readonly IMissionService _missionService;
        #endregion

        #region Contructors
        public MissionsReportController(ICatalogService catalogService, IMissionReportService missionReportService, IMissionService missionService)
        {
            var authorizationService = AuthorizationServiceFactory.Current;
            _missionReportService = missionReportService;
            _missionService = missionService;
            _viewModelMapperHelper = new ViewModelMapperHelper(ViewBag, catalogService, authorizationService, missionService, missionReportService);
        }
        #endregion

        public virtual ActionResult MissionsReportFilter()
        {
            _viewModelMapperHelper.GetMonitoringReport();
            return View();
        }

        public virtual FileResult DownloadPdfReport(string statusListFront, string typeListFront, string countryDepartmentListFront, string sectorDepartmentListFront, string countryListFront, string divisionListFront, bool allCountries, bool allStatus, bool allCountryDepartment, bool allSectorDepartment, bool allDivision, bool allType, string operationNumber, string missionMembers, string startDate, string endDate)
        {
            var response = _viewModelMapperHelper.FileReport(statusListFront, typeListFront, sectorDepartmentListFront, divisionListFront, countryDepartmentListFront, countryListFront, allStatus, allCountryDepartment, allCountries, allType, allSectorDepartment, allDivision, operationNumber, missionMembers, startDate, endDate, OutputFormatEnum.PDF);

            return !response.IsValid ? null : File(response.File, "application/pdf");
        }

        public virtual FileResult DownloadExcelReport(string statusListFront, string typeListFront, string countryDepartmentListFront, string sectorDepartmentListFront, string countryListFront, string divisionListFront, bool allCountries, bool allStatus, bool allCountryDepartment, bool allSectorDepartment, bool allDivision, bool allType, string operationNumber, string missionMembers, string startDate, string endDate)
        {
            var response = _viewModelMapperHelper.FileReport(statusListFront, typeListFront, sectorDepartmentListFront, divisionListFront, countryDepartmentListFront, countryListFront, allStatus, allCountryDepartment, allCountries, allType, allSectorDepartment, allDivision, operationNumber, missionMembers, startDate, endDate, OutputFormatEnum.Excel);

            return !response.IsValid ? null : File(response.File, "application/vnd.ms-excel", "MissionsReport.xls");
        }

        public virtual ActionResult MissionReportFilterSubmit(string statusListFront, string typeListFront, string countryDepartmentListFront, string sectorDepartmentListFront, string countryListFront, string divisionListFront, bool allCountries, bool allStatus, bool allCountryDepartment, bool allSectorDepartment, bool allDivision, bool allType, string operationNumber, string missionMembers, string startDate, string endDate)
        {
            operationNumber = operationNumber.Replace('_', ' ').ToUpper();
            missionMembers = missionMembers.Replace('_', ' ').ToUpper();

            var paramsReport = _viewModelMapperHelper.SetFilterOptions(statusListFront, typeListFront, sectorDepartmentListFront, divisionListFront, countryDepartmentListFront, countryListFront, allStatus, allCountryDepartment, allCountries, allType, allSectorDepartment, allDivision, operationNumber, missionMembers, startDate, endDate);

            var submitReportResponse = _missionReportService.SubmitReport(paramsReport);
            MissionsReportHeadViewModel missionsReportHeadViewModel = null;
            if (submitReportResponse.IsValid)
            {
                missionsReportHeadViewModel = _viewModelMapperHelper.GetResultHeadReport(submitReportResponse);
            }

            return PartialView("Partials/MissionsReportHeadResult", missionsReportHeadViewModel);
        }
    }
}