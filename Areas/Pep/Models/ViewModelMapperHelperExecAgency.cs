using System.Collections.Generic;
using System.Linq;

using IDB.MW.Application.PEPModule.Services.ExecutingAgency;
using IDB.MW.Application.PEPModule.ViewModels;

namespace IDB.Presentation.MVC4.Areas.Pep.Models
{
    public class ViewModelMapperHelperExecAgency
    {
        private readonly IPepExecutingAgencies _pepExecAgency;

        public ViewModelMapperHelperExecAgency(IPepExecutingAgencies pepExecAgency)
        {
            _pepExecAgency = pepExecAgency;
        }

        public List<ExecutingAgencyViewModels> ComboExecAgencyResponse(IList<ExecutingAgencyViewModel> executingList)
        {
            return executingList.Select(ae => new ExecutingAgencyViewModels
            {
                ExecutingId = ae.ExecutingAgencyId,
                ExecutingName = ae.ExecutingAgencyName,
                AcrNm = ae.AcrNm
            }).ToList();
        } 
    }
}