using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.MW.Application.PEPModule.Messages;
using IDB.MW.Application.PEPModule.Services;
using IDB.MW.Application.PEPModule.ViewModels;
using IDB.MW.Domain.Session;

namespace IDB.Presentation.MVC4.Areas.Pep.Models
{
    public class ViewModelMapperHelper
    {
        private readonly IPEPService _pepService;

        public ViewModelMapperHelper(IPEPService pepService)
        {
            _pepService = pepService;
        }

        public PepIndexViewModel PepIndexViewModel(GeneralInformationResponse getInformationGeneral)
        {
            var viewModel = new PepIndexViewModel
            {
                ErrorMessage = string.Empty,
                IsValid = true,
                Holidays = getInformationGeneral.HolidaysCountry.Holidays,
                CurrDisExpDate = getInformationGeneral.CurrentDisbursementExpiration.CurrDisExpDate,
                ResultsMatrixId = getInformationGeneral.ResultsMatrixId,
                PepTaskBucketId = getInformationGeneral.PepTaskBucketId,
                HasApproval = getInformationGeneral.Approval,
                HasStartUp = getInformationGeneral.StartUp,
                IsClosedOrCancelled = getInformationGeneral.IsClosedOrCancelled,
                FinanciesOthersTranches = getInformationGeneral.FinanceTranche,
                User = IDBContext.Current.UserName,
                CountryId = getInformationGeneral.CountryId,
                ExecutingAgencyId = IDBContext.Current.UserExternal.IsUserExternal ? 
                                    IDBContext.Current.UserExternal.IdInstitution : 
                                    getInformationGeneral.ExcecutingAgencyId,
                ExistContracts = getInformationGeneral.ContractDifExist,
                HideApprovalNumber = getInformationGeneral.HideApprovalNumber,
                NameExternalAgency = IDBContext.Current.UserExternal.NameInstitution,
                IsExternal = IDBContext.Current.UserExternal.IsUserExternal
            };

            List<TaskType> listTaskType = new List<TaskType>();
            TaskType itemList = null;

            foreach (var item in getInformationGeneral.TaskType)
            {
                itemList = new TaskType();
                itemList.IdTypeTask = item.IdTypeTask;
                itemList.CodeTypeTask = item.CodeTypeTask;
                itemList.NameEn = item.NameEn;
                itemList.NameEs = item.NameEs;
                itemList.NameFr = item.NameFr;
                itemList.NamePt = item.NamePt;
                listTaskType.Add(itemList);
            }

            viewModel.TaskType = listTaskType;

            viewModel.Permission = IDBContext.Current.Permissions;

            viewModel.Roles = IDBContext.Current.UserExternal.IsUserExternal ? 
                              IDBContext.Current.UserExternal.Role.ToList() : 
                              IDBContext.Current.Roles;
            List<TaskType> procurementStatus = new List<TaskType>();
            foreach (var procStatus in getInformationGeneral.ProcurementStatus)
            {
                procurementStatus.Add(new TaskType
                {
                    CodeTypeTask = procStatus.CodeTypeTask,
                    IdTypeTask = procStatus.IdTypeTask,
                    NameEn = procStatus.NameEn,
                    NameEs = procStatus.NameEs,
                    NameFr = procStatus.NameFr,
                    NamePt = procStatus.NamePt
                });
            }

            List<TaskType> activitiesStatus = new List<TaskType>();
            foreach (var actStatus in getInformationGeneral.ActivityStatus)
            {
                activitiesStatus.Add(new TaskType
                {
                    CodeTypeTask = actStatus.CodeTypeTask,
                    IdTypeTask = actStatus.IdTypeTask,
                    NameEn = actStatus.NameEn,
                    NameEs = actStatus.NameEs,
                    NameFr = actStatus.NameFr,
                    NamePt = actStatus.NamePt
                });
            }

            List<TaskType> pepBucketStatus = new List<TaskType>();
            foreach (var bucketStatus in getInformationGeneral.PepBucketStatus)
            {
                pepBucketStatus.Add(new TaskType
                {
                    CodeTypeTask = bucketStatus.CodeTypeTask,
                    IdTypeTask = bucketStatus.IdTypeTask,
                    NameEn = bucketStatus.NameEn,
                    NameEs = bucketStatus.NameEs,
                    NameFr = bucketStatus.NameFr,
                    NamePt = bucketStatus.NamePt
                });
            }

            viewModel.ProcurementStatus = procurementStatus;

            viewModel.ActivityStatus = activitiesStatus;

            viewModel.PepBucketStatus = pepBucketStatus;

            viewModel.StatusList = getInformationGeneral.StatusList;

            viewModel.HasSingleNotDraftBucket = getInformationGeneral.HasSingleNotDraftBucket;

            viewModel.CurrentApprovedAmount = getInformationGeneral.CurrentApprovedAmount;

            viewModel.PreparationLocalCounterPart = getInformationGeneral.PreparationLocalCounterPart;

            viewModel.PreparationCofinancing = getInformationGeneral.PreparationCofinancing;

            viewModel.PmrCycleCode = getInformationGeneral.PmrCycleCode;

            viewModel.Interval = getInformationGeneral.Interval;

            viewModel.PmrCycleYear = getInformationGeneral.PmrCycleYear;

            return viewModel;
        }

