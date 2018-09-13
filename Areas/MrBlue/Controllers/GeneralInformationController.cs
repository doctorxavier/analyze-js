using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.Architecture.Security.Models.UserIdentity;
using IDB.MW.Application.AdministrationModule.Services.SearchService;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Application.MrBlueModule.Enums;
using IDB.MW.Application.MrBlueModule.Messages.GeneralInformation;
using IDB.MW.Application.MrBlueModule.Services.GeneralInformation;
using IDB.MW.Application.MrBlueModule.Services.Keywords;
using IDB.MW.Application.MrBlueModule.ViewModels.GeneralInformation;
using IDB.MW.Application.TCAbstractModule.Enums;
using IDB.MW.Business.Core.SharepointSecurityService;
using IDB.MW.Domain.Contracts.DomainServices;
using IDB.MW.Domain.Session;
using IDB.MW.Infrastructure.Caching.Contracts;
using IDB.MW.Infrastructure.Configuration;
using IDB.MW.Infrastructure.Helpers;
using IDB.MW.Infrastructure.SecurityService;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.MW.Infrastructure.SecurityService.Enums;
using IDB.Presentation.MVC4.Areas.MrBlue.Models;
using IDB.Presentation.MVC4.General;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.MrBlue.Controllers
{
    public partial class GeneralInformationController : BaseController
    {
        #region Constants
        private const string NO_WRITE_PERMISSION = "TC.TCAbstractService.NoWritePermission";
        private const string URL_GENERAL_INFORMATION = "/MrBlue/GeneralInformation";
        #endregion

        #region Fields
        private readonly IGeneralInformationService _generalInformationService;
        private readonly ISearchService _searchService;
        private readonly ICatalogService _catalogService;
        private readonly IEnumMappingService _enumMappingService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IKeywordService _keywordService;
        private readonly ICacheStorageService _cacheStorageService;
        #endregion

        #region Contructors
        public GeneralInformationController(
            IGeneralInformationService generalInformationService,
            ISearchService searchService,
            ICatalogService catalogService,
            IAuthorizationService authorizationService,
            IEnumMappingService enumMappingService,
            IKeywordService keywordService,
            ICacheStorageService cacheStorageService)
            : base(authorizationService, enumMappingService)
        {
            _authorizationService = AuthorizationServiceFactory.Current;
            _searchService = searchService;
            _generalInformationService = generalInformationService;
            _catalogService = catalogService;
            _enumMappingService = enumMappingService;
            _keywordService = keywordService;
            _cacheStorageService = cacheStorageService;
        }
        #endregion

        #region General Information Members

        #region View

        public virtual ActionResult GeneralInformation(string operationNumber)
        {
            var model = GetGeneralInformation(operationNumber);

            SetViewBagRoles(operationNumber);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            model.Members = GetMemberForEsgGroup(model.EsgGroupsConfiguration);

            return View(model);
        }

        public virtual ActionResult GeneralInformationEdit(string operationNumber, string accessType)
        {
            SetViewBagRoles(operationNumber);

            if (!ViewBag.WriteRole)
            {
                return Json(new { ErrorMessage = Localization.GetText(NO_WRITE_PERMISSION) });
            }

            var errorMessage = SynchronizationHelper.AccessToResources(accessType, URL_GENERAL_INFORMATION, operationNumber, IDBContext.Current.UserLoginName);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return Json(new { ErrorMessage = errorMessage });
            }

            var model = GetGeneralInformation(operationNumber);

            if (!string.IsNullOrWhiteSpace(ViewBag.ErrorMessage))
            {
                return Json(new { ErrorMessage = ViewBag.ErrorMessage });
            }

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);

            model.Members = GetMemberForEsgGroup(model.EsgGroupsConfiguration);

            return PartialView("Partials/GeneralInformationPartial", model);
        }

        public virtual JsonResult FindUserAD(string filter)
        {
            var adMode = ConfigurationServiceFactory.Current.GetApplicationSettings().IsADMode;
            FindUserADResponse response = new FindUserADResponse();

            if (adMode)
            {
                response = _generalInformationService.FindUserAD(filter, IDBContext.Current.Operation);
            }
            else
            {
                response.ListResponse = usersMock();
                response.IsValid = true;
                response.ErrorMessage = string.Empty;
            }

            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual JsonResult LoadFundByFinancingType(string operationNumber, int financingType)
        {
            var response = _generalInformationService.GetFundsByFinancingType(operationNumber, financingType);
            var responseHtml = new SaveResponse()
            {
                IsValid = response.IsValid,
                ErrorMessage = response.ErrorMessage,
                ContentHTML = string.Empty
            };

            if (responseHtml.IsValid)
            {
                responseHtml.ContentHTML = convertToHtml(response);
            }

            return Json(responseHtml);
        }

        public string convertToHtml(GetFundsByFinancingTypeResponse response)
        {
            var container = new TagBuilder("div");
            container.AddCssClass("dropdown");
            container.MergeAttribute("style", "width: 100%;max-width: 170px;", true);

            var button = new TagBuilder("button");
            button.AddCssClass("btn btn-default dropdown-toggle");
            button.MergeAttribute("style", "width: 100%;", true);
            button.MergeAttribute("type", "button", true);
            button.MergeAttribute("id", "id-FundId", true);
            button.MergeAttribute("data-toggle", "dropdown", true);
            button.MergeAttribute("aria-haspopup", "true", true);
            button.MergeAttribute("aria-expanded", "true", true);
            button.MergeAttribute("onclick", "cargaDropDown(this)", true);
            button.MergeAttribute("onkeydown", "dropdownNavigation(this, event)", true);
            button.MergeAttribute("dd-find", string.Empty, true);

            var spanButton1 = new TagBuilder("span");
            spanButton1.AddCssClass("valueText");
            spanButton1.InnerHtml = "(Select an Option)";

            var spanButton2 = new TagBuilder("span");
            spanButton2.AddCssClass("caret");

            button.InnerHtml = spanButton1.ToString() + spanButton2.ToString();

            var inputHiddenDropdown = new TagBuilder("input");
            inputHiddenDropdown.AddCssClass("hide");
            inputHiddenDropdown.MergeAttribute("type", "text", true);
            inputHiddenDropdown.MergeAttribute("name", "FundId", true);
            inputHiddenDropdown.MergeAttribute("value", string.Empty, true);
            inputHiddenDropdown.MergeAttribute("data-parsley-required", "true", true);
            inputHiddenDropdown.MergeAttribute("data-force-parsley-validation", "true", true);

            var ulList = new TagBuilder("ul");
            ulList.AddCssClass("dropdown-menu");
            ulList.MergeAttribute("aria-labelledby", "id-FundId", true);
            ulList.MergeAttribute("style", "min-width: 161px;", true);

            var liList = new StringBuilder();

            var liEmpty = new TagBuilder("li");
            var aEmpty = new TagBuilder("a");
            aEmpty.MergeAttribute("dd-value", string.Empty, true);
            aEmpty.MergeAttribute("dd-selected", string.Empty, true);
            aEmpty.AddCssClass("placeholder");

            aEmpty.InnerHtml = "(Select an Option)";
            liEmpty.InnerHtml = aEmpty.ToString();

            liList.Append(liEmpty.ToString());

            foreach (var item in response.ListResponse)
            {
                var liAux = new TagBuilder("li");
                var aAux = new TagBuilder("a");
                aAux.MergeAttribute("dd-value", item.FundId.ToString(), true);
                aAux.MergeAttribute("dd-parent-id", string.Empty, true);
                aAux.MergeAttribute("data-fundAmount", item.FundAmount, true);

                aAux.InnerHtml = item.FundName;

                liAux.InnerHtml = aAux.ToString();
                liList.Append(liAux.ToString());
            }

            ulList.InnerHtml = liList.ToString();

            container.InnerHtml = button.ToString() + inputHiddenDropdown.ToString() + ulList.ToString();

            return container.ToString();
        }

        public virtual JsonResult LoadFundAmountByFund(string operationNumber, int fundId)
        {
            var response = _generalInformationService.GetFundAmountByOperation(operationNumber, fundId);

            return Json(new { IsValid = response.IsValid, ErrorMessage = response.ErrorMessage, Data = response });
        }

        public virtual JsonResult SearchSubphaseComponents(string parentId)
        {
            List<SelectListItem> componentList = null;
            var response = _catalogService.GetMasterDataListByTypeCode(false, ConvergenceMasterDataTypeEnum.Component.GetEnumDescription());
            if (response.IsValid)
            {
                var components = response.MasterDataCollection;
                int id;
                if (int.TryParse(parentId, out id))
                {
                    components = components.Where(x => x.ParentMasterId == id).ToList();
                }

                componentList = ControllerHelper.ConvertToSelectListItems(components);
            }

            return Json(componentList, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Save

        public virtual JsonResult GeneralInformationlSave(string operationNumber)
        {
            SaveGeneralInformationResponse response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<GeneralInformationViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateGeneralInformationlDataViewModel(jsonDataRequest.ClientFieldData, _enumMappingService);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources("edit", URL_GENERAL_INFORMATION, operationNumber, userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new SaveGeneralInformationResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _generalInformationService.SaveGeneralInformation(operationNumber, viewModel);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(URL_GENERAL_INFORMATION, operationNumber, userName);
                }
            }

            return Json(response);
        }

        #endregion

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

        private GeneralInformationViewModel GetGeneralInformation(string operationNumber)
        {
            var response = _generalInformationService.GetGeneralInformation(operationNumber);

            var viewModel = response.GeneralInformation;

            SetViewBagErrorMessageInvalidResponse(response);
            SetViewBagGeneralInformation(operationNumber);
            SetViewBagGeneralInformationRolesPermission(operationNumber);

            ViewBag.IsESGSpeacialistGlobal = IDBContext.Current.HasRole(Role.ESG_ADMIN) ||
                                             IDBContext.Current.HasRole(Role.ESG_SPECIALIST_GLOBAL);

            return viewModel;
        }

        private void SetViewBagGeneralInformation(string operationNumber)
        {
            this.SetViewBagListFromCatalog(_catalogService, new Dictionary<ConvergenceMasterDataTypeEnum, string>
            {
                { ConvergenceMasterDataTypeEnum.EsgClassification, "ImpactCategoryList" },
                { ConvergenceMasterDataTypeEnum.FinancingType, "FinancingTypeList" },
                { ConvergenceMasterDataTypeEnum.SustainabilityType, "SustainabilityTypeList" },
                { ConvergenceMasterDataTypeEnum.Component, "ComponentList" },
                { ConvergenceMasterDataTypeEnum.SafeguardPerfomance, "SafeguardPerfomanceList" },
                { ConvergenceMasterDataTypeEnum.HighRisk, "HighRiskList" },
                { ConvergenceMasterDataTypeEnum.ESGDocumentType, "DocumentTypeList" }
            });

            this.SetViewBagListMemberRoleFromCatalog(_catalogService, new Dictionary<ConvergenceMasterDataTypeEnum, string>
            {
                { ConvergenceMasterDataTypeEnum.MemberRole, "RoleAllList" },
            });

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

            var responseDictionaryFunds = _generalInformationService.GetDictionaryFundsByFinancingType(operationNumber, (List<SelectListItem>)ViewBag.FinancingTypeList);

            ViewBag.DisctionaryFunds = responseDictionaryFunds.DictionaryResponse;

            ViewBag.CurrentUser = IDBContext.Current.UserName;
            ViewBag.FundList = notifyFundCoordinatorFundList;
            SetViewBagEnumMapping<ESGFinancingTypeEnum>();
            SetViewBagEnumMapping<ESGSustainabilityTypeEnum>();
            SetViewBagEnumMapping<ESGRoleEnum>();
            SetViewBagEnumMapping<ESGDocumentTypeEnum>();
        }

        private void SetViewBagGeneralInformationRolesPermission(string operationNumber)
        {
            this.SetViewBagListMemberRoleFromCatalog(_catalogService, new Dictionary<ConvergenceMasterDataTypeEnum, string>
            {
                { ConvergenceMasterDataTypeEnum.MemberRole, "RoleByPermissionList" },
            });

            ViewBag.HasCreateNewPermission = IDBContext.Current.HasRole(Role.ESG_CHIEF)
                || IDBContext.Current.HasRole(Role.ESG_OPERATIONAL_GROUP_LEADER)
                || IDBContext.Current.HasRole(Role.ESG_ADMIN)
                || IDBContext.Current.HasRole(Role.ESG_PRIMARY_TEAM_MEMBER);

            var ModifyESGLead = IDBContext.Current.HasRole(Role.ESG_ADMIN)
             || IDBContext.Current.HasRole(Role.ESG_OPERATIONAL_GROUP_LEADER)
             || IDBContext.Current.HasRole(Role.ESG_CHIEF);

            var ModifyESGSpecialist = IDBContext.Current.HasRole(Role.ESG_ADMIN)
              || IDBContext.Current.HasRole(Role.ESG_OPERATIONAL_GROUP_LEADER)
              || IDBContext.Current.HasRole(Role.ESG_CHIEF)
              || IDBContext.Current.HasRole(Role.ESG_PRIMARY_TEAM_MEMBER);

            //List Items
            var listItemPrimaryTeamMembers = ((List<SelectListItem>)ViewBag.RoleByPermissionList).Single(x => x.Value == _enumMappingService.GetMappingCode(ESGRoleEnum.PrimaryTeamMember).ToString());
            var listItemReviewer = ((List<SelectListItem>)ViewBag.RoleByPermissionList).Single(x => x.Value == _enumMappingService.GetMappingCode(ESGRoleEnum.Reviewer).ToString());
            var listItemTeamMembers = ((List<SelectListItem>)ViewBag.RoleByPermissionList).Single(x => x.Value == _enumMappingService.GetMappingCode(ESGRoleEnum.TeamMember).ToString());

            if (!ModifyESGLead)
            {
                ((List<SelectListItem>)ViewBag.RoleByPermissionList).Remove(listItemPrimaryTeamMembers);
            }

            if (!ModifyESGSpecialist)
            {
                ((List<SelectListItem>)ViewBag.RoleByPermissionList).Remove(listItemTeamMembers);
            }

            ((List<SelectListItem>)ViewBag.RoleByPermissionList).Remove(listItemReviewer);
        }

        private List<ListItemViewModel> usersMock()
        {
            var response = new List<ListItemViewModel>();

            response.Add(
                new ListItemViewModel()
                {
                    Value = "1",
                    Text = "texto 1"
                });
            response.Add(
                new ListItemViewModel()
                {
                    Value = "2",
                    Text = "texto 2"
                });
            response.Add(
                new ListItemViewModel()
                {
                    Value = "3",
                    Text = "texto 3"
                });
            response.Add(
                new ListItemViewModel()
                {
                    Value = "4",
                    Text = "texto 4"
                });

            return response;
        }

        private void MockPermission()
        {
            ViewBag.WriteRole = true;
            ViewBag.ReadRole = true;
        }

        IList<string> GetMemberForEsgGroup(IList<EsgGroupsConfigurationViewModel> EsgGroupsConfiguration)
        {
            if (EsgGroupsConfiguration.Any())
            {
                List<string> listMembers = new List<string>();

                foreach (var esgGroup in EsgGroupsConfiguration)
                {
                    listMembers.Add(esgGroup.FullName);
                }

                return listMembers;
            }

            return null;
        }

        #endregion
    }
}