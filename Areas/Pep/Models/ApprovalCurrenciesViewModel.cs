namespace IDB.Presentation.MVC4.Areas.Pep.Models
{
    public class ApprovalCurrenciesViewModel
    {
        public string ApprovalNumber { get; set; }

        public int? CurrencyId { get; set; }

        public string Currency { get; set; }

        public decimal? ExchangeRateUsd { get; set; }
    }
}