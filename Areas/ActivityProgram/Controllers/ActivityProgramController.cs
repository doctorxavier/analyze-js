using System;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;

using IDB.MW.Application.ActivityProgram.Services;
using IDB.MW.Application.ActivityProgram.Messages;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.Domain.Contracts.DomainServices;
using IDB.Presentation.MVC4.Areas.ActivityProgram.Models;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Application.ActivityProgram.ViewModels;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.MW.Domain.Models.ActivityProgram;

namespace IDB.Presentation.MVC4.Areas.ActivityProgram.Controllers
{
    public partial class ActivityProgramController : Controller
    {
        #region fields
        private readonly IActivityProgramServices _AapServices;
        private readonly ViewModelMapperHelper _viewModelMapperHelper;
        private readonly IAuthorizationManager _authorizationManager;
        #endregion

        #region Constructor
        public ActivityProgramController(
            IActivityProgramServices AapServices,
            IAuthorizationManager authorizationManager)
        {
            _AapServices = AapServices;
            _viewModelMapperHelper = new ViewModelMapperHelper(_AapServices);
            _authorizationManager = authorizationManager;
        }
        #endregion

        #region Public Method
        public virtual ActionResult Index(string operationNumber)
        {
            AAPViewModel model = new AAPViewModel
            {
                ActOp = new List<AnnualActivityModel>(),
                YearsDropDwn = Enumerable.Empty<SelectListItem>(),
                DisplayedDropDwn = Enumerable.Empty<SelectListItem>()
            };
            model.DataUser = new DataUserModel();
            var yearDefault = DateTime.Now.Year;

            try
            {
                string userName = IDBContext.Current.UserName;
                operationNumber = operationNumber ?? IDBContext.Current.Operation;
                model.OperationNumber = operationNumber;
                model.DisplayDefault = AapGlobalValues.DISPLAY_DEFAULT;
                model.DisplayedDropDwn = _viewModelMapperHelper.GetDisplayedOptions();
                model.YearNowDefault = yearDefault.ToString();
                model.CurrentYear = yearDefault.ToString();
                var rolesByUser =
                    _authorizationManager.GetRoles(
                        IDBContext.Current.UserLoginName,
                        operationNumber).ToList();
                var permissionByUserRoles =
                    _authorizationManager.GetPermissions(
                        operationNumber,
                        rolesByUser,
                        userName);
                model.AapTeamMemberRead =
                    permissionByUserRoles.Any(o => o == Permission.AAP_TEAM_MEMBER_READ);
                model.AapTeamMemberWhite =
                    permissionByUserRoles.Any(o => o == Permission.AAP_TEAM_MEMBER_WRITE);
                model.AapGlobalManagementRead =
                    IDBContext.Current.HasPermission(Permission.AAP_GLOBAL_MANAGEMENT_READ);
                model.AapGlobalManagementWhite =
                    IDBContext.Current.HasPermission(Permission.AAP_GLOBAL_MANAGEMENT_WRITE);
                model.AapActivityAdmin =
                    IDBContext.Current.HasPermission(Permission.AAP_ACTIVITY_ADMIN);

                var dataInfo = _AapServices.GetUserInfo(userName, operationNumber);
                if (!dataInfo.IsValid)
                {
                    throw new Exception("Invalid document number.");
                }

                var listWBSRow = _AapServices.GetListActivities(operationNumber,
                    dataInfo.OrganizationalUnit,
                    yearDefault);
                if (!listWBSRow.IsValid)
                {
                    throw new Exception("Invalid Activities.");
                }

                model.ActOp = listWBSRow.ListActivities;
                model.YearsDropDwn = _viewModelMapperHelper.ConvertToSelectListItem(
                    listWBSRow.YearListItem);
                model.DataUser = new DataUserModel
                {
                    UserName = dataInfo.UserId,
                    UserFullName = dataInfo.UserFullName,
                    UnitOrganizational = dataInfo.OrganizationalUnit,
                    RolesUser = dataInfo.RolesUser,
                    PermissionsUser = dataInfo.PermissionsUser
                };
            }
            catch (Exception ex)
            {
                var ErrorMesage = ex.Message;
                return PartialView("~/Areas/ActivityProgram/Views/AnnualActivityProgram.cshtml", model);
            }

            return PartialView("~/Areas/ActivityProgram/Views/AnnualActivityProgram.cshtml", model);
        }

