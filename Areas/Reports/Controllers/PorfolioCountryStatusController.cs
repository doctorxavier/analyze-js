using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.MW.Domain.Models.Architecture.Reports.PMRExtraModels.Porfolio;
using IDB.MW.Domain.Contracts.ModelRepositories.Reports.Generic;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.Reports;
using IDB.Presentation.MVC4.Controllers;
using IDB.Architecture.Language;
using IDB.Presentation.MVC4.Report;

namespace IDB.Presentation.MVC4.Areas.Reports.Controllers
{
   public partial class PorfolioCountryStatusController : BaseController
   {
      #region Repositories

      IReportsGenericRepository _ClientGenericRepositoty = null;
      public IReportsGenericRepository ClientGenericRepositoty
      {
         get { return _ClientGenericRepositoty; }
         set { _ClientGenericRepositoty = value; }
      }

      IPmrCycleModelRepository _ClientPMRModelRepository = null;
      public IPmrCycleModelRepository ClientPMRModelRepository
      {
         get { return _ClientPMRModelRepository; }
         set { _ClientPMRModelRepository = value; }
      }

      #endregion

      #region Variables

      private string Lang = "EN";

      #endregion

      public virtual ActionResult Index()
      {
         CountryStatusModel ModelCountryStatus = new CountryStatusModel();

         var ListCountryDepartment = ClientGenericRepositoty.GetCountryDepartments(Lang).OrderBy(x => x.Name).ToList();
         var ListCountry = ClientGenericRepositoty.GetCountries(Lang).OrderBy(x => x.Name).ToList();
         var ListSectorDepartment = ClientGenericRepositoty.GetSectorDepartment(Lang).OrderBy(x => x.Name).ToList();
         var ListDivisions = ClientGenericRepositoty.GetDivisions(Lang).OrderBy(x => x.Name).ToList();
         var ListPMRCycles = ClientPMRModelRepository.GetAllCyclesSeptemberAndMarchCycles(Lang).OrderByDescending(x => x.PmrCycleId).ToList();

         var TempEV_PV = new Dictionary<bool, string>();
         TempEV_PV.Add(true, "Original Value");
         TempEV_PV.Add(false, "Annual Value");

         var TempESGTracking = new Dictionary<bool, string>();
         TempESGTracking.Add(true, Localization.GetText("Yes"));
         TempESGTracking.Add(false, Localization.GetText("No"));

         ViewBag.ListEV_PV = new SelectList(TempEV_PV, "Key", "Value");
         ViewBag.ListESGTracking = new SelectList(TempESGTracking, "Key", "Value");
         ViewBag.ListCountryDepartment = new MultiSelectList(ListCountryDepartment, "convergencemasterdataid", "name");
         ViewBag.ListCountry = new MultiSelectList(ListCountry, "convergencemasterdataid", "name");
         ViewBag.ListSectorDepartment = new MultiSelectList(ListSectorDepartment, "convergencemasterdataid", "name");
         ViewBag.ListDivisions = new MultiSelectList(ListDivisions, "convergencemasterdataid", "name");
         ViewBag.ListPMRCycles = new MultiSelectList(ListPMRCycles, "PmrCycleId ", "PmrCycleName");

         return View(ModelCountryStatus);
      }

      [HttpPost()]
      public virtual ActionResult Index(CountryStatusModel ModelCountryStatus)
      {
         string URLPMRPorfolioReport = string.Empty;
            
         string Header = ReportBuilder.GetReportHeader();

         URLPMRPorfolioReport += ReportBuilder.GetReportPreffix("ParamForPMRPorfolioCountryReport");

         if (Lang != null)
         {
            URLPMRPorfolioReport += "&LANG=" + Lang;
         }
         else
         {
            URLPMRPorfolioReport += "&LANG=EN";
         }

         if (ModelCountryStatus.CountryDepartment.Count > 0)
         {
            foreach (var itemCountryDepartment in ModelCountryStatus.CountryDepartment)
            {
               URLPMRPorfolioReport += "&COUNTRY_DEPARTMENT_ID=" + itemCountryDepartment;
            }
         }
         else
         {
            URLPMRPorfolioReport += "&COUNTRY_DEPARTMENT_ID=-1";
         }

         if (ModelCountryStatus.Country.Count > 0)
         {
            foreach (var itemCountry in ModelCountryStatus.Country)
            {
               URLPMRPorfolioReport += "&COUNTRY_ID=" + itemCountry;
            }
         }
         else
         {
            URLPMRPorfolioReport += "&COUNTRY_ID=-1";
         }

         if (ModelCountryStatus.Division.Count > 0)
         {
            foreach (var itemDivision in ModelCountryStatus.Division)
            {
               URLPMRPorfolioReport += "&DIVISION_ID=" + itemDivision;
            }
         }
         else
         {
            URLPMRPorfolioReport += "&DIVISION_ID=-1";
         }

         if (ModelCountryStatus.SectorDepartment.Count > 0)
         {
            foreach (var itemSectorDepartment in ModelCountryStatus.SectorDepartment)
            {
               URLPMRPorfolioReport += "&DEPARTMENT_ID=" + itemSectorDepartment;
            }
         }
         else
         {
            URLPMRPorfolioReport += "&DEPARTMENT_ID=-1";
         }

         if (ModelCountryStatus.ESGTracking)
         {
            URLPMRPorfolioReport += "&ESG_TRACKING=true";
         }
         else
         {
            URLPMRPorfolioReport += "&ESG_TRACKING=false";
         }

         URLPMRPorfolioReport += "&PMR_CYCLE_ID=" + ModelCountryStatus.PMRCycleId;

         if (ModelCountryStatus.EV_PV)
         {
            URLPMRPorfolioReport += "&IS_ORIGINAL=true";
         }
         else
         {
            URLPMRPorfolioReport += "&IS_ORIGINAL=false";
         }

         return Content(URLPMRPorfolioReport + Header);
      }

