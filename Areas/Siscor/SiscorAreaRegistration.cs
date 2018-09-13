using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.Siscor
{
    public class SiscorAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Siscor";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Siscor_default",
                "{operationNumber}/Siscor/{controller}/{action}",
                new
                {
                    action = "Index"
                });
            context.MapRoute(
                "Siscor_global",
                "Siscor/{controller}/{action}",
                new
                {
                    action = "Index"
                });
        }
    }
}