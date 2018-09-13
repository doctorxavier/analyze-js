using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web.Caching;
using System.Web.Mvc;

using IDB.Architecture.Cache;
using IDB.Architecture.Logging;
using IDB.MVCControls.General.Helpers;
using IDB.MW.Application.AdministrationModule.Services.InstitutionService;
using IDB.MW.Application.AttributesModule.Services;
using IDB.MW.Application.AttributesModule.ViewModels.AttributesService;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.DEMModule.Services;
using IDB.MW.Application.DEMModule.ViewModels;
using IDB.MW.Application.GlobalModule.Workflow;
using IDB.MW.Application.OPUSModule.Email;
using IDB.MW.Application.OPUSModule.Enums.K2;
using IDB.MW.Application.OPUSModule.K2Integration;
using IDB.MW.Application.OPUSModule.Messages.ApprovalIncreasesRevampService;
using IDB.MW.Application.OPUSModule.Messages.ApprovalOperationService;
using IDB.MW.Application.OPUSModule.Messages.CreationFormService;
using IDB.MW.Application.OPUSModule.Messages.DeliverableService;
using IDB.MW.Application.OPUSModule.Messages.EnvironmentalSocialDataService;
using IDB.MW.Application.OPUSModule.Messages.FinancialDataExecutionService;
using IDB.MW.Application.OPUSModule.Messages.FinancialDataPreparationService;
using IDB.MW.Application.OPUSModule.Messages.OperationDataService;
using IDB.MW.Application.OPUSModule.Services.ApprovalIncreasesRevampService;
using IDB.MW.Application.OPUSModule.Services.ApprovalOperationService;
using IDB.MW.Application.OPUSModule.Services.CofinancingService;
using IDB.MW.Application.OPUSModule.Services.CreationFormService;
using IDB.MW.Application.OPUSModule.Services.DeliverableService;
using IDB.MW.Application.OPUSModule.Services.EnvironmentalSocialDataService;
using IDB.MW.Application.OPUSModule.Services.FinancialDataExecutionService;
using IDB.MW.Application.OPUSModule.Services.FinancialDataPreparationService;
using IDB.MW.Application.OPUSModule.Services.OperationDataService;
using IDB.MW.Application.OPUSModule.ViewModels.ApprovalOperationService;
using IDB.MW.Application.OPUSModule.ViewModels.Common;
using IDB.MW.Application.OPUSModule.ViewModels.CreationFormService;
using IDB.MW.Application.OPUSModule.ViewModels.DeliverableService;
using IDB.MW.Application.OPUSModule.ViewModels.EnvironmentalSocialDataService;
using IDB.MW.Application.OPUSModule.ViewModels.FinancialDataExecutionService;
using IDB.MW.Application.OPUSModule.ViewModels.FinancialDataPreparationService;
using IDB.MW.Application.OPUSModule.ViewModels.Institutions;
using IDB.MW.Application.OPUSModule.ViewModels.OperationDataService;
using IDB.MW.Domain.EntityHelpers;
using IDB.MW.Domain.Models.Architecture.Enumerations;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.Domain.Values.Dem;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.MW.DomainModel.Contracts.Repositories.OPUSModule;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.MW.Infrastructure.SecurityService;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.Presentation.MVC4.Areas.Global.Controllers;
using IDB.Presentation.MVC4.Areas.OPUS.Models;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVC4.Models.ClientFieldData;
using IDB.Presentation.MVC4.Services.Core.DynamicFormsHelper;
using BusinessDem = IDB.MW.Business.DEMModule.Contracts;

namespace IDB.Presentation.MVC4.Areas.OPUS.Controllers
{
    public partial class SaveController : BaseController
    {
        #region Constants
        private const string URL_ENVIRONMENTAL_SOCIAL_DATA = "/OPUS/EnvironmentalSocialData";
        private const string URL_APPROVAL_OPERATION = "/OPUS/ApprovalOperation";
        private const string APPROVAL_NUMBER = "approvalNumber";
        private const string FINANCING_TYPE = "financingType";
        private const string FUND = "fund";
        private const string INCREASE_AMOUNT = "increaseAmount";
        private const string US_EQUIVALENT = "increaseUsAmount";
        private const string EFFECTIVE_DATE = "effectiveDate";
        private const string ROW_ID = "rowid";
        private const string FUND_CURRENCY = "fundCurrency";
        private const string CHANGE_STATUS = "changeStatus";
        private const string INSTITUTION_ID = "Institution_ID";
        private const string APPROVE_WORKFLOW = "ApproveWorkflow";
        private const string REJECT_WORKFLOW = "RejectWorkflow";
        private const string DATE_FORMAT = "dd MMM yyyy";
        private const string EDIT = "edit";
        private const string ZERO = "0";
        #endregion

