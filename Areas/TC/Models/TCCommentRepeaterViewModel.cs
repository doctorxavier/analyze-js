using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IDB.MW.Application.TCAbstractModule.ViewModels.TCAbstractService;
using IDB.Presentation.MVC4.Areas.TC.Enums;

namespace IDB.Presentation.MVC4.Areas.TC.Models
{
    public class TCCommentRepeaterViewModel
    {
        public List<TCAbstractCommentsViewModel> Comments { get; set; }
        public CommentTypeEnum Type { get; set; }
    }
}