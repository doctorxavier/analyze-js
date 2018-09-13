using System.Collections.Generic;

using IDB.MW.Application.PACI.ViewModels;
using IDB.MW.Domain.Models.Security;

namespace IDB.Presentation.MVC4.Areas.PACI.Models
{
    public class PaciViewModel
    {
        public PaciViewModel()
        {
            InstitutionQuestionnaires = new List<InstitutionQuestionnaireViewModel>();
        }

        public int OperationId { get; set; }
        public string OperationNumber { get; set; }
        public bool IsEdit { get; set; }
        public string ErrorMessage { get; set; }
        public IList<InstitutionQuestionnaireViewModel> InstitutionQuestionnaires { get; set; }
        public string Hash { get; set; }
        public int SelectedInstitution { get; set; }
        public SecurityViewModel Security { get; set; }
        public bool AddNewPaciIsVisible { get; set; }
    }
}