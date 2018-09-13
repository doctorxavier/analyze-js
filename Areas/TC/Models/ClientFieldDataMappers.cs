using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using IDB.Architecture.Extensions;
using IDB.Architecture.Logging;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.TCAbstractModule.Enums;
using IDB.MW.Application.TCAbstractModule.ViewModels.AwardFundEligibility;
using IDB.MW.Application.TCAbstractModule.ViewModels.DonorDecision;
using IDB.MW.Application.TCAbstractModule.ViewModels.EligibilityDecision;
using IDB.MW.Application.TCAbstractModule.ViewModels.ESCDecision;
using IDB.MW.Application.TCAbstractModule.ViewModels.FundInformation;
using IDB.MW.Application.TCAbstractModule.ViewModels.NotifyFunding;
using IDB.MW.Application.TCAbstractModule.ViewModels.RequestIncrease;
using IDB.MW.Application.TCAbstractModule.ViewModels.ReviewFundCoordination;
using IDB.MW.Application.TCAbstractModule.ViewModels.ReviewRegionalTL;
using IDB.MW.Application.TCAbstractModule.ViewModels.ReviewTCAbstract;
using IDB.MW.Application.TCAbstractModule.ViewModels.ReviewTCAbstractPostValidation;
using IDB.MW.Application.TCAbstractModule.ViewModels.SingleWindowOperationDecision;
using IDB.MW.Application.TCAbstractModule.ViewModels.TCAbstractService;
using IDB.MW.Application.TCAbstractModule.ViewModels.ValidationTCAbstract;
using IDB.MW.Infrastructure.Configuration;
using IDB.Presentation.MVC4.Areas.TC.Enums;
using IDB.Presentation.MVC4.Areas.TC.Values;
using IDB.Presentation.MVC4.Models.ClientFieldData;

namespace IDB.Presentation.MVC4.Areas.TC.Models
{
    public static class ClientFieldDataMappers
    {
        #region Constants

        public const string AWARD_ELIGIBILITY_SUBFUND_CHECK = "awardEligibilitySubFundCheck";
        public static readonly string DATETIME_PARSE_FORMAT;
        const string CHECKBOX_VALUE_FALSE = "False";
        const string CHECKBOX_VALUE_TRUE = "True";
        const string IGR_OPERATION = "IGR";
        const string DATA_PERSIST_OLD_KEY = "data-persist-old-id";
        const string DATA_PERSIST_NEW_KEY = "data-persist-new-id";
        const string FUND_COORD_APPICABLEFUND = "fund-coord-applicableFund";
        const string COUNTRY_DEPARTMENT_FIELD_KEY = "countryDepartment";
        const string REGIONAL_TEAM_LEADER_FIELD_KEY = "regionalTeamLeader";
        const string COUNTRY_DEPARTMENT_RG_OPERATION_FIELD_KEY = "countryDepartment-rgOperation";
        const string REGIONAL_TEAM_LEADER_RG_OPERATION_FIELD_KEY = "regionalTeamLeader-rgOperation";
        const string APPLICABLE_FUND = "applicableFund";
        const string FUND_DECISION = "fundDecision";
        const string SELECTED = "selected";
        const string YES_VALUE = "yes";

        #endregion

        #region Constructors

        static ClientFieldDataMappers()
        {
            DATETIME_PARSE_FORMAT = ConfigurationServiceFactory.Current.GetApplicationSettings()
                .FormatDate;
        }

        #endregion

        #region Mappers

        public static void UpdateTCAbstractViewModel(
            this TCAbstractViewModel model,
            ClientFieldData[] requestClient)
        {
            model.BasicData.UpdateTCBasicData(requestClient);

            //Results Matrix
            var outcomesIds = (from outcome in model.ResultsMatrix.Outcomes
                               select outcome.OutcomeId).ToList();
            var outcomes = requestClient.Where(x => x.Name == "outcomes");
            if (outcomes != null)
            {
                foreach (var outcome in outcomes)
                {
                    var firstKey = outcome.ExtraData.FirstOrDefault(x => x.Key.Contains("data-persist"));
                    var keyOutcome = firstKey.Key;
                    if (keyOutcome == DATA_PERSIST_OLD_KEY)
                    {
                        var outcomeOld = model.ResultsMatrix.Outcomes.FirstOrDefault(x =>
                            x.OutcomeId == Convert.ToInt32(firstKey.Value));
                        outcomeOld.Description = outcome.Value;
                        outcomesIds.Remove(Convert.ToInt32(firstKey.Value));
                    }
                    else
                    {
                        model.ResultsMatrix.Outcomes.Add(
                            new TCOutcomeViewModel
                            {
                                Description = outcome.Value
                            });
                    }
                }
            }

            //Delete Outcomes
            if (outcomesIds.Any())
            {
                foreach (var outcome in outcomesIds)
                {
                    var outcomeDelete = model.ResultsMatrix.Outcomes.FirstOrDefault(x =>
                        x.OutcomeId == outcome);
                    if (outcomeDelete != null)
                    {
                        model.ResultsMatrix.Outcomes.Remove(outcomeDelete);
                    }
                }
            }

            //Financing New
            var financingComponentsNew = requestClient.Where(c =>
                !string.IsNullOrWhiteSpace(c.Name) && c.Name == "financingComponent-new");
            var counterpartComponentsNew = requestClient.Where(c =>
                !string.IsNullOrWhiteSpace(c.Name) && c.Name == "counterpartFinancingComponent-new");

            var componentsIds = (from component in model.ResultsMatrix.Components
                                 select component.ComponentId).ToList();
            var componentsNames = requestClient.Where(x => x.Name == "componentName");
            if (componentsNames != null)
            {
                foreach (var componentName in componentsNames)
                {
                    var firstKey = componentName.ExtraData.FirstOrDefault(x =>
                        x.Key == DATA_PERSIST_OLD_KEY || x.Key == DATA_PERSIST_NEW_KEY);
                    var keyComponentName = firstKey.Key;
                    var componentDescription = requestClient.FirstOrDefault(x =>
                        x.Name == "componentDescription" && x.ExtraData.Any(y =>
                            (y.Value == firstKey.Value) && (y.Key == keyComponentName)));

                    if (keyComponentName == DATA_PERSIST_OLD_KEY)
                    {
                        var componentOld = model.ResultsMatrix.Components.FirstOrDefault(x =>
                            x.ComponentId == Convert.ToInt32(firstKey.Value));
                        if (componentOld != null)
                        {
                            componentOld.ComponentName = componentName.Value;
                            componentOld.ComponentDescription = componentDescription.Value;
                            componentsIds.Remove(Convert.ToInt32(firstKey.Value));

                            //Obtain Outputs
                            var output = requestClient.Where(x => x.Name == "output" &&
                                x.ExtraData.Any(y => (y.Value == firstKey.Value) &&
                                    (y.Key == "data-persist-old-parent-id")));
                            UpdateOutputs(output, componentOld.Outputs, componentOld);
                        }
                    }
                    else
                    {
                        //Obtain Outputs
                        var output = requestClient.Where(x => x.Name == "output");
                        output = output.Where(x => x.ExtraData.Any(y => (y.Value == firstKey.Value) &&
                            (y.Key == "data-persist-new-parent-id")));
                        model.ResultsMatrix.Components.Add(
                            new ComponentViewModel
                            {
                                ComponentName = componentName.Value,
                                ComponentDescription = componentDescription.Value,
                                Outputs = output != null ? (from outp in output
                                                            select new TCOutputViewModel
                                                            {
                                                                Description = outp.Value
                                                            }).ToList() : null
                            });

                        model.Financing.Components.Add(
                                new ComponentFinancingViewModel
                                {
                                    ComponentName = componentName.Value,
                                    Financing = financingComponentsNew != null ?
                                        (from fin in financingComponentsNew
                                         where fin.Id == firstKey.Value
                                         select Convert.ToDecimal(fin.Value)).First() :
                                         0M,
                                    CounterpartFinancing = counterpartComponentsNew != null ?
                                            (from counter in counterpartComponentsNew
                                             where counter.Id == firstKey.Value
                                             select Convert.ToDecimal(counter.Value)).First() : 0M,
                                });
                    }
                }
            }

            //Delete Components
            if (componentsIds.Any())
            {
                foreach (var component in componentsIds)
                {
                    var componentDelete = model.ResultsMatrix.Components.FirstOrDefault(x =>
                        x.ComponentId == component);
                    if (componentDelete != null)
                    {
                        model.ResultsMatrix.Components.Remove(componentDelete);
                    }
                }
            }

            //Financing Old
            var financingComponents = requestClient.Where(c =>
                !string.IsNullOrWhiteSpace(c.Name) && c.Name == "financingComponent");
            var counterpartComponents = requestClient.Where(c =>
                !string.IsNullOrWhiteSpace(c.Name) && c.Name == "counterpartFinancingComponent");
            if (financingComponents != null && counterpartComponents != null)
            {
                foreach (var financingComponent in financingComponents)
                {
                    var financingOld = model.Financing.Components.FirstOrDefault(x =>
                        x.ComponentId == Convert.ToInt32(financingComponent.Id));
                    var conterpartComponent = counterpartComponents.FirstOrDefault(x =>
                        x.Id == financingComponent.Id).Value;
                    if (financingOld != null)
                    {
                        financingOld.Financing = Convert.ToDecimal(financingComponent.Value);
                        financingOld.CounterpartFinancing = Convert.ToDecimal(conterpartComponent);
                    }
                }
            }

            //Risks
            var descriptionRisk = requestClient.FirstOrDefault(c =>
                !string.IsNullOrWhiteSpace(c.Name) && c.Name.Contains("descriptionRisk"));
            model.Risks = null;
            if (descriptionRisk != null)
            {
                model.Risks = descriptionRisk.Value;
            }

            UpdateComments(model.Comments, CommentTypeEnum.TCAbstract, requestClient);

            var isSustainability = requestClient.FirstOrDefault(x =>
                x.Name.Contains("sustainabilityType"));
            if (isSustainability != null)
            {
                model.IsSustainable = Convert.ToBoolean(isSustainability.Value);
            }

            var sustainabilityExplain = requestClient.FirstOrDefault(x =>
                x.Name == "sustainabilityExplain");
            if (sustainabilityExplain != null)
            {
                model.DescriptionSustainability =
                    model.IsSustainable ?? false ? sustainabilityExplain.Value : null;
            }

            var isLessonLearned = requestClient.FirstOrDefault(x =>
                x.Name.Contains("lessonLearnedType"));
            if (isLessonLearned != null)
            {
                model.IsLessonLearned = Convert.ToBoolean(isLessonLearned.Value);
            }

            var lessonLearnedExplain = requestClient.FirstOrDefault(x =>
                x.Name == "lessonLearnedExplain");
            if (lessonLearnedExplain != null)
            {
                model.DescriptionLesson =
                    model.IsLessonLearned ?? false ? lessonLearnedExplain.Value : null;
            }
        }

