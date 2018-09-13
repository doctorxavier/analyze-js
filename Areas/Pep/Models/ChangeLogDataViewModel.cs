namespace IDB.Presentation.MVC4.Areas.Pep.Models
{
    public class ChangeLogDataViewModel
    {
        public int Id { get; set; }

        public string ExecutingAgencyName { get; set; }

        public string Version { get; set; }

        public string Date { get; set; }

        public string User { get; set; }

        public string Role { get; set; }

        public string Action { get; set; }

        public string Comment { get; set; }
    }
}