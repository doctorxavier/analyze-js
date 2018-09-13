using System;
using System.Collections.Generic;
using System.Web.Mvc;

using IDB.Architecture.Logging;
using IDB.MW.Application.TCAbstractModule.Helpers;
using IDB.MW.Application.TCAbstractModule.Services.Mocks.TC;
using IDB.MW.Business.Core.K2Manager.Contracts;
using IDB.MW.Business.Core.Security;
using IDB.MW.Domain.Contracts.DomainServices;
using IDB.MW.Domain.Contracts.ModelRepositories.Global;
using IDB.MW.Domain.Session;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.MW.DomainModel.Contracts.Repositories.RolesAndPermissionsModule;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.MW.Infrastructure.SecurityService.Enums;

namespace IDB.Presentation.MVC4.Areas.TC.Controllers
{
    public partial class MockController : MVC4.Controllers.ConfluenceController
    {
        #region Fields
        private readonly IAuthorizationService _authorizationService;
        private readonly ISharePointSecurityProvider _sharePointSecurityProvider;
        private readonly ITCMockService _tcMockService;
        private readonly IOperationRepository _operationRepository;
        private readonly IK2Manager _k2Manager;
        private readonly IGlobalModelRepository _globalModelRepository;
        private readonly K2IntegrationHelper _k2IntegrationHelper;
        private readonly ISecurityGroupAssignedRepository _securityGroupAssignedRepository;
        #endregion

        #region Contructors
        public MockController(
            IAuthorizationService authorizationService,
            ISharePointSecurityProvider sharePointSecurityProvider,
            ITCMockService tcMockService,
            IOperationRepository operationRepository,
            IK2Manager k2Manager,
            IGlobalModelRepository globalModelRepository,
            IWorkflowInstanceTaskRepository workflowInstanceTaskRepository,
            IWorkflowInstanceRepository workflowInstanceRepository,
            ISecurityGroupAssignedRepository securityGroupAssignedRepository)
        {
            _authorizationService = authorizationService;
            _sharePointSecurityProvider = sharePointSecurityProvider;
            _tcMockService = tcMockService;
            _operationRepository = operationRepository;
            _k2Manager = k2Manager;
            _globalModelRepository = globalModelRepository;
            _k2IntegrationHelper = new K2IntegrationHelper(k2Manager, 
                workflowInstanceTaskRepository,
                workflowInstanceRepository);
            _securityGroupAssignedRepository = securityGroupAssignedRepository;
        }
        #endregion

        #region MockController

        public virtual ActionResult Index(string operationNumber)
        {
            var viewModel = IDBContext.Current.UserName;

            return View(model: viewModel);
        }

        public virtual ActionResult CancelWorkflow1(string folioId)
        {
            bool result = _globalModelRepository.CancelWorkflowInstance(folioId);
            ViewBag.ErrorMessage = result ? "Cancel Workflow 1 OK" : "Cancel Workflow 1 Error";
            return Json(new { ViewBag.ErrorMessage });
        }

        public virtual ActionResult CancelWorkflow2(string folioId)
        {
            bool result = _globalModelRepository.CancelWorkflowInstance(folioId);
            ViewBag.ErrorMessage = result ? "Cancel Workflow 1 OK" : "Cancel Workflow 1 Error";
            return Json(new { ViewBag.ErrorMessage });
        }

        public virtual ActionResult CompleteIFP(string operationNumber)
        {
            if (string.IsNullOrEmpty(operationNumber))
                operationNumber = IDBContext.Current.Operation;

            var response = _tcMockService.InitiateFundingProcess(
                operationNumber.Trim(), IDBContext.Current.UserName);

            return Json(new
            {
                IsValid = response.IsValid,
                ErrorMessage = response.ErrorMessage
            },
            JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult SyncFundsCoordinators()
        {
            var response = _tcMockService.SyncFundsCoordinators();

            return Json(new
            {
                IsValid = response.IsValid,
                ErrorMessage = response.ErrorMessage
            });
        }

        [HttpGet]
        public virtual JsonResult GetUserInfo(string username)
        {
            var result = Json(new
                {
                    Username = username,
                    UserFriendlyUserName = IDBContext.Current.UserName,
                    CurrentUser = IDBContext.Current.UserLoginName
                },
                JsonRequestBehavior.AllowGet);

            return result;
        }

        [HttpGet]
        public virtual JsonResult IsAuthorized(
            string operationNumber,
            string auth,
            string username)
        {
            ActionEnum actionEnum;
            Enum.TryParse(auth, out actionEnum);

            var result = Json(new
                {
                    Username = username,
                    UserFriendlyUserName = IDBContext.Current.UserName,
                    CurrentUser = IDBContext.Current.UserLoginName,
                    action = auth,
                    response = _authorizationService
                        .IsAuthorized(username, operationNumber, actionEnum)
                },
                JsonRequestBehavior.AllowGet);

            return result;
        }

        [HttpGet]
        public virtual JsonResult IsGlobalAuthorized(string auth, string username)
        {
            ActionEnum actionEnum;
            Enum.TryParse(auth, out actionEnum);

            var result = Json(new
                {
                    Username = username,
                    UserFriendlyUserName = IDBContext.Current.UserName,
                    CurrentUser = IDBContext.Current.UserLoginName,
                    action = auth,
                    response = _authorizationService.IsGlobalAuthorized(username, actionEnum)
                },
                JsonRequestBehavior.AllowGet);

            return result;
        }

        [HttpGet]
        public virtual JsonResult GetRoles(string operationNumber, string auth, string username)
        {
            List<string> roleCollection = null;

            try
            {
                roleCollection = SecurityManagerProxy.GetRoles(username, operationNumber);

                return Json(new
                    {
                        CurrentUser = IDBContext.Current.UserLoginName,
                        roles = string.Join(", ", roleCollection.ToArray())
                    },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new
                    {
                        Error_Message = e.Message,
                        Error_StackTrace = e.StackTrace
                    },
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public virtual JsonResult GetActionByRole(
            string operationNumber,
            string auth,
            string roleName)
        {
            List<string> permissionsByGroup = null;
            try
            {
                var manager = new SecurityManager(_securityGroupAssignedRepository);
                permissionsByGroup = manager.GetPermissionsByGroup(roleName, operationNumber);

                return Json(new
                    {
                        CurrentUser = IDBContext.Current.UserLoginName,
                        roles = string.Join(", ", permissionsByGroup.ToArray())
                    },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new
                    {
                        Error_Message = e.Message,
                        Error_StackTrace = e.StackTrace
                    },
                    JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}
