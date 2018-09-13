using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.MW.Domain.Contracts.ModelRepositories.Administration;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.Reports;
using IDB.MW.Domain.Contracts.ModelRepositories.Reports.Generic;
using IDB.MW.Domain.Models.Architecture.Reports.PMRExtraModels.PMRAggregate;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Report;

namespace IDB.Presentation.MVC4.Areas.Reports.Controllers
{
    public partial class PMRAggregateReportController : BaseController
    {
        // GET: /Reports/PMRAggregateReport/
        #region Interface Definitions

        private IPmrCycleModelRepository _ClientPMRForCycleModel = null;
        public IPmrCycleModelRepository ClientPMRForCycleModel
        {
            get { return _ClientPMRForCycleModel; }
            set { _ClientPMRForCycleModel = value; }
        }

        private IReportsGenericRepository _ClientForGenericReports = null;
        public IReportsGenericRepository ClientForGenericReports
        {
            get { return _ClientForGenericReports; }
            set { _ClientForGenericReports = value; }
        }

        //IAdministrationModelRepository
        private IAdministrationModelRepository _AdministrationModelRepository = null;
        public IAdministrationModelRepository AdministrationModelRepository
        {
            get { return _AdministrationModelRepository; }
            set { _AdministrationModelRepository = value; }
        }
        #endregion

        #region Variables

        private string Lang = "EN";

        #endregion

        public virtual ActionResult PMRAggregateReportCreate()
        {
            PMRAggregateReportModel ModelOperationReport = new PMRAggregateReportModel();

            var ListPMRCycles = ClientPMRForCycleModel.GetAllCyclesSeptemberAndMarchCycles(Lang).OrderByDescending(x => x.PmrCycleId).ToList();
            var ListContry = ClientForGenericReports.GetCountries(Lang).OrderBy(x => x.Name).ToList();
            var ListContryDepartment = ClientForGenericReports.GetCountryDepartments(Lang).OrderBy(x => x.Name).ToList();
            var ListSectorDepartment = ClientForGenericReports.GetSectorDepartment(Lang).OrderBy(x => x.Name).ToList();
            var ListDivision = ClientForGenericReports.GetDivisions(Lang).OrderBy(x => x.Name).ToList();
            var ListEsgTrakingOptions = new Dictionary<bool, string>();
            ListEsgTrakingOptions.Add(true, Localization.GetText("Yes"));
            ListEsgTrakingOptions.Add(false, Localization.GetText("No"));

            ViewBag.ListPMRCycle = new SelectList(ListPMRCycles, "PMRCycleId", "PmrCycleName");
            ViewBag.ListContry = new MultiSelectList(ListContry, "ConvergenceMasterDataId", "Name");
            ViewBag.ListContryDepartment = new MultiSelectList(ListContryDepartment, "ConvergenceMasterDataId", "Name");
            ViewBag.ListSectorDepartment = new MultiSelectList(ListSectorDepartment, "ConvergenceMasterDataId", "Name");
            ViewBag.ListDivision = new MultiSelectList(ListDivision, "ConvergenceMasterDataId", "Name");
            ViewBag.ListESGTrakingOptions = new SelectList(ListEsgTrakingOptions, "key", "value");

            return View();
        }

        [HttpPost()]
        public virtual ActionResult PMRAggregateReportCreate(PMRAggregateReportModel ModelPMRAggregateReport)
        {
            string URLPMRPorfolioReport = string.Empty;
            
            string Header = ReportBuilder.GetReportHeader();

            URLPMRPorfolioReport += ReportBuilder.GetReportPreffix("ParamForPMRPorfolioReport");

            if (Lang != null)
            {
                URLPMRPorfolioReport += "&LANG=" + Lang;
            }
            else
            {
                URLPMRPorfolioReport += "&LANG=EN";
            }

            if (ModelPMRAggregateReport.Country.Count > 0)
            {
                foreach (var itemCountry in ModelPMRAggregateReport.Country)
                {
                    URLPMRPorfolioReport += "&COUNTRY=" + itemCountry;
                }
            }
            else
            {
                URLPMRPorfolioReport += "&COUNTRY=-1";
            }

            if (ModelPMRAggregateReport.CountryDepartment.Count > 0)
            {
                foreach (var itemCountryDepartment in ModelPMRAggregateReport.CountryDepartment)
                {
                    URLPMRPorfolioReport += "&COUNTRY_DEPARTMENT=" + itemCountryDepartment;
                }
            }
            else
            {
                URLPMRPorfolioReport += "&COUNTRY_DEPARTMENT=-1";
            }

            if (ModelPMRAggregateReport.Division.Count > 0)
            {
                foreach (var itemDivision in ModelPMRAggregateReport.Division)
                {
                    URLPMRPorfolioReport += "&DIVISION=" + itemDivision;
                }
            }
            else
            {
                URLPMRPorfolioReport += "&DIVISION=-1";
            }

            if (ModelPMRAggregateReport.SectorDepartment.Count > 0)
            {
                foreach (var itemSectorDepartment in ModelPMRAggregateReport.SectorDepartment)
                {
                    URLPMRPorfolioReport += "&DEPARTMENT=" + itemSectorDepartment;
                }
            }
            else
            {
                URLPMRPorfolioReport += "&DEPARTMENT=-1";
            }

            if (ModelPMRAggregateReport.ESGTracking)
            {
                URLPMRPorfolioReport += "&ESG_TRACKING=1";
            }
            else
            {
                URLPMRPorfolioReport += "&ESG_TRACKING=0";
            }

            URLPMRPorfolioReport += "&PMR_CYCLE_ID=" + ModelPMRAggregateReport.PMRCycleId;

            return Content(URLPMRPorfolioReport + Header);
        }

