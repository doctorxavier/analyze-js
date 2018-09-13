using System;
using System.Collections.Generic;

using IDB.MW.Application.Core.ViewModels;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.AuthorizationAll
{
    public class AuthorizationAllTableViewModel
    {
        public List<AuthorizationAllTableRowViewModel> AuthorizationTable { get; set; }

        public IList<ListItemViewModel> DisplayOptions { get; set; }

        public IList<DateTime> UnavailableDates { get; set; }

        public bool IsAuthorize { get; set; }
    }
}