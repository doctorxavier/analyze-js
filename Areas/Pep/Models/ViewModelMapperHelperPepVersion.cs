using System.Collections.Generic;
using System.Linq;
using IDB.MW.Application.PEPModule.Services;
using ViewModelProcurementType = IDB.MW.Application.PEPModule.ViewModels;

namespace IDB.Presentation.MVC4.Areas.Pep.Models
{
    public class ViewModelMapperHelperPepVersion
    {
        private readonly IPEPService _pepService;

        public ViewModelMapperHelperPepVersion(IPEPService pepService)
        {
            _pepService = pepService;
        }

        public List<PepVersionViewModel> ComboPepVersionResponse(IList<ViewModelProcurementType.PepVersionViewModel> pepVersionList)
        {
            return pepVersionList.Select(pvl => new PepVersionViewModel
            {
                PepVersionId = pvl.PepVersionId,
                PepVersionName = pvl.PepVersionName
            }).ToList();
        } 
    }
}