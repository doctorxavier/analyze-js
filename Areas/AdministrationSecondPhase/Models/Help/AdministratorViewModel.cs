using IDB.MVCControls.Confluence.Models;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.Help
{
    public class AdministratorViewModel<T> where T : class, new()
    {
        public T Data { get; set; }

        public ButtonControlViewModel ButtonConfiguration { get; set; }

        public MultiLanguageBoxSimpleModel Descriptions { get; set; }

        public MultiLanguageBoxSimpleModel Urls { get; set; }

        public AdministratorViewModel()
        {
            Data = new T();
            ButtonConfiguration = new ButtonControlViewModel();
        }
    }
}