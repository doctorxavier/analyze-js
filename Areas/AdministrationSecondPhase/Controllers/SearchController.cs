using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;

using IDB.Architecture.DataTables.DataTable.ServerSide;
using IDB.Architecture.Language;
using IDB.MW.Application.AdministrationModule.Services.MasterDataService;
using IDB.MW.Application.AdministrationModule.Services.SearchService;
using IDB.MW.Application.AdministrationModule.ViewModels.SearchService;
using IDB.MW.Application.AttributesModule.Services;
using IDB.MW.Application.AttributesModule.ViewModels.AttributesService;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.Core.ViewModels.FormRendering;
using IDB.MW.Application.TCAbstractModule.Enums;
using IDB.MW.Business.Core.SharepointSecurityService;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.MW.Infrastructure.BaseClasses;
using IDB.MW.Infrastructure.Caching;
using IDB.MW.Infrastructure.Caching.Contracts;
using IDB.MW.Infrastructure.Helpers;
using IDB.MW.Infrastructure.SecurityService;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.MW.Infrastructure.SecurityService.Enums;
using IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.MasterData;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Controllers
{
    public partial class SearchController : MVC4.Controllers.ConfluenceController
    {
        #region Fields

        private readonly IAuthorizationService _authorizationService;
        private readonly ISearchService _searchService;
        private readonly ICatalogService _catalogService;
        private readonly IEnumMappingService _enumMappingService;
        private readonly IMasterDataService _masterDataService;
        private readonly IAttributesEngineService _attributesEngineService;

        // Este servicio se usa exclusivamente en fase de pruebas,
        // empleado para setear los permisos de usuario en este controller.
        string[] _pageSize = new[] { "10", "20", "50", "100" };
        private ICacheStorageService _cacheStorageService;
        #endregion

        #region Contructors
        public SearchController(
            ISearchService searchService,
            ICatalogService catalogService,
            IEnumMappingService enumMappingService,
            IAttributesEngineService attributesEngineService,
            IMasterDataService masterDataService)
        {
            _authorizationService = AuthorizationServiceFactory.Current;
            _searchService = searchService;
            _catalogService = catalogService;
            _cacheStorageService = CacheStorageFactory.Current;
            _enumMappingService = enumMappingService;
            _attributesEngineService = attributesEngineService;
            _masterDataService = masterDataService;
        }

        #endregion

        #region Search
        public virtual ActionResult Search()
        {
            var keyword = Request.QueryString["keyword"];
            var OperationNumber = Request.QueryString["OperationNumber"];

            SetViewBagSearch(keyword, OperationNumber);

            return View();
        }

        public virtual JsonResult SearchAjaxHandler(
            [ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            if (requestModel.CustomFilters.Count == 0)
            {
                return Json(
                    new DataTablesResponse(0, new List<object[]>().ToArray(), 0, 0), 
                    JsonRequestBehavior.AllowGet);
            }

            var request = requestModel.ConvertToDataTableRequestViewModel();            
            var response = _searchService.GetListItems(request);

            if (!response.IsValid)
            {
                return Json(
                    new DataTablesResponse(0, new List<object[]>().ToArray(), 0, 0), 
                    JsonRequestBehavior.AllowGet);
            }            

            var pathSharePoint = IDB.Architecture.Globals.GetSetting("BasePath");
            var operationURL = string.Format("{0}Operation/", pathSharePoint.EndsWith(SearchValues.DIVIDER) ?
                pathSharePoint : pathSharePoint + SearchValues.DIVIDER);
            var operationURLDraft = string.Format("{0}{1}", operationURL, SearchValues.URL_OPERATION_DRAFT);

            var result = 
                from c in response.Search
                         select new[] 
            {               
                (c.ValidationStage == "DRAFT" || c.ValidationStage == "REV_AUT" ||
                 c.ValidationStage == "REV_DC" || c.ValidationStage == "REV_GCM" ||
                 c.ValidationStage == "REV_REG" || c.ValidationStage == "REV_REP" ||
                 c.ValidationStage == "REV_TL" || c.ValidationStage == "REV_VPC") ?
                        string.Format(
                            "<a data-validationStage=\"{0}\"" +
                                "data-operationNumber = \"{1}\" " +
                                "href=\"{2}={1}\">{3}</a>",
                            c.ValidationStage,
                            c.OperationNumber,
                            operationURLDraft,
                            c.OperationNumber) :
                        string.Format(
                            "<a data-validationStage=\"{0}\"" +
                                "data-operationNumber = \"{1}\"" +
                                "href=\"{2}{1}\">{3}</a>",
                            c.ValidationStage, 
                            c.OperationNumber, 
                            operationURL, 
                            c.OperationNumber),

                    string.Format(
                        "<span data-operationNumber = \"{0}\">{1}</span>",
                    c.OperationNumber, 
                    c.OperationType),

                    string.Format(
                        "<span data-operationNumber = \"{0}\">{1}</span>",
                    c.OperationNumber, 
                    c.ResponsableUnit),

                    string.Format(
                        "<span class='pull-right' data-operationYear = \"{0}\">{1}</span>",
                    c.OperationNumber, 
                    c.OperationYear),

                    string.Format(
                        "<span data-operationNumber = \"{0}\">{1}</span>",
                    c.OperationNumber, 
                    c.ApprovalNumber),

                (c.ValidationStage == "DRAFT" || c.ValidationStage == "REV_AUT" ||
                 c.ValidationStage == "REV_DC" || c.ValidationStage == "REV_GCM" ||
                 c.ValidationStage == "REV_REG" || c.ValidationStage == "REV_REP" ||
                 c.ValidationStage == "REV_TL" || c.ValidationStage == "REV_VPC") ?
                        string.Format(
                            "<a data-validationStage=\"{0}\"" +
                                "data-operationNumber = \"{1}\"" +
                                "href=\"{2}={1}\">{3}</a>",
                            c.ValidationStage,
                            c.OperationNumber,
                            operationURLDraft,
                            c.OperationName) :
                        string.Format(
                            "<a data-validationStage=\"{0}\"" +
                                "data-operationNumber = \"{1}\"" +
                                "href=\"{2}{1}\">{3}</a>",
                            c.ValidationStage,
                            c.OperationNumber,
                            operationURL,
                            c.OperationName),

                string.Format("<span data-operationNumber = \"{0}\">{1}</span>",
                    c.OperationNumber, 
                    c.TeamLeader),

                string.Format("<span data-operationNumber = \"{0}\">{1}</span>",
                    c.OperationNumber, 
                    c.Status),

                string.Format("<span class='pull-right'data-operationNumber = \"{0}\">{1}</span>",
                    c.OperationNumber, 
                    c.OriginalAmount)
            };

            var jsonResponse = new DataTablesResponse(
                requestModel.Draw, result, response.TotalElements, response.TotalElements);

            return Json(jsonResponse, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult SaveSearchFavorites(Dictionary<string, string> favorites)
        {
            var response = _searchService.SaveFavorites(favorites);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetAttibutesSearch(int operationType)
        {
            var model = new SearchAttributesViewModel()
            {
                Attributes = new FormAttributeOperationCollectionViewModel(),
                FormAttributes = new FormDataViewModel()
            };

            var response = _searchService.GetAttributesByOperationType(operationType);

            model.Attributes = response.Attributes;
            model.FormAttributes = response.FormAttributes;

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            return PartialView("Partial/Search/SearchPartialAttribute", model);
        }

        public virtual ActionResult RetrieveFavorites()
        {
            var response = _searchService.RetrieveFavorites();

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetCountryByCountryDepartment(string countryId)
        {
            var countryList = new List<SelectListItem>();
            var response = _searchService.GetCountryByCountryDepartments(countryId);
            if (response.IsValid)
            {
                countryList = response.Country.Select(x => new SelectListItem
                {
                    Value = x.ConvergenceMasterDataId.ToString(),
                    Text = x.Code,
                }).ToList();

                var item = new SelectListItem();
                item.Text = "Country";
                countryList.Add(item);
            }

            ViewBag.Country = countryList;

            return Json(countryList, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetOverallStageByOperationType(string operationTypeId)
        {
            var masterDataRepository = _catalogService
                .GetMasterDataListByTypeCode(
                    hideExpired: true,
                    typeCodes: ConvergenceMasterDataTypeEnum.OverallStage.GetEnumDescription());

            var overallStageList = new List<MultiSelectListItem>();
            int opTypeId;

            if (int.TryParse(operationTypeId, out opTypeId))
            {
                var overallStageByTemplate = _searchService
                    .GetOverallStageByCaseTemplateAndOperationType(opTypeId);
                var overallStageByOpType = _searchService
                    .GetOverallStageByOperationType(opTypeId);

                overallStageList = overallStageByTemplate
                    .BuildMultiSelectItemList(
                        o => o.OperationStage.NameEn,
                        o => o.OperationStageId.ToString())
                    .ToList();

                if (overallStageByOpType.IsValid)
                {
                    overallStageList.AddRange(overallStageByOpType.Models
                        .BuildMultiSelectItemList(
                            o => o.NameEn,
                            o => o.ConvergenceMasterDataId.ToString()));
                }
            }
            else
            {
                foreach (var item in masterDataRepository.MasterDataCollection)
                {
                    switch (item.NameEn)
                    {
                        case SearchValues.OVERALLSTAGE_ORDER_1:
                            item.Order = 1;
                            break;

                        case SearchValues.OVERALLSTAGE_ORDER_3:
                            item.Order = 3;
                            break;

                        case SearchValues.OVERALLSTAGE_ORDER_4:
                            item.Order = 4;
                            break;

                        case SearchValues.OVERALLSTAGE_ORDER_5:
                            item.Order = 5;
                            break;

                        case SearchValues.OVERALLSTAGE_ORDER_6:
                            item.Order = 6;
                            break;

                        case SearchValues.OVERALLSTAGE_ORDER_7:
                            item.Order = 7;
                            break;

                        case SearchValues.OVERALLSTAGE_ORDER_8:
                            item.Order = 8;
                            break;

                        case SearchValues.OVERALLSTAGE_ORDER_9:
                            item.Order = 9;
                            break;

                        case SearchValues.OVERALLSTAGE_ORDER_10:
                            item.Order = 10;
                            break;

                        case SearchValues.OVERALLSTAGE_ORDER_11:
                            item.Order = 11;
                            break;

                        case SearchValues.OVERALLSTAGE_ORDER_12:
                            item.Order = 12;
                            break;

                        case SearchValues.OVERALLSTAGE_ORDER_13:
                            item.Order = 13;
                            break;

                        case SearchValues.OVERALLSTAGE_ORDER_14:
                            item.Order = 14;
                            break;

                        case SearchValues.OVERALLSTAGE_ORDER_15:
                            item.Order = 15;
                            break;

                        case SearchValues.OVERALLSTAGE_ORDER_16:
                            item.Order = 16;
                            break;

                        case SearchValues.OVERALLSTAGE_ORDER_17:
                            item.Order = 17;
                            break;

                        case SearchValues.OVERALLSTAGE_ORDER_18:
                            item.Order = 18;
                            break;

                        case SearchValues.OVERALLSTAGE_ORDER_19:
                            item.Order = 19;
                            break;

                        case SearchValues.OVERALLSTAGE_ORDER_20:
                            item.Order = 20;
                            break;

                        case SearchValues.OVERALLSTAGE_ORDER_21:
                            item.Order = 21;
                            break;

                        case SearchValues.OVERALLSTAGE_ORDER_22:
                            item.Order = 22;
                            break;

                        case SearchValues.OVERALLSTAGE_ORDER_23:
                            item.Order = 23;
                            break;
                            }
                            }

                masterDataRepository.MasterDataCollection =
                    masterDataRepository.MasterDataCollection.OrderBy(q => q.Order).ToList();

                if (masterDataRepository.IsValid && masterDataRepository.MasterDataCollection.Any())
                {
                    overallStageList = masterDataRepository.MasterDataCollection
                        .BuildMultiSelectItemList(
                            o => o.NameEn,
                            o => o.MasterId.ToString())
                        .ToList();
                }
            }

            ViewModelMapperHelper.FillDefaultOverallStage(masterDataRepository, overallStageList);

            ViewBag.OverallStage = overallStageList.DistinctBy(q => q.Value).ToList();

            return Json(overallStageList, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetDivisionBySector(string sectorId)
        {
            var divisionList = new List<SelectListItem>();
            var response = _searchService.GetDivisionOpusWithExpired();

            response.DivisionOpusWithExpired =
                response.DivisionOpusWithExpired.Where(q => q.SectorDepartmentId != null);

            if (!string.IsNullOrEmpty(sectorId))
            {
                var sector = Convert.ToInt32(sectorId);
                response.DivisionOpusWithExpired =
                    response.DivisionOpusWithExpired.Where(q => q.SectorDepartmentId.Value == sector);
            }

            if (response.IsValid)
            {
                divisionList = response.DivisionOpusWithExpired
                    .Select(x => new SelectListItem
                        {
                            Value = SqlFunctions.StringConvert((double)x.DivisionId).Trim(),
                            Text = x.DivisionCode + " - " + x.DivisionEn,
                        })
                    .ToList();

                var item = new SelectListItem();
                item.Text = "Division";

                divisionList.Add(item);
            }

            ViewBag.Division = divisionList;

            return Json(divisionList, JsonRequestBehavior.AllowGet);
        }

        public virtual FileResult DownloadReport(string filters)
        {
            var filterDetails = JsonConvert.DeserializeObject<FiltersDetailViewModel>(filters);
            var values = new Dictionary<string, string>();
            values.Add(FiltersValues.GENERAL_SEARCH, filterDetails.GeneralSearch);
            values.Add(FiltersValues.DIVISION_FILTER, filterDetails.DivisionFilter);
            values.Add(FiltersValues.COUNTRY_FILTER, filterDetails.CountryFilter);
            values.Add(FiltersValues.OPERATION_TYPE_FILTER, filterDetails.OperationTypeFilter);
            values.Add(FiltersValues.SIMPLE_EVENT_FILTER, filterDetails.SimpleEventFilter);
            values.Add(FiltersValues.SIMPLE_YEAR_FROM_FILTER, filterDetails.SimpleYearFromFilter);
            values.Add(FiltersValues.SIMPLE_YEAR_TO_FILTER, filterDetails.SimpleYearToFilter);
            values.Add(FiltersValues.ADVANCED_EVENT_FILTER, filterDetails.AdvancedEventFilter);
            values.Add(FiltersValues.ADVANCED_YEAR_FROM_FILTER, filterDetails.AdvancedYearFromFilter);
            values.Add(FiltersValues.ADVANCED_YEAR_TO_FILTER, filterDetails.AdvancedYearToFilter);
            values.Add(FiltersValues.DEPARTMENT_SECTOR_FILTER, filterDetails.DepartmentSectorFilter);
            values.Add(FiltersValues.COUNTRY_DEPARTMENT_FILTER, filterDetails.CountryDepartmentFilter);
            values.Add(FiltersValues.FUND_FILTER, filterDetails.FundFilter);
            values.Add(FiltersValues.SUBPHASE_FILTER, filterDetails.SubphaseFilter);
            values.Add(FiltersValues.TEAM_MEBMBER_FILTER, filterDetails.TeamMebmberFilter);
            values.Add(FiltersValues.ADVANCED_MY_OPERATIONS_FILTER, filterDetails.AdvancedMyOperationsFilter.ToString());
            values.Add(FiltersValues.CATEGORY_FILTER, filterDetails.CategoryFilter);
            values.Add(FiltersValues.ADVANCED_COUNTRY_FILTER, filterDetails.AdvancedCountryFilter);
            values.Add(FiltersValues.ADVANCED_DIVISION_FILTER, filterDetails.AdvancedDivisionFilter);
            values.Add(FiltersValues.ADVANCED_OPERATION_TYPE_FILTER, filterDetails.AdvancedOperationTypeFilter);
            values.Add(FiltersValues.ADVANCE_ACTIVE_FILTER, filterDetails.AdvancedActiveFilter);
            values.Add(FiltersValues.ADVANCE_EXITED_FILTER, filterDetails.AdvancedExitedFilter);
            values.Add(FiltersValues.ADVANCED_OPERATION_YEAR_FILTER, filterDetails.AdvancedOperationYearFilter != null ?
                string.Join(",", filterDetails.AdvancedOperationYearFilter) : string.Empty);
            values.Add(FiltersValues.ADVANCED_OVERALL_STAGE_FILTER, filterDetails.AdvancedOverallStageFilter != null ?
                string.Join(",", filterDetails.AdvancedOverallStageFilter) : string.Empty);
            values.Add(FiltersValues.PHASE_FILTER, filterDetails.PhaseFilter);
            values.Add(FiltersValues.OPERATION_STATUS, filterDetails.OperationStatus.ToString());
            values.Add(FiltersValues.ATTRIBUTE_SEARCH1, filterDetails.AttributeSearch1);
            values.Add(FiltersValues.ATTRIBUTE_SEARCH2, filterDetails.AttributeSearch2);
            values.Add(FiltersValues.ATTRIBUTE_SEARCH3, filterDetails.AttributeSearch3);
            values.Add(FiltersValues.ATTRIBUTE_SEARCH4, filterDetails.AttributeSearch4);
            values.Add(FiltersValues.ATTRIBUTE_SEARCH5, filterDetails.AttributeSearch5);
            values.Add(
                FiltersValues.ATTRIBUTE_PPP_FILTER,
                filterDetails.PublicPrivatePartnershipAttrFilter.ToString());

            var response = _searchService.GetSearchReport(values);

            if (!response.IsValid)
            {
                return null;
            }

            var application = SearchValues.APPLICATION + SearchValues.REPORT_FORMAT;
            var fileName = SearchValues.FILENAME_REPORT;

            return File(response.File, application, fileName);
        }
        #endregion

        #region Mock Methods

        public virtual JsonResult ChangeRol(string operationNumber, ActionEnum rol)
        {
            _cacheStorageService.Remove("AuthorizationInfo");

            var auth = new AuthorizationOperationInfo()
            {
                ActionList = new List<ActionEnum>(),
                RoleList = new List<RoleEnum>()
                {
                    RoleEnum.PCRProjectTeamLeader
                },
                UserName = IDBContext.Current.UserName
            };

            auth.ActionList.Add(ActionEnum.ConvergenceReadPermission);

            if (!auth.ActionList.Any(x => x == rol))
            {
                auth.ActionList.Add(rol);
            }

            var authPermission = new AuthorizationInfo()
            {
                Authorization = new Dictionary<string, AuthorizationOperationInfo>()
            };
            authPermission.Authorization.Add(operationNumber, auth);

            _cacheStorageService.Add("AuthorizationInfo", authPermission);

            return Json(new { IsValid = true, ErrorMessage = "Permissions was changed" });
        }

        public virtual JsonResult RemovePermission()
        {
            var authorizationInfo = _cacheStorageService.Get<AuthorizationInfo>("AuthorizationInfo");

            if (authorizationInfo == null)
            {
                return Json(new { IsValid = false, ErrorMessage = "No permission data to remove." });
            }

            _cacheStorageService.Remove("AuthorizationInfo");
            authorizationInfo = _cacheStorageService.Get<AuthorizationInfo>("AuthorizationInfo");

            if (authorizationInfo == null)
            {
                return Json(new { IsValid = true, ErrorMessage = "Permission data removed correctly." });
            }

            return Json(new { IsValid = false, ErrorMessage = "Permission data could not be removed." });
        }
        #endregion

        #region Private Methods
        private void SetPermissions(
            string operationNumber, 
            string user, 
            params ActionEnum[] permissions)
        {
            var authInfo = new AuthorizationInfo()
            {
                Authorization = new Dictionary<string, AuthorizationOperationInfo>()
            };

            authInfo.Authorization.Add(operationNumber, new AuthorizationOperationInfo()
            {
                UserName = user,
                RoleList = new List<RoleEnum>() { RoleEnum.PCRProjectTeamLeader },
                ActionList = new List<ActionEnum>(permissions)
            });

            _cacheStorageService.Add("AuthorizationInfo", authInfo);
        }

        private bool SetViewBagErrorMessageInvalidResponse(ResponseBase response)
        {
            if (!response.IsValid)
            {
                var urlDecode = HttpUtility.UrlDecode(response.ErrorMessage);
                if (urlDecode != null)
                {
                    ViewBag.ErrorMessage = 
                        HttpUtility.HtmlEncode(urlDecode.Replace(Environment.NewLine, " "));
                }
            }

            return response.IsValid;
        }

        private void SetViewBagRoles(
            string operationNumber, 
            ActionEnum? readPermission = null, 
            ActionEnum? writePermission = null)
        {
            if (ViewBag.ErrorMessage == null)
            {
                if (readPermission.HasValue)
                {
                    ViewBag.ReadRole = _authorizationService.IsAuthorized(
                        IDBContext.Current.UserName,
                        operationNumber,
                        readPermission.Value);
                }

                if (writePermission.HasValue)
                {
                    ViewBag.WriteRole = _authorizationService.IsAuthorized(
                        IDBContext.Current.UserName,
                        operationNumber,
                        writePermission.Value);
                }
            }
            else
            {
                ViewBag.ReadRole = false;
                ViewBag.WriteRole = false;
            }
        }

        private CustomEnumDictionary<int> SetViewBagEnumMapping<T>()
        {
            var result = _enumMappingService.GetMappingCodeCollection<T>();

            if (ViewBag.EnumMapping == null)
            {
                ViewBag.EnumMapping = result;
                return result;
            }

            ((CustomEnumDictionary<int>)ViewBag.EnumMapping).AddRange(result);
            return ViewBag.EnumMapping;
        }

        private void SetViewBagSearch(string keyword, string operationNumber)
        {
            this.SetViewBagListFromCatalog(
                _catalogService,
                new Dictionary<ConvergenceMasterDataTypeEnum, string>
            {
                    { ConvergenceMasterDataTypeEnum.Country, "Country" },
                    { ConvergenceMasterDataTypeEnum.OperationType, "OperationType" }
                },
                hideExpired: true);

            var numberPage = AddPageSize();            

            ViewBag.NumberPageList = numberPage;

            var countryDepartments = _searchService.GetCountryDepartments();
            var countryDepartmentsList = new List<SelectListItem>();

            if (countryDepartments.IsValid)
            {
                countryDepartmentsList = countryDepartments.CountryDepartments.Select(c =>
                    new SelectListItem
                    {
                        Text = c.Code,
                        Value = c.ConvergenceMasterDataId.ToString(CultureInfo.InvariantCulture)
                    }).OrderBy(x => x.Text).ToList();
            }

            ViewBag.CountryDepartment = countryDepartmentsList;

            var divisionOpusWithExpired = _searchService.GetDivisionOpusWithExpired();
            var divisionOpusList = new List<SelectListItem>();

            if (divisionOpusWithExpired.IsValid)
            {
                divisionOpusList = divisionOpusWithExpired.DivisionOpusWithExpired.Select(c =>
                        new SelectListItem
                        {
                            Text = c.DivisionCode + " - " + c.DivisionEn,
                            Value = SqlFunctions.StringConvert((double)c.DivisionId).Trim()
                        })
                    .OrderBy(x => x.Text).ToList();
            }

            ViewBag.Division = divisionOpusList;

            var sectorOpusWithExpired = _searchService.GetSectorOpusWithExpired();
            var sectorOpusList = new List<SelectListItem>();

            if (sectorOpusWithExpired.IsValid)
            {
                string[] groupDepartments = new[]
                {
                    DivisionCode.RES,
                    DivisionCode.INE,
                    DivisionCode.SCL,
                    DivisionCode.IFD,
                    DivisionCode.CSD,
                    DivisionCode.KNL,
                    DivisionCode.INT,
                    DivisionCode.KIC
                };

                sectorOpusList = sectorOpusWithExpired.DivisionOpusWithExpired.Where(x =>
                        (groupDepartments.Contains(x.SectorDepartmentCode) ||
                            (x.IsExpired && x.DivisionCode.Equals(x.SectorDepartmentCode))) &&
                        x.SectorDepartmentId == null)
                    .Select(c =>
                        new SelectListItem
                        {
                            Text = c.SectorDepartmentCode,
                            Value = SqlFunctions.StringConvert((double)c.DivisionId).Trim()
                        })
                    .OrderBy(x => x.Text).ToList();
            }

            ViewBag.Sector = sectorOpusList.DistinctBy(exp => exp.Text).ToList();

            var trustFundResponse = _searchService.GetAllTrustFunding();
            var notifyFundCoordinatorFundList = new List<SelectListItem>();

            if (trustFundResponse.IsValid)
            {
                notifyFundCoordinatorFundList = trustFundResponse.TrustFundingViewModels.Select(c =>
                    new SelectListItem
                    {
                        Text = c.FundCodeFundName,
                        Value = c.TrustFundingId.ToString(CultureInfo.InvariantCulture)
                    }).OrderBy(x => x.Text).ToList();
            }

            ViewBag.Fund = notifyFundCoordinatorFundList;
            ViewBag.Keyword = keyword ?? string.Empty;

            var subPhaseList = new List<SelectListItem>();
            var getSubPhasesResponse = _searchService.GetSubPhases();

            if (getSubPhasesResponse.IsValid)
            {
                subPhaseList = getSubPhasesResponse.ActivityItemLists.Select(x => new SelectListItem
                {
                    Value = x.AcitivityItemCode,
                    Text = x.Name,
                }).ToList();
            }

            ViewBag.SubPhase = subPhaseList;

            var categoryList = new List<SelectListItem>();
            var getCategoryResponse = _masterDataService.GetListItemMasterDataByFilterType(SearchValues.CATEGORY);

            if (getCategoryResponse.IsValid)
            {
                categoryList = getCategoryResponse.GetListMasterDataManagement.Select(x => new SelectListItem
                {
                    Value = x.MasterDataId.ToString(),
                    Text = x.Code
                }).ToList();

                categoryList = categoryList.DistinctBy(exp => exp.Text).ToList();
            }

            ViewBag.Category = categoryList;

            var overallStageList = new List<MultiSelectListItem>();
            var listRepository = _catalogService.GetMasterDataListByTypeCode(
                hideExpired: true,
                typeCodes: ConvergenceMasterDataTypeEnum.OverallStage.GetEnumDescription());

            foreach (var item in listRepository.MasterDataCollection)
            {
                switch (item.NameEn)
                {
                    case SearchValues.OVERALLSTAGE_ORDER_1:
                        item.Order = 1;
                        break;

                    case SearchValues.OVERALLSTAGE_ORDER_3:
                        item.Order = 3;
                        break;

                    case SearchValues.OVERALLSTAGE_ORDER_4:
                        item.Order = 4;
                        break;

                    case SearchValues.OVERALLSTAGE_ORDER_5:
                        item.Order = 5;
                        break;

                    case SearchValues.OVERALLSTAGE_ORDER_6:
                        item.Order = 6;
                        break;

                    case SearchValues.OVERALLSTAGE_ORDER_7:
                        item.Order = 7;
                        break;

                    case SearchValues.OVERALLSTAGE_ORDER_8:
                        item.Order = 8;
                        break;

                    case SearchValues.OVERALLSTAGE_ORDER_9:
                        item.Order = 9;
                        break;

                    case SearchValues.OVERALLSTAGE_ORDER_10:
                        item.Order = 10;
                        break;

                    case SearchValues.OVERALLSTAGE_ORDER_11:
                        item.Order = 11;
                        break;

                    case SearchValues.OVERALLSTAGE_ORDER_12:
                        item.Order = 12;
                        break;

                    case SearchValues.OVERALLSTAGE_ORDER_13:
                        item.Order = 13;
                        break;

                    case SearchValues.OVERALLSTAGE_ORDER_14:
                        item.Order = 14;
                        break;

                    case SearchValues.OVERALLSTAGE_ORDER_15:
                        item.Order = 15;
                        break;

                    case SearchValues.OVERALLSTAGE_ORDER_16:
                        item.Order = 16;
                        break;

                    case SearchValues.OVERALLSTAGE_ORDER_17:
                        item.Order = 17;
                        break;

                    case SearchValues.OVERALLSTAGE_ORDER_18:
                        item.Order = 18;
                        break;

                    case SearchValues.OVERALLSTAGE_ORDER_19:
                        item.Order = 19;
                        break;

                    case SearchValues.OVERALLSTAGE_ORDER_20:
                        item.Order = 20;
                        break;

                    case SearchValues.OVERALLSTAGE_ORDER_21:
                        item.Order = 21;
                        break;

                    case SearchValues.OVERALLSTAGE_ORDER_22:
                        item.Order = 22;
                        break;

                    case SearchValues.OVERALLSTAGE_ORDER_23:
                        item.Order = 23;
                        break;

                    default:
                        if (item.NameEn.Contains(SearchValues.OVERALLSTAGE_ORDER_2))
                        {
                            item.Order = 2;
                        }
                        else
                        {
                            item.Order = 99;
                        }

                        break;
                }
            }

            listRepository.MasterDataCollection = listRepository.MasterDataCollection.OrderBy(q => q.Order).ToList();

            if (listRepository.IsValid && listRepository.MasterDataCollection.Any())
            {
                foreach (var overall in listRepository.MasterDataCollection)
                {
                    overallStageList.Add(new MultiSelectListItem
                    {
                        Value = overall.MasterId.ToString(),
                        Text = overall.NameEn
                    });
                }
            }

            ViewBag.OverallStage = overallStageList;

            var years = Enumerable.Range(DateTime.Now.Year - 10, 20);
            var operationYearList = years.Select(year => new MultiSelectListItem
            {
                Text = year.ToString(),
                Value = year.ToString(),
            });

            ViewBag.OperationYear = operationYearList.ToList();
            ViewBag.OperationNumber = operationNumber ?? string.Empty;
        }

        private void MockPermission()
        {
            ViewBag.WriteRole = true;
            ViewBag.ReadRole = true;
        }

        private void OpusPermission(string operationNumber)
        {
            ViewBag.Permission = true;
            ViewBag.BasicDataPermission = true;
            ViewBag.ResponsabilityDataPermission = true;
            ViewBag.RelatedOperationPermission = true;
            ViewBag.DeliverablePermission = true;
            ViewBag.ClassificationDataPermission = true;
            ViewBag.LendingProgramPermission = true;

            //Creation Permision
            ViewBag.CreationWritePermission = true;
            ViewBag.CreationRegisterAuthorizerPermission = true;
            ViewBag.CreationRequestInstitutionPermission = true;
            ViewBag.VPCManagerPermission = true;
            ViewBag.EsgSpecialistAssignmentWritePermission = true;
        }

        private List<SelectListItem> AddPageSize()
        {
            var numberPage = new List<SelectListItem>();
            for (int i = 0; i <= 3; i++)
            {
                numberPage.Add(
                    new SelectListItem()
                    {
                        Value = _pageSize[i],
                        Text = string.Format(
                            "{0} {1}",
                            Localization.GetText("OPUS.Search.PageSize"),
                            Convert.ToInt32(_pageSize[i]))
                    });
            }

            return numberPage;
        }

        #endregion
    }
}
