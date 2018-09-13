using System.Collections.Generic;
using System.Web.Mvc;

using IDB.MW.Domain.Models.ActivityProgram;
using IDB.MW.Application.ActivityProgram.ViewModels;
using IDB.MW.Application.ActivityProgram.Messages;

namespace IDB.Presentation.MVC4.Areas.ActivityProgram.Models
{
    public class AAPViewModel
    {
        public DataUserModel DataUser { get; set; }

        public IEnumerable<SelectListItem> YearsDropDwn { get; set; }

        public string YearNowDefault { get; set; }

        public IEnumerable<SelectListItem> DisplayedDropDwn { get; set; }

        public string DisplayDefault { get; set; }

        public string PaginationDefault { get; set; }

        public IList<AnnualActivityModel> ActOp { get; set; }

        public string CurrentYear { get; set; }

        public bool AapTeamMemberRead { get; set; }

        public bool AapTeamMemberWhite { get; set; }

        public bool AapGlobalManagementRead { get; set; }

        public bool AapGlobalManagementWhite { get; set; }

        public bool AapActivityAdmin { get; set; }

        public string OperationNumber { get; set; }

        public bool IsValid { get; set; }

        public string ErrorMessage { get; set; }

        public bool ErrorSap { get; set; }

        public bool ErrorConvergence { get; set; }

        public bool ReLoadView { get; set; }
        
        public SapInfoResponse SapInfo { get; set; }
    }
}