        [HttpPost()]
        public virtual ActionResult PMRAggregateReportExport(PMRAggregateReportModel ModelPMRAggregateReport)
        {
            string URLPMRPorfolioReportExport = string.Empty;

            URLPMRPorfolioReportExport += ReportBuilder.GetReportPreffix("ParamForPMRPorfolioReport");

            if (Lang != null)
            {
                URLPMRPorfolioReportExport += "&LANG=" + Lang;
            }
            else
            {
                URLPMRPorfolioReportExport += "&LANG=EN";
            }

            if (ModelPMRAggregateReport.Country.Count > 0)
            {
                foreach (var itemCountry in ModelPMRAggregateReport.Country)
                {
                    URLPMRPorfolioReportExport += "&COUNTRY=" + itemCountry;
                }
            }
            else
            {
                URLPMRPorfolioReportExport += "&COUNTRY=-1";
            }

            if (ModelPMRAggregateReport.CountryDepartment.Count > 0)
            {
                foreach (var itemCountryDepartment in ModelPMRAggregateReport.CountryDepartment)
                {
                    URLPMRPorfolioReportExport += "&COUNTRY_DEPARTMENT=" + itemCountryDepartment;
                }
            }
            else
            {
                URLPMRPorfolioReportExport += "&COUNTRY_DEPARTMENT=-1";
            }

            if (ModelPMRAggregateReport.Division.Count > 0)
            {
                foreach (var itemDivision in ModelPMRAggregateReport.Division)
                {
                    URLPMRPorfolioReportExport += "&DIVISION=" + itemDivision;
                }
            }
            else
            {
                URLPMRPorfolioReportExport += "&DIVISION=-1";
            }

            if (ModelPMRAggregateReport.SectorDepartment.Count > 0)
            {
                foreach (var itemSectorDepartment in ModelPMRAggregateReport.SectorDepartment)
                {
                    URLPMRPorfolioReportExport += "&DEPARTMENT=" + itemSectorDepartment;
                }
            }
            else
            {
                URLPMRPorfolioReportExport += "&DEPARTMENT=-1";
            }

            if (ModelPMRAggregateReport.ESGTracking)
            {
                URLPMRPorfolioReportExport += "&ESG_TRACKING=1";
            }
            else
            {
                URLPMRPorfolioReportExport += "&ESG_TRACKING=0";
            }

            URLPMRPorfolioReportExport += "&PMR_CYCLE_ID=" + ModelPMRAggregateReport.PMRCycleId;

            URLPMRPorfolioReportExport += "&rs:Format=" + ModelPMRAggregateReport.ExportType;

            return Content(URLPMRPorfolioReportExport);
        }

        public virtual JsonResult PMRAggregateFilterCountrys(PMRAggregateReportModel ModelPMRAggregateReport)
        {
            var ListCountrys = ClientForGenericReports.GetCountriesFilter(Lang, ModelPMRAggregateReport.CountryDepartment).OrderBy(x => x.Name).ToList();
            return new JsonResult() { Data = ListCountrys };
        }

        public virtual JsonResult PMRAggregateFilterDivision(PMRAggregateReportModel ModelPMRAggregateReport)
        {
            var ListDivision = ClientForGenericReports.GetDivisionFilter(Lang, ModelPMRAggregateReport.SectorDepartment).OrderBy(x => x.Name).ToList();
            return new JsonResult() { Data = ListDivision };
        }

        [HttpPost]
        public virtual JsonResult PMRAggregateCountRegisters(int[] countryDepartment, int[] paises, int[] department, int[] division, int pmr_cycle_id, bool esg_tracking)
        {
            var lang = IDBContext.Current.CurrentLanguage;
            int cantRegisters = AdministrationModelRepository.CountRegistersPMRAggregate(esg_tracking, pmr_cycle_id, countryDepartment, paises, department, division, Lang);

            return new JsonResult() { Data = cantRegisters };
        }
    }
}
