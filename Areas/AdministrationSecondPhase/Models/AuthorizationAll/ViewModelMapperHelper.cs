using System;
using System.Collections.Generic;
using System.Linq;

using IDB.Architecture.Extensions;
using IDB.Architecture.Logging;
using IDB.MW.Domain.Session;
using IDB.MW.Application.AdministrationModule.Messages.DelegationService;
using IDB.MW.Application.AdministrationModule.ViewModels.Delegation;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.AuthorizationAll
{
    public class ViewModelMapperHelper
    {
        public AuthorizationAllViewModel AuthorizeAllToViewModel(GetListTableDelegatorResponse response)
        {
            var model = new AuthorizationAllViewModel
            {
                AuthorizationAllView = new AuthorizationAllTableViewModel
                {
                    AuthorizationTable = new List<AuthorizationAllTableRowViewModel>(),
                    UnavailableDates = response.UnavailableDates,
                    IsAuthorize = response.IsAuthorize
                },
                TableDocument = new List<TableDocumentViewModel>(),
                UserToAssign = new UserToAssignViewModel(),
                Filter = new DelegationFilterViewModel
                {
                    IsEditable = true
                }
            };

            try
            {
                model.IsEditable = true;
                model.IsAdmin = IDBContext.Current.HasPermission("Delegation Write");

                if (response.IsValid)
                {
                    model.CommentsDelegation = new List<CommentDelegationViewModel>();
                    if (response.GetListTableDelegator.HasAny())
                    {
                        var viewModelList = response.GetListTableDelegator
                       .Select(o => new AuthorizationAllTableRowViewModel
                       {
                           Status = true,
                           FullName = o.FullName,
                           Username = o.UserName,
                           OrganizationalUnit = o.OrganizationalUnit,
                           OrganizationalUnitID = o.OrgUnitId,
                           Country = o.Country,
                           CountryDepartment = o.CountryDept,
                           Department = o.Dept,
                           Division = o.Division,
                           Global = o.Global,
                           OperationNumber = o.OperationNumber,
                           OperationId = o.OperationId,
                           Role = o.RoleName,
                           RoleId = o.RoleId
                       });
                        model.AuthorizationAllView.AuthorizationTable.AddRange(viewModelList);
                    }                   
                }
            }
            catch (System.Exception ex)
            {
                Logger.GetLogger().WriteError("ViewModelMapperHelper", "AuthorizeAllToViewModel: ", ex);
            }

            return model;
        }

        public AuthorizationAllViewModel AuthorizeAllToViewModel(DelegationAuthorizeAllResponse response)
        {
            var model = new AuthorizationAllViewModel
            {
                AuthorizationAllView = new AuthorizationAllTableViewModel
                {
                    AuthorizationTable = new List<AuthorizationAllTableRowViewModel>(),
                    UnavailableDates = new List<DateTime>()
                },
                TableDocument = new List<TableDocumentViewModel>(),
                UserToAssign = new UserToAssignViewModel(),
                Filter = new DelegationFilterViewModel(),
                CommentsDelegation = new List<CommentDelegationViewModel>(),
            };

            try
            {
                model.IsEditable = response.AuthorizeAll.DelegationId == 0 || response.AuthorizeAll.IsEditable;
                model.IsAdmin = IDBContext.Current.HasPermission("Delegation Write");            
                if (response.IsValid)
                {
                    model.DelegationId = response.AuthorizeAll.DelegationId;
                    if (response.AuthorizeAll != null && response.AuthorizeAll.CommentsDelegation != null)
                    {
                        model.CommentsDelegation = response.AuthorizeAll.CommentsDelegation;
                    }
                    
                    var viewModelList = response.AuthorizeAll.GetListTableDelegator
                        .Select(o => new AuthorizationAllTableRowViewModel
                        {
                            Status = true,
                            FullName = o.FullName,
                            Username = o.UserName,
                            OrganizationalUnit = o.OrganizationalUnit,
                            OrganizationalUnitID = o.OrgUnitId,
                            Country = o.Country,
                            CountryDepartment = o.CountryDept,
                            Department = o.Dept,
                            Division = o.Division,
                            Global = o.Global,
                            OperationNumber = o.OperationNumber,
                            OperationId = o.OperationId,
                            Role = o.RoleName,
                            RoleId = o.RoleId
                        });
                    model.AuthorizationAllView.AuthorizationTable.AddRange(viewModelList);
                    model.TableDocument = response.AuthorizeAll.Documents.ToList();
                    model.UserToAssign = response.AuthorizeAll.UserToAssign;
                    model.Filter = response.AuthorizeAll.Filter;
                }
            }
            catch (System.Exception ex)
            {
                Logger.GetLogger().WriteError("ViewModelMapperHelper", "AuthorizeAllToViewModel: ", ex);
            }

            return model;
        }
    }
}