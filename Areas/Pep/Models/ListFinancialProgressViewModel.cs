using System.Collections.Generic;

namespace IDB.Presentation.MVC4.Areas.Pep.Models
{
    public class ListFinancialProgressViewModel
    {
        public IList<FinancialProgressViewModel> ListFinancialProgress { get; set; }

        public bool IsValid { get; set; }

        public decimal? CurrentTotalCost { get; set; }
    }
}