using System;
using System.Web;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.ResultMatrix.Outcomes;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.ResultMatrix.PredefinedIndicators;
using IDB.MW.Domain.Models.Architecture.Enumerations;
using IDB.MW.Domain.Models.Paging;
using IDB.MW.Domain.Models.ResultMatrix;
using IDB.MW.Domain.Models.ResultMatrix.PredefinedIndicator;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Areas.Administration.Models.PredefinedIndicators;
using IDB.Presentation.MVC4.Areas.ResultsMatrix.Models;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.MVCCommon;
using IDB.Presentation.MVC4.Models.Pagination;

namespace IDB.Presentation.MVC4.Areas.ResultsMatrix.Controllers
{
    public partial class PredefinedIndicatorController : BaseController
    {
        #region WCF REPOSITORY SERVICES
        public virtual IPredefinedIndicatorModelRepository ClientPredefinedIndicator
        {
            get { return _clientPredefinedIndicator; }
            set { _clientPredefinedIndicator = value; }
        }

        public virtual IResultsMatrixModelRepository ClientResultsMatrix
        {
            get { return _clientResultsMatrix; }
            set { _clientResultsMatrix = value; }
        }

        private IPredefinedIndicatorModelRepository _clientPredefinedIndicator = null;

        private IResultsMatrixModelRepository _clientResultsMatrix = null;

        #endregion

        public virtual ActionResult Index(int predefinedIndicatorId = 0, string module = "")
        {
            DefaultSearchModel initSearchModel = new DefaultSearchModel();

            initSearchModel.IndicatorTypes = this.ClientPredefinedIndicator.GetIndicatorTypes();
            initSearchModel.PriorityAreas = null;
            initSearchModel.ResultFrameworks = null;
            initSearchModel.IndicatorNumber = string.Empty;
            initSearchModel.IndicatorName = string.Empty;
            initSearchModel.UnitOfMeasure = string.Empty;
            initSearchModel.DisplayExpired = false;
            initSearchModel.CurrentIndicatorid = predefinedIndicatorId;
            initSearchModel.Module = module;

            return View(initSearchModel);
        }