        public static void UpdateValidationTCAbstractViewModel(
            this ValidationTCAbstractViewModel viewModel,
            ClientFieldData[] clientFieldData,
            IEnumMappingService enumMappingService)
        {
            var validationType = clientFieldData.FirstOrDefault(x => x.Name.Contains("validate"));
            if (validationType != null)
            {
                viewModel.Type = enumMappingService.GetMappedEnum<ValidationTypeEnum>(int.Parse(validationType.Value));
            }

            UpdateComments(viewModel.CommentsInternal, CommentTypeEnum.TCAbstractInternal, clientFieldData);
            if (viewModel.Type == ValidationTypeEnum.ReturnTeamLeader)
            {
                UpdateComments(viewModel.CommentsTeamLeader, CommentTypeEnum.TCAbstractTeamLeader, clientFieldData);
            }
        }

        public static void UpdatePriorityTCAbstractViewModel(
            this PriorityTCAbstractViewModel viewModel,
            ClientFieldData[] clientFieldData,
            IEnumMappingService enumMappingService)
        {
            var priorityType = clientFieldData.FirstOrDefault(x => x.Name == "priority");
            if (priorityType != null)
            {
                viewModel.Priority = enumMappingService
                    .GetMappedEnum<CountryPriorityTCAbstractEnum>(int.Parse(priorityType.Value));
            }

            UpdateComments(viewModel.Comments, CommentTypeEnum.TCRegionalTeamLeaderReview, clientFieldData);
        }

        public static void UpdateDecisionSWOpertationViewModel(
            this DecisionSWOperationViewModel viewModel,
            ClientFieldData[] clientFieldData,
            IEnumMappingService enumMappingService)
        {
            var operationType = clientFieldData.FirstOrDefault(x => x.Name == "operationType");
            if (operationType != null)
            {
                viewModel.OperationType = enumMappingService.GetMappedEnum<OperationTypeEnum>(int.Parse(operationType.Value));
            }

            if (viewModel.OperationType == OperationTypeEnum.FundingGap)
            {
                var decision = clientFieldData.FirstOrDefault(x => x.Name == "decision");
                if (decision != null)
                {
                    viewModel.Decision = enumMappingService.GetMappedEnum<DecisionOperationTypeEnum>(int.Parse(decision.Value));
                }

                var dateMeeting = clientFieldData.FirstOrDefault(x => x.Name == "dateMeeting");
                if (dateMeeting != null)
                {
                    viewModel.DateMeeting = DateTime.ParseExact(dateMeeting.Value, DATETIME_PARSE_FORMAT, CultureInfo.InvariantCulture);
                }
            }
            else if (viewModel.OperationType == OperationTypeEnum.Flex)
            {
                var decisionFlex = clientFieldData.FirstOrDefault(x => x.Name == "decisionFlex");
                if (decisionFlex != null)
                {
                    viewModel.Decision = enumMappingService.GetMappedEnum<DecisionOperationTypeEnum>(int.Parse(decisionFlex.Value));
                }
            }

            UpdateComments(viewModel.Comments, CommentTypeEnum.TCSingleWindowOperationsDecision, clientFieldData);
        }

        public static void UpdateValidationTCAbstractViewModel(
            this ReviewTCAbstractViewModel viewModel,
            ClientFieldData[] clientFieldData)
        {
            viewModel.TCAbstract.UpdateTCAbstractViewModel(clientFieldData);
        }

        public static void UpdateReviewTCAbstractPostValidationViewModel(
            this ReviewTCAbstractPostValidationViewModel viewModel,
            ClientFieldData[] clientFieldData)
        {
            viewModel.TCAbstract.UpdateTCAbstractViewModel(clientFieldData);
        }

        public static void UpdateFundCoordinatorTCAbstractViewModel(
            this FundCoordinatorTCAbstractViewModel viewModel,
            ClientFieldData[] clientFieldData,
            IEnumMappingService enumMappingService)
        {
            var validationType = clientFieldData.FirstOrDefault(x => x.Name.Equals("eligible"));
            if (validationType != null)
            {
                viewModel.FundCoordinatorDecision = (FundCoordinatorDecisionEnum)int.Parse(validationType.Value);

                switch (viewModel.FundCoordinatorDecision)
                {
                    case FundCoordinatorDecisionEnum.NotApplicable:
                        break;
                    case FundCoordinatorDecisionEnum.Elegible:
                        var typeEligibility = clientFieldData.FirstOrDefault(x => x.Name.Equals("typeEligibility"));
                        if (typeEligibility != null)
                        {
                            viewModel.TypeEligibility = Convert.ToInt32(typeEligibility.Value);
                        }

                        var changesNeeded = clientFieldData.FirstOrDefault(x => x.Name.Equals("changesNeeded"));
                        viewModel.ChangesNeeded = changesNeeded != null ? changesNeeded.Value : null;
                        break;
                    case FundCoordinatorDecisionEnum.NotElegible:
                        var causeOfNoFunding = clientFieldData.FirstOrDefault(x => x.Name.Equals("causeOfNoFunding"));
                        if (causeOfNoFunding != null)
                        {
                            viewModel.CauseOfNoFunding = Convert.ToInt32(causeOfNoFunding.Value);
                        }

                        var justification = clientFieldData.FirstOrDefault(x => x.Name.Equals("justificationTextArea"));
                        viewModel.Justification = justification != null ? justification.Value : null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("No se eligió una opción válida del enumerado " + typeof(FundCoordinatorDecisionEnum).Name);
                }
            }

            UpdateComments(viewModel.Comments, CommentTypeEnum.TCFundCordinatorReview, clientFieldData);
        }

        public static void UpdateTCReviewNotifyFCViewModel(
            this TCReviewNotifyFCViewModel viewModel,
            ClientFieldData[] clientFieldData)
        {
            viewModel.TCNotifyFundCoordinator = new List<TCNotifyFCFundCoordinatorViewModel>();

            var applicableFunds = clientFieldData.Where(x => x.Name.StartsWith("fund-coord-"));

            if (applicableFunds == null || !applicableFunds.Any())
                return;

            var oldItems = applicableFunds
                    .Where(x => x.ExtraData.Any(y => y.Key == DATA_PERSIST_OLD_KEY));
            var newItems = applicableFunds
                .Where(x => x.ExtraData.Any(y => y.Key == DATA_PERSIST_NEW_KEY));
            var oldItemsGroup = oldItems.GroupBy(x =>
                x.ExtraData.FirstOrDefault(y => y.Key == DATA_PERSIST_OLD_KEY));
            var newItemsGroup = newItems.GroupBy(x =>
                x.ExtraData.FirstOrDefault(y => y.Key == DATA_PERSIST_NEW_KEY));

            if (oldItemsGroup != null)
            {
                foreach (var oldItem in oldItemsGroup)
                {
                    AddFundCoordinatorToModel(viewModel, oldItem);
                }
            }

            if (newItemsGroup == null)
                return;

            foreach (var newItem in newItemsGroup)
            {
                AddFundCoordinatorToModel(viewModel, newItem);
            }
        }

