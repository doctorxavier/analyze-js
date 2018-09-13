using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.Clauses
{
    public class ClausesAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Clauses";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Clauses_Contracts",
                "{operation}/Clauses/{controller}/{action}",
                defaults: new { action = "Index" });
            context.MapRoute(
               "Clauses_Contracts_route",
               "{mainOperationNumber}/Clauses/{controller}/{action}",
               defaults: new { action = "Index" });
            context.MapRoute(
                "Clauses_default",
                "Clauses/{controller}/{action}",
                defaults: new { action = "Index" });
        }
    }
}