using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.Architecture.Logging;
using IDB.MVCControls.DataListView.Models;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Application.PCRModule.Enums;
using IDB.MW.Application.PCRModule.Messages.ChecklistService;
using IDB.MW.Application.PCRModule.Messages.FollowUpService;
using IDB.MW.Application.PCRModule.Services.ChecklistService;
using IDB.MW.Application.PCRModule.Services.FollowUpService;
using IDB.MW.Application.PCRModule.ViewModels.ChecklistService;
using IDB.MW.Application.PCRModule.ViewModels.DocumentService;
using IDB.MW.Application.PCRModule.ViewModels.FollowUpService;
using IDB.MW.Application.PCRModule.ViewModels.FollowUpService.Reports;
using IDB.MW.Application.PCRModule.ViewModels.FollowUpService.Tasks;
using IDB.MW.Domain.Entities;
using IDB.MW.Domain.Session;
using IDB.MW.DomainModel.Entities.PCRModule.Enums;
using IDB.MW.Infrastructure.ReportManager.Enums;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.MW.Infrastructure.SecurityService.Enums;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Domain.Values;

namespace IDB.Presentation.MVC4.Areas.PCR.Models
{
    public class ViewModelMapperHelper
    {
        internal const string PCRIDBDocumentList = "PCR_IDB_DOCUMENT_LIST";
        private const int StartYear = 2014;
        private const string Yes = "PCR.FollowUp.Yes";
        private const string No = "PCR.FollowUp.No";
        private const string AllValuesSelector = "-1";
        private readonly IAuthorizationService _authorizationService;
        private readonly ICatalogService _catalogService;
        private readonly IPCRChecklistService _pcrChecklistService;
        private readonly IPCRFollowUpService _pcrFollowUpService;
        private readonly dynamic _viewBag;

        public ViewModelMapperHelper(
            dynamic viewBag,
            IPCRChecklistService pcrChecklistService,
            ICatalogService catalogService,
            IAuthorizationService authorizationService,
            IPCRFollowUpService pcrFollowUpService)
        {
            _viewBag = viewBag;
            _pcrChecklistService = pcrChecklistService;
            _catalogService = catalogService;
            _authorizationService = authorizationService;
            _pcrFollowUpService = pcrFollowUpService;
        }

        public GetSummaryResponse GetSummaryViewModel(string operationNumber)
        {
            var response = _pcrChecklistService.GetSummary(operationNumber);

            if (response.Summary == null)
            {
                response.Summary = new PCRSummaryViewModel
                {
                    PCRUserCommentList = new List<MW.Application.PCRModule.ViewModels.ChecklistService.PCRUserComment>(),
                    SelectedLendingPrograms = new List<int>(),
                    SelectedRegionalDevelopmentGoals = string.Empty,
                    SelectedBankOutputContribution = string.Empty,
                    SummaryEffectivenessCoreList = new List<PCRSummaryEffectivenessCoreRowViewModel>(),
                    SummaryEffectivenessNonCoreList = new List<PCRSummaryEffectivenessNonCoreRowViewModel>()
                };
            }

            var model = response.Summary;

            if (!response.IsValid)
            {
                _viewBag.ErrorMessage = response.ErrorMessage;
                _viewBag.LockScreen = true;
                _viewBag.LockScreenWorkflowValidation = false;
                _viewBag.No95 = response.ErrorCode == 1;
            }
            else
            {
                _viewBag.No95 = false;
                _viewBag.ErrorMessage = null;
                _viewBag.LockScreenWorkflowValidation = false;

                var lendingProgramResponse = _catalogService
                    .GetMasterDataListByTypeCode(typeCodes: PCRIDBObjectiveType.LendingProgram);

                if (!lendingProgramResponse.IsValid)
                {
                    return null;
                }

                _viewBag.LendingProgramItems = MvcHelpers.GetMultiSelectListItems(
                    lendingProgramResponse.MasterDataCollection,
                    IDBContext.Current.CurrentLanguage);
                _viewBag.TextLendingProgram = string.Empty;

                if (model.SelectedLendingPrograms != null && model.SelectedLendingPrograms.Any())
                {
                    var selectedItemsLendingProgram = MvcHelpers.GetSelectedItems(
                        _viewBag.LendingProgramItems,
                        (List<int>)model.SelectedLendingPrograms);
                    _viewBag.TextLendingProgram = GetSelectedItemText(selectedItemsLendingProgram, true, false);
                }
            }

            _viewBag.CurrentStage = model.PCRCurrentStageType;
            SetPermissions(operationNumber, model.PCRCurrentStage);
            _viewBag.OperationNumber = operationNumber;
            _viewBag.UserName = IDBContext.Current.UserName;
            _viewBag.StageName = model.CurrentStageName;
            return response;
        }

