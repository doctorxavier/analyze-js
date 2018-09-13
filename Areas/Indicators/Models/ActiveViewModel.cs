namespace IDB.Presentation.MVC4.Areas.Indicators.Models
{
    public class ActiveViewModel
    {
        public ActiveViewModel()
        {
        }

        public int Id { get; set; }
        public bool IsActive { get; set; }
        public bool IsEditMode { get; set; }
    }
}