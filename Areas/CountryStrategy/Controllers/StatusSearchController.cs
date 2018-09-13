using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

using IDB.Architecture.DataTables.DataTable.ServerSide;
using IDB.Architecture.Language;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.CountryStrategyModule.Enums;
using IDB.MW.Application.CountryStrategyModule.Services.StatusSearch;
using IDB.MW.Application.CountryStrategyModule.ViewModels.Core;
using IDB.MW.Application.CountryStrategyModule.ViewModels.StatusSearch;
using IDB.MW.Application.TCAbstractModule.Enums;
using IDB.MW.Domain.Values;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.MW.Infrastructure.Caching;
using IDB.MW.Infrastructure.Caching.Contracts;
using IDB.MW.Infrastructure.Helpers;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.Presentation.MVC4.Areas.CountryStrategy.Mappers;
using IDB.Presentation.MVC4.Areas.CountryStrategy.Models;
using IDB.Presentation.MVC4.General;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.CountryStrategy.Controllers
{
    public partial class StatusSearchController : BaseController
    {
        #region Fields

        private readonly IAuthorizationService _authorizationService;
        private readonly IEnumMappingService _enumMappingService;
        private readonly IStatusSearchService _statusSearchService;
        private readonly ICacheStorageService _cacheService;
        private readonly ICatalogService _catalogService;
        private readonly IConvergenceMasterDataRepository _convergenceMasterDataRepository;

        private readonly int _varInitialYear;
        private readonly int _varFinalizeYear;

        #endregion

        #region Contructors

        public StatusSearchController(
            IAuthorizationService authorizationService,
            ICatalogService catalogService,
            IStatusSearchService statusSearchService,
            IEnumMappingService enumMappingService,
            IConvergenceMasterDataRepository convergenceMasterDataRepository)
            : base(authorizationService)
        {
            _authorizationService = authorizationService;
            _enumMappingService = enumMappingService;
            _statusSearchService = statusSearchService;
            _catalogService = catalogService;
            _cacheService = CacheStorageFactory.Current;
            _convergenceMasterDataRepository = convergenceMasterDataRepository;

            _varFinalizeYear = DateTime.Today.Year + 5;
            _varInitialYear = 2010;
        }

        #endregion

        #region Action Methods

        public virtual ActionResult Read()
        {
            SetSearchViewBag();
            var model = InitializeModelSearch();
            model = GetStatusSearch(model);
            return View(model);
        }

        public virtual ActionResult Search(SearchFilterViewModel filter)
        {
            var responseMVC = new ReloadHtmlContentResponse();
            var model = new StatusSearchViewModel();

            model.Filter = filter;
            model = GetStatusSearch(model);

            var html = this.RenderRazorViewToString("ReadPartial/NonPagedTables", model.Tables);
            responseMVC.ContentToReplace.Add("[data-section=\"NonPagedTables\"]", html);
            return Json(responseMVC);
        }

        public virtual ActionResult SearchExpiredTable([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            var request = requestModel.ConvertToDataTableRequestViewModel();

            var response = _statusSearchService.GetStatusSearchExpired(request);

            if (!response.IsValid)
            {
                response.Expired = new RowsFilteredViewModel<RowStatusViewModel>()
                {
                    TotalElements = 0,
                    Rows = new List<RowStatusViewModel>()
                };
            }

            var result = from c 
                             in response.Expired.Rows 
                         select new[] 
            { 
                GetProuctNumberLink(c.ProductNumber),
                c.CountryStrategyName,
                GetDateLink(c.CDCKickOff),
                GetDateLink(c.CDCPeerReviewMeeting),
                GetDateLink(c.OverciewPaperPCM),
                GetDateLink(c.CPEDraftDeliveredVPC),
                GetDateLink(c.QRR),
                GetDateLink(c.CSPCM),
                GetDateLink(c.CSPCB),
                GetDateLink(c.CSCommitteeWhole),
                GetDateLink(c.CSMonitoringExercise)
            };

            var jsonResponse = new DataTablesResponse(requestModel.Draw, result, response.Expired.TotalElements, response.Expired.TotalElements);
            return Json(jsonResponse, JsonRequestBehavior.AllowGet);
        }

        private void SetSearchViewBag()
        {
            ViewBag.StatusList = GetStatusList();
            ViewBag.YearList = GetStatusYearsList();
            ViewBag.CountryList = GetCountryList();
            ViewBag.CountryDepartmentList = GetCountryDepartmentList();
        }

        #endregion

        #region Private Methods

        private StatusSearchViewModel InitializeModelSearch()
        {
            var model = new StatusSearchViewModel() { Filter = new SearchFilterViewModel() };

            model.Filter.Status.Add((int)CSStatusSearchStatusOperation.UnderPreparation);
            model.Filter.Status.Add((int)CSStatusSearchStatusOperation.Approved);

            for (int i = _varInitialYear; i <= _varFinalizeYear; i++)
            {
                model.Filter.Year.Add(i);
            }

            foreach (var item in ViewBag.CountryList)
            {
                model.Filter.Country.Add(int.Parse(item.Value));
            }

            foreach (var item in ViewBag.CountryDepartmentList)
            {
                model.Filter.CountryDepartment.Add(int.Parse(item.Value));
            }

            return model;
        }

        private StatusSearchViewModel GetStatusSearch(StatusSearchViewModel model)
        {
            var response = _statusSearchService.GetStatusSearch(model.Filter);
            model.Tables = response.Tables;
            SetViewBagErrorMessageInvalidResponse(response);
            return model;
        }

        private List<MultiDropDownItem> GetStatusList()
        {
            var list = new List<MultiDropDownItem>
            {
                new MultiDropDownItem
                {
                    Value = ((int)CSStatusSearchStatusOperation.UnderPreparation).ToString(),
                    Text = Localization.GetText("CS.StatusSearch.UnderPreparation")
                },
                new MultiDropDownItem
                {
                    Value = ((int)CSStatusSearchStatusOperation.Approved).ToString(),
                    Text = Localization.GetText("CS.StatusSearch.Approved")
                },
                new MultiDropDownItem
                {
                    Value = ((int)CSStatusSearchStatusOperation.Expired).ToString(),
                    Text = Localization.GetText("CS.StatusSearch.Expired") 
                }
            };

            return list;
        }

        private List<MultiDropDownItem> GetStatusYearsList()
        {
            var list = new List<MultiDropDownItem>();

            for (int i = _varInitialYear; i <= _varFinalizeYear; i++)
            {
                list.Add(new MultiDropDownItem
                {
                    Text = i.ToString(),
                    Value = i.ToString()
                });
            }

            return list;
        }

        private List<MultiDropDownItem> GetCountryList()
        {
            var listRepository = _catalogService.GetMasterDataListByTypeCode(hideExpired: true, typeCodes: ConvergenceMasterDataTypeEnum.Country.GetEnumDescription());
            var codeUndefined = _enumMappingService.GetMappingCode<CountryStrategyCountriesEnum>(CountryStrategyCountriesEnum.Undefined);
            List<MultiDropDownItem> list = listRepository.MasterDataCollection.ConvertToMultiDropDownItems();
            list.Remove(list.Single(x => x.Value == codeUndefined.ToString()));
            return list;
        }

        List<MultiDropDownItem> GetCountryDepartmentList()
        {
            var listRepository = _catalogService.GetMasterDataListByTypeCode(
                hideExpired: true,
                typeCodes: ConvergenceMasterDataTypeEnum.ORGANIZATION_UNIT.GetEnumDescription());

            var repositoryList = listRepository.MasterDataCollection
                .Where(x =>
                    x.Code == CountryDeparmentCode.CAN ||
                    x.Code == CountryDeparmentCode.CCB ||
                    x.Code == CountryDeparmentCode.CID ||
                    x.Code == CountryDeparmentCode.CSC)
                .ToList();

            return repositoryList.ConvertToMultiDropDownItems().OrderBy(x => x.Text).ToList();
        }

        private string GetProuctNumberLink(string productNumber)
        {
            var url = "/Operation/" + productNumber;
            return "<button class='buttonLink nopadding' data-navigate='" + url + "'>" + productNumber + "</button>";
        }

        private string GetDateLink(ActivityItemViewModel activityItem)
        {
            var url = Url.Action("DownloadDocument", "DocumentModal", new { area = string.Empty, documentNumber = "{0}" });
            var docs = activityItem.Documents;
            var builder = new StringBuilder();

            if (docs != null && docs.Any())
            {
                if (docs.Count > 1)
                {
                    builder.AppendLine(string.Format("<a href=\"#\" data-activityItem-id=\"{0}\">{1}</a>", activityItem.Id, activityItem.Date));
                    builder.AppendLine(string.Format("<div class=\"hide modalBody\" data-activityitem-popup=\"{0}\">", activityItem.Id));
                    builder.AppendLine("<ul>");
                    foreach (var doc in docs)
                    {
                        builder.AppendLine(string.Format("<li>Document: {0} <a name='documentLink' href=\"{1}\">(reference: {2})</a></li>", doc.Description, string.Format(url, doc.DocNumber), doc.DocNumber));
                    }

                    builder.AppendLine("</ul>");
                    builder.AppendLine("</div>");
                }
                else
                {
                    builder.AppendLine(string.Format("<a name='documentLink' href=\"{0}\">{1}</a>", string.Format(url, docs[0].DocNumber), activityItem.Date));
                }
            }
            else
            {
                builder.AppendLine(activityItem.Date);
            }

            return builder.ToString();
        }
        #endregion
    }
}