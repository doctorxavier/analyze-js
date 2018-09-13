using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.Agreements
{
    public class AgreementAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Agreements";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Agreements_Conditions",
                "{operation}/Agreements/{controller}/{action}",
                defaults: new { action = "Index" });
            context.MapRoute(
               "Agreements_Conditions_route",
               "{mainOperationNumber}/Agreements/{controller}/{action}",
               defaults: new { action = "Index" });
            context.MapRoute(
                "Agreements_default",
                "Agreements/{controller}/{action}",
                defaults: new { action = "Index" });
        }
    }
}