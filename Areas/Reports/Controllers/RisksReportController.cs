using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.MW.Domain.Contracts.ModelRepositories.Reports.Generic;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.Reports;
using IDB.MW.Domain.Models.Architecture.Reports.PMRExtraModels.Risk;
using IDB.Presentation.MVC4.Controllers;
using IDB.Architecture.Language;
using IDB.Presentation.MVC4.Report;

namespace IDB.Presentation.MVC4.Areas.Reports.Controllers
{
    public partial class RisksReportController : BaseController
    {
        #region Repositories

        IPmrCycleModelRepository _ClientPMRCycle = null;
        public IPmrCycleModelRepository ClientPMRCycle
        {
            get { return _ClientPMRCycle; }
            set { _ClientPMRCycle = value; }
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

        public virtual ActionResult Create()
        {
            var ListAsOf = ClientPMRCycle.GetAllCyclesSeptemberAndMarchCycles(Lang).OrderByDescending(x => x.PmrCycleId);
            var ListCountryDepartment = ClientGenericRepositoty.GetCountryDepartments(Lang).OrderBy(x => x.Name).ToList();
            var ListDepartment = ClientGenericRepositoty.GetSectorDepartment(Lang).OrderBy(x => x.Name).ToList();
            var ListCountrys = ClientGenericRepositoty.GetCountries(Lang).OrderBy(x => x.Name).ToList();
            var ListDivisions = ClientGenericRepositoty.GetDivisions(Lang).OrderBy(x => x.Name).ToList();
            var ListRiskType = ClientGenericRepositoty.GetRiskType(Lang).OrderBy(x => x.Name).ToList();
            var ListESGClassifictionValue = ClientGenericRepositoty.GetESGClassifictionValue(Lang);
            var ListRiskLeven = ClientGenericRepositoty.GetRiskLeven(Lang).OrderBy(x => x.Name).ToList();
            var ListRiskStatus = ClientGenericRepositoty.GetRiskStatus(Lang).OrderBy(x => x.Name).ToList();

            ListESGClassifictionValue.Add(new MW.Domain.Models.Architecture.MasterDefinitions.ConvergenceMasterDataModel { ConvergenceMasterDataId = 0, Name = Localization.GetText("None") });

            ViewBag.ListAsOf = new MultiSelectList(ListAsOf, "PmrCycleId", "PmrCycleName");
            ViewBag.ListCountryDepartment = new MultiSelectList(ListCountryDepartment, "ConvergenceMasterDataId", "Name");
            ViewBag.ListDepartment = new MultiSelectList(ListDepartment, "ConvergenceMasterDataId", "Name");
            ViewBag.ListCountrys = new MultiSelectList(ListCountrys, "ConvergenceMasterDataId", "Name");
            ViewBag.ListDivisions = new MultiSelectList(ListDivisions, "ConvergenceMasterDataId", "Name");
            ViewBag.ListRiskType = new MultiSelectList(ListRiskType, "ConvergenceMasterDataId", "Name");
            ViewBag.ListESGClassifictionValue = new MultiSelectList(ListESGClassifictionValue, "ConvergenceMasterDataId", "Name");
            ViewBag.ListRiskLeven = new MultiSelectList(ListRiskLeven, "ConvergenceMasterDataId", "Name");
            ViewBag.ListRiskStatus = new MultiSelectList(ListRiskStatus, "ConvergenceMasterDataId", "Name");

            return View();
        }

        [HttpPost()]
        public virtual ActionResult Create(RisksReportModel RisksModel)
        {
            string URLRiskReport = string.Empty;

            //string ReportServer = ConfigurationManager.AppSettings["ParamReportServer"].ToString();
            //string Report = ConfigurationManager.AppSettings["ParamForRiskReport"].ToString();
            string Header = ReportBuilder.GetReportHeader();

            URLRiskReport += ReportBuilder.GetReportPreffix("ParamForRiskReport");

            if (Lang != null)
            {
                URLRiskReport += "&LANG=" + Lang;
            }
            else
            {
                URLRiskReport += "&LANG=EN";
            }

            string RiskReportUrlDinamic = string.Empty;

            RiskReportUrlDinamic += "&ESG_TRACKING=" + RisksModel.ESGTracking;

            if (RisksModel.CountryDepartment.Count == 0 && RisksModel.Country.Count == 0)
            {
                var CountryDepartment = ClientGenericRepositoty.GetCountryDepartments(Lang).ToList();
                var Country = ClientGenericRepositoty.GetCountries(Lang).OrderBy(x => x.Name).ToList();

                foreach (var itemCountryDepartment in CountryDepartment)
                {
                    RiskReportUrlDinamic += "&COUNTRY_DEPARTMENT_ID=" + itemCountryDepartment.ConvergenceMasterDataId;
                }

                foreach (var itemCountry in Country)
                {
                    RiskReportUrlDinamic += "&COUNTRY_ID=" + itemCountry.ConvergenceMasterDataId;
                }
            }
            else if (RisksModel.CountryDepartment.Count > 0 && RisksModel.Country.Count > 0)
            {
                for (int x = 0; x < RisksModel.CountryDepartment.Count; x++)
                {
                    RiskReportUrlDinamic += "&COUNTRY_DEPARTMENT_ID=" + RisksModel.CountryDepartment[x];
                }

                for (int x = 0; x < RisksModel.Country.Count; x++)
                {
                    RiskReportUrlDinamic += "&COUNTRY_ID=" + RisksModel.Country[x];
                }
            }
            else if (RisksModel.CountryDepartment.Count == 0 && RisksModel.Country.Count > 0)
            {
                for (int x = 0; x < RisksModel.Country.Count; x++)
                {
                    RiskReportUrlDinamic += "&COUNTRY_ID=" + RisksModel.Country[x];
                }

                var ListCountryDepartment = ClientGenericRepositoty.GetCountryDepartmentByCountry(Lang, RisksModel.Country);

                foreach (var itemIdCountryDepartment in ListCountryDepartment)
                {
                    RiskReportUrlDinamic += "&COUNTRY_DEPARTMENT_ID=" + itemIdCountryDepartment;
                }
            }
            else if (RisksModel.CountryDepartment.Count > 0 && RisksModel.Country.Count == 0)
            {
                for (int x = 0; x < RisksModel.CountryDepartment.Count; x++)
                {
                    RiskReportUrlDinamic += "&COUNTRY_DEPARTMENT_ID=" + RisksModel.CountryDepartment[x];
                }

                var ListCountrys = ClientGenericRepositoty.GetCountriesFilter(Lang, RisksModel.CountryDepartment).OrderBy(x => x.Name).ToList();

                foreach (var itemCountrys in ListCountrys)
                {
                    RiskReportUrlDinamic += "&COUNTRY_ID=" + itemCountrys.ConvergenceMasterDataId;
                }
            }

            if (RisksModel.Department.Count == 0 && RisksModel.Division.Count == 0)
            {
                var ContractNumber = ClientGenericRepositoty.GetSectorDepartment(Lang).OrderBy(x => x.Name).ToList();
                var Division = ClientGenericRepositoty.GetClauseType(Lang).OrderBy(x => x.Name).ToList();

                RiskReportUrlDinamic += "&DEPARTMENT_ID=" + -1;

                RiskReportUrlDinamic += "&DIVISION_ID=" + -1;
            }
            else if (RisksModel.Department.Count > 0 && RisksModel.Division.Count > 0)
            {
                for (int x = 0; x < RisksModel.Department.Count; x++)
                {
                    RiskReportUrlDinamic += "&DEPARTMENT_ID=" + RisksModel.Department[x];
                }

                for (int x = 0; x < RisksModel.Division.Count; x++)
                {
                    RiskReportUrlDinamic += "&DIVISION_ID=" + RisksModel.Division[x];
                }
            }
            else if (RisksModel.Department.Count == 0 && RisksModel.Division.Count > 0)
            {
                var ListDivisions = ClientGenericRepositoty.GetDepartmentByDivision(Lang, RisksModel.Division);

                foreach (var itemDepartment in ListDivisions)
                {
                    RiskReportUrlDinamic += "&DEPARTMENT_ID=" + itemDepartment;
                }

                for (int x = 0; x < RisksModel.Division.Count; x++)
                {
                    RiskReportUrlDinamic += "&DIVISION_ID=" + RisksModel.Division[x];
                }
            }
            else if (RisksModel.Department.Count > 0 && RisksModel.Division.Count == 0)
            {
                for (int x = 0; x < RisksModel.Department.Count; x++)
                {
                    RiskReportUrlDinamic += "&DEPARTMENT_ID=" + RisksModel.Department[x];
                }

                var ListDivision = ClientGenericRepositoty.GetDivisionFilter(Lang, RisksModel.Department).OrderBy(x => x.Name).ToList();
                foreach (var itemFilterDivision in ListDivision)
                {
                    RiskReportUrlDinamic += "&DIVISION_ID=" + itemFilterDivision.ConvergenceMasterDataId;
                }
            }

            if (RisksModel.RiskLeven.Count > 0)
            {
                for (int x = 0; x < RisksModel.RiskLeven.Count; x++)
                {
                    RiskReportUrlDinamic += "&RISK_LEVEL_ID=" + RisksModel.RiskLeven[x];
                }
            }
            else
            {
                var RiskLeven = ClientGenericRepositoty.GetRiskLeven(Lang).OrderBy(x => x.Name).ToList();
                foreach (var itemRiskLeven in RiskLeven)
                {
                    RiskReportUrlDinamic += "&RISK_LEVEL_ID=" + itemRiskLeven.ConvergenceMasterDataId;
                }
            }

            if (RisksModel.RiskStatus.Count > 0)
            {
                for (int x = 0; x < RisksModel.RiskStatus.Count; x++)
                {
                    RiskReportUrlDinamic += "&RISK_STATUS_ID=" + RisksModel.RiskStatus[x];
                }
            }
            else
            {
                var RiskStatus = ClientGenericRepositoty.GetRiskStatus(Lang).ToList();
                foreach (var itemRiskStatus in RiskStatus)
                {
                    RiskReportUrlDinamic += "&RISK_STATUS_ID=" + itemRiskStatus.ConvergenceMasterDataId;
                }
            }

            if (RisksModel.RiskType.Count > 0)
            {
                for (int x = 0; x < RisksModel.RiskType.Count; x++)
                {
                    RiskReportUrlDinamic += "&RISK_TYPE_ID=" + RisksModel.RiskType[x];
                }
            }
            else
            {
                var RiskType = ClientGenericRepositoty.GetRiskType(Lang).ToList();
                foreach (var itemRiskType in RiskType)
                {
                    RiskReportUrlDinamic += "&RISK_TYPE_ID=" + itemRiskType.ConvergenceMasterDataId;
                }
            }

            if (RisksModel.PMRCycle.Count > 0)
            {
                RiskReportUrlDinamic += "&PMR_CYCLE_ID=" + RisksModel.PMRCycle[0];
            }

            if (RisksModel.ESGClassification.Count > 0)
            {
                if (RisksModel.ESGClassification[0] != 0)
                {
                    RiskReportUrlDinamic += "&ESG_CLASSIFICATION_ID=" + RisksModel.ESGClassification[0];
                }
            }

            return Content(URLRiskReport + RiskReportUrlDinamic + Header);
        }

        public virtual ActionResult ExportRiskReport(RisksReportModel RisksModel)
        {
            string URLRiskReport = string.Empty;

            URLRiskReport += ReportBuilder.GetReportPreffix("ParamForRiskReport");

            if (Lang != null)
            {
                URLRiskReport += "&LANG=" + Lang;
            }
            else
            {
                URLRiskReport += "&LANG=EN";
            }

            string RiskReportUrlDinamic = string.Empty;

            RiskReportUrlDinamic += "&ESG_TRACKING=" + RisksModel.ESGTracking;

            if (RisksModel.CountryDepartment.Count == 0 && RisksModel.Country.Count == 0)
            {
                var CountryDepartment = ClientGenericRepositoty.GetCountryDepartments(Lang).ToList();
                var Country = ClientGenericRepositoty.GetCountries(Lang).OrderBy(x => x.Name).ToList();

                foreach (var itemCountryDepartment in CountryDepartment)
                {
                    RiskReportUrlDinamic += "&COUNTRY_DEPARTMENT_ID=" + itemCountryDepartment.ConvergenceMasterDataId;
                }

                foreach (var itemCountry in Country)
                {
                    RiskReportUrlDinamic += "&COUNTRY_ID=" + itemCountry.ConvergenceMasterDataId;
                }
            }
            else if (RisksModel.CountryDepartment.Count > 0 && RisksModel.Country.Count > 0)
            {
                for (int x = 0; x < RisksModel.CountryDepartment.Count; x++)
                {
                    RiskReportUrlDinamic += "&COUNTRY_DEPARTMENT_ID=" + RisksModel.CountryDepartment[x];
                }

                for (int x = 0; x < RisksModel.Country.Count; x++)
                {
                    RiskReportUrlDinamic += "&COUNTRY_ID=" + RisksModel.Country[x];
                }
            }
            else if (RisksModel.CountryDepartment.Count == 0 && RisksModel.Country.Count > 0)
            {
                for (int x = 0; x < RisksModel.Country.Count; x++)
                {
                    RiskReportUrlDinamic += "&COUNTRY_ID=" + RisksModel.Country[x];
                }

                var ListCountryDepartment = ClientGenericRepositoty.GetCountryDepartmentByCountry(Lang, RisksModel.Country);

                foreach (var itemIdCountryDepartment in ListCountryDepartment)
                {
                    RiskReportUrlDinamic += "&COUNTRY_DEPARTMENT_ID=" + itemIdCountryDepartment;
                }
            }
            else if (RisksModel.CountryDepartment.Count > 0 && RisksModel.Country.Count == 0)
            {
                for (int x = 0; x < RisksModel.CountryDepartment.Count; x++)
                {
                    RiskReportUrlDinamic += "&COUNTRY_DEPARTMENT_ID=" + RisksModel.CountryDepartment[x];
                }

                var ListCountrys = ClientGenericRepositoty.GetCountriesFilter(Lang, RisksModel.CountryDepartment).OrderBy(x => x.Name).ToList();

                foreach (var itemCountrys in ListCountrys)
                {
                    RiskReportUrlDinamic += "&COUNTRY_ID=" + itemCountrys.ConvergenceMasterDataId;
                }
            }

            if (RisksModel.Department.Count == 0 && RisksModel.Division.Count == 0)
            {
                var ContractNumber = ClientGenericRepositoty.GetSectorDepartment(Lang).OrderBy(x => x.Name).ToList();
                var Division = ClientGenericRepositoty.GetClauseType(Lang).OrderBy(x => x.Name).ToList();

                RiskReportUrlDinamic += "&DEPARTMENT_ID=" + -1;

                RiskReportUrlDinamic += "&DIVISION_ID=" + -1;
            }
            else if (RisksModel.Department.Count > 0 && RisksModel.Division.Count > 0)
            {
                for (int x = 0; x < RisksModel.Department.Count; x++)
                {
                    RiskReportUrlDinamic += "&DEPARTMENT_ID=" + RisksModel.Department[x];
                }

                for (int x = 0; x < RisksModel.Division.Count; x++)
                {
                    RiskReportUrlDinamic += "&DIVISION_ID=" + RisksModel.Division[x];
                }
            }
            else if (RisksModel.Department.Count == 0 && RisksModel.Division.Count > 0)
            {
                var ListDivisions = ClientGenericRepositoty.GetDepartmentByDivision(Lang, RisksModel.Division);

                foreach (var itemDepartment in ListDivisions)
                {
                    RiskReportUrlDinamic += "&DEPARTMENT_ID=" + itemDepartment;
                }

                for (int x = 0; x < RisksModel.Division.Count; x++)
                {
                    RiskReportUrlDinamic += "&DIVISION_ID=" + RisksModel.Division[x];
                }
            }
            else if (RisksModel.Department.Count > 0 && RisksModel.Division.Count == 0)
            {
                for (int x = 0; x < RisksModel.Department.Count; x++)
                {
                    RiskReportUrlDinamic += "&DEPARTMENT_ID=" + RisksModel.Department[x];
                }

                var ListDivision = ClientGenericRepositoty.GetDivisionFilter(Lang, RisksModel.Department).OrderBy(x => x.Name).ToList();
                foreach (var itemFilterDivision in ListDivision)
                {
                    RiskReportUrlDinamic += "&DIVISION_ID=" + itemFilterDivision.ConvergenceMasterDataId;
                }
            }

            if (RisksModel.RiskLeven.Count > 0)
            {
                for (int x = 0; x < RisksModel.RiskLeven.Count; x++)
                {
                    RiskReportUrlDinamic += "&RISK_LEVEL_ID=" + RisksModel.RiskLeven[x];
                }
            }
            else
            {
                var RiskLeven = ClientGenericRepositoty.GetRiskLeven(Lang).OrderBy(x => x.Name).ToList();
                foreach (var itemRiskLeven in RiskLeven)
                {
                    RiskReportUrlDinamic += "&RISK_LEVEL_ID=" + itemRiskLeven.ConvergenceMasterDataId;
                }
            }

            if (RisksModel.RiskStatus.Count > 0)
            {
                for (int x = 0; x < RisksModel.RiskStatus.Count; x++)
                {
                    RiskReportUrlDinamic += "&RISK_STATUS_ID=" + RisksModel.RiskStatus[x];
                }
            }
            else
            {
                var RiskStatus = ClientGenericRepositoty.GetRiskStatus(Lang).ToList();
                foreach (var itemRiskStatus in RiskStatus)
                {
                    RiskReportUrlDinamic += "&RISK_STATUS_ID=" + itemRiskStatus.ConvergenceMasterDataId;
                }
            }

            if (RisksModel.RiskType.Count > 0)
            {
                for (int x = 0; x < RisksModel.RiskType.Count; x++)
                {
                    RiskReportUrlDinamic += "&RISK_TYPE_ID=" + RisksModel.RiskType[x];
                }
            }
            else
            {
                var RiskType = ClientGenericRepositoty.GetRiskType(Lang).ToList();
                foreach (var itemRiskType in RiskType)
                {
                    RiskReportUrlDinamic += "&RISK_TYPE_ID=" + itemRiskType.ConvergenceMasterDataId;
                }
            }

            if (RisksModel.PMRCycle.Count > 0)
            {
                RiskReportUrlDinamic += "&PMR_CYCLE_ID=" + RisksModel.PMRCycle[0];
            }

            if (RisksModel.ESGClassification.Count > 0)
            {
                if (RisksModel.ESGClassification[0] != 0)
                {
                    RiskReportUrlDinamic += "&ESG_CLASSIFICATION_ID=" + RisksModel.ESGClassification[0];
                }
            }

            RiskReportUrlDinamic += "&rs:Format=" + RisksModel.ExportType;

            return Content(URLRiskReport + RiskReportUrlDinamic);
        }

        public virtual JsonResult FilterCountrys(RisksReportModel ModelRisksModel)
        {
            var ListCountrys = ClientGenericRepositoty.GetCountriesFilter(Lang, ModelRisksModel.CountryDepartment).OrderBy(x => x.Name).ToList();
            return new JsonResult() { Data = ListCountrys };
        }

        public virtual JsonResult FilterDivision(RisksReportModel ModelRisksModel)
        {
            var ListDivision = ClientGenericRepositoty.GetDivisionFilter(Lang, ModelRisksModel.Department).OrderBy(x => x.Name).ToList();
            return new JsonResult() { Data = ListDivision };
        }

        public virtual JsonResult GetAllDivisions(int[] Departments)
        {
            var ListDepartments = Departments.ToList();
            var ListDivision = ClientGenericRepositoty.GetDivisionFilter(Lang, ListDepartments).OrderBy(x => x.Name).ToList();
            return new JsonResult() { Data = ListDivision };
        }

        public virtual JsonResult GetAllCountry(List<int> CountryDepartmentsId)
        {
            var ListCountrys = ClientGenericRepositoty.GetCountriesFilter(Lang, CountryDepartmentsId).OrderBy(x => x.Name).ToList();
            return new JsonResult() { Data = ListCountrys };
        }
    }
}
