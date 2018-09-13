using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture;
using IDB.Architecture.Language;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.SupervisionPlans;
using IDB.MW.Domain.Contracts.ModelRepositories.Reports.Generic;
using IDB.MW.Domain.Models.Architecture.Reports.PMRExtraModels.SupervisionPlanPorfolio;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Report;

namespace IDB.Presentation.MVC4.Areas.Reports.Controllers
{
    public partial class SupervisionPorfolioReportController : MVC4.Controllers.ConfluenceController
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

        public virtual ActionResult Index()
        {
            SupervisionPorfolioModel ModelSupervisionPorfolio = new SupervisionPorfolioModel();

            var ListCountryDepartment = ClientGenericRepositoty.GetCountryDepartments(Globals.NeutralLanguage);
            var ListSectorDepartment = ClientGenericRepositoty.GetSectorDepartment(Globals.NeutralLanguage);
            var ListCountry = ClientGenericRepositoty.GetCountries(Globals.NeutralLanguage);
            var ListDivision = ClientGenericRepositoty.GetDivisions(Globals.NeutralLanguage);
            var ListValidationStageSupPlan = ClientGenericRepositoty.GetStatesForSupervisionReport();
            var ListESG_Tracking = new Dictionary<bool, string>();
            ListESG_Tracking.Add(true, Localization.GetText("Yes"));
            ListESG_Tracking.Add(false, Localization.GetText("No"));
            var YearsSupervision = ClientGenericRepositoty.GetMaxAndMinYearSupervisionPlanReport();
            var ListYears = new Dictionary<int, int>();

            ModelSupervisionPorfolio.StartYear = YearsSupervision["MinYear"];
            ModelSupervisionPorfolio.EndYear = YearsSupervision["MaxYear"];

            for (int x = ModelSupervisionPorfolio.StartYear; x <= ModelSupervisionPorfolio.EndYear; x++)
            {
                ListYears.Add(x, x);
            }

            ViewBag.ListCountryDepartment = new MultiSelectList(ListCountryDepartment, "ConvergenceMasterDataId", "Name");
            ViewBag.ListSectorDepartment = new MultiSelectList(ListSectorDepartment, "ConvergenceMasterDataId", "Name");
            ViewBag.ListCountry = new MultiSelectList(ListCountry, "ConvergenceMasterDataId", "Name");
            ViewBag.ListDivision = new MultiSelectList(ListDivision, "ConvergenceMasterDataId", "Name");
            ViewBag.ListValidationStageSupPlan = new SelectList(ListValidationStageSupPlan, "ConvergenceMasterDataId", "Name");
            ViewBag.ListESG_Tracking = new SelectList(ListESG_Tracking, "Key", "Value");
            ViewBag.ListStartYear = new SelectList(ListYears, "Key", "Value");
            ViewBag.ListEndYear = new SelectList(ListYears.OrderByDescending(x => x.Key).ToList(), "Key", "Value");

            return View(ModelSupervisionPorfolio);
        }

