using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.RelationshipManagement
{
    public class RelationshipManagementAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "RelationshipManagement";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context?.MapRoute(
                "RelationshipManagement_default",
                "{operationNumber}/RelationshipManagement/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });
        }
    }
}