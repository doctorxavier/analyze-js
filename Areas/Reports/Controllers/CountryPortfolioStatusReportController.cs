using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.Extensions;
using IDB.Architecture.Language;
using IDB.MW.Domain.Contracts.ModelRepositories.Reports.Generic;
using IDB.MW.Domain.Models.Architecture.Enumerations;
using IDB.MW.Domain.Models.Architecture.Reports.PMRExtraModels.PorfolioStatus;
using IDB.Presentation.MVC4.Report;

namespace IDB.Presentation.MVC4.Areas.Reports.Controllers
{
    public partial class CountryPortfolioStatusReportController : MVC4.Controllers.ConfluenceController
    {
        #region Repositories

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

        public virtual ActionResult Index(string OperationNumber, string Report)
        {
            PorfolioStatusModel PorfolioModel = new PorfolioStatusModel();
            PorfolioModel.OperationId = ClientGenericRepositoty.GetOperationIDForOperationNumber(OperationNumber);
            ViewBag.ReportSelected = Report;

            if (string.Compare(Report, DashReportsCodes.PMR.GetStringValue(), true) == 0)
            {
                ViewBag.ReportType = Localization.GetText("PMR");
            }
            else if (string.Compare(Report, DashReportsCodes.SupervisionPlan.GetStringValue(), true) == 0)
            {
                ViewBag.ReportType = Localization.GetText("Supervision Plan");
            }
            else if (string.Compare(Report, DashReportsCodes.Risks.GetStringValue(), true) == 0)
            {
                ViewBag.ReportType = Localization.GetText("Risks");
            }
            else if (string.Compare(Report, DashReportsCodes.Clauses.GetStringValue(), true) == 0)
            {
                ViewBag.ReportType = Localization.GetText("Clauses");
            }
            else if (string.Compare(Report, DashReportsCodes.Visualization.GetStringValue(), true) == 0)
            {
                ViewBag.ReportType = Localization.GetText("Visualization");
            }
            else if (string.Compare(Report, DashReportsCodes.DisbursementProjections.GetStringValue(), true) == 0)
            {
                ViewBag.ReportType = Localization.GetText("Disbursement Projections");
            }
            else
            {
                ViewBag.ReportType = string.Empty;
            }

            if (string.Compare(Report, DashReportsCodes.DisbursementProjections.GetStringValue(), true) == 0)
            {
                LoadCommonData();
                LoadDisbursementProjectionsData();
                ViewBag.AdditionalParameters = true;
            }
            else
            {
                LoadCommonData();
                ViewBag.AdditionalParameters = false;
            }

            return View(PorfolioModel);
        }

        public virtual ActionResult GenerateReport(PorfolioStatusModel ModelPorfolio, string Report)
        {
            string UrlDinamicDashBoardReport = string.Empty;
            if (ModelPorfolio.Format == string.Empty || string.IsNullOrEmpty(ModelPorfolio.Format))
            {
                string Header = ConfigurationManager.AppSettings["ParamHeaderDashboardReport"].ToString();

                UrlDinamicDashBoardReport = ReportBuilder.GetReportPreffix(AddReportSelected(Report, ModelPorfolio.TypeGroupping["Groupping"]));

                UrlDinamicDashBoardReport += GetUrlReport(ModelPorfolio, Report, ModelPorfolio.TypeGroupping["Groupping"]);

                return Content(UrlDinamicDashBoardReport + Header);
            }
            else
            {
                UrlDinamicDashBoardReport += ReportBuilder.GetReportPreffix(AddReportSelected(Report, ModelPorfolio.TypeGroupping["Groupping"]));
                UrlDinamicDashBoardReport += GetUrlReport(ModelPorfolio, Report, ModelPorfolio.TypeGroupping["Groupping"]) + "&rs:Format=" + ModelPorfolio.Format;
                ModelPorfolio.Format = string.Empty;
                return Content(UrlDinamicDashBoardReport);
            }
        }