        public PCRChecklistViewModel GetChecklistViewModel(string operationNumber)
        {
            var effectivenessResponse = _pcrChecklistService.GetEffectiveness(operationNumber);
            var generalResponse = _pcrChecklistService.GetGeneral(operationNumber);
            var validationResponse = _pcrChecklistService.GetValidations(operationNumber);

            var model = new PCRChecklistViewModel
            {
                PCREffectivenessViewModel = effectivenessResponse.PCREffectiveness,
                PCRGeneralViewModel = generalResponse.PCRGeneral,
                PCRValidationsViewModel = validationResponse.PCRValidations
            };

            if ((model.PCREffectivenessViewModel == null) ||
                (model.PCRGeneralViewModel == null) ||
                (model.PCRValidationsViewModel == null) ||
                !effectivenessResponse.IsValid ||
                !generalResponse.IsValid ||
                !validationResponse.IsValid)
            {
                model.PCREffectivenessViewModel = new PCREffectivenessViewModel
                {
                    PCREffectivenessSummaryList = new List<PCREffectivenessSummaryRowViewModel>(),
                    ComponentList = new List<ComponentViewModel>(),
                    OutcomeList = new List<OutcomeViewModel>(),
                    EffectivenessGuidelinesViewModel = new EffectivenessGuidelinesViewModel()
                };
                model.PCRGeneralViewModel = new PCRGeneralViewModel
                {
                    PCRGeneralViewModelList = new List<PCRGeneralRowViewModel>(),
                    GeneralGuidelinesViewModel = new GeneralGuidelinesViewModel()
                };
                model.PCRValidationsViewModel = new PCRValidationsViewModel
                {
                    PCRValidationsRowViewModelList = new List<PCRValidationsRowViewModel>()
                };
            }

            if (!effectivenessResponse.IsValid ||
                !generalResponse.IsValid ||
                !validationResponse.IsValid)
            {
                _viewBag.ErrorMessage = !string.IsNullOrWhiteSpace(effectivenessResponse.ErrorMessage) ?
                    effectivenessResponse.ErrorMessage : !string.IsNullOrWhiteSpace(generalResponse.ErrorMessage) ?
                        generalResponse.ErrorMessage : validationResponse.ErrorMessage;
                model.PCRId = int.MinValue;
                _viewBag.LockScreen = true;
            }
            else
            {
                _viewBag.ErrorMessage = null;
                _viewBag.LockScreen = false;
                model.PCRId = model.PCREffectivenessViewModel.PCRId;
            }

            _viewBag.CurrentStage = model.PCREffectivenessViewModel.PCRStageType;
            _viewBag.OperationNumber = operationNumber;
            _viewBag.UserName = IDBContext.Current.UserName;
            _viewBag.StageName = model.PCREffectivenessViewModel.CurrentStageName;

            SetPermissions(operationNumber, model.PCREffectivenessViewModel.PCRStage);

            return model;
        }

        public HeaderViewModel GetHeaderViewModel(string operationNumber)
        {
            var response = _pcrChecklistService.GetHeader(operationNumber);
            if (!response.IsValid)
            {
                _viewBag.ErrorMessage = _viewBag.ErrorMessage ?? string.Empty + " " + response.ErrorMessage;
                _viewBag.LockScreen = true;
            }

            _viewBag.IsPCRRequired = response.Header == null ?
                false : response.Header.IsPCRRequired;

            return response.Header ?? (response.Header = new HeaderViewModel());
        }

