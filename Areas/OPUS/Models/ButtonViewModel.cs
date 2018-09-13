using System.Collections.Generic;
using IDB.MW.Application.OPUSModule.ViewModels.Common;

namespace IDB.Presentation.MVC4.Areas.OPUS.Models
{
    public class ButtonViewModel
    {
        public string Id { get; set; }

        public string Text { get; set; }

        public string HtmlClass { get; set; }

        public string ScriptToExecute { get; set; }

        public string ScriptToExecuteAfter { get; set; }

        public string ScriptToExecuteBefore { get; set; }
    }
}