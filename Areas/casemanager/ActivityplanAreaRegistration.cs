using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.casemanager
{
    public class CasemanagerAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Casemanager";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Casemanager_route",
                "{operation}/casemanager/{controller}/{action}",
                defaults: new { action = "Index" });
        }
    }
}