        public List<SelectListItem> GetCategories()
        {
            return _viewBag.Categories =
                (from scoreCategory in _catalogService.GetPCRScoreCategories().ScoreCategoryViewModels
                 select new SelectListItem
                 {
                     Text = scoreCategory.Description,
                     Value = scoreCategory.Id
                 }).ToList();
        }

        public PCRFollowUpViewModel GetFollowUpHeader(string operationNumber)
        {
            var response = _pcrFollowUpService.GetFollowUpHeader(operationNumber);
            _viewBag.LockScreen = false;
            _viewBag.LockScreenWorkflowValidation = false;
            _viewBag.MsgInSeries = string.Empty;

            if (!response.IsValid)
            {
                _viewBag.ErrorMessage = response.ErrorMessage.ProcessStringForView();
                _viewBag.LockScreen = true;
                _viewBag.LockScreenWorkflowValidation = false;
            }

            _viewBag.WritePermission = true;

            if (response.FollowUpViewModel == null)
            {
                response.FollowUpViewModel = new PCRFollowUpViewModel
                {
                    Header = new PCRFollowUpHeaderViewModel(),
                    Roles = new List<PCRFollowUpRoleViewModel>(),
                    Tasks = new List<IPCRFollowUpTaskViewModel>(),
                    Documents = new List<PCRDocumentViewModel>()
                };
            }

            if (response.FollowUpViewModel.IsInSeries)
            {
                _viewBag.MsgInSeries = response.FollowUpViewModel.SeriesWarningMessage;
            }

            _viewBag.TaskList = response.FollowUpViewModel.Tasks.Select(task => new DataListItem
            {
                Order = task.TaskNumber,
                PartialViewName = GetPartialNameForTask(task),
                ViewModelData = GetViewModelForTask(task),
                Active = task.IsActive
            }).ToList();

            var task22 = response.FollowUpViewModel.Tasks.First(x => x.TaskNumber == 22);

            if (task22.CompletedDate.HasValue)
            {
                _viewBag.LockScreen = true;
                _viewBag.LockScreenWorkflowValidation = false;
            }

            _viewBag.Header = response.FollowUpViewModel.Header;
            SetPermissions(operationNumber, null);
            return response.FollowUpViewModel;
        }

        public PCRFollowUpViewModel GetFollowUp(string operationNumber)
        {
            var response = _pcrFollowUpService.GetFollowUp(operationNumber);
            _viewBag.LockScreen = false;
            _viewBag.LockScreenWorkflowValidation = false;
            _viewBag.MsgInSeries = string.Empty;
            _viewBag.operationNumber = operationNumber;
            _viewBag.CancelledOperation = response.CancelledOperation;

            if (!response.IsValid)
            {
                _viewBag.ErrorMessage = response.ErrorMessage.ProcessStringForView();
                _viewBag.LockScreen = true;
                _viewBag.LockScreenWorkflowValidation = false;
                _viewBag.No95 = response.ErrorCode == 1;
            }

            _viewBag.WritePermission = true;

            if (response.FollowUpViewModel == null)
            {
                response.FollowUpViewModel = new PCRFollowUpViewModel
                {
                    Header = new PCRFollowUpHeaderViewModel(),
                    Roles = new List<PCRFollowUpRoleViewModel>(),
                    Tasks = new List<IPCRFollowUpTaskViewModel>(),
                    Documents = new List<PCRDocumentViewModel>()
                };

                if (!response.CancelledOperation)
                {
                    response.FollowUpViewModel.MethodologyType = _pcrFollowUpService
                        .GetPCRMethodologyName(operationNumber);
                }
            }
            else
            {
                var task22 = response.FollowUpViewModel.Tasks.First(x => x.TaskNumber == 22);
                if (task22.IsCurrent && task22.CompletedDate.HasValue)
                {
                    _viewBag.LockScreen = true;
                    _viewBag.LockScreenWorkflowValidation = false;

                    response.FollowUpViewModel.Header.NumberofDaysDelayed = 0;
                    response.FollowUpViewModel.Header.PCRFollowUpStateName =
                        PCRFollowUpStateEnum.Status.ToString();
                    response.FollowUpViewModel.Header.PCRFollowUpTimelineStatusName =
                        PCRFollowUpStateEnum.Published.ToString();
                    response.FollowUpViewModel.Header.PCRFollowUpTimelineStatus =
                        PCRFollowUpTimelineStatusEnum.None;
                }
            }

            if (response.FollowUpViewModel.IsInSeries)
            {
                _viewBag.MsgInSeries = response.FollowUpViewModel.SeriesWarningMessage;
            }

            _viewBag.TaskList = response.FollowUpViewModel.Tasks.Select(task => new DataListItem
            {
                Order = task.TaskNumber,
                PartialViewName = GetPartialNameForTask(task),
                ViewModelData = GetViewModelForTask(task),
                Active = task.IsActive
            }).ToList();

            _viewBag.Header = response.FollowUpViewModel.Header;
            _viewBag.Documents = AddOptionsDocumentsDropDown(IDBContext.Current.CurrentLanguage);
            _viewBag.UserName = IDBContext.Current.UserName;
            _viewBag.IsPCRRequired = response.FollowUpViewModel.IsRequired;
            SetPermissions(operationNumber, null);

            return response.FollowUpViewModel;
        }

