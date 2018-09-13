using System.Collections.Generic;

using IDB.MW.Domain.Models.ActivityProgram;
using IDB.MW.Application.ActivityProgram.ViewModels;

namespace IDB.Presentation.MVC4.Areas.ActivityProgram.Models
{
    public class ActivityProgramModel
    {
        public string Year { get; set; }

        public string Display { get; set; }

        public string Pagination { get; set; }

        public string OrganizationalUnit { get; set; }

        public List<AnnualActivityModel> ListNewRowsACT { get; set; }

        public List<AnnualActivityModel> ListLogicDelete { get; set; }

        public List<AnnualActivityModel> ListModified { get; set; }

        public List<AnnualActivityModel> ModifiedDeleteAct { get; set; }

        public List<AnnualActivityModel> ModifiedDeleteNewAct { get; set; }

        public List<SapInfoViewModel> ListSap { get; set; }
    }
}