        #region Fields
        private readonly IAuthorizationService _authorizationService;
        private readonly IEnvironmentalSocialDataService _environmentalDataService;
        private readonly IFinancialDataPreparationService _financialDataPreparationService;
        private readonly IFinancialDataExecutionService _financialDataExecutionService;
        private readonly IEnumMappingService _enumMappingService;
        private readonly ICreationFormService _creationFormService;
        private readonly IOperationDataService _operationDataService;
        private readonly IApprovalOperationService _approvalOperationService;
        private readonly IInstitutionService _institutionService;
        private readonly IDeliverableService _deliverableService;
        private readonly IConvergenceMasterDataRepository _masterDataRepository;
        private readonly IOperationTeamDataRepository _operationTeamDataRepository;
        private readonly IOperationRepository _operationRepository;
        private readonly ICatalogService _catalogService;
        private readonly IDivisionOpusRepository _divisionRepository;
        private readonly IDEMService _demService;
        private readonly IApprovalIncreasesRevampService _approvalIncreasesRevampService;
        private readonly BusinessDem.IDEMService _businessDemService;
        private readonly IOpusEmailManager _opusEmailManager;
        private readonly IAttributesEngineService _attributesEngineService;
        private IK2IntegrationWorkflow _k2IntegrationWorkflow;
        private IK2IntegrationOpus _k2IntegrationOpus;
        private ICacheManagement _cacheData = null;

        private string _impactsIndicatorCacheName = string.Empty;
        private string _outputsPhysicalCacheName = string.Empty;
        private string _outputsFinancialCacheName = string.Empty;
        private string _outcomesCacheName = string.Empty;
        private ICofinancingService _cofinancingService;
        #endregion

        #region Constructor
        public SaveController(IEnvironmentalSocialDataService environmentalDataService,
            IFinancialDataPreparationService financialDataPreparationService,
            IEnumMappingService enumMappingService,
            ICreationFormService creationFormService,
            IOperationDataService operationDataService,
            IApprovalOperationService approvalOperationService,
            IFinancialDataExecutionService financialDataExecutionService,
            IInstitutionService institutionService,
            IK2IntegrationOpus k2IntegrationOpus,
            IDeliverableService deliverableService,
            IConvergenceMasterDataRepository masterDataRepository,
            IK2IntegrationWorkflow k2IntegrationWorkflow,
            IOperationTeamDataRepository operationTeamDataRepository,
            IOperationRepository operationRepository,
            ICatalogService catalogService,
            IDivisionOpusRepository divisionRepository,
            IDEMService demService,
            IApprovalIncreasesRevampService approvalIncreasesRevampService,
            ICacheManagement cacheData,
            BusinessDem.IDEMService businessDemService,
            ICofinancingService cofinancingService,
            IOpusEmailManager opusEmailManager,
            IAttributesEngineService attributesEngineService)
        {
            _authorizationService = AuthorizationServiceFactory.Current;
            _environmentalDataService = environmentalDataService;
            _creationFormService = creationFormService;
            _enumMappingService = enumMappingService;
            _financialDataPreparationService = financialDataPreparationService;
            _financialDataExecutionService = financialDataExecutionService;
            _operationDataService = operationDataService;
            _approvalOperationService = approvalOperationService;
            _k2IntegrationOpus = k2IntegrationOpus;
            _institutionService = institutionService;
            _deliverableService = deliverableService;
            _masterDataRepository = masterDataRepository;
            _k2IntegrationWorkflow = k2IntegrationWorkflow;
            _operationTeamDataRepository = operationTeamDataRepository;
            _operationRepository = operationRepository;
            _catalogService = catalogService;
            _divisionRepository = divisionRepository;
            _demService = demService;
            _approvalIncreasesRevampService = approvalIncreasesRevampService;
            _cacheData = cacheData;
            _businessDemService = businessDemService;
            _impactsIndicatorCacheName = string.Format(
               CacheNames.IMPACTS, IDBContext.Current.Operation);
            _outputsPhysicalCacheName = string.Format(
               CacheNames.OUTPUTS_PHYSICAL, IDBContext.Current.Operation);
            _outputsFinancialCacheName = string.Format(
               CacheNames.OUTPUTS_FINANCIAL, IDBContext.Current.Operation);
            _outcomesCacheName = string.Format(CacheNames.OUTCOMES,
               IDBContext.Current.Operation);
            _cofinancingService = cofinancingService;
            _opusEmailManager = opusEmailManager;
            _attributesEngineService = attributesEngineService;
        }
        #endregion

        #region SaveActions

        #region FinancialDataExecution
        public virtual JsonResult RegisterIncreases(string operationNumber, FormCollection formData)
        {
            var model = new IncreaseRowViewModel();

            model.ApprovalNumber = formData.Get(APPROVAL_NUMBER);
            model.FinancingType = formData.Get(FINANCING_TYPE);
            model.Fund = formData.Get(FUND);
            model.FundCurrency = formData.Get(FUND_CURRENCY);
            model.IncreaseAmount = decimal.Parse(formData.Get(INCREASE_AMOUNT));
            model.USEquivalent = decimal.Parse(formData.Get(US_EQUIVALENT));
            int idIncrease;
            model.EffectiveDate = DateTime.ParseExact(formData.Get(EFFECTIVE_DATE),
                DATE_FORMAT,
                CultureInfo.InvariantCulture);
            model.RowId = int.TryParse(formData.Get(ROW_ID), out idIncrease) ? idIncrease : 0;
            SaveFinancialDataExecutionResponse response = _financialDataExecutionService
                .SaveFinancialDataExecutionRegisterIncreases(operationNumber, model);
            response.rowID = formData.Get(ROW_ID);
            return Json(response);
        }