        public List<SelectListItem> AddOptionsDocumentsDropDown(string language)
        {
            var documentsList = new List<SelectListItem>();
            var descriptionsList = _catalogService
                .GetMasterDataListByTypeCode(true, typeCodes: PCRIDBDocumentList);

            if (descriptionsList != null)
            {
                documentsList.AddRange(descriptionsList.MasterDataCollection.Select(t => new SelectListItem
                {
                    Selected = false,
                    Text = MvcHelpers.GetItemName(t, language),
                    Value = t.MasterId.ToString()
                }));
            }

            return documentsList;
        }

        public void GetMonitoringReport()
        {
            var masterData = _catalogService.GetMasterDataListByTypeCode(
                false,
                MonitoringReportType.MonitoringStatus,
                MonitoringReportType.Operations,
                MonitoringReportType.Country,
                MonitoringReportType.SectorDepartment);

            if (!masterData.IsValid || masterData.MasterDataCollection == null)
            {
                Logger.GetLogger().WriteDebug("ViewModelMapperHelper", masterData.ErrorMessage);
                _viewBag.ErrorMessage = masterData.ErrorMessage;
                _viewBag.MonitoringStatusList = new List<SelectListItem>();
                _viewBag.ExerciseYearList = new List<SelectListItem>();
                _viewBag.DivisionGroupingList = new List<SelectListItem>();
                _viewBag.CountryDepartmentList = new List<SelectListItem>();
                _viewBag.SectorDepartmentList = new List<SelectListItem>();
                _viewBag.OperationsList = new List<SelectListItem>();
                _viewBag.CountryList = new List<SelectListItem>();
                _viewBag.DivisionList = new List<SelectListItem>();
            }
            else
            {
                var sectorDepartment = masterData.MasterDataCollection
                   .Where(m => m.TypeName == MonitoringReportType.SectorDepartment).ToList();

                _viewBag.MonitoringStatusList = GetListItems(
                    masterData.MasterDataCollection,
                    MonitoringReportType.MonitoringStatus,
                    IDBContext.Current.CurrentLanguage);
                _viewBag.ExerciseYearList = GetExerciseYear();
                _viewBag.DivisionGroupingList = GetDivisionGrouping();
                _viewBag.OperationsList = GetListItems(
                    masterData.MasterDataCollection,
                    MonitoringReportType.Operations,
                    IDBContext.Current.CurrentLanguage);
                _viewBag.CountryList = GetListItems(
                    masterData.MasterDataCollection,
                    MonitoringReportType.Country,
                    IDBContext.Current.CurrentLanguage);

                //Country Departments
                var countryDepartments = _catalogService.GetCountryDepartments(
                    masterData.MasterDataCollection
                        .Where(m => m.TypeName == MonitoringReportType.Country).ToList());

                if (countryDepartments.IsValid)
                {
                    _viewBag.CountryDepartmentList = GetListItemsCountryDepartments(
                        countryDepartments.CountryDepartments,
                        IDBContext.Current.CurrentLanguage);
                }

                //SectorDepartments
                var sector = _catalogService.GetSectorDepartments(sectorDepartment);

                if (sector.IsValid)
                {
                    _viewBag.SectorDepartmentList = GetListItemsCode(sector.SectorDepartments);
                }

                //Division
                var division = _catalogService.GetDivision(sectorDepartment);

                if (division.IsValid)
                {
                    _viewBag.DivisionList = GetListItemsCode(division.Division);
                }
            }
        }