        public virtual ActionResult EditActivityProgram(string operationNumber, ActivityProgramModel request)
        {
            AAPViewModel model = new AAPViewModel
            {
                ActOp = new List<AnnualActivityModel>(),
                YearsDropDwn = Enumerable.Empty<SelectListItem>(),
                DisplayedDropDwn = Enumerable.Empty<SelectListItem>()
            };

            try
            {
                var userName = IDBContext.Current.UserName;
                operationNumber = operationNumber ?? IDBContext.Current.Operation;
                var ListRoles = IDBContext.Current.Roles;
                model.DisplayDefault = request.Display;
                model.YearNowDefault = request.Year;
                model.PaginationDefault = request.Pagination;
                model.DisplayedDropDwn = _viewModelMapperHelper.GetDisplayedOptions();

                var dataInfo = _AapServices.GetUserInfo(userName, operationNumber);
                if (!dataInfo.IsValid)
                {
                    throw new Exception("Invalid User");
                }

                var listWBSRow = _AapServices.GetListActivities(operationNumber,
                    dataInfo.OrganizationalUnit,
                    int.Parse(request.Year));
                if (!listWBSRow.IsValid)
                {
                    throw new Exception("Invalid Activities.");
                }

                model.DataUser = new DataUserModel()
                {
                    UserName = dataInfo.UserId,
                    UserFullName = dataInfo.UserFullName,
                    UnitOrganizational = dataInfo.OrganizationalUnit,
                    RolesUser = dataInfo.RolesUser,
                    PermissionsUser = dataInfo.PermissionsUser
                };
                model.ActOp = listWBSRow.ActivitiesOperation;
                model.YearsDropDwn = _viewModelMapperHelper.ConvertToSelectListItem(
                    listWBSRow.YearListItem);
            }
            catch (Exception ex)
            {
                var ErrorMesage = ex.Message;
                return PartialView("~/Areas/ActivityProgram/Views/EditPartial/EditActivityProgram.cshtml", model);
            }

            return View("~/Areas/ActivityProgram/Views/EditPartial/EditActivityProgram.cshtml", model);
        }

        public virtual ActionResult ReadActivityProgram(string operationNumber, ActivityProgramModel request)
        {
            AAPViewModel model = new AAPViewModel();
            try
            {
                var userName = IDBContext.Current.UserName;
                operationNumber = operationNumber ?? IDBContext.Current.Operation;
                model.DisplayDefault = request.Display;
                model.DisplayedDropDwn = _viewModelMapperHelper.GetDisplayedOptions();
                model.YearNowDefault = request.Year;
                model.PaginationDefault = request.Pagination;
                model.CurrentYear = DateTime.Now.Year.ToString();

                var dataInfo = _AapServices.GetUserInfo(userName, operationNumber);
                if (!dataInfo.IsValid)
                {
                    throw new Exception("Invalid document number.");
                }

                var listWBSRow = _AapServices.GetListActivities(operationNumber,
                    dataInfo.OrganizationalUnit,
                    int.Parse(request.Year));
                if (!listWBSRow.IsValid)
                {
                    throw new Exception("Invalid Activities.");
                }

                model.DataUser = new DataUserModel()
                {
                    UserName = dataInfo.UserId,
                    UserFullName = dataInfo.UserFullName,
                    UnitOrganizational = dataInfo.OrganizationalUnit,
                    RolesUser = dataInfo.RolesUser,
                    PermissionsUser = dataInfo.PermissionsUser
                };

                model.ActOp = listWBSRow.ListActivities;
                model.YearsDropDwn = _viewModelMapperHelper.ConvertToSelectListItem(listWBSRow.YearListItem);
            }
            catch (Exception ex)
            {
                var ErrorMesage = ex.Message;
                return PartialView("~/Areas/ActivityProgram/Views/ReadPartial/ReadActivityProgram.cshtml", model);
            }

            return View("~/Areas/ActivityProgram/Views/ReadPartial/ReadActivityProgram.cshtml", model);
        }

        public virtual ActionResult SaveActivityProgram(string operationNumber, ActivityProgramModel request)
        {
            AAPViewModel model = new AAPViewModel();
            var yearDefault = DateTime.Now.Year;
            try
            {
                var userName = IDBContext.Current.UserName;
                operationNumber = operationNumber ?? IDBContext.Current.Operation;
                model.DisplayDefault = request.Display;
                model.DisplayedDropDwn = _viewModelMapperHelper.GetDisplayedOptions();
                model.PaginationDefault = request.Pagination;
                model.YearNowDefault = request.Year;
                model.CurrentYear = yearDefault.ToString();

                var dataInfo = _AapServices.GetUserInfo(userName, operationNumber);
                if (!dataInfo.IsValid)
                {
                    throw new Exception("User Invalid.");
                }

                var responseList = SaveActivityPro(operationNumber, request.ListSap);
                if (!responseList.IsValid)
                {
                    throw new Exception("User Invalid.");
                }

                var listWBSRow = _AapServices.GetListActivities(operationNumber,
                    dataInfo.OrganizationalUnit,
                    int.Parse(request.Year));

                model.DataUser = new DataUserModel()
                {
                    UserName = dataInfo.UserId,
                    UserFullName = dataInfo.UserFullName,
                    UnitOrganizational = dataInfo.OrganizationalUnit,
                    RolesUser = dataInfo.RolesUser,
                    PermissionsUser = dataInfo.PermissionsUser
                };
                model.ActOp = listWBSRow.ListActivities;
                model.YearsDropDwn = _viewModelMapperHelper.ConvertToSelectListItem(listWBSRow.YearListItem);
            }
            catch (Exception ex)
            {
                var ErrorMesage = ex.Message;
                return PartialView("~/Areas/ActivityProgram/Views/ReadPartial/ReadActivityProgram.cshtml", model);
            }

            return View("~/Areas/ActivityProgram/Views/ReadPartial/ReadActivityProgram.cshtml", model);
        }

