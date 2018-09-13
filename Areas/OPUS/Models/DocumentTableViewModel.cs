using System.Collections.Generic;
using IDB.MW.Application.OPUSModule.ViewModels.Common;
using IDB.MW.Application.Core.ViewModels.Common;

namespace IDB.Presentation.MVC4.Areas.OPUS.Models
{
    public class DocumentTableViewModel
    {
        public string DocumentTableName { get; set; }

        public bool ShowAddButton { get; set; }

        public bool ShowRegisterApprovalButton { get; set; }

        public string DocumentsInstructions { get; set; }

        public List<DocumentViewModel> Documents { get; set; }

        public string AddFunctionName { get; set; }

        public bool CanDelete { get; set; }

        public string BeforeDeleteFuncionName { get; set; }

        public List<ButtonViewModel> AdditionalButtons { get; set; }

        public string TextMessage { get; set; }
    }
}