        public virtual JsonResult FinancialDataExecution(string operationNumber)
        {
            SaveFinancialDataExecutionResponse response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<FinancialDataViewModel>(jsonDataRequest.SerializedData);

            viewModel.FundIncreases
                .UpdateFinancialDataExecutionViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(EDIT,
                OPUSGlobalValues.URL_FINANCIAL_DATA_EXECUTION,
                operationNumber,
                userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new SaveFinancialDataExecutionResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _financialDataExecutionService
                    .SaveFinancialDataExecutionIncreases(operationNumber, viewModel.FundIncreases);

                GlobalCommonLogic.SetLastOperation(operationNumber);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(
                        OPUSGlobalValues.URL_FINANCIAL_DATA_EXECUTION, operationNumber, userName);
                }
            }

            return Json(response);
        }

        public virtual JsonResult RevokeIncreases(string operationNumber, FormCollection formData)
        {
            var model = new IncreaseRowViewModel();

            model.ApprovalNumber = formData.Get(APPROVAL_NUMBER);
            model.RowId = int.Parse(formData.Get(ROW_ID));
            SaveFinancialDataExecutionResponse response = _financialDataExecutionService
                .RevokeIncreases(operationNumber, model);
            response.rowID = formData.Get(ROW_ID);
            return Json(response);
        }

        #endregion

        #region ApprovalOperation
        public virtual JsonResult ApprovalOperation(string operationNumber)
        {
            SaveApprovalOperationResponse response;

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<ApprovalOperationViewModel>(jsonDataRequest.SerializedData);

            var userName = IDBContext.Current.UserName;

            viewModel.UpdateApprovalOperationViewModel(jsonDataRequest.ClientFieldData);

            viewModel.AttributesApproval
                .UpdateAttributesViewModel(jsonDataRequest.ClientFieldData);

            response = _approvalOperationService
                .SaveApprovalOperations(operationNumber, viewModel);

            GlobalCommonLogic.SetLastOperation(operationNumber);

            if (response.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(
                    URL_APPROVAL_OPERATION,
                    operationNumber,
                    userName);
            }

            return Json(response);
        }

        public virtual JsonResult DeleteComment(int commentId)
        {
            var deleteComent = _approvalOperationService.DeleteComment(commentId);
            return Json(deleteComent, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult DeleteDocument(string operationNumber,
            string documentNumber,
            bool isIncrease)
        {
            var request = new SingleDocumentApprovalRequest
            {
                DocumentReference = documentNumber,
                IsIncrease = isIncrease
            };
            var response = _approvalOperationService.DeleteDocument(request);

            return Json(response);
        }

        public virtual JsonResult AddRemarks(string operationNumber,
            List<GenericCommentsViewModel> comment,
            List<int> deleteComment,
            List<GenericCommentsViewModel> updateComment)
        {
            var request = new ApprovalOperationRemarkRequest
            {
                OperationNumber = operationNumber,
                Comments = comment,
                DeleteComments = deleteComment,
                UpdateComments = updateComment
            };

            var response = _approvalOperationService.ApprovalOperationRemark(request);
            return Json(response);
        }

        public virtual JsonResult SaveDocuments(string operationNumber,
            List<ApprovalDocumentDetailViewModel> newDocumentList,
            List<ApprovalDocumentDetailViewModel> updateDocumentList,
            string documentDate,
            string documentNumber)
        {
            var request = new ApprovalOperationDocumentRequest
            {
                OperationNumber = operationNumber,
                NewDocumentList = newDocumentList,
                UpdateDocumentList = updateDocumentList,
                DocumentDate = documentDate,
                DocumentNumber = documentNumber
            };

            var response = _approvalOperationService.SaveApprovalDocument(request);
            return Json(response);
        }

        public virtual JsonResult SubmitTransaction(string operationNumber)
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);

            var request = jsonDataRequest.ClientFieldData
                .ConvertToTransactionRequest(operationNumber);

            ResponseBase response;

            if (request.IsAutomatic)
                response = _approvalIncreasesRevampService.AutomaticTransaction(request);
            else
                response = _approvalIncreasesRevampService.SendTransaction(request);

            return new JsonResult
            {
                Data = new
                {
                    IsValid = response.IsValid,
                    ErrorMessage = response.ErrorMessage
                },
            };
        }

        public virtual JsonResult DeleteIncrease(
            string operationNumber, 
            int fundOperationIncreaseId)
        {
            var response = _approvalOperationService
                .DeleteIncrease(operationNumber, fundOperationIncreaseId);

            return Json(response);
        }

        #endregion ApprovalOperation

