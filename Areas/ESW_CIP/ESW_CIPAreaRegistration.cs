using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.ESW_CIP
{
    public class ESW_CIPAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "ESW_CIP";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "ESW_CIP_default",
                "{operationNumber}/ESW_CIP/{controller}/{action}",
                new { action = "ProposalList" });
        }
    }
}