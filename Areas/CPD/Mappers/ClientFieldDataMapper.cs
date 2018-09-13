using System;
using System.Collections.Generic;
using System.Linq;

using IDB.MW.Application.CPDModule.ViewModels.Annexes;
using IDB.MW.Application.CPDModule.ViewModels.Core;
using IDB.MW.Application.CPDModule.ViewModels.CountryProgram;
using IDB.MW.Application.CPDModule.ViewModels.CSProgressToDate;
using IDB.MW.Application.CPDModule.ViewModels.ProductProfile;
using IDB.MW.Application.OPUSModule.ViewModels.Common;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVC4.Models.ClientFieldData;

namespace IDB.Presentation.MVC4.Areas.CPD.Mappers
{
    public static class ClientFieldDataMapper
    {
        #region Constants
        public static string RESPONSIBLE_UNITS_ROLE
        {
            get { return "responsibleUnitsRole"; }
        }

        public static string RESPONSIBLE_UNITS_ORG_UNIT
        {
            get { return "responsibleUnitsOrganizationalUnit"; }
        }

        public static string RESPONSIBLE_UNITS_EFFORT_DAYS
        {
            get { return "responsibleUnitsEffortInDays"; }
        }

        public static string USER_NAME_TEAM
        {
            get { return "userNameTeam"; }
        }

        public static string ORG_UNIT_CODE
        {
            get { return "OrganizationalUnitCode"; }
        }

        public static string USER_NAME_TEAM_TEXT
        {
            get { return "userNameTeam_text"; }
        }

        public static string OPERATION_TEAMS_ROLE
        {
            get { return "operationTeamsRole"; }
        }

        public static string NEW
        {
            get { return "new-"; }
        }

        public static string TRUE
        {
            get { return "True"; }
        }

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
        private const string COFINANCING_ID = "CoFinancingId";
        private const string COFINANCING_NAME = "CoFinancingName";
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
        private const string SUFFIX = "Suffix";
        private const string RESOLUTION_NUMBER = "ResolutionNumber";
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
        private const string PEP_NOT_INTEGRATED = "PlanningInstrumentsIntegratedCheck";
        #endregion

        #region Mappers

        #region ProgressToDate
        public static ProgressToDateTabViewModel UpdateProgressToDateTabViewModel(this ProgressToDateTabViewModel model, ClientFieldData[] formData)
        {
            var field = formData.FirstOrDefault(x => x.Name == "checkCSUpdateApprovedToDate");
            model.CSUpdatesApprovedToDate = field.ConvertToBool();

            field = formData.FirstOrDefault(x => x.Name == "idbLending-CommentArea");
            model.IDBLendingComment = field.ConvertToString();

            field = formData.FirstOrDefault(x => x.Name == "portfolioAlignment-CommentArea");
            model.PortfolioAlignmentComment = field.ConvertToString();

            field = formData.FirstOrDefault(x => x.Name == "highlightsComment");
            model.Highlights = field.ConvertToString();

            //Objectives
            model.CountryStrategyObjective.Clear();

            field = formData.FirstOrDefault(x => x.Name == "objectives");
            var objectivesSelected = field.ConvertToIntList();

            foreach (var itemObjective in objectivesSelected)
            {
                field = formData.FirstOrDefault(x => x.Id == itemObjective.ToString());
                model.CountryStrategyObjective.Add(itemObjective, field.ConvertToString());
            }

            return model;
        }
        #endregion

        #region UseOfCountrySystemsProgressToDate
        public static CSPDUseOfCountrySystemsTabViewModel UpdateUseOfCountrySystemsTabCSViewModel(this CSPDUseOfCountrySystemsTabViewModel model, ClientFieldData[] formData)
        {
            //Strenghening table
            model.StrengtheningInitiatives.Clear();

            var inputsUseCSStrengtheing = new string[]
            {
                "ForeseenActions",
                "comment",
                "systemType"
            };
            ClientFieldData field;

            var fieldsUseCSStrengtheing = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputsUseCSStrengtheing.Any(y => x.Name == y));
            var fieldsUseCSStrengtheingGrouped = fieldsUseCSStrengtheing.GroupBy(x => x.Id);

            foreach (var useCSStrengtheingItem in fieldsUseCSStrengtheingGrouped)
            {
                var itemId = useCSStrengtheingItem.Key.Contains("new") ? 0 : useCSStrengtheingItem.Key.ConvertToInt(0, ConvertModeEnum.ThrowExceptionInFail);

                var useCSStrengtheingModel = new SystemRowViewModel() { RowId = itemId };
                model.StrengtheningInitiatives.Add(useCSStrengtheingModel);

                field = useCSStrengtheingItem.FirstOrDefault(x => x.Name == inputsUseCSStrengtheing[0]);
                useCSStrengtheingModel.ForeSeenActionIds = field.ConvertToIntList();

                field = useCSStrengtheingItem.FirstOrDefault(x => x.Name == inputsUseCSStrengtheing[1]);
                useCSStrengtheingModel.Comment = field.ConvertToString();

                field = useCSStrengtheingItem.FirstOrDefault(x => x.Name == inputsUseCSStrengtheing[2]);
                useCSStrengtheingModel.RowTypeId = field.ConvertToInt();
            }