        public static void UpdateTCNotifyFCViewModel(
            this TCNotifyFCViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            var dateMeeting = clientFieldData.FirstOrDefault(x => x.Name == "dateMeeting");
            if (dateMeeting != null)
            {
                viewModel.DateMeeting = DateTime.ParseExact(dateMeeting.Value, DATETIME_PARSE_FORMAT, CultureInfo.InvariantCulture);
            }

            if (viewModel.TCNotifyFCRegionalTeamLeaderViewModel == null)
            {
                viewModel.TCNotifyFCRegionalTeamLeaderViewModel = new List<TCNotifyFCRegionalTeamLeaderViewModel>();
            }

            if (viewModel.TCNotifyFCRegionalTeamLeaderViewModel == null)
                viewModel.TCNotifyFCRegionalTeamLeaderViewModel =
                    new List<TCNotifyFCRegionalTeamLeaderViewModel>();

            viewModel.TCNotifyFCRegionalTeamLeaderViewModel.Clear();

            if (clientFieldData.Any(x => x.Name == COUNTRY_DEPARTMENT_FIELD_KEY))
            {
                viewModel.TCNotifyFCRegionalTeamLeaderViewModel = FillRTLNonRegionalOperation(clientFieldData);
            }
            else if (clientFieldData.Any(x => x.Name == COUNTRY_DEPARTMENT_RG_OPERATION_FIELD_KEY))
            {
                viewModel.TCNotifyFCRegionalTeamLeaderViewModel = FillRTLRegionalOperation(clientFieldData);
            }

            var singleWindowMeetingVisibilityCheckbox = clientFieldData
                .FirstOrDefault(x => x.Name == "singleWindowMeetingVisibilityCheckbox");
            if (singleWindowMeetingVisibilityCheckbox != null &&
                !string.IsNullOrWhiteSpace(singleWindowMeetingVisibilityCheckbox.Value))
            {
                bool isDisplayed;
                if (bool.TryParse(singleWindowMeetingVisibilityCheckbox.Value, out isDisplayed))
                {
                    viewModel.IsDisplayedSingleWindowMeeting = isDisplayed;
                }
            }

            if (viewModel.FundCoordinatorSelection == null)
            {
                viewModel.FundCoordinatorSelection = new List<TCNotifyFCFundCoordinatorViewModel>();
            }

            viewModel.FundCoordinatorSelection.Clear();

            var applicableFunds = clientFieldData.Where(x => x.Name.StartsWith("fund-coord-"));

            if (applicableFunds != null && applicableFunds.Any())
            {
                var oldItems = applicableFunds.Where(x => x.ExtraData.Any(y => y.Key == DATA_PERSIST_OLD_KEY));
                var newItems = applicableFunds.Where(x => x.ExtraData.Any(y => y.Key == DATA_PERSIST_NEW_KEY));

                var oldItemsGroup = oldItems.GroupBy(x => x.ExtraData.FirstOrDefault(y => y.Key == DATA_PERSIST_OLD_KEY));
                var newItemsGroup = newItems.GroupBy(x => x.ExtraData.FirstOrDefault(y => y.Key == DATA_PERSIST_NEW_KEY));

                if ((oldItemsGroup != null) && oldItemsGroup.Any())
                {
                    int id = 0;
                    foreach (var oldItem in oldItemsGroup)
                    {
                        var fundCoordinator = oldItem.FirstOrDefault(x => x.Name == FUND_COORD_APPICABLEFUND);
                        TCNotifyFCFundCoordinatorViewModel fundCoord = null;

                        if (fundCoordinator != null)
                        {
                            if (int.TryParse(fundCoordinator.Value, out id))
                            {
                                fundCoord = new TCNotifyFCFundCoordinatorViewModel()
                                {
                                    ApplicableFundId = id,
                                    TrustFundId = id,
                                };
                                viewModel.FundCoordinatorSelection.Add(fundCoord);
                            }
                        }
                    }
                }

                if ((newItemsGroup != null) && newItemsGroup.Any())
                {
                    int id = 0;
                    foreach (var newItem in newItemsGroup)
                    {
                        var fundCoordinator = newItem.FirstOrDefault(x => x.Name == FUND_COORD_APPICABLEFUND);
                        TCNotifyFCFundCoordinatorViewModel fundCoord = null;

                        if (fundCoordinator != null)
                        {
                            if (int.TryParse(fundCoordinator.Value, out id))
                            {
                                fundCoord = new TCNotifyFCFundCoordinatorViewModel()
                                {
                                    ApplicableFundId = id,
                                    TrustFundId = id,
                                };
                                viewModel.FundCoordinatorSelection.Add(fundCoord);
                            }
                        }
                    }
                }
            }
        }

        public static void UpdateSingleWindowDecisionViewModel(
            this SingleWindowDecisionViewModel viewModel,
            ClientFieldData[] clientFieldData,
            IEnumMappingService enumMappingService)
        {
            var operationType = clientFieldData.FirstOrDefault(x => x.Name == "typeDecision");
            if (operationType != null)
            {
                viewModel.Type = enumMappingService.GetMappedEnum<SingleWindowDecisionTypeEnum>(int.Parse(operationType.Value));
            }

            var withdrawalType = clientFieldData.FirstOrDefault(x => x.Name == "withdrawalType");
            if (withdrawalType != null)
            {
                viewModel.WithdrawOperation.WithdrawalType = enumMappingService.GetMappedEnum<WithdrawalTypeEnum>(int.Parse(withdrawalType.Value));
            }

            var withdrawalComment = clientFieldData.FirstOrDefault(x => x.Name == "additionalCommentWithdraw");
            if (withdrawalComment != null)
            {
                viewModel.WithdrawOperation.AdditionalComment = withdrawalComment.Value;
            }

            var reasonHoldType = clientFieldData.FirstOrDefault(x => x.Name == "reasonHold");
            if (reasonHoldType != null)
            {
                viewModel.PlaceHold.ReasonHoldOperation = enumMappingService.GetMappedEnum<ReasonHoldOperationEnum>(int.Parse(reasonHoldType.Value));
            }

            var additionalCommentPlaceHold = clientFieldData.FirstOrDefault(x => x.Name == "additionalCommentPlaceHold");
            if (additionalCommentPlaceHold != null)
            {
                viewModel.PlaceHold.Explanation = additionalCommentPlaceHold.Value;
            }

            viewModel.PreassignedFundCoordinatorList = UpdateSingleWindowDecisionFunds(viewModel.SingleWindowMeetingId, clientFieldData, enumMappingService);
        }

        public static void UpdateSingleWindowMeetingViewModel(
            this SingleWindowMeetingViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            if ((viewModel == null) || (clientFieldData == null) || !clientFieldData.Any())
            {
                return;
            }

            foreach (var standarComment in clientFieldData.Where(x => x.Name == "standardizedComments"))
            {
                var meeting = viewModel.Meetings.First(x => x.TCAbstractId.ToString() == standarComment.Id);
                meeting.StandardizedComment = int.Parse(standarComment.Value);
            }
        }

        public static void UpdateMeetingGeneralCommentsViewModel(
            this MeetingGeneralCommentsViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            if ((viewModel == null) || (clientFieldData == null))
            {
                return;
            }

            var sWCoordinatorComment = clientFieldData.Where(x => x.Name == "comment");
            var listOwnCommentIdFinal = sWCoordinatorComment.Select(x => Convert.ToInt32(x.Id)).Where(x => x > 0).Distinct().ToList();
            var allIds = viewModel.SWCoordinatorComments.Select(x => x.CommentId);

            var idsFromOtherOrToDelete = allIds.Where(x => !listOwnCommentIdFinal.Any(y => x == y)).ToList();

            foreach (var id in idsFromOtherOrToDelete)
            {
                var commentToDelete = viewModel.SWCoordinatorComments.First(x => x.CommentId == id);
                viewModel.SWCoordinatorComments.Remove(commentToDelete);
            }

            foreach (var c in sWCoordinatorComment)
            {
                var idAux = Convert.ToInt32(c.Id);

                var comentAux = viewModel.SWCoordinatorComments.FirstOrDefault(x => x.CommentId == idAux);

                if (comentAux == null)
                {
                    comentAux = new MeetingGeneralComment();
                    viewModel.SWCoordinatorComments.Add(comentAux);
                }

                comentAux.Comment = c.Value;
                comentAux.Date = DateTime.Now;
                comentAux.TypeComment = MeetingTypeComment.SingleWindowCoordinator;
            }
        }

