using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.BEOProcurement.Components
{
    public static class ChecklistExtension
    {
        public static Checklist Checklist(this HtmlHelper html, string name)
        {
            return new Checklist().Name(name);
        }
    }
}