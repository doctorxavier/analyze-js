namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.AuthorizationAll
{
    public class AuthorizationAllTableRowViewModel
    {
        public bool Status { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string OrganizationalUnit { get; set; }
        public int? OrganizationalUnitID { get; set; }
        public string Country { get; set; }
        public string CountryDepartment { get; set; }
        public string Department { get; set; }
        public string Division { get; set; }
        public string Global { get; set; }
        public string OperationNumber { get; set; }
        public int? OperationId { get; set; }
        public string Role { get; set; }
        public int RoleId { get; set; }
    }
}