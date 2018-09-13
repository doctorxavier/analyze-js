using System;

namespace IDB.Presentation.MVC4.Areas.Workspace.Models
{
    public static class WorkSpaceConstants
    {
        public static string BLANKCHART = "BLC";
        public static string EXCEL_SHEET_NAME = "Sheet1";
        public static char SEPARATOR = Convert.ToChar("~");
        public static string HEADER_INITIAL = "[";
        public static string HEADER_FINALLY = "]";
        public static char NEWLINE = Convert.ToChar("\n");

        //Pages
        public static string PAGE_CHART = "001-WorkspaceAdministration";
        public static string PAGE_ADM_TEMPLATE = "002-WorkspaceAdministrationTemplate";
        public static string PAGE_ROLES = "003-WorkspaceAdministrationRoles";
        public static string PAGE_VIEW = "004-WorkspaceView";
    }
}