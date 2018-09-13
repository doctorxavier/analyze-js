using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.MW.Domain.Contracts.ModelRepositories.Reports.Generic;
using IDB.MW.Domain.Models.Architecture.Reports.PMRExtraModels.Visualization.VisualizationPorfolio;
using IDB.Presentation.MVC4.Controllers;
using IDB.Architecture.Language;

namespace IDB.Presentation.MVC4.Areas.Reports.Controllers
{
    public partial class VisualizationPorfolioReportController : BaseController
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

        public virtual ActionResult Create(string OperationNumber)
        {
            VisualizationPorfolioSearch ModelVisualizationPorfolioSearch = new VisualizationPorfolioSearch();
            ModelVisualizationPorfolioSearch.OperationID = ClientGenericRepositoty.GetOperationIDForOperationNumber(OperationNumber);

            var ListCountryDepartment = ClientGenericRepositoty.GetCountryDepartments(Lang).OrderBy(x => x.Name);
            var ListSectorDepartment = ClientGenericRepositoty.GetSectorDepartment(Lang).OrderBy(x => x.Name);
            var ListVisualOutputDeliveryStatus = ClientGenericRepositoty.GetVisualOutputDeliveryStatus(Lang).OrderBy(x => x.Name);
            var ListCountry = ClientGenericRepositoty.GetCountries(Lang).OrderBy(x => x.Name);
            var ListDivision = ClientGenericRepositoty.GetDivisions(Lang).OrderBy(x => x.Name);

            var ListYears = ClientGenericRepositoty.GetYearsForLastResultMatrixByOperationID(ModelVisualizationPorfolioSearch.OperationID);
            ModelVisualizationPorfolioSearch.ListStartYear = ListYears;
            ModelVisualizationPorfolioSearch.ListEndYear = ListYears;

            var ListESGTracking = new Dictionary<bool, string>();
            ListESGTracking.Add(true, Localization.GetText("Yes"));
            ListESGTracking.Add(false, Localization.GetText("No"));

            var ListExternalInternalMap = new Dictionary<int, string>();
            ListExternalInternalMap.Add(2, "All");
            ListExternalInternalMap.Add(0, "Internal Map");
            ListExternalInternalMap.Add(1, "External Map");

            ViewBag.ListCountryDepartment = new MultiSelectList(ListCountryDepartment, "convergencemasterdataid", "name");
            ViewBag.ListSectorDepartment = new MultiSelectList(ListSectorDepartment, "convergencemasterdataid", "name");
            ViewBag.ListVisualOutputDeliveryStatus = new MultiSelectList(ListVisualOutputDeliveryStatus, "convergencemasterdataid", "name");
            ViewBag.ListCountry = new MultiSelectList(ListCountry, "convergencemasterdataid", "name");
            ViewBag.ListDivision = new MultiSelectList(ListDivision, "convergencemasterdataid", "name");
            ViewBag.ListStartYear = new SelectList(ModelVisualizationPorfolioSearch.ListStartYear, ModelVisualizationPorfolioSearch.ListStartYear.Select(x => new SelectListItem { Value = x, Text = x }));
            ViewBag.ListEndYear = new SelectList(ModelVisualizationPorfolioSearch.ListEndYear, ModelVisualizationPorfolioSearch.ListEndYear.Select(x => new SelectListItem { Value = x, Text = x }));
            ViewBag.ListESGTracking = new SelectList(ListESGTracking, "key", "value");
            ViewBag.ListExternalInternalMap = new SelectList(ListExternalInternalMap, "key", "value");

            return View(ModelVisualizationPorfolioSearch);
        }

        [HttpPost()]
        public virtual ActionResult Create(VisualizationPorfolioSearch ModelVisualizationPorfolioSearch)
        {
            return Content("No available");
        }

        public virtual ActionResult ExportVisualizationPorfolioSearchReport(VisualizationPorfolioSearch ModelVisualizationPorfolioSearch)
        {
            return Content("No available");
        }

        public virtual JsonResult FilterCountrys(VisualizationPorfolioSearch VisualizationPorfolioModel)
        {
            var ListCountrys = ClientGenericRepositoty.GetCountriesFilter(Lang, VisualizationPorfolioModel.CountryDepartment).OrderBy(x => x.Name).ToList();
            return new JsonResult() { Data = ListCountrys };
        }

        public virtual JsonResult FilterDivision(VisualizationPorfolioSearch VisualizationPorfolioModel)
        {
            var ListDivision = ClientGenericRepositoty.GetDivisionFilter(Lang, VisualizationPorfolioModel.SectorDepartment).OrderBy(x => x.Name).ToList();
            return new JsonResult() { Data = ListDivision };
        }
    }
}
