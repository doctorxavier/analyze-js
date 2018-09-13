using System;

namespace IDB.Presentation.MVC4.Areas.VMR.Models
{
    public class FilterCommentViewModel
    {        
        public int? TopicCommentId { get; set; }
        public int? OrganizationalUnitId { get; set; }
        public int? UserNameId { get; set; }
        public string UserName { get; set; }
        public int? CommentTypeId { get; set; }
        public string CommentTypeCode { get; set; }
        public bool OrderBy { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int Page { get; set; }
        public int Pagination { get; set; }
        public bool MyComments { get; set; }
        public bool IsCheckPublic { get; set; }
        public bool IsOpcAgreement { get; set; }
    }
}