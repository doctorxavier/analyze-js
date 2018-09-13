using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.SignaturesAndContacts
{
    public class SignaturesAndContactsRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "SignaturesAndContacts";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Signatures_operation",
                "{operation}/SignaturesAndContacts/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });
            context.MapRoute(
                "Signatures_default",
                "SignaturesAndContactst/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });
        }
    }
}