            //procurement
            var fields = formData.Where(x => x.Name == "procurementYear");
            model.Procurements = fields.ConvertToIntDictionary();

            field = formData.FirstOrDefault(x => x.Name == "procurementComment");
            model.ProcurementComment = field.ConvertToString();

            //Financial
            fields = formData.Where(x => x.Name == "financialManagmentYear");
            model.FinancialManagements = fields.ConvertToIntDictionary();

            field = formData.FirstOrDefault(x => x.Name == "financialManagementComment");
            model.FinancialManagementComment = field.ConvertToString();

            return model;
        }
        #endregion

        #region CountryProgram
        public static CountryProgramTabViewModel UpdateCountryProgramTabViewModel(this CountryProgramTabViewModel model, ClientFieldData[] formData)
        {
            // Expected Approvals 
            var field = formData.FirstOrDefault(x => x.Name == "expectedApproval-CommentArea");
            model.ExpectedApprovalComment = field.ConvertToString();

            // Expected Disbursements
            model.ExpectedDisbursements.Clear();

            var inputsExpectedDisbursements = new string[]
            {
               "expectedDisbursement-SGNumberOperations",
               "expectedDisbursement-SGTotalAmount",
               "expectedDisbursement-NSGNumberOperations",
               "expectedDisbursement-NSGTotalAmount",
            };

            var fieldsExpectedDisbursements = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputsExpectedDisbursements.Any(y => x.Name == y));
            var fieldsExpectedDisbursementsGrouped = fieldsExpectedDisbursements.GroupBy(x => x.Id);

            foreach (var fieldExpectedDisbursement in fieldsExpectedDisbursementsGrouped)
            {
                var itemId = fieldExpectedDisbursement.Key.Contains("new") ? 0 : fieldExpectedDisbursement.Key.ConvertToInt(0, ConvertModeEnum.ThrowExceptionInFail);

                var expectedDisbursementsModel = new ExpectedDisbursementsRowViewModel() { RowId = itemId };
                model.ExpectedDisbursements.Add(expectedDisbursementsModel);

                field = fieldExpectedDisbursement.FirstOrDefault(x => x.Name == inputsExpectedDisbursements[0]);
                expectedDisbursementsModel.SGNumberOfOperations = field.ConvertToNullableInt();

                field = fieldExpectedDisbursement.FirstOrDefault(x => x.Name == inputsExpectedDisbursements[1]);
                expectedDisbursementsModel.SGTotalAmount = field.ConvertToNullableDecimal();

                field = fieldExpectedDisbursement.FirstOrDefault(x => x.Name == inputsExpectedDisbursements[2]);
                expectedDisbursementsModel.NSGNumberOfOperations = field.ConvertToNullableInt();

                field = fieldExpectedDisbursement.FirstOrDefault(x => x.Name == inputsExpectedDisbursements[3]);
                expectedDisbursementsModel.NSGTotalAmount = field.ConvertToNullableDecimal();
            }

            // Portfolio Adjustments Foreseen in the CPD Year
            model.CancelationOfProjects.Clear();

            var inputsCancelationOfProjects = new string[]
            {
               "cancelationProject-AdjustmentTypeId",
               "cancelationProject-OperationName",
               "cancelationProject-OperationNumber",
               "cancelationProject-OriginalValue",
               "cancelationProject-CancelationAmount",
               "cancelationProject-Comment",
            };

