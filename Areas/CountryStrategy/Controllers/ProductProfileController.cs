using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.MW.Application.AttributesModule.Services;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.CountryStrategyModule.Services.ProductProfile;
using IDB.MW.Application.CountryStrategyModule.ViewModels.ProductProfile;
using IDB.MW.Application.OPUSModule.Services.ApprovalOperationService;
using IDB.MW.Application.OPUSModule.Services.ApprovalIncreasesRevampService;
using IDB.MW.Application.OPUSModule.Services.CreationFormService;
using IDB.MW.Application.OPUSModule.Services.CrossCreationOperationService;
using IDB.MW.Application.OPUSModule.Services.DeliverableService;
using IDB.MW.Application.OPUSModule.Services.DocumentService;
using IDB.MW.Application.OPUSModule.Services.FinancialDataExecutionService;
using IDB.MW.Application.OPUSModule.Services.FinancialDataPreparationService;
using IDB.MW.Application.OPUSModule.Services.OperationDataService;
using IDB.MW.Business.OPUSModule.Contracts;
using IDB.MW.Domain.Contracts.ModelRepositories.Security;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.MW.Infrastructure.Caching;
using IDB.MW.Infrastructure.Caching.Contracts;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.Presentation.MVC4.Areas.CountryStrategy.Mappers;
using IDB.Presentation.MVC4.Areas.OPUS.Models;
using IDB.Presentation.MVC4.General;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.CountryStrategy.Controllers
{
    public partial class ProductProfileController : BaseController
    {
        #region Constants
        private const string TAB_BASICDATA = "#linktabBasicData";
        private const string TAB_RESPDATA = "#linktabResponsibilityData";

        private const string CS_BASIC_DATA_URL = "/CountryStrategy/ProductProfile/BasicData";
        private const string CS_RESP_DATA_URL = "/CountryStrategy/ProductProfile/ResponsibilityData";
        private const string PAGE_CHART = "UI-CS-002-ProductProfileBasicData,UI-CS-003-ProductProfileResponsibilityData";
        #endregion

        #region Fields
        private readonly IAuthorizationService _authorizationService;
        private readonly IEnumMappingService _enumMappingService;
        private readonly ICacheStorageService _cacheService;
        private readonly ICatalogService _catalogService;
        private readonly IProductProfileService _productProfileService;
        private readonly IConvergenceRoleRepository _convergenceRoleRepository;
        private readonly IOperationDataService _operationDataService;

        private readonly IFinancialDataPreparationService _financialDataPreparationService;
        private readonly ICreationFormService _creationFormService;
        private readonly ICalculationOperationYearService _calculationOperationYearService;
        private readonly IDocumentService _documentService;
        private readonly IVerifyCountryService _verifyCountryService;
        private readonly IFinancialDataExecutionService _financialDataExecutionService;
        private readonly IFinancialDataBussinesService _getFinancingTypeListService;
        private readonly IDeliverableService _deliverableService;
        private readonly IApprovalOperationService _approvalOperationService;
        private readonly IConvergenceMasterDataRepository _convergenceMasterDataRepository;
        private readonly ICrossCreationOperationService _crossCreationOperationService;
        private readonly IAttributesEngineService _attributesEngineService;
        private readonly IRelationshipService _relationshipService;
        private readonly ISecurityModelRepository _securityModelRepository;
        private readonly IApprovalIncreasesRevampService _approvalIncreasesRevampService;

        private readonly ViewModelMapperHelper _viewModelMapperHelper;

        //TODO Merge 5.2
        //private readonly ICheckUsersOperationTeamDataService _teamService;
        #endregion

        #region Contructors

        public ProductProfileController(
            IAuthorizationService authorizationService,
            ICatalogService catalogService,
            IProductProfileService productProfileService,
            IEnumMappingService enumMappingService,
            IConvergenceRoleRepository convergenceRoleRepository,
            ICheckOperationTypeService checkOperationTypeService,
            IFinancialDataPreparationService financialDataPreparationService,
            ICreationFormService creationFormService,
            ICalculationEffortDaysService calculationEffortDaysService,
            IDocumentService documentService,
            IVerifyCountryService verifyCountryService,
            IFinancialDataExecutionService financialDataExecutionService,
            IFinancialDataBussinesService getFinancingTypeListService,
            IDeliverableService deliverableService,
            IApprovalOperationService approvalOperationService,
            IConvergenceMasterDataRepository convergenceMasterDataRepository,
            ICrossCreationOperationService crossCreationOperationService,
            IAttributesEngineService attributesEngineService,
            IRelationshipService relationshipService,
            IOperationDataService operationDataService,
            ISecurityModelRepository securityModelRepository,
            IApprovalIncreasesRevampService approvalIncreasesRevampService)
            ////TODO Merge 5.2
            ////, ICheckUsersOperationTeamDataService teamService)
            : base(authorizationService)
        {
            _authorizationService = authorizationService;
            _enumMappingService = enumMappingService;
            _catalogService = catalogService;
            _cacheService = CacheStorageFactory.Current;
            _productProfileService = productProfileService;
            _convergenceRoleRepository = convergenceRoleRepository;
            _operationDataService = operationDataService;

            _financialDataPreparationService = financialDataPreparationService;
            _creationFormService = creationFormService;
            _documentService = documentService;
            _verifyCountryService = verifyCountryService;
            _financialDataExecutionService = financialDataExecutionService;
            _getFinancingTypeListService = getFinancingTypeListService;
            _deliverableService = deliverableService;
            _approvalOperationService = approvalOperationService;
            _convergenceMasterDataRepository = convergenceMasterDataRepository;
            _crossCreationOperationService = crossCreationOperationService;
            _attributesEngineService = attributesEngineService;
            _relationshipService = relationshipService;
            _securityModelRepository = securityModelRepository;
            _approvalIncreasesRevampService = approvalIncreasesRevampService;

            ////TODO Merge 5.2
            ////_teamService = teamService;

            _viewModelMapperHelper = new ViewModelMapperHelper(ViewBag,
                _authorizationService,
                _financialDataPreparationService,
                _creationFormService,
                _operationDataService,
                _catalogService,
                _calculationOperationYearService,
                 calculationEffortDaysService,
                documentService,
                _verifyCountryService,
                _financialDataExecutionService,
                _getFinancingTypeListService,
                _deliverableService,
                _approvalOperationService,
                _convergenceMasterDataRepository,
                _crossCreationOperationService,
                _attributesEngineService,
                _relationshipService,
                _approvalIncreasesRevampService);
        }

        #endregion

        #region Action Methods

        public virtual ActionResult Read(string operationNumber, string tabName = null, string errorMessage = null)
        {
            ViewBag.ActiveTab = tabName;

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                ViewBag.ErrorMessage = errorMessage;
            }

            var model = GetProductProfileData(operationNumber, PAGE_CHART);
            LoadDelegatorMessage(operationNumber);
            return View(model);
        }

        public virtual ActionResult Cancel(string operationNumber, string tabName = null)
        {
            ViewBag.ActiveTab = tabName;

            if (tabName == TAB_BASICDATA || tabName == null)
            {
                SynchronizationHelper.TryReleaseLock(CS_BASIC_DATA_URL, operationNumber, IDBContext.Current.UserLoginName);
            }
            else
            {
                SynchronizationHelper.TryReleaseLock(CS_RESP_DATA_URL, operationNumber, IDBContext.Current.UserLoginName);
            }

            var model = GetProductProfileData(operationNumber, PAGE_CHART);
            return View("Read", model);
        }

        public virtual ActionResult EditBasicData(string operationNumber)
        {
            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, CS_BASIC_DATA_URL, operationNumber, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return RedirectToAction("Read", new { tabName = TAB_BASICDATA, errorMessage = errorMessage });
            }

            ViewBag.ActiveTab = TAB_BASICDATA;
            var model = GetProductProfileData(operationNumber, PAGE_CHART);
            ViewBag.SerializedBasicDataViewModel = PageSerializationHelper.SerializeObject(model.BasicData);
            SetViewBagEditModel(operationNumber);
            return View("Edit", model);
        }

        public virtual ActionResult EditResponsabilityData(string operationNumber)
        {
            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, CS_RESP_DATA_URL, operationNumber, IDBContext.Current.UserLoginName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return RedirectToAction("Read", new { tabName = TAB_RESPDATA, errorMessage = errorMessage });
            }

            ViewBag.ActiveTab = TAB_RESPDATA;
            var model = GetProductProfileData(operationNumber, PAGE_CHART);
            ViewBag.SerializedResponsibilityDataViewModel = PageSerializationHelper.SerializeObject(model.ResponsibilityData);
            SetViewBagEditResponsabilityData(operationNumber, model.ResponsibilityData);
            return View("Edit", model);
        }

        public virtual JsonResult GetUsersList(string operationNumber, string filter)
        {
            var response = _productProfileService.SearchUser(operationNumber, filter);

            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual JsonResult SaveBasicData(string operationNumber)
        {
            var responseView = new SaveResponse() { IsValid = true };

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var model = PageSerializationHelper.DeserializeObject<BasicDataViewModel>(jsonDataRequest.SerializedData);

            model.UpdateBasicDataViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, CS_BASIC_DATA_URL, operationNumber, userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
            }
            else
            {
                var response = _productProfileService.SaveBasicData(operationNumber, model);
                responseView.IsValid = response.IsValid;
                responseView.ErrorMessage = response.ErrorMessage;
            }

            if (responseView.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(CS_BASIC_DATA_URL, operationNumber, userName);
                var url = Url.Action("Read", "ProductProfile", new { area = "CountryStrategy", tabName = TAB_BASICDATA });
                responseView.UrlRedirect = url;
            }

            return Json(responseView);
        }

        public virtual JsonResult SaveResponsibilityData(string operationNumber)
        {
            var responseView = new SaveResponse() { IsValid = true };

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var model = PageSerializationHelper.DeserializeObject<ResponsibilityDataViewModel>(jsonDataRequest.SerializedData);

            model.UpdateProductDataResponsibilityData(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(SynchronizationHelper.EDIT_MODE, CS_RESP_DATA_URL, operationNumber, userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                responseView.IsValid = false;
                responseView.ErrorMessage = errorMessage;
            }
            else
            {
                var response = _productProfileService.SaveResponsibilityData(operationNumber, model);
                responseView.IsValid = response.IsValid;
                responseView.ErrorMessage = response.ErrorMessage;
            }

            if (responseView.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(CS_RESP_DATA_URL, operationNumber, userName);
                var url = Url.Action("Read", "ProductProfile", new { area = "CountryStrategy", tabName = TAB_RESPDATA });
                responseView.UrlRedirect = url;
            }

            return Json(responseView);
        }

        public virtual FileResult ExportTeamDataToExcel(string operationNumber)
        {
            var response = _productProfileService.GetOperationTeamReport(operationNumber);

            if (!response.IsValid)
            {
                return null;
            }

            return File(response.File, "application/doc", "TeamData.xls");
        }
        #endregion

        #region Private Methods

        private ProductDataViewModel GetProductProfileData(string operationNumber, string pageChart = null)
        {
            var response = _productProfileService.GetProductData(operationNumber);
            SetViewBagErrorMessageInvalidResponse(response);
            response.ProductData.FieldAccessList = _securityModelRepository.GetFieldBehaviorByPermissions(
                IDBContext.Current.Operation,
                pageChart,
                IDBContext.Current.Permissions,
                0,
                0).ToList();
            return response.ProductData;
        }

        private void SetViewBagEditResponsabilityData(string operationNumber, ResponsibilityDataViewModel model)
        {
            SetViewBagEditModel(operationNumber);

            ViewBag.ReponseRoleId = _catalogService.GetConvergenceMasterDataIdByCode(OrgUnitRoleCode.ORG_RESPONSIBLE, MasterType.ORG_UNIT_ROLE);
            ViewBag.ResponsibleUnitsRoleList = _catalogService.GetListMasterData(MasterType.ORG_UNIT_ROLE);
            ViewBag.ResponsibleUnitsOrganizationalUnitList = _viewModelMapperHelper.GetDivisionList();
        }

        private void SetViewBagEditModel(string operationNumber)
        {
            var includeValues = new List<string> { MemberRoleCode.FIDUCIARY_SPECIALIST, MemberRoleCode.PROCUREMENT_FIDUCIARY_SPECIALIST, MemberRoleCode.TEAM_LEADER, MemberRoleCode.VPC_COUNTRY_ECONOMIST, MemberRoleCode.VPC_TEAM_MEMBER };
            ViewBag.TeamLeaderRoleId = _catalogService.GetConvergenceMasterDataIdByCode(MemberRoleCode.TEAM_LEADER, MasterType.MEMBER_ROLE);
            ViewBag.OperationTeamsRoleList = _catalogService.GetListMasterData(MasterType.MEMBER_ROLE, includeByCode: includeValues);
        }

        private void LoadDelegatorMessage(string operationNumber)
        {
            var roleListModal = new List<string>();
            string UserName = IDBContext.Current.UserName;

            ViewBag.OperationNumber = operationNumber;
            ViewBag.DelegatorMessage = _securityModelRepository.GetDelegatorMessages(roleListModal, operationNumber, UserName);

            if (ViewBag.DelegatorMessage)
            {
                IDBContext.Current.ExpireAndReloadSecurityForOperation(operationNumber);
            }
        }
        #endregion
    }
}