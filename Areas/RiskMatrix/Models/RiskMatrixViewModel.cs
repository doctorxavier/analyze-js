using System.Collections.Generic;

using IDB.Architecture.Security.Models.UserIdentity;
using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Application.RiskMatrix.ViewModels;
using IDB.Presentation.MVC4.Areas.RiskMatrix.Models;
using IDB.Presentation.MVC4.Models.Security;

namespace IDB.Presentation.MVC4.Areas.RisksMatrix.Models
{
    public class RiskMatrixViewModel
    {
        public RiskMatrixViewModel()
        {
            Risks = new List<RiskViewModel>();
            UserComments = new List<MW.Application.RiskMatrix.ViewModels.UserCommentViewModel>();
            Documents = new List<DocumentViewModel>();           
            HeaderInfo = new RiskMatrixHeaderViewModel();
        }

        public string ErrorMessage { get; set; }
        public bool IsEdit { get; set; }
        public string OperationNumber { get; set; }
        public IList<RiskViewModel> Risks { get; set; }
        public IList<MW.Application.RiskMatrix.ViewModels.UserCommentViewModel> UserComments { get; set; }
        public IList<DocumentViewModel> Documents { get; set; }
        public IEnumerable<MultiSelectListItemViewModel> OutPuts { get; set; }
        public IEnumerable<MultiSelectListItemViewModel> OutComes { get; set; }
        public IEnumerable<MultiSelectListItemViewModel> RiskManagementStrategy { get; set; }
        public IEnumerable<ListItemViewModel> RiskProbabilityFactor { get; set; }
        public IEnumerable<MultiSelectListItemViewModel> ImpactScope { get; set; }
        public IEnumerable<ListItemViewModel> RiskProbability { get; set; }
        public IEnumerable<ListItemViewModel> RiskStatus { get; set; }
        public IEnumerable<ListItemViewModel> RiskImpact { get; set; }
        public IEnumerable<ListItemViewModel> RiskType { get; set; }
        public IEnumerable<ListItemViewModel> TypeImpact { get; set; }
        public IEnumerable<ListItemViewModel> RiskFundingSource { get; set; }
        public IEnumerable<ListItemViewModel> ActivityStatus { get; set; }

        public RiskMatrixHeaderViewModel HeaderInfo { get; set; }
        public string SelectedRisk { get; set; }
        public int IdInactiveStatus { get; set; }
        public int IdAcceptStatus { get; set; }
        public string CodeGuid { get; set; }
        public int OperationRiskId { get; set; }
        public string StrAccept { get; set; }
        public int IdMaterialized { get; set; }
        public IList<ApplicationParametersViewModel> Param { get; set; }
        public int IdCompletedStatus { get; set; }
        public ScreenSecurityViewModel ScreenSecurity { get; set; }
        public bool IsEditableRiskMatrix { get; set; }
        public string CodeStatusHigh { get; set; }
        public string CodeStatusMediumHigh { get; set; }
        public string CodeStatusMediumLow { get; set; }
        public string CodeStatusLow { get; set; }
        public string CodeStatusMedium { get; set; }
        public string Message { get; set; }
    }
}