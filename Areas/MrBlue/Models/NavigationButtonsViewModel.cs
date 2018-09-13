using IDB.MW.Application.MrBlueModule.Enums;

namespace IDB.Presentation.MVC4.Areas.MrBlue.Models
{
    public class SafeguardToolkitButtonsViewModel
    {
        public int VersionId { get; set; }
        public SafeguardToolkitStepEnum Step { get; set; }
        public bool ShowCancel { get; set; }
        public bool ShowBack { get; set; }
        public bool ShowNext { get; set; }
        public bool ShowSave { get; set; }
        public bool ShowFinalize { get; set; }
        public bool ShowClose { get; set; }
        public bool isStep2 { get; set; }
    }

    public class SupervisionReportButtonsViewModel
    {
        public int? VersionId { get; set; }
        public SupervisionReportStepEnum Step { get; set; }
        public bool ShowCancel { get; set; }
        public bool ShowBack { get; set; }
        public bool ShowNext { get; set; }
        public bool ShowSave { get; set; }
        public bool ShowFinalize { get; set; }
        public bool ShowClose { get; set; }
        public bool ShowPDF { get; set; }
    }
}