        #region FinancialDataPreparation
        public virtual JsonResult FinancialDataPreparation(string operationNumber)
        {
            SaveFinancialDataPreparation response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<FinancialDataPreparationViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateFinancialDataPreparationViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(EDIT,
                OPUSGlobalValues.URL_FINANCIAL_DATA_PREPARATION,
                operationNumber,
                userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new SaveFinancialDataPreparation
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _financialDataPreparationService
                    .SaveFinancialDataPreparation(operationNumber, viewModel);

                GlobalCommonLogic.SetLastOperation(operationNumber);

                if (response.IsValid)
                {
                    SynchronizationHelper
                        .TryReleaseLock(OPUSGlobalValues.URL_FINANCIAL_DATA_PREPARATION,
                            operationNumber,
                            userName);
                }

                var saveCounterpartDetailResponse = _cofinancingService.SaveCounterpartDetail(
                   viewModel.CounterpartFinancing.CounterpartFinancingDetail,
                   viewModel.ActivityPlanId.Value);

                if (!saveCounterpartDetailResponse.IsValid)
                {
                    response.IsValid = saveCounterpartDetailResponse.IsValid;
                    response.ErrorMessage = saveCounterpartDetailResponse.ErrorMessage;

                    return Json(response);
                }

                if (viewModel.CoFinancing.Id == null)
                {
                    var getCofinancingIdResponse = _cofinancingService.GetCofinancingId(
                        viewModel.OperationId,
                        viewModel.ActivityPlanId);

                    if (!getCofinancingIdResponse.IsValid)
                    {
                        response.IsValid = getCofinancingIdResponse.IsValid;
                        response.ErrorMessage = getCofinancingIdResponse.ErrorMessage;

                        return Json(response);
                    }

                    viewModel.CoFinancing.Id = getCofinancingIdResponse.CofinancingId;
                }

                var saveComplementaryDetailResponse = _cofinancingService.SaveCofinancingDetail(
                    viewModel.CoFinancing.CofinancingResourcesDetail,
                    viewModel.CoFinancing.Id.Value,
                    operationNumber,
                    userName);

                if (!saveComplementaryDetailResponse.IsValid)
                {
                    response.IsValid = saveCounterpartDetailResponse.IsValid;
                    response.ErrorMessage = saveCounterpartDetailResponse.ErrorMessage;

                    return Json(response);
                }
            }

            return Json(response);
        }

        public virtual JsonResult SaveRequestIncrease(
            string operationNumber,
            int transactionTypeId,
            bool isNewFund,
            int fundId,
            int financingTypeId,
            string currencyCode,
            decimal amount,
            decimal usAmount,
            int executingAgencyId,
            bool isRequestIncrease)
        {
            var request = new SaveRequestIncreaseRequest
            {
                OperationNumber = operationNumber,
                TransactionTypeId = transactionTypeId,
                IsNewFund = isNewFund,
                FundId = fundId,
                FinancingTypeId = financingTypeId,
                CurrencyCode = currencyCode,
                Amount = amount,
                UsAmount = usAmount,
                ExecutingAgencyId = executingAgencyId,
                IsRequestIncrease = isRequestIncrease
            };

            var response = _approvalIncreasesRevampService.SaveRequestIncrease(request);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult SaveChangeIncrease(
            string operationNumber,
            int transactionTypeId,
            bool isNewFund,
            int fundId,
            int financingTypeId,
            string currencyCode,
            decimal amount,
            int executingAgencyId)
        {
            var request = new SaveRequestIncreaseRequest
            {
                OperationNumber = operationNumber,
                TransactionTypeId = transactionTypeId,
                IsNewFund = isNewFund,
                FundId = fundId,
                FinancingTypeId = financingTypeId,
                CurrencyCode = currencyCode,
                Amount = amount,
                ExecutingAgencyId = executingAgencyId
            };

            var response = _approvalIncreasesRevampService.SaveChangeIncrease(request);

            return Json(response, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Environmental
        public virtual JsonResult EnvironmentalSocialDataSave(string operationNumber)
        {
            SaveEnvironmentalSocialDataResponse response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<EnvironmentalSocialDataViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateEnvironmentSocialDataViewModel(
                jsonDataRequest.ClientFieldData, _enumMappingService);

            var userName = IDBContext.Current.UserName;
            var errorMessage = SynchronizationHelper
                .AccessToResources(EDIT, URL_ENVIRONMENTAL_SOCIAL_DATA, operationNumber, userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new SaveEnvironmentalSocialDataResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };

                return Json(response);
            }

            response = _environmentalDataService
                .SaveEnvironmentalSocialData(operationNumber, viewModel);

            GlobalCommonLogic.SetLastOperation(operationNumber);

            if (response.IsValid)
            {
                SynchronizationHelper
                    .TryReleaseLock(URL_ENVIRONMENTAL_SOCIAL_DATA, operationNumber, userName);
            }

            return Json(response);
        }

        #endregion

        #region Deliverable Module

        public virtual JsonResult SaveDeliverable(string operationNumber)
        {
            SaveDeliverableResponse response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<DeliverableViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateDeliverableViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper
                .AccessToResources(
                    EDIT, OPUSGlobalValues.URL_DELIVERABLE, operationNumber, userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new SaveDeliverableResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _deliverableService.SaveDeliverable(operationNumber, viewModel);

                GlobalCommonLogic.SetLastOperation(operationNumber);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(
                        OPUSGlobalValues.URL_DELIVERABLE, operationNumber, userName);
                }
            }

            return Json(response);
        }

        public virtual JsonResult DeleteDocumentDeliverable(
            string operationNumber, int deliverableId)
        {
            var response = _deliverableService.DeleteDeliverableDocument(deliverableId);
            return Json(response);
        }

        #endregion

        #region Creation Form
        public virtual JsonResult CreationForm(string operationNumber)
        {
            SaveCreationFormResponse response;
            var isCreator = true;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<CreationFormViewModel>(jsonDataRequest.SerializedData);
            var roleCreationId = _masterDataRepository
                .GetOne(o => o.NameEn == OPUSGlobalValues.CREATOR_ROLE)
                .ConvergenceMasterDataId;
            if (operationNumber != null &&
                operationNumber != OPUSGlobalValues.NEW_OPERATION_NUMBER)
            {
                var operationId = _operationRepository
                    .GetOne(o => o.OperationNumber == operationNumber)
                    .OperationId;
                isCreator = _operationTeamDataRepository
                    .Any(o => o.OperationId == operationId &&
                        o.UserRoleId == roleCreationId &&
                        o.UserName == IDBContext.Current.UserName);
            }

            var igrId = _catalogService.GetConvergenceMasterDataIdByCode(
                OperationType.IGR, MasterType.OPERATION_TYPE).Id;
            var lonId = _catalogService.GetConvergenceMasterDataIdByCode(
                OperationType.LON, MasterType.OPERATION_TYPE).Id;
            var sgId = _catalogService.GetConvergenceMasterDataIdByCode(
                AttributeValue.SG, MasterType.LENDING_TYPE).Id;
            var nsgId = _catalogService.GetConvergenceMasterDataIdByCode(
                AttributeValue.NSG, MasterType.LENDING_TYPE).Id;
            var bankwideId = _catalogService.GetConvergenceMasterDataIdByCode(
                CountryCode.BK, MasterType.COUNTRY).Id;
            var hqId = _catalogService.GetConvergenceMasterDataIdByCode(
                CountryCode.HQ, MasterType.COUNTRY).Id;
            var administeredById = _catalogService.GetConvergenceMasterDataIdByCode(
                CountryRoleCode.ADMIN, MasterType.COUNTRY_ROLE).Id;
            var responsibleId = _catalogService.GetConvergenceMasterDataIdByCode(
                OrgUnitRoleCode.ORG_RESPONSIBLE, MasterType.ORG_UNIT_ROLE).Id;
            var groups = _divisionRepository.GetAll().ToList();
            var iicId = _catalogService.GetConvergenceMasterDataIdByCode(
                AttributeValue.TYPE_INSTRUMENT_IAIC_TC, MasterType.TYPE_INSTRUMENTS_FINANCED).Id;

            var validationMsg = viewModel.UpdateCreationFormViewModel(
                jsonDataRequest.ClientFieldData,
                operationNumber,
                roleCreationId,
                isCreator,
                igrId,
                lonId,
                sgId,
                nsgId,
                bankwideId,
                hqId,
                administeredById,
                responsibleId,
                iicId,
                groups);

            if (!string.IsNullOrEmpty(validationMsg))
            {
                response = new SaveCreationFormResponse
                {
                    IsValid = false,
                    ErrorMessage = validationMsg
                };
            }
            else
            {
                var notificationCode = string.Empty;
                var pppAttribHasChanged = CheckPPPAttributeHasChanged(
                    operationNumber, jsonDataRequest.ClientFieldData, ref notificationCode);

                viewModel.Attributes.UpdateAttributesViewModel(jsonDataRequest.ClientFieldData);

                var userName = IDBContext.Current.UserLoginName;
                var errorMessage = operationNumber != OPUSGlobalValues.NEW_OPERATION_NUMBER ?
                    SynchronizationHelper.AccessToResources(
                        EDIT, OPUSGlobalValues.URL_CREATION_FORM, operationNumber, userName) :
                    null;

                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    response = new SaveCreationFormResponse
                    {
                        IsValid = false,
                        ErrorMessage = errorMessage
                    };
                }
                else
                {
                    response = _creationFormService.SaveCreationForm(operationNumber, viewModel);

                    GlobalCommonLogic.SetLastOperation(response.OperationNumber);

                    if (pppAttribHasChanged)
                    {
                        var pppNotificationResponse = _opusEmailManager.NotifyPPPAttributeHasChanged(
                        response.OperationNumber, IDBContext.Current.UserName, notificationCode);

                        if (!pppNotificationResponse.IsValid)
                        {
                            response.ErrorMessage += pppNotificationResponse.ErrorMessage;
                            response.IsValid = false;
                        }
                    }

                    if (response.IsValid || !string.IsNullOrEmpty(response.OperationNumber))
                    {
                        SynchronizationHelper.TryReleaseLock(
                            OPUSGlobalValues.URL_CREATION_FORM, operationNumber, userName);
                    }
                }
            }

            return Json(response);
        }

