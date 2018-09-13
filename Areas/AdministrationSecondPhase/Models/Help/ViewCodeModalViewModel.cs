using System.Collections.Generic;
using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.Help
{
    public class ViewCodeModalViewModel
    {
        public IList<SelectListItem> ViewItems { get; set; }

        public string ViewCode { get; set; }
    }
}