using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.MrBlueModule.Enums;
using IDB.MW.Application.MrBlueModule.ViewModels.GeneralInformation;
using IDB.MW.Application.MrBlueModule.ViewModels.HighRisk;
using IDB.MW.Application.MrBlueModule.ViewModels.ManageMediaFiles;
using IDB.MW.Application.MrBlueModule.ViewModels.SafeguardToolkit;
using IDB.MW.Application.MrBlueModule.ViewModels.Star;
using IDB.MW.Application.MrBlueModule.ViewModels.SupervisionReport;
using IDB.MW.Infrastructure.Configuration;
using IDB.Presentation.MVC4.Models.ClientFieldData;

namespace IDB.Presentation.MVC4.Areas.MrBlue.Models
{
    public static class ClientFieldDataMappers
    {
        #region Constants

        private const string SUPERVISION_REPORT_DATE_FROM = "DateFrom";
        private const string SUPERVISION_REPORT_DATE_TO = "DateTo";
        private const string SUPERVISION_REPORT_PARTICIPANT = "esgPersonRow_text";

        #endregion

        #region Mappers

        #region General Information

        public static void UpdateGeneralInformationlDataViewModel(this GeneralInformationViewModel viewModel, ClientFieldData[] clientFieldData, IEnumMappingService enumMappingService)
        {
            // Reviewer
            viewModel.ESGReviewers.UpdateEsgReviewer(clientFieldData, enumMappingService);

            // Components
            viewModel.SustainabilityComponents.UpdateSustainabilityComponents(clientFieldData, enumMappingService);

            //Documents
            viewModel.Documents.UpdateDocuments(clientFieldData, enumMappingService);

            //Comments
            viewModel.CommentsESGOperation.UpdateCommentsESGOperation(clientFieldData, enumMappingService);
        }

        #endregion

        #region Supervision Report

        public static void UpdateSupervisionReportStep1(this SupervisionReportStep1ViewModel viewmodel, ClientFieldData[] clientFieldData, IEnumMappingService enumMappingService)
        {
            var dateFrom = clientFieldData.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c.Name) && c.Name.Contains(SUPERVISION_REPORT_DATE_FROM));
            if (!string.IsNullOrEmpty(dateFrom.Value) && dateFrom.Value != null)
            {
                viewmodel.DateFrom = DateTime.ParseExact(dateFrom.Value, ConfigurationServiceFactory.Current.GetApplicationSettings().FormatDateUpload, CultureInfo.InvariantCulture);
            }