        public List<BasicDataPresetationViewModel> BasicDataViewModel(IList<BasicDataViewModel> basicDataViewModels)
        {
            return basicDataViewModels.Select(basicData => new BasicDataPresetationViewModel
            {
                PepTaskBucket = basicData.PepTaskBucket,
                ApprovalNumber = basicData.ApprovalNumber,
                ApprovalNumberId = basicData.ApprovalNumberId,
                ExecutingAgency = basicData.ExecutingAgency,
                ExecutingAgencyId = basicData.ExecutingAgencyId,
                StatusId = basicData.StatusId,
                Status = basicData.Status,
                LastNonObjectionDate = basicData.LastNonObjectionDate,
                LastNonObjectionBy = basicData.LastNonObjectionBy
            }).ToList();
        }

        public List<ApprovalCurrenciesViewModel> ApprovalCurrenciesViewModel(
            IList<ApprovalCurrencyViewModel> approvalCurrencyViewModel)
        {
            return approvalCurrencyViewModel.Select(approval => new ApprovalCurrenciesViewModel
            {
                ApprovalNumber = approval.ApprovalNumber,
                CurrencyId = approval.CurrencyId,
                Currency = approval.Currency,
                ExchangeRateUsd = approval.ExchangeRateUsd
            }).ToList();
        }

        public ComboBoxYearViewModel ComboBoxYearViewModel(IList<SelectListItem> comboBoxYear)
        {
            var viewModel = new ComboBoxYearViewModel
            {
                ComboBoxYear = comboBoxYear
            };
            return viewModel;
        }

        public ListFinancialProgressViewModel ConvertToFinancialProgressList(RowFinancialProgressViewModel responseAplication)
        {
            ListFinancialProgressViewModel response = new ListFinancialProgressViewModel
            {
                IsValid = true
            };
            response.CurrentTotalCost = responseAplication.SumTotalCost;
            response.ListFinancialProgress = new List<FinancialProgressViewModel>();
            FinancialProgressViewModel pepTask = null;
            MonthFinancialProgressViewModel month = new MonthFinancialProgressViewModel();

            for (int i = 0; i < responseAplication.ListFinancialProgress.Count; i++)
            {
                pepTask = new FinancialProgressViewModel();
                pepTask.MonthFinancialProgress = new List<MonthFinancialProgressViewModel>();
                pepTask.FinancialYear = responseAplication.ListFinancialProgress[i].FinancialYear;
                pepTask.PepTaskId = responseAplication.ListFinancialProgress[i].PepTaskId;
                pepTask.Total = responseAplication.ListFinancialProgress[i].Total;
                pepTask.TotalIdb = responseAplication.ListFinancialProgress[i].TotalIdb;
                pepTask.TotalLocalCounterpart = responseAplication.ListFinancialProgress[i].TotalLocalCounterpart;
                pepTask.TotalCoFinancing = responseAplication.ListFinancialProgress[i].TotalCoFinancing;
                pepTask.TotalCostTemp = responseAplication.ListFinancialProgress[i].TotalCostTemp;
                pepTask.TaskColorFinancial = responseAplication.ListFinancialProgress[i].TaskColorFinancial;
                pepTask.Item = responseAplication.ListFinancialProgress[i].Item;
                pepTask.SumSourceIDB = responseAplication.ListFinancialProgress[i].SumSourceIDB;
                pepTask.SumSourceLC = responseAplication.ListFinancialProgress[i].SumSourceLC;
                pepTask.SumSourceCO = responseAplication.ListFinancialProgress[i].SumSourceCO;
                IList<IDB.MW.Application.PEPModule.ViewModels.MonthFinancialProgressViewModel> monthAplication = responseAplication.ListFinancialProgress[i].MonthFinancialProgress;

                for (int a = 0; a < monthAplication.Count; a++)
                {
                    month = new MonthFinancialProgressViewModel();
                    month.Month = monthAplication[a].Month;
                    month.Idb = monthAplication[a].Idb;
                    month.CoFinancing = monthAplication[a].CoFinancing;
                    month.LocalCounterpart = monthAplication[a].LocalCounterpart;
                    pepTask.MonthFinancialProgress.Add(month);
                }

                response.ListFinancialProgress.Add(pepTask);
            }

            return response;
        }

