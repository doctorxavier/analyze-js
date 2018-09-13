using System.Collections.Generic;
using IDB.Architecture.Security.Models.UserIdentity;

namespace IDB.Presentation.MVC4.Areas.Global.Models
{
    public class DelegatingTaskResultsModel
    {
        public int TaskId { get; set; }

        public string FilterApplied { get; set; }

        public List<UserIdentityModel> Users { get; set; }

        public UserIdentityModel[][] SplittedUsers { get; set; }
    }
}