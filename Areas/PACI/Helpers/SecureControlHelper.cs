using System.Collections.Generic;
using System.Linq;

using IDB.MW.Domain.Models.Security;

namespace IDB.Presentation.MVC4.Areas.PACI.Helpers
{
    public static class SecureControlHelper
    {
        public static bool IsAuthorizedControl(string pageName, string fieldName, IList<FieldAccessModel> controls)
        {
            return controls.Any(c => c.FieldName == fieldName && c.Page == pageName);
        }
    }
}