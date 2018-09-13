using System;

namespace IDB.Presentation.MVC4.Areas.ActivityProgram.Models
{
    public class WBSRowModel
    {
        public string WBS_NUMBER { get; set; }

        public string ACTIVITY_DESCRIPTION { get; set; }

        public decimal COST { get; set; }

        public int YEAR_ACTIVITY { get; set; }

        public int DELETE_STATUS { get; set; }

        public string ORGANIZATIONAL_UNIT { get; set; }

        public DateTime CREATED { get; set; }

        public string CREATED_BY { get; set; }

        public DateTime MODIFIED { get; set; }

        public string MODIFIED_BY { get; set; }
    }
}