namespace IDB.Presentation.MVC4.Areas.Pep.Models
{
    public class CommentPepViewModel
    {
        public int CommentId { get; set; }

        public string Comment { get; set; }

        public string CommentDate { get; set; }

        public string CommentUser { get; set; }

        public int? PepTaskId { get; set; }
    }
}