        public static void UpdateDonorDecisionViewModel(
            this DonorDecisionViewModel viewModel,
            ClientFieldData[] clientFieldData,
            IEnumMappingService enumMappingService)
        {
            var type = clientFieldData.FirstOrDefault(x => x.Name == "decision");
            if (type != null)
            {
                viewModel.Type = enumMappingService
                    .GetMappedEnum<DonorDecisionTypeEnum>(int.Parse(type.Value));
            }

            var waiverGranted = clientFieldData.FirstOrDefault(x => x.Name == "waiver");
            if (waiverGranted != null)
            {
                viewModel.WaiverGranted = bool.Parse(waiverGranted.Value);
            }

            var explain = clientFieldData.FirstOrDefault(x => x.Name == "explain");
            if (explain != null)
                viewModel.Explain = explain.Value;

            var reasonForOnHoldStatus = clientFieldData
                .FirstOrDefault(c => c.Name == "reasonForOnHoldStatus");
            if (reasonForOnHoldStatus != null)
            {
                viewModel.ReasonForOnHoldStatus = enumMappingService
                    .GetMappedEnum<ReasonHoldOperationEnum>(
                        Convert.ToInt32(reasonForOnHoldStatus.Value));
            }

            var causeOfNoFunding = clientFieldData.FirstOrDefault(x =>
                x.Name == "causeOfNoFunding");
            if (causeOfNoFunding != null)
            {
                viewModel.CauseOfNoFunding = enumMappingService
                    .GetMappedEnum<NotFundingCauseEnum>(int.Parse(causeOfNoFunding.Value));
            }

            ClientFieldData justification = null;

            switch (viewModel.Type)
            {
                case DonorDecisionTypeEnum.PlaceOperationOnHold:
                    justification = clientFieldData.FirstOrDefault(x =>
                        x.Name == "justificationOnHold");
                    break;
                case DonorDecisionTypeEnum.RejectFundEligibility:
                    justification = clientFieldData.FirstOrDefault(x =>
                        x.Name == "justificationReject");
                    break;
                case DonorDecisionTypeEnum.WithdrawOperation:
                    justification = clientFieldData.FirstOrDefault(x =>
                        x.Name == "reasonForWithdrawal");
                    break;
            }

            if (justification != null)
                viewModel.Justification = justification.Value;

            var modificationsTCDocuments = clientFieldData.FirstOrDefault(x =>
                x.Name == "modificationsTCDocuments");
            if (modificationsTCDocuments != null &&
                viewModel.Type == DonorDecisionTypeEnum.AwardEligibilityTCDocument)
            {
                viewModel.ModificationsTCDocuments = modificationsTCDocuments.Value;
            }

            UpdateDocuments(
                viewModel.Documents,
                clientFieldData,
                "DonorDecisionDocuments-docNumber",
                "DonorDecisionDocuments-description");

            if ((viewModel.Type == DonorDecisionTypeEnum.AwardFundEligibility) ||
                (viewModel.Type == DonorDecisionTypeEnum.AwardEligibilityTCDocument))
            {
                UpdateAwardFundEligibilityViewModel(
                    viewModel.AwardFundEligibility,
                    clientFieldData);
            }

            viewModel.AwardFundEligibility.UpdateSubFundViewModel(clientFieldData);
        }

        public static void UpdateEscDecisionViewModel(
            this ESCDecisionViewModel viewModel,
            ClientFieldData[] clientFieldData,
            IEnumMappingService enumMappingService)
        {
            var type = clientFieldData.FirstOrDefault(x => x.Name == "decision");
            if (type != null)
            {
                viewModel.Type = enumMappingService
                    .GetMappedEnum<ESCDecisionTypeEnum>(int.Parse(type.Value));
            }

            var typeOperation = clientFieldData.FirstOrDefault(x => x.Name == "typeOperation");
            if (typeOperation != null)
            {
                viewModel.TypeOperation = enumMappingService
                    .GetMappedEnum<EscDecisionOperationTypeEnum>(int.Parse(typeOperation.Value));
            }

            var causeOfNoFunding = clientFieldData.FirstOrDefault(x => x.Name == "causeOfNoFunding");
            if (causeOfNoFunding != null)
            {
                viewModel.CauseOfNoFunding = enumMappingService
                    .GetMappedEnum<NotFundingCauseEnum>(int.Parse(causeOfNoFunding.Value));
            }

            var reasonForOnHoldStatus = clientFieldData
                .FirstOrDefault(c => c.Name == "reasonForOnHoldStatus");
            if (reasonForOnHoldStatus != null)
            {
                viewModel.ReasonForOnHoldStatus = enumMappingService
                    .GetMappedEnum<ReasonHoldOperationEnum>(
                        Convert.ToInt32(reasonForOnHoldStatus.Value));
            }

            ClientFieldData justification = null;

            switch (viewModel.Type)
            {
                case ESCDecisionTypeEnum.PlaceOperationOnHold:
                    justification = clientFieldData.FirstOrDefault(x =>
                        x.Name == "justificationOnHold");
                    break;
                case ESCDecisionTypeEnum.RejectFundEligibility:
                    justification = clientFieldData.FirstOrDefault(x =>
                        x.Name == "justificationReject");
                    break;
                case ESCDecisionTypeEnum.WithdrawOperation:
                    justification = clientFieldData.FirstOrDefault(x =>
                        x.Name == "reasonForWithdrawal");
                    break;
            }

            if (justification != null)
                viewModel.Justification = justification.Value;

            var modificationsTCDocuments = clientFieldData.FirstOrDefault(x =>
                x.Name == "modificationsTCDocuments");

            if (modificationsTCDocuments != null)
            {
                viewModel.ModificationsTCDocuments = modificationsTCDocuments.Value;
            }

            UpdateDocuments(
                viewModel.Documents,
                clientFieldData,
                "EscDecisionDocuments-docNumber",
                "EscDecisionDocuments-description");

            if (viewModel.Type == ESCDecisionTypeEnum.AwardFundEligibility ||
                viewModel.Type == ESCDecisionTypeEnum.AwardEligibilityTCDocument)
            {
                UpdateAwardFundEligibilityViewModel(
                    viewModel.AwardFundEligibility, clientFieldData);
            }

            viewModel.AwardFundEligibility.UpdateSubFundViewModel(clientFieldData);
        }

        public static void UpdateRequestIncreaseViewModel(
            this RequestIncreaseViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            var increaseRequested = clientFieldData.FirstOrDefault(x => x.Name == "increaseRequested");
            viewModel.IncreaseRequested = null;
            if (increaseRequested != null)
            {
                viewModel.IncreaseRequested = decimal.Parse(increaseRequested.Value);
            }

            var background = clientFieldData.FirstOrDefault(x => x.Name == "background");
            viewModel.Background = null;
            if (background != null)
            {
                viewModel.Background = background.Value;
            }

            var recommendation = clientFieldData.FirstOrDefault(x => x.Name == "recommendation");
            viewModel.Recommendation = null;
            if (recommendation != null)
            {
                viewModel.Recommendation = recommendation.Value;
            }

            var authority = clientFieldData.FirstOrDefault(x => x.Name == "authority");
            viewModel.Authority = null;
            if (authority != null)
            {
                viewModel.Authority = authority.Value;
            }

            UpdateDocuments(viewModel.Documents, clientFieldData, "RequestIncreaseDocuments-docNumber", "RequestIncreaseDocuments-description");
        }

        public static void UpdateAwardFundEligibilityViewModel(
            this AwardFundEligibilityViewModel viewModel,
            ClientFieldData[] clientFieldData)
        {
            var percentageFinancedBy = clientFieldData
                .FirstOrDefault(c => c.Name == "readPercentageFinancedBy");

            viewModel.PercentageFinancedBy = 0;

            if (percentageFinancedBy != null)
                viewModel.PercentageFinancedBy = Convert.ToDecimal(percentageFinancedBy.Value);

            var amountOrdinaryCapitalFund = clientFieldData
                .FirstOrDefault(c => c.Name == "amountOrdinaryCapitalFund");

            viewModel.AmountOrdinaryCapitalFund = 0;

            if (amountOrdinaryCapitalFund != null)
                viewModel.AmountOrdinaryCapitalFund = Convert
                    .ToDecimal(amountOrdinaryCapitalFund.Value);

            var amountMultidonorFunds = clientFieldData
                .Where(c => c.Name == "amountMultidonorFund");

            foreach (var amountMultidonor in amountMultidonorFunds)
            {
                viewModel.AwardFundEligibilityMultidonors
                    .First(m => m.MultidonorFundId.ToString() == amountMultidonor.Id)
                    .AmountMultidonorFund = Convert.ToDecimal(amountMultidonor.Value);
            }

            var sf1Amounts = clientFieldData.Where(x => x.Name == "sf1Amount");

            foreach (var sf1Amount in sf1Amounts)
            {
                viewModel.AwardFundEligibilityRows
                    .First(x => x.ComponentId.ToString() == sf1Amount.Id)
                    .SF1Amount = Convert.ToDecimal(sf1Amount.Value);
            }

            var isCompleted = clientFieldData.FirstOrDefault(c => c.Name == "IsCompleted");

            viewModel.IsCompletedForMe = false;

            if (isCompleted != null)
                viewModel.IsCompletedForMe = isCompleted.Value == CHECKBOX_VALUE_TRUE;
        }

