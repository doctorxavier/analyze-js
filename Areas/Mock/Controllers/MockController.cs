using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.MW.Business.Core.SharepointSecurityService;
using IDB.MW.Infrastructure.Caching.Contracts;
using IDB.MW.Infrastructure.SecurityService;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.MW.Infrastructure.SecurityService.Enums;
using IDB.MW.Domain.Session;

namespace IDB.Presentation.MVC4.Areas.Mock.Controllers
{
    public partial class MockController : MVC4.Controllers.ConfluenceController
    {
        #region Fields
        private readonly ICacheStorageService _cacheStorageService;
        private readonly IAuthorizationService _authorizationService;
        #endregion

        #region Constructors
        public MockController(ICacheStorageService cacheStorageService)
        {
            _cacheStorageService = cacheStorageService;
            _authorizationService = AuthorizationServiceFactory.Current;
        }

        #endregion

        #region Actions
        // GET: Mock/Mock
        public virtual ActionResult Index()
        {
            return View();
        }

        public virtual JsonResult ChangeRoleAndPermissions(string operationNumber, RoleEnum role)
        {
            _cacheStorageService.Remove("AuthorizationInfo");

            var auth = new AuthorizationOperationInfo
            {
                ActionList = new List<ActionEnum>(),
                RoleList = new List<RoleEnum>
                {
                    RoleEnum.PCRProjectTeamLeader,
                    role
                },
                UserName = IDBContext.Current.UserLoginName
            };

            foreach (var permission in RolePermissions[role])
            {
                auth.ActionList.Add(permission);
            }

            var authPermission = new AuthorizationInfo
            {
                Authorization = new Dictionary<string, AuthorizationOperationInfo>()
            };
            authPermission.Authorization.Add(operationNumber, auth);

            _cacheStorageService.Add("AuthorizationInfo", authPermission);

            return Json(new { IsValid = true, ErrorMessage = "Permissions was changed" });
        }

        public virtual JsonResult ChangeRol(string operationNumber, ActionEnum permission)
        {
            _cacheStorageService.Remove("AuthorizationInfo");

            var auth = new AuthorizationOperationInfo()
            {
                ActionList = new List<ActionEnum>(),
                RoleList = new List<RoleEnum>()
                {
                    RoleEnum.PCRProjectTeamLeader
                },
                UserName = IDBContext.Current.UserName
            };

            auth.ActionList.Add(ActionEnum.ConvergenceReadPermission);

            if (!auth.ActionList.Any(x => x == permission))
            {
                auth.ActionList.Add(permission);
            }

            var authPermission = new AuthorizationInfo()
            {
                Authorization = new Dictionary<string, AuthorizationOperationInfo>()
            };
            authPermission.Authorization.Add(operationNumber, auth);

            _cacheStorageService.Add("AuthorizationInfo", authPermission);

            return Json(new { IsValid = true, ErrorMessage = "Permissions was changed" });
        }

        public virtual JsonResult ChangeRole(string role)
        {
            if (!IDBContext.Current.SecurityInfo.ContainsKey(IDBContext.Current.Operation))
            {
                IDBContext.Current.SecurityInfo.Add(IDBContext.Current.Operation, new SecurityInfo(new List<string>(), new List<string>(), new List<string>()));
            }

            IDBContext.Current.SecurityInfo[IDBContext.Current.Operation].Roles.Clear();

            if (!string.IsNullOrWhiteSpace(role))
            {
                IDBContext.Current.SecurityInfo[IDBContext.Current.Operation].Roles.Add(role);
            }

            return Json(new { IsValid = true, ErrorMessage = "Role was changed" });
        }

        public virtual JsonResult ChangePermission(string permission)
        {
            if (!IDBContext.Current.SecurityInfo.ContainsKey(IDBContext.Current.Operation))
            {
                IDBContext.Current.SecurityInfo.Add(IDBContext.Current.Operation, new SecurityInfo(new List<string>(), new List<string>(), new List<string>()));
            }

            IDBContext.Current.SecurityInfo[IDBContext.Current.Operation].Permissions.Clear();

            if (!string.IsNullOrWhiteSpace(permission))
            {
                IDBContext.Current.SecurityInfo[IDBContext.Current.Operation].Permissions.Add(permission);
            }

            return Json(new { IsValid = true, ErrorMessage = "Permissions was changed" });
        }

        public virtual JsonResult RemovePermission()
        {
            var authorizationInfo = _cacheStorageService.Get<AuthorizationInfo>("AuthorizationInfo");

            if (authorizationInfo == null)
            {
                return Json(new { IsValid = false, ErrorMessage = "No permission data to remove." });
            }

            _cacheStorageService.Remove("AuthorizationInfo");
            authorizationInfo = _cacheStorageService.Get<AuthorizationInfo>("AuthorizationInfo");

            if (authorizationInfo == null)
            {
                return Json(new { IsValid = true, ErrorMessage = "Permission data removed correctly." });
            }

            return Json(new { IsValid = false, ErrorMessage = "Permission data could not be removed." });
        }

