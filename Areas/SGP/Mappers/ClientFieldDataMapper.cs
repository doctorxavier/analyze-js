using System.Collections.Generic;
using System.Linq;
using System;

using IDB.MW.Application.SGPModule.ViewModels;
using IDB.MW.Application.SGPModule.ViewModels.ProcurementDetail;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVC4.Models.ClientFieldData;

namespace IDB.Presentation.MVC4.Areas.SGP.Mappers
{
    public static class ClientFieldDataMapper
    {
        #region Constants
        #endregion

        #region Mappers

        #region Procurement Plan

        public static ProcurementPlanViewModel Update(this ProcurementPlanViewModel model, ClientFieldData[] formData)
        {
            if (model.Parametrization != null)
            {
                model.Parametrization.UpdateParameterization(formData);
            }

            if (model.ProcurementLevel != null)
            {
                model.ProcurementLevel.UpdateProcurementLevel(formData);
            }

            if (model.OperationLevel != null)
            {
                model.OperationLevel.UpdateOperationLevel(formData);
            }

            return model;
        }

        #endregion

        #region Procurement Details

        public static ProcurementDetailViewModel UpdateProcurementDetailsViewModel(this ProcurementDetailViewModel model, ClientFieldData[] formData)
        {
            ClientFieldData field;

            field = formData.FirstOrDefault(x => x.Name == "manualId");
            model.DetailTab.ManualId = field.ConvertToString();

            field = formData.FirstOrDefault(x => x.Name == "country");
            model.DetailTab.Country = field.ConvertToString();

            field = formData.FirstOrDefault(x => x.Name == "state");
            model.DetailTab.State = field.ConvertToString();

            field = formData.FirstOrDefault(x => x.Name == "district");
            model.DetailTab.District = field.ConvertToString();

            model.DetailTab.Documents.Clear();

            var inputsOtherDocument = new string[] 
            {
                "documentTableDetails-Description",
                "documentTableDetails-DocumentIDBDoc",
            };

            var fieldsOtherDocument = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputsOtherDocument.Any(y => x.Name == y));
            var fieldsOtherDocumentGrouped = fieldsOtherDocument.GroupBy(x => x.Id);

            foreach (var otherDocumentItem in fieldsOtherDocumentGrouped)
            {
                var itemId = otherDocumentItem.Key.Contains("new") ? 0 : otherDocumentItem.Key.ConvertToInt(0, ConvertModeEnum.ThrowExceptionInFail);

                var documentModel = new SimpleDocRowViewModel() { Id = itemId };
                model.DetailTab.Documents.Add(documentModel);

                field = otherDocumentItem.FirstOrDefault(x => x.Name == "documentTableDetails-Description");
                documentModel.Description = field.ConvertToString();

                field = otherDocumentItem.FirstOrDefault(x => x.Name == "documentTableDetails-DocumentIDBDoc");
                documentModel.DocumentNumber = field.ConvertToString();
            }

            return model;
        }

        #endregion

        #region Checklist

        public static ProcurementDetailViewModel UpdateChecklist(this ProcurementDetailViewModel model, ClientFieldData[] formData)
        {
            ClientFieldData field;
            var inputsChecklist = new string[] 
            {
                "isComplete-checklist",
                "baselineDate-checklist",
                "updatePlanningDate-checklist",
                "completionDate-checklist",
            };

            var fieldsChecklist = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputsChecklist.Any(y => x.Name == y));
            var fieldsChecklistGrouped = fieldsChecklist.GroupBy(x => x.Id);

            model.ChecklistTab.Items.Clear();

            foreach (var checklistItem in fieldsChecklistGrouped)
            {
                var modelItem = new ChecklistRowViewModel() { Id = checklistItem.Key.ConvertToInt(0) };

                field = checklistItem.FirstOrDefault(x => x.Name == inputsChecklist[0]);
                modelItem.MilestoneIsComplete = field.ConvertToBool();

                field = checklistItem.FirstOrDefault(x => x.Name == inputsChecklist[1]);
                modelItem.BaselineDate = field.ConvertToNullableDateTime();

                field = checklistItem.FirstOrDefault(x => x.Name == inputsChecklist[2]);
                modelItem.UpdatedPlanningDate = field.ConvertToNullableDateTime();

                field = checklistItem.FirstOrDefault(x => x.Name == inputsChecklist[3]);
                modelItem.CompletionDate = field.ConvertToNullableDateTime();

                model.ChecklistTab.Items.Add(modelItem);
            }

