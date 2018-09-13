using System.Collections.Generic;

namespace IDB.Presentation.MVC4.Areas.Pep.Models
{
    public class FinancialProgressViewModel
    {
        public int PepTaskId { get; set; }

        public int FinancialYear { get; set; }

        public decimal? TotalCostTemp { get; set; }

        public bool AutoCalculate { get; set; }

        public IList<MonthFinancialProgressViewModel> MonthFinancialProgress { get; set; }

        public decimal? TotalIdb { get; set; }

        public decimal? TotalLocalCounterpart { get; set; }

        public decimal? TotalCoFinancing { get; set; }

        public decimal? Total { get; set; }

        public string TaskColorFinancial { get; set; }

        public decimal? SumSourceIDB { get; set; }

        public decimal? SumSourceLC { get; set; }

        public decimal? SumSourceCO { get; set; }

        public int? Item { get; set; }
    }
}