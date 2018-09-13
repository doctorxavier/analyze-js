using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

using IDB.Architecture;
using IDB.Architecture.Logging;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Contracts.ModelRepositories.Security;

namespace IDB.Presentation.MVC4.Areas.Global.Controllers
{
    public partial class SecurityController : MVC4.Controllers.ConfluenceController
    {
        private readonly ISecurityModelRepository _securityModelRepository;

        public SecurityController(ISecurityModelRepository securityModelRepository)
        {
            _securityModelRepository = securityModelRepository;
        }

        public virtual ActionResult Authentication()
        {
            if (Globals.GetSetting("SecurityMode", string.Empty).Equals("Local"))
            {
                FormsAuthentication
                    .RedirectFromLoginPage(IDBContext.Current.UserLoginName, false);
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public virtual JsonResult AuthenticateUser(string ticket, string expectedUser)
        {
            Logger.GetLogger().WriteDebug("Security", string.Format(
                "Authenticating ticket: {0} from expected user {1}", ticket, expectedUser));

            string[] arrTokenContent;
            string error = SecurityUtils.TryDecodeToken(
                ticket, HttpContext.Session, out arrTokenContent);

            if (!string.IsNullOrEmpty(error))
            {
                Logger.GetLogger().WriteMessage("Security", error);

                return new JsonResult()
                {
                    Data = error
                };
            }

            // The following code snippet tries to detect a problem in SharePoint regarding
            // mixed credentials when logging in. When the issue in SharePoint is detected and fixed
            // this code could be removed and the 'expectedUser' parameter won't be necessary.
            string decodedUser = SecurityUtils.GetDecodedUser(arrTokenContent);
            if (string.Compare(decodedUser, expectedUser, true, CultureInfo.InvariantCulture) != 0)
            {
                string errorMessage =
                    "Something went wrong with tickets management in SharePoint: the ticket sent" +
                    " does not match the user that tried to log in. The issue has been stored in" +
                    " the log for further review by the support team. Please, try to log in again.";

                string logMessage = string.Format(
                    "The ticket '{0}' corresponds to user '{1}', but the expected user was '{2}'",
                    ticket,
                    decodedUser,
                    expectedUser);

                Logger.GetLogger().WriteError("Security", logMessage, new Exception(errorMessage));

                return new JsonResult()
                {
                    Data = errorMessage
                };
            }

            FormsAuthentication.SetAuthCookie(
                string.Format("{0}|{1}", arrTokenContent[0], ticket), false);

            return new JsonResult() { Data = "true" };
        }

        [HttpGet]
        [AllowAnonymous]
        public virtual JsonResult PermissionsByUser(string operationNumber, string ticket)
        {
            try
            {
                Logger.GetLogger().WriteDebug("SecurityController", string.Format(
                "Authenticating ticket: {0} In PermissionsByUser - Controller Method", ticket));

                if (string.IsNullOrEmpty(operationNumber) || string.IsNullOrEmpty(ticket))
                    return Json(string.Empty, JsonRequestBehavior.AllowGet);

                var username = string.IsNullOrEmpty(IDBContext.Current.UserName) ?
                    string.Empty :
                    IDBContext.Current.UserName;

                var roles = SecurityManagerProxy.GetRoles(ticket, operationNumber);
                var permissions = SecurityManagerProxy
                    .GetPermissions(operationNumber, roles, username);

                return Json(permissions, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Logger.GetLogger().WriteError(
                    "SecurityController",
                    string.Format(
                        "Error when getting permissions by user. Operation: {0} ticket: {1}",
                        operationNumber,
                        ticket),
                    e);
                return Json(e.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public virtual JsonResult GetFieldsBehaviourByPage(
            string pageNames, string countryId, string typeId)
        {
            if (string.IsNullOrEmpty(pageNames))
                return Json(string.Empty);

            var operationNumber = IDBContext.Current.Operation ?? string.Empty;
            var arrPermissions = IDBContext.Current.Permissions ?? new List<string>();

            int countryID = 0;
            int typeID = 0;

            if (string.IsNullOrEmpty(countryId) || string.IsNullOrEmpty(typeId))
            {
                var fielAccessBehavior = _securityModelRepository.GetFieldBehaviorByPermissions(
                    operationNumber, pageNames, arrPermissions, countryID, typeID);

                return Json(fielAccessBehavior);
            }

            bool parsedCountry = int.TryParse(countryId, out countryID);
            bool parsedType = int.TryParse(typeId, out typeID);

            if (parsedCountry && parsedType)
            {
                var fielAccessBehaviorAlt = _securityModelRepository.GetFieldBehaviorByPermissions(
                operationNumber, pageNames, arrPermissions, countryID, typeID);

                return Json(fielAccessBehaviorAlt);
            }

            return Json(string.Empty);
        }
    }
}