        public FilterOptionsReportViewModel SetFilterOptions(
            string monitoringStatus,
            string exerciseYear,
            string divisionGrouping,
            string countryDepartment,
            string sectorDepartment,
            string operations,
            string country,
            string division,
            bool allCountries,
            bool allMonitoringStatus,
            bool allCountryDepartment,
            bool allSectorDepartment,
            bool allDivision)
        {
            if (monitoringStatus == AllValuesSelector ||
                countryDepartment == AllValuesSelector ||
                sectorDepartment == AllValuesSelector ||
                country == AllValuesSelector ||
                division == AllValuesSelector)
            {
                GetMonitoringReport();

                if (monitoringStatus == AllValuesSelector)
                {
                    monitoringStatus = string
                        .Join(",", ((List<SelectListItem>)_viewBag.MonitoringStatusList)
                            .Select(i => i.Value));
                }

                if (countryDepartment == AllValuesSelector)
                {
                    countryDepartment = string
                        .Join(",", ((List<SelectListItem>)_viewBag.CountryDepartmentList)
                            .Select(i => i.Value));
                }

                if (sectorDepartment == AllValuesSelector)
                {
                    sectorDepartment = string
                        .Join(",", ((List<SelectListItem>)_viewBag.SectorDepartmentList)
                            .Select(i => i.Value));
                }

                if (country == AllValuesSelector)
                {
                    country = string
                        .Join(",", ((List<SelectListItem>)_viewBag.CountryList)
                            .Select(i => i.Value));
                }

                if (division == AllValuesSelector)
                {
                    division = string
                        .Join(",", ((List<SelectListItem>)_viewBag.DivisionList)
                            .Select(i => i.Value));
                }
            }

            return new FilterOptionsReportViewModel
            {
                PCRMonitoringStatus = monitoringStatus.Split(',').ToList(),
                ExerciseYear = exerciseYear.Split(',').ToList(),
                DivisionGrouping = divisionGrouping,
                CountryDepartment = countryDepartment.Split(',').ToList(),
                SectorDepartment = sectorDepartment.Split(',').ToList(),
                OperationInASeriesIndividual = operations == "null" ? string.Empty : operations,
                Country = country.Split(',').ToList(),
                Division = division.Split(',').ToList(),
                AllMonitoringStatus = allMonitoringStatus,
                AllCountries = allCountries,
                AllCountryDepartment = allCountryDepartment,
                AllSectorDepartment = allSectorDepartment,
                AllDivision = allDivision
            };
        }

        public DownloadMonitoringReportResponse FileReport(
            string monitoringStatus,
            string exerciseYear,
            string divisionGrouping,
            string countryDepartment,
            string sectorDepartment,
            string operations,
            string country,
            string division,
            bool allCountries,
            bool allMonitoringStatus,
            bool allCountryDepartment,
            bool allSectorDepartment,
            bool allDivision,
            OutputFormatEnum fileFormat)
        {
            var paramsReport = SetFilterOptions(
                monitoringStatus,
                exerciseYear,
                divisionGrouping,
                countryDepartment,
                sectorDepartment,
                operations,
                country,
                division,
                allCountries,
                allMonitoringStatus,
                allCountryDepartment,
                allSectorDepartment,
                allDivision);

            return _pcrFollowUpService.DownloadMonitoringReport(paramsReport, fileFormat);
        }

