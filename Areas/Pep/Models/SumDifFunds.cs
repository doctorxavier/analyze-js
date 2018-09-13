namespace IDB.Presentation.MVC4.Areas.Pep.Models
{
    public class SumDifFunds
    {
        public string ContractNumber { get; set; }
        public decimal CurrentApprovalAmount { get; set; }
        public decimal? TotalEop { get; set; }
        public bool DiffAmount { get; set; }
        public decimal? ExecutedAmount { get; set; }
    }
}