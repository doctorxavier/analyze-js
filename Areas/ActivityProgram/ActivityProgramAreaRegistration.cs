using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.ActivityProgram
{
    public class ActivityProgramAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "ActivityProgram";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "ActivityProgram_default",
                "ActivityProgram/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });
        }
    }
}