        public ResponseBase SaveActivityPro(string operationNumber, List<SapInfoViewModel> InfoSap)
        {
            var response = new ResponseBase { IsValid = true };
            try
            {
                var userName = IDBContext.Current.UserName;
                operationNumber = operationNumber ?? IDBContext.Current.Operation;

                var dataInfo = _AapServices.GetUserInfo(userName, operationNumber);
                if (!dataInfo.IsValid)
                {
                    response.ErrorMessage = "User Invalid.";
                    response.IsValid = false;
                    return response;
                }

                if (InfoSap != null)
                {
                    var responseNew = _AapServices.SaveNewActivitiesNew(operationNumber,
                        dataInfo.OrganizationalUnit,
                        InfoSap);
                    if (!responseNew.IsValid)
                    {
                        response.ErrorMessage = responseNew.ErrorMessage;
                        response.IsValid = false;
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.IsValid = false;
            }

            return response;
        }

        public virtual ActionResult SaveActivityProgramNew(string operationNumber,
            ActivityProgramModel request)
        {
            AAPViewModel model = new AAPViewModel
            {
                ErrorSap = false,
                ErrorConvergence = false,
                ReLoadView = false
            };

            var actResponse = new SeveActResponse();
            var userName = IDBContext.Current.UserName;
            operationNumber = operationNumber ?? IDBContext.Current.Operation;

            var dataInfo = _AapServices.GetUserInfo(userName, operationNumber);
            if (!dataInfo.IsValid && dataInfo != null)
            {
                throw new Exception("User Invalid.");
            }

            if (request.ModifiedDeleteNewAct != null)
            {
                actResponse = _AapServices.SaveModifiedDeleteActInt(operationNumber,
                    dataInfo.OrganizationalUnit,
                    request.ModifiedDeleteNewAct);

                if (!actResponse.IsValid)
                {
                    model.IsValid = actResponse.IsValid;
                    model.ErrorMessage = actResponse.ErrorMessage;
                    model.ErrorConvergence = true;

                    return Json(model, JsonRequestBehavior.AllowGet);
                }
            }

            //// Enviar los datos para ser prosesados por Biztalk.
            var responseSap = _AapServices.SaveInSapNew(operationNumber,
                actResponse.ActivitiesType);
            if (!responseSap.IsValid)
            {
                model.ErrorMessage = responseSap.ErrorMessage;
                model.ErrorSap = true;

                return Json(model, JsonRequestBehavior.AllowGet);
            }

            if (responseSap.InfoSap == null)
            {
                model.ErrorMessage = "Invalid Sap List";
                model.ErrorSap = true;

                return Json(model, JsonRequestBehavior.AllowGet);
            }

            var existError = responseSap.InfoSap.Any(x => x.SapStatus == "E");
            if (existError)
            {
                model.SapInfo = new SapInfoResponse
                {
                    InfoSap = responseSap.InfoSap,
                    InfoConvergence = AapGlobalValues.UPDATED_BD_CONVERGENCE_AAP
                };
                return Json(model, JsonRequestBehavior.AllowGet);
            }

            //// validar el Save final a BBDD.
            model.ReLoadView = true;
            var responseUpConvergence = SaveActivityPro(operationNumber, responseSap.InfoSap);
            if (!responseUpConvergence.IsValid)
            {
                model.ErrorConvergence = true;
                model.ErrorMessage = responseUpConvergence.ErrorMessage;
                return Json(model, JsonRequestBehavior.AllowGet);
            }

            //// Validar la entrega de parametro a la vista .
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult AddNewAnnualActivityProgram(DataUserModel dataUser)
        {
            return PartialView("~/Areas/ActivityProgram/Views/Shared/ActivityProgramRow.cshtml", dataUser);
        }

        public virtual FileResult DownloadAnnualActivityProgramFile(string operationNumber,
            string formatType,
            string organizationalUnit,
            string year)
        {
            operationNumber = operationNumber ?? IDBContext.Current.Operation;
            var response = _AapServices.GetAnnualActivityProgramExport(operationNumber,
                formatType,
                organizationalUnit,
                year);

            if (!response.IsValid)
            {
                return null;
            }

            var fileName = string.Format("{0}_Annual_Activity_Program_File_{1}",
                operationNumber.Replace("-", "_"),
                year);
            string application;
            switch (formatType)
            {
                case AapGlobalValues.EXCEL:
                    application = MimeTypeMap.GetMimeType(AapGlobalValues.EXCEL);
                    fileName = fileName + ".xls";
                    break;
                default:
                    application = MimeTypeMap.GetMimeType(AapGlobalValues.PDF);
                    fileName = fileName + ".pdf";
                    break;
            }

            return File(response.File, application, fileName);
        }

        public virtual ActionResult SaveInSapPartial(SapInfoResponse request)
        {
            return PartialView("~/Areas/ActivityProgram/Views/Shared/SapInformation.cshtml", request);
        }
        #endregion
    }
}