        [OutputCache(Duration = 60)]
        [AllowAnonymous]
        public virtual JsonResult GetIndicatorTypes()
        {
            var types = this.ClientPredefinedIndicator.GetIndicatorTypes();
            return Json(new { data = types }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public virtual JsonResult GetIndicatorAreas(int indicatorId)
        {
            var areas = this.ClientPredefinedIndicator.GetAreaTypes(indicatorId);
            return Json(new { areas }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public virtual ActionResult Link(int predefinedIndicatorId = 0, string module = "")
        {
            DefaultSearchModel initSearchModel = new DefaultSearchModel()
            {
                IndicatorTypes = this.ClientPredefinedIndicator.GetIndicatorTypes(),
                PriorityAreas = null,
                ResultFrameworks = null,
                IndicatorNumber = string.Empty,
                IndicatorName = string.Empty,
                UnitOfMeasure = string.Empty,
                DisplayExpired = false,
                CurrentIndicatorid = predefinedIndicatorId,
                Module = module
            };

            return View(initSearchModel);
        }

        [HttpPost]
        public virtual JsonResult Search(PredefinedIndicatorFilter filter)
        {
            // Set page and pagesize values according to the needs of every view
            PagingHelper pageHelper = new PagingHelper();
            QuerySettingsPage querySettingsPage = pageHelper.GetQueryPageConfiguration(
                filter.Page,
                PageDefaultSetUp.DefaultPagePredefineIndicator,
                filter.Size,
                PageDefaultSetUp.DefaultPageSizePredefineIndicator);

            // Get paged data
            var indicators = this.ClientPredefinedIndicator.GetFilteredIndicators(
                ref querySettingsPage, filter);

            // Set Paging information to send to the view
            PageSettings pageSettings = pageHelper.GetPagingInformation(querySettingsPage);

            pageSettings.BaseURL = Url.Action("Search", "PredefinedIndicator");

            return Json(new { Indicators = indicators, PageSettings = pageSettings });
        }

        [HttpPost]
        public virtual JsonResult LinkPredefinedIndicators(
            CustomLinkPredefinedIndicatorModel linkModel, IndicatorType type)
        {
            var result = ClientPredefinedIndicator.LinkIndicator(
                linkModel.baseIndicatorId, linkModel.predefinedIndicatorId, type);
            var updatedUrl = UpdateRequestModuleUrl(linkModel.backUrl, result, type, 66, 99);

            return Json(new { ok = result, section = type, backUrl = updatedUrl });
        }

        public virtual JsonResult GetDetailedIndicator(int predefinedIndicatorId)
        {
            var indicator = this.ClientPredefinedIndicator.GetDetailedIndicator(predefinedIndicatorId);
            return Json(new { indicator }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public virtual ActionResult UnlinkPredefinedIndicator(int resultsMatrixId, int indicatorId, int module)
        {
            // Check if the administrator is accessing
            var accessedByAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);

            // Get results matrix with interval data associated to administrator
            var resultModel = ClientResultsMatrix.GetLightResultsMatrixModel(new IDB.MW.Domain.Models.Architecture.ResultMatrix.Outcomes.ResultsMatrixModel() { ResultsMatrixId = resultsMatrixId, AccessedByAdministrator = accessedByAdministrator });

            // Define model to render in the view
            UnlinkPredefinedIndicatorModel unLinkPredefinedIndicator = new UnlinkPredefinedIndicatorModel() { ResultsMatrixId = resultsMatrixId, IndicatorId = indicatorId, IndicatorType = module, AccessedByAdministrator = accessedByAdministrator, IntervalId = resultModel.Interval.IntervalId, IsThirdInterval = resultModel.IsThirdInterval };

            // Set default unlink message
            ViewData["defaultUnlinkPredefinedIndicatorMessage"] = Localization.GetText("This action cannot be undone, are you sure you wish to continue?");

            // Set warning maessage
            ViewData["thirdIntervalunLinkPredefinedIndicatorMessage"] = (resultModel.Interval.IntervalId == ResultsMatrixCodes.ThirdInterval || (accessedByAdministrator && resultModel.IsThirdInterval)) ? Localization.GetText("This change will be registered on the “Changes to the Matrix” section and you will be required to explain it. Would you like to proceed?") : string.Empty;

            return PartialView("Unlink", unLinkPredefinedIndicator);
        }

        [HttpPost]
        public virtual ActionResult Unlink(UnlinkPredefinedIndicatorModel unlinkPredefinedIndicator)
        {
            bool success = true;

            try
            {
                this.ClientPredefinedIndicator.UnlinkPredefinedIndicator(unlinkPredefinedIndicator);
            }
            catch (System.Exception)
            {
                success = false;
            }

            var updatedUrl = UpdateRequestModule_Url(unlinkPredefinedIndicator.RequesModuleUrl, success, unlinkPredefinedIndicator.IndicatorType, 67, 100);

            return Redirect(updatedUrl);
        }

        [HttpPost]
        public virtual JsonResult GetPriorityAreas(int indicatorId = 0)
        {
            var areas = this.ClientPredefinedIndicator.GetAreaTypes(indicatorId);
            return Json(new { Areas = areas });
        }

        [HttpGet]
        public virtual ActionResult Warning(int indicatorId = 0, string module = "")
        {
            this.ViewData["indicatorId"] = indicatorId;

            this.ViewData["module"] = module;

            return PartialView();
        }

        private string UpdateRequestModuleUrl(string urlRequest, bool status, IndicatorType module, int successCode, int errorCode)
        {
            string baseUrl = string.Empty, urlKey = string.Empty, newKeyValue = string.Empty;

            switch (module)
            {
                case IndicatorType.Impact:
                    urlKey = "code";
                    newKeyValue = status ? successCode.ToString() : errorCode.ToString();
                    break;
                case IndicatorType.Output:
                    urlKey = "messageStatus";
                    newKeyValue = status ? successCode.ToString() : errorCode.ToString();
                    break;
                case IndicatorType.Outcome:
                    urlKey = "code";
                    newKeyValue = status ? successCode.ToString() : errorCode.ToString();
                    break;
            }

            var uri = new Uri(urlRequest);

            // Gets all the query string key value pairs as a collection
            var newQueryString = HttpUtility.ParseQueryString(uri.Query);

            // Removes the key if exists
            newQueryString.Remove(urlKey);

            // Add new key with new value
            newQueryString.Add(urlKey, newKeyValue);

            // Get base url  
            baseUrl = uri.GetLeftPart(UriPartial.Path);

            // Get updated url
            return newQueryString.Count > 0 ? string.Format("{0}?{1}", baseUrl, newQueryString) : baseUrl;
        }

        private string UpdateRequestModule_Url(string urlRequest, bool status, int module, int successCode, int errorCode)
        {
            string baseUrl = string.Empty, urlKey = string.Empty, newKeyValue = string.Empty;

            if (module == PredefinedIndicatorCodes.IMPACTINDICATOR || module == PredefinedIndicatorCodes.OUTCOMEINDICATOR)
            {
                urlKey = "code";
                newKeyValue = status ? "67" : "100";
            }
            else
            {
                urlKey = "messageStatus";
                newKeyValue = status ? "67" : "100";
            }

            var uri = new Uri(urlRequest);

            // Gets all the query string key value pairs as a collection
            var newQueryString = HttpUtility.ParseQueryString(uri.Query);

            // Removes the key if exists
            newQueryString.Remove(urlKey);

            // Add new key with new value
            newQueryString.Add(urlKey, newKeyValue);

            // Get base url  
            baseUrl = uri.GetLeftPart(UriPartial.Path);

            // Get updated url
            return newQueryString.Count > 0 ? string.Format("{0}?{1}", baseUrl, newQueryString) : baseUrl;
        }
    }
}