using System.Collections.Generic;
using System.Linq;
using IDB.MW.Application.PEPModule.Services.Procurement;
using ViewModelProcurementType = IDB.MW.Application.PEPModule.ViewModels;

namespace IDB.Presentation.MVC4.Areas.Pep.Models
{
    public class ViewModelMapperHelperProcurementType
    {
        private readonly IProcurement _pepProcurementType;

        public ViewModelMapperHelperProcurementType(IProcurement pepProcurementType)
        {
            _pepProcurementType = pepProcurementType;
        }

        public List<ProcurementTypeViewModel> ComboProcurementTypeResponse(IList<ViewModelProcurementType.ProcurementTypeViewModel> executingList)
        {
            return executingList.Select(ae => new ProcurementTypeViewModel
            {
                ProcurementTypeId = ae.ProcurementTypeId,
                ProcurementTypeName = ae.ProcurementTypeName
            }).ToList();
        } 
    }
}