            var fieldsCancelationOfProjects = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputsCancelationOfProjects.Any(y => x.Name == y));
            var fieldsCancelationOfProjectsGrouped = fieldsCancelationOfProjects.GroupBy(x => x.Id);

            foreach (var cancelationItem in fieldsCancelationOfProjectsGrouped)
            {
                var itemId = cancelationItem.Key.Contains("new") ? 0 : cancelationItem.Key.ConvertToInt(0, ConvertModeEnum.ThrowExceptionInFail);

                var cancelationOfProjectsModel = new PortfolioAdjustmentsForeseenRowViewModel() { RowId = itemId };
                model.CancelationOfProjects.Add(cancelationOfProjectsModel);

                field = cancelationItem.FirstOrDefault(x => x.Name == inputsCancelationOfProjects[0]);
                cancelationOfProjectsModel.AdjustmentTypeId = field.ConvertToInt();

                field = cancelationItem.FirstOrDefault(x => x.Name == inputsCancelationOfProjects[1]);
                cancelationOfProjectsModel.OperationName = field.ConvertToString();

                field = cancelationItem.FirstOrDefault(x => x.Name == inputsCancelationOfProjects[2]);
                cancelationOfProjectsModel.OperationNumber = field.ConvertToString();

                field = cancelationItem.FirstOrDefault(x => x.Name == inputsCancelationOfProjects[3]);
                cancelationOfProjectsModel.OriginalValue = field.ConvertToNullableDecimal();

                field = cancelationItem.FirstOrDefault(x => x.Name == inputsCancelationOfProjects[4]);
                cancelationOfProjectsModel.CancelationAmount = field.ConvertToNullableDecimal();

                field = cancelationItem.FirstOrDefault(x => x.Name == inputsCancelationOfProjects[5]);
                cancelationOfProjectsModel.Comment = field.ConvertToString();
            }

            // Reformulation of Projects
            model.ReformulationOfProjects.Clear();

            var inputsReformulationOfProjects = new string[]
            {
               "reformulationProject-AdjustmentTypeId",
               "reformulationProject-OperationName",
               "reformulationProject-OperationNumber",
               "reformulationProject-OriginalValue",
               "reformulationProject-CancelationAmount",
               "reformulationProject-Comment",
            };

            var fieldsReformulationOfProjects = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputsReformulationOfProjects.Any(y => x.Name == y));
            var fieldsReformulationOfProjectsGrouped = fieldsReformulationOfProjects.GroupBy(x => x.Id);

            foreach (var reformulationItem in fieldsReformulationOfProjectsGrouped)
            {
                var itemId = reformulationItem.Key.Contains("new") ? 0 : reformulationItem.Key.ConvertToInt(0, ConvertModeEnum.ThrowExceptionInFail);

                var reformulationOfProjectsModel = new PortfolioAdjustmentsForeseenRowViewModel() { RowId = itemId };
                model.ReformulationOfProjects.Add(reformulationOfProjectsModel);

                field = reformulationItem.FirstOrDefault(x => x.Name == inputsReformulationOfProjects[0]);
                reformulationOfProjectsModel.AdjustmentTypeId = field.ConvertToInt();

                field = reformulationItem.FirstOrDefault(x => x.Name == inputsReformulationOfProjects[1]);
                reformulationOfProjectsModel.OperationName = field.ConvertToString();

                field = reformulationItem.FirstOrDefault(x => x.Name == inputsReformulationOfProjects[2]);
                reformulationOfProjectsModel.OperationNumber = field.ConvertToString();

                field = reformulationItem.FirstOrDefault(x => x.Name == inputsReformulationOfProjects[3]);
                reformulationOfProjectsModel.OriginalValue = field.ConvertToNullableDecimal();

                field = reformulationItem.FirstOrDefault(x => x.Name == inputsReformulationOfProjects[4]);
                reformulationOfProjectsModel.CancelationAmount = field.ConvertToNullableDecimal();

                field = reformulationItem.FirstOrDefault(x => x.Name == inputsReformulationOfProjects[5]);
                reformulationOfProjectsModel.Comment = field.ConvertToString();
            }

            // Risks/Factors 
            field = formData.FirstOrDefault(x => x.Name == "risksFactorsText");
            model.RiskFactors = field.ConvertToString();

            return model;
        }
        #endregion

        #region UseOfCountrySystemsProgram
        public static UseOfCountrySystemsTabViewModel UpdateUseOfCountrySystemsTabViewModel(this UseOfCountrySystemsTabViewModel model, ClientFieldData[] formData)
        {
            //Strenghening table
            model.StrengtheningInitiatives.Clear();

            var inputsUseCSStrengtheing = new string[]
            {
                "ForeseenActions",
                "comment",
                "SystemType"
            };
            ClientFieldData field;

            var fieldsUseCSStrengtheing = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputsUseCSStrengtheing.Any(y => x.Name == y));
            var fieldsUseCSStrengtheingGrouped = fieldsUseCSStrengtheing.GroupBy(x => x.Id);

            foreach (var useCSStrengtheingItem in fieldsUseCSStrengtheingGrouped)
            {
                var itemId = useCSStrengtheingItem.Key.Contains("new") ? 0 : useCSStrengtheingItem.Key.ConvertToInt(0, ConvertModeEnum.ThrowExceptionInFail);

                var useCSStrengtheingModel = new SystemRowViewModel() { RowId = itemId };
                model.StrengtheningInitiatives.Add(useCSStrengtheingModel);

                field = useCSStrengtheingItem.FirstOrDefault(x => x.Name == inputsUseCSStrengtheing[0]);
                useCSStrengtheingModel.ForeSeenActionIds = field.ConvertToIntList();

                field = useCSStrengtheingItem.FirstOrDefault(x => x.Name == inputsUseCSStrengtheing[1]);
                useCSStrengtheingModel.Comment = field.ConvertToString();

                field = useCSStrengtheingItem.FirstOrDefault(x => x.Name == inputsUseCSStrengtheing[2]);
                useCSStrengtheingModel.RowTypeId = field.ConvertToInt();
            }

            //procurement
            var fields = formData.Where(x => x.Name == "procurementYear");
            model.DiagProcurements = fields.ConvertToIntDictionary();

            field = formData.FirstOrDefault(x => x.Name == "procurementComment");
            model.ProcurementComment = field.ConvertToString();

            //Financial
            fields = formData.Where(x => x.Name == "financialManagmentYear");
            model.DiagFinancialManagements = fields.ConvertToIntDictionary();

            field = formData.FirstOrDefault(x => x.Name == "financialManagementComment");
            model.FinancialManagementComment = field.ConvertToString();

            //Financial Table
            model.FinancialManagements.UseOfCountrySystemsList.Clear();

            var inputsFinanacialTable = new string[]
            {
                "FinancialManagement-ProgrammingYear",
                "FinancialManagement-ForeseenActions",
                "FinancialManagement-UseOfCountryType"
            };

            var fieldsFinancialManagementsTable = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputsFinanacialTable.Any(y => x.Name == y));
            var fieldsFinancialManagementsTableGrouped = fieldsFinancialManagementsTable.GroupBy(x => x.Id);

            foreach (var financialManaguementTableItem in fieldsFinancialManagementsTableGrouped)
            {
                var itemId = financialManaguementTableItem.Key.Contains("new") ? 0 : financialManaguementTableItem.Key.ConvertToInt(0, ConvertModeEnum.ThrowExceptionInFail);

                var financialManaguementTableModel = new UseOfCountrySystemsRowViewModel() { RowId = itemId };
                model.FinancialManagements.UseOfCountrySystemsList.Add(financialManaguementTableModel);

                field = financialManaguementTableItem.FirstOrDefault(x => x.Name == inputsFinanacialTable[0]);
                financialManaguementTableModel.ProgrammingYear = field.ConvertToNullableInt();

                field = financialManaguementTableItem.FirstOrDefault(x => x.Name == inputsFinanacialTable[1]);
                financialManaguementTableModel.ForeseenActionIds = field.ConvertToIntList();

                field = financialManaguementTableItem.FirstOrDefault(x => x.Name == inputsFinanacialTable[2]);
                financialManaguementTableModel.RowTypeId = field.ConvertToInt();
            }

            //Procurement table
            model.Procurements.UseOfCountrySystemsList.Clear();

            var inputsProcurementTable = new string[]
            {
                "Procurements-ProgrammingYear",
                "Procurements-ForeseenActions",
                "Procurements-UseOfCountryType"
            };

            var fieldsProcurementTable = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputsProcurementTable.Any(y => x.Name == y));
            var fieldsProcurementTableGrouped = fieldsProcurementTable.GroupBy(x => x.Id);

            foreach (var procurementTableItem in fieldsProcurementTableGrouped)
            {
                var itemId = procurementTableItem.Key.Contains("new") ? 0 : procurementTableItem.Key.ConvertToInt(0, ConvertModeEnum.ThrowExceptionInFail);

                var procurementTableModel = new UseOfCountrySystemsRowViewModel() { RowId = itemId };
                model.Procurements.UseOfCountrySystemsList.Add(procurementTableModel);

                field = procurementTableItem.FirstOrDefault(x => x.Name == inputsProcurementTable[0]);
                procurementTableModel.ProgrammingYear = field.ConvertToNullableInt();

                field = procurementTableItem.FirstOrDefault(x => x.Name == inputsProcurementTable[1]);
                procurementTableModel.ForeseenActionIds = field.ConvertToIntList();

                field = procurementTableItem.FirstOrDefault(x => x.Name == inputsProcurementTable[2]);
                procurementTableModel.RowTypeId = field.ConvertToInt();
            }

            return model;
        }
        #endregion

        #region StrategicAlignment
        public static StrategicAlignmentTabViewModel UpdateStrategicAlignmentTabViewModel(this StrategicAlignmentTabViewModel model, ClientFieldData[] formData, string operationNumber)
        {
            ClientFieldData field;

            model.StrategicAlignmentDEMToCSs.StrategicAlignmentCPDToCSs.Clear();
            model.ResponsivenessToChangingCountryNeeds.Clear();
            model.CountryStrategicImplementations.Clear();
            model.StrategicAlignmentIndicators.Clear();

            var inputsStrategicAlignmentCPDToCSs = new string[]
            {
                "StrategicAlignmentCPDToCSs-RowTypeId",
                "StrategicAlignmentCPDToCSs-Percentage",
                "StrategicAlignmentCPDToCSs-Quantity",
                "StrategicAlignmentCPDToCSs-TotalQuantity",
            };

            var inputsResponsivenessToChangingCountryNeeds = new string[]
            {
                "ResponsivenessToChangingCountryNeeds-RowTypeId",
                "ResponsivenessToChangingCountryNeeds-Percentage",
                "ResponsivenessToChangingCountryNeeds-Quantity",
                "ResponsivenessToChangingCountryNeeds-TotalQuantity",
            };

            var inputsCountryStrategicImplementations = new string[]
            {
                "CountryStrategicImplementations-RowTypeId",
                "CountryStrategicImplementations-Percentage",
                "CountryStrategicImplementations-Quantity",
                "CountryStrategicImplementations-TotalQuantity",
            };

            var inputsStrategicAlignmentIndicators = new string[]
            {
                "CRFStrategicAlignmentIndicators-RowTypeId",
                "CRFStrategicAlignmentIndicators-Percentage",
            };

            var fieldsStrategicAlignmentCPDToCSs = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputsStrategicAlignmentCPDToCSs.Any(y => x.Name == y)).ToList();
            var fieldsStrategicAlignmentCPDToCSsGrouped = fieldsStrategicAlignmentCPDToCSs.GroupBy(x => x.Id);

            foreach (var fieldsStrategicAlignmentCPDToCSsItem in fieldsStrategicAlignmentCPDToCSsGrouped)
            {
                var itemId = fieldsStrategicAlignmentCPDToCSsItem.Key.ConvertToInt();
                var strategicAlignmentCPDToCSRow = new DEMSummaryRowViewModel() { RowId = itemId };
                model.StrategicAlignmentDEMToCSs.StrategicAlignmentCPDToCSs.Add(strategicAlignmentCPDToCSRow);

                field = fieldsStrategicAlignmentCPDToCSsItem.FirstOrDefault(x => x.Name == "StrategicAlignmentCPDToCSs-RowTypeId");
                strategicAlignmentCPDToCSRow.RowTypeId = field.ConvertToInt();

                field = fieldsStrategicAlignmentCPDToCSsItem.FirstOrDefault(x => x.Name == "StrategicAlignmentCPDToCSs-Percentage");
                strategicAlignmentCPDToCSRow.Percentage = field.ConvertToNullableDecimal();

                field = fieldsStrategicAlignmentCPDToCSsItem.FirstOrDefault(x => x.Name == "StrategicAlignmentCPDToCSs-Quantity");
                strategicAlignmentCPDToCSRow.Quantity = field.ConvertToNullableInt();

                field = fieldsStrategicAlignmentCPDToCSsItem.FirstOrDefault(x => x.Name == "StrategicAlignmentCPDToCSs-TotalQuantity");
                strategicAlignmentCPDToCSRow.TotalQuantity = field.ConvertToNullableInt();
            }

            var fieldsResponsivenessToChangingCountryNeeds = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputsResponsivenessToChangingCountryNeeds.Any(y => x.Name == y)).ToList();
            var fieldsResponsivenessToChangingCountryNeedsGrouped = fieldsResponsivenessToChangingCountryNeeds.GroupBy(x => x.Id);

            foreach (var fieldsResponsivenessToChangingCountryNeedsItem in fieldsResponsivenessToChangingCountryNeedsGrouped)
            {
                var itemId = fieldsResponsivenessToChangingCountryNeedsItem.Key.ConvertToInt();
                var responsivenessToChangingCountryNeeds = new DEMSummaryRowViewModel() { RowId = itemId };
                model.ResponsivenessToChangingCountryNeeds.Add(responsivenessToChangingCountryNeeds);

                field = fieldsResponsivenessToChangingCountryNeedsItem.FirstOrDefault(x => x.Name == "ResponsivenessToChangingCountryNeeds-RowTypeId");
                responsivenessToChangingCountryNeeds.RowTypeId = field.ConvertToInt();

                field = fieldsResponsivenessToChangingCountryNeedsItem.FirstOrDefault(x => x.Name == "ResponsivenessToChangingCountryNeeds-Percentage");
                responsivenessToChangingCountryNeeds.Percentage = field.ConvertToNullableDecimal();

                field = fieldsResponsivenessToChangingCountryNeedsItem.FirstOrDefault(x => x.Name == "ResponsivenessToChangingCountryNeeds-Quantity");
                responsivenessToChangingCountryNeeds.Quantity = field.ConvertToNullableInt();

                field = fieldsResponsivenessToChangingCountryNeedsItem.FirstOrDefault(x => x.Name == "ResponsivenessToChangingCountryNeeds-TotalQuantity");
                responsivenessToChangingCountryNeeds.TotalQuantity = field.ConvertToNullableInt();
            }

            field = formData.FirstOrDefault(x => x.Name == "isGreather2017");
            var isGreather2017 = field.ConvertToBool(false);

            if (!isGreather2017)
            {
                var fieldsCountryStrategicImplementations = formData.Where(x =>
                    !string.IsNullOrWhiteSpace(x.Id) &&
                    inputsCountryStrategicImplementations.Any(y => x.Name == y))
                    .ToList();

                var fieldsCountryStrategicImplementationsGrouped =
                    fieldsCountryStrategicImplementations.GroupBy(x => x.Id);

                foreach (var fieldsCountryStrategicImplementationsGroupedItem in fieldsCountryStrategicImplementationsGrouped)
                {
                    var itemId = fieldsCountryStrategicImplementationsGroupedItem.Key.ConvertToInt();
                    var countryStrategicImplementations = new DEMSummaryRowViewModel() { RowId = itemId };
                    model.CountryStrategicImplementations.Add(countryStrategicImplementations);

                    field = fieldsCountryStrategicImplementationsGroupedItem.FirstOrDefault(x => x.Name == "CountryStrategicImplementations-RowTypeId");
                    countryStrategicImplementations.RowTypeId = field.ConvertToInt();

                    field = fieldsCountryStrategicImplementationsGroupedItem.FirstOrDefault(x => x.Name == "CountryStrategicImplementations-Percentage");
                    countryStrategicImplementations.Percentage = field.ConvertToNullableDecimal();

                    field = fieldsCountryStrategicImplementationsGroupedItem.FirstOrDefault(x => x.Name == "CountryStrategicImplementations-Quantity");
                    countryStrategicImplementations.Quantity = field.ConvertToNullableInt();

                    field = fieldsCountryStrategicImplementationsGroupedItem.FirstOrDefault(x => x.Name == "CountryStrategicImplementations-TotalQuantity");
                    countryStrategicImplementations.TotalQuantity = field.ConvertToNullableInt();
                }
            }

            var fieldsCRFStrategicAlignmentIndicators = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputsStrategicAlignmentIndicators.Any(y => x.Name == y)).ToList();
            var fieldsCRFStrategicAlignmentIndicatorsGrouped = fieldsCRFStrategicAlignmentIndicators.GroupBy(x => x.Id);

            foreach (var fieldsCRFStrategicAlignmentIndicatorsItem in fieldsCRFStrategicAlignmentIndicatorsGrouped)
            {
                var itemId = fieldsCRFStrategicAlignmentIndicatorsItem.Key.ConvertToInt();
                var strategicAlignmentIndicatorRow = new StrategicAlignmentIndicatorRowViewModel() { RowId = itemId };
                model.StrategicAlignmentIndicators.Add(strategicAlignmentIndicatorRow);

                field = fieldsCRFStrategicAlignmentIndicatorsItem.FirstOrDefault(x => x.Name == "CRFStrategicAlignmentIndicators-RowTypeId");
                strategicAlignmentIndicatorRow.RowTypeId = field.ConvertToInt();

                field = fieldsCRFStrategicAlignmentIndicatorsItem.FirstOrDefault(x => x.Name == "CRFStrategicAlignmentIndicators-Percentage");
                strategicAlignmentIndicatorRow.PercentageSG = field.ConvertToNullableDecimal();
            }

            field = formData.FirstOrDefault(x => x.Name == "saIndicators-CommentArea");
            model.SAIndicatorsComment = field.ConvertToString();

            return model;
        }
        #endregion

        #region Indicative Pipeline
        public static IndicativePipelineTabViewModel UpdateIndicativePipelineTabViewModel(this IndicativePipelineTabViewModel model, ClientFieldData[] formData)
        {
            ClientFieldData field;

            var sectionName = "idbIICFinanOp";
            var inputsIICFinancialOperations = new string[]
            {
                sectionName + "-OperationName",
                sectionName + "-OperationNumber",
                sectionName + "-OperationObjetive",
                sectionName + "-Amount"
            };

            model.IICFinancialOperations.Operations.Clear();

            var fieldsIndicativePipeline = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputsIICFinancialOperations.Any(y => x.Name == y));
            var fieldsIndicativePipelineGrouped = fieldsIndicativePipeline.GroupBy(x => x.Id);

            foreach (var operationItem in fieldsIndicativePipelineGrouped)
            {
                var itemId = operationItem.Key.Contains("new") ? 0 : operationItem.Key.ConvertToInt(mode: ConvertModeEnum.ThrowExceptionInFail);

                var iicFinancialOperationsModel = new OperationRowViewModel() { RowId = itemId };
                model.IICFinancialOperations.Operations.Add(iicFinancialOperationsModel);

                field = operationItem.FirstOrDefault(x => x.Name == inputsIICFinancialOperations[0]);
                iicFinancialOperationsModel.OperationName = field.ConvertToString();

                field = operationItem.FirstOrDefault(x => x.Name == inputsIICFinancialOperations[1]);
                iicFinancialOperationsModel.OperationNumber = field.ConvertToString();

                field = operationItem.FirstOrDefault(x => x.Name == inputsIICFinancialOperations[2]);
                iicFinancialOperationsModel.OperationObjective = field.ConvertToString();

                field = operationItem.FirstOrDefault(x => x.Name == inputsIICFinancialOperations[3]);
                iicFinancialOperationsModel.Amount = field.ConvertToDecimal(mode: ConvertModeEnum.ThrowExceptionInFail);
            }

            return model;
        }
        #endregion

        #region Annexes Result Matrix
        public static ResultMatrixTabViewModel Update(this ResultMatrixTabViewModel model, ClientFieldData[] formData)
        {
            var resultMatrixFieldNames = new string[]
            {
                "interventions-ucscpd",
                "outcomesAchieved-ucscpd",
                "expectedOutcomeOutput-ucscpd",
            };

            var fieldsGroupedForObjectives = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && resultMatrixFieldNames.Contains(x.Name));
            var fieldsGroupedForObjective = fieldsGroupedForObjectives.GroupBy(x => x.Id);

            model.StrategicObjectives.Clear();

            foreach (var fieldsForObjective in fieldsGroupedForObjective)
            {
                var objectiveId = fieldsForObjective.Key.ConvertToInt(0);

                var objective = new StrategicObjectiveRowViewModel()
                {
                    ObjectiveId = objectiveId
                };

                var fieldsGroupedForOutcome = fieldsForObjective
                                             .Where(x => x.ExtraData.ContainsKey("data-persist-outcomeid"))
                                             .GroupBy(x => x.ExtraData.Single(y => y.Key == "data-persist-outcomeid"));

                foreach (var fieldsForOutcome in fieldsGroupedForOutcome)
                {
                    var outcomeId = fieldsForOutcome.Key.Value.ConvertToInt(0);

                    var fieldsGroupedForIndicator = fieldsForOutcome
                                                    .Where(x => x.ExtraData.ContainsKey("data-persist-indicatorid"))
                                                    .GroupBy(x => x.ExtraData.Single(y => y.Key == "data-persist-indicatorid"));

                    foreach (var fieldsForIndicator in fieldsGroupedForIndicator)
                    {
                        var indicatorId = fieldsForIndicator.Key.Value.ConvertToInt(0);

                        var outcomeIndicator = new StrategicOutcomeRowViewModel();

                        outcomeIndicator.ExpectedOutcomeId = outcomeId;
                        outcomeIndicator.IndicatorId = indicatorId;
                        outcomeIndicator.ObjectiveId = objectiveId;

                        var field = fieldsForIndicator.FirstOrDefault(x => x.Name == resultMatrixFieldNames[0]);
                        outcomeIndicator.InterventionsProgrammingYear = field.ConvertToString();

                        field = fieldsForIndicator.FirstOrDefault(x => x.Name == resultMatrixFieldNames[1]);
                        outcomeIndicator.OutcomesAchieved = field.ConvertToString();

                        field = fieldsForIndicator.FirstOrDefault(x => x.Name == resultMatrixFieldNames[2]);
                        outcomeIndicator.ExpectedOutcomeOutput = field.ConvertToString();

                        objective.StrategicOutcomes.Add(outcomeIndicator);
                    }
                }

                model.StrategicObjectives.Add(objective);
            }

            return model;
        }
        #endregion

        #region ResponsibilityUpdate
        public static ResponsibilityDataViewModel UpdateProductDataResponsibilityData(this ResponsibilityDataViewModel model, ClientFieldData[] formData)
        {
            var inputsUnits = new string[]
            {
                RESPONSIBLE_UNITS_ROLE,
                RESPONSIBLE_UNITS_ORG_UNIT,
                RESPONSIBLE_UNITS_EFFORT_DAYS,
            };

            var unitsGrouped = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputsUnits.Any(y => x.Name == y)).GroupBy(x => x.Id);
            model.ResponsibilityUnits.Clear();

            foreach (var unitData in unitsGrouped)
            {
                var unit = new ResponsibleRowViewModel();
                model.ResponsibilityUnits.Add(unit);

                if (!unitData.Key.Contains(NEW))
                {
                    unit.RowId = int.Parse(unitData.Key);
                }

                var fieldCode = unitData.FirstOrDefault(x => x.Name == RESPONSIBLE_UNITS_ROLE);
                unit.RoleId = fieldCode.ConvertToInt();

                var field = unitData.FirstOrDefault(x => x.Name == RESPONSIBLE_UNITS_ORG_UNIT);
                unit.OrganizationalUnitId = field.ConvertToInt();

                field = unitData.FirstOrDefault(x => x.Name == RESPONSIBLE_UNITS_EFFORT_DAYS);
                if (field != null)
                {
                    unit.EffortInDays = field.ConvertToInt();
                }
            }

            var inputsTeams = new string[]
            {
                USER_NAME_TEAM,
                ORG_UNIT_CODE,
                USER_NAME_TEAM_TEXT,
                OPERATION_TEAMS_ROLE,
            };

            var teamsGrouped = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputsTeams.Any(y => x.Name == y)).GroupBy(x => x.Id);
            model.OperationTeam.Clear();
            foreach (var teamData in teamsGrouped)
            {
                var member = new OperationTeamRowViewModel();
                model.OperationTeam.Add(member);

                if (!teamData.Key.Contains(NEW))
                {
                    member.RowId = int.Parse(teamData.Key);
                }
                else
                {
                    var fieldCode = teamData.FirstOrDefault(x => x.Name == ORG_UNIT_CODE);
                    member.OrganizationalUnitCode = fieldCode.ConvertToString();
                }

                var field = teamData.FirstOrDefault(x => x.Name == USER_NAME_TEAM_TEXT);
                member.Name = field.ConvertToString();

                field = teamData.FirstOrDefault(x => x.Name == USER_NAME_TEAM);
                member.UserName = field.ConvertToString();

                field = teamData.FirstOrDefault(x => x.Name == OPERATION_TEAMS_ROLE);
                member.RoleId = field.ConvertToInt();
            }

            return model;
        }
        #endregion

        #region Basic Data
        public static BasicDataViewModel UpdateProductDataBasicData(this BasicDataViewModel model, ClientFieldData[] formData)
        {
            var field = formData.FirstOrDefault(x => x.Name == "isOperationClosed");
            var isOperationClosed = field.ConvertToBool(false);

            if (!isOperationClosed)
            {
                field = formData.FirstOrDefault(x => x.Name == "productYear");
                model.ProductYear = field.ConvertToInt(mode: ConvertModeEnum.ThrowExceptionInFail);

                field = formData.FirstOrDefault(x => x.Name == "ProductNameEn");
                model.OperationNameEn = field.ConvertToString();

                field = formData.FirstOrDefault(x => x.Name == "ProductNameEs");
                model.OperationNameEs = field.ConvertToString();

                field = formData.FirstOrDefault(x => x.Name == "ProductNameFr");
                model.OperationNameFr = field.ConvertToString();

                field = formData.FirstOrDefault(x => x.Name == "ProductNamePt");
                model.OperationNamePt = field.ConvertToString();
            }

            field = formData.FirstOrDefault(x => x.Name == "poprAssociatedIsApproved");
            model.POPRAssociatedIsApproved = field.ConvertToBool();

            if (!model.POPRAssociatedIsApproved)
            {
                if (model.BasicData == null)
                {
                    model.BasicData = new MW.Application.OPUSModule.ViewModels.OperationDataService.BasicDataViewModel()
                    {
                        Relationships = new List<RelationsViewModel>()
                    };
                }

                var inputsOperationRelated = new string[]
                {
                   "OperationRelatedId",
                   "relatedOperationRelationshipId",
                   "relatedRelationRoleId",
                   "relatedComments",
                   "relatioshipTypeCode",
                   "relatioshipTypeId",
                   "operationTypeCode"
                };

                var fieldsInputsOperationRelated = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputsOperationRelated.Any(y => x.Name == y));

                var inputsOperationRelatationshipGrouped = fieldsInputsOperationRelated
                    .Where(x => x.ExtraData.Any() && x.ExtraData.ContainsKey("data-persist-id"))
                    .GroupBy(x => x.ExtraData["data-persist-id"]);

                model.BasicData.Relationships = new List<RelationsViewModel>();

                foreach (var fieldOperationReltatioship in inputsOperationRelatationshipGrouped)
                {
                    var relationShipId = fieldOperationReltatioship.Key.ConvertToInt();

                    var relationShipModel = new RelationsViewModel();

                    field = fieldOperationReltatioship.FirstOrDefault(x => x.Name == inputsOperationRelated[4]);
                    relationShipModel.RelationshipTypeName = field.ConvertToString();

                    field = fieldOperationReltatioship.FirstOrDefault(x => x.Name == inputsOperationRelated[5]);
                    relationShipModel.RelationshipTypeId = field.ConvertToInt();

                    model.BasicData.Relationships.Add(relationShipModel);

                    relationShipModel.RelatedOperations = new List<RelatedOperationRowViewModel>();

                    var fieldsGroupedForOperationRelated = fieldOperationReltatioship.GroupBy(x => x.Id);

                    foreach (var fieldsForOpRelated in fieldsGroupedForOperationRelated)
                    {
                        var relatedOperationRowViewModel = new RelatedOperationRowViewModel();

                        relatedOperationRowViewModel.RowId = fieldsForOpRelated.Key.ConvertToInt(0);

                        field = fieldsForOpRelated.FirstOrDefault(x => x.Name == inputsOperationRelated[0]);
                        relatedOperationRowViewModel.OperationRelatedId = field.ConvertToInt();

                        field = fieldsForOpRelated.FirstOrDefault(x => x.Name == inputsOperationRelated[1]);
                        relatedOperationRowViewModel.OperationRelationshipId = field.ConvertToInt();

                        field = fieldsForOpRelated.FirstOrDefault(x => x.Name == inputsOperationRelated[2]);
                        relatedOperationRowViewModel.RelationshipRoleId = field.ConvertToInt();

                        field = fieldsForOpRelated.FirstOrDefault(x => x.Name == inputsOperationRelated[3]);
                        relatedOperationRowViewModel.Comments = field.ConvertToString();

                        field = fieldsForOpRelated.FirstOrDefault(x => x.Name == inputsOperationRelated[6]);
                        relatedOperationRowViewModel.OperationTypeCode = field.ConvertToString();

                        relationShipModel.RelatedOperations.Add(relatedOperationRowViewModel);
                    }
                }
            }

            return model;
        }
        #endregion
        #endregion

        #region Private Methods
        #endregion
    }
}