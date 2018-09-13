namespace IDB.Presentation.MVC4.Areas.PEP.Models
{
    public class PoaReportRequest
    {
        public string OperationNumber { get; set; }
        public int CountryId { get; set; }
        public int FilterType { get; set; }
        public int FilterValue { get; set; }
    }
}