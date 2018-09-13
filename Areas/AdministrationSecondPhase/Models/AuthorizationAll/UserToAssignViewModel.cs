using System.Collections.Generic;
using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.Delegation
{
    public class UserToAssignViewModel :
        MW.Application.AdministrationModule.ViewModels.Delegation.UserToAssignViewModel
    {
        public IList<SelectListItem> DisplayReasons { get; set; }
        public string DisplayReason { get; set; }
        public IList<SelectListItem> DisplayTypes { get; set; }
    }
}