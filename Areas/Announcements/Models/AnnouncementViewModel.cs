using System.Collections.Generic;
using IDB.MW.Business.Core.Mainframe.DTO.Announcement;
using IDB.MW.Domain.Session;

namespace IDB.Presentation.MVC4.Areas.Announcements.Models
{
    public class AnnouncementViewModel
    {
        public List<ScopeDTO> ScopeTypes { get; set; }
        public AnnouncementDTO Announcement { get; set; }
        public bool IsNew { get; set; }
        public int DefaultScopeId { get; set; }
        public string DefaultScopeName { get; set; }
        public bool HasWritePermission
        {
            get
            {
                return IDBContext.Current.HasPermission(Permission.SYSTEM_ADMINISTRATOR);
            }
        }
    }
}