        public virtual JsonResult ChangeWorspaceActionOrRole(string workspace, RoleEnum? role, ActionEnum? action)
        {
            _cacheStorageService.Remove("AuthorizationInfo");

            var auth = new AuthorizationOperationInfo()
            {
                ActionList = new List<ActionEnum>(),
                RoleList = new List<RoleEnum>(),
                UserName = IDBContext.Current.UserLoginName
            };

            if (role.HasValue)
            {
                auth.RoleList.Add(role.Value);
            }

            auth.ActionList.Add(ActionEnum.ConvergenceReadPermission);

            if (!auth.ActionList.Any(x => x == action))
            {
                if (action.HasValue)
                {
                    auth.ActionList.Add(action.Value);
                }
            }

            var authPermission = new AuthorizationInfo()
            {
                Authorization = new Dictionary<string, AuthorizationOperationInfo>()
            };
            authPermission.WorkspaceAuthorization.Add(workspace, auth);

            _cacheStorageService.Add("AuthorizationInfo", authPermission);

            return Json(new { IsValid = true, ErrorMessage = "Permissions was changed" });
        }

        public virtual JsonResult ChangeGlobalActionOrRole(string workspace, RoleEnum? role, ActionEnum? actionEnum)
        {
            _cacheStorageService.Remove("AuthorizationInfo");

            var auth = new AuthorizationOperationInfo()
            {
                ActionList = new List<ActionEnum>(),
                RoleList = new List<RoleEnum>(),
                UserName = IDBContext.Current.UserLoginName
            };

            if (role.HasValue)
            {
                auth.RoleList.Add(role.Value);
            }

            auth.ActionList.Add(ActionEnum.ConvergenceReadPermission);
            auth.ActionList.Add(ActionEnum.MyMeetingsReadPermission);
            auth.ActionList.Add(ActionEnum.SingleWindowOperationsReadPermission);

            if (!auth.ActionList.Any(x => x == actionEnum))
            {
                if (actionEnum.HasValue)
                {
                    auth.ActionList.Add(actionEnum.Value);
                }
            }

            var authPermission = new AuthorizationInfo();

            authPermission.GlobalAuthorization = auth;

            _cacheStorageService.Add("AuthorizationInfo", authPermission);

            return Json(new { IsValid = true, ErrorMessage = "Permissions was changed" });
        }

        public virtual JsonResult ChangeGlobalActionOrRoles(string workspace, RoleEnum? role, ActionEnum[] actionEnum)
        {
            _cacheStorageService.Remove("AuthorizationInfo");

            var auth = new AuthorizationOperationInfo()
            {
                ActionList = new List<ActionEnum>(),
                RoleList = new List<RoleEnum>(),
                UserName = IDBContext.Current.UserLoginName
            };

            if (role.HasValue)
            {
                auth.RoleList.Add(role.Value);
            }

            for (var i = 0; i < actionEnum.Count(); i++)
            {
                if (!auth.ActionList.Any(x => x == actionEnum[i]))
                {
                    auth.ActionList.Add(actionEnum[i]);
                }
            }

            var authPermission = new AuthorizationInfo();

            authPermission.GlobalAuthorization = auth;

            _cacheStorageService.Add("AuthorizationInfo", authPermission);

            return Json(new { IsValid = true, ErrorMessage = "Permissions was changed" });
        }
        #endregion

        private Dictionary<RoleEnum, List<ActionEnum>> RolePermissions = new Dictionary<RoleEnum, List<ActionEnum>>
        {
            { 
                RoleEnum.ESGAdmin, 
                new List<ActionEnum>
                {
                    ActionEnum.ConvergenceReadPermission, 
                    ActionEnum.EnvironmentalWritePermission,
                    ActionEnum.StandardConvergenceUserWritePermission,
                    ActionEnum.ESGReviewerWritePermission,
                    ActionEnum.ESGSpecialistWritePermission,
                    ActionEnum.ESGLeadWritePermission,
                    ActionEnum.ESGOperationalGroupLeaderWritePermission,
                    ActionEnum.ESGChiefWritePermission,
                    ActionEnum.ESGAdminWritePermission
                }
            },
            {
                RoleEnum.ESGLead, 
                new List<ActionEnum>
                {
                    ActionEnum.ConvergenceReadPermission, 
                    ActionEnum.EnvironmentalWritePermission,
                    ActionEnum.StandardConvergenceUserWritePermission,
                    ActionEnum.ESGReviewerWritePermission,
                    ActionEnum.ESGSpecialistWritePermission,
                    ActionEnum.ESGLeadWritePermission 
                }
            },
            {
                RoleEnum.ESGTeamMember, 
                new List<ActionEnum>
                {
                    ActionEnum.ConvergenceReadPermission, 
                    ActionEnum.EnvironmentalWritePermission,
                    ActionEnum.StandardConvergenceUserWritePermission,
                    ActionEnum.ESGReviewerWritePermission,
                    ActionEnum.ESGSpecialistWritePermission 
                }
            },
            {
                RoleEnum.ESGReviewer,
                new List<ActionEnum>
                {
                    ActionEnum.ConvergenceReadPermission,
                    ActionEnum.EnvironmentalWritePermission,
                    ActionEnum.StandardConvergenceUserWritePermission,
                    ActionEnum.ESGReviewerWritePermission
                }
            }
        };
    }
}