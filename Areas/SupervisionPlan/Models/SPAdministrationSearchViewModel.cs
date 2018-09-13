using System.Collections.Generic;
using System.Web.Mvc;
using IDB.MW.Business.SupervisionPlan.DTOs;
using IDB.MW.Business.SupervisionPlan.ViewModels.Administration;

namespace IDB.Presentation.MVC4.Areas.SupervisionPlan.Models
{    
    public class SPAdministrationSearchViewModel
    {
        public List<SelectListItem> UnitSearch { get; set; }
        public List<SelectListItem> PrevMarchPMRClass { get; set; }
        public List<SelectListItem> CurrMarchPMRClass { get; set; }
        public List<SelectListItem> ExceedFinExtCri { get; set; }
        public List<SelectListItem> SPModality { get; set; }
        public List<SelectListItem> Displayed { get; set; }
        public List<SPAdminitrationDTO> ResultsSearch { get; set; }
        public SPSearchByFilterDTO SearchByFilters { get; set; }
        public ICollection<ParentOperationWithRelatedModel> ParentRelationship { get; set; }
    }
}