      public virtual ActionResult ExportCountryStatusReport(CountryStatusModel ModelCountryStatus)
      {
         string URLPMRPorfolioReportExport = string.Empty;

         URLPMRPorfolioReportExport += ReportBuilder.GetReportPreffix("ParamForPMRPorfolioCountryReport");

         if (Lang != null)
         {
            URLPMRPorfolioReportExport += "&LANG=" + Lang;
         }
         else
         {
            URLPMRPorfolioReportExport += "&LANG=EN";
         }

         if (ModelCountryStatus.Country.Count > 0)
         {
            foreach (var itemCountry in ModelCountryStatus.Country)
            {
               URLPMRPorfolioReportExport += "&COUNTRY_ID=" + itemCountry;
            }
         }
         else
         {
            URLPMRPorfolioReportExport += "&COUNTRY_ID=-1";
         }

         if (ModelCountryStatus.CountryDepartment.Count > 0)
         {
            foreach (var itemCountryDepartment in ModelCountryStatus.CountryDepartment)
            {
               URLPMRPorfolioReportExport += "&COUNTRY_DEPARTMENT_ID=" + itemCountryDepartment;
            }
         }
         else
         {
            URLPMRPorfolioReportExport += "&COUNTRY_DEPARTMENT_ID=-1";
         }

         if (ModelCountryStatus.Division.Count > 0)
         {
            foreach (var itemDivision in ModelCountryStatus.Division)
            {
               URLPMRPorfolioReportExport += "&DIVISION_ID=" + itemDivision;
            }
         }
         else
         {
            URLPMRPorfolioReportExport += "&DIVISION_ID=-1";
         }

         if (ModelCountryStatus.SectorDepartment.Count > 0)
         {
            foreach (var itemSectorDepartment in ModelCountryStatus.SectorDepartment)
            {
               URLPMRPorfolioReportExport += "&DEPARTMENT_ID=" + itemSectorDepartment;
            }
         }
         else
         {
            URLPMRPorfolioReportExport += "&DEPARTMENT_ID=-1";
         }

         if (ModelCountryStatus.ESGTracking)
         {
            URLPMRPorfolioReportExport += "&ESG_TRACKING=true";
         }
         else
         {
            URLPMRPorfolioReportExport += "&ESG_TRACKING=false";
         }

         if (ModelCountryStatus.EV_PV)
         {
            URLPMRPorfolioReportExport += "&IS_ORIGINAL=true";
         }
         else
         {
            URLPMRPorfolioReportExport += "&IS_ORIGINAL=false";
         }

         URLPMRPorfolioReportExport += "&PMR_CYCLE_ID=" + ModelCountryStatus.PMRCycleId;

         URLPMRPorfolioReportExport += "&rs:Format=" + ModelCountryStatus.ExportType;

         return Content(URLPMRPorfolioReportExport);
      }

      public virtual JsonResult FilterCountrys(CountryStatusModel ModelCountryStatusModel)
      {
         var ListCountrys = ClientGenericRepositoty.GetCountriesFilter(Lang, ModelCountryStatusModel.CountryDepartment).OrderBy(x => x.Name).ToList();
         return new JsonResult() { Data = ListCountrys };
      }

      public virtual JsonResult FilterDivision(CountryStatusModel ModelCountryStatusModel)
      {
         var ListDivision = ClientGenericRepositoty.GetDivisionFilter(Lang, ModelCountryStatusModel.SectorDepartment).OrderBy(x => x.Name).ToList();
         return new JsonResult() { Data = ListDivision };
      }
   }
}
