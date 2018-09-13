using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Diagnostics;
using IDB.Architecture;
using IDB.MW.Business.SupervisionPlan.DTOs;
using IDB.MW.Business.SupervisionPlan.Messages;
using IDB.MW.Business.SupervisionPlan.Services.Administration;
using IDB.MW.Domain.Contracts.DomainServices;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Areas.SupervisionPlan.Models;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.SupervisionPlanModule.Services;
using IDB.MW.Domain.Values;
using IDB.Architecture.Logging;
using IDB.Architecture.Language;
using IDB.Presentation.MVC4.Controllers;

namespace IDB.Presentation.MVC4.Areas.SupervisionPlan.Controllers
{
    public partial class AdministrationSpController : BaseController
    {
        private readonly ISPAdministrationService _spAdministrationService;
        private readonly ViewModelMapperHelper _viewModelMapperHelper;
        private readonly ICatalogService _catalogService;
        private readonly ISpNotificationService _notificationService;

        public AdministrationSpController(ISPAdministrationService administrationService, ICatalogService catalogService, ISpNotificationService notificationService)
        {
            _catalogService = catalogService;
            _spAdministrationService = administrationService;
            _notificationService = notificationService;
            _viewModelMapperHelper = new ViewModelMapperHelper(_spAdministrationService, _catalogService, _notificationService);
        }