        [HttpPost()]
        public virtual ActionResult Index(SupervisionPorfolioModel ModelSupervisionPorfolio)
        {
            var ListCountryDepartment = ClientGenericRepositoty.GetCountryDepartments(Globals.NeutralLanguage);
            var ListSectorDepartment = ClientGenericRepositoty.GetSectorDepartment(Globals.NeutralLanguage);
            var ListCountry = ClientGenericRepositoty.GetCountries(Globals.NeutralLanguage);
            var ListDivision = ClientGenericRepositoty.GetDivisions(Globals.NeutralLanguage);
            var ListValidationStageSupPlan = ClientGenericRepositoty.GetStatesForSupervisionReport();
            var ListESG_Tracking = new Dictionary<bool, string>();
            ListESG_Tracking.Add(true, Localization.GetText("Yes"));
            ListESG_Tracking.Add(false, Localization.GetText("No"));
            var YearsSupervision = ClientGenericRepositoty.GetMaxAndMinYearSupervisionPlanReport();
            var ListYears = new Dictionary<int, int>();

            var startYear = YearsSupervision["MinYear"];
            var endYear = YearsSupervision["MaxYear"];

            for (int x = startYear; x <= endYear; x++)
            {
                ListYears.Add(x, x);
            }

            ViewBag.ListCountryDepartment = new MultiSelectList(ListCountryDepartment, "ConvergenceMasterDataId", "Name");
            ViewBag.ListSectorDepartment = new MultiSelectList(ListSectorDepartment, "ConvergenceMasterDataId", "Name");
            ViewBag.ListCountry = new MultiSelectList(ListCountry, "ConvergenceMasterDataId", "Name");
            ViewBag.ListDivision = new MultiSelectList(ListDivision, "ConvergenceMasterDataId", "Name");
            ViewBag.ListValidationStageSupPlan = new SelectList(ListValidationStageSupPlan, "ConvergenceMasterDataId", "Name");
            ViewBag.ListESG_Tracking = new SelectList(ListESG_Tracking, "Key", "Value");
            ViewBag.ListStartYear = new SelectList(ListYears, "Key", "Value");
            ViewBag.ListEndYear = new SelectList(ListYears.OrderByDescending(x => x.Key).ToList(), "Key", "Value");

            string lang = string.Empty;
            if (!string.IsNullOrEmpty(IDBContext.Current.CurrentLanguage))
            {
                lang = IDBContext.Current.CurrentLanguage.ToUpper();
            }
            else
            {
                lang = Globals.NeutralLanguage.ToUpper();
            }

            var SupervisionResults = _ClientGenericRepositoty.GetSupervisionPlanresults(ModelSupervisionPorfolio, lang);
            ModelSupervisionPorfolio.ListSupervionResults.AddRange(SupervisionResults);

            return View(ModelSupervisionPorfolio);
        }

        [HttpPost()]
        public virtual ActionResult ExportReport(SupervisionPorfolioModel ModelSupervisionPorfolio, string Operations, string Supervisions, string Format)
        {
            string[] ParamOperations = Operations.Split(',');
            string[] ParamSupervisions = Supervisions.Split(',');

            string URLSupervisionPorfolioReport = ReportBuilder.GetReportPreffix("ParamForSupervisionPortfolioReportReport");

            if (!string.IsNullOrEmpty(IDBContext.Current.CurrentLanguage))
            {
                URLSupervisionPorfolioReport += "&LANG=" + IDBContext.Current.CurrentLanguage.ToUpper();
            }
            else
            {
                URLSupervisionPorfolioReport += "&LANG=" + Globals.NeutralLanguage.ToUpper();
            }

            if (ParamOperations.Length == ParamSupervisions.Length)
            {
                for (int i = 0; i < ParamOperations.Length; i++)
                {
                    if (i == 0)
                        URLSupervisionPorfolioReport = URLSupervisionPorfolioReport + "&dato=" + ParamOperations[i] + "_" + ParamSupervisions[i];
                    else
                        URLSupervisionPorfolioReport = URLSupervisionPorfolioReport + "," + ParamOperations[i] + "_" + ParamSupervisions[i];
                }

                URLSupervisionPorfolioReport = URLSupervisionPorfolioReport + "&rs:Format=" + Format;
            }

            return Content(URLSupervisionPorfolioReport);
        }

        public virtual JsonResult FilterCountrys(SupervisionPorfolioModel ModelSupervisionPorfolio)
        {
            var ListCountrys = ClientGenericRepositoty.GetCountriesFilter(Globals.NeutralLanguage, ModelSupervisionPorfolio.ListCountryDepartment).OrderBy(x => x.Name).ToList();
            return new JsonResult() { Data = ListCountrys };
        }

        public virtual JsonResult FilterDivision(SupervisionPorfolioModel ModelSupervisionPorfolio)
        {
            var ListDivision = ClientGenericRepositoty.GetDivisionFilter(Globals.NeutralLanguage, ModelSupervisionPorfolio.ListSectorDepartment).OrderBy(x => x.Name).ToList();
            return new JsonResult() { Data = ListDivision };
        }

        [HttpPost()]
        public virtual ActionResult SearchRegisterSupervisionPorfolio(SupervisionPorfolioModel ModelSupervisionPorfolio)
        {
            return PartialView();
        }
    }
}