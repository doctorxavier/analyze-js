using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

using IDB.MW.Application.BEOProcurementModule.Enums;
using IDB.MW.Application.BEOProcurementModule.ViewModels.FirmProcurement;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Application.ProcurementModule.ViewModels.FirmProcurement;
using IDB.MW.Infrastructure.Configuration;
using IDB.Presentation.MVC4.Areas.BEOProcurement.Helper.ConstantHelper;
using IDB.Architecture.Security.Models.UserIdentity;
using IDB.Presentation.MVC4.Areas.Procurement.Helper.ConstantHelper;
using IDB.Presentation.MVC4.Models.ClientFieldData;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.BEOProcurement.Mappers
{
    public static class ClientFieldDataMapper
    {
        public static readonly string DATETIME_PARSE_FORMAT;

        #region Constructors
        static ClientFieldDataMapper()
        {
            DATETIME_PARSE_FORMAT = ConfigurationServiceFactory.Current.GetApplicationSettings().FormatDateUpload;
        }
        #endregion

        #region Mappers
        #region Procurement List
        public static void UpdateProcurementList(this ProcurementViewModel viewmodel, ClientFieldData[] formData, IEnumMappingService _enumMappingService)
        {
            viewmodel.ProcurementList.Clear();

            var taskSimplified = _enumMappingService.GetMappingCode(ProcurementModalityEnum.TaskSimplifiedSelection);
            var taskSingle = _enumMappingService.GetMappingCode(ProcurementModalityEnum.TaskSingleSourceSelection);
            var itemsList = formData.Where(x => x.Id != null && x.Id != "new-").GroupBy(x => x.Id);

            ClientFieldData field;

            foreach (var item in itemsList)
            {
                var model = new ProcurementRowViewModel();

                field = item.FirstOrDefault(x => x.Name == "IsReadOnly");
                model.IsReadOnly = field.ConvertToBool();

                if (model.IsReadOnly == false)
                {
                    if (!item.Key.Contains("new-"))
                    {
                        model.Id = int.Parse(item.Key);
                    }

                    field = item.FirstOrDefault(x => x.Name == "name");
                    model.Name = field.ConvertToString();

                    field = item.FirstOrDefault(x => x.Name == "stageId");
                    model.StageId = field.ConvertToInt(mode: ConvertModeEnum.ThrowExceptionInFail);

                    field = item.FirstOrDefault(x => x.Name == "statusId");
                    model.StatusId = field.ConvertToInt(
                        mode: ConvertModeEnum.ThrowExceptionInFail);

                    field = item.FirstOrDefault(x => x.Name == "modalityId");
                    model.ModalityId = field.ConvertToInt(
                        mode: ConvertModeEnum.ThrowExceptionInFail);
                    var isTask = model.ModalityId == taskSimplified || model.ModalityId == taskSingle;

                    field = item.FirstOrDefault(x => x.Name == "confidential");
                    model.Confidential = field.ConvertToBool();

                    field = item.FirstOrDefault(x => x.Name == "comments");
                    model.Comments = field.ConvertToString();

                    field = item.FirstOrDefault(x => x.Name == "associatedFramework_text");
                    model.AssociatedFrameworkText = isTask ? field.ConvertToString() : null;

                    field = item.FirstOrDefault(x => x.Name == "associatedFramework");
                    model.AssociatedFrameworkValue = isTask ? field.ConvertToString() : null;

                    viewmodel.ProcurementList.Add(model);
                }
            }
        }
        #endregion

        #region Firm Procurement Tabs
        public static void UpdateIdentification(
            this IdentificationViewModel viewmodel,
            ClientFieldData[] formData)
        {
            if (viewmodel != null)
            {
                viewmodel.CommitFunds.UpdateTCCommitFunds(formData,
                TableValue.IDE_COMMIT_FUNDS_TABLE_NAME);
                viewmodel.CommitFundsSecond.UpdateCommitFunds(formData,
                    TableValue.IDE_COMMIT_FUNDS_SECOND_TABLE_NAME);
                viewmodel.TermsReference.BindDataFormDocsSimpleToModel(formData,
                    TableValue.IDE_TERMS_REFERENCE_TABLE_NAME);
                viewmodel.REOIs.BindReoi(formData, TableValue.IDE_REOIS_TABLE_NAME);
                viewmodel.ParticipatingFirm.UpdateParticipatingFirms(formData,
                    TableValue.IDE_PARTICIPATING_FIRM_TABLE_NAME);
                viewmodel.EvaluationsCommittee.BindDataFormDocsSimpleToModel(formData,
                    TableValue.IDE_EVALUATION_COMITEE_TABLE_NAME);
                viewmodel.Checklist.UpdateCheckList(formData, TableValue.IDE_CHECKLIST_TABLE_NAME);
            }
        }

        public static void UpdatePreparation(this PreparationViewModel viewmodel, ClientFieldData[] formData)
        {
            if (viewmodel != null)
            {
                viewmodel.Communications.BindDataFormDocsSimpleToModel(
                    formData,
                    TableValue.PRE_COMMUNICATIONS_TABLE_NAME);
                viewmodel.RequestProposals.BindDataFormDocsSimpleToModel(
                    formData,
                    TableValue.PRE_REQUST_PROPOSAL_TABLE_NAME);
                viewmodel.Justifications.BindDataFormDocsSimpleToModel(
                    formData,
                    TableValue.PRE_SINGLE_JUSTIFICATION_TABLE_NAME);
                viewmodel.Checklist.UpdateCheckList(formData, TableValue.PRE_CHECKLIST_TABLE_NAME);
            }
        }

        public static void UpdateEvaluation(this FirmProcurementViewModel viewmodel, ClientFieldData[] formData)
        {
            if (viewmodel.Evaluation != null)
            {
                viewmodel.UpdateEvaluationNotSSS(formData);
            }

            if (viewmodel.EvaluationSSS != null)
            {
                viewmodel.UpdateEvaluationSSS(formData);
            }
        }

        public static void UpdateNegotiation(this NegotiationViewModel viewmodel, ClientFieldData[] formData)
        {
            ClientFieldData field;

            field = formData.FirstOrDefault(x => x.Name == "IsCertifyProtest");
            viewmodel.IsCertifyProtest = field.ConvertToBool();

            field = formData.FirstOrDefault(x => x.Name == "ContractIsCertify");
            viewmodel.IsCertifyContract = field.ConvertToBool();

            field = formData.FirstOrDefault(x => x.Name == "HasProtest");
            viewmodel.HasProtest = field.ConvertToBool();

            field = formData.FirstOrDefault(x => x.Name == "hiddenJustification");
            if (field != null)
            {
                viewmodel.WinnerChanges = new WinnerChangeViewModel();
                viewmodel.WinnerChanges.Justification = field.ConvertToString();
            }

            viewmodel.FinalFirmScorings.UpdateDatatableFirmScoring(formData,
                TableValue.NEG_FINAL_FIRM_SCORING_TABLE_NAME);
            viewmodel.Comunications
                .BindDataFormDocsSimpleToModel(formData, TableValue.NEG_COMMUNICATIONS_TABLE_NAME);
            viewmodel.Negotiations.BindDocsFirm(formData,
                TableValue.NEG_NEGOTIATION_MINUTES_TABLE_NAME);
            viewmodel.CommitFunds.UpdateNegTCCommitFunds(formData,
                TableValue.NEG_COMMIT_FUNDS_TABLE_NAME);
            viewmodel.CommitFundsSecond.UpdateNegCommitFunds(formData,
                TableValue.NEG_COMMIT_FUNDS_SECOND_TABLE_NAME);
            viewmodel.ContractDates.UpdateContractDates(
                formData,
                TableValue.NEG_CONTRACT_DATES_TABLE_NAME,
                TableValue.NEG_FINAL_FIRM_SCORING_TABLE_NAME,
                viewmodel.Modality);
            viewmodel.ContractDocs.BindReoi(formData, TableValue.NEG_CONTRACT_DOCUMENTS_TABLE_NAME);
            viewmodel.ProtestDocs.Rows.BindProtestModel(formData, TableValue.NEG_PROTESTS_TABLE_NAME);
            viewmodel.Checklist.UpdateCheckList(formData, TableValue.NEG_CHECKLIST_TABLE_NAME);
        }

        public static void UpdateExecution(this FirmProcurementViewModel viewmodel, ClientFieldData[] formData)
        {
            if (viewmodel.Execution != null)
            {
                viewmodel.UpdateExecutionNoFRW(formData);
            }

            if (viewmodel.ExecutionFRW != null)
            {
                viewmodel.UpdateExecutionFRW(formData);
            }
        }

        public static void UpdateCancelReason(
            this CancelProcurementViewModel viewmodel,
            ClientFieldData[] formData)
        {
            if (viewmodel == null)
            {
                return;
            }

            var field = formData.FirstOrDefault(x => x.Name == "cancelReasonId");
            if (field != null)
            {
                viewmodel.ReasonId = field.ConvertToInt(
                    mode: ConvertModeEnum.ThrowExceptionInFail);
            }

            field = formData.FirstOrDefault(x => x.Name == "reason");
            if (field != null)
            {
                viewmodel.Reason = field.ConvertToString();
            }

            field = formData.FirstOrDefault(x => x.Name == "otherReason");
            if (field != null)
            {
                viewmodel.OtherReason = field.ConvertToString();
            }

            viewmodel.Documents.BindDataFormDocsSimpleToModel(formData, TableValue.CAN_REASON);
        }

        #endregion
        #endregion

        #region Private Methods
        #region Identification
        private static void BindReoi(
            this List<DocumentPublishRowViewModel> viewmodel,
            ClientFieldData[] formData,
            string table)
        {
            viewmodel.Clear();

            var itemsTable = GroupInputData(formData, table);

            for (int i = 0; i < itemsTable.Count; i++)
            {
                DateTime creationDate;
                DateTime.TryParse(itemsTable[i].FirstOrDefault(x => x.Name == "CreationDate")
                    .ConvertToString(), out creationDate);
                DateTime discloseDate;
                DateTime.TryParse(itemsTable[i].FirstOrDefault(x => x.Name == "DiscloseDate")
                    .ConvertToString(), out discloseDate);
                DateTime deadlineDate;
                DateTime.TryParse(itemsTable[i].FirstOrDefault(x => x.Name == "DeadlineDate")
                    .ConvertToString(), out deadlineDate);
                DocumentPublishRowViewModel model = new DocumentPublishRowViewModel
                {
                    Id = itemsTable[i].FirstOrDefault(x => x.Name == "DocumentId").ConvertToInt(),
                    User = itemsTable[i].FirstOrDefault(x => x.Name == "Author")
                    .ConvertToString(),
                    Description = itemsTable[i].FirstOrDefault(x => x.Name == "DocumentName")
                    .ConvertToString(),
                    DocumentNumber = itemsTable[i].FirstOrDefault(x => x.Name == "DocumentNumber")
                    .ConvertToString(),
                    Date = creationDate,
                    DeadlineDate = deadlineDate == default(DateTime)
                    ? (DateTime?)null : deadlineDate,
                    IsPublishIDB = itemsTable[i].FirstOrDefault(x => x.Name == "IsPublishIDB")
                    .ConvertToBool(),
                    IsPublishUNDB = itemsTable[i].FirstOrDefault(x => x.Name == "IsPublishUNDB")
                    .ConvertToBool(),
                    PublicationDate = discloseDate == default(DateTime)
                    ? (DateTime?)null : discloseDate,
                    IsForPublication = itemsTable[i].FirstOrDefault(x => x.Name == "IsPublished")
                    .ConvertToBool()
                };

                viewmodel.Add(model);
            }
        }

        private static void UpdateParticipatingFirms(this ParticipatingFirmViewModel viewmodel, ClientFieldData[] formData, string table)
        {
            viewmodel.Rows.Clear();

            var itemsTable = formData.Where(x => x.Name.Contains(table) && !string.IsNullOrWhiteSpace(x.Id));
            var itemsGrouped = itemsTable.GroupBy(x => x.Id);

            ClientFieldData field;

            field = formData.FirstOrDefault(x => x.Name == "ParticipatingFirmIsCertify");
            viewmodel.IsCertify = field.ConvertToBool();

            foreach (var item in itemsGrouped)
            {
                var model = new ParticipatingFirmRowViewModel();

                if (item.Key.Contains("new-"))
                {
                    model.OfferId = int.Parse(item.Key.Replace("new", string.Empty));
                }
                else
                {
                    model.OfferId = int.Parse(item.Key);
                }

                field = item.FirstOrDefault(x => x.Name == table + "-IncludeInShortList");
                model.IsShortlisted = field.ConvertToBool();

                field = item.FirstOrDefault(x => x.Name == table + "-FirmId");
                model.FirmId = field.ConvertToInt();

                field = item.FirstOrDefault(x => x.Name == table + "-FirmId_text");
                model.FirmName = field.ConvertToString();

                field = item.FirstOrDefault(x => x.Name == table + "-FirmNationality");
                model.NationalityId = field.ConvertToInt(
                    mode: ConvertModeEnum.ThrowExceptionInFail);

                field = item.FirstOrDefault(x => x.Name == table + "-Description");
                model.Description = field.ConvertToString();

                field = item.FirstOrDefault(x => x.Name == table + "-DocumentIDBDoc");
                model.DocumentNumber = field.ConvertToString();

                field = item.FirstOrDefault(x => x.Name == table + "-DocumentId");
                model.Id = field.ConvertToInt();

                viewmodel.Rows.Add(model);
            }
        }
        #endregion

        #region Preparation
        #endregion

        #region Evaluation
        private static void UpdateEvaluationNotSSS(this FirmProcurementViewModel viewmodel, ClientFieldData[] formData)
        {
            ClientFieldData field;

            field = formData.FirstOrDefault(x => x.Name == "TechnicalScoreWeight");
            viewmodel.Evaluation.TechnicalWeight = field.ConvertToNullableInt(
                mode: ConvertModeEnum.ThrowExceptionInFail);

            field = formData.FirstOrDefault(x => x.Name == "financialScoringWeight");
            viewmodel.Evaluation.FinancialWeight = field.ConvertToNullableInt(
                mode: ConvertModeEnum.ThrowExceptionInFail);

            field = formData.FirstOrDefault(x => x.Name == "MinimalTechnicalScoreWeight");
            viewmodel.Evaluation.MinimalTechnicalWeight = field.ConvertToNullableInt(
                mode: ConvertModeEnum.ThrowExceptionInFail);

            field = formData.FirstOrDefault(x => x.Name == "IsCertifyClarification");
            viewmodel.Evaluation.IsCertifyClarification = field.ConvertToBool();

            field = formData.FirstOrDefault(x => x.Name == "IsBAFOResquested");
            viewmodel.Evaluation.IsBAFOResquested = field.ConvertToBool();

            field = formData.FirstOrDefault(x => x.Name == "IsCertifyScoring");
            viewmodel.Evaluation.IsCertifyScoring = field.ConvertToBool();

            viewmodel.Evaluation.Clarifications.BindDocsFirm(formData,
                TableValue.EVA_CLARIFICATIONS_TABLE_NAME);
            viewmodel.Evaluation.TechnicalProposalDocs.BindDocsFirm(
                formData,
                TableValue.EVA_TECHNICAL_PROPOSAL_DOCUMENTS_TABLE_NAME);
            viewmodel.Evaluation.FinancialProposalDocs.BindDocsFirm(
                formData,
                TableValue.EVA_FINANCIAL_PROPOSAL_DOCUMENTS_TABLE_NAME);
            viewmodel.Evaluation.ScoringDocs.BindDataFormDocsSimpleToModel(
                formData,
                TableValue.EVA_SCORING_DOCUMENTS_TABLE_NAME);
            viewmodel.Evaluation.BAFODocs.Clear();
            if (viewmodel.Evaluation.IsBAFOResquested)
            {
                viewmodel.Evaluation.BAFODocs.BindDocsFirm(formData, TableValue.EVA_BAFO_TABLE_NAME);
            }

            viewmodel.Evaluation.FirmScoring.UpdateDatatableFirmScoring(formData,
                TableValue.EVA_FIRM_SCORING_TABLE_NAME);
            viewmodel.Evaluation.CommunicationsRelatedDocs
                .BindDataFormDocsSimpleToModel(formData,
                TableValue.EVA_COMMUNICATIONS_DOCS_TABLE_NAME);
            viewmodel.Evaluation.Checklist.UpdateCheckList(formData,
                TableValue.EVA_CHECKLIST_TABLE_NAME);
        }

        private static void UpdateEvaluationSSS(this FirmProcurementViewModel viewmodel, ClientFieldData[] formData)
        {
            ClientFieldData field;

            field = formData.FirstOrDefault(x => x.Name == "IsCertifyProposalSumary");
            viewmodel.EvaluationSSS.IsProposalSummary = field.ConvertToBool();

            viewmodel.EvaluationSSS.ProposalDocs.BindDocsFirm(
                formData,
                TableValue.EVA_PROPOSAL_DOCUMENTS_TABLE_NAME);
            viewmodel.EvaluationSSS.ProposalSummary.UpdateDatatableProposalSummary(
                formData,
                TableValue.EVA_PROPOSAL_SUMMARY_TABLE_NAME);
            viewmodel.EvaluationSSS.CommunicationsRelatedDocs.BindDataFormDocsSimpleToModel(
                formData,
                TableValue.EVA_COMMUNICATIONS_DOCS_TABLE_NAME);
        }

        private static void UpdateDatatableFirmScoring(this List<FirmScoringRowViewModel> viewmodel, ClientFieldData[] formData, string table)
        {
            viewmodel.Clear();

            var fieldNames = new string[]
            {
                table + "-Winner",
                table + "-TechnicalScoring",
                table + "-FinancialScoring",
                table + "-CurrencyScore",
                table + "-TotalScoring",
                table + "-Price",
                table + "-StatusId",
                table + "-Comment",
                "ScoringWinner",
            };

            var itemsTable = formData.Where(x => fieldNames.Contains(x.Name));
            var itemsList = itemsTable.Where(x => x.Id != null && x.Id != "new-").GroupBy(x => x.Id);

            ClientFieldData field;

            foreach (var item in itemsList)
            {
                var model = new FirmScoringRowViewModel();

                if (!item.Key.Contains("new-"))
                {
                    model.OfferId = int.Parse(item.Key);
                }

                field = item.FirstOrDefault(x => x.Name == table + "-Winner");
                if (field != null)
                {
                    model.IsWinner = field.ConvertToBool();
                }
                else
                {
                    field = item.FirstOrDefault(x => x.Name == "ScoringWinner");
                    model.IsWinner = field.ConvertToBool();
                }

                field = item.FirstOrDefault(x => x.Name == table + "-TechnicalScoring");
                model.TechnicalScoring = field.ConvertToNullableInt();

                field = item.FirstOrDefault(x => x.Name == table + "-FinancialScoring");
                model.FinancialScoring = field.ConvertToNullableInt();

                field = item.FirstOrDefault(x => x.Name == table + "-CurrencyScore");
                model.CurrencyId = field.ConvertToNullableInt();

                field = item.FirstOrDefault(x => x.Name == table + "-TotalScoring");
                model.TotalScoring = field.ConvertToNullableInt();

                field = item.FirstOrDefault(x => x.Name == table + "-Price");
                model.Price = field.ConvertToNullableDecimal();

                field = item.FirstOrDefault(x => x.Name == table + "-StatusId");
                model.StatusId = field.ConvertToNullableInt();

                field = item.FirstOrDefault(x => x.Name == table + "-Comment");
                model.Comment = field.ConvertToString();

                viewmodel.Add(model);
            }
        }

        private static void UpdateDatatableProposalSummary(this List<ProposalSummaryRowViewModel> viewmodel, ClientFieldData[] formData, string table)
        {
            viewmodel.Clear();

            var itemsTable = formData.Where(x => x.Name.Contains(table));
            var itemsList = itemsTable.Where(x => x.Id != null && x.Id != "new-").GroupBy(x => x.Id);

            ClientFieldData field;

            foreach (var item in itemsList)
            {
                var model = new ProposalSummaryRowViewModel();

                if (!item.Key.Contains("new-"))
                {
                    model.OfferId = int.Parse(item.Key);
                }

                field = item.FirstOrDefault(x => x.Name == table + "-Winner");
                if (field != null)
                {
                    model.IsWinner = field.ConvertToBool();
                }

                field = item.FirstOrDefault(x => x.Name == table + "-Currency");
                model.CurrencyId = field.ConvertToNullableInt();

                field = item.FirstOrDefault(x => x.Name == table + "-Price");
                model.Price = field.ConvertToNullableDecimal();

                field = item.FirstOrDefault(x => x.Name == table + "-StatusId");
                model.StatusId = field.ConvertToNullableInt();

                field = item.FirstOrDefault(x => x.Name == table + "-Comment");
                model.Comment = field.ConvertToString();

                viewmodel.Add(model);
            }
        }
        #endregion

        #region Negotiation

        private static void UpdateContractDates(this List<ContractDateRowViewModel> viewmodel, ClientFieldData[] formData, string table, string tableWinner, string modality)
        {
            var itemsTable = formData.Where(x => x.Name.Contains(tableWinner));
            var itemsList = itemsTable.Where(x => x.Id != null).GroupBy(x => x.Id);

            ClientFieldData field;

            List<int> isWinnerList = new List<int>();
            foreach (var item in itemsList)
            {
                field = item.FirstOrDefault(x => x.Name == tableWinner + "-Winner");
                if (field != null && field.ConvertToBool())
                {
                    isWinnerList.Add(int.Parse(item.Key));
                }
            }

            viewmodel.Clear();

            itemsTable = formData.Where(x => x.Name.Contains(table));
            itemsList = itemsTable.Where(x => x.Id != null).GroupBy(x => x.Id);

            foreach (var item in itemsList)
            {
                field = item.FirstOrDefault(x => x.Name == table + "-firmId");

                var firmId = field.ConvertToInt(mode: ConvertModeEnum.ThrowExceptionInFail);

                if (isWinnerList.Contains(firmId) || modality == Modality.SingleSourceSelection || modality == Modality.TaskSingleSourceSelection)
                {
                    var model = new ContractDateRowViewModel();

                    if (!item.Key.Contains("new-"))
                    {
                        model.ContractId = int.Parse(item.Key);
                    }

                    model.FirmId = firmId;

                    field = item.FirstOrDefault(x => x.Name == table + "-VendorID");
                    model.VendorID = field.ConvertToString();

                    field = item.FirstOrDefault(x => x.Name == table + "-startDate");
                    model.StartDate = field.ConvertToNullableDateTime();

                    field = item.FirstOrDefault(x => x.Name == table + "-endDate");
                    model.EndDate = field.ConvertToNullableDateTime();

                    field = item.FirstOrDefault(x => x.Name == table + "-contractSignDate");
                    model.ContractSignDate = field.ConvertToNullableDateTime();

                    field = item.FirstOrDefault(x => x.Name == table + "-Currency");
                    model.CurrencyId = field.ConvertToNullableInt();

                    field = item.FirstOrDefault(x => x.Name == table + "-Amount");
                    model.Amount = field.ConvertToNullableDecimal();

                    field = item.FirstOrDefault(x => x.Name == table + "-Description");
                    model.Description = field.ConvertToString();

                    field = item.FirstOrDefault(x => x.Name == table + "-DocumentIDBDoc");
                    model.DocumentNumber = field.ConvertToString();

                    field = item.FirstOrDefault(x => x.Name == table + "-DocumentId");
                    model.Id = field.ConvertToInt();

                    viewmodel.Add(model);
                }
            }
        }

        private static void BindProtestModel(
            this List<ProtestDocRowViewModel> viewmodel,
            ClientFieldData[] formData,
            string table)
        {
            viewmodel.Clear();

            var itemsTable = GroupInputData(formData, table);

            for (int i = 0; i < itemsTable.Count; i++)
            {
                DateTime creationDate;
                DateTime.TryParse(itemsTable[i].FirstOrDefault(x => x.Name == "CreationDate")
                    .ConvertToString(), out creationDate);
                ProtestDocRowViewModel model = new ProtestDocRowViewModel
                {
                    Id = itemsTable[i].FirstOrDefault(x => x.Name == "DocumentId").ConvertToInt(),
                    User = itemsTable[i].FirstOrDefault(x => x.Name == "Author")
                    .ConvertToString(),
                    Description = itemsTable[i].FirstOrDefault(x => x.Name == "DocumentName")
                    .ConvertToString(),
                    DocumentNumber = itemsTable[i].FirstOrDefault(x => x.Name == "DocumentNumber")
                    .ConvertToString(),
                    Date = creationDate,
                    OfferId = itemsTable[i].FirstOrDefault(x => x.Name == "FirmId").ConvertToInt(),
                    OrderNumber = itemsTable[i].FirstOrDefault(x => x.Name == "ProtestOrder")
                        .ConvertToInt(),
                    IsForPublication = itemsTable[i].FirstOrDefault(x => x.Name == "IsPublished")
                    .ConvertToBool()
                };

                viewmodel.Add(model);
            }
        }
        #endregion

        #region Execution
        private static void BindDatatableModifications(this List<ModificationDocRowViewModel> viewmodel, ClientFieldData[] formData, string table, int offerId)
        {
            viewmodel.Clear();
            List<IGrouping<string, ClientFieldData>> itemsTable = GroupInputData(
                formData, table + offerId.ToString());

            for (int i = 0; i < itemsTable.Count; i++)
            {
                ModificationDocRowViewModel model = new ModificationDocRowViewModel
                {
                    Id = itemsTable[i].FirstOrDefault(x => x.Name == "DocumentId").ConvertToInt(),
                    User = itemsTable[i].FirstOrDefault(x => x.Name == "Author").ConvertToString(),
                    Description = itemsTable[i].FirstOrDefault(x => x.Name == "DocumentName")
                        .ConvertToString(),
                    DocumentNumber = itemsTable[i].FirstOrDefault(x => x.Name == "DocumentNumber")
                        .ConvertToString(),
                    Date = itemsTable[i].FirstOrDefault(x => x.Name == "CreationDate")
                        .ConvertToDateTime(DateTime.MinValue),
                    OfferId = offerId,
                    ContractId = itemsTable[i].FirstOrDefault(x => x.Name == "ContractId")
                        .ConvertToInt(),
                    Modifications = itemsTable[i].FirstOrDefault(x => x.Name == "ModificationList")
                        .ConvertToIntList(),
                    NewEndDate = itemsTable[i].FirstOrDefault(x => x.Name == "EndDate")
                        .ConvertToNullableDateTime(),
                    NewTotalAmount = itemsTable[i].FirstOrDefault(
                        x => x.Name == "ContractTotalAmount").ConvertToNullableDecimal(),
                    IsForPublication = itemsTable[i].FirstOrDefault(x => x.Name == "IsPublished")
                        .ConvertToBool()
                };

                viewmodel.Add(model);
            }
        }

        private static void UpdatePerformanceEvaluation(this List<PerformanceEvaluationViewModel> viewmodel, ClientFieldData[] formData, string table)
        {
            viewmodel.Clear();

            var itemsTable = formData.Where(x => x.Name.Contains("answer-"));

            foreach (var item in itemsTable)
            {
                var model = new PerformanceEvaluationViewModel();

                model.Score = item.ConvertToInt(mode: ConvertModeEnum.ThrowExceptionInFail);
                model.Id = int.Parse(item.Id);

                viewmodel.Add(model);
            }
        }

        private static void UpdateExecutionFRW(this FirmProcurementViewModel viewmodel, ClientFieldData[] formData)
        {
            UpdateContractModifications(formData, viewmodel.ExecutionFRW.ContractsModifications.ToArray());

            var executionDocs = viewmodel.ExecutionFRW.ExecutionDocs.ToList();
            executionDocs.BindDataFormDocsSimpleToModel(formData, TableValue.EXE_DOCUMENTS);

            viewmodel.ExecutionFRW.ExecutionDocs = executionDocs;
        }

        private static void UpdateExecutionNoFRW(this FirmProcurementViewModel viewmodel, ClientFieldData[] formData)
        {
            ClientFieldData field;

            field = formData.FirstOrDefault(x => x.Name == "BriefDescription");
            viewmodel.Execution.FirmEvaluation.BriefDescription = field.ConvertToString();

            field = formData.FirstOrDefault(x => x.Name == "generalCommentText");
            viewmodel.Execution.FirmEvaluation.GeneralComments = field.ConvertToString();

            UpdateContractModifications(formData, viewmodel.Execution.ContractModifications);

            field = formData.FirstOrDefault(x => x.Name == "radioSanctionsNonPerformance");
            viewmodel.Execution.HasSanctions = field.ConvertToBool();

            viewmodel.Execution.SanctionComments = null;
            viewmodel.Execution.SanctionsStartDate = null;
            viewmodel.Execution.SanctionsEndDate = null;
            viewmodel.Execution.SanctionDocs.Clear();

            if (viewmodel.Execution.HasSanctions)
            {
                field = formData.FirstOrDefault(x => x.Name == "SanctionComment");
                viewmodel.Execution.SanctionComments = field.ConvertToString();

                field = formData.FirstOrDefault(x => x.Name == "inputSanctionsStartDate");
                viewmodel.Execution.SanctionsStartDate = field.ConvertToNullableDateTime();

                field = formData.FirstOrDefault(x => x.Name == "inputSanctionsEndDate");
                viewmodel.Execution.SanctionsEndDate = field.ConvertToNullableDateTime();

                viewmodel.Execution.SanctionDocs.BindDocsFirm(
                    formData,
                    TableValue.EXE_SANCTIONS_NON_PERFORMANCE_TABLE_NAME);
            }

            field = formData.FirstOrDefault(x => x.Name == "Recomended");
            if (field != null)
            {
                viewmodel.Execution.FirmEvaluation.IsRecomend = field.ConvertToNullableBool();
            }

            viewmodel.Execution.FirmEvaluation.Evaluation.UpdatePerformanceEvaluation(formData,
                TableValue.EXE_PERFORMANCE_EVALUATION_TABLE_NAME);

            var executionDocs = viewmodel.Execution.ExecutionDocs.ToList();
            executionDocs.BindDataFormDocsSimpleToModel(formData,
                TableValue.EXE_DOCUMENTS);

            viewmodel.Execution.ExecutionDocs = executionDocs;
        }

        private static void UpdateContractModifications(
            ClientFieldData[] formData,
             params ContractModificationsViewModels[] contractMods)
        {
            ClientFieldData field;

            foreach (var contractMod in contractMods)
            {
                contractMod.ModificationDocs.Clear();
                contractMod.Comunications.Clear();

                field = formData.FirstOrDefault(x => x.Name == string.Format("radioContractModifications-{0}", contractMod.OfferId));
                var hasModification = field.ConvertToBool();

                if (hasModification)
                {
                    field = formData.FirstOrDefault(x => (x.Name == "IsCertifyModifications") && (x.Id == contractMod.OfferId.ToString()));
                    contractMod.IsCertified = field.ConvertToBool();

                    contractMod.ModificationDocs.BindDatatableModifications(
                        formData,
                        TableValue.EXE_CONTRACT_MODIFICATION_TABLE_NAME,
                        contractMod.OfferId);
                    contractMod.Comunications.BindDocsFirm(
                        formData,
                        TableValue.EXE_COMMUNICATIONS_RELATED_DOCS_TABLE_NAME + contractMod.OfferId,
                        contractMod.OfferId);
                }
                else
                {
                    contractMod.IsCertified = false;

                    contractMod.ModificationDocs.Clear();
                    contractMod.Comunications.Clear();
                }
            }
        }
        #endregion

        #region Common

        #region Updates Old
        private static void UpdateTCCommitFunds(this List<CommitFundRowViewModel> viewmodel, ClientFieldData[] formData, string table)
        {
            var originalOutputs = viewmodel.ToDictionary(x => x.Id, x => x.ActiveOutputs);
            viewmodel.Clear();

            var itemsTable = formData.Where(x => x.Name.Contains(table));
            var itemsList = itemsTable.Where(x => x.Id != null && x.Id != "new-").GroupBy(x => x.Id);

            ClientFieldData field;

            foreach (var item in itemsList)
            {
                var model = new CommitFundRowViewModel();

                if (!item.Key.Contains("new-"))
                {
                    model.Id = int.Parse(item.Key);
                }

                field = item.FirstOrDefault(x => x.Name == table + "-ApprovalNumber_text");
                model.ApprovalNumber = field.ConvertToString();

                field = item.FirstOrDefault(x => x.Name == table + "hiddenOperationNumber");
                model.OperatinNumber = field.ConvertToString();

                field = item.FirstOrDefault(x => x.Name == table + "ApprovalCurrency");
                model.ApprovalCurrency = field.ConvertToString();

                field = item.FirstOrDefault(x => x.Name == table + "-hiddenTotalFunds");
                model.TotalFundsCurrentlyAvailable = field.ConvertToDecimal(
                    mode: ConvertModeEnum.ThrowExceptionInFail);

                field = item.FirstOrDefault(x => x.Name == table + "-originalFunds");
                model.OriginalProcurementEstimate = field.ConvertToDecimal(
                    mode: ConvertModeEnum.ThrowExceptionInFail);

                field = item.FirstOrDefault(x => x.Name == table + "-activeOutputs");
                var outputIds = field.ConvertToIntList();
                if (originalOutputs.ContainsKey(model.Id))
                {
                    model.ActiveOutputs = originalOutputs[model.Id];
                    model.ActiveOutputs.ForEach(x => x.Select = outputIds.Contains(int.Parse(x.Value)));
                }
                else
                {
                    outputIds.ForEach(x => model.ActiveOutputs.Add(new ListItemViewModel()
                    {
                        Value = x.ToString(),
                        Select = true
                    }));
                }

                viewmodel.Add(model);
            }
        }

        private static void UpdateCommitFunds(this List<CommitFundRowViewModel> viewmodel, ClientFieldData[] formData, string table)
        {
            viewmodel.Clear();

            var itemsTable = formData.Where(x => x.Name.Contains(table));
            var itemsList = itemsTable.Where(x => x.Id != null && x.Id != "new-").GroupBy(x => x.Id);

            ClientFieldData field;

            foreach (var item in itemsList)
            {
                var model = new CommitFundRowViewModel();

                if (!item.Key.Contains("new-"))
                {
                    model.Id = int.Parse(item.Key);
                }

                field = item.FirstOrDefault(x => x.Name == table + "-ApprovalNumber_text");
                model.ApprovalNumber = field.ConvertToString();

                field = item.FirstOrDefault(x => x.Name == table + "hiddenOperationNumber");
                model.OperatinNumber = field.ConvertToString();

                field = item.FirstOrDefault(x => x.Name == table + "ApprovalCurrency");
                model.ApprovalCurrency = field.ConvertToString();

                field = item.FirstOrDefault(x => x.Name == table + "-hiddenTotalFunds");
                model.TotalFundsCurrentlyAvailable = field.ConvertToDecimal(
                    mode: ConvertModeEnum.ThrowExceptionInFail);

                field = item.FirstOrDefault(x => x.Name == table + "-originalFunds");
                model.OriginalProcurementEstimate = field.ConvertToDecimal(
                    mode: ConvertModeEnum.ThrowExceptionInFail);

                viewmodel.Add(model);
            }
        }

        private static void UpdateNegTCCommitFunds(this List<CommitFundRowViewModel> viewmodel, ClientFieldData[] formData, string table)
        {
            var originalOutputs = viewmodel.ToDictionary(x => x.Id, x => Tuple.Create(x.IsIdentification, x.ActiveOutputs));
            viewmodel.Clear();

            var itemsTable = formData.Where(x => x.Name.Contains(table));
            var itemsList = itemsTable.Where(x => x.Id != null && x.Id != "new-").GroupBy(x => x.Id);

            ClientFieldData field;

            foreach (var item in itemsList)
            {
                var model = new CommitFundRowViewModel();

                if (!item.Key.Contains("new-"))
                {
                    model.Id = int.Parse(item.Key);
                }

                field = item.FirstOrDefault(x => x.Name == table + "-ApprovalNumber");
                if (field == null)
                {
                    field = item.FirstOrDefault(x => x.Name == table + "-ProductActivityNumber");
                }

                model.ApprovalNumber = field.ConvertToString();

                field = item.FirstOrDefault(x => x.Name == table + "-IsIdentificationHidden");
                model.IsIdentification = field.ConvertToBool();

                field = item.FirstOrDefault(x => x.Name == table + "-fundsAvailableAtStartHidden");
                model.OriginalProcurementEstimate = field.ConvertToNullableDecimal();

                field = item.FirstOrDefault(x => x.Name == table + "-HiddenOperationNumber");
                model.OperatinNumber = field.ConvertToString();

                field = item.FirstOrDefault(x => x.Name == table + "-ApprovalCurrency");
                model.ApprovalCurrency = field.ConvertToString();

                field = item.FirstOrDefault(x => x.Name == table + "-LMSNumber");
                model.LMSNumber = field.ConvertToString();

                field = item.FirstOrDefault(x => x.Name == table + "-CMOCurrency");
                model.CMOCurrency = field.ConvertToString();

                field = item.FirstOrDefault(x => x.Name == table + "-CMOCurrentAmountApproval");
                model.CMOCurrentAmountApproval = field.ConvertToNullableDecimal();

                field = item.FirstOrDefault(x => x.Name == table + "-CMOCurrentAmount");
                model.CMOCurrentAmount = field.ConvertToNullableDecimal();

                field = item.FirstOrDefault(x => x.Name == table + "-activeOutputs");
                var outputIds = field.ConvertToIntList();
                if (originalOutputs.ContainsKey(model.Id))
                {
                    model.IsIdentification = originalOutputs[model.Id].Item1;
                    model.ActiveOutputs = originalOutputs[model.Id].Item2;
                    if (!model.IsIdentification)
                    {
                        model.ActiveOutputs.ForEach(x => x.Select = outputIds.Contains(int.Parse(x.Value)));
                    }
                }
                else
                {
                    outputIds.ForEach(x => model.ActiveOutputs.Add(new ListItemViewModel()
                    {
                        Value = x.ToString(),
                        Select = true
                    }));
                }

                viewmodel.Add(model);
            }
        }

        private static void UpdateNegCommitFunds(this List<CommitFundRowViewModel> viewmodel, ClientFieldData[] formData, string table)
        {
            viewmodel.Clear();

            var itemsTable = formData.Where(x => x.Name.Contains(table));
            var itemsList = itemsTable.Where(x => x.Id != null && x.Id != "new-").GroupBy(x => x.Id);

            ClientFieldData field;

            foreach (var item in itemsList)
            {
                var model = new CommitFundRowViewModel();

                if (!item.Key.Contains("new-"))
                {
                    model.Id = int.Parse(item.Key);
                }

                field = item.FirstOrDefault(x => x.Name == table + "-ApprovalNumber");
                if (field == null)
                {
                    field = item.FirstOrDefault(x => x.Name == table + "-ProductActivityNumber");
                }

                model.ApprovalNumber = field.ConvertToString();

                field = item.FirstOrDefault(x => x.Name == table + "-IsIdentificationHidden");
                model.IsIdentification = field.ConvertToBool();

                field = item.FirstOrDefault(x => x.Name == table + "-fundsAvailableAtStartHidden");
                model.OriginalProcurementEstimate = field.ConvertToNullableDecimal();

                if (table == "NegCommitFundsNormal")
                {
                    field = item.FirstOrDefault(x => x.Name == table + "-HiddenOperationNumber");
                    model.OperatinNumber = field.ConvertToString();

                    field = item.FirstOrDefault(x => x.Name == table + "-ApprovalCurrency");
                    model.ApprovalCurrency = field.ConvertToString();

                    field = item.FirstOrDefault(x => x.Name == table + "-LMSNumber");
                    model.LMSNumber = field.ConvertToString();

                    field = item.FirstOrDefault(x => x.Name == table + "-CMOCurrency");
                    model.CMOCurrency = field.ConvertToString();

                    field = item.FirstOrDefault(x => x.Name == table + "-CMOCurrentAmountApproval");
                    model.CMOCurrentAmountApproval = field.ConvertToNullableDecimal();

                    field = item.FirstOrDefault(x => x.Name == table + "-CMOCurrentAmount");
                    model.CMOCurrentAmount = field.ConvertToNullableDecimal();
                }
                else
                {
                    field = item.FirstOrDefault(x => x.Name == table + "-PONumber");
                    model.PONumber = field.ConvertToString();

                    field = item.FirstOrDefault(x => x.Name == table + "-POAmount");
                    model.FinalProcurementAmount = field.ConvertToNullableDecimal();
                }

                viewmodel.Add(model);
            }
        }

        private static void BindDataFormDocsSimpleToModel(
            this IList<DocumentRowViewModel> documentModel,
            ClientFieldData[] formData,
            string table)
        {
            var itemsTable = GroupInputData(formData, table);

            for (int i = 0; i < itemsTable.Count; i++)
            {
                DateTime date;
                DateTime.TryParse(itemsTable[i].FirstOrDefault(x => x.Name == "CreationDate")
                    .ConvertToString(), out date);
                DocumentRowViewModel model = new DocumentRowViewModel
                {
                    Id = itemsTable[i].FirstOrDefault(x => x.Name == "DocumentId")
                        .ConvertToInt(mode: ConvertModeEnum.ThrowExceptionInFail),
                    User = itemsTable[i].FirstOrDefault(x => x.Name == "Author")?.Value,
                    Description = itemsTable[i].FirstOrDefault(x => x.Name == "DocumentName")
                    ?.Value,
                    DocumentNumber = itemsTable[i].FirstOrDefault(x => x.Name == "DocumentNumber")
                    ?.Value,
                    Date = date,
                    IsForPublication = itemsTable[i].FirstOrDefault(x => x.Name == "IsPublished")
                    .ConvertToBool()
                };

                documentModel.Add(model);
            }
        }

        private static void BindDocsFirm(
            this List<DocumentFirmRowViewModel> viewmodel,
            ClientFieldData[] formData,
            string table,
            int? offerId = null)
        {
            var itemsTable = GroupInputData(formData, table);

            for (int i = 0; i < itemsTable.Count; i++)
            {
                var id = itemsTable[i].FirstOrDefault(x => x.Name == "FirmId").ConvertToInt();
                DocumentFirmRowViewModel model = new DocumentFirmRowViewModel();
                if (!offerId.HasValue || (offerId.Value == id))
                {
                    DateTime date;
                    DateTime.TryParse(itemsTable[i].FirstOrDefault(x => x.Name == "CreationDate")
                        .ConvertToString(), out date);
                    model.OfferId = id;
                    model.Id = itemsTable[i].FirstOrDefault(x => x.Name == "DocumentId")
                        .ConvertToInt(mode: ConvertModeEnum.ThrowExceptionInFail);
                    model.User = itemsTable[i].FirstOrDefault(x => x.Name == "Author")?.Value;
                    model.Description = itemsTable[i].FirstOrDefault(x => x.Name == "DocumentName")
                        ?.Value;
                    model.DocumentNumber = itemsTable[i]
                        .FirstOrDefault(x => x.Name == "DocumentNumber")
                        ?.Value;
                    model.Date = date;
                    model.IsForPublication = itemsTable[i]
                        .FirstOrDefault(x => x.Name == "IsPublished").ConvertToBool();
                }

                viewmodel.Add(model);
            }
        }

        private static void UpdateCheckList(this List<ChecklistRowViewModel> viewmodel, ClientFieldData[] formData, string table)
        {
            var itemsTable = formData.Where(x => x.Name.Contains("isCompleted") ||
                                                 x.Name.Contains("dateCompleted"));
            var itemsList = itemsTable.Where(x => x.Id != null).GroupBy(x => x.Id);

            var newCheckListValues = new List<ChecklistRowViewModel>();
            ClientFieldData field;
            foreach (var item in itemsList)
            {
                var model = new ChecklistRowViewModel();
                model.ChecklistStageModalityItemId = int.Parse(item.Key);

                field = item.FirstOrDefault(x => x.Name == "dateCompleted");
                model.DateCompleted = field.ConvertToNullableDateTime();

                field = item.FirstOrDefault(x => x.Name == "isCompleted");
                model.Completed = field.ConvertToBool();

                newCheckListValues.Add(model);
            }

            foreach (var newChecklistItem in newCheckListValues)
            {
                var oldChecklistItem = viewmodel.First(x => x.ChecklistStageModalityItemId == newChecklistItem.ChecklistStageModalityItemId);
                if ((newChecklistItem.Completed != oldChecklistItem.Completed) || (newChecklistItem.DateCompleted != oldChecklistItem.DateCompleted))
                {
                    oldChecklistItem.Completed = newChecklistItem.Completed;
                    oldChecklistItem.DateCompleted = newChecklistItem.DateCompleted;
                }
                else
                {
                    viewmodel.Remove(oldChecklistItem);
                }
            }
        }
        #endregion
        #endregion
        #endregion

        private static List<IGrouping<string, ClientFieldData>> GroupInputData(
            ClientFieldData[] formData,
            string tableName)
        {
            var groups = formData.Where(x => x.Name.Contains(tableName)).Select(x =>
               {
                   var name = x.Name.Split('.').LastOrDefault();
                   var id = Regex.Match(x.Name, @"(?<=\[).+?(?=\])").Value;
                   return new ClientFieldData
                   {
                       Id = id,
                       Name = name,
                       Value = x.Value
                   };
               }).GroupBy(g => g.Id).ToList();

            return groups;
        }
    }
}