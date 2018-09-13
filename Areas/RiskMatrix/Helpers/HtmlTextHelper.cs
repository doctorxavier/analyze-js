using System.Web.Mvc;

using IDB.Architecture.Language;

namespace IDB.Presentation.MVC4.Areas.RiskMatrix.Helpers
{
    public class HtmlTextHelper
    {
        public static MvcHtmlString ModalText(string stringCode)
        {
            string textSection = "<p>" + Localization.GetText(stringCode) + "</p>";
            return new MvcHtmlString(textSection);
        }
    }
}