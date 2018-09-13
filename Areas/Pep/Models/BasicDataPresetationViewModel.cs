namespace IDB.Presentation.MVC4.Areas.Pep.Models
{
    public class BasicDataPresetationViewModel
    {
        public int PepTaskBucket { get; set; }

        public int? ApprovalNumberId { get; set; }

        public string ApprovalNumber { get; set; }

        public int? ExecutingAgencyId { get; set; }

        public string ExecutingAgency { get; set; }

        public int StatusId { get; set; }

        public string Status { get; set; }

        public string LastNonObjectionDate { get; set; }

        public string LastNonObjectionBy { get; set; }
    }
}