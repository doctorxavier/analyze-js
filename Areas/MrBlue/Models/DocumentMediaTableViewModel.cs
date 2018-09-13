using System.Collections.Generic;

using IDB.MW.Application.MrBlueModule.ViewModels.ManageMediaFiles;

namespace IDB.Presentation.MVC4.Areas.MrBlue.Models
{
    public class DocumentMediaTableViewModel
    {
        public string DocumentTableName { get; set; }

        public bool ShowAddButton { get; set; }

        public List<ManageMediaFilesRowViewModel> Documents { get; set; }

        public string AddFunctionName { get; set; }

        public bool CanDelete { get; set; }

        public string BeforeDeleteFuncionName { get; set; }
    }
}