using System;
using System.Collections.Generic;

namespace IDB.Presentation.MVC4.Areas.Indicators.Models
{
    public class LinkViewModel
    {
        public LinkViewModel()
        {
            Parameters = new Dictionary<string, object>();
        }

        public string Action { get; set; }
        public string Controller { get; set; }
        public string Area { get; set; }
        public string Text { get; set; }
        public DateTime EffectiveDate { get; set; }
        public bool isActive { get; set; }

        public IDictionary<string, object> Parameters { get; set; }
    }
}