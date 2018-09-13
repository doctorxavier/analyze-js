using System.Collections.Generic;

using IDB.MW.Application.PACI.ViewModels;

namespace IDB.Presentation.MVC4.Areas.PACI.Models
{
    public class ReportViewModel
    {
        public ReportViewModel()
        {
            Modules = new List<ModuleQuestionnaireViewModel>();
            ModulesTabs = new ModuleDataTabsViewModel();
        }

        public int QuestionnaireId { get; set; }
        public string OperationNumber { get; set; }
        public bool IsEdit { get; set; }
        public string ErrorMessage { get; set; }
        public string Hash { get; set; }
        public string TabId { get; set; }
        public string ModuleTabId { get; set; }
        public string InstitutionName { get; set; }
        public int InstitutionId { get; set; }
        public string PaciStage { get; set; }
        public bool OperationIsComplete { get; set; }
        public ModuleDataTabsViewModel ModulesTabs { get; set; }
        public IList<ModuleQuestionnaireViewModel> Modules { get; set; }
        public MoveAssessmentViewModel SelectedAssessment { get; set; }
        public IList<string> PageName { get; set; }
        public SecurityViewModel Security { get; internal set; }
        public bool IsAllEvaluated { get; set; }
        public bool CanEvaluate { get; set; }
        public bool PaciIsComplete { get; set; }
    }
}