        public static void UpdateEligibilityDecisionViewModel(
            this EligibilityDecisionViewModel viewModel,
            ClientFieldData[] clientFieldData,
            IEnumMappingService enumMappingService)
        {
            var decision = clientFieldData.FirstOrDefault(c => c.Name == "decision");
            viewModel.Type = EligibilityDecisionTypeEnum.NA;
            if (decision != null)
            {
                viewModel.Type = enumMappingService
                    .GetMappedEnum<EligibilityDecisionTypeEnum>(Convert.ToInt32(decision.Value));
            }

            switch (viewModel.Type)
            {
                case EligibilityDecisionTypeEnum.PlaceOperationOnHold:
                    var reasonForOnHoldStatus = clientFieldData.FirstOrDefault(c =>
                        c.Name == "reasonForOnHoldStatus");
                    var explanation = clientFieldData.FirstOrDefault(c => c.Name == "explanation");
                    viewModel.Explanation = null;

                    if (reasonForOnHoldStatus != null)
                    {
                        viewModel.ReasonForOnHoldStatus = enumMappingService
                            .GetMappedEnum<ReasonHoldOperationEnum>(
                                Convert.ToInt32(reasonForOnHoldStatus.Value));
                    }

                    if (explanation != null)
                        viewModel.Explanation = explanation.Value;
                    break;
                case EligibilityDecisionTypeEnum.EligibilityByDelegation:
                    UpdateAwardFundEligibilityViewModel(
                        viewModel.AwardFundEligibility,
                        clientFieldData);
                    break;
                case EligibilityDecisionTypeEnum.WithdrawOperation:
                    var reasonWithdraw = clientFieldData.FirstOrDefault(c =>
                        c.Name == "reasonForWithdrawal");
                    viewModel.Explanation = null;
                    if (reasonWithdraw != null)
                        viewModel.Explanation = reasonWithdraw.Value;
                    break;
            }

            UpdateDocuments(viewModel.Documents,
                clientFieldData,
                "EligibilityDecisionDocuments-docNumber",
                "EligibilityDecisionDocuments-description");

            viewModel.AwardFundEligibility.UpdateSubFundViewModel(clientFieldData);
        }

        public static void UpdateFundInformationBasicInformationViewModel(
            this FIBasicDataViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            if ((viewModel == null) || (clientFieldData == null) || !clientFieldData.Any())
            {
                return;
            }

            //------------------------------------- Basic Data  -------------------------------------//
            //Tying Condition
            var tyingCondition = clientFieldData.FirstOrDefault(c => c.Name == "TyingCondition");
            if (tyingCondition != null)
            {
                viewModel.TyingCondition = tyingCondition.Value;
            }

            //Tied Percentage
            var tiedPercentage = clientFieldData.FirstOrDefault(c => c.Name == "TiedPercentage");
            if (tiedPercentage != null && tyingCondition != null && tyingCondition.Value == "Tied")
            {
                viewModel.TiedPercentage = decimal.Parse(tiedPercentage.Value);
            }
            else
            {
                viewModel.TiedPercentage = null;
            }

            //Display Fund on Web
            var displayFundOnWeb = clientFieldData.FirstOrDefault(c => c.Name == "DisplayFundWeb");
            if (displayFundOnWeb.Value == "Yes")
            {
                viewModel.DisplayFundWeb = true;
            }
            else
            {
                viewModel.DisplayFundWeb = false;
            }

            // Fund Description
            var fundDescription = clientFieldData.FirstOrDefault(c => c.Name == "FundDescription");
            viewModel.FundDescription = fundDescription.GetString(isOptional: true, defaultValue: string.Empty);

            var fundDescriptionEs = clientFieldData.FirstOrDefault(c => c.Name == "FundDescriptionEs");
            viewModel.FundDescriptionEs = fundDescriptionEs.GetString(isOptional: true, defaultValue: string.Empty);

            // Fund Project Size
            var projectSizeEn = clientFieldData.FirstOrDefault(c => c.Name == "ProjectSizeEn");
            viewModel.ProjectSizeEn = projectSizeEn.GetString(isOptional: true, defaultValue: string.Empty);

            var projectSizeEs = clientFieldData.FirstOrDefault(c => c.Name == "ProjectSizeEs");
            viewModel.ProjectSizeEs = projectSizeEs.GetString(isOptional: true, defaultValue: string.Empty);

            // Fund Web Name
            var webNameEn = clientFieldData.FirstOrDefault(c => c.Name == "WebNameEn");
            viewModel.WebNameEn = webNameEn.GetString(isOptional: true, defaultValue: string.Empty);

            var webNameEs = clientFieldData.FirstOrDefault(c => c.Name == "WebNameEs");
            viewModel.WebNameEs = webNameEs.GetString(isOptional: true, defaultValue: string.Empty);

            //------------------------------------- Fund Attributes  -------------------------------------//
            //Fund Type
            var fundType = clientFieldData.FirstOrDefault(c => c.Name == "FundTypeId");
            viewModel.FundTypeId = fundType != null ? fundType.Value : string.Empty;

            // Themes
            var themes = clientFieldData.FirstOrDefault(c => c.Name == "ThemeList");
            viewModel.Themes = themes.ConvertToListInt();

            // Theme I Focusses
            var themeIFocusses = clientFieldData.FirstOrDefault(c => c.Name == "themeIFocussesList");
            viewModel.ThemeIFocusses = themeIFocusses.ConvertToListInt();

            // Theme II Focusses
            var themeIIFocusses = clientFieldData.FirstOrDefault(c => c.Name == "themeIIFocussesList");
            viewModel.ThemeIIFocusses = themeIIFocusses.ConvertToListInt();

            // Theme III Focusses
            var themeIIIFocusses = clientFieldData.FirstOrDefault(c => c.Name == "themeIIIFocussesList");
            viewModel.ThemeIIIFocusses = themeIIIFocusses.ConvertToListInt();

            // Countries Assigned 
            var countriesAssigned = clientFieldData.FirstOrDefault(c => c.Name == "CountriesAssignedList");
            viewModel.CountriesAssigned = countriesAssigned.ConvertToListInt();

            // Owner(s) of the Fund 
            var ownersOfTheFund = clientFieldData.FirstOrDefault(c => c.Name == "ownersOfTheFundList");
            viewModel.FundOwners = ownersOfTheFund.ConvertToListInt();

            // User(s) of the Fund 
            var usersOfTheFund = clientFieldData.FirstOrDefault(c => c.Name == "usersOfTheFundList");
            viewModel.FundUsers = usersOfTheFund.ConvertToListInt();

            // Economic Sectors Assigned EconomicSectorsAssignedList
            var economicSectorsAssigned = clientFieldData.FirstOrDefault(c => c.Name == "EconomicSectorsAssignedList");
            viewModel.EconomicSectorsAssigned = economicSectorsAssigned.ConvertToListInt();

            // EconomicSubSectorsAssignedList
            var economicSubSectorsAssigned = clientFieldData.FirstOrDefault(c => c.Name == "EconomicSubSectorsAssignedList");
            viewModel.EconomicSubSectorsAssigned = economicSubSectorsAssigned.ConvertToListInt();

            // GCI9 Sector Priorities List
            var cgi9SectorPriorities = clientFieldData.FirstOrDefault(c => c.Name == "GCI9SectorPrioritiesList");
            viewModel.GCI9SectorPriorities = cgi9SectorPriorities.ConvertToListInt();

            // Institutional Challenges and Cross-Cutting Themes
            var iChallengesCCThemes = clientFieldData.FirstOrDefault(c => c.Name == "IChallengesCrossCuttingThemesList");
            viewModel.InstChallCrossCuttingThemes = iChallengesCCThemes.ConvertToListInt();

            //Fund Coordinator
            var fundCoordinator = clientFieldData.FirstOrDefault(c => c.Name == "fundCoordinator_text");
            if (fundCoordinator != null)
            {
                viewModel.FundCoordinatorFullName = fundCoordinator.Value;
            }

            fundCoordinator = clientFieldData.FirstOrDefault(c => c.Name == "fundCoordinator");
            if (fundCoordinator != null)
            {
                viewModel.FundCoordinatorUserName = fundCoordinator.Value.ToLower();
            }

            //Alternate Fund Coordinator(s)
            var fcAlternates = clientFieldData.Where(c => c.Name == "fundCoordinatorAlternate" || c.Name == "fundCoordinatorAlternate_text").ToList();
            UpdateDelegates(viewModel, fcAlternates);

            // Type of Instruments financed by the fund 
            var typeInstrumentsFinanced = clientFieldData.FirstOrDefault(c => c.Name == "TypeInstrumentsFinancedList");
            viewModel.TypeInstFinancedFund = typeInstrumentsFinanced.ConvertToListInt();

            //------------------------------------- External Audits  -------------------------------------//
            var lastAuditDate = clientFieldData.FirstOrDefault(c => c.Name == "LastAuditDate");
            if (lastAuditDate != null)
            {
                viewModel.LastAuditDate = DateTime.Parse(lastAuditDate.Value);
            }
            else
            {
                viewModel.LastAuditDate = null;
            }

            var nextAuditDate = clientFieldData.FirstOrDefault(c => c.Name == "NextAuditDate");
            if (nextAuditDate != null)
            {
                viewModel.NextAuditDate = DateTime.Parse(nextAuditDate.Value);
            }
            else
            {
                viewModel.NextAuditDate = null;
            }

            //------------------------------------- Funding Process Information  -------------------------------------//
            var isRetroactiveExpense = clientFieldData.FirstOrDefault(c => c.Name == "IsRetroactiveExpense");
            if (isRetroactiveExpense != null && !string.IsNullOrEmpty(isRetroactiveExpense.Value))
            {
                viewModel.ISSWPasses = isRetroactiveExpense.Value.ToLower() == YES_VALUE;
            }
            else
            {
                viewModel.ISSWPasses = false;
            }

            var eligibilityByelegation = clientFieldData.FirstOrDefault(c => c.Name == "EligibilityByelegation");
            bool eligibilityByelegationValue = false;
            if (eligibilityByelegation != null && !string.IsNullOrEmpty(eligibilityByelegation.Value))
            {
                eligibilityByelegationValue = Convert.ToBoolean(eligibilityByelegation.Value);
                viewModel.EligibilityByDelegation = eligibilityByelegationValue;
            }
            else
            {
                viewModel.EligibilityByDelegation = false;
            }

            var sendToESC = clientFieldData.FirstOrDefault(c => c.Name == "SendToESC");
            bool sendToESCValue = false;
            if (sendToESC != null && !string.IsNullOrEmpty(sendToESC.Value))
            {
                sendToESCValue = Convert.ToBoolean(sendToESC.Value);
                viewModel.SendToESC = sendToESCValue;
            }
            else
            {
                viewModel.SendToESC = false;
            }

            var sendToDonor = clientFieldData.FirstOrDefault(c => c.Name == "SendToDonor");
            if (sendToDonor != null && !string.IsNullOrEmpty(sendToDonor.Value))
            {
                viewModel.SendToDonor = Convert.ToBoolean(sendToDonor.Value);
            }
            else
            {
                viewModel.SendToDonor = false;
            }

            var delegatedForAmountUpto = clientFieldData.FirstOrDefault(c => c.Name == "DelegatedForAmountUpto");
            if (delegatedForAmountUpto != null && !string.IsNullOrEmpty(delegatedForAmountUpto.Value) && eligibilityByelegationValue)
            {
                viewModel.AmountUpto = Convert.ToDecimal(delegatedForAmountUpto.Value);
            }
            else
            {
                viewModel.AmountUpto = null;
            }

            var escComiteeEmail = clientFieldData.FirstOrDefault(c => c.Name == "ESCCommitteeE-mail");
            if (escComiteeEmail != null && !string.IsNullOrEmpty(escComiteeEmail.Value) && sendToESCValue)
            {
                viewModel.EscTeamID = int.Parse(escComiteeEmail.Value);
            }
            else
            {
                viewModel.EscTeamID = null;
            }

            var template = clientFieldData.FirstOrDefault(c => c.Name == "Template");
            if (template != null && !string.IsNullOrEmpty(template.Value))
            {
                viewModel.EscEmailTemplateID = int.Parse(template.Value);
            }
            else
            {
                viewModel.EscEmailTemplateID = null;
            }
        }

