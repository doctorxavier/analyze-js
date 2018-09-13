using System.Collections.Generic;

using IDB.MW.Application.RiskMatrix.ViewModels;
using IDB.MW.Application.Core.ViewModels;
using IDB.Presentation.MVC4.Models.Security;
using IDB.Architecture.Security.Models.UserIdentity;

namespace IDB.Presentation.MVC4.Areas.RiskMatrix.Models
{
    public class RiskMaterializedViewModel
    {
        public RiskMaterializedViewModel()
        {
            HeaderInfo = new RiskMatrixHeaderViewModel();
            MaterializedRisks = new List<MaterializedViewModel>();
            MaterializedCategories = new List<ConvergenceMasterDataViewModel>();
            MaterializedCategoryTitles = new List<ConvergenceMasterDataViewModel>();
        }

        public string ErrorMessage { get; set; }
        public bool IsEdit { get; set; }
        public string OperationNumber { get; set; }
        public RiskMatrixHeaderViewModel HeaderInfo { get; set; }
        public IList<MaterializedViewModel> MaterializedRisks { get; set; }
        public IEnumerable<ListItemViewModel> Stages { get; set; }
        public IList<ConvergenceMasterDataViewModel> MaterializedCategories { get; set; }
        public IList<ConvergenceMasterDataViewModel> MaterializedCategoryTitles { get; set; }
        public ScreenSecurityViewModel ScreenSecurity { get; set; }
    }
}