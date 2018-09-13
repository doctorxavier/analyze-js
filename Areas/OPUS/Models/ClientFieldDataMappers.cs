using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;

using IDB.Architecture.Extensions;
using IDB.Architecture.Language;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Application.DEMModule.ViewModels;
using IDB.MW.Application.OPUSModule.Enums;
using IDB.MW.Application.OPUSModule.K2Integration;
using IDB.MW.Application.OPUSModule.Messages.ApprovalIncreasesRevampService;
using IDB.MW.Application.OPUSModule.ViewModels.ApprovalOperationService;
using IDB.MW.Application.OPUSModule.ViewModels.CofinancingService;
using IDB.MW.Application.OPUSModule.ViewModels.Common;
using IDB.MW.Application.OPUSModule.ViewModels.CreationFormService;
using IDB.MW.Application.OPUSModule.ViewModels.DeliverableService;
using IDB.MW.Application.OPUSModule.ViewModels.EnvironmentalSocialDataService;
using IDB.MW.Application.OPUSModule.ViewModels.FinancialDataExecutionService;
using IDB.MW.Application.OPUSModule.ViewModels.FinancialDataPreparationService;
using IDB.MW.Application.OPUSModule.ViewModels.Institutions;
using IDB.MW.Application.OPUSModule.ViewModels.OperationDataService;
using IDB.MW.Domain.Entities;
using IDB.MW.Domain.EntityHelpers;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.Presentation.MVC4.Models.ClientFieldData;

namespace IDB.Presentation.MVC4.Areas.OPUS.Models
{
    public static class ClientFieldDataMappers
    {
        #region Constants
        private const string ROLE_ID = "RoleId";
        private const string COMPONENT_AMOUNT = "ComponentAmount";
        private const string COMPONENT_ID = "ComponentId";
        private const string FUND_AMOUNT = "FundAmount";
        private const string SUSTAINABILITY_TYPE_ID = "SustainabilityTypeId";
        private const char SEPARATOR = ',';
        #endregion

        #region Field Data
        private const string IMPACT_CATEGORY = "impactCategory";
        private const string FUND_OPERATION_ID = "FundOperationId";
        private const string FINANCING_TYPE_ID = "FinancingTypeId";
        private const string SELECTED_PSG_DONORS = "selectedPsgDonors";
        private const string DONORS = "Donors";
        private const string FUND_ID = "FundId";
        private const string FUND_TBD = "FundTBD";
        private const string US_AMOUNT = "UsAmount";
        private const string EXECUTING_AGENCY_ID = "ExecutingAgencyId";
        private const string COUNTERPART_FINANCING_IN_KIND = "CounterpartFinancingInKind";
        private const string COUNTERPART_FINANCING_AMOUNT = "CounterpartFinancingAmount";
        private const string DESCRIPTION = "Description";
        private const string COFINANCING_IN_KIND = "CoFinancingInKind";
        private const string COFINANCING_AMMOUNT_TOTAL_VALUE = "CoFinancingAmmountTotalValue";
        private const string COFINANCING_NAME = "CoFinancingName";
        private const string PIPELINE_CATEGORY_ID = "PipelineCategoryId";
        private const string COFINANCING_DESCRIPTION = "CoFinancingDescription";
        private const string IS_RETROACTIVE_EXPENSE = "IsRetroactiveExpense";
        private const string RETROACTIVE_EXPENSE_PERCENT = "RetroactiveExpensePercent";
        private const string STARTING_DATE = "StartingDate";
        private const string END_DATE = "EndDate";
        private const string HD_INCREASE_ID = "hdIncreaseId";
        private const string HD_FUND_ID = "hdFundId";
        private const string FUND_CURRENCY = "fundCurrency";
        private const string AMOUNT = "Amount";
        private const string EFFECTIVE_DATE = "EffectiveDate";
        private const string HD_VALIDATION_STAGE_ID = "hdValidationStageId";
        private const string COUNTRY = "Country";
        private const string NAME = "Name";
        private const string ADDRESS = "Address";
        private const string CITY = "City";
        private const string STATE = "State";
        private const string ZIP_CODE = "ZipCode";
        private const string TELEPHONE_NUMBER = "TelephoneNumber";
        private const string FAX_NUMBER = "FaxNumber";
        private const string WEBSITE = "Website";
        private const string TYPE_INSTITUTION = "TypeInstitution";
        private const string ACRONYM_DESC = "AcronymDesc";
        private const string IS_SAVE_ONLY = "isGoingToSaveOnly";
        private const string OPERATION_TYPE = "operationType";
        private const string COUNTRY_DEPARTMENT_ID = "countryDepartmentId";
        private const string OPERATION_YEAR = "operationYear";
        private const string CATEGORY = "category";
        private const string MBF_CODE = "mbfCode";
        private const string APPROVAL_DATE = "approvalDate";
        private const string DATE_FORMAT = "dd MMM yyyy";
        private const string SECTOR = "sector";
        private const string SUB_SECTOR = "subsector";
        private const string OPERATION_NAME_SPANISH = "operationNameSpanish";
        private const string OPERATION_NAME_ENGLISH = "operationNameEnglish";
        private const string OPERATION_NAME_FRENCH = "operationNameFrech";
        private const string OPERATION_NAME_PORTUGUESE = "operationNamePortuguese";
        private const string OPERATION_OBJECTIVE_SPANISH = "operationObjectiveSpanish";
        private const string OPERATION_OBJECTIVE_ENGLISH = "operationObjectiveEnglish";
        private const string OPERATION_OBJECTIVE_FRENCH = "operationObjectiveFrench";
        private const string OPERATION_OBJECTIVE_PORTUGUESE = "operationObjectivePortuguese";
        private const string RELATED_OPERATION_ID = "relatedOperationId";
        private const string OPERATION_RELATED_ID = "OperationRelatedId";
        private const string RELATED_RELATION_ROLE_ID = "relatedRelationRoleId";
        private const string RELATED_RELATION_TYPE_ID = "relatedRelationTypeId";
        private const string RELATED_START_DATE = "relatedStartDate";
        private const string RELATED_END_DATE = "relatedEndDate";
        private const string RELATED_USER_COMMENT_ID = "relatedUserCommentId";
        private const string RELATED_COMMENTS = "relatedComments";
        private const string RELATED_OPERATION_RELATIONSHIP_ID = "relatedOperationRelationshipId";
        private const string RELATIONSHIP_ROLE_THIS_OPERATION_ID = "RelationShipRoleThisOperationId";
        private const string RESPONSIBLE_UNIT_ID = "responsibleUnitId";
        private const string RESPONSIBLE_UNITS_ROLE = "responsibleUnitsRole";
        private const string RESPONSIBLE_UNITS_ORGANIZATIONAL_UNIT = "responsibleUnitsOrganizationalUnit";
        private const string RESPONSIBLE_UNITS_EFFORT_IN_DAYS = "responsibleUnitsEffortInDays";
        private const string RESULT_MATRIX_ID = "ResultMatrixId";
        private const string COUNTRY_RELATED_ID = "CountryRelatedId";
        private const string ASSOCIATED_COUNTRIES_ROLE = "associatedCountriesRole";
        private const string LIST_COUNTRY = "listCountry";
        private const string INSTITUTION_RELATED_ID = "InstitutionRelatedId";
        private const string ASSOCIATED_INSTITUTIONS_ROLE = "associatedInstitutionsRole";
        private const string ASSOCIATED_INSTITUTIONS = "associatedInstitutions";
        private const string ROLE_ORGANIZATIONAL_UNIT = "RoleOrganizationalUnit";
        private const string ORGANIZATIONAL_UNIT = "OrganizationalUnit";
        private const string ORGANIZATIONAL_UNIT_ROW_ID = "OrganizationalUnitRowId";
        private const string DEFAULT_ID = "0";
        private const string DEFAULT_VALUE = "0";
        private const string OPERATION_TEAM_DATA_ID = "operationTeamDataId";
        private const string OPERATION_TEAMS_NAME = "operationTeamsName";
        private const string OPERATION_TEAMS_ROLE = "operationTeamsRole";
        private const string USERNAME_TEAM = "userNameTeam";
        private const string ORGANIZATIONAL_UNIT_TEAM = "organizationalUnitTeam";
        private const string OPERATION_TEAM_CODE_TYPE = "operationTeamCodeType";
        private const string INDICATOR_CHECK = "indicator-Check";
        private const string IMPACT_INDICATORS_COMBO = "impactIndicators-Combo";
        private const string DELIVERABLE_ID = "deliverableId";
        private const string DELIVERABLE_TYPE = "deliverableType";
        private const string DELIVERABLES_YEAR_PLANNED = "deliverablesYearPlanned";
        private const string DELIVERABLES_RESPONSIBLE = "deliverablesResponsible";
        private const string DELIVERABLES_COUNTRY = "deliverablesCountry";
        private const string PLANED_DATE = "planedDate";
        private const string COMPLETED_DATE = "completedDate";
        private const string DELIVERABLES_DOCUMENT_NUMBER = "deliverablesDocumentNumber";
        private const string DOCUMENT_NUMBER_HIDDEN = "documentNumberHidden";
        private const string DELIVERABLE_NAME = "deliverableName";
        private const string IS_DISMISS = "IsDismiss";
        private const string COUNTRY_GROUP = "countryGroup";
        private const string IS_PMR_NOT_REQUIRED = "IsPmrNotRequired";
        private const string IS_OBJECTIVE_REFORMULATED = "IsObjectiveReformulated";
        private const string REFORMULATION_APPROVAL_DATE = "ReformulationApprovalDate";
        private const string COMMENT_COMMENT = "commentComment";
        private const string COMMENT_COMMENT_ID = "commentCommentId";
        private const string OUTCOME_INDICATORS_COMBO = "outcomeIndicators-Combo";
        private const string OUTPUTS_COMBO = "outputs-Combo";
        private const string JUSTIFICATION = "justification";
        private const string DATA_PERSIST_SUBINDICATOR_ID = "data-persist-subindicatorid";
        private const string TEXT_COMMENT = "textComment";
        private const string COMMENT_ID = "commentId";
        private const string NEW_COMMENT = "newComment";
        private const string DELETE_COMMENTS = "deleteComments";
        private const string DOCUMENT_DESCRIPTION = "documentDescription";
        private const string DOCUMENT_NUMBER = "documentNumber";
        private const string DOCUMENT_DATE = "documentDate";
        private const string DOCUMMENT_NUMBERS = "docummentNumbers";
        private const string IDB_APPROVED_AMOUNT = "IDBApprovedAmount";
        private const string IDB_US_APPROVED_AMOUNT = "USApprovedAmount";
        private const string EXECUTOR = "Executor";
        private const string IS_INCREASE = "IsIncrease";
        private const string IS_AUTOMATIC = "IsAutomatic";
        private const string IS_CURRENT_REFORMULATION = "IsCurrentReformulation";
        private const string SUFFIX = "Suffix";
        private const string APPROVAL_NUMBER = "newApprovalNumber_text";
        private const string RESOLUTION_NUMBER = "ResolutionNumber";
        private const string DOCUMENT_NUMBER_APPR = "DocumentNumber";
        private const string DOCUMENT_DATE_APPR = "DocumentDate";
        private const string OPERATION_TEAM_DATA_ID_SPECIALIST = "OperationTeamDataId";
        private const string DATA_PERSIST_NEW = "data-persist-new";
        private const string TRUE_VAL = "true";
        private const string DELIVERABLE_DOCUMENT_ID = "deliverableDocumentId";
        private const string DELETED_RELATIONSHIPS = "delRelRegisterList";
        private const string ACTIVITY_PLAN_ID = "ActivityPlanId";
        private const string IS_CURRENT = "IsCurrent";
        private const string DATA_PERSIST_PARENT_ID = "data-persist-parent-id";
        private const string APPROVAL_AUTHORITY = "ApprovalAuthority";
        private const string APPROVAL_PROCESS = "ApprovalProcessSubTable";
        private const string APPROVAL_OPERATION_DATE = "ApprovalDate";
        private const string COUNTERPART_ID_DETAIL = "CounterpartDetailId";
        private const string COUNTERPART_FINANCING_TYPES = "CounterpartFinancingTypes";
        private const string COUNTERPART_FINANCING_FUNDING_SOURCE = "CounterpartFundingSource";
        private const string COUNTERPART_FINANCING_CASH_OR_INKIND = "CounterpartFinancingCashOrInKind";
        private const string COUNTERPART_FINANCING_DESCRIPTION = "CounterpartFinancingDescription";
        private const string COUNTERPART_AMOUNT = "CounterPartAmount";
        private const string COUNTERPART_STATUS = "CounterpartStatus";
        private const string COFINANCING_ID = "CoFinancingId";
        private const string COFINANCING_FUNDING_SOURCE = "CoFinancingFundingSource";
        private const string COFINANCING_MODALITY = "CoFinancingModality";
        private const string COFINANCING_AMOUNT = "CoFinancingAmount";
        private const string COFINANCING_SIGNATURE_DATE = "CoFinancingSignatureDate";
        private const string COFINANCING_ID_DETAIL = "CofinancingDetailId";
        private const string COFINANCING_CASH_OR_INKIND = "CoFinancingCashOrInKind";
        private const string COFINANCING_STATUS = "CoFinancingStatus";
        private const string OPERATION_ID = "OperationId";
        private const string PEP_NOT_INTEGRATED = "PlanningInstrumentsIntegratedCheck";
        private const string HDN_FUND = "hidFundId";
        private const string HDN_FINANCING = "hidFinancingTypeId";

        #endregion

        #region Mappers
        #region EnvironmentSocialData
        public static void UpdateEnvironmentSocialDataViewModel(
            this EnvironmentalSocialDataViewModel viewModel,
            ClientFieldData[] clientFieldData,
            IEnumMappingService enumMappingService)
        {
            var impactCategory = clientFieldData
                .FirstOrDefault(c => c.Name.Contains(IMPACT_CATEGORY));

            if (impactCategory != null)
            {
                viewModel.ImpactCategory = Convert.ToInt32(impactCategory.Value);
            }

            viewModel.ESGTeamMembers
                .UpdateESGTeamMembers(clientFieldData, enumMappingService);

            viewModel.SustainabilityComponents
                .UpdateSustainabilityComponents(clientFieldData, enumMappingService);
        }
        #endregion