        public virtual ActionResult Index(SPSearchByFilterDTO filters, string roleSelected = null)
        {
            if (Session["Display"] == null)
            {
                Session["Display"] = string.Empty;
            }

            ViewBag.Loader = true;
            ViewBag.LoaderOptional = true;
            ViewBag.RoleSelected = roleSelected;
            var model = new List<SPAdminitrationDTO>();
            var responsePrev = _spAdministrationService.GetSPPreviousMarchPMRCLass();
            var responseCurr = _spAdministrationService.GetSPCurrentMarchPMRCLass();
            var responseExc = _spAdministrationService.GetSPExcdFinExtCri();
            var responseMod = _spAdministrationService.GetSPModality();
            var responseDis = _spAdministrationService.GetSPDisplayed();
            var userName = IDBContext.Current.UserName;
            ViewBag.ListRoles = _viewModelMapperHelper.GetUserRoleList();

            var firstLevelPermission = IDBContext.Current.HasPermission(SpGlobalValues.FIRST_LEVEL);
            var secondLevelPermission = IDBContext.Current.HasPermission(SpGlobalValues.SECOND_LEVEL);
            var isRoleSelected = roleSelected != null;

            if (isRoleSelected)
            {
                firstLevelPermission = roleSelected == SpGlobalValues.ChiefofOperations;
                secondLevelPermission = roleSelected == SpGlobalValues.CountryRepresentative;
            }

            var roleResponse = _spAdministrationService.GetRole(firstLevelPermission, secondLevelPermission);

            if (roleResponse.IsValid)
            {
                if (Request.RequestType == "Get")
                {
                    Session.Remove(userName + roleSelected);
                    roleSelected = null;
                }

                if (roleSelected == null)
                {
                    if (roleResponse.Quantity == 2)
                    {
                        ViewBag.ShowModalSelectRole = true;
                        return View();
                    }

                    roleSelected = roleResponse.Role;
                    Session[userName + roleSelected] = roleSelected;
                    ViewBag.RoleSelected = roleResponse.Role;
                }
                else
                {
                    ViewBag.ShowModalSelectRole = false;
                    Session[userName + roleSelected] = roleSelected;
                }
            }
            else
            {
                return RedirectToAction("NotAccess");
            }

            var aManager = Globals.Resolve<IAuthorizationManager>();
            var securityGroup = aManager.GetAdGroups(userName);
            securityGroup =
                _spAdministrationService.LoadSecurityGroupsAccordingToPermissions(
                    securityGroup.ToList(), firstLevelPermission, secondLevelPermission);
            ViewBag.spModality = _viewModelMapperHelper.GetListMasterData(SpGlobalValues.ModalityType);
            var roleId = _spAdministrationService.GetRoleIdByRole(Session[userName + roleSelected].ToString());
            var responseUnit = _spAdministrationService.GetSPUnit(string.Join(",", securityGroup.ToArray()), Session[userName + roleSelected].ToString());

            var responseSearch = _spAdministrationService.Filters(new SearchByFiltersRequest
            {
                OperationNumber = filters.OperationNum,
                OperationName = filters.OperationName,
                ApprovalYear = filters.ApprovalYear,
                Unit = filters.Unit,
                PrevYearMarhPmrClass = filters.PrevYearMarhPmrClass,
                CurrYearMarPmrClass = filters.CurrYearMarPmrClass,
                ExFinanciatExtCri = filters.ExFinanciatExtCri,
                SpModality = filters.SpModality,
                SpModalityAfter = filters.SpModalityAfter,
                LastEditedBy = filters.LastEditedBy,
                IsRevRequired = filters.IsRevRequired == null ? false : filters.IsRevRequired,
                SecurityGroup = string.Join(",", securityGroup.ToArray()),
                RoleUser = _spAdministrationService.GetRoleIdByRole(Session[userName + roleSelected].ToString())
            });

            if (!responseUnit.IsValid)
            {
                ViewBag.ShowError = responseUnit.ErrorMessage;
                throw new Exception(responseUnit.ErrorMessage);
            }

            if (!responsePrev.IsValid)
            {
                ViewBag.ShowError = responsePrev.ErrorMessage;
                throw new Exception(responsePrev.ErrorMessage);
            }

            if (!responseCurr.IsValid)
            {
                ViewBag.ShowError = responseCurr.ErrorMessage;
                throw new Exception(responseCurr.ErrorMessage);
            }

            if (!responseExc.IsValid)
            {
                ViewBag.ShowError = responseExc.ErrorMessage;
                throw new Exception(responseExc.ErrorMessage);
            }

            if (!responseMod.IsValid)
            {
                ViewBag.ShowError = responseMod.ErrorMessage;
                throw new Exception(responseMod.ErrorMessage);
            }

            if (!responseDis.IsValid)
            {
                ViewBag.ShowError = responseDis.ErrorMessage;
                throw new Exception(responseDis.ErrorMessage);
            }

            var result = new SPAdministrationSearchViewModel
            {
                UnitSearch = responseUnit.Units,
                PrevMarchPMRClass = responsePrev.PrevMarchPMRClass,
                CurrMarchPMRClass = responseCurr.CurrMarchPMRClass,
                ExceedFinExtCri = responseExc.ExceedFinExtCri,
                SPModality = responseMod.SPModality,
                Displayed = responseDis.Displayed,
                ResultsSearch = responseSearch.Results,
                SearchByFilters = filters,
                ParentRelationship = _spAdministrationService.ResolveRelatedOperationsWithStatus(
                    responseSearch.Results.Select(r => r.OperationNumber).ToList()).Models
            };

            ViewBag.SerializedAdministration = PageSerializationHelper.SerializeObject(model);

            ViewBag.CountryName = _spAdministrationService.GetCountryNames(securityGroup.ToList());

            var canEdit = _spAdministrationService.GetPermissionEdit(Session[userName + roleSelected].ToString());
            if (!canEdit.IsValid)
            {
                ViewBag.ShowError = canEdit.ErrorMessage;
            }

            var lstOperationSpStatus = _viewModelMapperHelper.GetItemsCodeMasterData(SpGlobalValues.OperationSpStatusType);
            canEdit = _spAdministrationService.RevalidateEditPermissions(
                result.ResultsSearch,
                canEdit,
                Session[userName + roleSelected].ToString(),
                lstOperationSpStatus, string.Join(",",
                securityGroup.ToArray()),
                _spAdministrationService.GetRoleIdByRole(Session[userName + roleSelected].ToString()));

            bool canSubmit = true;
            if (canEdit.CanEdit && Session[userName + roleSelected].ToString().Equals(SpGlobalValues.CountryRepresentative))
            {
                canSubmit = _spAdministrationService.GetSubmitPermission(canEdit.CurrentPeriod, securityGroup.ToList());
            }

            ViewBag.CooRole = _spAdministrationService.GetRoleIdByRole(SpGlobalValues.ChiefofOperations);
            ViewBag.CrRole = _spAdministrationService.GetRoleIdByRole(SpGlobalValues.CountryRepresentative);
            ViewBag.UserRole = roleId;
            ViewBag.ActualCycle = canEdit.CurrentPeriod;
            ViewBag.isPermissionForEdit = canEdit.CanEdit;
            ViewBag.currentYear = canEdit.CurrentYear;
            ViewBag.DetailedId = _spAdministrationService.GetIdDetailed(SpGlobalValues.DetailedId);
            ViewBag.lstOperationSpStatus = lstOperationSpStatus;
            ViewBag.SortIndex = filters.OrderIndex;
            ViewBag.SortCount = filters.OrderCount;
            ViewBag.CanSubmit = canSubmit;
            ViewBag.UserName = userName;
            var textoModal = string.Empty;
            switch (canEdit.CurrentPeriod)
            {
                case 1:
                    textoModal = Session[userName + roleSelected].ToString() == SpGlobalValues.ChiefofOperations ? Localization.GetText("SP.Administration.Warning.Coo1") : Localization.GetText("SP.Administration.Warning.Cr1");
                    break;
                case 2:
                    textoModal = Session[userName + roleSelected].ToString() == SpGlobalValues.ChiefofOperations ? Localization.GetText("SP.Administration.Warning.Coo2") : Localization.GetText("SP.Administration.Warning.Cr2");
                    break;
            }

            ViewBag.LiteralModal = textoModal;

            return View(result);
        }

