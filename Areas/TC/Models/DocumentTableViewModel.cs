using System.Collections.Generic;
using IDB.MW.Application.TCAbstractModule.ViewModels.TCAbstractService;

namespace IDB.Presentation.MVC4.Areas.TC.Models
{
    public class DocumentTableViewModel
    {
        public string DocumentTableName { get; set; }

        public bool ShowAddButton { get; set; }

        public string DocumentsInstructions { get; set; }

        public List<DocumentViewModel> Documents { get; set; }

        public string AddFunctionName { get; set; }

        public bool CanDelete { get; set; }

        public string BeforeDeleteFuncionName { get; set; }

        public string BusinessAreaCode { get; set; }

        public bool PageAreaRequestIncrease { get; set; }
    }
}