using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.ResultMatrix.PredefinedIndicators;
using IDB.MW.Domain.Models.Administration;
using IDB.MW.Domain.Models.Common;
using IDB.MW.Domain.Models.Paging;
using IDB.MW.Domain.Models.ResultMatrix;
using IDB.Presentation.MVC4.Areas.Administration.Models.PredefinedIndicators;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.MVCCommon;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVC4.Models.Pagination;

namespace IDB.Presentation.MVC4.Areas.Administration.Controllers
{
    public partial class PredefinedIndicatorsController : BaseController
    {
        #region wcf services repositories

        public virtual IPredefinedIndicatorModelRepository PredefinedIndicatorModelRepository { get; set; }

        #endregion

        // GET: /Administration/PredefinedIndicator/
        public virtual ActionResult Index()
        {
            DefaultSearchModel initSearchModel = new DefaultSearchModel()
            {
                IndicatorTypes = null,
                PriorityAreas = null,
                ResultFrameworks = null,
                IndicatorNumber = string.Empty,
                IndicatorName = string.Empty,
                UnitOfMeasure = string.Empty,
                DisplayExpired = false
            };

            return View(initSearchModel);
        }

        [HttpPost]
        public virtual JsonResult Search(PredefinedIndicatorFilter filter = null)
        {
            // Set page and pagesize values according to the needs of every view
            PagingHelper pageHelper = new PagingHelper();
            QuerySettingsPage querySettingsPage = pageHelper.GetQueryPageConfiguration(
                filter.Page,
                PageDefaultSetUp.DefaultPagePredefineIndicator,
                filter.Size,
                PageDefaultSetUp.DefaultPageSizePredefineIndicator);

            // Get paged data
            var indicators = this.PredefinedIndicatorModelRepository.GetFilteredIndicators(ref querySettingsPage, filter);

            // Set detail url
            indicators.ForEach(ind => ind.DetailUri = Url.Action("Detail", "PredefinedIndicators", new { area = "Administration" }));

            // Set Paging information to send to the view
            PageSettings pageSettings = pageHelper.GetPagingInformation(querySettingsPage);

            pageSettings.BaseURL = Url.Action("Search", "PredefinedIndicators", new { area = "Administration" });

            return Json(indicators);
        }

        [HttpPost]
        public virtual JsonResult GetPriorityAreasIndicator()
        {
            var areas = this.PredefinedIndicatorModelRepository.GetPriorityAreasIndicator();
            return Json(areas);
        }

        public virtual ActionResult Detail(int predefinedIndicatorId = 0)
        {
            return View(predefinedIndicatorId);
        }

        public virtual ActionResult Edit()
        {
            return View();
        }

        [HttpPost]
        public virtual string GetDetailIndicator(int predefinedIndicatorId = 0)
        {
            var indicator = new CompletePredefinedIndicator
            {
                Disaggregations = new List<SimpleI18Resource>(),
                Sectors = new List<PredefinedIndicatorSubType>(),
                Countries = new List<PredefinedIndicatorSubType>(),
            };

            if (predefinedIndicatorId > 0)
            {
                indicator = this.PredefinedIndicatorModelRepository.GetDetailedIndicator(predefinedIndicatorId);
            }
            else
            {
                indicator.TechnicalFields = this.PredefinedIndicatorModelRepository.GetTechnicalFieldsMasterData();
            }

            indicator.PriorityAreas = this.PredefinedIndicatorModelRepository.GetPriorityAreasIndicator();

            IsoDateTimeConverter converter = new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yy", Culture = CultureInfo.InstalledUICulture };

            string isoJson = JsonConvert.SerializeObject(indicator, converter);

            return isoJson;
        }

        public virtual JsonResult SavePredefinedIndicatorDetails(CompletePredefinedIndicator indicator)
        {
            if (!string.IsNullOrEmpty(indicator.TypeText))
            {
                indicator.TypeIndicator = indicator.TypeText;
            }

            var result = this.PredefinedIndicatorModelRepository.SavePredefinedIndicatorDetail(indicator);
            MessageConfiguration msg = new MessageConfiguration
            {
                Message = result,
                Type = "Error",
                Duration = "100",
                AutoClose = "false"
            };

            ViewData["message"] = msg;

            return Json(result);
        }

        /// <summary>
        /// Get list of Disaggregations.
        /// </summary>
        /// <returns>List of Disaggregations</returns>
        public virtual JsonResult GetTypesIndicator(int selectedIndicatorTypes)
        {
            var indicatorTypes = this.PredefinedIndicatorModelRepository.GetIndicatorTypesResults(selectedIndicatorTypes);
            var return_ = Json(new List<SimpleI18Resource>());
            for (int i = 0; i < indicatorTypes.Count; i++)
            {
                switch (indicatorTypes[i].Code.ToString())
                {
                    case "COUNTRY": return_ = GetCountries();
                        i = indicatorTypes.Count * 2;
                        break;
                    case "RESULTS_FRAMEWORK": return_ = GetResultsFrameWorkSubType();
                        i = indicatorTypes.Count * 2;
                        break;
                    case "SECTOR": return_ = GetSectors();
                        i = indicatorTypes.Count * 2;
                        break;
                }
            }

            return return_;
        }

        /// <summary>
        /// Get list of Disaggregations.
        /// </summary>
        /// <returns>List of Disaggregations</returns>
        public virtual JsonResult GetIndicatorTypes()
        {
            var indicatorTypes = this.PredefinedIndicatorModelRepository.GetIndicatorTypes();
            return Json(indicatorTypes);
        }

        /// <summary>
        /// Get list RF.
        /// </summary>
        /// <returns>List of Disaggregations</returns>
        public virtual JsonResult GetResultsFrameWorkSubType()
        {
            var indicatorTypes = this.PredefinedIndicatorModelRepository.GetResultsFrameWorkSubType();

            return Json(indicatorTypes);
        }

        /// <summary>
        /// Get list of Disaggregations.
        /// </summary>
        /// <returns>List of Disaggregations</returns>
        public virtual JsonResult GetDisaggregations()
        {
            var disaggregations = this.PredefinedIndicatorModelRepository.GetDisaggregations();
            return Json(disaggregations);
        }

        /// <summary>
        /// Get list of countries
        /// </summary>
        /// <returns>List of countries</returns>
        public virtual JsonResult GetCountries()
        {
            var countries = this.PredefinedIndicatorModelRepository.GetCountries();
            return Json(countries);
        }

        /// <summary>
        /// Get list of sectors
        /// </summary>
        /// <returns>List of sectors</returns>
        public virtual JsonResult GetSectors()
        {
            var sectors = this.PredefinedIndicatorModelRepository.GetSectors();
            return Json(sectors);
        }
    }
}