        public virtual JsonResult RegisterOperation(string operationNumber)
        {
            var response = _creationFormService.RegisterOperation(operationNumber, true);

            return Json(response);
        }

        public virtual JsonResult SaveInstitution(string operationNumber)
        {
            SaveInstitutionResponse response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = new InstitutionViewModel();

            viewModel.UpdateInstitutionViewModel(jsonDataRequest.ClientFieldData);

            viewModel.ValidationStage = viewModel.TypeInstitution;

            var userNameK = IDBContext.Current.UserName;
            response = _creationFormService.SaveInstitution(viewModel);

            if (response.IsValid)
            {
                try
                {
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add(K2HelperOpus.TaskUserName, userNameK);
                    parameters.Add(K2HelperOpus.Tag, response.InstitutionId);

                    bool isValid = _k2IntegrationOpus.StartAdvanceWorkflowOpus(
                        K2HelperOpus.WorkflowType004,
                        ZERO,
                        operationNumber,
                        K2HelperOpus.entityTypeOpusInstitution,
                        null,
                        parameters,
                        K2IntegrationEnumerator.GeneralActions.StartWorkflow,
                        0);

                    if (isValid)
                        _institutionService.ChangeInstitutionStatus(
                             response.InstitutionId,
                             InstitutionStatusCode.INST_REVIEW);

                    response.IsValid = true;
                }
                catch (Exception e)
                {
                    response.IsValid = false;
                    response.ErrorMessage = e.ToString();
                }
            }

            response.IsValid = false;

            return Json(response);
        }