            return model;
        }

        #endregion

        #region Bidding Documents

        public static ProcurementDetailViewModel UpdateBiddingDocuments(this ProcurementDetailViewModel model, ClientFieldData[] formData)
        {
            ClientFieldData field;
            var inputsBiddingPackage = new string[] { "actualDate-bidding" };

            var fieldsBiddingPackage = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputsBiddingPackage.Any(y => x.Name == y));
            var fieldsBiddingPackageGrouped = fieldsBiddingPackage.GroupBy(x => x.Id);

            model.BiddingDocTab.BidPackages.Clear();
            foreach (var biddingPackageItem in fieldsBiddingPackageGrouped)
            {
                var modelBidPackage = new BidPackagesRowViewModel() { BidPackageConfProcId = biddingPackageItem.Key.ConvertToInt(0) };
                model.BiddingDocTab.BidPackages.Add(modelBidPackage);

                field = biddingPackageItem.FirstOrDefault(x => x.Name == inputsBiddingPackage[0]);
                modelBidPackage.ActualDate = field.ConvertToNullableDateTime();
            }

            var seralizedPackageDetails = formData.FirstOrDefault(x => x.Name == "serializedPackageDetail");
            if (seralizedPackageDetails != null)
            {
                var packageDetailsModel = PageSerializationHelper.DeserializeObject<PackageDetailViewModel>(seralizedPackageDetails.ConvertToString());
                model.BiddingDocTab.PackageSelected = packageDetailsModel;

                var inputsBiddingDetailsRow = new string[]
                { 
                    "documentType-bidding",
                    "DocumentIDBDoc-bidding",
                    "resultType-bidding",
                    "description-bidding",
                    "nonObjectionStatus-bidding",
                    "isMandatory-bidding",
                    "isAmendment-bidding"
                };

                var fiedsBiddingDoc = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputsBiddingDetailsRow.Any(y => x.Name == y));
                var fiedsBiddingDocGrouped = fiedsBiddingDoc.GroupBy(x => x.Id);

                model.BiddingDocTab.PackageSelected.BiddingDocRow.Clear();
                foreach (var biddingDocItem in fiedsBiddingDocGrouped)
                {
                    var docModel = new BiddingDocRowViewModel() { DocumentId = biddingDocItem.Key.ConvertToInt(0) };
                    model.BiddingDocTab.PackageSelected.BiddingDocRow.Add(docModel);

                    field = biddingDocItem.FirstOrDefault(x => x.Name == inputsBiddingDetailsRow[0]);
                    var valueType = field.ConvertToString().Split('-');
                    docModel.DocumentTypeId = valueType[1];

                    field = biddingDocItem.FirstOrDefault(x => x.Name == inputsBiddingDetailsRow[1]);
                    docModel.DocumentNumber = field.ConvertToString();

                    field = biddingDocItem.FirstOrDefault(x => x.Name == inputsBiddingDetailsRow[2]);
                    docModel.ResultTypeId = field.ConvertToString();

                    field = biddingDocItem.FirstOrDefault(x => x.Name == inputsBiddingDetailsRow[3]);
                    docModel.Description = field.ConvertToString();

                    field = biddingDocItem.FirstOrDefault(x => x.Name == inputsBiddingDetailsRow[4]);
                    docModel.NonObjectionStatusTypeId = field.ConvertToInt();

                    field = biddingDocItem.FirstOrDefault(x => x.Name == inputsBiddingDetailsRow[5]);
                    docModel.IsMandatory = field.ConvertToBool();

                    field = biddingDocItem.FirstOrDefault(x => x.Name == inputsBiddingDetailsRow[6]);
                    docModel.IsAmendmet = field.ConvertToBool();
                }
            }

            return model;
        }

        #endregion

        #region Participant

        public static ProcurementDetailViewModel UpdateParticipant(this ProcurementDetailViewModel model, ClientFieldData[] formData)
        {
            ClientFieldData field;
            var inputsBidder = new string[] 
            {
                "name-bidder_text",
                "name-bidder",
                "consortium-bidder",
                "nationality-bidder",
                "technicalScore-bidder",
                "financialScore-bidder",
                "awardedAmount-bidder",
                "results-bidder",
                "gender-bidder",
                "economicSector-bidder",
                "address-bidder",
                "country-bidder",
                "state-bidder",
                "district-bidder",
                "name-participant_text",
                "name-participant",
                "nationality-participant",
                "gender-participant",
                "economicSector-participant",
                "address-participant",
                "country-participant",
                "state-participant",
                "district-participant"
            };

            model.ParticipantTab.Bidders.Clear();

            var fieldsBidders = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputsBidder.Any(y => x.Name == y));
            var fieldsBiddersGrouped = fieldsBidders.GroupBy(x => x.Id);

            foreach (var filedsByBidder in fieldsBiddersGrouped)
            {
                var bidderId = filedsByBidder.Key.Contains("new") ? 0 : filedsByBidder.Key.ConvertToInt(0, ConvertModeEnum.ThrowExceptionInFail);
                var bidderRow = new BidderRowViewModel() { BidderId = bidderId, Details = new BidderDetailsViewModel() };

                field = filedsByBidder.FirstOrDefault(x => x.Name == "name-bidder");
                bidderRow.NameBidderId = field.ConvertToNullableInt();

                field = filedsByBidder.FirstOrDefault(x => x.Name == "name-bidder_text");
                bidderRow.NameBidderText = field.ConvertToString();

                field = filedsByBidder.FirstOrDefault(x => x.Name == "consortium-bidder");
                bidderRow.Consortium = field.ConvertToBool();

                field = filedsByBidder.FirstOrDefault(x => x.Name == "nationality-bidder");
                bidderRow.NationalityTypeId = field.ConvertToNullableInt();

                field = filedsByBidder.FirstOrDefault(x => x.Name == "technicalScore-bidder");
                bidderRow.TechnicalScore = field.ConvertToNullableDecimal();

                field = filedsByBidder.FirstOrDefault(x => x.Name == "financialScore-bidder");
                bidderRow.FinancialScore = field.ConvertToNullableDecimal();

                field = filedsByBidder.FirstOrDefault(x => x.Name == "awardedAmount-bidder");
                bidderRow.AwardedAmount = field.ConvertToNullableDecimal();

                field = filedsByBidder.FirstOrDefault(x => x.Name == "results-bidder");
                bidderRow.ResultsTypeId = field.ConvertToInt();

                field = filedsByBidder.FirstOrDefault(x => x.Name == "gender-bidder");
                bidderRow.Details.GenderTypeId = field.ConvertToNullableInt();

                field = filedsByBidder.FirstOrDefault(x => x.Name == "economicSector-bidder");
                bidderRow.Details.EconomicSectorId = field.ConvertToNullableInt();

                field = filedsByBidder.FirstOrDefault(x => x.Name == "address-bidder");
                bidderRow.Details.Address = field.ConvertToString();

                field = filedsByBidder.FirstOrDefault(x => x.Name == "country-bidder");
                bidderRow.Details.Country = field.ConvertToString();

                field = filedsByBidder.FirstOrDefault(x => x.Name == "state-bidder");
                bidderRow.Details.State = field.ConvertToString();

                field = filedsByBidder.FirstOrDefault(x => x.Name == "district-bidder");
                bidderRow.Details.District = field.ConvertToString();

                var fieldsGroupedForParticipant = filedsByBidder
                                                  .Where(x => x.ExtraData.ContainsKey("data-persist-participantid"))
                                                  .GroupBy(x => x.ExtraData.Single(y => y.Key == "data-persist-participantid"));

                foreach (var filedsByParticipant in fieldsGroupedForParticipant)
                {
                    var participantId = filedsByParticipant.Key.Value.Contains("new") ? 0 : filedsByParticipant.Key.Value.ConvertToInt(0, ConvertModeEnum.ThrowExceptionInFail);

                    var participantRow = new ParticipantRowViewModel() { BidderId = bidderId, ParticipantId = participantId, Details = new BidderDetailsViewModel() };

                    field = filedsByParticipant.FirstOrDefault(x => x.Name == "name-participant");
                    participantRow.NameParticipantId = field.ConvertToNullableInt();

                    field = filedsByParticipant.FirstOrDefault(x => x.Name == "name-participant_text");
                    participantRow.NameParticipantText = field.ConvertToString();

                    field = filedsByParticipant.FirstOrDefault(x => x.Name == "nationality-participant");
                    participantRow.NationalityTypeId = field.ConvertToNullableInt();

                    field = filedsByParticipant.FirstOrDefault(x => x.Name == "gender-participant");
                    participantRow.Details.GenderTypeId = field.ConvertToNullableInt();

                    field = filedsByParticipant.FirstOrDefault(x => x.Name == "economicSector-participant");
                    participantRow.Details.EconomicSectorId = field.ConvertToNullableInt();

                    field = filedsByParticipant.FirstOrDefault(x => x.Name == "address-participant");
                    participantRow.Details.Address = field.ConvertToString();

                    field = filedsByParticipant.FirstOrDefault(x => x.Name == "country-participant");
                    participantRow.Details.Country = field.ConvertToString();

                    field = filedsByParticipant.FirstOrDefault(x => x.Name == "state-participant");
                    participantRow.Details.State = field.ConvertToString();

                    field = filedsByParticipant.FirstOrDefault(x => x.Name == "district-participant");
                    participantRow.Details.District = field.ConvertToString();

                    bidderRow.Participants.Add(participantRow);
                }

                model.ParticipantTab.Bidders.Add(bidderRow);
            }

            return model;
        }

        #endregion

        #region Contracts

        public static ProcurementDetailViewModel UpdateContract(this ProcurementDetailViewModel model, ClientFieldData[] formData)
        {
            ClientFieldData field;

            var inputsContractRow = new string[] 
            {
                "contractNumber-Contracts",
                "bidder-Contracts",
                "status-Contracts"
            };

            model.ContractsTab.Contracts.Clear();

            var fieldsContractsTable = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputsContractRow.Any(y => x.Name == y));
            var fieldsContractsTableGrouped = fieldsContractsTable.GroupBy(x => x.Id);

            foreach (var contractTableItem in fieldsContractsTableGrouped)
            {
                var contractRow = new ContractRowViewModel() { Id = contractTableItem.Key.ConvertToInt(0) };
                model.ContractsTab.Contracts.Add(contractRow);

                field = contractTableItem.FirstOrDefault(x => x.Name == inputsContractRow[0]);
                contractRow.ContractNumber = field.ConvertToString();

                field = contractTableItem.FirstOrDefault(x => x.Name == inputsContractRow[1]);
                contractRow.BidderId = field.ConvertToString();

                field = contractTableItem.FirstOrDefault(x => x.Name == inputsContractRow[2]);
                contractRow.StatusId = field.ConvertToInt();
            }

            var seralizedContradDetail = formData.FirstOrDefault(x => x.Name == "serializedContractDetail");
            model.ContractsTab.ContractDetails = new ContractDetailsViewModel() { DetailsOriginal = new ContractDetailsDataViewModel() };

            if (seralizedContradDetail != null)
            {
                var contractDetailsModel = PageSerializationHelper.DeserializeObject<ContractDetailsViewModel>(seralizedContradDetail.ConvertToString());
                model.ContractsTab.ContractDetails = contractDetailsModel;

                var modelDetails = model.ContractsTab.ContractDetails;

                var inputsContractDetails = new string[] 
                {
                    "dateSignature-contracts",
                    "dateStart-contracts",
                    "dateEnd-contracts",
                    "country",
                    "state",
                    "district",
                };

                var fieldsContractsDetails = formData.Where(x => inputsContractDetails.Any(y => x.Name == y));

                field = fieldsContractsDetails.FirstOrDefault(x => x.Name == inputsContractDetails[0]);
                modelDetails.DetailsOriginal.SignatureDate = new Tuple<DateTime?, string>(field.ConvertToNullableDateTime(), string.Empty);

                field = fieldsContractsDetails.FirstOrDefault(x => x.Name == inputsContractDetails[1]);
                modelDetails.DetailsOriginal.StartDate = new Tuple<DateTime?, string>(field.ConvertToNullableDateTime(), string.Empty);

                field = fieldsContractsDetails.FirstOrDefault(x => x.Name == inputsContractDetails[2]);
                modelDetails.DetailsOriginal.EndDate = new Tuple<DateTime?, string>(field.ConvertToNullableDateTime(), string.Empty);

                field = fieldsContractsDetails.FirstOrDefault(x => x.Name == inputsContractDetails[3]);
                modelDetails.Country = field.ConvertToString();

                field = fieldsContractsDetails.FirstOrDefault(x => x.Name == inputsContractDetails[4]);
                modelDetails.State = field.ConvertToString();

                field = fieldsContractsDetails.FirstOrDefault(x => x.Name == inputsContractDetails[5]);
                modelDetails.District = field.ConvertToString();

                var inputsContractAmount = new string[] 
                {
                    "currencyType-Amount",
                    "total-Amount",
                };

                var fieldsContractsAmount = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputsContractAmount.Any(y => x.Name == y));
                var fieldsContractsAmountGrouped = fieldsContractsAmount.GroupBy(x => x.Id);

                modelDetails.DetailsOriginal.CurrencyAmounts.Clear();
                foreach (var contractAmountItem in fieldsContractsAmountGrouped)
                {
                    var amountRow = new ContractDataAmountViewModel() { Id = contractAmountItem.Key.ConvertToInt(0) };
                    modelDetails.DetailsOriginal.CurrencyAmounts.Add(amountRow);

                    field = contractAmountItem.FirstOrDefault(x => x.Name == inputsContractAmount[0]);
                    amountRow.CurrencyAmountTypeId = field.ConvertToInt();

                    field = contractAmountItem.FirstOrDefault(x => x.Name == inputsContractAmount[1]);
                    amountRow.TotalContractAmount = new Tuple<decimal?, string>(field.ConvertToNullableDecimal(), string.Empty);
                }

                var inputsContractLot = new string[] 
                {
                    "name-Lots",
                    "quantity-Lots",
                    "amount-Lots",
                };

                var fieldsContractsLots = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputsContractLot.Any(y => x.Name == y));
                var fieldsContractsLotsGrouped = fieldsContractsLots.GroupBy(x => x.Id);

                modelDetails.DetailsOriginal.Lots.Clear();
                foreach (var contractLotItem in fieldsContractsLotsGrouped)
                {
                    var amountLot = new ContractDataLotViewModel() { Id = contractLotItem.Key.ConvertToInt(0) };
                    modelDetails.DetailsOriginal.Lots.Add(amountLot);

                    field = contractLotItem.FirstOrDefault(x => x.Name == inputsContractLot[0]);
                    amountLot.Name = new Tuple<string, string>(field.ConvertToString(), string.Empty);

                    field = contractLotItem.FirstOrDefault(x => x.Name == inputsContractLot[1]);
                    amountLot.Quantity = new Tuple<int?, string>(field.ConvertToNullableInt(), string.Empty);

                    field = contractLotItem.FirstOrDefault(x => x.Name == inputsContractLot[2]);
                    amountLot.Amount = new Tuple<decimal?, string>(field.ConvertToNullableDecimal(), string.Empty);
                }

                var inputContractDocument = new string[]
                {
                    "contractDocuments-documentType",
                    "user-document",
                    "contractDocuments-DocumentIDBDoc",
                    "contractDocuments-description",
                    "contractDocuments-statusId",
                };

                modelDetails.ContractDocuments.Clear();
                var fieldsContractsDocuments = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputContractDocument.Any(y => x.Name == y));
                var fieldsContractsDocumentsGrouped = fieldsContractsDocuments.GroupBy(x => x.Id);

                foreach (var contractDocItem in fieldsContractsDocumentsGrouped)
                {
                    var contractDoc = new ContractDataDocumentViewModel() { Id = contractDocItem.Key.ConvertToInt(0) };
                    modelDetails.ContractDocuments.Add(contractDoc);

                    field = contractDocItem.FirstOrDefault(x => x.Name == inputContractDocument[0]);
                    contractDoc.DocumentTypeId = field.ConvertToInt();

                    field = contractDocItem.FirstOrDefault(x => x.Name == inputContractDocument[1]);
                    contractDoc.User = field.ConvertToString();

                    field = contractDocItem.FirstOrDefault(x => x.Name == inputContractDocument[2]);
                    contractDoc.DocumentNumber = field.ConvertToString();

                    field = contractDocItem.FirstOrDefault(x => x.Name == inputContractDocument[3]);
                    contractDoc.Description = field.ConvertToString();

                    field = contractDocItem.FirstOrDefault(x => x.Name == inputContractDocument[4]);
                    contractDoc.NonObjectionStatusId = field.ConvertToInt();
                }

                model.ContractsTab.ContractDetails = modelDetails;

                var inputContractAmendment = new string[]
                {
                    "row-excluded-amendment",
                    "user-amendment",
                    "date-amendment",
                    "documentNumber-amendment",
                    "modifications-amendment",
                    "signatureDate-amendment",
                    "description-amendment",
                    "nonObjectionStatus-amendment",
                    "dateStartDate-amendment",
                    "dateEndDate-amendment",
                    "idAssociatedAmount-amendment",
                    "currencyType-amendment",
                    "totalAmount-amendment",
                    "idAssociatedLot-amendment",
                    "lotName-amendment",
                    "quantity-amendment",
                    "lotAmount-amendment",
                };

                model.ContractsTab.ContractDetails.ContractAmendments.Clear();
                var fieldsContractsAmendment = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputContractAmendment.Any(y => x.Name == y));
                var fieldsContractsAmendmentGrouped = fieldsContractsAmendment.GroupBy(x => x.Id);

                foreach (var contractAmendmentItem in fieldsContractsAmendmentGrouped)
                {
                    var amendment = new ContractDataDocumentViewModel() { Id = contractAmendmentItem.Key.ConvertToInt(0), DetailsAmendment = new ContractDetailsDataViewModel() };

                    field = contractAmendmentItem.FirstOrDefault(x => x.Name == inputContractAmendment[0]);
                    var isExcluded = field.ConvertToString() == "excluded";

                    if (!isExcluded)
                    {
                        model.ContractsTab.ContractDetails.ContractAmendments.Add(amendment);

                        field = contractAmendmentItem.FirstOrDefault(x => x.Name == inputContractAmendment[1]);
                        amendment.User = field.ConvertToString();

                        field = contractAmendmentItem.FirstOrDefault(x => x.Name == inputContractAmendment[2]);
                        amendment.Date = field.ConvertToNullableDateTime();

                        field = contractAmendmentItem.FirstOrDefault(x => x.Name == inputContractAmendment[3]);
                        amendment.DocumentNumber = field.ConvertToString();

                        field = contractAmendmentItem.FirstOrDefault(x => x.Name == inputContractAmendment[4]);
                        amendment.ModificationIds = field.ConvertToIntList();

                        field = contractAmendmentItem.FirstOrDefault(x => x.Name == inputContractAmendment[5]);
                        amendment.DetailsAmendment.SignatureDate = new Tuple<DateTime?, string>(field.ConvertToNullableDateTime(), string.Empty);

                        field = contractAmendmentItem.FirstOrDefault(x => x.Name == inputContractAmendment[6]);
                        amendment.Description = field.ConvertToString();

                        field = contractAmendmentItem.FirstOrDefault(x => x.Name == inputContractAmendment[7]);
                        amendment.NonObjectionStatusId = field.ConvertToInt();

                        field = contractAmendmentItem.FirstOrDefault(x => x.Name == inputContractAmendment[8]);
                        amendment.DetailsAmendment.StartDate = new Tuple<DateTime?, string>(field.ConvertToNullableDateTime(), string.Empty);

                        field = contractAmendmentItem.FirstOrDefault(x => x.Name == inputContractAmendment[9]);
                        amendment.DetailsAmendment.EndDate = new Tuple<DateTime?, string>(field.ConvertToNullableDateTime(), string.Empty);

                        var currencyAmountAmendmentGrouped = contractAmendmentItem.Where(x => x.ExtraData.ContainsKey("data-persist-amountid")).GroupBy(x => x.ExtraData["data-persist-amountid"]);
                        foreach (var currencyAmountAmendmentItem in currencyAmountAmendmentGrouped)
                        {
                            var amountModel = new ContractDataAmountViewModel() { Id = currencyAmountAmendmentItem.Key.ConvertToInt(0) };
                            amendment.DetailsAmendment.CurrencyAmounts.Add(amountModel);

                            field = currencyAmountAmendmentItem.FirstOrDefault(x => x.Name == inputContractAmendment[10]);
                            amountModel.IdAssociatedAmount = field.ConvertToNullableInt();

                            field = currencyAmountAmendmentItem.FirstOrDefault(x => x.Name == inputContractAmendment[11]);
                            amountModel.CurrencyAmountTypeId = field.ConvertToInt();

                            field = currencyAmountAmendmentItem.FirstOrDefault(x => x.Name == inputContractAmendment[12]);
                            amountModel.TotalContractAmount = new Tuple<decimal?, string>(field.ConvertToNullableDecimal(), string.Empty);
                        }

                        var lotAmendmentGrouped = contractAmendmentItem.Where(x => x.ExtraData.ContainsKey("data-persist-lotid")).GroupBy(x => x.ExtraData["data-persist-lotid"]);
                        foreach (var lotAmendmentItem in lotAmendmentGrouped)
                        {
                            var lotModel = new ContractDataLotViewModel() { Id = lotAmendmentItem.Key.ConvertToInt(0) };
                            amendment.DetailsAmendment.Lots.Add(lotModel);

                            field = lotAmendmentItem.FirstOrDefault(x => x.Name == inputContractAmendment[13]);
                            lotModel.IdAssociatedLot = field.ConvertToNullableInt();

                            field = lotAmendmentItem.FirstOrDefault(x => x.Name == inputContractAmendment[14]);
                            lotModel.Name = new Tuple<string, string>(field.ConvertToString(), string.Empty);

                            field = lotAmendmentItem.FirstOrDefault(x => x.Name == inputContractAmendment[15]);
                            lotModel.Quantity = new Tuple<int?, string>(field.ConvertToNullableInt(), string.Empty);

                            field = lotAmendmentItem.FirstOrDefault(x => x.Name == inputContractAmendment[16]);
                            lotModel.Amount = new Tuple<decimal?, string>(field.ConvertToNullableDecimal(), string.Empty);
                        }
                    }
                }
            }

            return model;
        }

        #endregion

        #region Cancel Process

        public static DeclareIneligibilityViewModel UpdateDeclareInegibility(this DeclareIneligibilityViewModel model, ClientFieldData[] formData)
        {
            var declareInegibilityFieldNames = new string[] 
            {
                "reasonText-inegibility",
                "reasonTypeId-inegbility"
            };

            model = new DeclareIneligibilityViewModel();

            var field = formData.FirstOrDefault(x => x.Name == declareInegibilityFieldNames[0]);
            model.Reason = field.ConvertToString();
            field = formData.FirstOrDefault(x => x.Name == declareInegibilityFieldNames[1]);
            model.IneligibilityReasonTypeId = field.ConvertToInt();

            return model;
        }

        public static CancelProcessViewModel UpdateCancelProcess(this CancelProcessViewModel model, ClientFieldData[] formData)
        {
            model = new CancelProcessViewModel();

            var cancelProcessFieldNames = new string[] 
            {
                "reason-cancelProcess"
            };

            var field = formData.FirstOrDefault(x => x.Name == cancelProcessFieldNames[0]);
            model.Reason = field.ConvertToString();
            return model;
        }

        #endregion

        #endregion

        #region Private Methods

        #region Procurement Plan

        private static void UpdateOperationLevel(this OperationLevelViewModel model, ClientFieldData[] formData)
        {
            ClientFieldData field;

            var noticeNames = new List<string>() 
            {
                "GeneralNotice-Description",
                "GeneralNotice-DocumentIDBDoc",
            };

            var noticesData = formData.Where(x => noticeNames.Contains(x.Name) && !string.IsNullOrEmpty(x.Id)).GroupBy(x => x.Id);

            model.GeneralNotice.Clear();
            foreach (var item in noticesData)
            {
                var notice = new DocumentNoticeRowViewModel();
                model.GeneralNotice.Add(notice);
                if (!item.Key.Contains("new"))
                {
                    notice.Id = item.Key.ConvertToInt();
                }

                field = item.FirstOrDefault(x => x.Name == noticeNames[0]);
                notice.Description = field.ConvertToString();

                field = item.FirstOrDefault(x => x.Name == noticeNames[1]);
                notice.DocumentNumber = field.ConvertToString();
            }

            var planNames = new List<string>() 
            {
                "ProcurementPlan-Description",
                "ProcurementPlan-DocumentIDBDoc",
            };

            var planData = formData.Where(x => planNames.Contains(x.Name) && !string.IsNullOrEmpty(x.Id)).GroupBy(x => x.Id);

            model.ProcurementPlanTable.Clear();
            foreach (var item in planData)
            {
                var planDoc = new ProcurementPlanRowViewModel();
                model.ProcurementPlanTable.Add(planDoc);
                if (!item.Key.Contains("new"))
                {
                    planDoc.Id = item.Key.ConvertToInt();
                }

                field = item.FirstOrDefault(x => x.Name == planNames[0]);
                planDoc.Description = field.ConvertToString();

                field = item.FirstOrDefault(x => x.Name == planNames[1]);
                planDoc.DocumentNumber = field.ConvertToString();
            }
        }

        private static void UpdateProcurementLevel(this ProcurementLevelViewModel model, ClientFieldData[] formData)
        {
            ClientFieldData field;

            var procNames = new List<string>() 
            {
                "procurementNameForPlan",
                "descriptionForAgency",
                "comment",
            };
                var procsModel = formData.First(x => x.Name == "serializedProcurements").ConvertToString();
                var procs = PageSerializationHelper.DeserializeObject<List<ProcurementRowViewModel>>(procsModel);
                model.Procurements = procs;

            var procurementsData = formData.Where(x => procNames.Contains(x.Name) && !string.IsNullOrEmpty(x.Id)).GroupBy(x => x.Id);

            foreach (var item in procurementsData)
            {
                var procId = item.Key.ConvertToInt();
                var procModel = model.Procurements.FirstOrDefault(x => x.PepProcurementId == procId);

                if (procModel != null)
                {
                    field = item.FirstOrDefault(x => x.Name == procNames[0]);
                    procModel.ProcurementName = field.ConvertToString();

                    field = item.FirstOrDefault(x => x.Name == procNames[1]);
                    procModel.Description = field.ConvertToString();

                    procModel.Comments.Clear();
                    foreach (var itemComment in item.Where(x => x.Name == procNames[2]))
                    {
                        var commentId = 0;
                        if (itemComment.ExtraData["data-persist-comment-id"] != "-1")
                        {
                            commentId = itemComment.ExtraData["data-persist-comment-id"].ConvertToInt();
                        }

                        procModel.Comments.Add(new ProcurementCommentViewModel()
                        {
                            Id = commentId,
                            Comment = itemComment.ConvertToString()
                        });
                    }
                }
            }
        }

        private static void UpdateParameterization(this ParameterizationViewModel model, ClientFieldData[] formData)
        {
            ClientFieldData field;

            var customMethodForm = new[]
            {   
                "parametrization-custom-superVisionMethodRB",
                "parametrization-custom-ProcurementType",
                "parametrization-custom-ProcurementMethod",
            };

            var customMethods = formData.Where(x =>
                !string.IsNullOrWhiteSpace(x.Id) &&
                (customMethodForm.Contains(x.Name) || x.Name.StartsWith(customMethodForm[0])));
            var customMethodsGrouped = customMethods.GroupBy(x => x.Id);

            model.CustomProcurementMethod.Clear();

            foreach (var item in customMethodsGrouped)
            {
                var customModel = new CustomProcurementMethodViewModel()
                {
                    IdMethod = item.Key.ConvertToInt()
                };

                field = item.FirstOrDefault(x => x.Name == customMethodForm[1]);
                customModel.ProcurementTypeId = field.ConvertToIntList();

                field = item.FirstOrDefault(x => x.Name == customMethodForm[2]);
                customModel.CustomProcurementMethodText = field.ConvertToString();

                field = item.FirstOrDefault(x => x.Name.StartsWith(customMethodForm[0]));
                customModel.IsMethodExAnte = field.ConvertToBool();

                model.CustomProcurementMethod.Add(customModel);
            }

            var nationalSystemForm = new[]
            {
                "parametrization-national-chb",
            };

            var nationalSystems = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && nationalSystemForm.Contains(x.Name));
            var nationalSystemGrouped = nationalSystems.GroupBy(x => x.Id);

            foreach (var item in nationalSystemGrouped)
            {
                var methodId = item.Key.ConvertToInt();
                var nationalModel = model.SelectNationalSystem.First(x => x.MethodId == methodId);

                field = item.FirstOrDefault(x => x.Name == nationalSystemForm[0]);
                nationalModel.IsSelected = field.ConvertToBool();
            }

            var executingAgencyFields = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && x.Name == "threshold" && x.ExtraData.ContainsKey("data-persist-agency-id"));
            var executingAgencyGrouped = executingAgencyFields.GroupBy(x => x.ExtraData["data-persist-agency-id"]);

            foreach (var agency in model.ExecutingAgencys)
            {
                var fields = executingAgencyGrouped.First(x => x.Key == agency.ExecutingAgencyId.ToString());

                foreach (var agencyRow in agency.ExecutingAgencyRows)
                {
                    field = fields.FirstOrDefault(x => (x.Name == "threshold") && (x.Id == agencyRow.Group.ToString()));
                    agencyRow.ThresholdDecimal = field.ConvertToNullableDecimal();
                }
            }
        }

        #endregion

        #endregion
    }
}