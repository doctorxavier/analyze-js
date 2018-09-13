namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.Help
{
    public class ButtonControlViewModel
    {
        public string ActionCancel { get; set; }

        public string ActionEdit { get; set; }

        public string ActionSave { get; set; }

        public bool HasEditButton { get; set; }

        public bool HasNewButton { get; set; }

        public string NavigationCancel { get; set; }

        public string NavigationNew { get; set; }

        public string UrlSave { get; set; }

        public bool HasEditPermission { get; set; }

        public ButtonControlViewModel()
        {
            ActionCancel = string.Empty;
            ActionEdit = string.Empty;
            ActionSave = string.Empty;
            HasEditButton = false;
            HasNewButton = false;
            NavigationCancel = string.Empty;
            NavigationNew = string.Empty;
            UrlSave = string.Empty;
            HasEditPermission = true;
        }
    }
}