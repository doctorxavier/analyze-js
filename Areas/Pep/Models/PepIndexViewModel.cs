using System;
using System.Collections.Generic;

namespace IDB.Presentation.MVC4.Areas.Pep.Models
{
    [Serializable]
    public class PepIndexViewModel
    {
        public IList<string> Holidays { get; set; }

        public DateTime? CurrDisExpDate { get; set; }

        public int ResultsMatrixId { get; set; }

        public int PepTaskBucketId { get; set; }

        public bool PepWrite { get; set; }

        public bool PepSubmit { get; set; }

        public IList<TaskType> TaskType { get; set; }

        public bool IsValid { get; set; }

        public string ErrorMessage { get; set; }

        public bool HasApproval { get; set; }

        public bool HasStartUp { get; set; }

        public bool IsClosedOrCancelled { get; set; }

        public bool FinanciesOthersTranches { get; set; }

        public decimal? TotalCostCurrentOriginal { get; set; }

        public string User { get; set; }

        public IList<string> Permission { get; set; }

        public IList<string> Roles { get; set; }

        public int CountryId { get; set; }

        public IList<TaskType> ProcurementStatus { get; set; }

        public IList<TaskType> ActivityStatus { get; set; }

        public IList<TaskType> PepBucketStatus { get; set; }

        public int? ExecutingAgencyId { get; set; }

        public bool ExistContracts { get; set; }

        public bool HideApprovalNumber { get; set; }

        public string OperationNumber { get; set; }

        public bool IsExternal { get; set; }

        public string NameExternalAgency { get; set; }

        public string[] StatusList { get; set; }

        public bool HasSingleNotDraftBucket { get; set; }

        public decimal? CurrentApprovedAmount { get; set; }

        public decimal? PreparationLocalCounterPart { get; set; }

        public decimal? PreparationCofinancing { get; set; }

        public string PmrCycleCode { get; set; }

        public int Interval { get; set; }

        public int PmrCycleYear { get; set; }
    }
}