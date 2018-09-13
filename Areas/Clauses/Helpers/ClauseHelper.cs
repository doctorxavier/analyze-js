using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using IDB.MW.Domain.Models.K2Service;
using IDB.MW.Domain.Session;

namespace IDB.Presentation.MVC4.Areas.Clauses.Helpers
{
    public static class ClauseHelper
    {
        public static string GetUserName(OperationRelatedModelWorkflow callInstance)
        {
            return string.IsNullOrEmpty(callInstance.UserName)
                ? IDBContext.Current.UserName : callInstance.UserName;
        }
    }
}