            var dateTo = clientFieldData.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c.Name) && c.Name.Contains(SUPERVISION_REPORT_DATE_TO));
            if (!string.IsNullOrEmpty(dateTo.Value) && dateFrom.Value != null)
            {
                viewmodel.DateTo = DateTime.ParseExact(dateTo.Value, ConfigurationServiceFactory.Current.GetApplicationSettings().FormatDateUpload, CultureInfo.InvariantCulture);
            }

            viewmodel.SupervisionReportParticipants.UpdateSupervisionReportParticipants(clientFieldData, enumMappingService);
        }

        public static void UpdateSupervisionReportStep2(this SupervisionReportStep2ViewModel viewmodel, ClientFieldData[] clientFieldData, IEnumMappingService enumMappingService)
        {
            //ESPI TABLE
            viewmodel.SupervisionReportESPI.UpdateESPIReport(viewmodel.PreviousVersionId != 0, clientFieldData, enumMappingService);

            //NEW ACTIONS TABLE
            viewmodel.SupervisionReportNewActions.UpdateActions(viewmodel.PreviousVersionId != 0, clientFieldData, enumMappingService);

            //STAFF CONSULTED TABLE
            viewmodel.SupervisionReportStaffConsulted.UpdateStaff(clientFieldData, enumMappingService);

            //MEDIA FILES TABLE
            viewmodel.SupervisionReportMediaFiles.UpdateMediaFiles(clientFieldData);
        }

        #endregion

        #region Star

        public static void UpdateStar(this StarViewModel viewModel, ClientFieldData[] clientFieldData, IEnumMappingService enumMappingService)
        {
        }

        #endregion

        #region High Risk

        public static void UpdateHighRisk(this HighRiskViewModel viewModel, ClientFieldData[] clientFieldData, IEnumMappingService enumMappingService)
        {
        }

        #endregion

        #region SafeguardToolkit

        public static void UpdateSafeguardToolkit2(this SafeguardToolkitStep2ViewModel viewModel, ClientFieldData[] clientFieldData, IEnumMappingService enumMappingService)
        {
        }

        public static void UpdateSafeguardToolkit3(this SafeguardToolkitStep3ViewModel viewModel, ClientFieldData[] clientFieldData, IEnumMappingService enumMappingService)
        {
        }

        public static void UpdateSafeguardToolkit4(this SafeguardToolkitStep4ViewModel viewModel, ClientFieldData[] clientFieldData, IEnumMappingService enumMappingService)
        {
        }

        public static void UpdateSafeguardToolkit5(this SafeguardToolkitStep5ViewModel viewModel, ClientFieldData[] clientFieldData, IEnumMappingService enumMappingService)
        {
            var environmentalCategory = clientFieldData.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c.Name) && c.Name == "environmentalCategory");
            if (environmentalCategory != null)
            {
                viewModel.EnvironmentalCategory = enumMappingService.GetMappedEnum<ESGClassificationEnum>(int.Parse(environmentalCategory.Value));
            }

            var environmentalCategoryOverriden = clientFieldData.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c.Name) && c.Name == "environmentalCategoryOverriden");
            if (environmentalCategoryOverriden != null && !string.IsNullOrEmpty(environmentalCategoryOverriden.Value))
            {
                viewModel.EnvironmentalCategoryOverriden = enumMappingService.GetMappedEnum<ESGClassificationEnum>(int.Parse(environmentalCategoryOverriden.Value));
            }

            var environmentalCategoryJustification = clientFieldData.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c.Name) && c.Name == "environmentalCategoryJustification");
            if (environmentalCategoryJustification != null && !string.IsNullOrEmpty(environmentalCategoryJustification.Value))
            {
                viewModel.JustificationOverriden = enumMappingService.GetMappedEnum<ESGJustificationOverridenEnum>(int.Parse(environmentalCategoryJustification.Value));
            }

            var environmentalCategoryComment = clientFieldData.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c.Name) && c.Name == "environmentalCategoryComment");
            if (environmentalCategoryComment != null)
            {
                viewModel.EnvironmentalCategoryComment = environmentalCategoryComment.Value;
            }

            var disasterRiskCategory = clientFieldData.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c.Name) && c.Name == "disasterRiskCategory");
            if (disasterRiskCategory != null)
            {
                viewModel.DisasterRiskCategoryString = disasterRiskCategory.Value;
            }

            var policiesDirectivesTriggered = clientFieldData.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c.Name) && c.Name == "policiesDirectivesTriggered");
            if (policiesDirectivesTriggered != null)
            {
                viewModel.PoliciesDirectivesTriggered = policiesDirectivesTriggered.Value;
            }

            var potencialPoliciesDirectivesTriggered = clientFieldData.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c.Name) && c.Name == "potenctialPoliciesDirectivesTriggered");
            if (potencialPoliciesDirectivesTriggered != null)
            {
                viewModel.PotentialPoliciesDirectivesTriggered = potencialPoliciesDirectivesTriggered.Value;
            }

            var evriomentalOverridden = clientFieldData.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c.Name) && c.Name == "enviromentalOverridden");
            if (evriomentalOverridden != null)
            {
                viewModel.enviromentalOverridden = evriomentalOverridden.Value == "True";
            }
            else
            {
                viewModel.enviromentalOverridden = false;
            }
        }

        public static void UpdateSafeguardToolkit6(this SafeguardToolkitStep6ViewModel viewModel, ClientFieldData[] clientFieldData, IEnumMappingService enumMappingService)
        {
        }

        #endregion

        #endregion

        #region Private Methods

        private static void UpdateSupervisionReportParticipants(this List<SupervisionReportParticipantsRowViewModel> participantsViewModelList, ClientFieldData[] clientFieldData, IEnumMappingService enumMappingService)
        {
            var participants = clientFieldData.Where(x => x.Name == SUPERVISION_REPORT_PARTICIPANT && !string.IsNullOrEmpty(x.Value));
            participantsViewModelList.Clear();

            foreach (var item in participants)
            {
                var viewModel = new SupervisionReportParticipantsRowViewModel();

                //Calculate data from Active Directory or user entry.
                string loginName = string.Empty;
                string fullName = string.Empty;
                bool isActiveDirectory = bool.Parse(item.ExtraData.Single(x => x.Key == "data-persist-is-active-directory").Value);
                if (isActiveDirectory)
                {
                    loginName = item.ExtraData.Single(x => x.Key == "data-persist-login-name").Value;
                    fullName = item.ExtraData.Single(x => x.Key == "data-persist-full-name").Value;
                }
                else
                {
                    loginName = item.Value;
                    fullName = item.Value;
                }

                //Map view model
                viewModel.ADUserName = loginName;
                viewModel.ADFullName = fullName;
                viewModel.IsActiveDirectory = isActiveDirectory;

                participantsViewModelList.Add(viewModel);
            }
        }

        private static void UpdateEsgReviewer(this List<ESGReviewersViewModel> esgReviewersViewModelList, ClientFieldData[] clientFieldData, IEnumMappingService enumMappingService)
        {
            var clientFieldDataReviewer = clientFieldData.Where(x => x.Name == "NameReviewer_text");

            var reviewerOldList = clientFieldDataReviewer.Where(x => !string.IsNullOrWhiteSpace(x.Id));
            var reviewerNewList = clientFieldDataReviewer.Where(x => string.IsNullOrWhiteSpace(x.Id));

            var originalId = esgReviewersViewModelList.Select(x => x.RowId).Distinct();
            var finalId = reviewerOldList.Select(x => x.Id).Distinct();
            var idsToRemove = originalId.Where(x => finalId.All(y => x.ToString() != y)).ToList();

            foreach (var idRemove in idsToRemove)
            {
                esgReviewersViewModelList.Remove(esgReviewersViewModelList.First(x => x.RowId == idRemove));
            }

            foreach (var reviewerlistOld in reviewerOldList)
            {
                var esgReviewerViewModel = esgReviewersViewModelList.First(x => x.RowId == int.Parse(reviewerlistOld.Id));

                if (!string.IsNullOrWhiteSpace(reviewerlistOld.Value))
                {
                    switch (reviewerlistOld.Name)
                    {
                        case "NameReviewer_text":
                            esgReviewerViewModel.ADUserName = reviewerlistOld.Value;
                            break;
                    }
                }
            }

            var esgReviewerToInsertAll = reviewerNewList.Where(x => x.ExtraData.Any(y => y.Key == "data-persist-new"));

            var esgReviewerToInsert = 
                from reviewerNew in esgReviewerToInsertAll
                group reviewerNew
                by 
                reviewerNew.ExtraData.First().Value 
                into specialist
                select new ESGReviewersViewModel
                {
                    ADUserName = specialist.First(x => x.Name == "NameReviewer_text").Value,
                };

            esgReviewersViewModelList.AddRange(esgReviewerToInsert);
        }

        private static void UpdateSustainabilityComponents(this List<SustainabilityComponentsViewModel> sustainabilityComponentsViewModelList, ClientFieldData[] clientFieldData, IEnumMappingService enumMappingService)
        {
            var clientFieldDataSustainabilityComponent = clientFieldData.Where(x => x.Name == "FinancingTypeId"
             || x.Name == "FundId" || x.Name == "FundAmount" || x.Name == "SustainabilityTypeId"
             || x.Name == "SustainabilityComponentsId" || x.Name == "ComponentAmount");

            var sustainabilityComponentOldList = clientFieldDataSustainabilityComponent.Where(x => !string.IsNullOrWhiteSpace(x.Id));
            var sustainabilityComponentNewList = clientFieldDataSustainabilityComponent.Where(x => string.IsNullOrWhiteSpace(x.Id));

            var originalId = sustainabilityComponentsViewModelList.Select(x => x.SustainabilityComponentsId).Distinct();
            var finalId = sustainabilityComponentOldList.Select(x => x.Id).Distinct();
            var idsToRemove = originalId.Where(x => finalId.All(y => x.ToString() != y)).ToList();

            foreach (var idRemove in idsToRemove)
            {
                sustainabilityComponentsViewModelList.Remove(sustainabilityComponentsViewModelList.First(x => x.SustainabilityComponentsId == idRemove));
            }

            foreach (var sustainabilityComponentOld in sustainabilityComponentOldList)
            {
                var sustainabilityComponentViewModel = sustainabilityComponentsViewModelList.First(x => x.SustainabilityComponentsId == int.Parse(sustainabilityComponentOld.Id));

                switch (sustainabilityComponentOld.Name)
                {
                    case "FinancingTypeId":
                        sustainabilityComponentViewModel.FinancingTypeId = enumMappingService.GetMappedEnum<ESGFinancingTypeEnum>(int.Parse(sustainabilityComponentOld.Value));
                        break;
                    case "FundId":
                        sustainabilityComponentViewModel.FundId = int.Parse(sustainabilityComponentOld.Value);
                        break;
                    case "FundAmount":
                        sustainabilityComponentViewModel.FundAmount = decimal.Parse(sustainabilityComponentOld.Value);
                        break;
                    case "SustainabilityTypeId":
                        sustainabilityComponentViewModel.SustainabilityTypeId = enumMappingService.GetMappedEnum<ESGSustainabilityTypeEnum>(int.Parse(sustainabilityComponentOld.Value));
                        break;
                    case "SustainabilityComponentsId":
                        sustainabilityComponentViewModel.ComponentId = int.Parse(sustainabilityComponentOld.Value);
                        break;
                    case "ComponentAmount":
                        sustainabilityComponentViewModel.ComponentAmount = decimal.Parse(sustainabilityComponentOld.Value);
                        break;
                }
            }

            var sustainabilityComponentToInsertAll = sustainabilityComponentNewList.Where(x => x.ExtraData.Any(y => y.Key == "data-persist-new"));

            var sustainabilityComponentsToInsert = from sustainabilityComponentNew in sustainabilityComponentToInsertAll
                                                   group sustainabilityComponentNew by sustainabilityComponentNew.ExtraData.First().Value
                                                       into sustainabilityComponent
                                                       select new
                                                       {
                                                           FinancingTypeId = int.Parse(sustainabilityComponent.First(x => x.Name == "FinancingTypeId").Value),
                                                           FundId = int.Parse(sustainabilityComponent.First(x => x.Name == "FundId").Value),
                                                           FundAmount = decimal.Parse(sustainabilityComponent.First(x => x.Name == "FundAmount").Value),
                                                           SustainabilityTypeId = int.Parse(sustainabilityComponent.First(x => x.Name.Contains("SustainabilityTypeId")).Value),
                                                           ComponentId = int.Parse(sustainabilityComponent.First(x => x.Name.Contains("SustainabilityComponentsId")).Value),
                                                           ComponentAmount = decimal.Parse(sustainabilityComponent.First(x => x.Name == "ComponentAmount").Value)
                                                       };

            var list = new List<SustainabilityComponentsViewModel>();
            foreach (var item in sustainabilityComponentsToInsert)
            {
                var sustainabilityComponentsViewModel = new SustainabilityComponentsViewModel
                {
                    FinancingTypeId = enumMappingService.GetMappedEnum<ESGFinancingTypeEnum>(item.FinancingTypeId),
                    FundId = item.FundId,
                    FundAmount = item.FundAmount,
                    SustainabilityTypeId = enumMappingService.GetMappedEnum<ESGSustainabilityTypeEnum>(item.SustainabilityTypeId),
                    ComponentId = item.ComponentId,
                    ComponentAmount = item.ComponentAmount
                };
                sustainabilityComponentsViewModelList.Add(sustainabilityComponentsViewModel);
            }
        }

        private static void UpdateCommentsESGOperation(this List<CommentsViewModel> commentsViewModel, ClientFieldData[] clientFieldData, IEnumMappingService enumMappingService)
        {
            var clientFieldDataSpecialists = clientFieldData.Where(x => x.Name == "comment" && x.Id != "-1");

            var commentsOldList = clientFieldDataSpecialists.Where(x => x.Id != "0");
            var commentsNewList = clientFieldDataSpecialists.Where(x => x.Id == "0");

            var originalId = commentsViewModel.Select(x => x.CommentId).Distinct();
            var finalId = commentsOldList.Select(x => x.Id).Distinct();
            var idsToRemove = originalId.Where(x => finalId.All(y => x.ToString() != y)).ToList();

            foreach (var idRemove in idsToRemove)
            {
                commentsViewModel.Remove(commentsViewModel.First(x => x.CommentId == idRemove));
            }

            foreach (var commentsOld in commentsOldList)
            {
                var commentsViewModelAux = commentsViewModel.First(x => x.CommentId == int.Parse(commentsOld.Id));

                switch (commentsOld.Name)
                {
                    case "comment":
                        commentsViewModelAux.Comment = commentsOld.Value;
                        break;
                }
            }

            var commentsToInsertAll = commentsNewList.Where(x => x.ExtraData.Any(y => y.Key == "data-persist-new"));

            var commentsToInsert = 
                from commentNew in commentsToInsertAll
                group commentNew
                by 
                commentNew.ExtraData.First().Value 
                into comment
                select new CommentsViewModel
                {
                    Comment = comment.First(x => x.Name == "comment").Value,
                };

            commentsViewModel.AddRange(commentsToInsert);
        }

        private static void UpdateDocuments(this List<DocumentViewModel> documentViewModelList, ClientFieldData[] clientFieldData, IEnumMappingService enumMappingService)
        {
            var formDocuments = clientFieldData.Where(x => x.Name == "typeDocument" || x.Name == "DocumentGeneralInformationTable-docNumber" || x.Name == "DocumentGeneralInformationTable-docName");

            var newDocumentsData = formDocuments.Where(d => d.Id == "newDocument");
            var oldDocumentsData = formDocuments.Where(d => d.Id != "newDocument");

            // Delete old DocNumber - Document Reference
            var finalIds = oldDocumentsData.Select(x => x.Id).Distinct();
            var originalIds = documentViewModelList.Select(x => x.DocumentId.ToString());
            var idsToRemove = originalIds.Where(x => !finalIds.Any(y => x == y)).ToList();

            foreach (var id in idsToRemove)
            {
                var elementToRemove = documentViewModelList.First(x => x.DocumentId.ToString() == id);
                documentViewModelList.Remove(elementToRemove);
            }

            // Update old elements
            var updatedGrouped = oldDocumentsData.GroupBy(x => x.Id);
            foreach (var doc in updatedGrouped)
            {
                var documentViewModel = documentViewModelList.First(x => x.DocumentId == int.Parse(doc.Key));
                if (documentViewModel != null)
                {
                    var type = doc.FirstOrDefault(x => x.Name == "typeDocument");

                    if (!string.IsNullOrEmpty(type.Value))
                    {
                        documentViewModel.TypeId = int.Parse(type.Value);
                    }
                }
            }

            // Add new DocNumber - Document Reference
            var newGrouped = newDocumentsData.GroupBy(x => x.ExtraData.FirstOrDefault(y => y.Key == "data-persist-new"));
            foreach (var _new in newGrouped)
            {
                var docName = _new.FirstOrDefault(x => x.Name == "DocumentGeneralInformationTable-docName");
                var docNumber = _new.FirstOrDefault(x => x.Name == "DocumentGeneralInformationTable-docNumber");
                var type = _new.FirstOrDefault(x => x.Name == "typeDocument");

                if (!(string.IsNullOrEmpty(docNumber.Value) || string.IsNullOrEmpty(type.Value) || string.IsNullOrEmpty(docName.Value)))
                {
                    var newDocument = new DocumentViewModel()
                    {
                        DocumentName = docName.Value,
                        DocNumber = docNumber.Value,
                        TypeId = int.Parse(type.Value)
                    };
                    documentViewModelList.Add(newDocument);
                }
            }
        }

        private static void UpdateESPIReport(this List<SupervisionReportESPIRowViewModel> viewModel, bool isCloned, ClientFieldData[] clientFieldData, IEnumMappingService enumMappingService)
        {
            var clientFieldDataInputList = clientFieldData
                .Where(x => 
                    (x.Name == "espiType" || 
                        x.Name == "espiRating" || 
                        x.Name == "ratingJustification" || 
                        x.Name == "espiComment") && 
                    !string.IsNullOrEmpty(x.Value));

            var oldGroupList = clientFieldDataInputList.Where(x => !string.IsNullOrWhiteSpace(x.Id)).GroupBy(x => x.Id);

            viewModel.Clear();

            foreach (var item in oldGroupList)
            {
                var rowViewModel = new SupervisionReportESPIRowViewModel();

                //Map view model from ClientFieldData
                //If data-id attribute contains new- in front of the Id, the row is new; else the attribute data-id will have the primary key from a row that already exists.
                rowViewModel.RowId = item.Key.Contains("new") || isCloned ? 0 : int.Parse(item.Key);

                rowViewModel.Type = int.Parse(item.Single(c => c.Name == "espiType").Value);
                rowViewModel.Rating = int.Parse(item.Single(c => c.Name == "espiRating").Value);
                rowViewModel.RatingJustification = item.Single(c => c.Name == "ratingJustification").Value;
                rowViewModel.Comments = item.Single(c => c.Name == "espiComment").Value;

                viewModel.Add(rowViewModel);
            }
        }

        private static void UpdateActions(this List<SupervisionReportActionsRowViewModel> viewModel, bool isCloned, ClientFieldData[] clientFieldData, IEnumMappingService enumMappingService)
        {
            var clientFieldDataInputList = clientFieldData
                .Where(x => (x.Name == "esgPersonActions_text" || 
                        x.Name == "newActionCompleteActionDate" || 
                        x.Name == "newStatus" || 
                        x.Name == "action" || 
                        x.Name == "actionComment") && 
                    !string.IsNullOrEmpty(x.Value));

            var oldGroupList = clientFieldDataInputList.Where(x => !string.IsNullOrWhiteSpace(x.Id)).GroupBy(x => x.Id);

            viewModel.Clear();

            foreach (var item in oldGroupList)
            {
                var rowViewModel = new SupervisionReportActionsRowViewModel();

                //Map view model from ClientFieldData
                //If data-id attribute contains new- in front of the Id, the row is new; else the attribute data-id will have the primary key from a row that already exists.
                rowViewModel.RowId = item.Key.Contains("new") || isCloned ? 0 : int.Parse(item.Key);

                rowViewModel.ADUserName = item.Single(c => c.Name == "esgPersonActions_text").Value;
                rowViewModel.ADFullName = item.Single(c => c.Name == "esgPersonActions_text").Value;
                rowViewModel.Status = int.Parse(item.Single(c => c.Name == "newStatus").Value);
                var a = item.Single(c => c.Name == "newActionCompleteActionDate").Value;
                var b = ConfigurationServiceFactory.Current.GetApplicationSettings();
                rowViewModel.ActionCompletedOn = DateTime.ParseExact(a, b.FormatDateUpload, CultureInfo.CurrentCulture);
                rowViewModel.Action = item.Single(c => c.Name == "action").Value;
                rowViewModel.Comments = item.Single(c => c.Name == "actionComment").Value;

                viewModel.Add(rowViewModel);
            }
        }

        private static void UpdateStaff(this List<SupervisionReportStaffConsultedRowViewModel> viewModel, ClientFieldData[] clientFieldData, IEnumMappingService enumMappingService)
        {
            var participants = clientFieldData.Where(x => x.Name == SUPERVISION_REPORT_PARTICIPANT && !string.IsNullOrEmpty(x.Value));
            viewModel.Clear();

            foreach (var item in participants)
            {
                var rowViewModel = new SupervisionReportStaffConsultedRowViewModel();

                //Calculate data from Active Directory or user entry.
                string loginName = string.Empty;
                string fullName = string.Empty;
                bool isActiveDirectory = bool.Parse(item.ExtraData.Single(x => x.Key == "data-persist-is-active-directory").Value);
                if (isActiveDirectory)
                {
                    loginName = item.ExtraData.Single(x => x.Key == "data-persist-login-name").Value;
                    fullName = item.ExtraData.Single(x => x.Key == "data-persist-full-name").Value;
                }
                else
                {
                    loginName = item.Value;
                    fullName = item.Value;
                }

                //Map view model
                rowViewModel.ADUserName = loginName;
                rowViewModel.ADFullName = fullName;
                rowViewModel.IsActiveDirectory = isActiveDirectory;

                viewModel.Add(rowViewModel);
            }
        }

        private static void UpdateMediaFiles(this List<ManageMediaFilesRowViewModel> documentViewModelList, ClientFieldData[] clientFieldData)
        {
            var formDocuments = clientFieldData.Where(x => x.Name == "documentMediaTable-RowId");

            var newDocumentsData = formDocuments.Where(d => d.Id == "newDocument" && d.Id != "template");
            var oldDocumentsData = formDocuments.Where(d => d.Id != "newDocument" && d.Id != "template");

            // Delete old DocNumber - Document Reference
            var finalIds = oldDocumentsData.Select(x => x.Id).Distinct();
            var originalIds = documentViewModelList.Select(x => x.RowId.ToString());
            var idsToRemove = originalIds.Where(x => !finalIds.Any(y => x == y)).ToList();

            foreach (var id in idsToRemove)
            {
                var elementToRemove = documentViewModelList.First(x => x.RowId.ToString() == id);
                documentViewModelList.Remove(elementToRemove);
            }

            // Add new 
            var newGrouped = newDocumentsData.GroupBy(x => x.ExtraData.FirstOrDefault(y => y.Key == "data-persist-new"));
            foreach (var _new in newGrouped)
            {
                var docId = _new.FirstOrDefault(x => x.Name == "documentMediaTable-RowId");

                if (!string.IsNullOrEmpty(docId.Value))
                {
                    var newDocument = new ManageMediaFilesRowViewModel()
                    {
                        RowId = int.Parse(docId.Value)
                    };
                    documentViewModelList.Add(newDocument);
                }
            }
        }

        #endregion
    }
}