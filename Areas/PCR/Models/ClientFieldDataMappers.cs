using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;

using IDB.MW.Application.PCRModule.ViewModels.ChecklistService;
using IDB.Presentation.MVC4.Models.ClientFieldData;
using IDB.MW.Application.PCRModule.ViewModels.FollowUpService;
using IDB.MW.Application.PCRModule.ViewModels.FollowUpService.Tasks;
using IDB.MW.Application.PCRModule.ViewModels.DocumentService;
using IDB.MW.DomainModel.Entities.PCRModule.Enums;
using IDB.MW.Application.PCRModule.ViewModels.WorkflowIntegrationService;
using IDB.MW.Application.Core.ViewModels;

namespace IDB.Presentation.MVC4.Areas.PCR.Models
{
    public static class ClientFieldDataMappers
    {
        #region Constants
        private const string Format = "dd/MM/yyyy";
        #endregion

        #region PCR-1
        public static void UpdatePCREffectivenessViewModel(this PCREffectivenessViewModel effectivenessViewModel, ClientFieldData[] clientFieldData)
        {
            var outputFields = clientFieldData.Where(c => c.Name.Contains("Output")).ToList();
            var outcomeFields = clientFieldData.Where(c => c.Name.Contains("Outcome")).ToList();

            foreach (var outputField in outputFields.Where(o => !string.IsNullOrWhiteSpace(o.Id) && o.Name == "txtOutputAchieved"))
            {
                var outputFieldId = Convert.ToInt32(outputField.Id);
                var component = effectivenessViewModel.ComponentList.First(c => c.OutputList.Any(o => o.OutputId == outputFieldId));
                var outputRow = component.OutputList.First(o => o.OutputId == outputFieldId);
                var outputCommentField = outputFields.FirstOrDefault(o => o.Id == outputField.Id && o.Name == "txtOutputComment");
                var outputAchievedField = outputFields.FirstOrDefault(o => o.Id == outputField.Id && o.Name == "txtOutputAchieved");

                if (outputCommentField != null)
                {
                    if (outputRow.CommentsList == null)
                    {
                        outputRow.CommentsList = new List<OutputUserComment>();
                    }

                    var commentId = int.Parse(outputCommentField.ExtraData.FirstOrDefault(x => x.Key == "data-persist-id").Value);

                    if (commentId != 0)
                    {
                        //Update comment
                        var comment = outputRow.CommentsList.First(x => x.CommentId == commentId);

                        if (comment.Comment != outputCommentField.Value)
                        {
                            comment.Comment = outputCommentField.Value;
                            comment.Date = DateTime.Now;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(outputCommentField.Value))
                    {
                        //Add comment
                        var newComment = new OutputUserComment
                        {
                            Comment = outputCommentField.Value,
                            Date = DateTime.Now,
                            PCRStageType = effectivenessViewModel.PCRStageType
                        };

                        outputRow.CommentsList.Add(newComment);
                    }
                }

                var achievedValue = outputAchievedField == null || string.IsNullOrWhiteSpace(outputAchievedField.Value) ? (decimal?)null : Convert.ToDecimal(outputAchievedField.Value);

                switch (effectivenessViewModel.PCRStageType)
                {
                    case PCRStageTypeEnum.TeamLeaderPreValidationStage:
                        outputRow.AchievedBasedOnTeamLeaderPreValidation = achievedValue;
                        break;
                    case PCRStageTypeEnum.SPDPreValidationStage:
                        outputRow.AchievedBasedOnSPDPreValidation = achievedValue;
                        break;
                    case PCRStageTypeEnum.TeamLeaderValidationStage:
                        outputRow.AchievedBasedOnTeamLeaderValidation = achievedValue;
                        break;
                    case PCRStageTypeEnum.SPDValidationStage:
                        outputRow.AchievedBasedOnSPDValidation = achievedValue;
                        break;
                }
            }

            foreach (var outcomeField in outcomeFields.Where(o => !string.IsNullOrWhiteSpace(o.Id) && o.Name == "txtOutcomeAchieved"))
            {
                var outcomeFieldId = Convert.ToInt32(outcomeField.Id);
                var outcome = effectivenessViewModel.OutcomeList.First(o => o.OutcomeIndicatorList.Any(or => or.OutcomeIndicatorId == outcomeFieldId));
                var outcomeIndicator = outcome.OutcomeIndicatorList.First(o => o.OutcomeIndicatorId == outcomeFieldId);

                var outcomeAchievedCommentField = outcomeFields.FirstOrDefault(o => o.Id == outcomeField.Id && o.Name == "txtOutcomeAchievedComment");
                var outcomeAchievedField = outcomeFields.FirstOrDefault(o => o.Id == outcomeField.Id && o.Name == "txtOutcomeAchieved");

                var outcomeAttributableField = outcomeFields.FirstOrDefault(o => o.Id == outcomeField.Id && o.Name == "cboOutcomeAttributable");
                var outcomeAttributableCommentField = outcomeFields.FirstOrDefault(o => o.Id == outcomeField.Id && o.Name == "txtOutcomeAttributableComment");

                //Achieved comment
                if (outcomeAchievedCommentField != null)
                {
                    if (outcomeIndicator.CommentsList == null)
                    {
                        outcomeIndicator.CommentsList = new List<OutcomeIndicatorUserComment>();
                    }

                    var commentId = int.Parse(outcomeAchievedCommentField.ExtraData.FirstOrDefault(x => x.Key == "data-persist-id").Value);

                    if (commentId != 0)
                    {
                        //Update comment
                        var comment = outcomeIndicator.CommentsList.FirstOrDefault(x => x.CommentId == commentId);

                        if (comment.Comment != outcomeAchievedCommentField.Value)
                        {
                            comment.Comment = outcomeAchievedCommentField.Value;
                            comment.Date = DateTime.Now;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(outcomeAchievedCommentField.Value))
                    {
                        //Add comment
                        var newComment = new OutcomeIndicatorUserComment
                        {
                            IsAttributableComment = false,
                            Comment = outcomeAchievedCommentField.Value,
                            Date = DateTime.Now,
                            PCRStageType = effectivenessViewModel.PCRStageType
                        };

                        outcomeIndicator.CommentsList.Add(newComment);
                    }
                }

                //Attributable comment
                if (outcomeAttributableCommentField != null)
                {
                    if (outcomeIndicator.CommentsList == null)
                    {
                        outcomeIndicator.CommentsList = new List<OutcomeIndicatorUserComment>();
                    }

                    var commentId = int.Parse(outcomeAttributableCommentField.ExtraData.FirstOrDefault(x => x.Key == "data-persist-id").Value);

                    if (commentId != 0)
                    {
                        //Update comment
                        var comment = outcomeIndicator.CommentsList.FirstOrDefault(x => x.CommentId == commentId);

                        if (comment.Comment != outcomeAttributableCommentField.Value)
                        {
                            comment.Comment = outcomeAttributableCommentField.Value;
                            comment.Date = DateTime.Now;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(outcomeAttributableCommentField.Value))
                    {
                        //Add comment
                        var newComment = new OutcomeIndicatorUserComment
                        {
                            IsAttributableComment = true,
                            Comment = outcomeAttributableCommentField.Value,
                            Date = DateTime.Now,
                            PCRStageType = effectivenessViewModel.PCRStageType
                        };

                        outcomeIndicator.CommentsList.Add(newComment);
                    }
                }

                var outcomeAchievedValue = outcomeAchievedField == null || string.IsNullOrWhiteSpace(outcomeAchievedField.Value) ? (decimal?)null : Convert.ToDecimal(outcomeAchievedField.Value);
                var outcomeAttributableValue = outcomeAttributableField == null || string.IsNullOrWhiteSpace(outcomeAttributableField.Value) ? (bool?)null : Convert.ToBoolean(outcomeAttributableField.Value);

                switch (effectivenessViewModel.PCRStageType)
                {
                    case PCRStageTypeEnum.TeamLeaderPreValidationStage:
                        outcomeIndicator.AchievedBasedOnTeamLeaderPreValidation = outcomeAchievedValue;
                        outcomeIndicator.AttributableBasedOnTeamLeaderPreValidation = outcomeAttributableValue;
                        break;
                    case PCRStageTypeEnum.SPDPreValidationStage:
                        outcomeIndicator.AchievedBasedOnSPDPreValidation = outcomeAchievedValue;
                        outcomeIndicator.AttributableBasedOnSPDPreValidation = outcomeAttributableValue;
                        break;
                    case PCRStageTypeEnum.TeamLeaderValidationStage:
                        outcomeIndicator.AchievedBasedOnTeamLeaderValidation = outcomeAchievedValue;
                        outcomeIndicator.AttributableBasedOnTeamLeaderValidation = outcomeAttributableValue;
                        break;
                    case PCRStageTypeEnum.SPDValidationStage:
                        outcomeIndicator.AchievedBasedOnSPDValidation = outcomeAchievedValue;
                        outcomeIndicator.AttributableBasedOnSPDValidation = outcomeAttributableValue;
                        break;
                }
            }
        }

        public static void UpdatePCRSummaryViewModel(this PCRSummaryViewModel summaryViewModel, ClientFieldData[] clientFieldData)
        {
            var lendingProgramList = clientFieldData.Where(c => !string.IsNullOrWhiteSpace(c.Name) && c.Name.Contains("cboLendingProgram")).ToList();
            summaryViewModel.SelectedLendingPrograms = GetValues(lendingProgramList.FirstOrDefault());

            var countryObjective = clientFieldData.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c.Name) && c.Name.Contains("countryObjective"));
            if (countryObjective != null)
            {
                summaryViewModel.CountryObjective = countryObjective.Value;
            }

            var programRelevance = clientFieldData.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c.Name) && c.Name.Contains("programRelevance"));
            if (programRelevance != null)
            {
                summaryViewModel.ProgramRelevance = programRelevance.Value;
            }