        public List<ChangeLogDataViewModel> ChangeLogViewModel(IList<ChangeLogViewModel> changeLogViewModel)
        {
            return changeLogViewModel.Select(changeLog => new ChangeLogDataViewModel
            {
                Id = changeLog.Id,
                ExecutingAgencyName = changeLog.ExecutingAgencyName,
                Version = changeLog.Version,
                Date = changeLog.Date != null ? changeLog.Date.ToString() : string.Empty,
                User = changeLog.User,
                Action = changeLog.Action,
                Comment = changeLog.Comment
            }).ToList();
        }

        public List<ChangeLogDetailHistoryViewModel> ConvertChangeLogHistoryViewModel(IList<ChangeLogHistoryViewModel> changeLogHistViewModel)
        {
            return changeLogHistViewModel.Select(changeLogHist => new ChangeLogDetailHistoryViewModel
            {
                ElementName = changeLogHist.ElementName,
                ElementType = changeLogHist.ElementType,
                ChangeType = changeLogHist.ChangeType,
                DateModification = changeLogHist.DateModification.Value.ToShortDateString(),
                ValueChanged = changeLogHist.ValueChanged,
                PreviousValue = changeLogHist.PreviousValue,
                NewValue = changeLogHist.NewValue
            }).ToList();
        }
        
        public List<CommentPepViewModel> ConvertCommentPepViewModel(IList<RowTablePepUserCommentViewModel> commentPepResponse)
        {
            var response = new List<CommentPepViewModel>();

            foreach (var comment in commentPepResponse)
            {
                var commentViewModel = new CommentPepViewModel
                {
                    CommentId = comment.CommentId,
                    Comment = comment.Comment,
                    CommentDate = comment.DateComment.ToString(),
                    CommentUser = comment.UserComment,
                    PepTaskId = comment.PepTaskId
                };

                response.Add(commentViewModel);
            }

            return response;
        }
        
        public List<ProcurementMethodModel> ProcurementMethodResponse(IList<ProcurementMethodViewModel> executingList)
        {
            return executingList.Select(ae => new ProcurementMethodModel
            {
                ProcurementTypeId = ae.ProcurementTypeId,
                ProcurementMethodId = ae.ProcurementMethodId,
                MethodName = ae.MethodName
            }).ToList();
        }

        public List<SupervisionMethodModel> ComboSupervisionMethodResponse(IList<SupervisionMethodViewModel> executingList)
        {
            return executingList.Select(ae => new SupervisionMethodModel
            {
                SupervisionMethodId = ae.SupervisionMethodId,
                SupervisionMethodName = ae.SupervisionMethodName,
                ProcurementMethodId = ae.ProcurementMethodId,
                ProcurementTypeId = ae.ProcurementTypeId,
                MinLimit = ae.MinLimit,
                MaxLimit = ae.MaxLimit
            }).ToList();
        }

        public List<ApprovalNumberViewModel> ConvertToComboContract(IList<ApprovalViewModel> approvalNumber)
        {
            return approvalNumber.Select(an => new ApprovalNumberViewModel
            {
                ApprovalId = an.ApprovalId,
                ApprovalName = an.ApprovalName,
                ExchangeRateUsd = an.ExchangeRateUsd,
                ExecutorAcrNm = an.ExecutorAcrNm.Trim()
            }).ToList();
        }

        public IList<SumDifFunds> ConvertToDifFundsTotal(IList<ContractFundViewModel> contractFund)
        {
            return contractFund.Select(funds => new SumDifFunds
            {
                ContractNumber = funds.ContractNumber,
                CurrentApprovalAmount = funds.CurrentApprovedAmount,
                TotalEop = funds.SumTotalEop,
                DiffAmount = funds.DifAmount,
                ExecutedAmount = funds.ExecutedAmount
            }).ToList();
        }

        public List<VerifyContentModel> ConvertToVerifyContentViewModel(IList<VerifyContentViewModel> verifyContentList)
        {
            return verifyContentList.Select(vc => new VerifyContentModel
            {
                Code = vc.Code,
                Description = vc.Description
            }).ToList();
        }
    }
}