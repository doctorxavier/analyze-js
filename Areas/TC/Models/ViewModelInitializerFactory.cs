using System;
using System.Collections.Generic;

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
using IDB.MW.Application.TCAbstractModule.ViewModels.Shared;
using IDB.MW.Application.TCAbstractModule.ViewModels.SingleWindowDecision;
using IDB.MW.Application.TCAbstractModule.ViewModels.SingleWindowOperationDecision;
using IDB.MW.Application.TCAbstractModule.ViewModels.TCAbstractService;
using IDB.MW.Application.TCAbstractModule.ViewModels.ValidationTCAbstract;

namespace IDB.Presentation.MVC4.Areas.TC.Models
{
    public static class ViewModelInitializerFactory
    {
        public static TCAbstractViewModel InitializeTCAbstractViewModel()
        {
            return new TCAbstractViewModel()
            {
                BasicData = new TCAbstractBasicDataViewModel
                {
                    Taxonomy = new TCAbstractFieldViewModel(),
                    OperationalSupport = new TCOperationalSupportViewModel(),
                },
                Comments = new List<TCAbstractCommentsViewModel>(),
                Financing = new TCAbstractFinancingViewModel()
                {
                    Components = new List<ComponentFinancingViewModel>()
                },
                Header = new HeaderViewModel(),
                MembersData = new List<TCMemberDataViewModel>(),
                ResultsMatrix = new TCAbstractResultsMatrixViewModel()
                {
                    Components = new List<ComponentViewModel>(),
                    Outcomes = new List<TCOutcomeViewModel>(),
                },
                TCId = 0
            };
        }

        public static ReviewTCAbstractViewModel InitializeReviewTcAbstractViewModel()
        {
            return new ReviewTCAbstractViewModel
            {
                TCAbstract = InitializeTCAbstractViewModel(),
                Comments = new List<TCAbstractCommentsViewModel>()
            };
        }

        public static ValidationTCAbstractViewModel InitializeValidationTcAbstractViewModel()
        {
            return new ValidationTCAbstractViewModel
            {
                TCAbstract = InitializeTCAbstractViewModel(),
                CommentsInternal = new List<TCAbstractCommentsViewModel>(),
                CommentsTeamLeader = new List<TCAbstractCommentsViewModel>()
            };
        }

        public static PriorityTCAbstractViewModel InitializePriorityTcAbstractViewModel(
            IEnumMappingService enumMappingService)
        {
            return new PriorityTCAbstractViewModel()
            {
                TCAbstract = InitializeTCAbstractViewModel(),
                Priority = CountryPriorityTCAbstractEnum.Low,
                Comments = new List<TCAbstractCommentsViewModel>()
            };
        }

        public static TCNotifyFCViewModel InitializeTCNotifyFCViewModel()
        {
            return new TCNotifyFCViewModel()
            {
                DateMeeting = DateTime.Today,
                FundCoordinatorSelection = new List<TCNotifyFCFundCoordinatorViewModel>(),
                Objective = string.Empty,
                TCId = 0,
                TCNotifyFCRegionalTeamLeaderViewModel =
                    new List<TCNotifyFCRegionalTeamLeaderViewModel>()
            };
        }

        public static TCReviewNotifyFCViewModel InitializeTCReviewNotifyFCViewModel()
        {
            return new TCReviewNotifyFCViewModel()
            {
                TCReviewFCSelectedViewModel = new List<TCReviewFCSelectedViewModel>(),
                TCId = 0,
                TCReviewRTLSelectedViewModel = new List<TCReviewRTLSelectedViewModel>(),
                TCNotifyFundCoordinator = new List<TCNotifyFCFundCoordinatorViewModel>()
            };
        }

        public static SingleWindowMeetingViewModel InitializeSingleWindowMeetingViewModel()
        {
            return new SingleWindowMeetingViewModel()
            {
                DateMeeting = DateTime.Now
            };
        }

        public static DecisionSWOperationViewModel InitializeDecisionSWOperationViewModel()
        {
            var model = new DecisionSWOperationViewModel
            {
                TCId = 0,
                OperationType = OperationTypeEnum.NA,
                Decision = DecisionOperationTypeEnum.NA,
                Comments = new List<TCAbstractCommentsViewModel>(),
                DateMeeting = DateTime.Now
            };
            return model;
        }

        public static SingleWindowDecisionViewModel InitializeSingleWindowDecisionViewModel()
        {
            return new SingleWindowDecisionViewModel
            {
                PlaceHold = new PlaceHoldViewModel(),
                PreassignedFundCoordinatorList = new List<NotifyFundCoordinatorRowViewModel>(),
                ReturnProjectTeam = new List<ReturnProjectTeamViewModel>(),
                WithdrawOperation = new WithdrawOperationViewModel(),
                Type = SingleWindowDecisionTypeEnum.NA,
                TCAbstractId = 0,
                SingleWindowMeetingId = 0
            };
        }

