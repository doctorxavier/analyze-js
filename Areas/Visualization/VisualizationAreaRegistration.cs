using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.Visualization
{
    public class VisualizationAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Visualization";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
               "New_Visualization_map",
               "{operationNumber}/Visualization/VisualData/{action}/{id}",
               new { controller = "VisualData", action = "Grid", id = UrlParameter.Optional });

            context.MapRoute(
               "New_Visualization_api",
               "{operationNumber}/Visualization/Api/{action}/{id}",
               new { controller = "Api", action = "VisualizationDataSave", id = UrlParameter.Optional });
      
            context.MapRoute(
                "Visualization_default",
                "Visualization/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });
        }
    }
}