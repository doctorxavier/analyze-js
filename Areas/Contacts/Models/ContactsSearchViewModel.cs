using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IDB.MW.Business.Core.Mainframe.DTO.Contacts;

namespace IDB.Presentation.MVC4.Areas.Contacts.Models
{
    public class ContactsSearchViewModel
    {
        public SearchByCriteriaDTO SearchFilters { get; set; }
    }
}