        public static void UpdateFundInformationFundedOperationsViewModel(
            this List<FIRowExtFundWorkforceViewModel> viewModel, ClientFieldData[] clientFieldData)
        {
            if ((viewModel == null) || (clientFieldData == null) || !clientFieldData.Any())
            {
                return;
            }

            // Externally Funded Workforce
            var externallyFWName = clientFieldData.Where(c => c.Name == "name");
            viewModel.ForEach(x => x.Name = null);
            if (externallyFWName != null)
            {
                foreach (var name in externallyFWName)
                {
                    if (!string.IsNullOrEmpty(name.Id))
                    {
                        var extFundWorkforce = viewModel.First(x => x.ExtFundWorkforceID == Convert.ToInt32(name.Id));
                        extFundWorkforce.Name = name.Value;
                    }
                }
            }

            var externallyFWHireDate = clientFieldData.Where(c => c.Name == "hireDate");
            viewModel.ForEach(x => x.HireDate = null);
            if (externallyFWHireDate != null)
            {
                foreach (var hireDate in externallyFWHireDate)
                {
                    if (!string.IsNullOrEmpty(hireDate.Id))
                    {
                        var extFundWorkforce = viewModel.First(x => x.ExtFundWorkforceID == Convert.ToInt32(hireDate.Id));
                        if (hireDate.Value != null)
                        {
                            extFundWorkforce.HireDate = DateTime.ParseExact(hireDate.Value, DATETIME_PARSE_FORMAT, CultureInfo.InvariantCulture);
                        }
                    }
                }
            }

            var externallyFWEndDate = clientFieldData.Where(c => c.Name == "endDate");
            viewModel.ForEach(x => x.EndDate = null);

            if (externallyFWEndDate != null)
            {
                foreach (var endDate in externallyFWEndDate)
                {
                    if (!string.IsNullOrEmpty(endDate.Id))
                    {
                        var extFundWorkforce = viewModel.First(x => x.ExtFundWorkforceID == Convert.ToInt32(endDate.Id));
                        if (endDate.Value != null)
                        {
                            extFundWorkforce.EndDate = DateTime.ParseExact(endDate.Value, DATETIME_PARSE_FORMAT, CultureInfo.InvariantCulture);
                        }
                    }
                }
            }

            var externallyFWCountry = clientFieldData.Where(c => c.Name == "country");
            viewModel.ForEach(x => x.Country = null);

            if (externallyFWCountry != null)
            {
                foreach (var country in externallyFWCountry)
                {
                    if (!string.IsNullOrEmpty(country.Id))
                    {
                        var extFundWorkforce = viewModel.First(x => x.ExtFundWorkforceID == Convert.ToInt32(country.Id));
                        extFundWorkforce.Country = int.Parse(country.Value);
                    }
                }
            }

            if (viewModel.FirstOrDefault() != null && viewModel.FirstOrDefault().ExtFundWorkforceID < 0)
            {
                foreach (var vm in viewModel)
                {
                    vm.ExtFundWorkforceID = 0;
                }
            }
        }

        public static void UpdateFundInformationDocumentsViewModel(
            this List<DocumentViewModel> viewModel, ClientFieldData[] clientFieldData)
        {
            if ((viewModel == null) || (clientFieldData == null))
            {
                return;
            }

            UpdateDocuments(viewModel, clientFieldData, "FundInformationDocuments-docNumber", "FundInformationDocuments-description");
        }

        public static void UpdateDonorContactDetails(
            this FIDonorContactDetailViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            var increaseRequested = clientFieldData.FirstOrDefault(x => x.Name == "ContactName");
            viewModel.ContactName = increaseRequested.GetString(isOptional: true, defaultValue: string.Empty);

            var background = clientFieldData.FirstOrDefault(x => x.Name == "ContactEmail");
            viewModel.ContactEmail = background.GetString(isOptional: true, defaultValue: string.Empty);

            var recommendation = clientFieldData.FirstOrDefault(x => x.Name == "ContactAddress");
            viewModel.ContactAddress = recommendation.GetString(isOptional: true, defaultValue: string.Empty);

            var authority = clientFieldData.FirstOrDefault(x => x.Name == "Comments");
            viewModel.Comments = authority.GetString(isOptional: true, defaultValue: string.Empty);
        }

        #endregion

        #region Private Methods

        private static void UpdateOutputs(
            IEnumerable<ClientFieldData> output,
            List<TCOutputViewModel> outputs,
            ComponentViewModel component)
        {
            var outputsIds = (from outputId in outputs
                              select outputId.OutputId).ToList();
            foreach (var outp in output)
            {
                var outputId = (from o in outp.ExtraData
                                where o.Key.Contains(DATA_PERSIST_OLD_KEY) || o.Key.Contains(DATA_PERSIST_NEW_KEY)
                                select Convert.ToInt32(o.Value)).First();
                var oldOutput = outputs.FirstOrDefault(x => x.OutputId == outputId);
                if (oldOutput != null)
                {
                    oldOutput.Description = outp.Value;
                    outputsIds.Remove(outputId);
                }
                else
                {
                    outputs.Add(
                        new TCOutputViewModel
                        {
                            Description = outp.Value
                        });
                }
            }

            //Delete Outputs
            if (outputsIds.Any())
            {
                foreach (var outputId in outputsIds)
                {
                    var outputDelete = component.Outputs.FirstOrDefault(x => x.OutputId == outputId);
                    if (outputDelete != null)
                    {
                        component.Outputs.Remove(outputDelete);
                    }
                }
            }
        }