            var effectivenessNote = clientFieldData.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c.Name) && c.Name.Contains("effectivenessNote"));
            if (effectivenessNote != null)
            {
                summaryViewModel.EffectivenessNote = effectivenessNote.Value;
            }

            var comments = clientFieldData.Where(c => !string.IsNullOrWhiteSpace(c.Name) && c.Name.Contains("txtFieldComment")).ToList();

            if (comments.Any())
            {
                foreach (var comment in comments)
                {
                    var commentId = int.Parse(comment.ExtraData.FirstOrDefault(x => x.Key == "data-persist-id").Value);
                    if (commentId != 0)
                    {
                        var viewModel = summaryViewModel.PCRUserCommentList.First(x => x.CommentId == commentId);

                        //Update comment
                        if (viewModel.Comment != comment.Value)
                        {
                            viewModel.Comment = comment.Value;
                            viewModel.Date = DateTime.Now;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(comment.Value))
                    {
                        //Add new comment
                        var newComment = new PCRUserComment
                        {
                            Comment = comment.Value,
                            Date = DateTime.Now,
                            PCRStage = summaryViewModel.PCRCurrentStageType,
                            IDBObjetivesType = (PCRCommentFieldTypeEnum)Enum.Parse(typeof(PCRCommentFieldTypeEnum), comment.Id)
                        };

                        summaryViewModel.PCRUserCommentList.Add(newComment);
                    }
                }
            }
        }

        public static void UpdatePCRGeneralViewModel(this PCRGeneralViewModel generalsViewModel, ClientFieldData[] clientFieldData)
        {
            // set default value to general non core (last stage value or "N/A"), later if have value change it.
            SetDefaultValuesToGeneralNonCore(generalsViewModel);

            foreach (var generalFields in clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Id)).GroupBy(x => x.Id))
            {
                var scoreFieldId = Convert.ToInt32(generalFields.Key);

                var scoreField = generalFields.FirstOrDefault(x => x.Name.Contains("cboScore"));

                decimal? scoreValue = null;
                if (scoreField != null)
                {
                    scoreValue = !string.IsNullOrWhiteSpace(scoreField.Value) ? (decimal?)Convert.ToDecimal(scoreField.Value) : null;
                }

                var commentField = generalFields.FirstOrDefault(x => x.Name.Contains("txtCoreCriteriaComment") || x.Name.Contains("txtNonCoreCriteriaComment"));
                var general = generalsViewModel.PCRGeneralViewModelList.First(x => x.PCRGeneralId == scoreFieldId);

                switch (generalsViewModel.CurrentStageType)
                {
                    case PCRStageTypeEnum.TeamLeaderPreValidationStage:
                        general.ScoreBasedOnTeamLeaderPreValidation = scoreValue;
                        break;
                    case PCRStageTypeEnum.SPDPreValidationStage:
                        general.ScoreBasedOnSPDPreValidation = scoreValue;
                        break;
                    case PCRStageTypeEnum.TeamLeaderValidationStage:
                        general.ScoreBasedOnTeamLeaderValidation = scoreValue;
                        break;
                    case PCRStageTypeEnum.SPDValidationStage:
                        general.ScoreBasedOnSPDValidation = scoreValue;
                        break;
                }

                if (commentField != null)
                {
                    var commentId = int.Parse(commentField.ExtraData.FirstOrDefault(x => x.Key == "data-persist-id").Value);
                    if (commentId != 0)
                    {
                        var viewModel = general.CommentList.First(x => x.CommentId == commentId);

                        //Update comment
                        if (viewModel.Comment != commentField.Value)
                        {
                            viewModel.Comment = commentField.Value;
                            viewModel.Date = DateTime.Now;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(commentField.Value))
                    {
                        //Add new comment
                        var newComment = new GeneralUserComment
                        {
                            Comment = commentField.Value,
                            Date = DateTime.Now,
                            PCRGeneralStageType = generalsViewModel.CurrentStageType
                        };

                        general.CommentList.Add(newComment);
                    }
                }
            }
        }

        public static void UpdateEffectivenessGuidelinesViewModel(this EffectivenessGuidelinesViewModel effectivenessGuidelineViewModel, ClientFieldData[] clientFieldData)
        {
            var spd = clientFieldData.First(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("txtSPDGuidelines"));
            var specialist = clientFieldData.First(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("txtSectorGuidelines"));

            effectivenessGuidelineViewModel.EffectivenessGuidelinesSectorSpd = spd.Value;
            effectivenessGuidelineViewModel.EffectivenessGuidelinesSectorSpecialist = specialist.Value;
        }

        public static void UpdateGeneralGuidelinesViewModel(this GeneralGuidelinesViewModel effectivenessGuidelineViewModel, ClientFieldData[] clientFieldData)
        {
            var core = clientFieldData.First(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("txtCoreGuidelineField"));
            var nonCore = clientFieldData.First(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("txtNonCoreGuidelineField"));

            effectivenessGuidelineViewModel.GeneralGuidelinesCore = core.Value;
            effectivenessGuidelineViewModel.GeneralGuidelinesNonCore = nonCore.Value;
        }
        #endregion

        #region PCR-2
        public static void UpdateFollowUp(
            this PCRFollowUpViewModel pcrFollowUpViewModel,
            ClientFieldData[] clientFieldData,
            int currentTask)
        {
            //Tasks
            var completedDate = clientFieldData.FirstOrDefault(
                x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("txtCompletedDate"));

            completedDate = GetDate(completedDate);

            var editPCRCorrd = clientFieldData.FirstOrDefault(
                x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("editpcrcoord"));

            var chkActive = clientFieldData.Where(
                x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("chkActive"));
            var cultureInfo = new CultureInfo("en-US");

            if (completedDate == null || string.IsNullOrEmpty(completedDate.Value)) 
            {
                if (pcrFollowUpViewModel.Tasks.Any(t => t.IsActive && (t.TaskNumber == 8 || t.TaskNumber == 2)))
                {
                    pcrFollowUpViewModel.Tasks.First(x => x.TaskNumber == currentTask).IsCurrent = false;
                    if (currentTask == 7 || currentTask == 1)
                        currentTask++;
                    if (currentTask == 3)
                        currentTask--;
                    pcrFollowUpViewModel.Header.CurrentTask = currentTask;
                    pcrFollowUpViewModel.Tasks.First(x => x.TaskNumber == currentTask).IsCurrent = true;
                }
            }

            var documentIndex = 0;
            switch (currentTask)
            {
                case 2:
                case 8:
                    var taskExtension = (PCRFollowUpTaskExtensionViewModel)pcrFollowUpViewModel.Tasks.First(x => x.TaskNumber == currentTask);

                    var approvedBy = clientFieldData.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("approvedBy"));
                    var extensionReason = clientFieldData.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("txtExtensionReason"));
                    var updatedDeadLine = clientFieldData.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("txtUpdatedDeadLineDate"));
                    updatedDeadLine = GetDate(updatedDeadLine);

                    if (approvedBy != null && !string.IsNullOrWhiteSpace(approvedBy.Value))
                    {
                        taskExtension.ApprovedBy = approvedBy.Value;
                    }

                    if (extensionReason != null && !string.IsNullOrWhiteSpace(extensionReason.Value))
                    {
                        taskExtension.Reason = extensionReason.Value;
                    }

                    if (updatedDeadLine != null && !string.IsNullOrWhiteSpace(updatedDeadLine.Value))
                    {
                        taskExtension.UpdatedDeadline = DateTime.ParseExact(updatedDeadLine.Value, Format, cultureInfo);
                    }

                    break;
                case 12:
                    var task12 = (PCRFollowUpTask12ViewModel)pcrFollowUpViewModel.Tasks.First(x => x.TaskNumber == currentTask);

                    var isCommentSent = clientFieldData.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("isCommentSent"));
                    var isWorkshopConducted = clientFieldData.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("isWorkshopConducted"));
                    var workshopDate = clientFieldData.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("txtWorkshopDate"));

                    if (isCommentSent != null && !string.IsNullOrWhiteSpace(isCommentSent.Value))
                    {
                        task12.IsCommentSent = isCommentSent.Value == "Yes";
                    }

                    if (isWorkshopConducted != null && !string.IsNullOrWhiteSpace(isWorkshopConducted.Value))
                    {
                        task12.IsWorkshopConducted = isWorkshopConducted.Value == "Yes";
                    }

                    if (workshopDate != null && !string.IsNullOrWhiteSpace(workshopDate.Value))
                    {
                        task12.WorkshopDate = DateTime.ParseExact(GetDate(workshopDate).Value, Format, cultureInfo);                        
                    }

                    if (completedDate != null && !string.IsNullOrWhiteSpace(completedDate.Value))
                    {
                        task12.CompletedDate = DateTime.ParseExact(completedDate.Value, Format, cultureInfo);
                    }

                    //Deleted Tasks 13, 15, 19 y 20
                    if (task12.IsCommentSent.HasValue && !task12.IsCommentSent.Value)
                    {
                        var task13 = (PCRFollowUpTaskBaseViewModel)pcrFollowUpViewModel.Tasks.FirstOrDefault(x => x.TaskNumber == 13);
                        if (task13 != null)
                        {
                            task13.IsDeleted = true;
                        }

                        var task15 = (PCRFollowUpTaskBaseViewModel)pcrFollowUpViewModel.Tasks.FirstOrDefault(x => x.TaskNumber == 15);
                        if (task15 != null)
                        {
                            task15.IsDeleted = true;
                        }

                        var task19 = (PCRFollowUpTaskBaseViewModel)pcrFollowUpViewModel.Tasks.FirstOrDefault(x => x.TaskNumber == 19);
                        if (task19 != null)
                        {
                            task19.IsDeleted = true;
                        }

                        var task20n = (PCRFollowUpTaskBaseViewModel)pcrFollowUpViewModel.Tasks.FirstOrDefault(x => x.TaskNumber == 20);
                        if (task20n != null)
                        {
                            task20n.IsDeleted = true;
                        }
                    }

                    SetAllTasksCurrentFieldToFalse(pcrFollowUpViewModel);
                    task12.IsCurrent = true;

                    break;
                case 20:
                    var task20 = (PCRFollowUpTask20ViewModel)pcrFollowUpViewModel.Tasks.First(x => x.TaskNumber == currentTask);

                    var isDisclosure = clientFieldData.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("isDisclosure"));

                    if (isDisclosure != null && !string.IsNullOrWhiteSpace(isDisclosure.Value))
                    {
                        task20.IsDisclosure = isDisclosure.Value == "Yes";
                    }

                    if (completedDate != null && !string.IsNullOrWhiteSpace(completedDate.Value))
                    {
                        task20.CompletedDate = DateTime.ParseExact(completedDate.Value, Format, cultureInfo);
                    }

                    //Deleted Task 21
                    if (task20.IsDisclosure.HasValue && !task20.IsDisclosure.Value)
                    {
                        var task21 = (PCRFollowUpTaskBaseViewModel)pcrFollowUpViewModel.Tasks.FirstOrDefault(x => x.TaskNumber == 21);
                        if (task21 != null)
                        {
                            task21.IsDeleted = true;
                        }
                    }

                    SetAllTasksCurrentFieldToFalse(pcrFollowUpViewModel);
                    task20.IsCurrent = true;

                    break;
                default:
                    SetAllTasksCurrentFieldToFalse(pcrFollowUpViewModel);

                    var taskChange = pcrFollowUpViewModel.Tasks.Where(x => x.IsCurrent);
                    foreach (var taskTemp in taskChange)
                    {
                        taskTemp.IsCurrent = false;
                    }

                    var task = pcrFollowUpViewModel.Tasks.First(x => x.TaskNumber == currentTask);
                    task.IsCurrent = true;

                    if (completedDate != null && !string.IsNullOrWhiteSpace(completedDate.Value))
                    {
                        if (completedDate.Value != string.Empty)
                        {
                            task.CompletedDate = DateTime.ParseExact(completedDate.Value, Format, cultureInfo);
                        }
                    }

                    break;
            }

            if (editPCRCorrd != null)
            {
                var taskTemp = pcrFollowUpViewModel.Tasks.First(x => x.TaskNumber == currentTask);
                taskTemp.IsEditPcrCoord = true;
            }

            //Add Tasks 2 or 8            
            foreach (var taskCheck in chkActive.Where(task => task.Value == "True").Select(task => (PCRFollowUpTaskExtensionViewModel)pcrFollowUpViewModel.Tasks.First(x => x.TaskNumber == int.Parse(task.Id))).Where(taskCheck => !taskCheck.IsActive))
            {
                taskCheck.IsNew = true;
            }

            //Deleted Tasks 2 or 8
            foreach (var taskCheck in chkActive.Where(task => task.Value == "False").Select(task => (PCRFollowUpTaskExtensionViewModel)pcrFollowUpViewModel.Tasks.First(x => x.TaskNumber == int.Parse(task.Id))).Where(taskCheck => !taskCheck.IsNew && taskCheck.IsActive))
            {
                taskCheck.IsDeleted = true;
            }

            //Workflow Status
            var isRequired = clientFieldData.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("isRequired"));
            if (isRequired != null && !string.IsNullOrWhiteSpace(isRequired.Value))
            {
                pcrFollowUpViewModel.IsRequired = isRequired.Value == "Yes";
            }

            if (isRequired != null && isRequired.Value == "No" && currentTask == 1)
            {
                var commentNotRequired = clientFieldData.FirstOrDefault(
                x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("txtNotRequiredReason"));

                if (pcrFollowUpViewModel.NotRequiredComment == null)
                {
                    pcrFollowUpViewModel.NotRequiredComment = new PCRNotRequiredUserComment
                    {
                        Comment = commentNotRequired.Value.ToString(),
                        CommentId = 0,
                        OperationId = pcrFollowUpViewModel.OperationId
                    };
                }
                else
                {
                    pcrFollowUpViewModel.NotRequiredComment.Comment = commentNotRequired.Value.ToString();
                }
            }

            var seriesCode = clientFieldData.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("seriesCode"));
            if (seriesCode != null && !string.IsNullOrWhiteSpace(seriesCode.Value))
            {
                pcrFollowUpViewModel.SeriesCode = seriesCode.Value;
            }

            var isLastInSeries = clientFieldData.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("isLastInSeries"));
            if (isLastInSeries != null && !string.IsNullOrWhiteSpace(isLastInSeries.Value))
            {
                pcrFollowUpViewModel.IsLastInSeries = isLastInSeries.Value == "Yes";
            }

            //Add, Remove and Update Documents
            var documentDescription = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("documentDescription"));

            var documentNumber = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("documentNumber"));

            var documentsToDelete = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("deleteDocument"));

            foreach (var document in documentDescription)
            {
                if (document.Value == "Select your option")
                {
                    document.Value = null;
                }

                if (document.Id != string.Empty)
                {
                    var documentOld = pcrFollowUpViewModel.Documents.FirstOrDefault(x => x.DocumentId == int.Parse(document.Id));
                    if (documentOld != null)
                    {
                        documentOld.Description = document.Value;
                    }

                    var documentToDelete = documentsToDelete.FirstOrDefault(x => x.Id == document.Id);

                    if (documentToDelete != null)
                    {
                        if (documentToDelete.Value == "yes")
                        {
                            pcrFollowUpViewModel.Documents.Remove(documentOld);
                        }
                    }
                }
                else if (document.Id == string.Empty)
                {
                    var documentNumberValue = documentNumber.ToArray()[documentIndex].Value;

                    pcrFollowUpViewModel.Documents.Add(
                        new PCRDocumentViewModel
                        {
                            Description = document.Value,
                            DocumentNumber = documentNumberValue
                        });

                    documentIndex++;
                }
            }
        }
        #endregion

        #region K2
        public static void UpdateWorkflowTask(
            this PCRWorkflowTaskViewModel taskViewModel, ClientFieldData[] clientFieldData)
        {
            var comments = clientFieldData.Where(
                c => !string.IsNullOrWhiteSpace(c.Name) && c.Name.Contains("txtFieldComment")).ToList();

            if (comments.Any())
            {
                foreach (var comment in comments)
                {
                    var commentId = int.Parse(comment.ExtraData.FirstOrDefault(
                        x => x.Key == "data-persist-id").Value);
                    if (commentId != 0)
                    {
                        var viewModel = taskViewModel.UserComments.First(x => x.CommentId == commentId);

                        //Update comment
                        if (viewModel.Comment != comment.Value)
                        {
                            viewModel.Comment = comment.Value;
                            viewModel.IsChange = true;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(comment.Value))
                    {
                        //Add new comment
                        var newComment = new UserCommentViewModel
                        {
                            Comment = comment.Value,
                            WorkflowTaskId = taskViewModel.WorkflowInstanceTaskId,
                            IsChange = true
                        };
                        taskViewModel.CommentReject = newComment.Comment;
                        taskViewModel.UserComments.Add(newComment);
                    }
                }
            }
        }
        #endregion

        #region Private Methods
        private static List<int> GetValues(ClientFieldData fieldValue)
        {
            var values = new List<int>();
            if (fieldValue != null && !string.IsNullOrWhiteSpace(fieldValue.Value))
            {
                values.AddRange(fieldValue.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(item => Convert.ToInt32(item)));
            }

            return values;
        }

        private static void SetDefaultValuesToGeneralNonCore(PCRGeneralViewModel generalsViewModel)
        {
            foreach (var general in generalsViewModel.PCRGeneralViewModelList)
            {
                switch (generalsViewModel.CurrentStageType)
                {
                    case PCRStageTypeEnum.TeamLeaderPreValidationStage:
                        if (!general.IsCore)
                        {
                            general.ScoreBasedOnTeamLeaderPreValidation = 0;
                            if (general.PCRGeneralType == IDB.MW.Domain.Values.PCRGeneralTypeEnum.EnvironmentalAndSocialSafeguards)
                            {
                                general.ScoreBasedOnTeamLeaderPreValidation = null;
                            }
                        }

                        break;
                    case PCRStageTypeEnum.SPDPreValidationStage:
                        if (!general.IsCore)
                        {
                            general.ScoreBasedOnSPDPreValidation = general.ScoreBasedOnTeamLeaderPreValidation;
                            if (general.PCRGeneralType == IDB.MW.Domain.Values.PCRGeneralTypeEnum.EnvironmentalAndSocialSafeguards)
                            {
                                general.ScoreBasedOnSPDPreValidation = null;
                            }
                        }

                        break;
                    case PCRStageTypeEnum.TeamLeaderValidationStage:
                        if (!general.IsCore)
                        {
                            general.ScoreBasedOnTeamLeaderValidation = general.ScoreBasedOnSPDPreValidation;
                            if (general.PCRGeneralType == IDB.MW.Domain.Values.PCRGeneralTypeEnum.EnvironmentalAndSocialSafeguards)
                            {
                                general.ScoreBasedOnTeamLeaderValidation = null;
                            }
                        }

                        break;
                    default:
                        if (!general.IsCore)
                        {
                            general.ScoreBasedOnSPDValidation = general.ScoreBasedOnTeamLeaderValidation;
                            if (general.PCRGeneralType == IDB.MW.Domain.Values.PCRGeneralTypeEnum.EnvironmentalAndSocialSafeguards)
                            {
                                general.ScoreBasedOnSPDValidation = null;
                            }
                        }

                        break;
                }
            }
        }

        private static ClientFieldData GetDate(ClientFieldData date)
        {
            if (date != null)
            {
                if (date.Value != string.Empty && !date.Value.Contains("/"))
                {
                    date.Value = Convert.ToDateTime(date.Value).ToString("dd/MM/yyyy");
                }
            }

            return date;
        }

        private static void SetAllTasksCurrentFieldToFalse(PCRFollowUpViewModel pcrFollowUpViewModel)
        {
            var taskChange = pcrFollowUpViewModel.Tasks.Where(x => x.IsCurrent);
            foreach (var taskTemp in taskChange)
            {
                taskTemp.IsCurrent = false;
            }
        }
        #endregion
    }
}