        public virtual JsonResult DeleteCreationForm(string operationNumber)
        {
            var response = _creationFormService.DeleteCreationForm(operationNumber);
            Logger.GetLogger().WriteDebug("DeleteCreationForm", response.ErrorMessage);
            return Json(response);
        }
        #endregion

        #region OperationData
        public virtual JsonResult OperationBasicData(string operationNumber)
        {
            SaveBasicDataResponse response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<BasicDataViewModel>(jsonDataRequest.SerializedData);
            var investementId = _catalogService.GetConvergenceMasterDataIdByCode(
                AttributeValue.TYPE_INSTRUMENT_INVESTMENT, MasterType.SG_INSTRUMENTS).Id;
            var cndId = _catalogService.GetConvergenceMasterDataIdByCode(
                AttributeValue.CND, AttributeCode.LON_MODALITY).Id;
            var uccfId = _catalogService.GetConvergenceMasterDataIdByCode(
                RelationTypeCode.UCCF, MasterType.RELATIONSHIP_TYPE).Id;
            var sgId = _catalogService.GetConvergenceMasterDataIdByCode(
                AttributeValue.SG, MasterType.LENDING_TYPE).Id;

            var validationMsg = viewModel.UpdateBasicDataViewModel(
                jsonDataRequest.ClientFieldData,
                sgId,
                cndId,
                investementId,
                uccfId);

            if (!string.IsNullOrEmpty(validationMsg))
            {
                response = new SaveBasicDataResponse
                {
                    IsValid = false,
                    ErrorMessage = validationMsg
                };

                return Json(response);
            }

            var notificationCode = string.Empty;
            var pppAttribHasChanged = CheckPPPAttributeHasChanged(
                operationNumber, jsonDataRequest.ClientFieldData, ref notificationCode);

            viewModel.Attributes.UpdateAttributesViewModel(jsonDataRequest.ClientFieldData);

            if (viewModel.IsPmrNotRequired)
            {
                viewModel.IsPmrNotRequired = false;
            }
            else
            {
                viewModel.IsPmrNotRequired = true;
            }

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(
                EDIT, OPUSGlobalValues.URL_OPERATION_BASIC_DATA, operationNumber, userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new SaveBasicDataResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };

                return Json(response);
            }

            response = _operationDataService.SaveBasicData(operationNumber, viewModel);

            GlobalCommonLogic.SetLastOperation(operationNumber);

            if (pppAttribHasChanged)
            {
                var pppNotificationResponse = _opusEmailManager.NotifyPPPAttributeHasChanged(
                    operationNumber, IDBContext.Current.UserName, notificationCode);

                if (!pppNotificationResponse.IsValid)
                {
                    response.ErrorMessage += pppNotificationResponse.ErrorMessage;
                    response.IsValid = false;
                }
            }

            if (response.IsValid)
            {
                SynchronizationHelper.TryReleaseLock(
                    OPUSGlobalValues.URL_OPERATION_BASIC_DATA, operationNumber, userName);
            }