        private static void UpdateComments(
            ICollection<TCAbstractCommentsViewModel> comments,
            CommentTypeEnum typeComment,
            IEnumerable<ClientFieldData> clientFieldData)
        {
            var internalType = ((byte)typeComment).ToString(CultureInfo.InvariantCulture);
            var internalComments = clientFieldData.Where(x => x.ExtraData.Any(y => y.Key == "data-persist-type" && y.Value == internalType));

            var listOwnCommenyIdFinal = internalComments.Select(x => Convert.ToInt32(x.Id)).Where(x => x > 0).Distinct().ToList();
            var allIds = comments.Select(x => x.CommentId);

            var idsFromOtherOrToDelete = allIds.Where(x => !listOwnCommenyIdFinal.Any(y => x == y)).ToList();

            foreach (var id in idsFromOtherOrToDelete)
            {
                var commentToDelete = comments.First(x => x.CommentId == id);
                comments.Remove(commentToDelete);
            }

            foreach (var c in internalComments)
            {
                var idAux = Convert.ToInt32(c.Id);

                var comentAux = comments.FirstOrDefault(x => x.CommentId == idAux);
                if (comentAux == null)
                {
                    comentAux = new TCAbstractCommentsViewModel();
                    comments.Add(comentAux);
                }

                comentAux.Comment = c.Value;
                comentAux.Date = DateTime.Now;
            }
        }

        static List<NotifyFundCoordinatorRowViewModel> UpdateSingleWindowDecisionFunds(
            int singleWindowMeetingId,
            IEnumerable<ClientFieldData> clientFieldData,
            IEnumMappingService enumMappingService)
        {
            var clientFieldDatas = clientFieldData as IList<ClientFieldData> ??
                clientFieldData.ToList();
            var funds = clientFieldDatas.Where(x =>
                x.ExtraData.ContainsKey(DATA_PERSIST_OLD_KEY) ||
                x.ExtraData.ContainsKey(DATA_PERSIST_NEW_KEY));

            var result = new List<NotifyFundCoordinatorRowViewModel>();
            var applicableFunds = funds.Where(c => c.Name == APPLICABLE_FUND).ToList();
            var fundDecisions = funds.Where(c => c.Name == FUND_DECISION).ToList();
            var selectedItems = funds.Where(c => c.Name == SELECTED).ToList();

            if (applicableFunds.Count != fundDecisions.Count ||
                fundDecisions.Count != selectedItems.Count ||
                applicableFunds.Count != fundDecisions.Count)
            {
                string errorMessage = string.Format(
                    "Error when parsing FC to update SWD Funds, the lists don't match!! Counts: {0} {1} {2}",
                        applicableFunds.Count,
                        fundDecisions.Count,
                        selectedItems.Count);

                Logger.GetLogger().WriteError(
                    "ClientFieldDataMappers - TC", errorMessage, new Exception(errorMessage));

                return result;
            }

            for (int i = 0; i < applicableFunds.Count(); i++)
            {
                result.Add(new NotifyFundCoordinatorRowViewModel
                {
                    ApplicableFundId = int.Parse(applicableFunds[i].Value),
                    DecisionId = enumMappingService.GetMappedEnum<DecisionSWFCDecisionEnum>(
                        int.Parse(fundDecisions[i].Value)),
                    Notify = bool.Parse(selectedItems[i].Value),
                    SingleWindowMeetingId = singleWindowMeetingId,
                });
            }

            return result;
        }

        private static void UpdateDocuments(
            ICollection<DocumentViewModel> documents,
            IEnumerable<ClientFieldData> clientFieldData,
            string docNumberName,
            string descriptionName)
        {
            var formDocuments = clientFieldData.Where(x => (x.Name == docNumberName) || (x.Name == descriptionName)).ToList();

            var newDocumentsData = formDocuments.Where(d => d.Id == "newDocument");
            var oldDocumentsData = formDocuments.Where(d => d.Id != "newDocument");

            // Delete old DocNumber - Document Reference
            var finalIds = oldDocumentsData.Select(x => x.Id).Distinct();
            var originalIds = documents.Select(x => x.DocumentId.ToString());
            var idsToRemove = originalIds.Where(x => !finalIds.Any(y => x == y)).ToList();

            foreach (var id in idsToRemove)
            {
                var elementToRemove = documents.First(x => x.DocumentId.ToString() == id);
                documents.Remove(elementToRemove);
            }

            // Update old elements
            var oldGrouped = oldDocumentsData.GroupBy(x => x.Id);
            foreach (var old in oldGrouped)
            {
                var original = documents.FirstOrDefault(x => x.DocumentId.ToString() == old.Key);
                if (original != null)
                {
                    var description = old.FirstOrDefault(x => x.Name == descriptionName);
                    original.Description = null;
                    if ((description != null) && (description.Value != null) && !string.IsNullOrWhiteSpace(description.Value))
                    {
                        original.Description = description.Value;
                    }
                }
            }

            var newGrouped = newDocumentsData.GroupBy(x => x.ExtraData.FirstOrDefault(y => y.Key == "data-persist-new"));

            // Add new DocNumber - Document Reference
            foreach (var _new in newGrouped)
            {
                var docNumber = _new.FirstOrDefault(x => x.Name == docNumberName);
                var newDocument = new DocumentViewModel()
                {
                    DocNumber = docNumber.Value
                };
                documents.Add(newDocument);

                var description = _new.FirstOrDefault(x => x.Name == descriptionName);
                if ((description != null) && (description.Value != null) && !string.IsNullOrWhiteSpace(description.Value))
                {
                    newDocument.Description = description.Value;
                }
            }
        }

        static List<int> ConvertToListInt(this ClientFieldData field)
        {
            if (field == null || string.IsNullOrWhiteSpace(field.Value))
                return new List<int>();

            var values = field.Value.Split(',');
            var valuesInt = values.Select(x => int.Parse(x)).ToList();

            return valuesInt;
        }

        private static void UpdateDelegates(
            FIBasicDataViewModel viewModel,
            List<ClientFieldData> delegates)
        {
            var delegatesViewModel = viewModel.Delegates;
            var originalOldIds = viewModel.Delegates.Select(x => x.DelegatesID).ToList();

            var oldItems = delegates.Where(x => x.ExtraData.Any(y => y.Key == DATA_PERSIST_OLD_KEY)).ToList();
            var newItems = delegates.Where(x => x.ExtraData.Any(y => y.Key == DATA_PERSIST_NEW_KEY));

            var oldItemsGroup = oldItems.GroupBy(x => x.ExtraData.FirstOrDefault(y => y.Key == DATA_PERSIST_OLD_KEY));
            var newItemsGroup = newItems.GroupBy(x => x.ExtraData.FirstOrDefault(y => y.Key == DATA_PERSIST_NEW_KEY));

            foreach (var newItem in newItemsGroup)
            {
                delegatesViewModel.Add(new FIDelegatesViewModel
                {
                    DelegateFullName = newItem.First(x => x.Name == "fundCoordinatorAlternate_text").Value,
                    DelegateUserName = newItem.First(x => x.Name == "fundCoordinatorAlternate").Value,
                });
            }

            var finalOldIds = oldItemsGroup.Select(x => int.Parse(x.Key.Value)).ToList();
            var idsToRemove = originalOldIds.Where(x => !finalOldIds.Any(y => x == y));

            delegatesViewModel.RemoveAll(x => idsToRemove.Any(y => y == x.DelegatesID));
        }

        private static void AddFundCoordinatorToModel(
            TCReviewNotifyFCViewModel viewModel,
            IGrouping<KeyValuePair<string, string>, ClientFieldData> group)
        {
            var fundCoordinator = group
                .FirstOrDefault(x => x.Name == FUND_COORD_APPICABLEFUND);

            if (fundCoordinator == null)
                return;

            int id;
            if (int.TryParse(fundCoordinator.Value, out id))
            {
                var fundCoord = new TCNotifyFCFundCoordinatorViewModel()
                {
                    ApplicableFundId = id,
                    TrustFundId = id,
                };

                viewModel.TCNotifyFundCoordinator.Add(fundCoord);
            }
        }

        static IList<TCNotifyFCRegionalTeamLeaderViewModel> FillRTLRegionalOperation(
            ClientFieldData[] clientFieldData)
        {
            var result = new List<TCNotifyFCRegionalTeamLeaderViewModel>();
            var countryDepartments = clientFieldData
                .Where(x => x.Name == COUNTRY_DEPARTMENT_RG_OPERATION_FIELD_KEY).ToList();
            var regionalTeamLeaderNames = clientFieldData
                .Where(x => x.Name.StartsWith(REGIONAL_TEAM_LEADER_RG_OPERATION_FIELD_KEY)).ToList();

            if (countryDepartments != null && regionalTeamLeaderNames != null)
            {
                var oldItemsCountryDepts = countryDepartments
                    .Where(x => x.ExtraData.Any(y => y.Key == DATA_PERSIST_OLD_KEY));
                var newItemsCountryDepts = countryDepartments
                    .Where(x => x.ExtraData.Any(y => y.Key == DATA_PERSIST_NEW_KEY));
                List<ClientFieldData> allItemsCountryDepts = oldItemsCountryDepts.ToList();
                allItemsCountryDepts.AddRange(newItemsCountryDepts);

                var oldItemsRegionalTLNames = regionalTeamLeaderNames
                    .Where(x => x.ExtraData.Any(y => y.Key == DATA_PERSIST_OLD_KEY));
                var newItemsRegionalTLNames = regionalTeamLeaderNames
                    .Where(x => x.ExtraData.Any(y => y.Key == DATA_PERSIST_NEW_KEY));
                List<ClientFieldData> allItemsRegionalTLNames = oldItemsRegionalTLNames.ToList();
                allItemsRegionalTLNames.AddRange(newItemsRegionalTLNames);

                if ((allItemsCountryDepts != null && allItemsCountryDepts.Any()) &&
                    allItemsRegionalTLNames != null && allItemsRegionalTLNames.Any())
                {
                    int id = 0;
                    for (int i = 0; i < allItemsCountryDepts.Count(); i++)
                    {
                        if (int.TryParse(allItemsCountryDepts[i].Value, out id))
                        {
                            result.Add(new TCNotifyFCRegionalTeamLeaderViewModel
                            {
                                CountryLiaisonId = id,
                                RegionalFullName = allItemsRegionalTLNames[i].Value
                            });
                        }
                    }
                }
            }

            return result;
        }

