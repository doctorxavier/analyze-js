using System.Collections.Generic;

namespace IDB.Presentation.MVC4.Areas.ActivityProgram.Models
{
    public class DataUserModel
    {
        public string UserName { get; set; }

        public string UserFullName { get; set; }

        public IEnumerable<string> RolesUser { get; set; }

        public IEnumerable<string> PermissionsUser { get; set; }

        public string UnitOrganizational { get; set; }
    }
}