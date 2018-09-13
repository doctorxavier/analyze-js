using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDB.Presentation.MVC4.Areas.TCM.Models
{
    public class CustomLinkPredefinedIndicatorModel
    {
        public int baseIndicatorId { get; set; }
        public int predefinedIndicatorId { get; set; }
        public string backUrl { get; set; }
        public int module { get; set; }
    }
}