        public static SingleWindowOperationsViewModel InitializeSingleWindowOperationsViewModel()
        {
            var viewModel = new SingleWindowOperationsViewModel
            {
                Operations = new List<OperationViewModel>()
            };
            return viewModel;
        }

        public static RequestIncreaseViewModel InitializeRequestIncreaseViewModel()
        {
            var model = new RequestIncreaseViewModel()
            {
                ApprovedNumber = string.Empty,
                TotalApproved = (decimal)0.00,
                IncreaseRequested = (decimal)0.00,
                Background = string.Empty,
                Recommendation = string.Empty,
                Authority = string.Empty,
                Documents = new List<DocumentViewModel>()
            };

            return model;
        }

        public static ReviewTCAbstractPostValidationViewModel InitializeReviewPostValidationViewModel()
        {
            return new ReviewTCAbstractPostValidationViewModel
            {
                TCAbstract = InitializeTCAbstractViewModel(),
                Explanation = string.Empty
            };
        }

        public static AwardFundEligibilityViewModel InitializeAwardFundEligibilityViewModel()
        {
            return new AwardFundEligibilityViewModel
            {
                AwardFundEligibilityRows = new List<AwardFundEligibilityRowViewModel>()
            };
        }

        public static FundCoordinatorTCAbstractViewModel InitializeFundCoordinatorTCAbstractViewModel()
        {
            var model = new FundCoordinatorTCAbstractViewModel
            {
                TCAbstract = InitializeTCAbstractViewModel(),
                CurrentAvailability = string.Empty,
                ApprovalsYTD = string.Empty,
                UpcomingContributions = string.Empty,
                CurrentYrPipeline = string.Empty,
                Comments = new List<TCAbstractCommentsViewModel>(),
                TypeEligibility = 0,
                ChangesNeeded = string.Empty,
                CauseOfNoFunding = (int)FundCoordinatorCauseNoFundingEnum.NA,
                Justification = string.Empty
            };

            model.TCAbstract.Header.TCAbstractStatus = string.Empty;

            return model;
        }

        public static EligibilityDecisionViewModel InitializeEligibilityDecisionViewModel()
        {
            return new EligibilityDecisionViewModel
            {
                Documents = new List<DocumentViewModel>(),
                OptionsType = new List<EligibilityDecisionTypeEnum>(),
                AwardFundEligibility = new AwardFundEligibilityViewModel()
            };
        }

        public static FundInformationViewModel InitializeFundInformationViewModel()
        {
            Logger.GetLogger().WriteDebug("InitializeFundInformationViewModel", "Start. InitializeFundInformationViewModel....");
            return new FundInformationViewModel()
            {
                FundID = 0,

                BasicData = new FIBasicDataViewModel()
                {
                    ThemeIIFocusses = new List<int>(),

                    ThemeIIIFocusses = new List<int>(),

                    CountriesAssigned = new List<int>(),

                    EconomicSectorsAssigned = new List<int>(),

                    EconomicSubSectorsAssigned = new List<int>(),

                    InstChallCrossCuttingThemes = new List<int>(),

                    TypeInstFinancedFund = new List<int>(),

                    Delegates = new List<FIDelegatesViewModel>(),

                    DisplayFundWeb = false
                },

                Figures = new FIFiguresViewModel(),

                PendingPledges = new List<FIRowPendingPledgesViewModel>(),

                ExtFundWorkforce = new List<FIRowExtFundWorkforceViewModel>(),

                Donors = new List<FIRowDonorsViewModel>(),

                Documents = new List<DocumentViewModel>(),
            };
        }

        public static FIDonorContactDetailViewModel InitializeDonorContactDetailsPartialViewModel()
        {
            return new FIDonorContactDetailViewModel()
            {
            };
        }

        public static ESCDecisionViewModel InitializeEscWindowDecisionViewModel()
        {
            return new ESCDecisionViewModel
            {
                Type = ESCDecisionTypeEnum.NA,
                CauseOfNoFunding = NotFundingCauseEnum.FundsDepleted,
                Documents = new List<DocumentViewModel>(),
                AwardFundEligibility = ViewModelInitializerFactory.InitializeAwardFundEligibilityViewModel()
            };
        }

        public static DonorDecisionViewModel InitializeDonorWindowDecisionViewModel()
        {
            return new DonorDecisionViewModel
            {
                Type = DonorDecisionTypeEnum.NA,
                CauseOfNoFunding = NotFundingCauseEnum.FundsDepleted,
                Documents = new List<DocumentViewModel>(),
                AwardFundEligibility = ViewModelInitializerFactory.InitializeAwardFundEligibilityViewModel()
            };
        }

        internal static MeetingGeneralCommentsViewModel InitializeMeetingListGeneralCommentsViewModel(int tcAbstractId)
        {
            return new MeetingGeneralCommentsViewModel()
            {
                TCAbstractId = tcAbstractId,
                GeneralComments = new List<MeetingGeneralComment>(),
                SWCoordinatorComments = new List<MeetingGeneralComment>()
            };
        }
    }
}