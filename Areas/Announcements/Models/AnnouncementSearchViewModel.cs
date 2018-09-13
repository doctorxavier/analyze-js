using System.Collections.Generic;
using IDB.MW.Business.Core.Mainframe.DTO.Announcement;
using IDB.MW.Domain.Session;

namespace IDB.Presentation.MVC4.Areas.Announcements.Models
{
    public class AnnouncementSearchViewModel
    {
        public IEnumerable<AnnouncementSearchItemDTO> SearchResults { get; set; }
        public SearchByCriteriaDTO SearchFilters { get; set; }
        public List<ScopeDTO> ScopeTypes { get; set; }
        public int TotalPages { get; set; }
        public int ActualPage { get; set; }
        public bool HasPreviousPage
        {
            get
            {
                return ActualPage - 1 < 1;
            }
        }

        public bool HasNextPage
        {
            get
            {
                return ActualPage + 1 > TotalPages;
            }
        }

        public bool HasWritePermission
        {
            get
            {
                return IDBContext.Current.HasPermission(Permission.SYSTEM_ADMINISTRATOR);
            }
        }

        public bool HasReadPermission
        {
            get
            {
                return IDBContext.Current.HasPermission(Permission.GCM_READ);
            }
        }
    }
}