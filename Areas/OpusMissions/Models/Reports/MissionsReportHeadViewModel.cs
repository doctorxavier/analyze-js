using System.Collections.Generic;

using IDB.MW.Application.OpusMissionsModule.ViewModels.Report;

namespace IDB.Presentation.MVC4.Areas.OpusMissions.Models.Reports
{
    public class MissionsReportHeadViewModel
    {
        public string Types { get; set; }

        public string Status { get; set; }

        public string Countries { get; set; }

        public string CountryDepartments { get; set; }

        public string SectorDepartments { get; set; }

        public string Divisions { get; set; }

        public string OperationNumber { get; set; }

        public string MissionMembers { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public IList<MissionsResportListViewModel> Missions { get; set; }
    }
}