        public string GetUrlReport(PorfolioStatusModel ModelPorfolio, string Report, string Groupping)
        {
            var DashUrl = string.Empty;

            var ListCountryDepartments = ClientGenericRepositoty.GetCountryDepartments(Lang);
            var ListCountry = ClientGenericRepositoty.GetCountries(Lang).OrderBy(x => x.Name).ToList();
            var ListSectorDepartment = ClientGenericRepositoty.GetSectorDepartment(Lang);
            var ListDivision = ClientGenericRepositoty.GetDivisions(Lang);

            if (ModelPorfolio.CountryDepartment.Count == 0 && ModelPorfolio.Country.Count == 0)
            {
                DashUrl += "&ReportFilterCountryDepartment=-ALL COUNTRY DEPARTMENT";
                DashUrl += "&ReportFilterCountry=-ALL COUNTRY";
            }
            else if (ModelPorfolio.CountryDepartment.Count > 0 && ModelPorfolio.Country.Count > 0)
            {
                foreach (var item in ModelPorfolio.CountryDepartment)
                {
                    DashUrl += "&ReportFilterCountryDepartment=" + item.ToString();
                }

                foreach (var item in ModelPorfolio.Country)
                {
                    DashUrl += "&ReportFilterCountry=" + item;
                }
            }
            else if (ModelPorfolio.CountryDepartment.Count == 0 && ModelPorfolio.Country.Count > 0)
            {
                foreach (var item in ModelPorfolio.Country)
                {
                    DashUrl += "&ReportFilterCountry=" + item;
                }

                var ListCountryDepartment = ClientGenericRepositoty.GetCountryDepartmentByCountryCode(Lang, ModelPorfolio.Country);

                foreach (var item in ListCountryDepartment)
                {
                    DashUrl += "&ReportFilterCountryDepartment=" + item.ToString();
                }
            }
            else if (ModelPorfolio.CountryDepartment.Count > 0 && ModelPorfolio.Country.Count == 0)
            {
                foreach (var item in ModelPorfolio.CountryDepartment)
                {
                    DashUrl += "&ReportFilterCountryDepartment=" + item.ToString();
                }

                DashUrl += "&ReportFilterCountry=-ALL COUNTRY";
            }

            if (ModelPorfolio.DepartmentSector.Count == 0 && ModelPorfolio.Division.Count == 0)
            {
                DashUrl += "&ReportFilterDepartmentSector=-ALL DEPARTMENT SECTOR";
                DashUrl += "&ReportFilterDivision=-ALL DIVISION";
            }
            else if (ModelPorfolio.DepartmentSector.Count > 0 && ModelPorfolio.Division.Count > 0)
            {
                foreach (var item in ModelPorfolio.DepartmentSector)
                {
                    DashUrl += "&ReportFilterDepartmentSector=" + item;
                }

                foreach (var item in ModelPorfolio.Division)
                {
                    DashUrl += "&ReportFilterDivision=" + item;
                }
            }
            else if (ModelPorfolio.DepartmentSector.Count == 0 && ModelPorfolio.Division.Count > 0)
            {
                foreach (var item in ModelPorfolio.Division)
                {
                    DashUrl += "&ReportFilterDivision=" + item;
                }

                var ListDepartmentSector = ClientGenericRepositoty.GetDepartmentByDivisionCode(Lang, ModelPorfolio.Division);

                foreach (var item in ListDepartmentSector)
                {
                    DashUrl += "&ReportFilterDepartmentSector=" + item.ToString();
                }
            }
            else if (ModelPorfolio.DepartmentSector.Count > 0 && ModelPorfolio.Division.Count == 0)
            {
                foreach (var item in ModelPorfolio.DepartmentSector)
                {
                    DashUrl += "&ReportFilterDepartmentSector=" + item;
                }

                var ListDivisionCode = ClientGenericRepositoty.GetDivisionFilterCode(Lang, ModelPorfolio.DepartmentSector);

                DashUrl += "&ReportFilterDivision=-ALL DIVISION";
            }

            if (Report == DashReportsCodes.DisbursementProjections.GetStringValue())
            {
                if (ModelPorfolio.LendingInstrument.Count > 0)
                {
                    foreach (var item in ModelPorfolio.LendingInstrument)
                    {
                        DashUrl += "&ReportParameterLendingInstrument=" + item;
                    }
                }
                else
                {
                    DashUrl += "&ReportParameterLendingInstrument=-ALL LENDING INSTRUMENT";
                }

                if (ModelPorfolio.Funds.Count > 0)
                {
                    foreach (var item in ModelPorfolio.Funds)
                    {
                        DashUrl += "&ReportParameterFund=" + item;
                    }
                }
                else
                {
                    DashUrl += "&ReportParameterFund=-ALL FUNDS";
                }

                DashUrl += "&ReportParameterYear=" + ModelPorfolio.Year;
            }

            return DashUrl;
        }

        public virtual JsonResult FilterCountrys(PorfolioStatusModel ModelRisksModel)
        {
            var ListCountrys = ClientGenericRepositoty.GetCountrysFilterCode(Lang, ModelRisksModel.CountryDepartment).OrderBy(x => x.Name).ToList();
            return new JsonResult() { Data = ListCountrys };
        }

        public virtual JsonResult FilterDivision(PorfolioStatusModel ModelRisksModel)
        {
            var ListDivision = ClientGenericRepositoty.GetDivisionFilterCode(Lang, ModelRisksModel.DepartmentSector).OrderBy(x => x.Name).ToList();
            return new JsonResult() { Data = ListDivision };
        }

        public virtual JsonResult GetAllDivisions(List<string> Departments)
        {
            var ListDepartments = ClientGenericRepositoty.GetConvergenceIdByCodeDivision(Departments);
            var ListDivision = ClientGenericRepositoty.GetDivisionFilter(Lang, ListDepartments).OrderBy(x => x.Name).ToList();
            return new JsonResult() { Data = ListDivision };
        }