        public bool IsOldMethodology(string operationNumber)
        {
            return _pcrFollowUpService.GetPCRMethodologyCode(operationNumber) == PCRConstants.METHODOLOGY_OLD;
        }

        #region Private Methods

        private static string GetSelectedItemText(
            IEnumerable<MultiSelectListItem> items,
            bool includeParent,
            bool ignoreFirstLevel)
        {
            var textBuffer = new StringBuilder();
            MultiSelectListItem previousParent = null;
            var orderedList = items.OrderBy(
                s => s.Parent != null &&
                    ((ignoreFirstLevel && !s.Parent.IsFirstLevel) || !ignoreFirstLevel) ?
                    s.Parent.Text.Substring(0, 4) + ": " : string.Empty + s.Text.Substring(0, 4)).ToList();
            var firstTime = true;

            foreach (var item in orderedList)
            {
                item.Selected = true;

                var text = string.Empty;
                if ((includeParent && item.Parent != null) &&
                    ((ignoreFirstLevel && !item.Parent.IsFirstLevel) || !ignoreFirstLevel))
                {
                    if (previousParent != item.Parent)
                    {
                        if (!firstTime)
                        {
                            text = "\n";
                        }

                        firstTime = false;
                        previousParent = item.Parent;
                        text = text + item.Parent.Text + ": ";
                    }

                    text += item.Text;
                }
                else
                {
                    text = item.Text;
                }

                textBuffer.AppendFormat("{0}, ", text);
            }

            if (textBuffer.Length > 0)
            {
                textBuffer.Length = textBuffer.Length - 2;
            }

            return textBuffer.ToString();
        }

        private static List<SelectListItem> GetExerciseYear()
        {
            var currentDate = DateTime.Now;
            var endYear = currentDate.AddYears(1).Year;

            var listYear = new List<SelectListItem>();

            for (var i = StartYear; i <= endYear; i++)
            {
                var year = new SelectListItem
                {
                    Selected = i == currentDate.Year,
                    Text = i.ToString(),
                    Value = i.ToString()
                };

                listYear.Add(year);
            }

            return listYear;
        }

        private static List<SelectListItem> GetListItems(
            IEnumerable<MasterDataViewModel> masterData,
            string type,
            string language)
        {
            if (type == MonitoringReportType.Country)
                masterData = masterData.Where(x => x.Code != CountryCode.UND && x.Code != CountryCode.IDB
                    && (!x.ExpirationDate.HasValue || x.ExpirationDate.Value > DateTime.Today)
                    && x.TypeName == type);
            return (from item in masterData
                    where item.TypeName == type
                    select new SelectListItem
                    {
                        Selected = false,
                        Text = MvcHelpers.GetItemName(item, language),
                        Value = item.MasterId.ToString(),
                        Group = new SelectListGroup
                        {
                            Name = item.ParentMasterId.ToString()
                        }
                    }).ToList();
        }

        private static List<SelectListItem> GetListItemsCode(List<MasterDataViewModel> masterData)
        {
            var listItem = (from item in masterData
                            select new SelectListItem
                            {
                                Selected = false,
                                Text = item.Code,
                                Value = item.MasterId.ToString(),
                                Group = new SelectListGroup
                                {
                                    Name = item.ParentMasterId.ToString()
                                }
                            }).ToList();

            return listItem;
        }

        private static List<SelectListItem> GetListItemsCountryDepartments(
            List<ConvergenceMasterData> countryDepartments,
            string language)
        {
            var listItem = (from item in countryDepartments
                            select new SelectListItem
                            {
                                Selected = false,
                                Text = MvcHelpers.GetItemName(item, language),
                                Value = item.ConvergenceMasterDataId.ToString()
                            }).ToList();

            return listItem;
        }

        private static List<SelectListItem> GetDivisionGrouping()
        {
            return new List<SelectListItem>
            {
                new SelectListItem
                {
                    Selected = false,
                    Text = Localization.GetText(Yes),
                    Value = "1",
                    Group = new SelectListGroup()
                },
                new SelectListItem
                {
                    Selected = false,
                    Text = Localization.GetText(No),
                    Value = "0",
                    Group = new SelectListGroup()
                }
            }.ToList();
        }

