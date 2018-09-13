using IDB.Architecture.Security.Messages.UserIdentity;

namespace IDB.Presentation.MVC4.Areas.SupervisionPlan.Models
{
    public class GlobalIdentityTestModel
    {
        public GetUsersByGroupsResponse SearchUsersByGroups { get; set; }
        public GetUserByPCmailOrNameResponse SearchUsersByNameOrFullName { get; set; }
        public GetUsersResponse SearchUser { get; set; }
        public GetUserByAdResponse SearchUserbyAd { get; set; }
        public GetUsersByAdGroupResponse SearchUserByAdGroup { get; set; }
        public GetUserByRolResponse UsersByRole { get; set; }
        public SearchUsersResponse SearchUsersByFullNameOrName { get; set; }
        public GetUserFullNamesResponse SearchFullUserNamesByUser { get; set; }
        public SearchFullNameByUserName SearchFullNameByUserName { get; set; }
    }
}