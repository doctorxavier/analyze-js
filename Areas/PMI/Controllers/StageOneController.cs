using System.Linq;
using System.Web.Mvc;
using System.Configuration;

using IDB.MW.Domain.Contracts.ModelRepositories.Supervision.PMI;
using IDB.MW.Domain.Models.Supervision.PMI;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Controllers;
using IDB.Architecture.Cache;
using IDB.MW.Domain.EntityHelpers;
using IDB.Presentation.MVC4.Areas.PMI.Helpers;

namespace IDB.Presentation.MVC4.Areas.PMI.Controllers
{
    public partial class StageOneController : BaseController
    {
        private int _timeCachingVal = int.Parse(ConfigurationManager.AppSettings["CacheMediumTime"].ToString());

        #region wcf services repositories
        private IPMIDetailsModelRepository _clientPMIDetails = null;
        private ICacheManagement _cacheData = null;
        #endregion

        public StageOneController(IPMIDetailsModelRepository clientPMIDetails, ICacheManagement cacheData)
        {
            _clientPMIDetails = clientPMIDetails;
            _cacheData = cacheData;
        }

        public void LoadFromApprovalToLegalEffectiveness(PMIDetailsModel pmi)
        {
            var transformed = new
            {
                MaxLevel = pmi.PmiEscalarBenchmark.ListPMIEscalar
                .Where(x => x.PmiTypeId == pmi.MasterToEFF)
                .Select(x => x.MaxLevel)
                .FirstOrDefault(),
                AlertLevel = pmi.PmiEscalarBenchmark.ListPMIEscalar
                .Where(x => x.PmiTypeId == pmi.MasterToEFF)
                .Select(x => x.AlertLevel)
                .FirstOrDefault(),
                ProblemLevel = pmi.PmiEscalarBenchmark.ListPMIEscalar
                .Where(x => x.PmiTypeId == pmi.MasterToEFF)
                .Select(x => x.ProblemLevel)
                .FirstOrDefault(),
                resultSum = pmi.PmiEscalarBenchmark.ListPMIEscalar
                .Where(x => x.PmiTypeId == pmi.MasterToEFF)
                .Select(x => x.ProblemLevel)
                .FirstOrDefault() - pmi.PmiEscalarBenchmark.ListPMIEscalar
                .Where(x => x.PmiTypeId == pmi.MasterToEFF)
                .Select(x => x.MaxLevel)
                .FirstOrDefault(),
                Efectiveness = pmi.LegalEfectiveness,
            };

            ViewBag.FromApprovalToLegalEffectiveness = transformed;
        }

        public void LoadFromApprovalToLegalEffectivenessToEligibility(PMIDetailsModel pmi)
        {
            var transformed = new
            {
                MaxLevel2 = pmi.PmiEscalarBenchmark.ListPMIEscalar
                .Where(x => x.PmiTypeId == pmi.MasterToELIG)
                .Select(x => x.MaxLevel)
                .FirstOrDefault(),
                AlertLevel2 = pmi.PmiEscalarBenchmark.ListPMIEscalar
                .Where(x => x.PmiTypeId == pmi.MasterToELIG)
                .Select(x => x.AlertLevel)
                .FirstOrDefault(),
                ProblemLevel2 = pmi.PmiEscalarBenchmark.ListPMIEscalar
                .Where(x => x.PmiTypeId == pmi.MasterToELIG)
                .Select(x => x.ProblemLevel)
                .FirstOrDefault(),
                Eligibility = pmi.ToEligibility,
            };

            ViewBag.FromApprovalToLegalEffectivenessToEligibility = transformed;
        }

        public void LoadNormalClausesPriorToEligibility(PMIDetailsModel pmi)
        {
            var transformed = new
            {
                DatesForTypeAndNormal = pmi.ForTypeAndNormal,
                DatesClausesIsNormalCompleted = pmi.PercentageNormal,
            };

            ViewBag.NormalClausesPriorToEligibility = transformed;
        }

        public void LoadSpecialClausesPriorToEligibility(PMIDetailsModel pmi)
        {
            var transformed = new
            {
                DatesForTypeAndSpecial = pmi.ForTypeAndSpecial,
                DatesClausesIsSpecial = pmi.PercentageSpecial,
            };

            ViewBag.SpecialClausesPriorToEligibility = transformed;
        }

        public virtual ActionResult Index(
            string operationNumber, bool calculateInMemory = true, bool isLive = false)
        {
            ViewBag.OperationNUmber = operationNumber;

            string cacheName = CacheHelper.GetCacheName(isLive, IDBContext.Current.Operation);

            var pmiOperation = _cacheData.Get<PMIOperation>(cacheName) ??
                _cacheData.Add(
                    cacheName,
                    _clientPMIDetails.LoadOperationContext(operationNumber, calculateInMemory, isLive),
                    _timeCachingVal);

            IDBContext.Current.ContextPMIOperation = pmiOperation;
            ViewData["OperationNumber"] = operationNumber;
            ViewBag.Tab = "S1G";

            PMIDetailsModel model = _clientPMIDetails.GetCurrentPMIEscalarBenchmark(
                operationNumber, pmiOperation, isLive);
            model.IsLiveMode = isLive;

            LoadFromApprovalToLegalEffectiveness(model);
            LoadFromApprovalToLegalEffectivenessToEligibility(model);
            LoadNormalClausesPriorToEligibility(model);
            LoadSpecialClausesPriorToEligibility(model);

            //TODO: The entire model should be sent to the view instead of using the viewbag
            return View(isLive);
        }

        public virtual ActionResult IndexSummaryTable(
            string operationNumber, bool calculateInMemory = true, bool isLive = false)
        {
            string cacheName = CacheHelper.GetCacheName(isLive, IDBContext.Current.Operation);

            var pmiOperation = _cacheData.Get<PMIOperation>(cacheName) ??
                _cacheData.Add(cacheName,
                _clientPMIDetails.LoadOperationContext(operationNumber, calculateInMemory, isLive),
                _timeCachingVal);

            var pmi = _clientPMIDetails.GetCurrentPMIEscalarBenchmark(operationNumber, pmiOperation, isLive);
            pmi.IsLiveMode = isLive;

            ViewBag.OperationType = OperationTypeHelper.GetOperationTypes(operationNumber);
            ViewBag.OperationNUmber = operationNumber;
            ViewBag.Tab = "S1T";

            return View(pmi);
        }

        public virtual ActionResult RenderSummaryTable(
            string operationNumber, bool calculateInMemory = true, bool isLive = false)
        {
            string cacheName = CacheHelper.GetCacheName(isLive, IDBContext.Current.Operation);

            var pmiOperation = _cacheData.Get<PMIOperation>(cacheName) ??
                _cacheData.Add(
                    cacheName,
                    _clientPMIDetails.LoadOperationContext(operationNumber, calculateInMemory, isLive),
                    _timeCachingVal);

            var pmi = _clientPMIDetails.GetCurrentPMIEscalarBenchmark(operationNumber, pmiOperation, isLive);
            pmi.IsLiveMode = isLive;

            ViewBag.OperationType = OperationTypeHelper.GetOperationTypes(operationNumber);

            ViewBag.OperationNUmber = operationNumber;
            ViewBag.Tab = "S1T";

            return View(pmi);
        }
    }
}