        private static string GetPartialNameForTask(IPCRFollowUpTaskViewModel task)
        {
            switch (task.TaskNumber)
            {
                case 2:
                case 8:
                    return "Partials/DataListViews/PCRExtensionTaskDataItem";
                case 12:
                    return "Partials/DataListViews/PCRTask12DataItem";
                case 20:
                    return "Partials/DataListViews/PCRTask20DataItem";
                default:
                    return "Partials/DataListViews/PCRDefaultTaskDataItem";
            }
        }

        private static dynamic GetViewModelForTask(IPCRFollowUpTaskViewModel task)
        {
            switch (task.TaskNumber)
            {
                case 2:
                case 8:
                    return (PCRFollowUpTaskExtensionViewModel)task;
                case 12:
                    return (PCRFollowUpTask12ViewModel)task;
                case 20:
                    return (PCRFollowUpTask20ViewModel)task;
                default:
                    return (PCRFollowUpTaskBaseViewModel)task;
            }
        }

        private void SetPermissions(string operationNumber, PCRStageEnum? currentStage)
        {
            _viewBag.SPDRole = _authorizationService.IsAuthorized(
                IDBContext.Current.UserLoginName,
                operationNumber,
                ActionEnum.PCRSPDLeaderWrite,
                true);
            _viewBag.SPDCoord = IDBContext.Current.HasRole(Role.SPD_COORDINATOR);
            _viewBag.TeamRole = _authorizationService.IsAuthorized(
                IDBContext.Current.UserLoginName,
                operationNumber,
                ActionEnum.PCRTeamLeaderWrite,
                true);
            _viewBag.ReadRole = _authorizationService.IsAuthorized(
                IDBContext.Current.UserLoginName,
                operationNumber,
                ActionEnum.ConvergenceReadPermission,
                true);
            _viewBag.FollowUpActivationRole = _authorizationService.IsAuthorized(
                IDBContext.Current.UserLoginName,
                operationNumber,
                ActionEnum.PCRFollowUpActivationWritePermission,
                true);
            _viewBag.PCRRequiredRole = _authorizationService.IsAuthorized(
                IDBContext.Current.UserLoginName,
                operationNumber,
                ActionEnum.PCRRequiredWritePermission,
                true);
            _viewBag.FollowUpWriteRole = _authorizationService.IsAuthorized(
                IDBContext.Current.UserLoginName,
                operationNumber,
                ActionEnum.PCRFollowUpWritePermission,
                true);

            if (currentStage.HasValue)
            {
                _viewBag.WritePermissionStage =
                    (_viewBag.SPDRole &&
                        (currentStage.Value == PCRStageEnum.SPDPreValidationStageUnderReview ||
                        currentStage.Value == PCRStageEnum.SPDValidationStageUnderReview)) ||
                    (_viewBag.SPDRole &&
                        (currentStage.Value == PCRStageEnum.TeamLeaderPreValidationStageReviewed ||
                        currentStage.Value == PCRStageEnum.TeamLeaderValidationStageReviewed)) ||
                    (_viewBag.TeamRole &&
                        (currentStage.Value == PCRStageEnum.TeamLeaderPreValidationStageUnderReview ||
                        currentStage.Value == PCRStageEnum.TeamLeaderValidationStageUnderReview) ||
                        (_viewBag.TeamRole && (currentStage.Value == PCRStageEnum.SPDPreValidationStageReviewed)) ||
                        (_viewBag.TeamRole && (currentStage.Value == PCRStageEnum.None)));

                if (IDBContext.Current.IsLocalEnvironment)
                    _viewBag.WritePermissionStage = true;

                _viewBag.WritePermissionValidations =
                    currentStage.Value != PCRStageEnum.SPDValidationStageReviewed &&
                    (_viewBag.SPDRole || _viewBag.TeamRole);
            }
        }

        #endregion
    }
}