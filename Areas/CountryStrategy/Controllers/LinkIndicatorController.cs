using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.DataTables.DataTable.ServerSide;
using IDB.Architecture.Language;
using IDB.MW.Application.IndicatorsModuleNew.Services.ResultFramework;
using IDB.MW.Application.IndicatorsModuleNew.Services.LinkPredefinedIndicator;
using IDB.MW.Application.IndicatorsModuleNew.ViewModels.Core;
using IDB.MW.Application.IndicatorsModuleNew.ViewModels.LinkPredefinedIndicator;
using IDB.Presentation.MVC4.General;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVC4.Models.Inputs;

namespace IDB.Presentation.MVC4.Areas.CountryStrategy.Controllers
{
    public partial class LinkIndicatorController : MVC4.Controllers.ConfluenceController
    {
        #region Constants
        #endregion

        #region Fields
        private readonly ILinkPredefinedIndicatorService _linkPredefinedIndicatorService;
        private readonly IResultFrameworkService _resultFrameworkService;
        #endregion

        #region Contructors

        public LinkIndicatorController(ILinkPredefinedIndicatorService linkPredefinedIndicatorService, IResultFrameworkService resultFrameworkService)
            : base()
        {
            _linkPredefinedIndicatorService = linkPredefinedIndicatorService;
            _resultFrameworkService = resultFrameworkService;
        }

        #endregion

        #region Mock Methods
        public virtual ActionResult PruebaLink()
        {
            var outcomeResponse = _linkPredefinedIndicatorService.GetLinkIndicatorModel(LinkIndicatorTypeEnum.Outcomes, true, "Outcome");
            ViewBag.Outcome = outcomeResponse.Filter;

            var resultMatrixResponse = _linkPredefinedIndicatorService.GetLinkIndicatorModel(LinkIndicatorTypeEnum.ResultMatrix, true, "Result");
            ViewBag.Result = resultMatrixResponse.Filter;
            return View();
        }

        public virtual ActionResult TablaTest()
        {
            var impactsResponse = _linkPredefinedIndicatorService.GetLinkIndicatorModel(LinkIndicatorTypeEnum.Impacts, false, "Impacts");
            ViewBag.Impacts = impactsResponse.Filter;
            impactsResponse = _linkPredefinedIndicatorService.GetLinkIndicatorModel(LinkIndicatorTypeEnum.Outcomes, false, "Outcomes");
            ViewBag.Outcomes = impactsResponse.Filter;
            impactsResponse = _linkPredefinedIndicatorService.GetLinkIndicatorModel(LinkIndicatorTypeEnum.Outputs, false, "Outputs");
            ViewBag.Outputs = impactsResponse.Filter;

            return View();
        }

        public virtual ActionResult IndicatorsListTest()
        {
            var response = _resultFrameworkService.GetIndicatorsListTest();
            return View(response.Indicators);
        }
        #endregion

        #region Search Methods

        public virtual JsonResult SearchIndicators([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, LinkIndicatorTypeEnum? type = null, bool isPageLoad = false)
        {
            var request = requestModel.ConvertToDataTableRequestViewModel(removeArrayNon: true);

            if (isPageLoad || !type.HasValue || !request.CustomFilters.Any())
            {
                var res = from c in new List<string>() select new[] { c };
                return Json(new DataTablesResponse(requestModel.Draw, res, 0, 0), JsonRequestBehavior.AllowGet);
            }

            var response = _linkPredefinedIndicatorService.SearchIndicatorLink(request, type.Value);

            if (!response.IsValid)
            {
                response.Indicators = new RowsFilteredViewModel<RowIndicatorViewModel>()
                {
                    TotalElements = 0,
                    Rows = new List<RowIndicatorViewModel>()
                };
            }

            var result = from c in response.Indicators.Rows
                         select new[] 
            { 
                DisplayCheckboxIndicator(c),
                c.IndicatorNumber,
                c.IndicatorNameEn,
                c.UnitOfMeasureEn,
                GetUrlForCountryIndicator(c)
            };

            var jsonResponse = new DataTablesResponse(requestModel.Draw, result, response.Indicators.TotalElements, response.Indicators.TotalElements);
            return Json(jsonResponse, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult GetDetailIndicator(int indicatorId)
        {
            var result = new ReloadHtmlContentResponse();

            var response = _linkPredefinedIndicatorService.GetIndicatorDetail(indicatorId);
            result.IsValid = response.IsValid;
            result.Message = response.ErrorMessage;

            if (response.IsValid)
            {
                var html = this.RenderRazorViewToString("DetailIndicatorPartial", response.Indicator);
                result.ContentToReplace.Add("[data-name=\"detail-indicator-section\"]", html);
            }

            if (!result.ContentToReplace.Any())
            {
                result.ContentToReplace.Add("[data-name=\"detail-indicator-section\"]", string.Empty);
            }

            return Json(result);
        }
        #endregion

        #region Private Methods
        private string DisplayRadioButtonIndicator(RowIndicatorViewModel indicator)
        {
            var radioModel = new RadioButtonModel(
                indicator.IndicatorId.ToString(),
                "indicator-radio-button",
                dataAttributes: new Dictionary<string, string>()
                {
                    { "tableSelectable", "true" },
                },
                htmlClass: "radiobutton-selectable",
                jsonModel: indicator);

            return this.RenderRazorViewToString("Controls/Inputs/RadioButton", radioModel);
        }

        private string DisplayCheckboxIndicator(RowIndicatorViewModel indicator)
        {
            var checkboxModel = new CheckboxModel(
                false,
                "indicator-radio-button",
                indicator.IndicatorId.ToString(),
                dataAttributes: new Dictionary<string, string>()
                {
                    { "tableSelectable", "true" },
                },
                htmlClass: "radiobutton-selectable",
                jsonModel: indicator);

            return this.RenderRazorViewToString("Controls/Inputs/Checkbox", checkboxModel);
        }

        private string GetUrlForCountryIndicator(RowIndicatorViewModel indicator)
        {
            string urlTemplate = "<a data-id=\"{0}\" href=\"#\">{1}</a>";
            var result = string.Format(urlTemplate, indicator.IndicatorId, Localization.GetText("IM.EditReadIndicator.LinkRFPIndicator.IndicatorTable.DetailLink"));
            return result;
        }
        #endregion
    }
}