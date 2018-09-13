using System;

using IDB.Architecture.Cache;

namespace IDB.Presentation.MVC4.Areas.PMI.Helpers
{
    public static class CacheHelper
    {
        public static string GetCacheName(bool isLive, string operationNumber)
        {
            if (string.IsNullOrEmpty(operationNumber))
                throw new ArgumentNullException("Error getting cache name for null operation number");

            if (isLive)
                return string.Format(CacheNames.PMI_LIVE, operationNumber);

            return string.Format(CacheNames.PMI_CYCLE, operationNumber);
        }
    }
}