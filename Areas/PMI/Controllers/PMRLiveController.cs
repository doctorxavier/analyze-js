using System.Configuration;
using System.Web.Mvc;
using System.Web.Caching;

using IDB.Architecture.Cache;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Contracts.ModelRepositories.Supervision.PMI;
using IDB.Presentation.MVC4.Controllers;
using IDB.MW.Domain.Models.Supervision.PMI;
using IDB.Presentation.MVC4.Areas.PMI.Helpers;

namespace IDB.Presentation.MVC4.Areas.PMI.Controllers
{
    public partial class PMRLiveController : BaseController
    {
        private ICacheManagement _cacheData = null;
        private IPMIDetailsModelRepository _clientPMIDetails = null;
        private int _timeCachingVal = int.Parse(
            ConfigurationManager.AppSettings["CacheMediumTime"]);

        public PMRLiveController(
            IPMIDetailsModelRepository clientPMIDetails, ICacheManagement cacheData)
        {
            _cacheData = cacheData;
            _clientPMIDetails = clientPMIDetails;
        }

        public virtual ActionResult Index(string operationNumber)
        {
            string cacheName = CacheHelper.GetCacheName(true, IDBContext.Current.Operation);

            var pmiOperation = _cacheData.Get<PMIOperation>(cacheName) ??
                _cacheData.Add(
                    cacheName,
                    _clientPMIDetails.LoadOperationContext(operationNumber, true, true),
                    _timeCachingVal);

            string controllerToRedirect = GetControllerNameFromLastestStageAchieved(
                pmiOperation.StageValue);

            return RedirectToAction(
                "Index",
                controllerToRedirect,
                new
                {
                    area = "PMI",
                    operationNumber = operationNumber,
                    isLive = true
                });
        }

        //This action method is useful during tests and UAT, to force fresh data
        public virtual ActionResult CleanCache(string operationNumber)
        {
            string cacheName = CacheHelper.GetCacheName(true, operationNumber);
            _cacheData.Remove(cacheName, CacheItemRemovedReason.Expired);

            return null;
        }

        string GetControllerNameFromLastestStageAchieved(int stage)
        {
            if (stage == 1)
            {
                return "StageOne";
            }

            if (stage == 2)
            {
                return "StageTwo";
            }

            return "StageThree";
        }
    }
}