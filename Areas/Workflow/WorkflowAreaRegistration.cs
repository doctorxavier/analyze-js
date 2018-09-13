using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.Workflow
{
    public class WorkflowAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Workflow";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
               "GenericWorkflow_default",
                "{operationNumber}/Workflow/{controller}/{action}",
               new { action = "Index" });
        }
    }
}