using System.Collections.Generic;

using IDB.MW.Application.PACI.ViewModels;

namespace IDB.Presentation.MVC4.Areas.PACI.Models
{
    public class QuestionnaireViewModel
    {
        public QuestionnaireViewModel()
        {
            ModulesTabs = new ModuleDataTabsViewModel();
            DisplayModule = new ModuleQuestionnaireViewModel();
            Tabs = new List<TabViewModel>();
            Security = new SecurityViewModel();
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
        public int PaciConditionId { get; set; }
        public int QuestionId { get; set; }
        public ModuleDataTabsViewModel ModulesTabs { get; set; }
        public IList<ModuleQuestionnaireViewModel> Modules { get; set; }
        public ModuleQuestionnaireViewModel DisplayModule { get; set; }
        public IList<TabViewModel> Tabs { get; set; }
        public SecurityViewModel Security { get; internal set; }
    }
}