using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.DocumentTemplate
{
    public class DocumentTemplateAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "DocumentTemplate";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
               "DocumentTemplate_default",
               "DocumentTemplate/{controller}/{action}",
               new { action = "GeneralInformation" });
        }
    }
}