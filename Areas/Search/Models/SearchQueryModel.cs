using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDB.Presentation.MVC4.Areas.Search.Models
{
    public class SearchQueryModel
    {
        public string SectorDepartmentCode { get; set; }

        public string DivisionCode { get; set; }

        public string CountryDepartmentCode { get; set; }

        public string CountryCode { get; set; }

        public string OperationTypeCode { get; set; }

        public string OverallStageCode { get; set; }

        public string TeamMemberKeyword { get; set; }

        public string FundCode { get; set; }

        public string ApprovalYear { get; set; }

        public DateTime EligibilityDateFrom { get; set; }

        public DateTime EligibilityDateTo { get; set; }

        public string SelectedFilter { get; set; }

        public string UserName { get; set; }

        public string ClauseStatus { get; set; }
    }
}