using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.Indicators
{
    public class IndicatorsAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Indicators";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute("Indicators_default", "Indicators/{controller}/{action}");
        }
    }
}