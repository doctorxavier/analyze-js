using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.Workspace
{
    public class WorkspaceAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Workspace";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Workspace_default",
                "Workspace/{controller}/{action}",
                new { controller = "View", action = "Index" });
        }
    }
}