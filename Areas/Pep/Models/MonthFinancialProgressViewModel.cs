namespace IDB.Presentation.MVC4.Areas.Pep.Models
{
    public class MonthFinancialProgressViewModel
    {
        public int Month { get; set; }

        public decimal? Idb { get; set; }

        public decimal? LocalCounterpart { get; set; }

        public decimal? CoFinancing { get; set; }
    }
}