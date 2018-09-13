namespace IDB.Presentation.MVC4.Areas.Pep.Models
{
    public class SupervisionMethodModel
    {
        public int SupervisionMethodId { get; set; }

        public string SupervisionMethodName { get; set; }

        public int ProcurementMethodId { get; set; }

        public int ProcurementTypeId { get; set; }

        public int? MinLimit { get; set; }

        public int? MaxLimit { get; set; }
    }
}