        #region FinancialData
        public static void UpdateFinancialDataPreparationViewModel(
            this FinancialDataPreparationViewModel viewModel,
            ClientFieldData[] clientFieldData)
        {
            var fundOperationId = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(FUND_OPERATION_ID))
                .ToList();
            var financingType = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(FINANCING_TYPE_ID))
                .ToList();
            var funds = clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                o.Name.Equals(FUND_ID)).ToList();
            var fundsTBD = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(FUND_TBD))
                .ToList();
            var UsfundAmount = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(US_AMOUNT))
                .ToList();
            var donors = clientFieldData
                .Where(o => (!string.IsNullOrWhiteSpace(o.Name)) && (o.Name.Equals(SELECTED_PSG_DONORS) || o.Name.Equals(DONORS)))
                .ToList();
            var executingAgency = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                    o.Name.Equals(EXECUTING_AGENCY_ID))
                .ToList();

            viewModel.ExpectedIDB = (
                from financing in financingType
                join fund in funds
                    on financingType.IndexOf(financing) equals funds.IndexOf(fund)
                join amount in UsfundAmount
                    on financingType.IndexOf(financing) equals UsfundAmount.IndexOf(amount)
                join fundId in fundOperationId
                    on financingType.IndexOf(financing) equals fundOperationId.IndexOf(fundId)
                join fundTBD in fundsTBD
                    on financingType.IndexOf(financing) equals fundsTBD.IndexOf(fundTBD)
                join donor in donors
                    on financingType.IndexOf(financing) equals donors.IndexOf(donor) into tmpDonor
                from donor in tmpDonor.DefaultIfEmpty()
                join executingAgencyId in executingAgency
                    on financingType.IndexOf(financing)
                    equals executingAgency.IndexOf(executingAgencyId)
                select new ExpectedIDBViewModel
                {
                    IdFundOperation = int.Parse(fundId.Value),
                    FinancingTypeId = int.Parse(financing.Value),
                    FundId = int.Parse(fund.Value),
                    FundsTbd =
                        !string.IsNullOrEmpty(fundTBD.Value) ?
                            fundTBD.Value.Split(',').Select(int.Parse).ToList() :
                            new List<int>(),
                    UsAmount = decimal.Parse(amount.Value),
                    Amount = decimal.Parse(amount.Value),
                    ExecutingAgencyId = int.Parse(executingAgencyId.Value),
                    Donors =
                        donor == null || string.IsNullOrEmpty(donor.Value) ? string.Empty : donor.Value
                }).ToList();

            viewModel.DonorsForRemoveMilestone = GetDonorsForRemoveMilestone(viewModel);

            viewModel.CounterpartFinancing.CounterpartFinancingDetail =
                GetCounterpartDetails(clientFieldData);

            var counterpartAmountTotal = viewModel.CounterpartFinancing
                .CounterpartFinancingDetail.Where(x => x.StatusTypeCode !=
                    OPUSGlobalValues.CANCELLED_STATUS).Select(x => x.Amount)
                    .ToList();

            viewModel.CounterpartFinancing.CounterpartAmount = counterpartAmountTotal != null ?
                counterpartAmountTotal.Sum(x => x) : 0.00m;

            viewModel.CoFinancing.CofinancingResourcesDetail =
                GetCofinancingDetails(clientFieldData);

            var complementaryAmountTotal = viewModel.CoFinancing
                .CofinancingResourcesDetail.Where(x => x.StatusTypeCode !=
                    OPUSGlobalValues.CANCELLED_STATUS).Select(x => x.Amount)
                    .ToList();

            viewModel.CoFinancing.CoFinancingAmount = complementaryAmountTotal != null ?
                complementaryAmountTotal.Sum(x => x) : 0.00m;

            viewModel.CoFinancingAmount = viewModel.CoFinancing.CoFinancingAmount;

            var counterpartFinancingInKind = clientFieldData
                .FirstOrDefault(o => o.Name.Equals(COUNTERPART_FINANCING_IN_KIND));

            if (counterpartFinancingInKind != null)
            {
                viewModel.CounterpartFinancing.InKind =
                    bool.Parse(counterpartFinancingInKind.Value);
            }

            var description = clientFieldData
                .FirstOrDefault(o => o.Name.Equals(DESCRIPTION));

            if (description != null)
            {
                viewModel.CounterpartFinancing.Description = description.Value;
            }

            var coFinancingInKind = clientFieldData
                .FirstOrDefault(o => o.Name.Equals(COFINANCING_IN_KIND));

            if (coFinancingInKind != null)
            {
                viewModel.CoFinancingInKind = bool.Parse(coFinancingInKind.Value);
            }

            var coFinancingId = clientFieldData
                .FirstOrDefault(o => o.Name.Equals(COFINANCING_ID));

            if (coFinancingId != null && coFinancingId.Id != null)
            {
                viewModel.CoFinancing.Id = int.Parse(coFinancingId.Value);
            }

            var coFinancingName = clientFieldData
                .FirstOrDefault(o => o.Name.Equals(COFINANCING_NAME));

            if (coFinancingName != null)
            {
                viewModel.CoFinancing.Name = coFinancingName.Value;
            }

            var coFinancingDescription = clientFieldData
                .FirstOrDefault(o => o.Name.Equals(COFINANCING_DESCRIPTION));

            if (coFinancingDescription != null)
            {
                viewModel.CoFinancing.Description = coFinancingDescription.Value;
            }

            var isRetroactiveExpense = clientFieldData
                .FirstOrDefault(o => o.Name.Equals(IS_RETROACTIVE_EXPENSE));

            if (isRetroactiveExpense != null)
            {
                viewModel.IsRetroactiveExpense =
                    isRetroactiveExpense.Value.ToUpper() == AttributeValue.YES.ToUpper();
            }

            var activityPlanId = clientFieldData
                .FirstOrDefault(o => o.Name.Equals(ACTIVITY_PLAN_ID));

            viewModel.ActivityPlanId = 0;
            if (activityPlanId != null && activityPlanId.Value != null)
            {
                viewModel.ActivityPlanId = int.Parse(activityPlanId.Value);
            }

            var reformulationApprovalDate = clientFieldData.FirstOrDefault(o =>
                !string.IsNullOrWhiteSpace(o.Name) &&
                o.Name.Equals(REFORMULATION_APPROVAL_DATE));

            viewModel.ReformulationApprovalDate = null;
            if (reformulationApprovalDate != null &&
                !string.IsNullOrEmpty(reformulationApprovalDate.Value))
            {
                viewModel.ReformulationApprovalDate = Convert.ToDateTime(
                    reformulationApprovalDate.Value);
            }

            var isCurrent = clientFieldData.FirstOrDefault(c => c.Name == IS_CURRENT);

            viewModel.IsCurrent = false;
            if (!string.IsNullOrEmpty(isCurrent.Value))
            {
                viewModel.IsCurrent = bool.Parse(isCurrent.Value);
            }

            if (!viewModel.IsRetroactiveExpense)
            {
                viewModel.RetroactiveExpensePercent = null;
                viewModel.StartingDate = null;
                viewModel.EndDate = null;
            }
            else
            {
                var retroactiveExpensePercent = clientFieldData
                    .FirstOrDefault(o => o.Name.Equals(RETROACTIVE_EXPENSE_PERCENT));

                if (retroactiveExpensePercent != null)
                {
                    if (viewModel.IsRetroactiveExpense)
                    {
                        viewModel.RetroactiveExpensePercent =
                            decimal.Parse(retroactiveExpensePercent.Value) / 100;
                    }
                    else
                    {
                        viewModel.RetroactiveExpensePercent = 0;
                    }
                }

                var startingDate = clientFieldData
                    .FirstOrDefault(o => o.Name.Equals(STARTING_DATE));

                if (startingDate != null && !string.IsNullOrEmpty(startingDate.Value))
                {
                    viewModel.StartingDate = Convert.ToDateTime(startingDate.Value);
                }

                var endDate = clientFieldData
                    .FirstOrDefault(o => o.Name.Equals(END_DATE));

                if (endDate != null && !string.IsNullOrEmpty(endDate.Value))
                {
                    viewModel.EndDate = Convert.ToDateTime(endDate.Value);
                }
            }

            var pipelineCategoryId = clientFieldData
                .FirstOrDefault(o => o.Name.Equals(PIPELINE_CATEGORY_ID));

            if (pipelineCategoryId != null && !string.IsNullOrEmpty(pipelineCategoryId.Value))
            {
                viewModel.PipelineCategoryId = pipelineCategoryId.Value.ConvertToInt();
            }

            var operationId = clientFieldData
                   .FirstOrDefault(o => o.Name.Equals(OPERATION_ID));

            if (operationId != null && !string.IsNullOrEmpty(operationId.Value))
            {
                viewModel.OperationId = int.Parse(operationId.Value);
            }
        }
        #endregion

        #region ExecutionData
        public static void UpdateFinancialDataExecutionViewModel(
            this FundIncreasesViewModel viewModel,
            ClientFieldData[] clientFieldData)
        {
            var increaseIds = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                    o.Name.Equals(HD_INCREASE_ID))
                .ToList();
            var approvalNumbers = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                    o.Name.Equals(FINANCING_TYPE_ID))
                .ToList();
            var funds = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                    o.Name.Equals(HD_FUND_ID))
                .ToList();
            var fundsCurrency = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                    o.Name.Equals(FUND_CURRENCY))
                .ToList();
            var increaseAmounts = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                    o.Name.Equals(AMOUNT))
                .ToList();
            var effectiveDates = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                    o.Name.Equals(EFFECTIVE_DATE))
                .ToList();
            var validationStageIds = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                    o.Name.Equals(HD_VALIDATION_STAGE_ID))
                .ToList();
            int rowId = 0;
            int valStageId = 0;

            viewModel.Increases =
                (from increaseId in increaseIds
                 join approvalNumber in approvalNumbers on increaseIds.IndexOf(increaseId)
                     equals approvalNumbers.IndexOf(approvalNumber)
                 join fund in funds on increaseIds.IndexOf(increaseId)
                     equals funds.IndexOf(fund)
                 join fundCurrency in fundsCurrency on increaseIds.IndexOf(increaseId)
                     equals fundsCurrency.IndexOf(fundCurrency)
                 join increaseAmount in increaseAmounts on increaseIds.IndexOf(increaseId)
                     equals increaseAmounts.IndexOf(increaseAmount)
                 join effectiveDate in effectiveDates on increaseIds.IndexOf(increaseId)
                     equals effectiveDates.IndexOf(effectiveDate)
                 join validationStageId in validationStageIds
                     on increaseIds.IndexOf(increaseId)
                     equals validationStageIds.IndexOf(validationStageId)
                 select new IncreaseRowViewModel
                 {
                     RowId = int.TryParse(increaseId.Value, out rowId) ? rowId : 0,
                     ApprovalNumber = approvalNumber.Value,
                     Fund = fund.Value,
                     FundCurrency = fundCurrency.Value,
                     IncreaseAmount = decimal.Parse(increaseAmount.Value),
                     EquivalentIncreaseAmount = (fundCurrency.Value == TCGlobalValues.CURRENCY_CODE_USD) ?
                         decimal.Parse(increaseAmount.Value) : 0,
                     EffectiveDate = effectiveDate.Value != string.Empty ?
                         DateTime.Parse(effectiveDate.Value) : DateTime.MinValue,
                     ValidationStageId = int.TryParse(validationStageId.Value, out valStageId) ?
                         valStageId : 0
                 }).ToList();
        }
        #endregion

        #region Creation Form
        public static void UpdateInstitutionViewModel(this InstitutionViewModel viewModel,
            ClientFieldData[] clientFieldData)
        {
            var name = clientFieldData.FirstOrDefault(o => o.Name.Equals(NAME));

            if (name != null)
            {
                viewModel.Name = name.Value;
            }

            var address = clientFieldData.FirstOrDefault(o => o.Name.Equals(ADDRESS));

            if (address != null)
            {
                viewModel.Address = address.Value;
            }

            var city = clientFieldData.FirstOrDefault(o => o.Name.Equals(CITY));

            if (city != null)
            {
                viewModel.City = city.Value;
            }

            var state = clientFieldData.FirstOrDefault(o => o.Name.Equals(STATE));

            if (state != null)
            {
                viewModel.State = state.Value;
            }

            var zipCode = clientFieldData.FirstOrDefault(o => o.Name.Equals(ZIP_CODE));

            if (zipCode != null)
            {
                viewModel.ZipCode = zipCode.Value;
            }

            var country = clientFieldData.FirstOrDefault(o => o.Name.Equals(COUNTRY));

            if (country != null)
            {
                viewModel.Country = Convert.ToInt32(country.Value);
            }

            var telephoneNumber = clientFieldData
                .FirstOrDefault(o => o.Name.Equals(TELEPHONE_NUMBER));

            if (telephoneNumber != null)
            {
                viewModel.TelephoneNumber = telephoneNumber.Value;
            }

            var faxNumber = clientFieldData
                .FirstOrDefault(o => o.Name.Equals(FAX_NUMBER));

            if (faxNumber != null)
            {
                viewModel.FaxNumber = faxNumber.Value;
            }

            var website = clientFieldData.FirstOrDefault(o => o.Name.Equals(WEBSITE));

            if (website != null)
            {
                viewModel.Website = website.Value;
            }

            var typeInstitution = clientFieldData
                .FirstOrDefault(o => o.Name.Equals(TYPE_INSTITUTION));

            if (typeInstitution != null)
            {
                viewModel.TypeInstitution = int.Parse(typeInstitution.Value);
            }

            var acronymDesc = clientFieldData
                .FirstOrDefault(o => o.Name.Equals(ACRONYM_DESC));

            if (acronymDesc != null)
            {
                viewModel.Acronym = acronymDesc.Value;
            }
        }

        public static string UpdateCreationFormViewModel(
            this CreationFormViewModel viewModel,
            ClientFieldData[] clientFieldData,
            string operationNumber,
            int roleCreationId,
            bool isCreator,
            int igrId,
            int lonId,
            int sgId,
            int nsgId,
            int bankwideId,
            int hqId,
            int administeredById,
            int responsibleId,
            int iicId,
            IList<DivisionOpus> groups)
        {
            var editPemission = IDBContext.Current.HasPermission(Permission.CREATION_WRITE) ||
                operationNumber == OPUSGlobalValues.NEW_OPERATION_NUMBER;

            if (editPemission || isCreator)
            {
                var operationType = clientFieldData.FirstOrDefault(
                        o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(OPERATION_TYPE));

                if (operationType != null)
                {
                    viewModel.BasicData.OperationType = Convert.ToInt32(operationType.Value);
                }

                var country = clientFieldData.FirstOrDefault(o =>
                    !string.IsNullOrWhiteSpace(o.Name) && o.Name.ToLower().Equals(COUNTRY.ToLower()));

                if (country != null && country.Value != string.Empty)
                {
                    viewModel.BasicData.CountryId = Convert.ToInt32(country.Value);
                }

                var countryDep = clientFieldData.FirstOrDefault(o =>
                    !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(COUNTRY_DEPARTMENT_ID));

                if (countryDep != null && countryDep.Value != string.Empty)
                {
                    viewModel.BasicData.CountryDepartmentId = Convert.ToInt32(countryDep.Value);
                }
                else
                {
                    viewModel.BasicData.CountryDepartmentId = null;
                }

                var operationYear = clientFieldData.FirstOrDefault(o =>
                    !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(OPERATION_YEAR));

                if (operationYear != null)
                {
                    if (!string.IsNullOrEmpty(operationYear.Value))
                    {
                        viewModel.BasicData.OperationYear = Convert.ToInt32(operationYear.Value);
                    }
                    else
                    {
                        viewModel.BasicData.OperationYear = null;
                    }
                }

                var category = clientFieldData.FirstOrDefault(o =>
                    !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(CATEGORY));

                if (category != null)
                {
                    if (!string.IsNullOrEmpty(category.Value))
                    {
                        viewModel.BasicData.Category = Convert.ToInt32(category.Value);
                    }
                    else
                    {
                        viewModel.BasicData.Category = null;
                    }
                }

                var mbfCode = clientFieldData.FirstOrDefault(o =>
                    !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(MBF_CODE));

                if (mbfCode != null)
                {
                    if (!string.IsNullOrEmpty(mbfCode.Value))
                    {
                        viewModel.BasicData.MbfCode = Convert.ToInt32(mbfCode.Value);
                    }
                    else
                    {
                        viewModel.BasicData.MbfCode = null;
                    }
                }

                var approvalDate = clientFieldData.FirstOrDefault(o =>
                    !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(APPROVAL_DATE));

                if (approvalDate != null)
                {
                    if (!string.IsNullOrEmpty(approvalDate.Value))
                    {
                        viewModel.BasicData.ApprovalDate = DateTime
                            .ParseExact(approvalDate.Value, DATE_FORMAT, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        viewModel.BasicData.ApprovalDate = null;
                    }
                }

                var sector = clientFieldData.FirstOrDefault(o =>
                    !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(SECTOR));

                if (sector != null)
                {
                    if (!string.IsNullOrEmpty(sector.Value))
                    {
                        viewModel.BasicData.Sector = Convert.ToInt32(sector.Value);
                    }
                }

                var subSector = clientFieldData.FirstOrDefault(o =>
                    !string.IsNullOrWhiteSpace(o.Name) && o.Name.ToLower().Equals(SUB_SECTOR));

                if (subSector != null)
                {
                    if (!string.IsNullOrEmpty(subSector.Value))
                    {
                        viewModel.BasicData.Subsector = Convert.ToInt32(subSector.Value);
                    }
                    else if (!string.IsNullOrEmpty(sector.Value))
                    {
                        viewModel.BasicData.Subsector = Convert.ToInt32(sector.Value);
                    }
                    else
                    {
                        viewModel.BasicData.Subsector = null;
                    }
                }

                var operationNameSpanish = clientFieldData.FirstOrDefault(o =>
                    !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(OPERATION_NAME_SPANISH));

                if (operationNameSpanish != null)
                {
                    viewModel.BasicData.OperationNameSpanish =
                        operationNameSpanish.Value.StripDoubleQuotes();
                }

                var operationNameEnglish = clientFieldData.FirstOrDefault(o =>
                    !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(OPERATION_NAME_ENGLISH));

                if (operationNameEnglish != null)
                {
                    viewModel.BasicData.OperationNameEnglish =
                        operationNameEnglish.Value.StripDoubleQuotes();
                }

                var operationNameFrench = clientFieldData.FirstOrDefault(o =>
                    !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(OPERATION_NAME_FRENCH));

                if (operationNameFrench != null)
                {
                    viewModel.BasicData.OperationNameFrench =
                        operationNameFrench.Value.StripDoubleQuotes();
                }

                var operationNamePortuguese = clientFieldData.FirstOrDefault(o =>
                    !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(OPERATION_NAME_PORTUGUESE));

                if (operationNamePortuguese != null)
                {
                    viewModel.BasicData.OperationNamePortuguese =
                        operationNamePortuguese.Value.StripDoubleQuotes();
                }

                var operationObjectiveSpanish = clientFieldData.FirstOrDefault(o =>
                    !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(OPERATION_OBJECTIVE_SPANISH));

                if (operationObjectiveSpanish != null)
                {
                    viewModel.BasicData.OperationObjectiveSpanish = operationObjectiveSpanish.Value;
                }

                var operationObjectiveEnglish = clientFieldData.FirstOrDefault(o =>
                    !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(OPERATION_OBJECTIVE_ENGLISH));

                if (operationObjectiveEnglish != null)
                {
                    viewModel.BasicData.OperationObjectiveEnglish = operationObjectiveEnglish.Value;
                }

                var operationObjectiveFrench = clientFieldData.FirstOrDefault(o =>
                    !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(OPERATION_OBJECTIVE_FRENCH));

                if (operationObjectiveFrench != null)
                {
                    viewModel.BasicData.OperationObjectiveFrench = operationObjectiveFrench.Value;
                }

                var operationObjectivePortuguese = clientFieldData.FirstOrDefault(o =>
                    !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(OPERATION_OBJECTIVE_PORTUGUESE));

                if (operationObjectivePortuguese != null)
                {
                    viewModel.BasicData.OperationObjectivePortuguese = operationObjectivePortuguese.Value;
                }

                var relatedOperationIds = clientFieldData
                    .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                        o.Name.Equals(RELATED_OPERATION_ID))
                    .ToList();
                var operationRelatedIds = clientFieldData
                    .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                        o.Name.Equals(OPERATION_RELATED_ID))
                    .ToList();
                var relatedRelationshipRoleIds = clientFieldData
                    .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                        o.Name.Equals(RELATED_RELATION_ROLE_ID))
                    .ToList();
                var relatedOperationRelationTypeIds = clientFieldData
                    .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                        o.Name.Equals(RELATED_RELATION_TYPE_ID))
                    .ToList();
                var relatedOperationStartDates = clientFieldData
                    .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                        o.Name.Equals(RELATED_START_DATE))
                    .ToList();
                var relatedOperationExpirationDates = clientFieldData
                    .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                        o.Name.Equals(RELATED_END_DATE))
                    .ToList();
                var relatedOperationUserCommentIds = clientFieldData
                    .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                        o.Name.Equals(RELATED_USER_COMMENT_ID))
                    .ToList();
                var relatedOperationUserComments = clientFieldData
                    .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                        o.Name.Equals(RELATED_COMMENTS))
                    .ToList();
                var relatedOperationRelationshipIds = clientFieldData
                    .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                        o.Name.Equals(RELATED_OPERATION_RELATIONSHIP_ID))
                    .ToList();
                var relationShipRoleThisOperationIds = clientFieldData
                    .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                        o.Name.Equals(RELATIONSHIP_ROLE_THIS_OPERATION_ID))
                    .ToList();

                viewModel.BasicData.RelatedOperations =
                    (from relatedOperationId in relatedOperationIds
                     join operationRelatedId in operationRelatedIds on
                         relatedOperationIds.IndexOf(relatedOperationId) equals
                         operationRelatedIds.IndexOf(operationRelatedId)
                     join operationRelatedRoleId in relatedRelationshipRoleIds on
                         relatedOperationIds.IndexOf(relatedOperationId) equals
                         relatedRelationshipRoleIds.IndexOf(operationRelatedRoleId)
                     join operationRelationTypeId in relatedOperationRelationTypeIds on
                         relatedOperationIds.IndexOf(relatedOperationId) equals
                         relatedOperationRelationTypeIds.IndexOf(operationRelationTypeId)
                     join relatedOperationStartDate in relatedOperationStartDates on
                         relatedOperationIds.IndexOf(relatedOperationId) equals
                         relatedOperationStartDates.IndexOf(relatedOperationStartDate)
                     join relatedOperationExpirationDate in relatedOperationExpirationDates on
                         relatedOperationIds.IndexOf(relatedOperationId) equals
                         relatedOperationExpirationDates.IndexOf(relatedOperationExpirationDate)
                     join relatedOperationUserComentId in relatedOperationUserCommentIds on
                         relatedOperationIds.IndexOf(relatedOperationId) equals
                         relatedOperationUserCommentIds.IndexOf(relatedOperationUserComentId)
                     join relatedOperationUserComment in relatedOperationUserComments on
                         relatedOperationIds.IndexOf(relatedOperationId) equals
                         relatedOperationUserComments.IndexOf(relatedOperationUserComment)
                     join relatedOperationRelationshipId in relatedOperationRelationshipIds on
                         relatedOperationIds.IndexOf(relatedOperationId) equals
                         relatedOperationRelationshipIds.IndexOf(relatedOperationRelationshipId)
                     join relationShipRoleThisOperationId in relationShipRoleThisOperationIds on
                         relatedOperationIds.IndexOf(relatedOperationId) equals
                         relationShipRoleThisOperationIds.IndexOf(relationShipRoleThisOperationId)
                     select new RelatedOperationRowViewModel
                     {
                         RowId = int.Parse(relatedOperationId.Value),
                         OperationRelatedId = int.Parse(operationRelatedId.Value),
                         RelationshipTypeId = int.Parse(operationRelationTypeId.Value),
                         RelationshipRoleId = int.Parse(operationRelatedRoleId.Value),
                         OperationRelationshipId = int.Parse(relatedOperationRelationshipId.Value),
                         StartDate = DateTime.Parse(relatedOperationStartDate.Value),
                         EndDate =
                             relatedOperationExpirationDate.Value != string.Empty ?
                                 DateTime.Parse(relatedOperationExpirationDate.Value) :
                                 DateTime.MinValue,
                         UserCommentId = int.Parse(relatedOperationUserComentId.Value),
                         Comments = relatedOperationUserComment.Value,
                         RelationShipRoleThisOperationId = int.Parse(relationShipRoleThisOperationId.Value)
                     }).ToList();

                var deletedRelatedInView = clientFieldData
                    .FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) &&
                        o.Name.Equals(DELETED_RELATIONSHIPS));

                viewModel.BasicData.DeletedRelatedOperations = deletedRelatedInView
                    .ConvertToListInt(true);

                var fundOperationId = clientFieldData
                    .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                        o.Name.Equals(FUND_OPERATION_ID))
                    .ToList();
                var financingType = clientFieldData
                    .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                        o.Name.Equals(FINANCING_TYPE_ID))
                    .ToList();
                var funds = clientFieldData
                    .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                        o.Name.Equals(FUND_ID))
                    .ToList();
                var fundsTbd = clientFieldData
                    .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                        o.Name.Equals(FUND_TBD))
                    .ToList();
                var fundAmount = clientFieldData
                    .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(US_AMOUNT))
                    .ToList();
                var donors = clientFieldData
                    .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                        o.Name.Equals(SELECTED_PSG_DONORS))
                    .ToList();

                viewModel.ExpectedIDB =
                    (from financing in financingType
                     join fund in funds on financingType.IndexOf(financing) equals funds.IndexOf(fund)
                     join fundTbd in fundsTbd on financingType.IndexOf(financing) equals fundsTbd.IndexOf(fundTbd)
                     join amount in fundAmount on financingType.IndexOf(financing) equals fundAmount.IndexOf(amount)
                     join fundId in fundOperationId on financingType.IndexOf(financing) equals
                         fundOperationId.IndexOf(fundId)
                     join donor in donors
                        on financingType.IndexOf(financing) equals donors.IndexOf(donor) into tmpDonor
                     from donor in tmpDonor.DefaultIfEmpty()
                     select new ExpectedIDBViewModel
                     {
                         IdFundOperation = int.Parse(fundId.Value),
                         FinancingTypeId = int.Parse(financing.Value),
                         FundId = int.Parse(fund.Value),
                         FundsTbd =
                             fundTbd.Value != null ?
                                 fundTbd.Value.Split(',').Select(int.Parse).ToList() :
                                 new List<int>(),
                         Amount = decimal.Parse(amount.Value),
                         Donors =
                            donor == null || string.IsNullOrEmpty(donor.Value) ?
                                string.Empty : donor.Value
                     }).ToList();

                var countryRelatedIds = clientFieldData
                    .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                        o.Name.Equals(COUNTRY_RELATED_ID))
                    .ToList();
                var associatedCountriesRoles = clientFieldData
                    .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                        o.Name.Equals(ASSOCIATED_COUNTRIES_ROLE))
                    .ToList();

                var associatedCountriesCountries = clientFieldData
                    .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(LIST_COUNTRY))
                    .ToList();

                viewModel.ResponsibilityData.CountryRelated =
                    (from countryRelatedId in countryRelatedIds
                     join associatedCountriesRole in associatedCountriesRoles on
                         countryRelatedIds.IndexOf(countryRelatedId) equals
                         associatedCountriesRoles.IndexOf(associatedCountriesRole)
                     join associatedCountriesCountry in associatedCountriesCountries on
                         countryRelatedIds.IndexOf(countryRelatedId) equals
                         associatedCountriesCountries.IndexOf(associatedCountriesCountry)
                     select new AssociatedCountriesRowViewModel
                     {
                         RowId = int.Parse(countryRelatedId.Value),
                         Role = int.Parse(associatedCountriesRole.Value),
                         Countries = associatedCountriesCountry.Value != null ?
                             associatedCountriesCountry.Value.Split(',').Select(int.Parse).ToList() :
                             new List<int>()
                     }).ToList();

                var institutionRelatedIds = clientFieldData
                    .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                        o.Name.Equals(INSTITUTION_RELATED_ID))
                    .ToList();
                var associatedInstitutionsRoles = clientFieldData
                    .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                        o.Name.Equals(ASSOCIATED_INSTITUTIONS_ROLE))
                    .ToList();
                var associatedInstitutions = clientFieldData
                    .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                        o.Name.Equals(ASSOCIATED_INSTITUTIONS))
                    .ToList();

                viewModel.ResponsibilityData.AssociatedInstitutions =
                    (from institutionRelatedId in institutionRelatedIds
                     join associatedInstitutionsRole in associatedInstitutionsRoles on
                         institutionRelatedIds.IndexOf(institutionRelatedId) equals
                         associatedInstitutionsRoles.IndexOf(associatedInstitutionsRole)
                     join associatedInstitution in associatedInstitutions on
                         institutionRelatedIds.IndexOf(institutionRelatedId) equals
                         associatedInstitutions.IndexOf(associatedInstitution)
                     select new AssociatedInstitutionsRowViewModel
                     {
                         RowId = int.Parse(institutionRelatedId.Value),
                         Role = int.Parse(associatedInstitutionsRole.Value),
                         Institutions = int.Parse(associatedInstitution.Value)
                     }).ToList();

                var organizationalUnitRole = clientFieldData
                    .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                        o.Name.Equals(ROLE_ORGANIZATIONAL_UNIT))
                    .ToList();
                var organizationalUnit = clientFieldData
                    .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                        o.Name.Equals(ORGANIZATIONAL_UNIT))
                    .ToList();
                var organizationalUnitId = clientFieldData
                    .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                        o.Name.Equals(ORGANIZATIONAL_UNIT_ROW_ID))
                    .ToList();

                viewModel.ResponsibilityData.OrganizationalUnit =
                    (from role in organizationalUnitRole
                     join organization in organizationalUnit on
                         organizationalUnitRole.IndexOf(role) equals
                         organizationalUnit.IndexOf(organization)
                     join id in organizationalUnitId on organizationalUnitRole.IndexOf(role) equals
                         organizationalUnitId.IndexOf(id)
                     select new OrganizationalUnitRowViewModel
                     {
                         RowId = int.Parse(id.Value),
                         Role = int.Parse(role.Value),
                         OrganizationalUnit = int.Parse(organization.Value)
                     }).ToList();

                if (viewModel.BasicData.OperationType == igrId ||
                    viewModel.BasicData.OperationType == lonId)
                {
                    var message = ValidateWorkflow(
                        viewModel,
                        clientFieldData,
                        igrId,
                        lonId,
                        administeredById,
                        hqId,
                        responsibleId,
                        groups,
                        bankwideId,
                        sgId,
                        nsgId,
                        iicId);

                    if (!string.IsNullOrEmpty(message))
                        return message;
                }
            }

            var codeType = GetCodeType(editPemission, isCreator);

            if (codeType == string.Empty)
            {
                return string.Empty;
            }

            viewModel.ResponsibilityData.OperationTeams = viewModel.ResponsibilityData.OperationTeams
                .GetPrimaryFilterOpRel(codeType, editPemission);

            var operationTeamDataIds = clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(OPERATION_TEAM_DATA_ID)).ToList();
            operationTeamDataIds.Insert(0, new ClientFieldData
            {
                Id = DEFAULT_ID,
                Name = OPERATION_TEAM_DATA_ID,
                Value = DEFAULT_VALUE
            });
            var operationTeamsNames = clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(OPERATION_TEAMS_NAME)).ToList();
            operationTeamsNames.Insert(0, new ClientFieldData
            {
                Id = DEFAULT_ID,
                Name = OPERATION_TEAMS_NAME,
                Value = IDBContext.Current.UserName
            });
            var operationTeamsRoles = clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(OPERATION_TEAMS_ROLE)).ToList();
            operationTeamsRoles.Insert(0, new ClientFieldData
            {
                Id = DEFAULT_ID,
                Name = OPERATION_TEAMS_ROLE,
                Value = roleCreationId.ToString()
            });
            var userNamesTeam = clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(USERNAME_TEAM)).ToList();
            userNamesTeam.Insert(0, new ClientFieldData
            {
                Id = DEFAULT_ID,
                Name = USERNAME_TEAM,
                Value = IDBContext.Current.UserName
            });
            var organizationalUnitsTeam = clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(ORGANIZATIONAL_UNIT_TEAM)).ToList();
            organizationalUnitsTeam.Insert(0, new ClientFieldData
            {
                Id = DEFAULT_ID,
                Name = ORGANIZATIONAL_UNIT_TEAM,
                Value = DEFAULT_VALUE
            });
            var operationTeamCodeTypes =
                clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                o.Name.Equals(OPERATION_TEAM_CODE_TYPE) &&
                (o.Value == OPUSGlobalValues.MULTIPLE_PERMISSIONS_IN_ROLE ||
                CanEditRole(o.Value, editPemission, isCreator))).ToList();
            operationTeamCodeTypes.Insert(0, new ClientFieldData
            {
                Id = DEFAULT_ID,
                Name = OPERATION_TEAM_CODE_TYPE,
                Value = DEFAULT_VALUE
            });

            var listOperationTeamView = (from operationTeamDataId in operationTeamDataIds
                                         join operationTeamsName in operationTeamsNames
                                             on operationTeamDataIds.IndexOf(operationTeamDataId)
                                             equals operationTeamsNames.IndexOf(operationTeamsName)
                                         join operationTeamsRole in operationTeamsRoles
                                             on operationTeamDataIds.IndexOf(operationTeamDataId)
                                             equals operationTeamsRoles.IndexOf(operationTeamsRole)
                                         join userNameTeam in userNamesTeam
                                             on operationTeamDataIds.IndexOf(operationTeamDataId)
                                             equals userNamesTeam.IndexOf(userNameTeam)
                                         join organizationalUnitTeam in organizationalUnitsTeam
                                             on operationTeamDataIds.IndexOf(operationTeamDataId)
                                             equals organizationalUnitsTeam.IndexOf(organizationalUnitTeam)
                                         join operationTeamCodeType in operationTeamCodeTypes
                                             on operationTeamDataIds.IndexOf(operationTeamDataId)
                                             equals operationTeamCodeTypes.IndexOf(operationTeamCodeType)
                                         select new OperationTeamRowViewModel
                                         {
                                             RowId = int.Parse(operationTeamDataId.Value),
                                             Name = operationTeamsName.Value,
                                             Role = int.Parse(operationTeamsRole.Value),
                                             UserName = userNameTeam.Value,
                                             OrganizationalUnitId = int.Parse(organizationalUnitTeam.Value),
                                             CodeRoleType = operationTeamCodeType.Value
                                         }).ToList();

            foreach (var teamRow in listOperationTeamView)
            {
                viewModel.ResponsibilityData.OperationTeams.Add(teamRow);
            }

            var isSaveOnly = clientFieldData
                .FirstOrDefault(a => a.Name.Equals(IS_SAVE_ONLY));

            if (isSaveOnly != null)
            {
                bool result;
                bool.TryParse(isSaveOnly.Value, out result);

                viewModel.IsSaveOnly = result;
            }

            return string.Empty;
        }
        #endregion

        #region Deliverable Module

        public static void UpdateDeliverableViewModel(this DeliverableViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            var deliverableIds = clientFieldData.Where(
                o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(DELIVERABLE_ID)).ToList();
            var deliverableTypes = clientFieldData.Where(
                o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(DELIVERABLE_TYPE)).ToList();
            var deliverablesYearsPlanned = clientFieldData.Where(
                o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(DELIVERABLES_YEAR_PLANNED)).ToList();
            var deliverablesResponsibles = clientFieldData.Where(
                o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(DELIVERABLES_RESPONSIBLE)).ToList();
            var deliverablesCountries = clientFieldData.Where(
                o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(DELIVERABLES_COUNTRY)).ToList();
            var planedDates = clientFieldData.Where(
                o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(PLANED_DATE)).ToList();
            var completedDates = clientFieldData.Where(
                o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(COMPLETED_DATE)).ToList();
            var documentNumbers = clientFieldData.Where(
                o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(DOCUMENT_NUMBER_HIDDEN)).ToList();
            var deliverableNames = clientFieldData.Where(
                o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(DELIVERABLE_NAME)).ToList();
            var deliverableIsDismisses = clientFieldData.Where(
                o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(IS_DISMISS)).ToList();
            var deliverableDocumentId = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                    o.Name.Equals(DELIVERABLE_DOCUMENT_ID))
                .ToList();

            viewModel.DeliverableData = new List<DeliverableRowViewModel>();

            if (deliverableIds.Count() > 0)
            {
                for (int i = 0; i < deliverableIds.Count(); i++)
                {
                    var deliverable = new DeliverableRowViewModel
                    {
                        RowId = int.Parse(deliverableIds.ElementAt(i).Value),
                        Type = int.Parse(deliverableTypes.ElementAt(i).Value),
                        YearPlanned = int.Parse(deliverablesYearsPlanned.ElementAt(i).Value),
                        Responsible = int.Parse(deliverablesResponsibles.ElementAt(i).Value),
                        Country = int.Parse(deliverablesCountries.ElementAt(i).Value),
                        PlanedDate = DateTime.Parse(planedDates.ElementAt(i).Value),
                        CompletedDate = completedDates.ElementAt(i).Value != string.Empty ?
                            DateTime.Parse(completedDates.ElementAt(i).Value) : DateTime.MinValue,
                        DocumentNumber = documentNumbers.ElementAt(i).Value != string.Empty ?
                            documentNumbers.ElementAt(i).Value : string.Empty,
                        Name = deliverableNames.ElementAt(i).Value,
                        IsDismiss = bool.Parse(deliverableIsDismisses.ElementAt(i).Value)
                    };

                    int documentId;

                    int.TryParse(deliverableDocumentId.ElementAt(i).Value, out documentId);

                    if (documentId > 0)
                    {
                        deliverable.DocumentId = documentId;
                    }
                    else
                    {
                        deliverable.DocumentId = null;
                    }

                    viewModel.DeliverableData.Add(deliverable);
                }
            }

            foreach (var deliverable in viewModel.DeliverableData)
            {
                var fields = clientFieldData.Where(x => x.Id == deliverable.RowId.ToString());
                var field = fields.FirstOrDefault(x => x.Name == "PublicationName");

                if (field != null && !string.IsNullOrWhiteSpace(field.Value))
                {
                    deliverable.Publication = new PublicationViewModel()
                    {
                        CanPublish = true,
                        PublicationName = field.ConvertToString()
                    };

                    field = fields.FirstOrDefault(x => x.Name == "PublicationId");
                    deliverable.Publication.PublicationId = field.ConvertToInt(true);

                    field = fields.FirstOrDefault(x => x.Name == "CoPublication");
                    deliverable.Publication.CoPublication = field.ConvertToBool() == true;

                    field = fields.FirstOrDefault(x => x.Name == "PublicationDate");
                    deliverable.Publication.PublicationDate = field.ConvertToDateTime();

                    field = fields.FirstOrDefault(x => x.Name == "MainLanguaje");
                    deliverable.Publication.MainLanguageId = field.ConvertToInt(true);
                }
            }
        }

        #endregion

        #region Operation Data

        #region BasicData
        public static string UpdateBasicDataViewModel(
            this BasicDataViewModel viewModel,
            ClientFieldData[] clientFieldData,
            int sgId,
            int cndId,
            int investementId,
            int uccfId)
        {
            var operationYear = clientFieldData.FirstOrDefault(o =>
                !string.IsNullOrWhiteSpace(o.Name) &&
                o.Name.Equals(OPERATION_YEAR));

            if (operationYear != null && !string.IsNullOrEmpty(operationYear.Value))
            {
                viewModel.OperationYear = Convert.ToInt32(operationYear.Value);
            }
            else
            {
                viewModel.OperationYear = null;
            }

            var countryGroup = clientFieldData.FirstOrDefault(o =>
                !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(COUNTRY_GROUP));

            if (countryGroup != null)
            {
                if (!string.IsNullOrEmpty(countryGroup.Value))
                {
                    viewModel.CountryGroup = countryGroup.Value;
                }
                else
                {
                    viewModel.CountryGroup = null;
                }
            }

            var sector = clientFieldData.FirstOrDefault(o =>
                !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(SECTOR));

            if (sector != null)
            {
                int sectorId;

                if (int.TryParse(sector.Value, out sectorId))
                {
                    viewModel.Sector = sectorId;
                }
                else
                {
                    viewModel.Sector = null;
                }
            }

            var subSector = clientFieldData.FirstOrDefault(o =>
                !string.IsNullOrWhiteSpace(o.Name) && o.Name.ToLower().Equals(SUB_SECTOR));

            if (subSector != null && sector != null)
            {
                if (!string.IsNullOrEmpty(subSector.Value))
                {
                    viewModel.Subsector = Convert.ToInt32(subSector.Value);
                }
                else if (!string.IsNullOrEmpty(sector.Value))
                {
                    viewModel.Subsector = Convert.ToInt32(sector.Value);
                }
                else
                {
                    viewModel.Subsector = null;
                }
            }

            var mbfCode = clientFieldData.FirstOrDefault(o =>
                !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(MBF_CODE));

            if (mbfCode != null)
            {
                viewModel.MbfCode = mbfCode.Value == string.Empty ?
                    0 : Convert.ToInt32(mbfCode.Value);
            }

            var isPmrRequired = clientFieldData.FirstOrDefault(o =>
                !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(IS_PMR_NOT_REQUIRED));

            if (isPmrRequired != null)
            {
                viewModel.IsPmrNotRequired = Convert.ToBoolean(isPmrRequired.Value);
            }

            var isObjectiveReformulated = clientFieldData.FirstOrDefault(o =>
                !string.IsNullOrWhiteSpace(o.Name) &&
                o.Name.Equals(IS_OBJECTIVE_REFORMULATED));

            if (isObjectiveReformulated != null)
            {
                viewModel.IsObjectiveReformulated = Convert.ToBoolean(
                    isObjectiveReformulated.Value);
            }

            var reformulationApprovalDate = clientFieldData.FirstOrDefault(o =>
                !string.IsNullOrWhiteSpace(o.Name) &&
                o.Name.Equals(REFORMULATION_APPROVAL_DATE));

            if (reformulationApprovalDate != null &&
                !reformulationApprovalDate.Value.Trim().Equals(string.Empty))
            {
                viewModel.ReformulationApprovalDate = Convert.ToDateTime(
                    reformulationApprovalDate.Value);
            }

            var category = clientFieldData.FirstOrDefault(o =>
                !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(CATEGORY));

            if (category != null)
            {
                if (!string.IsNullOrEmpty(category.Value))
                {
                    viewModel.Category = Convert.ToInt32(category.Value);
                }
                else
                {
                    viewModel.Category = null;
                }
            }

            var countryDepartmentId = clientFieldData.FirstOrDefault(o =>
                !string.IsNullOrWhiteSpace(o.Name) &&
                o.Name.Equals(COUNTRY_DEPARTMENT_ID));

            if (countryDepartmentId != null)
            {
                if (!string.IsNullOrEmpty(countryDepartmentId.Value))
                {
                    viewModel.CountryDepartmentId = Convert.ToInt32(
                        countryDepartmentId.Value);
                }
                else
                {
                    viewModel.CountryDepartmentId = null;
                }
            }

            var operationsEn = clientFieldData.FirstOrDefault(o =>
                !string.IsNullOrWhiteSpace(o.Name) &&
                o.Name.Equals(OPERATION_NAME_ENGLISH));

            if (operationsEn != null)
            {
                viewModel.OperationNameEnglish = operationsEn.Value.Trim();
            }

            var operationsEs = clientFieldData.FirstOrDefault(o =>
                !string.IsNullOrWhiteSpace(o.Name) &&
                o.Name.Equals(OPERATION_NAME_SPANISH));

            if (operationsEs != null)
            {
                viewModel.OperationNameSpanish = operationsEs.Value.Trim();
            }

            var operationsFr = clientFieldData.FirstOrDefault(o =>
                !string.IsNullOrWhiteSpace(o.Name) &&
                o.Name.Equals(OPERATION_NAME_FRENCH));

            if (operationsFr != null)
            {
                viewModel.OperationNameFrench = operationsFr.Value.Trim();
            }

            var operationsPo = clientFieldData.FirstOrDefault(o =>
                !string.IsNullOrWhiteSpace(o.Name) &&
                o.Name.Equals(OPERATION_NAME_PORTUGUESE));

            if (operationsPo != null)
            {
                viewModel.OperationNamePortuguese = operationsPo.Value.Trim();
            }

            var objetivesEn = clientFieldData.FirstOrDefault(o =>
                !string.IsNullOrWhiteSpace(o.Name) &&
                o.Name.Equals(OPERATION_OBJECTIVE_ENGLISH));

            if (objetivesEn != null)
            {
                viewModel.OperationObjectiveEnglish = objetivesEn.Value.Trim();
            }

            var objetivesEs = clientFieldData.FirstOrDefault(o =>
                !string.IsNullOrWhiteSpace(o.Name) &&
                o.Name.Equals(OPERATION_OBJECTIVE_SPANISH));

            if (objetivesEs != null)
            {
                viewModel.OperationObjectiveSpanish = objetivesEs.Value.Trim();
            }

            var objetivesPt = clientFieldData.FirstOrDefault(o =>
                !string.IsNullOrWhiteSpace(o.Name) &&
                o.Name.Equals(OPERATION_OBJECTIVE_PORTUGUESE));

            if (objetivesPt != null)
            {
                viewModel.OperationObjectivePortuguese = objetivesPt.Value.Trim();
            }

            var objetivesFr = clientFieldData.FirstOrDefault(o =>
                !string.IsNullOrWhiteSpace(o.Name) &&
                o.Name.Equals(OPERATION_OBJECTIVE_FRENCH));

            if (objetivesFr != null)
            {
                viewModel.OperationObjectiveFrench = objetivesFr.Value.Trim();
            }

            var pepRmNotIntegrated = clientFieldData.FirstOrDefault(o =>
                !string.IsNullOrWhiteSpace(o.Name) &&
                o.Name.Equals(PEP_NOT_INTEGRATED));

            if (pepRmNotIntegrated != null)
            {
                var pepRmNotIntegratedBoolean = Convert.ToBoolean(pepRmNotIntegrated.Value);
                if (viewModel.PlanningInstrumentsIntegrated != pepRmNotIntegratedBoolean)
                {
                    viewModel.PlanningInstrumentsIntegrated = !pepRmNotIntegratedBoolean;
                    viewModel.LastModifiedIntegration = DateTime.Today;
                }
            }
            else
            {
                viewModel.PlanningInstrumentsIntegrated = null;
            }

            var comments = clientFieldData.Where(o =>
                !string.IsNullOrWhiteSpace(o.Name) &&
                o.Name.Equals(COMMENT_COMMENT)).ToList();
            var commentIds = clientFieldData.Where(o =>
                !string.IsNullOrWhiteSpace(o.Name) &&
                o.Name.Equals(COMMENT_COMMENT_ID)).ToList();

            if (commentIds.Count != 0 && comments.Count != 0)
            {
                viewModel.Comments = (from commentId in commentIds
                                      join comment in comments
                                          on commentIds.IndexOf(commentId) equals comments.IndexOf(comment)
                                      select new GenericCommentsViewModel
                                      {
                                          CommentId = int.Parse(commentId.Value),
                                          Comment = comment.Value
                                      }).ToList();
            }
            else
            {
                viewModel.Comments = new List<GenericCommentsViewModel>();
            }

            viewModel.RelatedOperations = ResolveRowsOfRelatedOperations(clientFieldData).ToList();

            var deletedRelatedInView = clientFieldData
                .FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) &&
                    o.Name.Equals(DELETED_RELATIONSHIPS));

            viewModel.DeletedRelatedOperations = deletedRelatedInView.ConvertToListInt(true);

            var lendingType = clientFieldData.FirstOrDefault(o =>
                !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(AttributeCode.LENDING_TYPE));
            var loanInstrument = clientFieldData.FirstOrDefault(o =>
                !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(AttributeCode.SG_INSTRUMENTS));
            var loanModality = clientFieldData.FirstOrDefault(o =>
                !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(AttributeCode.LON_MODALITY));

            int instrumentId = 0;

            if (loanInstrument != null)
            {
                int.TryParse(loanInstrument.Value, out instrumentId);
            }

            int modalityId = 0;

            if (loanModality != null)
            {
                int.TryParse(loanModality.Value, out modalityId);
            }

            int lendingId = 0;

            if (lendingType != null)
            {
                int.TryParse(lendingType.Value, out lendingId);
            }

            if (lendingId == sgId && modalityId == cndId && instrumentId == investementId &&
                    !viewModel.RelatedOperations.Any(r => r.RelationshipTypeId.Equals(uccfId)))
            {
                return Localization
                    .GetText("OP.CR.CreationForm.Message.MandatoryRelationshipUCCF");
            }

            return string.Empty;
        }
        #endregion

        #region ResponsabilityData

        public static string UpdateResponsabilityDataViewModel(
            this ResponsabilityDataViewModel viewModel,
            ClientFieldData[] clientFieldData,
            Operation operation,
            IList<DivisionOpus> groups,
            int responsibleId)
        {
            var editPemissionResponsability = IDBContext.Current
                .HasPermission(Permission.RESPONSIBILITY_DATA_WRITE);

            if (editPemissionResponsability)
            {
                var opType = OperationTypeHelper.GetOperationMainType(operation.OperationId);

                var responsibleUnitIds = clientFieldData.Where(o => o.Name.Equals(RESPONSIBLE_UNIT_ID)).ToList();
                var responsibleUnitsRoles =
                    clientFieldData.Where(o => o.Name.Equals(RESPONSIBLE_UNITS_ROLE)).ToList();
                var responsibleUnitsOrganizationalUnits =
                    clientFieldData.Where(o => o.Name.Equals(RESPONSIBLE_UNITS_ORGANIZATIONAL_UNIT)).ToList();
                var responsibleUnitsEffortsInDays =
                    clientFieldData.Where(o => o.Name.Equals(RESPONSIBLE_UNITS_EFFORT_IN_DAYS)).ToList();
                var effort = 0;

                viewModel.ResponsibleUnits =
                    (from responsibleUnitsRole in responsibleUnitsRoles
                     join responsibleUnitsOrganizationalUnit in responsibleUnitsOrganizationalUnits on
                         responsibleUnitsRoles.IndexOf(responsibleUnitsRole) equals
                         responsibleUnitsOrganizationalUnits.IndexOf(responsibleUnitsOrganizationalUnit)
                     join responsibleUnitsEffortInDays in responsibleUnitsEffortsInDays on
                         responsibleUnitsRoles.IndexOf(responsibleUnitsRole) equals
                         responsibleUnitsEffortsInDays.IndexOf(responsibleUnitsEffortInDays)
                     join responsibleUnitId in responsibleUnitIds on
                         responsibleUnitsRoles.IndexOf(responsibleUnitsRole) equals
                         responsibleUnitIds.IndexOf(responsibleUnitId)
                     select new ResponsibleRowViewModel
                     {
                         RowId = int.Parse(responsibleUnitId.Value),
                         Role = int.Parse(responsibleUnitsRole.Value),
                         OrganizationalUnit = int.Parse(responsibleUnitsOrganizationalUnit.Value),
                         EffortInDays = int.TryParse(responsibleUnitsEffortInDays.Value, out effort) ?
                             effort : default(int?)
                     }).ToList();

                if (opType == OperationType.LON ||
                    (opType == OperationType.IGR &&
                        operation.FundOperation != null &&
                        operation.FundOperation.Sum(o => o.UsFundAmount) > K2HelperOpus.MaxFund))
                {
                    var lending = operation.FormAttributeOperation
                        .Where(o => o.FormAttribute.Code == AttributeCode.LENDING_TYPE);
                    var lendingCode = lending != null && lending.Any() ?
                        lending.First().ListValue.Code : string.Empty;

                    var responsible = viewModel.ResponsibleUnits.First(o => o.Role.Equals(responsibleId));
                    var isDivision = groups
                        .Any(o => o.Group.Equals(DivisionOpusGroup.DIVISION) &&
                            o.DivisionId.Equals(responsible.OrganizationalUnit));
                    var isMif = groups
                        .Any(o => o.Group.Equals(DivisionOpusGroup.MIF) &&
                            o.DivisionId.Equals(responsible.OrganizationalUnit));
                    var isVpc = groups
                        .Any(o => o.Group.Equals(DivisionOpusGroup.VPC) &&
                            o.DivisionId.Equals(responsible.OrganizationalUnit));

                    var isNsgIic = false;

                    if (lendingCode == AttributeValue.NSG)
                    {
                        var categorization = operation.FormAttributeOperation
                            .Where(o => o.FormAttribute.Code == AttributeCode.INSTRUMENTS);
                        isNsgIic = categorization != null && categorization.Any() &&
                            categorization.First().ListValue.Code == AttributeValue.TYPE_INSTRUMENT_IAIC_TC;
                    }

                    var isIic = groups
                        .Any(o => o.DivisionCode.Equals(OrgUnitCode.IIC) &&
                            o.DivisionId.Equals(responsible.OrganizationalUnit));

                    var message = ValidateOrgUnits(
                        lendingCode == AttributeValue.SG,
                        lendingCode == AttributeValue.NSG,
                        isDivision,
                        isMif,
                        isNsgIic,
                        isIic,
                        isVpc,
                        opType);

                    if (!string.IsNullOrEmpty(message))
                        return message;
                }

                var countryRelatedIds = clientFieldData
                    .Where(o => o.Name.Equals(COUNTRY_RELATED_ID)).ToList();
                var associatedCountriesRoles = clientFieldData
                    .Where(o => o.Name.Equals(ASSOCIATED_COUNTRIES_ROLE)).ToList();
                var associatedCountriesCountries = clientFieldData
                    .Where(o => o.Name.Equals(LIST_COUNTRY)).ToList();

                viewModel.AssociatedCountries =
                    (from countryRelatedId in countryRelatedIds
                     join associatedCountriesRole in associatedCountriesRoles on
                         countryRelatedIds.IndexOf(countryRelatedId) equals
                         associatedCountriesRoles.IndexOf(associatedCountriesRole)
                     join associatedCountriesCountry in associatedCountriesCountries on
                         countryRelatedIds.IndexOf(countryRelatedId) equals
                         associatedCountriesCountries.IndexOf(associatedCountriesCountry)
                     select new AssociatedCountriesRowViewModel
                     {
                         RowId = int.Parse(countryRelatedId.Value),
                         Role = int.Parse(associatedCountriesRole.Value),
                         Countries = associatedCountriesCountry.Value != null ?
                             associatedCountriesCountry.Value.Split(SEPARATOR).Select(int.Parse).ToList() :
                             new List<int>()
                     }).ToList();

                var institutionRelatedIds =
                    clientFieldData.Where(o => o.Name.Equals(INSTITUTION_RELATED_ID)).ToList();
                var associatedInstitutionsRoles =
                    clientFieldData.Where(o => o.Name.Equals(ASSOCIATED_INSTITUTIONS_ROLE)).ToList();
                var associatedInstitutions =
                    clientFieldData.Where(o => o.Name.Equals(ASSOCIATED_INSTITUTIONS)).ToList();

                viewModel.AssociatedInstitutions =
                    (from institutionRelatedId in institutionRelatedIds
                     join associatedInstitutionsRole in associatedInstitutionsRoles on
                         institutionRelatedIds.IndexOf(institutionRelatedId) equals
                         associatedInstitutionsRoles.IndexOf(associatedInstitutionsRole)
                     join associatedInstitution in associatedInstitutions on
                         institutionRelatedIds.IndexOf(institutionRelatedId) equals
                         associatedInstitutions.IndexOf(associatedInstitution)
                     select new AssociatedInstitutionsRowViewModel
                     {
                         RowId = int.Parse(institutionRelatedId.Value),
                         Role = int.Parse(associatedInstitutionsRole.Value),
                         Institutions = int.Parse(associatedInstitution.Value)
                     }).ToList();
            }

            var codeType = GetCodeType(editPemissionResponsability);

            if (codeType != string.Empty)
            {
                viewModel.OperationTeams =
                    viewModel.OperationTeams.GetPrimaryFilterOpRel(codeType, editPemissionResponsability);
                var operationTeamDataIds =
                    clientFieldData.Where(o => o.Name.Equals(OPERATION_TEAM_DATA_ID)).ToList();

                foreach (var row in operationTeamDataIds)
                {
                    var name = clientFieldData.FirstOrDefault(
                        o => !string.IsNullOrEmpty(o.Id) &&
                             o.Id == row.Id &&
                             !string.IsNullOrWhiteSpace(o.Name) &&
                             o.Name.Equals(OPERATION_TEAMS_NAME));

                    var role = clientFieldData.FirstOrDefault(
                        o => !string.IsNullOrEmpty(o.Id) &&
                             o.Id == row.Id &&
                             !string.IsNullOrWhiteSpace(o.Name) &&
                             o.Name.Equals(OPERATION_TEAMS_ROLE));

                    var userName = clientFieldData.FirstOrDefault(
                        o => !string.IsNullOrEmpty(o.Id) &&
                             o.Id == row.Id &&
                             !string.IsNullOrWhiteSpace(o.Name) &&
                             o.Name.Equals(USERNAME_TEAM));

                    var orgUnit = clientFieldData.FirstOrDefault(
                        o => !string.IsNullOrEmpty(o.Id) &&
                             o.Id == row.Id &&
                             !string.IsNullOrWhiteSpace(o.Name) &&
                             o.Name.Equals(ORGANIZATIONAL_UNIT_TEAM));

                    var codeRoleType = clientFieldData.FirstOrDefault(
                        o => !string.IsNullOrEmpty(o.Id) &&
                             o.Id == row.Id &&
                             !string.IsNullOrWhiteSpace(o.Name) &&
                             o.Name.Equals(OPERATION_TEAM_CODE_TYPE) &&
                             (o.Value == OPUSGlobalValues.MULTIPLE_PERMISSIONS_IN_ROLE ||
                              CanEditRole(o.Value, editPemissionResponsability)));

                    if (name != null &&
                        role != null &&
                        userName != null &&
                        orgUnit != null &&
                        codeRoleType != null)
                    {
                        viewModel.OperationTeams.Add(new OperationTeamRowViewModel
                        {
                            RowId = int.Parse(row.Value),
                            Name = name.Value,
                            Role = int.Parse(role.Value),
                            UserName = userName.Value,
                            OrganizationalUnitId = int.Parse(orgUnit.Value),
                            CodeRoleType = codeRoleType.Value
                        });
                    }
                }
            }

            return string.Empty;
        }

        #endregion

        #region Strategic Alignment

        public static StrategicAlignmentViewModel UpdateStrategicAlignmentViewModel(
            this StrategicAlignmentViewModel model,
            ClientFieldData[] formData)
        {
            ClientFieldData field;
            List<ClientFieldData> fields;

            bool isChecked;

            var impactValue = new List<string>();
            var outcomeValue = new List<string>();
            var outputValue = new List<string>();
            string justification;

            var newModel = new StrategicAlignmentViewModel()
            {
                IndicatorDataList = new List<SAIndicatorDataViewModel>(model.IndicatorDataList),
                CountryStrategyData = model.CountryStrategyData,
                CountryProgramData = model.CountryProgramData
            };

            field = formData.FirstOrDefault(x => x.Name == "isOperationApproved");
            newModel.IsOperationApproved = field.ConvertToBool() ?? false;

            field = formData.FirstOrDefault(x => x.Name == "isOperationPCRStarted");
            newModel.ISOperationPCRStarted = field.ConvertToBool() ?? false;

            field = formData.FirstOrDefault(x => x.Name == "isERMCompleted");
            newModel.IsERMCompleted = field.ConvertToBool() ?? false;

            field = formData.FirstOrDefault(x => x.Name == RESULT_MATRIX_ID);
            newModel.ResultMatrixId = field.ConvertToInt();

            if (!newModel.ISOperationPCRStarted)
            {
                newModel.IndicatorDataList.Clear();

                foreach (var indicatorData in model.IndicatorDataList)
                {
                    var fieldsForIndicator = formData.Where(
                        x => !x.ExtraData.Any() && x.Id == indicatorData.IndicatorId.ToString());

                    field = fieldsForIndicator.FirstOrDefault(x => x.Name == INDICATOR_CHECK);
                    isChecked = GetBoolValue(field, false);

                    if (!indicatorData.IsValid && !isChecked)
                    {
                        newModel.IndicatorDataList.Add(indicatorData);
                    }
                    else if (indicatorData.IsValid)
                    {
                        field = fieldsForIndicator.FirstOrDefault(x => x.Name == IMPACT_INDICATORS_COMBO);
                        impactValue = ConvertToListString(field);

                        field = fieldsForIndicator.FirstOrDefault(x => x.Name == OUTCOME_INDICATORS_COMBO);
                        outcomeValue = ConvertToListString(field);

                        field = fieldsForIndicator.FirstOrDefault(x => x.Name == OUTPUTS_COMBO);
                        outputValue = ConvertToListString(field);

                        field = fieldsForIndicator.FirstOrDefault(x => x.Name == JUSTIFICATION);
                        justification = GetString(field);

                        indicatorData.IsChecked = isChecked;

                        indicatorData.ImpactIndicator.Clear();
                        indicatorData.ImpactIndicator = isChecked ? impactValue : new List<string>();
                        indicatorData.OutcomeIndicator.Clear();
                        indicatorData.OutcomeIndicator = isChecked ? outcomeValue : new List<string>();
                        indicatorData.Output.Clear();
                        indicatorData.Output = isChecked ? outputValue : new List<string>();
                        indicatorData.Justification = isChecked ? justification : string.Empty;
                        newModel.IndicatorDataList.Add(indicatorData);

                        var fieldsForSubindicator = formData.Where(x =>
                            x.Id == indicatorData.IndicatorId.ToString() &&
                            x.ExtraData.Any(y => y.Key == DATA_PERSIST_SUBINDICATOR_ID)).ToArray();
                        indicatorData.SubindicatorList.UpdateSASubindicatorViewModel(fieldsForSubindicator);
                    }
                }
            }

            newModel.IsOperationEswType = model.IsOperationEswType;
            newModel.CountryStrategyData = new SACountryStrategyViewModel();

            if (!newModel.IsOperationEswType)
            {
                field = formData.FirstOrDefault(x => x.Name == "objectivesAligned");
                fields = formData.Where(x => x.Name == "inputPercent").ToList();
                newModel.CountryStrategyData.ObjectivesAlignedList.Clear();
                newModel.CountryStrategyData.ObjectivesNotAlignedList.Clear();
                newModel.CountryStrategyData.ExpiredObjectivesToRemove.Clear();

                var expiredOperationObjectiveIds = formData.Where(x => x.Name == "expired-objective").ToList();
                foreach (var input in expiredOperationObjectiveIds)
                {
                    if (input.Value == "False")
                    {
                        newModel.CountryStrategyData.ExpiredObjectivesToRemove.Add(input.Id.ConvertToInt());
                    }
                }

                if (field != null)
                {
                    if (field.Value == "aligned")
                    {
                        newModel.CountryStrategyData.IsObjecticeAligned = true;

                        field = formData.FirstOrDefault(x => x.Name == "objectives");
                        newModel.CountryStrategyData.ObjectivesAlignedIdList = ConvertToListString(field);

                        var list = newModel.CountryStrategyData.ObjectivesAlignedIdList;
                        foreach (var input in fields)
                        {
                            if (list.Contains(input.Id))
                            {
                                var objectiveModel = new SACountryStrategyObjectiveViewMode();
                                objectiveModel.ObjectiveId = input.Id.ConvertToInt();
                                objectiveModel.IsObjectiveAligned = true;
                                newModel.CountryStrategyData.ObjectivesAlignedList.Add(objectiveModel);
                            }
                        }

                        //newModel.CountryProgramData.EnAreaValue = string.Empty;
                        //newModel.CountryProgramData.EsAreaValue = string.Empty;
                        //newModel.CountryProgramData.FrAreaValue = string.Empty;
                        //newModel.CountryProgramData.PtAreaValue = string.Empty;
                    }

                    if (field.Value == "not-aligned")
                    {
                        newModel.CountryStrategyData.IsObjecticeAligned = false;

                        field = formData.FirstOrDefault(x => x.Name == "objectivesNotAligned");
                        newModel.CountryStrategyData.ObjectivesNotAligned = ConvertToString(field);
                        newModel.CountryStrategyData.ObjectivesNotAlignedIdList = ConvertToListString(field);

                        var list = newModel.CountryStrategyData.ObjectivesNotAlignedIdList;
                        foreach (var input in fields)
                        {
                            if (list.Contains(input.Id))
                            {
                                var objectiveModel = new SACountryStrategyObjectiveViewMode();
                                input.Id = input.Id.Substring(5, input.Id.Length - 5);
                                objectiveModel.ObjectiveId = input.Id.ConvertToInt();
                                objectiveModel.IsObjectiveAligned = false;
                                newModel.CountryStrategyData.ObjectivesNotAlignedList.Add(objectiveModel);
                            }
                        }

                        //var indicatorPipeline = formData.FirstOrDefault(x => x.Name == "hiddenIndicativePipelines");

                        //if (indicatorPipeline != null && indicatorPipeline.Value.Equals("False"))
                        //{
                        //    var enAreaValue = formData.FirstOrDefault(x => x.Name == "enRelevanceArea");
                        //    var esAreaValue = formData.FirstOrDefault(x => x.Name == "esRelevanceArea");
                        //    var frAreaValue = formData.FirstOrDefault(x => x.Name == "frRelevanceArea");
                        //    var ptAreaValue = formData.FirstOrDefault(x => x.Name == "ptRelevanceArea");

                        //    newModel.CountryProgramData.EnAreaValue =
                        //        enAreaValue != null && !string.IsNullOrEmpty(enAreaValue.Value)
                        //        ? enAreaValue.Value.TruncateRelevanceArea(1000)
                        //        : string.Empty;
                        //    newModel.CountryProgramData.EsAreaValue =
                        //        esAreaValue != null && !string.IsNullOrEmpty(esAreaValue.Value)
                        //        ? esAreaValue.Value.TruncateRelevanceArea(1000)
                        //        : string.Empty;
                        //    newModel.CountryProgramData.FrAreaValue =
                        //        frAreaValue != null && !string.IsNullOrEmpty(frAreaValue.Value)
                        //        ? frAreaValue.Value.TruncateRelevanceArea(1000)
                        //        : string.Empty;
                        //    newModel.CountryProgramData.PtAreaValue =
                        //        ptAreaValue != null && !string.IsNullOrEmpty(ptAreaValue.Value)
                        //        ? ptAreaValue.Value.TruncateRelevanceArea(1000) :
                        //        string.Empty;
                        //}
                        //else
                        //{
                        //    newModel.CountryProgramData.EnAreaValue = string.Empty;
                        //    newModel.CountryProgramData.EsAreaValue = string.Empty;
                        //    newModel.CountryProgramData.FrAreaValue = string.Empty;
                        //    newModel.CountryProgramData.PtAreaValue = string.Empty;
                        //}
                    }
                }
            }

            newModel.IsModuleDEM = model.IsModuleDEM;
            if (model.IsModuleDEM)
            {
                newModel.InformationDem = model.InformationDem;
                newModel.CountryStrategyData.InformationDem = GetAttributesDem(model.InformationDem);
                newModel.CountryProgramData.InformationDem = GetAttributesDem(model.InformationDem);

                newModel.CountryStrategyData.UserCommentCountryList = model.CountryStrategyData.UserCommentCountryList;
                newModel.CountryProgramData.UserCommentCountryProgramList = model.CountryProgramData.UserCommentCountryProgramList;

                var updateComments = formData.Where(o => o.Name.Equals("updateCommentTextIndicator"));
                var userCommentUpdateList = new List<UserCommentIndicatorViewModel>();
                if (updateComments != null)
                {
                    var editCommentsId = formData.Where(o => o.Name.Equals("updateCommentIdIndicator"));
                    var indicatorIdList = formData.Where(o => o.Name.Equals("updateIndicadorIdIndicator"));
                    var subIndicatorIdList = formData.Where(o => o.Name.Equals("updateSubIndicadorIdIndicator"));

                    for (int i = 0; i < updateComments.Count(); i++)
                    {
                        userCommentUpdateList.Add(new UserCommentIndicatorViewModel()
                        {
                            IndicatorId = Convert.ToInt32(indicatorIdList.ElementAt(i).Value),
                            SubIndicatorId = Convert.ToInt32(subIndicatorIdList.ElementAt(i).Value),
                            CommentId = Convert.ToInt32(editCommentsId.ElementAt(i).Value),
                            Comment = updateComments.ElementAt(i).Value.Trim()
                        });
                    }

                    foreach (var commentView in userCommentUpdateList)
                    {
                        foreach (var indicator in newModel.IndicatorDataList)
                        {
                            if (indicator.IndicatorId == commentView.IndicatorId)
                            {
                                foreach (var commentIndicator in indicator.UserCommentIndicatorList)
                                {
                                    if ((commentIndicator.CommentId == commentView.CommentId)
                                        && (commentView.Comment != commentIndicator.Comment))
                                    {
                                        commentIndicator.Comment = commentView.Comment;
                                        commentIndicator.IsModifiedComment = true;
                                        newModel.InformationDem.IsUpdateComment = true;
                                    }
                                }
                            }
                        }
                    }

                    foreach (var commentView in userCommentUpdateList)
                    {
                        if (commentView.SubIndicatorId != 0)
                        {
                            foreach (var indicator in newModel.IndicatorDataList)
                            {
                                if (indicator.SubindicatorList.Any())
                                {
                                    foreach (var subIndicator in indicator.SubindicatorList)
                                    {
                                        foreach (var commentSubIndicator in subIndicator.UserCommentIndicatorList)
                                        {
                                            if ((commentSubIndicator.CommentId == commentView.CommentId)
                                                && (commentView.Comment != commentSubIndicator.Comment))
                                            {
                                                commentSubIndicator.Comment = commentView.Comment;
                                                commentSubIndicator.IsModifiedComment = true;
                                                newModel.InformationDem.IsUpdateComment = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                var newTextCommentIndicator = formData.Where(o => o.Name.Equals("newTextCommentIndicator"));

                if (newTextCommentIndicator != null)
                {
                    var newIndicatorIdIndicator = formData.Where(o => o.Name.Equals("newIndicatorIdIndicator"));
                    var newSubIndicatorIdIndicator = formData.Where(o => o.Name.Equals("newSubIndicatorIdIndicator"));

                    var newCommentIndicatorList = new List<UserCommentIndicatorViewModel>();
                    for (int i = 0; i < newTextCommentIndicator.Count(); i++)
                    {
                        var subInd = Convert.ToInt32(newSubIndicatorIdIndicator.ElementAt(i).Value);
                        newCommentIndicatorList.Add(new UserCommentIndicatorViewModel()
                        {
                            IndicatorId = Convert.ToInt32(newIndicatorIdIndicator.ElementAt(i).Value),
                            SubIndicatorId = subInd,
                            CommentId = 0,
                            Comment = newTextCommentIndicator.ElementAt(i).Value.Trim(),
                            Role = newModel.InformationDem.Role
                        });
                    }

                    newModel.InformationDem.NewCommentIndicatorList = newCommentIndicatorList;
                }

                var deleteComments = formData.FirstOrDefault(o => o.Name.Equals("deleteCommentsIndicator"));
                if (deleteComments != null)
                {
                    string[] idCommentList = deleteComments.Value.ToString().Split('|')
                                            .Where(x => !string.IsNullOrEmpty(x)).ToArray();

                    newModel.InformationDem.DeleteCommentsIndicatorList = new List<int>();
                    foreach (string idComment in idCommentList)
                    {
                        if (idComment != "0" && idComment != "'0'")
                        {
                            newModel.InformationDem.DeleteCommentsIndicatorList.Add(Convert.ToInt32(idComment));
                        }
                    }
                }

                var newTextCommentCountryStrategy = formData.Where(o => o.Name.Equals("newTextCommentCountryStrategy"));
                if (newTextCommentCountryStrategy != null)
                {
                    var newCommentCountryStrategyList = new List<UserCommentCountryStrategyViewModel>();
                    for (int i = 0; i < newTextCommentCountryStrategy.Count(); i++)
                    {
                        newCommentCountryStrategyList.Add(new UserCommentCountryStrategyViewModel()
                        {
                            CommentId = 0,
                            Comment = newTextCommentCountryStrategy.ElementAt(i).Value.Trim(),
                            Role = newModel.InformationDem.Role
                        });
                    }

                    newModel.CountryStrategyData.InformationDem.NewCommentCountryStrategyList = newCommentCountryStrategyList;
                }

                var updateCommentsCountryStrategy = formData.Where(o => o.Name.Equals("updateCommentTextCountryStrategy"));
                var userCommentUpdateCountryStrategyList = new List<UserCommentCountryStrategyViewModel>();
                if (updateCommentsCountryStrategy != null)
                {
                    var editCommentsIdCountryStrategy = formData.Where(o => o.Name.Equals("updateCommentIdCountryStrategy"));
                    for (int i = 0; i < updateCommentsCountryStrategy.Count(); i++)
                    {
                        userCommentUpdateCountryStrategyList.Add(new UserCommentCountryStrategyViewModel()
                        {
                            CommentId = Convert.ToInt32(editCommentsIdCountryStrategy.ElementAt(i).Value),
                            Comment = updateCommentsCountryStrategy.ElementAt(i).Value.Trim()
                        });
                    }

                    foreach (var commentView in userCommentUpdateCountryStrategyList)
                    {
                        foreach (var commentCountryStrategy in newModel.CountryStrategyData.UserCommentCountryList)
                        {
                            if ((commentCountryStrategy.CommentId == commentView.CommentId)
                                && (commentView.Comment != commentCountryStrategy.Comment))
                            {
                                commentCountryStrategy.Comment = commentView.Comment;
                                commentCountryStrategy.IsModifiedComment = true;
                                newModel.CountryStrategyData.InformationDem.IsUpdateComment = true;
                            }
                        }
                    }
                }

                var deleteCommentsCountryStrategy = formData.FirstOrDefault(o => o.Name.Equals("deleteCommentsSectionCountryStrategy"));
                if (deleteCommentsCountryStrategy != null)
                {
                    string[] idCommentList = deleteCommentsCountryStrategy.Value.ToString().Split('|')
                                            .Where(x => !string.IsNullOrEmpty(x)).ToArray();

                    newModel.CountryStrategyData.InformationDem.DeleteCommentsIndicatorList = new List<int>();
                    foreach (string idComment in idCommentList)
                    {
                        if (idComment != "0" && idComment != "'0'")
                        {
                            newModel.CountryStrategyData.InformationDem.DeleteCommentsCountryStrategyList.Add(Convert.ToInt32(idComment));
                        }
                    }
                }

                var newTextCommentCountryProgram = formData.Where(o => o.Name.Equals("newTextCommentCountryProgram"));
                if (newTextCommentCountryProgram != null)
                {
                    var newCommentCountryProgramList = new List<UserCommentCountryProgramViewModel>();
                    for (int i = 0; i < newTextCommentCountryProgram.Count(); i++)
                    {
                        newCommentCountryProgramList.Add(new UserCommentCountryProgramViewModel()
                        {
                            CommentId = 0,
                            Comment = newTextCommentCountryProgram.ElementAt(i).Value.Trim(),
                            Role = newModel.InformationDem.Role
                        });
                    }

                    newModel.CountryProgramData.InformationDem.NewCommentCountryProgramList = newCommentCountryProgramList;
                }

                var updateCommentsCountryProgram = formData.Where(o => o.Name.Equals("updateCommentTextCountryProgram"));
                var userCommentUpdateCountryProgramList = new List<UserCommentCountryProgramViewModel>();
                if (updateCommentsCountryProgram != null)
                {
                    var editCommentsIdCountryProgram = formData.Where(o => o.Name.Equals("updateCommentIdCountryProgram"));
                    for (int i = 0; i < updateCommentsCountryProgram.Count(); i++)
                    {
                        userCommentUpdateCountryProgramList.Add(new UserCommentCountryProgramViewModel()
                        {
                            CommentId = Convert.ToInt32(editCommentsIdCountryProgram.ElementAt(i).Value),
                            Comment = updateCommentsCountryProgram.ElementAt(i).Value.Trim()
                        });
                    }

                    foreach (var commentView in userCommentUpdateCountryProgramList)
                    {
                        foreach (var commentCountryProgram in newModel.CountryProgramData.UserCommentCountryProgramList)
                        {
                            if ((commentCountryProgram.CommentId == commentView.CommentId)
                                && (commentView.Comment != commentCountryProgram.Comment))
                            {
                                commentCountryProgram.Comment = commentView.Comment;
                                commentCountryProgram.IsModifiedComment = true;
                                newModel.CountryProgramData.InformationDem.IsUpdateComment = true;
                            }
                        }
                    }
                }

                var deleteCommentsCountryProgram = formData.FirstOrDefault(o => o.Name.Equals("deleteCommentsSectionCountryProgram"));
                if (deleteCommentsCountryProgram != null)
                {
                    string[] idCommentList = deleteCommentsCountryProgram.Value.ToString().Split('|')
                                            .Where(x => !string.IsNullOrEmpty(x)).ToArray();

                    newModel.CountryProgramData.InformationDem.DeleteCommentsCountryProgramList = new List<int>();
                    foreach (string idComment in idCommentList)
                    {
                        if (idComment != "0" && idComment != "'0'")
                        {
                            newModel.CountryProgramData.InformationDem.DeleteCommentsCountryProgramList.Add(Convert.ToInt32(idComment));
                        }
                    }
                }
            }

            return newModel;
        }

        #endregion
        #endregion

        #region Institution
        public static void UpdateInstitutionWorkFlowViewModel(
            this InstitutionsWorkflowViewModels viewModel,
            ClientFieldData[] clientFieldData)
        {
            if (viewModel.UserComments == null)
            {
                viewModel.UserComments = new List<UserCommentViewModel>();
            }

            var editComments = clientFieldData.Where(o => o.Name.Equals(TEXT_COMMENT));
            var editCommentsId = clientFieldData.Where(o => o.Name.Equals(COMMENT_ID));

            viewModel.UserComments = MapperEditComments(viewModel.UserComments,
                editComments,
                editCommentsId);

            var newComments = clientFieldData.Where(o => o.Name.Equals(NEW_COMMENT));

            viewModel.UserComments = MapperNewComment(viewModel.UserComments, newComments);

            var deleteComments = clientFieldData
                .FirstOrDefault(o => o.Name.Equals(DELETE_COMMENTS));

            if (deleteComments != null)
            {
                string[] deleteC = deleteComments.Value.ToString()
                    .Split('|')
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();

                if (viewModel.DeleteComments == null)
                {
                    viewModel.DeleteComments = new List<int>();
                }

                foreach (string s in deleteC)
                {
                    viewModel.DeleteComments.Add(Convert.ToInt32(s));
                    viewModel.UserComments
                        .RemoveAll(d => d.CommentId.Equals(Convert.ToInt32(s)));
                }
            }

            if (viewModel.Documents == null)
            {
                viewModel.Documents = new List<InstitutionsWorkflowDocumentsViewModels>();
            }

            var documentDescription = clientFieldData
                .Where(x => !string.IsNullOrWhiteSpace(x.Name) &&
                    x.Name.Contains(DOCUMENT_DESCRIPTION));
            var documentNumber = clientFieldData
                .Where(x => !string.IsNullOrWhiteSpace(x.Name) &&
                    x.Name.Contains(DOCUMENT_NUMBER));
            var documentIndex = 0;

            foreach (var document in documentDescription)
            {
                var documentNumberValue = documentNumber.ToArray()[documentIndex].Value;
                viewModel.Documents.Add(
                    new InstitutionsWorkflowDocumentsViewModels
                    {
                        Description = document.Value,
                        DocNumber = documentNumberValue
                    });
                documentIndex++;
            }
        }
        #endregion

        #region Approval

        public static void UpdateApprovalOperationViewModel(
            this ApprovalOperationViewModel viewModel,
            ClientFieldData[] clientFieldData)
        {
            viewModel.Financings = new List<FinancingViewModel>();
            var model = new FinancingViewModel();
            model.FinancingRows = new List<FinancingRowViewModel>();

            var fundCurrencies = clientFieldData.Where(
                x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals(FUND_CURRENCY));
            var idbApprovedAmounts = clientFieldData.Where(
                x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals(IDB_APPROVED_AMOUNT));
            var idbUsApprovedAmounts = clientFieldData.Where(
                x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals(IDB_US_APPROVED_AMOUNT));
            var executors = clientFieldData.Where(
                x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals(EXECUTOR));

            var dataListPersistParent = clientFieldData
                .Where(o => o.ExtraData.ContainsKey(DATA_PERSIST_PARENT_ID));

            var dataListPersistParentDistinct = dataListPersistParent
                .Select(x => x.ExtraData[DATA_PERSIST_PARENT_ID]).Distinct();

            foreach (var persistParent in dataListPersistParentDistinct)
            {
                var financingRow = new FinancingRowViewModel
                {
                    FinancingDetails = new List<FinancingDetailViewModel>()
                };

                var itemEquals = dataListPersistParent
                    .Where(x => x.ExtraData[DATA_PERSIST_PARENT_ID] == persistParent);

                var detail = new FinancingDetailViewModel();
                var details = new List<FinancingDetailViewModel>();

                var fundCurrency = itemEquals.Where(
                 x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals(FUND_CURRENCY)).ToList();

                if (!string.IsNullOrEmpty(fundCurrency.FirstOrDefault().Value))
                {
                    financingRow.FundCurrency = Convert.ToInt32(fundCurrency.FirstOrDefault().Value);
                }
                else
                {
                    financingRow.FundCurrency = null;
                }

                var financingType = itemEquals
                    .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Name) &&
                        x.Name.Equals(FINANCING_TYPE_ID));

                if (financingType == null)
                {
                    financingType = itemEquals
                        .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Name) &&
                            x.Name.Equals(HDN_FINANCING));
                }

                financingRow.FinancingTypeId = financingType.ConvertToInt(true);

                var fund = itemEquals.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Name) &&
                    x.Name.Equals(FUND_ID));

                if (fund == null)
                {
                    fund = itemEquals
                        .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Name) &&
                            x.Name.Equals(HDN_FUND));
                }

                financingRow.FundId = fund.ConvertToInt(true);

                model.FinancingRows.Add(financingRow);

                var idbApprovedAmount = itemEquals
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals(IDB_APPROVED_AMOUNT))
                    .ToList();

                var idbUsApprovedAmount = itemEquals
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals(IDB_US_APPROVED_AMOUNT))
                    .ToList();

                var executor = itemEquals
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals(EXECUTOR))
                    .ToList();

                var increase = itemEquals
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals(IS_INCREASE))
                    .ToList();

                var suffix = itemEquals
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals(SUFFIX))
                    .ToList();

                var approvalNumber = itemEquals
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals(APPROVAL_NUMBER))
                    .ToList();

                var resolutionNumber = itemEquals
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals(RESOLUTION_NUMBER))
                    .ToList();

                var approvalAuthority = itemEquals
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals(APPROVAL_AUTHORITY))
                    .ToList();

                var approvalProcess = itemEquals
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals(APPROVAL_PROCESS))
                    .ToList();

                var approvalDate = itemEquals
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals(APPROVAL_OPERATION_DATE))
                    .ToList();

                var documentNumber = itemEquals
                   .Where(x =>
                       !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals(DOCUMENT_NUMBER_APPR))
                   .ToList();

                var documentDate = itemEquals
                   .Where(x =>
                       !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals(DOCUMENT_DATE_APPR))
                   .ToList();

                var financialDetails = (
                from idbAmount in idbApprovedAmount
                join idbUsAmount in idbUsApprovedAmount
                    on idbApprovedAmount.IndexOf(idbAmount) equals
                        idbUsApprovedAmount.IndexOf(idbUsAmount)
                join exec in executor
                    on idbUsApprovedAmount.IndexOf(idbUsAmount) equals executor.IndexOf(exec)
                join isIncrease in increase
                    on executor.IndexOf(exec) equals increase.IndexOf(isIncrease)
                join suf in suffix
                    on increase.IndexOf(isIncrease) equals suffix.IndexOf(suf)
                join resolution in resolutionNumber
                    on suffix.IndexOf(suf) equals resolutionNumber.IndexOf(resolution)
                join authority in approvalAuthority
                    on resolutionNumber.IndexOf(resolution) equals approvalAuthority.IndexOf(authority)
                join process in approvalProcess
                    on approvalAuthority.IndexOf(authority) equals approvalProcess.IndexOf(process)
                join docNumber in documentNumber
                    on approvalProcess.IndexOf(process) equals documentNumber.IndexOf(docNumber)
                join docDate in documentDate
                    on documentNumber.IndexOf(docNumber) equals documentDate.IndexOf(docDate)
                join approvDate in approvalDate
                    on documentDate.IndexOf(docDate) equals approvalDate.IndexOf(approvDate)
                join apprNumber in approvalNumber
                    on approvalDate.IndexOf(approvDate) equals approvalNumber.IndexOf(apprNumber) into tmpAppr
                from apprNumber in tmpAppr.DefaultIfEmpty(new ClientFieldData { Value = string.Empty })
                select new FinancingDetailViewModel
                {
                    IDBApprovedAmount = idbAmount.Value.ConvertToDecimal(true),
                    USApprovedAmount = idbUsAmount.Value.ConvertToDecimal(true),
                    FinancingDetailId = Convert.ToInt32(idbAmount.Id),
                    Executor = detail.Executor = (exec.Value != null && exec.Value.Length > 0) ?
                        Convert.ToInt32(exec.Value) : (int?)null,
                    IsIncrease = Convert.ToBoolean(isIncrease.Value),
                    Suffix = suf.Value,
                    ApprovalNumber = apprNumber.Value,
                    ResolutionNumber = resolution.Value,
                    ApprovalAuthority = (authority.Value != null && authority.Value.Length > 0) ?
                        int.Parse(authority.Value) : (int?)null,
                    ApprovalProcess = (process.Value != null && process.Value.Length > 0) ?
                        int.Parse(process.Value) : (int?)null,
                    ApprovalDate = approvDate.Value != string.Empty ?
                        DateTime.Parse(approvDate.Value) :
                        (DateTime?)null,
                    FinancingTypeId = financingRow.FinancingTypeId,
                    FundId = financingRow.FundId,
                    DocumentDate = docDate.Value != string.Empty ?
                        DateTime.Parse(docDate.Value) :
                        (DateTime?)null,
                    DocumentNumber = docNumber.Value
                }).ToList();

                model.FinancingRows.Last().FinancingDetails = financialDetails;
            }

            foreach (var row in model.FinancingRows)
            {
                var financingMaster = new FinancingViewModel
                {
                    FinancingRows = new List<FinancingRowViewModel>()
                };

                financingMaster.FinancingRows.Add(row);
                viewModel.Financings.Add(financingMaster);
            }
        }

        public static TransactionRequest ConvertToTransactionRequest(
            this ClientFieldData[] requestClient,
            string operationNumber)
        {
            if (!requestClient.HasAny())
                return new TransactionRequest();

            ClientFieldData field;

            field = requestClient.FirstOrDefault(rc => rc.Name == IS_INCREASE);
            int increaseId = int.MinValue;
            if (field != null)
                int.TryParse(field.Id, out increaseId);

            bool isIncrease = false;
            if (field != null)
                bool.TryParse(field.Value, out isIncrease);

            field = requestClient.FirstOrDefault(rc => rc.Name == IS_AUTOMATIC);
            bool isAutomatic = false;
            if (field != null)
                bool.TryParse(field.Value, out isAutomatic);

            field = requestClient.FirstOrDefault(rc => rc.Name == IS_CURRENT_REFORMULATION);
            bool isCurrentReformulation = true;
            if (field != null)
                bool.TryParse(field.Value, out isCurrentReformulation);

            field = requestClient.FirstOrDefault(rc => rc.Name == EXECUTOR);
            int agencyId = int.MinValue;
            if (field != null)
                int.TryParse(field.Value, out agencyId);

            field = requestClient.FirstOrDefault(rc => rc.Name == IDB_US_APPROVED_AMOUNT);
            decimal usAmount = decimal.MinValue;
            if (field != null)
                decimal.TryParse(field.Value, out usAmount);

            field = requestClient.FirstOrDefault(rc => rc.Name == IDB_APPROVED_AMOUNT);
            decimal amount = decimal.MinValue;
            if (field != null)
                decimal.TryParse(field.Value, out amount);

            field = requestClient.FirstOrDefault(rc => rc.Name == APPROVAL_OPERATION_DATE);
            DateTime approvalDate = DateTime.MinValue;
            if (field != null)
                DateTime.TryParse(field.Value, out approvalDate);

            field = requestClient.FirstOrDefault(rc => rc.Name == APPROVAL_AUTHORITY);
            int authorityId = int.MinValue;
            if (field != null)
                int.TryParse(field.Value, out authorityId);

            field = requestClient.FirstOrDefault(rc => rc.Name == APPROVAL_PROCESS);
            int processId = int.MinValue;
            if (field != null)
                int.TryParse(field.Value, out processId);

            field = requestClient.FirstOrDefault(rc => rc.Name == APPROVAL_NUMBER);
            string approvalNumber = string.Empty;
            if (field != null)
                approvalNumber = field.Value.Trim();

            field = requestClient.FirstOrDefault(rc => rc.Name == RESOLUTION_NUMBER);
            string resolution = string.Empty;
            if (field != null)
                resolution = field.Value.Trim();

            field = requestClient.FirstOrDefault(rc => rc.Name == DOCUMENT_DATE_APPR);
            DateTime documentDate = DateTime.MinValue;
            if (field != null)
                DateTime.TryParse(field.Value, out documentDate);

            field = requestClient.FirstOrDefault(rc => rc.Name == DOCUMENT_NUMBER_APPR);
            string documentNumber = string.Empty;
            if (field != null)
                documentNumber = field.Value.Trim();

            return new TransactionRequest
            {
                OperationNumber = operationNumber,
                Id = increaseId,
                IsAutomatic = isAutomatic,
                IsIncrease = isIncrease,
                IsCurrentReformulation = isCurrentReformulation,
                ApprovalNumber = approvalNumber,
                ExecutingAgencyId = agencyId,
                UsApprovedAmount = usAmount,
                ApprovedAmount = amount,
                ApprovalDate = approvalDate,
                ResolutionNumber = resolution,
                ApprovalAuthorityId = authorityId,
                ApprovalProcessId = processId,
                DocumentNumber = documentNumber,
                DocumentDate = documentDate == DateTime.MinValue ? (DateTime?)null : documentDate
            };
        }

        #endregion

        public static void UpdateCreationWorkFlowViewModel(
            this CreationK2TaskViewModel viewModel,
            ClientFieldData[] clientFieldData)
        {
            if (viewModel.UserComments == null)
            {
                viewModel.UserComments = new List<UserCommentViewModel>();
            }

            var editComments = clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(TEXT_COMMENT));
            var editCommentsId = clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(COMMENT_ID));

            viewModel.UserComments = MapperEditComments(viewModel.UserComments,
                editComments,
                editCommentsId);

            var newComments = clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(NEW_COMMENT));

            viewModel.UserComments = MapperNewComment(viewModel.UserComments, newComments);

            var deleteComments = clientFieldData
                .FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(DELETE_COMMENTS));

            if (deleteComments != null)
            {
                string[] deleteC =
                    deleteComments.Value.ToString()
                    .Split('|')
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();

                if (viewModel.DeleteComments == null)
                {
                    viewModel.DeleteComments = new List<int>();
                }

                foreach (string s in deleteC)
                {
                    viewModel.DeleteComments.Add(Convert.ToInt32(s));
                    viewModel.UserComments
                        .RemoveAll(d => d.CommentId.Equals(Convert.ToInt32(s)));
                }
            }
        }

        public static string ConvertToClienDataXML(this ClientFieldData[] clientFieldData)
        {
            DataTable table = new DataTable();
            table.TableName = RulesEngAttributes.XML_TABLENAME;

            table.Columns.Add(RulesEngAttributes.XML_ID);
            table.Columns.Add(RulesEngAttributes.XML_NAME);
            table.Columns.Add(RulesEngAttributes.XML_VALUE);

            table.Columns.Add(RulesEngAttributes.XML_EXTRA_ID);
            table.Columns.Add(RulesEngAttributes.XML_EXTRA_NAME);
            table.Columns.Add(RulesEngAttributes.XML_EXTRA_VALUE);

            if (clientFieldData != null)
            {
                clientFieldData
                    .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                        o.Name == DELETED_RELATIONSHIPS)
                    .ForEach(x => x.Value = string.Empty);

                foreach (var data in clientFieldData)
                {
                    table.Rows.Add(
                        new object[]
                        {
                            data.Id,
                            data.Name,
                            data.Value,
                            data.ExtraData != null ?
                                data.ExtraData[RulesEngAttributes.XML_INDEX_EXTRA_ID] : null,
                            data.ExtraData != null ?
                                data.ExtraData[RulesEngAttributes.XML_INDEX_EXTRA_NAME] : null,
                            data.ExtraData != null ?
                                data.ExtraData[RulesEngAttributes.XML_INDEX_EXTRA_VALUE] : null
                        });
                }
            }

            StringWriter sw = new StringWriter();
            table.WriteXml(sw, XmlWriteMode.WriteSchema);

            return sw.ToString();
        }
        #endregion

        #region Private Methods

        private static List<OperationTeamRowViewModel> GetPrimaryFilterOpRel(
            this List<OperationTeamRowViewModel> listOperationTeam,
            string codeType,
            bool canEdit)
        {
            var list = new List<OperationTeamRowViewModel>();

            if (codeType != OPUSGlobalValues.MULTIPLE_PERMISSIONS_IN_ROLE)
            {
                if (IDBContext.Current.HasPermission(Permission.SPD_DEM_RESPONSIBILITY_WRITE))
                {
                    list.AddRange(listOperationTeam.Where(x =>
                        x.RoleName != OPUSGlobalValues.SPD_SPECIALIST));
                }
                else
                {
                    list = listOperationTeam.Where(o =>
                        o.CodeRoleType != codeType &&
                        o.CodeRoleType != OPUSGlobalValues.TML_OR_ALTML &&
                        o.CodeRoleType != OPUSGlobalValues.OA_ROLE).ToList();
                }
            }
            else
            {
                if (!canEdit)
                {
                    list.AddRange(listOperationTeam.Where(o => o.CodeRoleType == OPUSGlobalValues.ALL_ROLE).ToList());
                }

                if (!IDBContext.Current.HasPermission(Permission.ESG_SPECIALIST_ASSIGNMENT_WRITE))
                {
                    list.AddRange(listOperationTeam.Where(o => o.CodeRoleType == OPUSGlobalValues.ESG_ROLE).ToList());
                }

                if (!IDBContext.Current.HasPermission(Permission.SPD_PCR_WRITE))
                {
                    list.AddRange(listOperationTeam.Where(o => o.CodeRoleType == OPUSGlobalValues.PCR_ROLE).ToList());
                }

                if (IDBContext.Current.HasPermission(Permission.DEM_TEAM_LEADER_WRITE) &&
                    !IDBContext.Current.HasPermission(Permission.SPD_DEM_RESPONSIBILITY_WRITE))
                {
                    list.AddRange(listOperationTeam.Where(o =>
                        o.RoleName == OPUSGlobalValues.SPD_SPECIALIST).ToList());
                }
            }

            list.AddRange(listOperationTeam.Where(o => o.CodeRoleType == OPUSGlobalValues.TML_OR_ALTML).ToList());
            list.AddRange(listOperationTeam.Where(o => o.CodeRoleType == OPUSGlobalValues.OA_ROLE).ToList());

            return list;
        }

        private static List<UserCommentViewModel> MapperEditComments(
            List<UserCommentViewModel> userComments,
            IEnumerable<ClientFieldData> editComments,
            IEnumerable<ClientFieldData> editCommentsId)
        {
            if (editComments != null && editCommentsId != null)
            {
                for (int i = 0; i < editComments.Count(); i++)
                {
                    var index = userComments.FindIndex(x =>
                        x.CommentId == Convert.ToInt32(editCommentsId.ElementAt(i).Value));

                    if (!userComments.ElementAt(index).Comment
                        .Equals(editComments.ElementAt(i).Value.Trim()))
                    {
                        userComments.ElementAt(index).Comment =
                            editComments.ElementAt(i).Value.Trim();
                    }
                }
            }

            return userComments;
        }

        private static List<UserCommentViewModel> MapperNewComment(
            List<UserCommentViewModel> userComments,
            IEnumerable<ClientFieldData> newComments)
        {
            if (newComments != null)
            {
                for (int i = 0; i < newComments.Count(); i++)
                {
                    var comment = new UserCommentViewModel();
                    comment.Comment = newComments.ElementAt(i).Value.Trim();
                    userComments.Add(comment);
                }
            }

            return userComments;
        }

        private static void UpdateESGTeamMembers(
            this List<ESGTeamMembersViewModel> esgTeamMemberViewModelList,
            ClientFieldData[] clientFieldData,
            IEnumMappingService enumMappingService)
        {
            var clientFieldDataSpecialists = clientFieldData.Where(x => x.Name == OPERATION_TEAM_DATA_ID_SPECIALIST || x.Name == ROLE_ID);

            var specialistsOldList = clientFieldDataSpecialists.Where(x => !string.IsNullOrWhiteSpace(x.Id));
            var specialistsNewList = clientFieldDataSpecialists.Where(x => string.IsNullOrWhiteSpace(x.Id));

            var originalId = esgTeamMemberViewModelList.Select(x => x.OperationTeamDataId).Distinct();
            var finalId = specialistsOldList.Select(x => x.Id).Distinct();
            var idsToRemove = originalId.Where(x => finalId.All(y => x.ToString() != y)).ToList();

            foreach (var idRemove in idsToRemove)
            {
                esgTeamMemberViewModelList.Remove(
                    esgTeamMemberViewModelList.First(x => x.OperationTeamDataId == idRemove));
            }

            foreach (var specialistOld in specialistsOldList.Where(x => x.Name == ROLE_ID))
            {
                esgTeamMemberViewModelList.First(x => x.OperationTeamDataId == int.Parse(specialistOld.Id))
                    .RoleId = enumMappingService.GetMappedEnum<EnvironmentalSocialDataRoleEnum>(int.Parse(specialistOld.Value));
            }

            var specialistsToInsertAll = specialistsNewList.Where(x => x.ExtraData.Any(y => y.Key == DATA_PERSIST_NEW));

            var specialistsToInsert =
                from specialistNew in specialistsToInsertAll
                group specialistNew by specialistNew.ExtraData.First().Value
                    into specialist
                select new ESGTeamMembersViewModel
                {
                    ADUserName = specialist.First(x => x.Name == OPERATION_TEAM_DATA_ID_SPECIALIST).Value,
                    RoleId = enumMappingService.GetMappedEnum<EnvironmentalSocialDataRoleEnum>(
                        int.Parse(specialist.First(x => x.Name == ROLE_ID).Value))
                };
            esgTeamMemberViewModelList.AddRange(specialistsToInsert);
        }

        private static void UpdateSustainabilityComponents(
            this List<SustainabilityComponentsViewModel> sustainabilityComponentsViewModelList,
            ClientFieldData[] clientFieldData,
            IEnumMappingService enumMappingService)
        {
            var clientFieldDataSustainabilityComponent = clientFieldData
                .Where(x => x.Name == FINANCING_TYPE_ID ||
                    x.Name == FUND_ID ||
                    x.Name == FUND_AMOUNT ||
                    x.Name.Contains(SUSTAINABILITY_TYPE_ID) ||
                    x.Name.Contains(COMPONENT_ID) ||
                    x.Name == COMPONENT_AMOUNT);
            var sustainabilityComponentsOld = clientFieldDataSustainabilityComponent
                .Where(x => !string.IsNullOrWhiteSpace(x.Id));
            var sustainabilityComponentsNew = clientFieldDataSustainabilityComponent
                .Where(x => string.IsNullOrWhiteSpace(x.Id));
            var originalId = sustainabilityComponentsViewModelList
                .Select(x => x.SustainabilityComponentsId)
                .Distinct();
            var finalId = sustainabilityComponentsOld.Select(x => x.Id).Distinct();
            var idsToRemove = originalId
                .Where(x => finalId.All(y => x.ToString() != y))
                .ToList();

            foreach (var idRemove in idsToRemove)
            {
                sustainabilityComponentsViewModelList.Remove(
                    sustainabilityComponentsViewModelList
                        .First(x => x.SustainabilityComponentsId == idRemove));
            }

            foreach (var sustainabilityComponentOld in sustainabilityComponentsOld)
            {
                var sustainabilityComponentViewModel = sustainabilityComponentsViewModelList
                    .First(x => x.SustainabilityComponentsId ==
                        int.Parse(sustainabilityComponentOld.Id));

                switch (sustainabilityComponentOld.Name)
                {
                    case FINANCING_TYPE_ID:
                        sustainabilityComponentViewModel.FinancingTypeId = enumMappingService
                            .GetMappedEnum<EnvironmentalSocialDataFinancingTypeEnum>(
                                int.Parse(sustainabilityComponentOld.Value));
                        break;
                    case FUND_ID:
                        sustainabilityComponentViewModel.FundId =
                            int.Parse(sustainabilityComponentOld.Value);
                        break;
                    case FUND_AMOUNT:
                        sustainabilityComponentViewModel.FundAmount =
                            decimal.Parse(sustainabilityComponentOld.Value);
                        break;
                    case SUSTAINABILITY_TYPE_ID:
                        sustainabilityComponentViewModel.SustainabilityTypeId =
                            enumMappingService
                                .GetMappedEnum<EnvironmentalSocialDataSustainabilityTypeEnum>(
                                    int.Parse(sustainabilityComponentOld.Value));
                        break;
                    case COMPONENT_ID:
                        sustainabilityComponentViewModel.ComponentId =
                            int.Parse(sustainabilityComponentOld.Value);
                        break;
                    case COMPONENT_AMOUNT:
                        sustainabilityComponentViewModel.ComponentAmount =
                            decimal.Parse(sustainabilityComponentOld.Value);
                        break;
                }
            }

            var sustainabilityComponentToInsertAll = sustainabilityComponentsNew
                .Where(x => x.ExtraData.Any(y => y.Key == DATA_PERSIST_NEW));

            var sustainabilityComponentsToInsert =
                from sustainabilityComponentNew in sustainabilityComponentToInsertAll
                group sustainabilityComponentNew
                    by sustainabilityComponentNew.ExtraData.First().Value
                    into sustainabilityComponent
                select new
                {
                    FinancingTypeId = int.Parse(sustainabilityComponent
                        .First(x => x.Name == FINANCING_TYPE_ID).Value),
                    FundId = int.Parse(sustainabilityComponent
                        .First(x => x.Name == FUND_ID).Value),
                    FundAmount = decimal.Parse(sustainabilityComponent
                        .First(x => x.Name == FUND_AMOUNT).Value),
                    SustainabilityTypeId = int.Parse(sustainabilityComponent
                        .First(x => x.Name.Contains(SUSTAINABILITY_TYPE_ID)).Value),
                    ComponentId = int.Parse(sustainabilityComponent
                        .First(x => x.Name.Contains(COMPONENT_ID)).Value),
                    ComponentAmount = decimal.Parse(sustainabilityComponent
                        .First(x => x.Name == COMPONENT_AMOUNT).Value)
                };

            var list = new List<SustainabilityComponentsViewModel>();
            foreach (var item in sustainabilityComponentsToInsert)
            {
                var sustainabilityComponentsViewModel = new SustainabilityComponentsViewModel
                {
                    FinancingTypeId = enumMappingService
                        .GetMappedEnum<EnvironmentalSocialDataFinancingTypeEnum>(
                            item.FinancingTypeId),
                    FundId = item.FundId,
                    FundAmount = item.FundAmount,
                    SustainabilityTypeId = enumMappingService
                        .GetMappedEnum<EnvironmentalSocialDataSustainabilityTypeEnum>(
                            item.SustainabilityTypeId),
                    ComponentId = item.ComponentId,
                    ComponentAmount = item.ComponentAmount
                };
                sustainabilityComponentsViewModelList.Add(sustainabilityComponentsViewModel);
            }
        }

        private static string GetValueFromClientFieldDataByVariableName(
            this ClientFieldData[] clientFieldData,
            string variableName,
            int operationTeamDataId)
        {
            var response = clientFieldData
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Name) &&
                    x.Name.Contains(variableName) &&
                    Convert.ToInt32(x.Id) == operationTeamDataId);

            return (response != null) ? response.Value : null;
        }

        private static void UpdateSASubindicatorViewModel(
            this List<SAIndicatorDataViewModel> subindicatorList,
            ClientFieldData[] formData)
        {
            ClientFieldData field;
            bool isChecked;
            var impactValue = new List<string>();
            var outcomeValue = new List<string>();
            var outputValue = new List<string>();
            string justification;

            foreach (var subindicator in subindicatorList)
            {
                var fieldForSubIndicator = formData
                    .Where(x => x.ExtraData
                        .First(y => y.Key == DATA_PERSIST_SUBINDICATOR_ID)
                        .Value == subindicator.SubindicatorId.ToString());

                field = fieldForSubIndicator
                    .FirstOrDefault(x => x.Name == INDICATOR_CHECK);
                isChecked = GetBoolValue(field, false);

                field = fieldForSubIndicator
                    .FirstOrDefault(x => x.Name == IMPACT_INDICATORS_COMBO);
                impactValue = ConvertToListString(field);

                field = fieldForSubIndicator
                    .FirstOrDefault(x => x.Name == OUTCOME_INDICATORS_COMBO);
                outcomeValue = ConvertToListString(field);

                field = fieldForSubIndicator
                    .FirstOrDefault(x => x.Name == OUTPUTS_COMBO);
                outputValue = ConvertToListString(field);

                field = fieldForSubIndicator
                    .FirstOrDefault(x => x.Name == JUSTIFICATION);
                justification = GetString(field);

                subindicator.IsChecked = isChecked;
                subindicator.ImpactIndicator = isChecked ? impactValue : new List<string>();
                subindicator.OutcomeIndicator = isChecked ? outcomeValue : new List<string>();
                subindicator.Output = isChecked ? outputValue : new List<string>();
                subindicator.Justification = isChecked ? justification : string.Empty;
            }
        }

        private static bool GetBoolValue(string value, bool valueIfNull = false)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            value = value.ToLowerInvariant();
            return value == TRUE_VAL;
        }

        private static bool GetBoolValue(ClientFieldData field, bool valueIfNull = false)
        {
            if (field == null)
            {
                return valueIfNull;
            }

            return GetBoolValue(field.Value, valueIfNull);
        }

        private static int? GetNullableInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            int? result = null;
            int tmp;

            if (int.TryParse(value, out tmp))
            {
                result = tmp;
            }

            return result;
        }

        private static int? GetNullableInt(ClientFieldData field)
        {
            if (field == null)
            {
                return null;
            }

            return GetNullableInt(field.Value);
        }

        private static string GetString(ClientFieldData field, string valueIfNull = null)
        {
            if (field == null)
            {
                return valueIfNull;
            }

            return field.Value ?? valueIfNull;
        }

        private static string GetCodeType(bool editPermission, bool isCreator = false)
        {
            var codeType = string.Empty;
            var countPermissions = 0;
            if (editPermission || isCreator)
            {
                codeType = OPUSGlobalValues.ALL_ROLE;
                countPermissions++;
            }

            if (IDBContext.Current.HasPermission(Permission.ESG_SPECIALIST_ASSIGNMENT_WRITE))
            {
                codeType = OPUSGlobalValues.ESG_ROLE;
                countPermissions++;
            }

            if (IDBContext.Current.HasPermission(Permission.SPD_PCR_WRITE))
            {
                codeType = OPUSGlobalValues.PCR_ROLE;
                countPermissions++;
            }

            if (countPermissions > 1)
            {
                codeType = OPUSGlobalValues.MULTIPLE_PERMISSIONS_IN_ROLE;
            }

            return codeType;
        }

        private static bool CanEditRole(string roleType, bool editPermission, bool isCreator = false)
        {
            if (roleType == OPUSGlobalValues.ALL_ROLE && (editPermission || isCreator))
            {
                return true;
            }

            if (roleType == OPUSGlobalValues.ESG_ROLE &&
                IDBContext.Current.HasPermission(Permission.ESG_SPECIALIST_ASSIGNMENT_WRITE))
            {
                return true;
            }

            if (roleType == OPUSGlobalValues.PCR_ROLE &&
                IDBContext.Current.HasPermission(Permission.SPD_PCR_WRITE))
            {
                return true;
            }

            return false;
        }

        private static int ConvertToInt(this ClientFieldData field, bool returnCeroInFail = false)
        {
            if (field == null)
            {
                if (returnCeroInFail)
                {
                    return 0;
                }
                else
                {
                    throw new ArgumentException();
                }
            }

            return field.Value.ConvertToInt(returnCeroInFail);
        }

        private static int ConvertToInt(this string field, bool returnCeroInFail = false)
        {
            int intValue;
            if (!int.TryParse(field, out intValue))
            {
                if (returnCeroInFail)
                {
                    intValue = 0;
                }
                else
                {
                    throw new ArgumentException();
                }
            }

            return intValue;
        }

        private static decimal ConvertToDecimal(this string field, bool returnCeroInFail = false)
        {
            decimal decimalValue;
            if (!decimal.TryParse(field, out decimalValue))
            {
                if (returnCeroInFail)
                {
                    decimalValue = 0.0m;
                }
                else
                {
                    throw new ArgumentException();
                }
            }

            return decimalValue;
        }

        private static List<int> ConvertToListInt(this ClientFieldData field, bool returnCeroInFail = false)
        {
            if (field == null || field.Value == null || string.IsNullOrEmpty(field.Value.ToString()))
            {
                return new List<int>();
            }

            return field.Value.Split(',').Select(x => x.ToString().ConvertToInt(returnCeroInFail)).ToList();
        }

        private static List<string> ConvertToListString(this ClientFieldData field, bool returnCeroInFail = false)
        {
            if (field == null || field.Value == null || string.IsNullOrEmpty(field.Value.ToString()))
            {
                return new List<string>();
            }

            return field.Value.Split(',').ToList();
        }

        private static string ContertToString(this ClientFieldData field, bool returnCeroInFail = false)
        {
            if (field == null || field.Value == null || string.IsNullOrEmpty(field.Value.ToString()))
            {
                return string.Empty;
            }

            return field.Value.ToString().Replace(",", string.Empty);
        }

        private static string ConvertToString(this ClientFieldData field)
        {
            if (field == null)
            {
                return null;
            }

            return field.Value;
        }

        private static bool? ConvertToBool(this ClientFieldData field)
        {
            if (field == null || string.IsNullOrEmpty(field.Value))
            {
                return null;
            }

            return field.Value.Equals("True");
        }

        private static DateTime ConvertToDateTime(this ClientFieldData field)
        {
            if (field == null || string.IsNullOrEmpty(field.Value))
            {
                return DateTime.MinValue;
            }

            return DateTime.Parse(field.Value);
        }

        private static string ValidateWorkflow(
            CreationFormViewModel viewModel,
            ClientFieldData[] clientFieldData,
            int igrId,
            int lonId,
            int administeredById,
            int hqId,
            int responsibleId,
            IList<DivisionOpus> groups,
            int bankwideId,
            int sgId,
            int nsgId,
            int iicId)
        {
            var lendingType = clientFieldData
                .FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) &&
                    o.Name.Equals(AttributeCode.LENDING_TYPE));

            var lendingId = 0;
            int.TryParse(lendingType.Value, out lendingId);

            var nsgCategorization = clientFieldData
                .FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) &&
                    o.Name.Equals(AttributeCode.INSTRUMENTS));

            var nsgCategorizationId = 0;
            int.TryParse(nsgCategorization.Value, out nsgCategorizationId);

            var isHqAdministered = viewModel.ResponsibilityData.CountryRelated
                .Any(o => o.Role.Equals(administeredById) && o.Countries.Contains(hqId));

            var responsible = viewModel.ResponsibilityData.OrganizationalUnit
                .First(o => o.Role.Equals(responsibleId));

            var isDivision = groups
                .Any(o => o.Group.Equals(DivisionOpusGroup.DIVISION) &&
                    o.DivisionId.Equals(responsible.OrganizationalUnit));

            var isIic = groups
                .Any(o => o.DivisionCode.Equals(OrgUnitCode.IIC) &&
                    o.DivisionId.Equals(responsible.OrganizationalUnit));

            var isMif = groups
                .Any(o => o.Group.Equals(DivisionOpusGroup.MIF) &&
                    o.DivisionId.Equals(responsible.OrganizationalUnit));

            var isVpc = groups
                .Any(o => o.Group.Equals(DivisionOpusGroup.VPC) &&
                    o.DivisionId.Equals(responsible.OrganizationalUnit));

            var fundAmount = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(US_AMOUNT))
                .ToList();

            var maxFund = fundAmount == null ? 0 : fundAmount.Sum(o => decimal.Parse(o.Value));

            var countryDep = clientFieldData
                .FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) &&
                    o.Name.Equals(COUNTRY_DEPARTMENT_ID));

            if (viewModel.BasicData.OperationType == lonId ||
                (maxFund >= K2HelperOpus.MaxFund &&
                    viewModel.BasicData.OperationType == igrId))
            {
                var isRegional = CountryHelper.Get()
                    .IsRegionalCountry(viewModel.BasicData.CountryId ?? 0);

                if (isRegional && (countryDep == null || countryDep.Value == string.Empty))
                {
                    return OPUSGlobalValues.REQUIRED;
                }

                if (viewModel.BasicData.CountryId == bankwideId)
                {
                    return Localization
                        .GetText("OP.CR.CreationForm.Message.BankwideNoAvailable");
                }

                if (isHqAdministered)
                {
                    return Localization
                        .GetText("OP.CR.CreationForm.Message.HqNoAvailableCountryRelated");
                }

                var message = ValidateOrgUnits(
                   lendingId == sgId,
                   lendingId == nsgId,
                   isDivision,
                   isMif,
                   nsgCategorizationId == iicId,
                   isIic,
                   isVpc,
                   viewModel.BasicData.OperationType == igrId
                   ? OperationType.IGR
                   : OperationType.LON);

                if (!string.IsNullOrEmpty(message))
                    return message;
            }

            if (viewModel.BasicData.CountryId == hqId)
            {
                return Localization
                    .GetText("OP.CR.CreationForm.Message.HqNoAvailableCountry");
            }

            return string.Empty;
        }

        private static string ValidateOrgUnits(
            bool isSg,
            bool isNsg,
            bool isDivision,
            bool isMif,
            bool isNsgIic,
            bool isIic,
            bool isVpc,
            string opType)
        {
            if (isSg && !(isDivision || isVpc))
            {
                return opType == OperationType.IGR ?
                    Localization.GetText(
                        "OP.CR.CreationForm.Message.OnlyDivisionValidation") :
                    Localization.GetText(
                        "OP.CR.CreationForm.Message.OnlyDivisionVPCValidation");
            }

            if (isNsgIic && !isIic)
            {
                return Localization
                    .GetText("OP.CR.CreationForm.Message.OnlyIICValidation");
            }

            if (isNsg && !isNsgIic && !isMif)
            {
                return Localization
                    .GetText("OP.CR.CreationForm.Message.OnlyMifValidation");
            }

            return string.Empty;
        }

        private static DataDEMViewModel GetAttributesDem(DataDEMViewModel informationDem)
        {
            var newModel = new DataDEMViewModel();

            newModel.DemOperationId = informationDem.DemOperationId;
            newModel.UserName = informationDem.UserName;
            newModel.FullName = informationDem.FullName;
            newModel.Role = informationDem.Role;
            newModel.Date = informationDem.Date;
            newModel.Stage = informationDem.Stage;
            newModel.StageId = informationDem.StageId;
            newModel.CheckListVersion = informationDem.CheckListVersion;
            newModel.CheckListVersionId = informationDem.CheckListVersionId;
            newModel.IsUpdateComment = informationDem.IsUpdateComment;

            return newModel;
        }

        private static string TruncateRelevanceArea(this string relevance, int maxLength)
        {
            return relevance != null && relevance.Length > maxLength ? relevance.Substring(0, maxLength) : relevance;
        }

        private static List<CounterpartFinancingDetailViewModel> GetCounterpartDetails(
            ClientFieldData[] clientFieldData)
        {
            var response = new List<CounterpartFinancingDetailViewModel>();

            var counterpartTypeCode = clientFieldData
               .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
               o.Name.Equals(COUNTERPART_FINANCING_TYPES))
               .ToList();

            var counterpartCashKindCode = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                o.Name.Equals(COUNTERPART_FINANCING_CASH_OR_INKIND))
                .ToList();

            var counterpartFundingSourceCode = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                o.Name.Equals(COUNTERPART_FINANCING_FUNDING_SOURCE))
                .ToList();

            var counterpartDescription = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                o.Name.Equals(COUNTERPART_FINANCING_DESCRIPTION))
                .ToList();

            var counterpartAmount = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                o.Name.Equals(COUNTERPART_AMOUNT))
                .ToList();

            var counterpartStatus = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                o.Name.Equals(COUNTERPART_STATUS))
                .ToList();

            var counterpartIds = clientFieldData
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
                o.Name.Equals(COUNTERPART_ID_DETAIL))
                .ToList();

            response = (
                from counterpart in counterpartTypeCode
                join cashKind in counterpartCashKindCode
                    on counterpartTypeCode.IndexOf(counterpart) equals
                    counterpartCashKindCode.IndexOf(cashKind)
                join fundSource in counterpartFundingSourceCode
                    on counterpartTypeCode.IndexOf(counterpart) equals
                    counterpartFundingSourceCode.IndexOf(fundSource)
                join description in counterpartDescription
                    on counterpartTypeCode.IndexOf(counterpart) equals
                    counterpartDescription.IndexOf(description)
                join amount in counterpartAmount
                    on counterpartTypeCode.IndexOf(counterpart) equals
                    counterpartAmount.IndexOf(amount)
                join counterpartId in counterpartIds
                    on counterpartTypeCode.IndexOf(counterpart) equals
                counterpartIds.IndexOf(counterpartId)
                join counterpartStatusCode in counterpartStatus
                    on counterpartTypeCode.IndexOf(counterpart) equals
                    counterpartStatus.IndexOf(counterpartStatusCode)
                select new CounterpartFinancingDetailViewModel
                {
                    Id = int.Parse(counterpartId.Value),
                    CashKindCode = cashKind.Value,
                    FundingSourceCode = fundSource.Value,
                    DescriptionResources = description.Value,
                    TypeCode = counterpart.Value,
                    Amount = decimal.Parse(amount.Value),
                    StatusTypeCode = counterpartStatusCode.Value
                }).ToList();

            return response;
        }

        private static List<CofinancingResourcesDetailViewModel> GetCofinancingDetails(
            ClientFieldData[] clientFieldData)
        {
            var response = new List<CofinancingResourcesDetailViewModel>();

            var cofinancingFundingSource = clientFieldData
              .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
              o.Name.Equals(COFINANCING_FUNDING_SOURCE))
              .ToList();

            var cofinancingModality = clientFieldData
              .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
              o.Name.Equals(COFINANCING_MODALITY))
              .ToList();

            var cofinancingDescription = clientFieldData
              .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
              o.Name.Equals(COFINANCING_DESCRIPTION))
              .ToList();

            var cofinancingCashInKinds = clientFieldData
              .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
              o.Name.Equals(COFINANCING_CASH_OR_INKIND))
              .ToList();

            var cofinancingAmount = clientFieldData
              .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
              o.Name.Equals(COFINANCING_AMOUNT))
              .ToList();

            var cofinancingSignatureDate = clientFieldData
              .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
              o.Name.Equals(COFINANCING_SIGNATURE_DATE))
              .ToList();

            var cofinancingStatus = clientFieldData
              .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
              o.Name.Equals(COFINANCING_STATUS))
              .ToList();

            var cofinancingIds = clientFieldData
              .Where(o => !string.IsNullOrWhiteSpace(o.Name) &&
              o.Name.Equals(COFINANCING_ID_DETAIL))
              .ToList();
            var auxDateTime = DateTime.Now;
            response = (
                from fundSource in cofinancingFundingSource
                join modality in cofinancingModality
                    on cofinancingFundingSource.IndexOf(fundSource) equals
                    cofinancingModality.IndexOf(modality)
                join description in cofinancingDescription
                    on cofinancingFundingSource.IndexOf(fundSource) equals
                    cofinancingDescription.IndexOf(description)
                join amount in cofinancingAmount
                    on cofinancingFundingSource.IndexOf(fundSource) equals
                    cofinancingAmount.IndexOf(amount)
                join signatureDate in cofinancingSignatureDate
                    on cofinancingFundingSource.IndexOf(fundSource) equals
                    cofinancingSignatureDate.IndexOf(signatureDate)
                join complementaryId in cofinancingIds
                    on cofinancingFundingSource.IndexOf(fundSource) equals
                    cofinancingIds.IndexOf(complementaryId)
                join cofinancingCashInKindId in cofinancingCashInKinds
                    on cofinancingFundingSource.IndexOf(fundSource) equals
                    cofinancingCashInKinds.IndexOf(cofinancingCashInKindId)
                join cofinancingStatu in cofinancingStatus
                    on cofinancingFundingSource.IndexOf(fundSource) equals
                    cofinancingStatus.IndexOf(cofinancingStatu)
                select new CofinancingResourcesDetailViewModel
                {
                    Id = int.Parse(complementaryId.Value),
                    ModalityTypeCode = modality.Value,
                    FundingSourceCode = fundSource.Value,
                    DescriptionResources = description.Value,
                    SignatureDate = DateTime.TryParse(signatureDate.Value, out auxDateTime)
                        ? auxDateTime
                        : (DateTime?)null,
                    Amount = decimal.Parse(amount.Value),
                    CashKindCode = cofinancingCashInKindId.Value,
                    StatusTypeCode = cofinancingStatu.Value
                }).ToList();

            return response;
        }

        private static IEnumerable<string> GetDonorsForRemoveMilestone(
            FinancialDataPreparationViewModel model)
        {
            var availablePsgDonorsIds = model.AvailablePsgDonors
                .Select(exp => exp.DonorId.ToString())
                .Distinct();

            var unavailablePsgDonorsIds = model.UnavailablePsgDonors
                .Select(exp => exp.DonorId.ToString())
                .Distinct();

            var currentDonors = model.ExpectedIDB
                .Select(exp => exp.Donors)
                .SelectMany(don => don.Split(','))
                .Where(don => !string.IsNullOrEmpty(don))
                .Distinct();

            return availablePsgDonorsIds
                .Union(unavailablePsgDonorsIds).Distinct()
                .Except(currentDonors);
        }

        static IEnumerable<RelatedOperationRowViewModel> ResolveRowsOfRelatedOperations(
            ClientFieldData[] clientFieldData)
        {
            var models = new List<RelatedOperationRowViewModel>();

            var rowsData = clientFieldData
                .Where(cfd => !string.IsNullOrWhiteSpace(cfd.Name) &&
                    (cfd.Name == RELATED_OPERATION_ID ||
                        cfd.Name == OPERATION_RELATED_ID ||
                        cfd.Name == RELATED_RELATION_ROLE_ID ||
                        cfd.Name == RELATED_RELATION_TYPE_ID ||
                        cfd.Name == RELATED_START_DATE ||
                        cfd.Name == RELATED_END_DATE ||
                        cfd.Name == RELATED_USER_COMMENT_ID ||
                        cfd.Name == RELATED_COMMENTS ||
                        cfd.Name == RELATED_OPERATION_RELATIONSHIP_ID ||
                        cfd.Name == RELATIONSHIP_ROLE_THIS_OPERATION_ID));

            if (!rowsData.HasAny())
                return models;

            rowsData.GroupBy(cdf => cdf.Id)
                .Where(gr => !string.IsNullOrEmpty(gr.Key))
                .ForEach(rowData =>
                {
                    models.Add(new RelatedOperationRowViewModel
                    {
                        RowId = rowData.ParseToIntByFieldName(RELATED_OPERATION_ID),
                        OperationRelatedId = rowData.ParseToIntByFieldName(OPERATION_RELATED_ID),
                        RelationshipRoleId = rowData
                            .ParseToIntByFieldName(RELATED_RELATION_ROLE_ID),
                        RelationshipTypeId = rowData
                            .ParseToIntByFieldName(RELATED_RELATION_TYPE_ID),
                        StartDate = rowData.ParseToDateTimeByFieldName(RELATED_START_DATE),
                        EndDate = rowData.ParseToDateTimeByFieldName(RELATED_END_DATE),
                        UserCommentId = rowData.ParseToIntByFieldName(RELATED_USER_COMMENT_ID),
                        Comments = rowData.GetValueByFieldName(RELATED_COMMENTS),
                        OperationRelationshipId = rowData
                            .ParseToIntByFieldName(RELATED_OPERATION_RELATIONSHIP_ID),
                        RelationShipRoleThisOperationId = rowData
                            .ParseToIntByFieldName(RELATIONSHIP_ROLE_THIS_OPERATION_ID),
                    });
                });

            return models.Where(m => m.RelationShipRoleThisOperationId > 0);
        }

        static int ParseToIntByFieldName(
            this IGrouping<string, ClientFieldData> group,
            string fieldName)
        {
            return int.Parse(group.First(cdf => cdf.Name == fieldName).Value);
        }

        static DateTime ParseToDateTimeByFieldName(
            this IGrouping<string, ClientFieldData> group,
            string fieldName)
        {
            var fieldValue = group.First(cdf => cdf.Name == fieldName).Value;

            if (string.IsNullOrEmpty(fieldValue))
                return DateTime.MinValue;

            return DateTime.Parse(fieldValue);
        }

        static string GetValueByFieldName(
            this IGrouping<string, ClientFieldData> group,
            string fieldName)
        {
            var field = group.FirstOrDefault(cdf => cdf.Name == fieldName);

            if (field == null)
                return string.Empty;

            return field.Value;
        }
        #endregion      
    }
}