        public virtual JsonResult GetAllCountry(List<string> CountryDepartmentsId)
        {
            var ListCountryDepartmentsId = ClientGenericRepositoty.GetConvergenceIdByCode(CountryDepartmentsId);
            var ListCountrys = ClientGenericRepositoty.GetCountriesFilter(Lang, ListCountryDepartmentsId).OrderBy(x => x.Name).ToList();
            return new JsonResult() { Data = ListCountrys };
        }

        private void LoadCommonData()
        {
            var ListCountryDepartment = ClientGenericRepositoty.GetCountryDepartments(Lang).OrderBy(x => x.Name).ToList();
            var ListCountry = ClientGenericRepositoty.GetCountries(Lang).OrderBy(x => x.Name).ToList();
            var ListSectorDepartment = ClientGenericRepositoty.GetSectorDepartment(Lang).OrderBy(x => x.Name).ToList();
            var ListDivision = ClientGenericRepositoty.GetDivisions(Lang).OrderBy(x => x.Name).ToList();

            ViewBag.ListCountryDepartment = new MultiSelectList(ListCountryDepartment, "Code", "Name");
            ViewBag.ListCountry = new MultiSelectList(ListCountry, "Code", "Name");
            ViewBag.ListSectorDepartment = new MultiSelectList(ListSectorDepartment, "Code", "Name");
            ViewBag.ListDivision = new MultiSelectList(ListDivision, "Code", "Name");
        }

        private void LoadDisbursementProjectionsData()
        {
            var ListLendingInstrument = ClientGenericRepositoty.GetFinancing(Lang).OrderBy(x => x.Name).ToList();
            var ListFunds = ClientGenericRepositoty.GetFund(Lang).OrderBy(x => x.Name).ToList();
            var ListYear = ClientGenericRepositoty.GetYearsDashboard(DateTime.Now.Year).OrderBy(x => x.Key).ToList();

            ViewBag.ListLendingInstrument = new MultiSelectList(ListLendingInstrument, "Code", "Name");
            ViewBag.ListFunds = new MultiSelectList(ListFunds, "Code", "Name");
            ViewBag.ListYear = new SelectList(ListYear, "Key", "Value");
        }

        private string AddReportSelected(string Report, string TypeSelected)
        {
            string ReportSelected = string.Empty;

            if (GrouppingType.GrouppingCountry.GetStringValue() == TypeSelected)
            {
                if (string.Compare(Report, DashReportsCodes.PMR.GetStringValue(), true) == 0)
                {
                    ReportSelected = "ParamForDashPMRCountryReport";
                }
                else if (string.Compare(Report, DashReportsCodes.SupervisionPlan.GetStringValue(), true) == 0)
                {
                    ReportSelected = "ParamForDashSupCountryReport";
                }
                else if (string.Compare(Report, DashReportsCodes.Risks.GetStringValue(), true) == 0)
                {
                    ReportSelected = "ParamForDashRiskCountryReport";
                }
                else if (string.Compare(Report, DashReportsCodes.Clauses.GetStringValue(), true) == 0)
                {
                    ReportSelected = "ParamForDashClauseCountryReport";
                }
                else if (string.Compare(Report, DashReportsCodes.Visualization.GetStringValue(), true) == 0)
                {
                    ReportSelected = "ParamForDashVisCountryReport";
                }
                else if (string.Compare(Report, DashReportsCodes.DisbursementProjections.GetStringValue(), true) == 0)
                {
                    ReportSelected = "ParamForDashDisCountryReport";
                }
                else
                {
                    return string.Empty;
                }
            }
            else if (GrouppingType.GrouppingSector.GetStringValue() == TypeSelected)
            {
                if (string.Compare(Report, DashReportsCodes.PMR.GetStringValue(), true) == 0)
                {
                    ReportSelected = "ParamForDashPMRSectorReport";
                }
                else if (string.Compare(Report, DashReportsCodes.SupervisionPlan.GetStringValue(), true) == 0)
                {
                    ReportSelected = "ParamForDashSupSectorReport";
                }
                else if (string.Compare(Report, DashReportsCodes.Risks.GetStringValue(), true) == 0)
                {
                    ReportSelected = "ParamForDashRiskSectorReport";
                }
                else if (string.Compare(Report, DashReportsCodes.Clauses.GetStringValue(), true) == 0)
                {
                    ReportSelected = "ParamForDashClauseSectorReport";
                }
                else if (string.Compare(Report, DashReportsCodes.Visualization.GetStringValue(), true) == 0)
                {
                    ReportSelected = "ParamForDashVisSectorReport";
                }
                else if (string.Compare(Report, DashReportsCodes.DisbursementProjections.GetStringValue(), true) == 0)
                {
                    ReportSelected = "ParamForDashDisSectorReport";
                }
                else
                {
                    return string.Empty;
                }
            }

            return ReportSelected;
        }
    }
}
