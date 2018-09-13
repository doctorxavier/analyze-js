using System.Web.Mvc;
using System.Linq;
using System.Collections.Generic;

using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Controllers;
using IDB.MW.Application.AdministrationModule.Services.UserAccess;
using IDB.Architecture.Language;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Controllers
{
    public partial class UserAccessController : BaseController
    {
        private readonly IUserAccessService _userAccessService;

        public UserAccessController(IUserAccessService userAccessService)
        {
            _userAccessService = userAccessService;
        }

        public virtual ActionResult Index()
        {
            var userName = IDBContext.Current.UserName;
            var roles = IDBContext.Current.Roles;
            var permissions = IDBContext.Current.Permissions;

            var rolesAndPermissions = _userAccessService
                .GetRolesAndPermissions(userName, roles, permissions);

            return View(rolesAndPermissions);
        }

        [HttpGet]
        public virtual JsonResult SearchRolesAndPermissions(string operation)
        {
            var userName = IDBContext.Current.UserName;

            var result = _userAccessService.GetRolesAndPermissionsSearch(userName, operation);

            if (!result.IsValid)
            {
                result.ErrorMessage = Localization.GetText("GLOBAL.SERVICE.GeneralError");
            }
            else if (!result.Model.HasOperation)
            {
                result.ErrorMessage = Localization.GetText("UA.Message.NoOperation");
            }
            else if (!result.Model.Member)
            {
                result.ErrorMessage = Localization.GetText("UA.Message.TeamMember");
            }
            else if (result.Model.Roles.Count == 0 || result.Model.Permissions.Count == 0)
            {
                result.ErrorMessage = Localization.GetText("COMMON.SVQ.NoResults");
            }
            else
            {
                result.ErrorMessage = string.Empty;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
