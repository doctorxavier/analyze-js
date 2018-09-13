using System.Collections.Generic;

using IDB.MW.Application.AdministrationModule.ViewModels.Delegation;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.AuthorizationAll
{
    public class AuthorizationAllViewModel
    {
        public string UserName { get; set; }
        public AuthorizationAllTableViewModel AuthorizationAllView { get; set; }

        public List<TableDocumentViewModel> TableDocument { get; set; }

        public bool IsAdmin { get; set; }

        public UserToAssignViewModel UserToAssign { get; set; }

        public int DelegationId { get; set; }

        public List<CommentDelegationViewModel> CommentsDelegation { get; set; }
        
        public DelegationFilterViewModel Filter { get; set; }

        public string OtherReasonId { get; set; }

        public bool IsEditable { get; set; }
    }
}