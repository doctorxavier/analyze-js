using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.BEOProcurement.Components
{
    public static class CollapseAllButtonExtension
    {
        private static int internalId = 0;
        public static CollapseAllButton CollapseAll(this HtmlHelper html, string name)
        {
            if (internalId > 9999)
            {
                internalId = 0;
            }

            internalId++;
            return new CollapseAllButton(internalId).Name(name);
        }
    }
}