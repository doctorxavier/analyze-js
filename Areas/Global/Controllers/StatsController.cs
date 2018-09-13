using System;
using System.Collections.Generic;
using System.Web.Mvc;

using IDB.Architecture;
using IDB.Architecture.BusinessRules;
using IDB.MW.Domain.Contracts.ModelRepositories.Global;
using IDB.MW.Domain.Contracts.ModelRepositories.Supervision.PMI;
using IDB.MW.Domain.Models.Global;
using IDB.MW.Domain.Models.OperationProfile.BasicData;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Controllers;

namespace IDB.Presentation.MVC4.Areas.Global.Controllers
{
    public partial class StatsController : BaseController
    {
        private IGlobalModelRepository OperationRepository { get; set; }

        public StatsController()
        {
            OperationRepository = Globals.Resolve<IGlobalModelRepository>();
        }

        public virtual ActionResult Statistics(bool myOperations = false)
        {
            var UserName = string.Empty;

            if (myOperations == true)
            {
                UserName = IDBContext.Current.UserName;
            }

            var statsModel = OperationRepository.GetStatisticsModel(myOperations, IDBContext.Current.UserName);

            if (!TempData.ContainsKey("Stats"))
                TempData.Add("Stats", statsModel);
            else
                TempData["Stats"] = statsModel;

            ViewData["UserName"] = UserName;

            return View(statsModel);
        }

        public virtual PartialViewResult _CountryDepartment(int id)
        {
            var model = (OperationStatsModel)TempData["Stats"];
            List<StatisticsValuesModel> values;
            switch (id)
            {
                case 0:
                    ViewData["filterName"] = "Country";
                    values = model.CountryStats;
                    break;
                case 1:
                    ViewData["filterName"] = "CountryDepartment";
                    values = model.CountryDepartmentStats;
                    break;
                default:
                    values = new List<StatisticsValuesModel>();
                    break;
            }

            TempData["Stats"] = model;

            return PartialView("_StatisticsValueView", values);
        }

        public virtual PartialViewResult _SectorDepartment(int id)
        {
            var model = (OperationStatsModel)TempData["Stats"];
            List<StatisticsValuesModel> values;
            switch (id)
            {
                case 0:
                    ViewData["filterName"] = "Sector";
                    values = model.SectorStats;
                    break;
                case 1:
                    ViewData["filterName"] = "SectorDepartment";
                    values = model.SectorDepartmentStats;
                    break;
                default:
                    values = new List<StatisticsValuesModel>();
                    break;
            }

            TempData["Stats"] = model;

            return PartialView("_StatisticsValueView", values);
        }

        public virtual ActionResult SystemStatus(string language)
        {
            if (!string.IsNullOrEmpty(language))
            {
                IDBContext.Current.CurrentLanguage = language;
            }

            return View(new BasicDataModel());
        }

        public virtual ActionResult PersistsPMIIndicators(string operationNumber)
        {
            if (string.IsNullOrEmpty(operationNumber))
            {
                return Content("No selected operation");
            }

            var repository = Globals.Resolve<IPMIDetailsModelRepository>();
            repository.PersistsIndicators(operationNumber);
            return Content(string.Format("Indicator calculated for operation \"{0}\"", operationNumber));
        }

        public virtual ActionResult RenderBusinessModel(BusinessContext model)
        {
            return View(model);
        }
    }
}