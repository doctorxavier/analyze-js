using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.FindingRecomendations
{
    public class FindingRecomendationsAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "FindingRecomendations";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "FindingRecomendations_operation",
                "{operation}/FindingRecomendations/{controller}/{action}",
                defaults: new { controller = "FindingRecommendationController", action = "Index" });

            context.MapRoute(
                "FindingRecomendations_default",
                "FindingRecomendations/{controller}/{action}",
                defaults: new { controller = "FindingRecommendationController", action = "Index" });
        }
    }
}