using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IDB.MW.Domain.Models.Security;

namespace IDB.Presentation.MVC4.Areas.PACI.Models
{
    public class SecurityViewModel
    {
        public string PageName { get; set; }
        public IList<FieldAccessModel> Security { get; set; }
    }
}