using System.Web.Mvc;
using System.Configuration;
using System.Collections.Generic;

using IDB.Architecture.Language;
using IDB.MW.Domain.Contracts.ModelRepositories.Supervision.PMI;
using IDB.MW.Domain.Models.Supervision.PMI;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Controllers;
using IDB.Architecture.Cache;
using IDB.MW.Domain.EntityHelpers;
using IDB.MW.Domain.Values;
using IDB.Presentation.MVC4.Areas.PMI.Helpers;

namespace IDB.Presentation.MVC4.Areas.PMI.Controllers
{
    public partial class StageController : BaseController
    {
        private int _timeCachingVal = int.Parse(ConfigurationManager.AppSettings["CacheMediumTime"]);
        private IPMIDetailsModelRepository _clientPMIDetails = null;
        private ICacheManagement _cacheData = null;

        public static bool IsSpecialOperation(IList<string> opTypes)
        {
            return
                opTypes.Contains(OperationType.PBL) ||
                opTypes.Contains(OperationType.PBP) ||
                opTypes.Contains(OperationType.PDL) ||
                opTypes.Contains(OperationType.HIB);
        }

        public static bool IsSpecialOperation(int operationId)
        {
            IList<string> opTypes = OperationTypeHelper.GetOperationTypes(operationId);

            return IsSpecialOperation(opTypes);
        }

        public static bool IsSpecialOperationOrSUPWithoutRelation(
            int operationId, IList<string> opTypes)
        {
            bool isSuplementaryWithNoRelatedOperations = opTypes.Contains(OperationType.SUP) &&
                !RelationshipHelper.HasOperationRelatedOperations(operationId, OperationType.SUP);

            return IsSpecialOperation(opTypes) || isSuplementaryWithNoRelatedOperations;
        }

        public static bool IsSpecialOperationOrSUPWithoutRelation(int operationId)
        {
            IList<string> opTypes = OperationTypeHelper.GetOperationTypes(operationId);

            return IsSpecialOperationOrSUPWithoutRelation(operationId, opTypes);
        }

        public StageController(IPMIDetailsModelRepository clientPMIDetails, ICacheManagement cacheData)
        {
            _clientPMIDetails = clientPMIDetails;
            _cacheData = cacheData;
        }

        public virtual ActionResult Header(
            string operationNumber, int stage, bool calculateInMemory = true, bool isLive = false)
        {
            string cacheName = CacheHelper.GetCacheName(isLive, IDBContext.Current.Operation);

            var pmiOperation = _cacheData.Get<PMIOperation>(cacheName) ??
                _cacheData.Add(
                cacheName,
                _clientPMIDetails.LoadOperationContext(operationNumber, calculateInMemory, isLive),
                _timeCachingVal);

            ViewBag.firstDisbursement = pmiOperation.YearOfDisbursement;
            int stageTitle = pmiOperation.StageValue;

            var classification = pmiOperation.SyntethicIndicatorState;
            var pmrCycleName = pmiOperation.PmrCycleName;

            var operationTypes = OperationTypeHelper.GetOperationTypes(operationNumber);
            if (operationTypes.Contains(OperationType.PDL) ||
               operationTypes.Contains(OperationType.PBL) ||
               operationTypes.Contains(OperationType.PBP) ||
               operationTypes.Contains(OperationType.HIB) ||
                string.IsNullOrEmpty(classification))
            {
                classification = "N/A";
            }

            var basicPmiDetails = new BasicPMIDetailsModel
            {
                OperationId = pmiOperation.OperationId,
                OperationNumber = operationNumber,
                OperationTypes = operationTypes,
                LastStageValueAchieved = pmiOperation.StageValue,
                StageNumber = stage,
                Stage = stageTitle == 1 ?
                    Localization.GetText("From Approval to Eligibility") :
                    stageTitle == 2 ? Localization.GetText("After Eligibility") :
                    stageTitle == 3 ? Localization.GetText("After Operation Reaches 95% of total Disbursements") :
                    Localization.GetText("Validation process status"),
                SyntheticIndicator = (double)pmiOperation.SyntheticIndicator,
                LastValidatedClassification = pmiOperation.LastValidationClass,
                Classification = classification,
                CycleName = pmrCycleName,
                CycleId = pmiOperation.PmrCycleId,
                CycleYear = pmiOperation.CycleYear,
                CycleTypeCode = pmiOperation.CycleTypeCode,
                IsLiveMode = isLive,
                IsModelComplete = pmiOperation.IsModelComplete
            };

            HttpContext.Items["classification"] = basicPmiDetails.Classification;

            return PartialView("IndexSharedHeader", basicPmiDetails);
        }        
    }
}