        public virtual PartialViewResult GetJustificationNewRow()
        {
            return PartialView("Partials/NewJustification");
        }

        public virtual PartialViewResult GetCommentBox()
        {
            return PartialView("Partials/CommentBox");
        }

        public virtual ActionResult NotAccess()
        {
            return View();
        }

        public virtual PartialViewResult GetJustifications(string operationNumber, int currentYear)
        {
            var model = _spAdministrationService.GetJustifications(operationNumber, currentYear);
            return PartialView("Partials/Justifications", model.Justifications);
        }

        public virtual JsonResult SaveOperationPlan(List<SPSaveViewModel> dataSave, List<SPSaveViewModel> operationNumberSave, List<SPJustifications> justificationSave, SPSearchByFilterDTO filters, bool submit, int currentPeriod, int currentYear, string roleSelected)
        {
            var userName = IDBContext.Current.UserName;
            var role = Session[userName + roleSelected].ToString();
            var roleUser = _spAdministrationService.GetRoleIdByRole(Session[userName + roleSelected].ToString());
            var getSecurityGroup = Globals.Resolve<IAuthorizationManager>().GetAdGroups(userName);
            getSecurityGroup = _spAdministrationService.FilterSecurityGroups(getSecurityGroup, roleUser);
            var securityGroup = string.Join(",", getSecurityGroup.ToArray());

            if (currentPeriod == 1)
            {
                if (role == SpGlobalValues.ChiefofOperations)
                {
                    if (dataSave != null)
                    {
                        foreach (var item in dataSave)
                        {
                            var existData = _viewModelMapperHelper.GetSupervisionPlanModality(item.OperationNumber);
                            if (existData.IsValid)
                            {
                                if (item.Value != item.ModalityValues)
                                {
                                    _viewModelMapperHelper.UpdateSupervisionPlanModality(item, role);
                                }
                            }
                            else
                            {
                                _viewModelMapperHelper.SaveSupervisionPlanModality(item, role);
                            }
                        }
                    }

                    if (justificationSave != null)
                    {
                        foreach (var item in justificationSave)
                        {
                            var idSupervisionPlan =
                                _viewModelMapperHelper.GetSupervisionPlanId(item.OperationNumber, currentYear);

                            if (idSupervisionPlan == 0)
                            {
                                idSupervisionPlan =
                                    _viewModelMapperHelper.CreateNewSupervisionPlan(item.OperationNumber,
                                        currentYear);
                            }

                            _spAdministrationService.SaveJustifications(item.Value, idSupervisionPlan);
                        }
                    }

                    if (submit)
                    {
                        _spAdministrationService.SupervisionPlanSubmit(filters.OperationNum, filters.OperationName, filters.ApprovalYear.ToString(), filters.Unit, filters.PrevYearMarhPmrClass, filters.CurrYearMarPmrClass, filters.ExFinanciatExtCri, filters.SpModality, filters.SpModalityAfter, filters.LastEditedBy, securityGroup, roleUser, filters.IsRevRequired, currentYear, userName, role, currentPeriod);
                        _viewModelMapperHelper.SendMessageCountryRepresentativeBySecurityGroup(getSecurityGroup.ToList());
                    }
                }
                else
                {
                    if (dataSave != null)
                    {
                        foreach (var item in dataSave)
                        {
                            var existData = _viewModelMapperHelper.GetSupervisionPlanModality(item.OperationNumber);
                            if (existData.IsValid)
                            {
                                _viewModelMapperHelper.UpdateSupervisionPlanModality(item, role);
                            }
                            else
                            {
                                _viewModelMapperHelper.SaveSupervisionPlanModality(item, role);
                            }
                        }
                    }

                    if (justificationSave != null)
                    {
                        foreach (var item in justificationSave)
                        {
                            var idSupervisionPlan =
                                _viewModelMapperHelper.GetSupervisionPlanId(item.OperationNumber, currentYear);

                            if (idSupervisionPlan == 0)
                            {
                                idSupervisionPlan =
                                    _viewModelMapperHelper.CreateNewSupervisionPlan(item.OperationNumber,
                                        currentYear);
                            }

                            _spAdministrationService.SaveJustifications(item.Value, idSupervisionPlan);
                        }
                    }

                    if (submit)
                    {
                        var response = _spAdministrationService.SupervisionPlanSubmit(
                            filters.OperationNum,
                            filters.OperationName,
                            filters.ApprovalYear.ToString(),
                            filters.Unit,
                            filters.PrevYearMarhPmrClass,
                            filters.CurrYearMarPmrClass,
                            filters.ExFinanciatExtCri,
                            filters.SpModality,
                            filters.SpModalityAfter,
                            filters.LastEditedBy,
                            securityGroup,
                            roleUser,
                            filters.IsRevRequired,
                            currentYear,
                            userName,
                            role,
                            currentPeriod);

                        _viewModelMapperHelper.SendMessageTeamLeader(response.SPCreatedOperations);
                        _viewModelMapperHelper.SendMessageChiefOperationsBySecurityGroup(getSecurityGroup.ToList());
                    }
                }
            }
            else
            {
                if (role == SpGlobalValues.ChiefofOperations)
                {
                    if (dataSave != null)
                    {
                        foreach (var item in dataSave)
                        {
                            var existData = _viewModelMapperHelper.GetSupervisionPlanModality(item.OperationNumber);
                            if (existData.IsValid)
                            {
                                if (item.Value != item.ModalityValues)
                                {
                                    _viewModelMapperHelper.UpdateSupervisionPlanModality(item, role);
                                }
                            }
                            else
                            {
                                _viewModelMapperHelper.SaveSupervisionPlanModality(item, role);
                            }
                        }
                    }

                    if (justificationSave != null)
                    {
                        foreach (var item in justificationSave)
                        {
                            var idSupervisionPlan =
                                _viewModelMapperHelper.GetSupervisionPlanId(item.OperationNumber, currentYear);

                            if (idSupervisionPlan == 0)
                            {
                                idSupervisionPlan =
                                    _viewModelMapperHelper.CreateNewSupervisionPlan(item.OperationNumber,
                                        currentYear);
                            }

                            _spAdministrationService.SaveJustifications(item.Value, idSupervisionPlan);
                        }
                    }

                    if (submit)
                    {
                        _spAdministrationService.SupervisionPlanSubmit(filters.OperationNum, filters.OperationName, filters.ApprovalYear.ToString(), filters.Unit, filters.PrevYearMarhPmrClass, filters.CurrYearMarPmrClass, filters.ExFinanciatExtCri, filters.SpModality, filters.SpModalityAfter, filters.LastEditedBy, securityGroup, roleUser, filters.IsRevRequired, currentYear, userName, role, currentPeriod);
                        _viewModelMapperHelper.SendMessageCountryRepresentativeBySecurityGroup(getSecurityGroup.ToList(), false);
                    }
                }
                else
                {
                    if (dataSave != null)
                    {
                        foreach (var item in dataSave)
                        {
                            var existData = _viewModelMapperHelper.GetSupervisionPlanModality(item.OperationNumber);
                            if (existData.IsValid)
                            {
                                if (item.Value != item.ModalityValues)
                                {
                                    _viewModelMapperHelper.UpdateSupervisionPlanModality(item, role);
                                }
                            }
                            else
                            {
                                _viewModelMapperHelper.SaveSupervisionPlanModality(item, role);
                            }
                        }
                    }

                    if (justificationSave != null)
                    {
                        foreach (var item in justificationSave)
                        {
                            var idSupervisionPlan =
                                _viewModelMapperHelper.GetSupervisionPlanId(item.OperationNumber, currentYear);

                            if (idSupervisionPlan == 0)
                            {
                                idSupervisionPlan =
                                    _viewModelMapperHelper.CreateNewSupervisionPlan(item.OperationNumber,
                                        currentYear);
                            }

                            _spAdministrationService.SaveJustifications(item.Value, idSupervisionPlan);
                        }
                    }

                    if (submit)
                    {
                        var response = _spAdministrationService.SupervisionPlanSubmit(filters.OperationNum, filters.OperationName, filters.ApprovalYear.ToString(), filters.Unit, filters.PrevYearMarhPmrClass, filters.CurrYearMarPmrClass, filters.ExFinanciatExtCri, filters.SpModality, filters.SpModalityAfter, filters.LastEditedBy, securityGroup, roleUser, filters.IsRevRequired, currentYear, userName, role, currentPeriod);
                        _viewModelMapperHelper.SendMessageTeamLeader(response.SPCreatedOperations);
                        _viewModelMapperHelper.SendMessageTeamLeader(response.SPUpdatedOperations, true);
                        _viewModelMapperHelper.SendMessageChiefOperationsBySecurityGroup(getSecurityGroup.ToList(), false);
                    }
                }
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult SearchResultAdministration(SPSearchByFilterDTO filters, string roleSelected)
        {
            ViewBag.Loader = true;
            var model = new List<SPAdminitrationDTO>();
            var responsePrev = _spAdministrationService.GetSPPreviousMarchPMRCLass();
            var responseCurr = _spAdministrationService.GetSPCurrentMarchPMRCLass();
            var responseExc = _spAdministrationService.GetSPExcdFinExtCri();
            var responseMod = _spAdministrationService.GetSPModality();
            var responseDis = _spAdministrationService.GetSPDisplayed();
            var userName = IDBContext.Current.UserName;
            var aManager = Globals.Resolve<IAuthorizationManager>();
            var securityGroup = aManager.GetAdGroups(userName);
            ViewBag.spModality = _viewModelMapperHelper.GetListMasterData(SpGlobalValues.ModalityType);
            var responseUnit = _spAdministrationService.GetSPUnit(string.Join(",", securityGroup.ToArray()), Session[userName + roleSelected].ToString());

            var responseSearch = _spAdministrationService.Filters(new SearchByFiltersRequest
            {
                OperationNumber = filters.OperationNum,
                OperationName = filters.OperationName,
                ApprovalYear = filters.ApprovalYear,
                Unit = filters.Unit,
                PrevYearMarhPmrClass = filters.PrevYearMarhPmrClass,
                CurrYearMarPmrClass = filters.CurrYearMarPmrClass,
                ExFinanciatExtCri = filters.ExFinanciatExtCri,
                SpModality = filters.SpModality,
                SpModalityAfter = filters.SpModalityAfter,
                LastEditedBy = filters.LastEditedBy,
                IsRevRequired = filters.IsRevRequired,
                SecurityGroup = string.Join(",", securityGroup.ToArray()),
                RoleUser = _spAdministrationService.GetRoleIdByRole(Session[userName + roleSelected].ToString())
            });

            if (!responseUnit.IsValid)
            {
                ViewBag.ShowError = responseUnit.ErrorMessage;
                throw new Exception(responseUnit.ErrorMessage);
            }

            if (!responsePrev.IsValid)
            {
                ViewBag.ShowError = responsePrev.ErrorMessage;
                throw new Exception(responsePrev.ErrorMessage);
            }

            if (!responseCurr.IsValid)
            {
                ViewBag.ShowError = responseCurr.ErrorMessage;
                throw new Exception(responseCurr.ErrorMessage);
            }

            if (!responseExc.IsValid)
            {
                ViewBag.ShowError = responseExc.ErrorMessage;
                throw new Exception(responseExc.ErrorMessage);
            }

            if (!responseMod.IsValid)
            {
                ViewBag.ShowError = responseMod.ErrorMessage;
                throw new Exception(responseMod.ErrorMessage);
            }

            if (!responseDis.IsValid)
            {
                ViewBag.ShowError = responseDis.ErrorMessage;
                throw new Exception(responseDis.ErrorMessage);
            }

            var result = new SPAdministrationSearchViewModel
            {
                UnitSearch = responseUnit.Units,
                PrevMarchPMRClass = responsePrev.PrevMarchPMRClass,
                CurrMarchPMRClass = responseCurr.CurrMarchPMRClass,
                ExceedFinExtCri = responseExc.ExceedFinExtCri,
                SPModality = responseMod.SPModality,
                Displayed = responseDis.Displayed,
                ResultsSearch = responseSearch.Results,
                SearchByFilters = filters
            };

            ViewBag.SerializedAdministration = PageSerializationHelper.SerializeObject(model);

            var canEdit = _spAdministrationService.GetPermissionEdit(Session[userName + roleSelected].ToString());
            if (!canEdit.IsValid)
            {
                ViewBag.ShowError = canEdit.ErrorMessage;
            }

            var lstOperationSpStatus = _viewModelMapperHelper.GetItemsCodeMasterData(SpGlobalValues.OperationSpStatusType);
            canEdit = _spAdministrationService.RevalidateEditPermissions(
                result.ResultsSearch,
                canEdit,
                Session[userName + roleSelected].ToString(),
                lstOperationSpStatus, string.Join(",",
                securityGroup.ToArray()),
                _spAdministrationService.GetRoleIdByRole(Session[userName + roleSelected].ToString()));

            ViewBag.UserRole = _spAdministrationService.GetRoleIdByRole(Session[userName + roleSelected].ToString());
            ViewBag.ActualCycle = canEdit.CurrentPeriod;
            ViewBag.isPermissionForEdit = canEdit.CanEdit;
            ViewBag.currentYear = canEdit.CurrentYear;
            ViewBag.DetailedId = _spAdministrationService.GetIdDetailed(SpGlobalValues.DetailedId);
            ViewBag.lstOperationSpStatus = lstOperationSpStatus;

            return View("Partials/SearchResultAdministration", result);
        }

        public virtual JsonResult ChangeDisplay(string number)
        {
            Session["Display"] = number;
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public virtual PartialViewResult UserRoleSelect()
        {
            ViewBag.ListRoles = _viewModelMapperHelper.GetUserRoleList();

            return PartialView("Partials/SelectRolePartial");
        }
    }
}