            return Json(response);
        }

        public virtual JsonResult OperationResponsabilityData(string operationNumber)
        {
            SaveResponsabilityDataResponse response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<ResponsabilityDataViewModel>(jsonDataRequest.SerializedData);

            var operation = _operationRepository.GetOne(o => o.OperationNumber == operationNumber);
            var groups = _divisionRepository.GetAll().ToList();
            var responsible = _catalogService.GetConvergenceMasterDataIdByCode(
                OrgUnitRoleCode.ORG_RESPONSIBLE, MasterType.ORG_UNIT_ROLE);

            if (!responsible.IsValid)
            {
                return Json(new SaveResponsabilityDataResponse
                {
                    IsValid = false,
                    ErrorMessage = responsible.ErrorMessage
                });
            }

            var validationMessage = viewModel.UpdateResponsabilityDataViewModel(
                jsonDataRequest.ClientFieldData, operation, groups, responsible.Id);

            if (!string.IsNullOrEmpty(validationMessage))
            {
                return Json(new SaveResponsabilityDataResponse
                {
                    IsValid = false,
                    ErrorMessage = validationMessage
                });
            }

            var userName = IDBContext.Current.UserLoginName;

            var errorMessage = SynchronizationHelper
                .AccessToResources(EDIT,
                    OPUSGlobalValues.URL_OPERATION_RESPONSABILITY_DATA,
                    operationNumber,
                    userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new SaveResponsabilityDataResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _operationDataService.SaveResponsabilityData(operationNumber, viewModel);

                GlobalCommonLogic.SetLastOperation(operationNumber);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(
                        OPUSGlobalValues.URL_OPERATION_RESPONSABILITY_DATA, operationNumber, userName);
                }
            }

            return Json(response);
        }

        public virtual JsonResult ClassificationData(string operationNumber)
        {
            SaveClassificationDataResponse response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<FormAttributeOperationCollectionViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateAttributesViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(
                EDIT, OPUSGlobalValues.URL_OPERATION_CLASSIFICATION_DATA, operationNumber, userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new SaveClassificationDataResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _operationDataService.SaveClassificationData(viewModel);

                GlobalCommonLogic.SetLastOperation(operationNumber);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(
                        OPUSGlobalValues.URL_OPERATION_CLASSIFICATION_DATA, operationNumber, userName);
                }
            }

            return Json(response);
        }

        public virtual ActionResult StrategicAlignment(string operationNumber)
        {
            SaveStrategicAlignmentResponse response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<StrategicAlignmentViewModel>(jsonDataRequest.SerializedData);

            viewModel = viewModel.UpdateStrategicAlignmentViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(EDIT,
                OPUSGlobalValues.URL_OPERATION_STRATEGIC_ALIGNMENT,
                operationNumber,
                userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new SaveStrategicAlignmentResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _operationDataService.SaveStrategicAlignment(operationNumber, viewModel);

                if (response.IsValid)
                {
                    _cacheData.Remove(_impactsIndicatorCacheName, CacheItemRemovedReason.DependencyChanged);
                    _cacheData.Remove(_outputsPhysicalCacheName, CacheItemRemovedReason.DependencyChanged);
                    _cacheData.Remove(_outputsFinancialCacheName, CacheItemRemovedReason.DependencyChanged);
                    _cacheData.Remove(_outcomesCacheName, CacheItemRemovedReason.DependencyChanged);

                    SynchronizationHelper.TryReleaseLock(
                        OPUSGlobalValues.URL_OPERATION_STRATEGIC_ALIGNMENT, operationNumber, userName);
                }
            }

            if (viewModel.IsModuleDEM)
            {
                var strategicAlignmentDEM = new GetStrategicAlignmentDEMResponse();
                strategicAlignmentDEM.StrategicAlignment = response.StrategicAlignment;
                strategicAlignmentDEM.IsValid = response.IsValid;
                strategicAlignmentDEM.ErrorMessage = response.ErrorMessage;
                var summaryOrResumen = _demService.GetSummaryOrResumen(operationNumber, true);
                strategicAlignmentDEM.SummaryModel = summaryOrResumen.SummaryModel;
                strategicAlignmentDEM.ResumenModel = summaryOrResumen.ResumenModel;

                var viewModelDem = new DemViewModel()
                {
                    Stage = viewModel.InformationDem.Stage,
                    CurrentUser = viewModel.InformationDem.UserName,
                    LastUpdate = Convert.ToDateTime(viewModel.InformationDem.Date),
                    DemChecklistStatus = viewModel.InformationDem.CheckListVersion
                };

                if (response.IsValid)
                {
                    var isWriteDemAfterFinalVersion = _demService.IsFinalVersionCompletedDem(operationNumber) &&
                        !_demService.IsCompletedAppr(operationNumber);

                    var demChecklistStatusCodeInVps = _demService.GetPreviousDemChecklistStatusCode(operationNumber);

                    if (isWriteDemAfterFinalVersion)
                    {
                        var operationId = _businessDemService.GetOperationId(operationNumber);

                        var rspnseCompleted = _businessDemService.DraftVersionToCompletedVersion(
                            operationNumber,
                            operationId,
                            demChecklistStatusCodeInVps);

                        var responseSetCompleted = _businessDemService.SetCompletedVersionDem(
                            operationNumber,
                            DemGlobalValues.LAST_VERSION_COMPLETED);

                        if (rspnseCompleted.IsValid)
                        {
                            viewModelDem.DemChecklistStatus =
                                _demService.GetPreviousDemChecklistStatus(_demService.GetDemOpereationIdCompleted(operationNumber));
                        }

                        response.IsValid = rspnseCompleted.IsValid;
                        response.ErrorMessage = rspnseCompleted.ErrorMessage;
                    }

                    if (response.IsValid)
                    {
                        var validationProcessStatus = _demService.GetValidationProcessStatus(
                        operationNumber, IDBContext.Current.CurrentLanguage);

                        if (validationProcessStatus.DEMValidationProcessStatusList != null)
                        {
                            viewModelDem.ValidationProcessStatus = validationProcessStatus.DEMValidationProcessStatusList;
                        }

                        viewModelDem.CanDownloadSpecialist = _demService.DownloadSPDQRRSpecialist(operationNumber);
                        viewModelDem.CanDownloadCoordinator = _demService.DownloadSPDQRRCoordinator(operationNumber);
                        viewModelDem.CanDownloadChecklist = _demService.CanDownloadChecklist(
                                    operationNumber,
                                    demChecklistStatusCodeInVps,
                                    viewModel.InformationDem.Role);
                        viewModelDem.OfflineIsEnabled = _demService.GetOfflineFunctionalityStatus(
                                    operationNumber).OfflineFunctionalityIsEnabled;
                        viewModelDem.CurrentRole = viewModel.InformationDem.Role;
                    }
                }

                return Json(new JsonResult
                {
                    Data = new
                    {
                        partialSummary = this.RenderRazorViewToString(
                                "~/Areas/DEM/Views/View/Partials/Tabs/PSummaryDualVersion.cshtml",
                                strategicAlignmentDEM.SummaryModel),
                        partialResumen = this.RenderRazorViewToString(
                                "~/Areas/DEM/Views/View/Partials/Tabs/PSummaryDualVersion.cshtml",
                                strategicAlignmentDEM.ResumenModel),
                        partialHeader = this.RenderRazorViewToString(
                                "~/Areas/DEM/Views/View/Partials/PHeaderInfo.cshtml",
                                viewModelDem),
                        partialValidation = this.RenderRazorViewToString(
                                "~/Areas/DEM/Views/View/Partials/Tabs/PValidationProcessStatus.cshtml",
                                viewModelDem),
                        IsValid = response.IsValid,
                        ErrorMessage = response.ErrorMessage
                    }
                });
            }
            else
            {
                return Json(response);
            }
        }

        [HttpPost]
        public virtual ActionResult UpdateRMIndicatorRelationsToSAClassifications(
            StrategicAlignmentIndicatorsViewModel saIndicatorsViewModel)
        {
            var response = _operationDataService
                .UpdateRMIndicatorRelationsToSAClassifications(saIndicatorsViewModel);

            if (response.IsValid)
            {
                _cacheData.Remove(_impactsIndicatorCacheName, CacheItemRemovedReason.DependencyChanged);
                _cacheData.Remove(_outputsPhysicalCacheName, CacheItemRemovedReason.DependencyChanged);
                _cacheData.Remove(_outputsFinancialCacheName, CacheItemRemovedReason.DependencyChanged);
                _cacheData.Remove(_outcomesCacheName, CacheItemRemovedReason.DependencyChanged);
            }

            return Json(response);
        }

        #endregion

        #region Institution
        public virtual JsonResult InstitutionWorkFlow(string operationNumber)
        {
            ResponseBase response = new ResponseBase();
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<InstitutionsWorkflowViewModels>(jsonDataRequest.SerializedData);

            try
            {
                viewModel.UpdateInstitutionWorkFlowViewModel(jsonDataRequest.ClientFieldData);
                response = _institutionService.SaveInstitutionWorkFlow(viewModel);

                var workFlow = jsonDataRequest.ClientFieldData
                    .FirstOrDefault(o => o.Name.Equals(CHANGE_STATUS));

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add(K2HelperOpus.TaskUserName, viewModel.UserName);
                parameters.Add(K2HelperOpus.InstitutionIdNoK2,
                    viewModel.TaskDataModel.Parameters[INSTITUTION_ID]);

                switch (workFlow.Value)
                {
                    case APPROVE_WORKFLOW:
                        response.IsValid = _k2IntegrationOpus.StartAdvanceWorkflowOpus(
                            viewModel.TaskDataModel.Code,
                            viewModel.TaskDataModel.TaskFolio,
                            operationNumber,
                            WorkflowInstanceEntityTypes.INSTITUTION_WF_INSTANCE_ENT_TYPE,
                            viewModel.TaskDataModel.TaskTypeCode,
                            parameters,
                            K2IntegrationEnumerator.GeneralActions.ApproveWorkflow,
                            viewModel.TaskDataModel.TaskId);
                        if (response.IsValid)
                            _institutionService.RegisterUpdateInstitutionLms(
                                viewModel.institutionId,
                                ValidationStageCode.NEW);

                        break;
                    case REJECT_WORKFLOW:
                        response.IsValid = _k2IntegrationOpus.StartAdvanceWorkflowOpus(viewModel.TaskDataModel.Code, viewModel.TaskDataModel.TaskFolio, operationNumber, K2HelperOpus.entityTypeOpusInstitution, viewModel.TaskDataModel.TaskTypeCode, parameters, K2IntegrationEnumerator.GeneralActions.RejectWorkflow, viewModel.TaskDataModel.TaskId);
                        if (response.IsValid)
                            response = _institutionService
                                .ChangeInstitutionStatus(
                                    viewModel.institutionId,
                                    InstitutionStatusCode.INST_DRAFT);
                        break;
                }
            }
            catch (Exception e)
            {
                response.IsValid = false;
                response.ErrorMessage = e.ToString();
            }

            return Json(response);
        }

        public virtual JsonResult AddNewDocument(string documentNumber)
        {
            var document = new
            {
                DocumentNumber = documentNumber,
                Date = FormatHelper.Format(DateTime.Now.Date),
                User = IDBContext.Current.UserName,
                Url = DownloadDocumentHelper.GetDocumentLink(documentNumber)
            };

            var result = Json(new { data = document }, JsonRequestBehavior.AllowGet);

            return result;
        }

        #endregion

        #region AJAX

        public virtual JsonResult EditOPUS(string operationNumber)
        {
            var responseEditOPUS = string.Empty;
            return Json(responseEditOPUS);
        }

        #endregion
        #endregion

        private bool CheckPPPAttributeHasChanged(
            string operationNumber,
            ClientFieldData[] attributesNewValues,
            ref string notificationCode)
        {
            var pppAttributeUpdated = attributesNewValues.FirstOrDefault(a =>
                a.Name == AttributeCode.PUBLIC_PRIVATE_PARTNERSHIP);

            if (pppAttributeUpdated == null)
                return false;
            
            var pppAttributeCurrent = _attributesEngineService.GetAttributeValueByCode(
                operationNumber, AttributeCode.PUBLIC_PRIVATE_PARTNERSHIP);

            if (pppAttributeCurrent.IsValid)
            {
                var currentNumericValue = decimal.Zero;
                var newValue = false;

                decimal.TryParse(pppAttributeCurrent.Value, out currentNumericValue);
                bool.TryParse(pppAttributeUpdated.Value, out newValue);

                var currentValue = currentNumericValue == decimal.One;
                var attribHasChanged = currentValue != newValue;

                if (attribHasChanged)
                    notificationCode = newValue ?
                        EmailCodes.PPP_ATTRIBUTE_ASSIGNED_NOTIFICATION_TEMPLATE :
                        EmailCodes.PPP_ATTRIBUTE_UNASSIGNED_NOTIFICATION_TEMPLATE;

                return attribHasChanged;
            }

            return false;
        }
    }
}