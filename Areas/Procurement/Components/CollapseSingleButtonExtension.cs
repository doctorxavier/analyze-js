using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.BEOProcurement.Components
{
    public static class CollapseSingleButtonExtension
    {
        private static int internalId = 0;
        public static CollapseSingleButton CollapseSingle(this HtmlHelper html, string name)
        {
            if (internalId > 9999)
            {
                internalId = 0;
            }

            internalId++;
            return new CollapseSingleButton(internalId).Name(name);
        }
    }
}