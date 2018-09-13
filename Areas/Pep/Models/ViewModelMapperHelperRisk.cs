using System.Collections.Generic;
using System.Linq;
using IDB.MW.Application.PEPModule.Services.Risk;
using ViewModelRisk = IDB.MW.Application.PEPModule.ViewModels;

namespace IDB.Presentation.MVC4.Areas.Pep.Models
{
    public class ViewModelMapperHelperRisk
    {
        private readonly IRiskService _pepRisk;

        public ViewModelMapperHelperRisk(IRiskService pepRisk)
        {
            _pepRisk = pepRisk;
        }

        public List<RiskViewModel> ComboRiskResponse(IList<ViewModelRisk.RiskViewModel> riskCollection)
        {
            return riskCollection.Select(rvm => new RiskViewModel
            {
                RiskId = rvm.RiskId,
                RiskName = rvm.RiskName
            }).ToList();
        }
    }
}