using IDB.MW.Domain.Models.Global;

namespace IDB.Presentation.MVC4.Areas.Global.Models
{
    public class DelegatedTaskUserModel
    {
        public string OperationNumber { get; set; }

        public int TaskId { get; set; }

        public string FilterApplied { get; set; }

        public TaskDelegatedModel UserData { get; set; }
    }
}