        static IList<TCNotifyFCRegionalTeamLeaderViewModel> FillRTLNonRegionalOperation(
            ClientFieldData[] clientFieldData)
        {
            var result = new List<TCNotifyFCRegionalTeamLeaderViewModel>();

            var countryDepartment = clientFieldData
                .FirstOrDefault(x => x.Name == COUNTRY_DEPARTMENT_FIELD_KEY);
            var regionalTeamLeaderName = clientFieldData
                .FirstOrDefault(x => x.Name == REGIONAL_TEAM_LEADER_FIELD_KEY);

            if ((countryDepartment != null) && regionalTeamLeaderName != null)
            {
                int id = 0;
                if (int.TryParse(countryDepartment.Value, out id))
                {
                    result.Add(new TCNotifyFCRegionalTeamLeaderViewModel
                    {
                        CountryLiaisonId = id,
                        RegionalFullName = regionalTeamLeaderName.Value
                    });
                }
            }

            return result;
        }

        private static string GetString(
            this ClientFieldData field, bool isOptional = true, string defaultValue = null)
        {
            if ((field == null) || string.IsNullOrWhiteSpace(field.Value))
            {
                if (isOptional)
                {
                    return defaultValue;
                }
                else
                {
                    throw new Exception();
                }
            }

            return field.Value;
        }

        private static void UpdateSubFundViewModel(
            this AwardFundEligibilityViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            var awardEligibilitySubFundCheck = clientFieldData
                .FirstOrDefault(c => c.Name == AWARD_ELIGIBILITY_SUBFUND_CHECK);

            if (awardEligibilitySubFundCheck != null)
            {
                viewModel.SubFund.isChecked = bool.Parse(awardEligibilitySubFundCheck.Value);
            }
        }

        static void UpdateTCBasicData(
            this TCAbstractBasicDataViewModel model,
            ClientFieldData[] requestClient)
        {
            if (!requestClient.HasAny())
            {
                model = new TCAbstractBasicDataViewModel();

                return;
            }

            ClientFieldData field;

            field = requestClient.GetFieldByName(TCBasicDataField.Beneficiary);
            model.Beneficiary = field == null ? null : field.Value;

            field = requestClient.GetFieldByName(TCBasicDataField.ReferenceRequest);
            model.ReferenceRequest = field == null ? null : field.Value;

            field = requestClient.GetFieldByName(TCBasicDataField.ContactName);
            model.ContactName = field == null ? null : field.Value;

            field = requestClient.GetFieldByName(TCBasicDataField.Objetive);
            model.Objective = field == null ? null : field.Value;

            field = requestClient.GetFieldByName(TCBasicDataField.ObjetiveForeignLanguage);
            model.ObjectiveForeignLanguage = field == null ? null : field.Value;

            field = requestClient.GetFieldByName(TCBasicDataField.Jutification);
            model.Justification = field == null ? null : field.Value;

            field = requestClient.GetFieldByName(TCBasicDataField.Description);
            model.Description = field == null ? null : field.Value;

            field = requestClient.GetFieldByName(TCBasicDataField.ContactInformation);
            model.ContactInformation = field == null ? null : field.Value;

            field = requestClient.GetFieldByName(TCBasicDataField.ExecutingAgencyStructure);
            model.ExecutingAgencyStructure = field == null ? null : field.Value;

            field = requestClient.GetFieldByName(TCBasicDataField.JustificationExecutionStructure);
            model.JustificationExecutionStructure = field == null ? null : field.Value;

            field = requestClient.GetFieldByName(TCBasicDataField.CountryDepartment);
            model.CountryDepartment = field.GetParseValueToNullableInt();

            field = requestClient.GetFieldByName(TCBasicDataField.PreparedByUnit);
            model.PreparedByUnit = field.GetParseValueToNullableInt();

            field = requestClient.GetFieldByName(TCBasicDataField.UnitDisbursementResponsability);
            model.UnitDisbursementResponsability = field.GetParseValueToNullableInt();

            field = requestClient.GetFieldByName(TCBasicDataField.OperationTheme1);
            model.OperationTheme1Id = field.GetParseValueToNullableInt();

            field = requestClient.GetFieldByName(TCBasicDataField.OperationTheme2);
            model.OperationTheme2Id = field.GetParseValueToNullableInt();

            field = requestClient.GetFieldByName(TCBasicDataField.OperationTheme3);
            model.OperationTheme3Id = field.GetParseValueToNullableInt();

            field = requestClient.GetFieldByName(TCBasicDataField.IsIncludeCPD);
            model.IsIncludedCPD = false;
            if (field != null)
                model.IsIncludedCPD = field.Value == "Yes" ? true : false;

            field = requestClient.GetFieldByName(TCBasicDataField.IsIncludeCountry);
            model.IsIncludedCountryStrategy = false;
            if (field != null)
                model.IsIncludedCountryStrategy = field.Value == "Yes" ? true : false;

            field = requestClient
                .FirstOrDefault(f => f.Name == TCBasicDataField.TypesOfConsultants);
            model.TypesOfConsultants = field.ConvertToListInt();

            field = requestClient
                .FirstOrDefault(f => f.Name == TCBasicDataField.GCISectorPriorities);
            model.GCISectorPriority = field.ConvertToListInt();

            model.FinancingData.UpdateTCFinancingData(requestClient);
            model.OperationalSupport = requestClient.ConvertToOperationalSupport();
        }

        static void UpdateTCFinancingData(
            this TCFinancingDataViewModel model,
            ClientFieldData[] requestClient)
        {
            ClientFieldData field;

            field = requestClient.GetFieldByName(TCBasicDataField.DisbursementPeriod);
            model.DisbursementPeriod = default(int);
            if (field != null)
            {
                field.Value = field.Value.Replace(".", string.Empty).Replace(",", string.Empty);
                model.DisbursementPeriod = field.GetParseValueToInt();
            }
        }

        static TCOperationalSupportViewModel ConvertToOperationalSupport(
            this ClientFieldData[] requestClient)
        {
            var model = new TCOperationalSupportViewModel
            {
                SupportedOperations = new List<TCSupportedOperationViewModel>(),
            };

            if (!requestClient.HasAny())
                return model;

            var supportedOperations = requestClient
                .Where(x => x.Name == TCBasicDataField.SupportedOperationType);

            if (!supportedOperations.HasAny())
                return model;

            var checkedOperations = requestClient
                .Where(x => x.Name == TCBasicDataField.CheckRelatedOperation);

            foreach (var supportOp in supportedOperations)
            {
                model.SupportedOperations.Add(
                    new TCSupportedOperationViewModel
                    {
                        SupportTypeId = supportOp.GetParseValueToInt(),
                        SupportedOperationId = supportOp.GetParseIdToInt(),
                        IsChecked = checkedOperations.IsCheckedSupportedOperation(supportOp.Id),
                    });
            }

            return model;
        }

        static bool IsCheckedSupportedOperation(
            this IEnumerable<ClientFieldData> fields,
            string fieldId)
        {
            var field = fields.FirstOrDefault(f => f.Id == fieldId);

            return field.GetParseValueToBool(true);
        }

        static ClientFieldData GetFieldByName(
            this ClientFieldData[] requestClient,
            string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
                return null;

            return requestClient.FirstOrDefault(field =>
                !string.IsNullOrWhiteSpace(field.Name) && field.Name == fieldName);
        }

        static int? GetParseValueToNullableInt(this ClientFieldData field)
        {
            if (field == null)
                return null;

            int result;
            int.TryParse(field.Value, out result);

            return result;
        }

        static int GetParseValueToInt(this ClientFieldData field)
        {
            if (field == null)
                return int.MinValue;

            int result;
            int.TryParse(field.Value, out result);

            return result;
        }

        static int GetParseIdToInt(this ClientFieldData field)
        {
            if (field == null)
                return int.MinValue;

            int result;
            int.TryParse(field.Id, out result);

            return result;
        }

        static bool GetParseValueToBool(this ClientFieldData field, bool defaultValue)
        {
            if (field == null)
                return defaultValue;

            bool result;
            bool.TryParse(field.Value, out result);

            return result;
        }

        static decimal? GetParseValueToDecimal(this ClientFieldData field)
        {
            if (field == null)
                return null;

            decimal result;
            decimal.TryParse(field.Value, out result);

            return result;
        }

        #endregion
    }
}