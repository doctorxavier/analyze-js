using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.MW.Application.AdministrationModule.Services.DelegationService;
using IDB.MW.Application.AdministrationModule.ViewModels.Delegation;
using IDB.MW.Application.Core.ViewModels;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.Delegation
{
    public class ViewModelMapperHelper
    {
        private readonly dynamic _viewBag;
        private readonly IDelegationService _delegationService;

        public ViewModelMapperHelper(dynamic viewBag, IDelegationService delegationService)
        {
            _viewBag = viewBag;
            _delegationService = delegationService;
        }

        public DelegationViewModel GetDelegation(int delegationId)
        {
            var response = _delegationService.GetDelegation(delegationId);
            if (!response.IsValid)
            {
                _viewBag.ErrorMessage = response.ErrorMessage;
                _viewBag.LockScreen = true;
            }

            return response.Delegation;
        }

        public DelegationViewModel GetRefreshDelegation(
           string userName,
           string[] operationNumbers,
           int delegationId,
           int? roleId,
           string[] country,
           string[] cdepartment,
           string[] department,
           string[] division)
        {
            return _delegationService.RefreshDelegation(
                userName,
                operationNumbers,
                delegationId,
                roleId,
                country,
                cdepartment,
                department,
                division).Delegation;
        }

        public DelegationViewModel GetDelegablePermissionsTasks(
            string userName,
            string[] operationNumbers,
            int delegationId,
            int? roleId,
            string[] country,
            string[] cdepartment,
            string[] department,
            string[] division,
            string delegateUsername)
        {
            return _delegationService.GetDelegablePermissionsTasks(
                userName,
                operationNumbers,
                delegationId,
                roleId,
                country,
                cdepartment,
                department,
                division,
                delegateUsername).Delegation;
        }

        public List<SelectListItem> GetRoleList()
        {
            var response = new List<SelectListItem>();
            var rolesRepository = _delegationService.GetRoleList();

            if (rolesRepository.GetListItemRoles != null
                && rolesRepository.GetListItemRoles.Count > 0)
            {
                response = rolesRepository.GetListItemRoles.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.Value,
                    Text = o.Text
                }).ToList();
            }

            return response;
        }

        public List<SelectListItem> GetRoleListByUserName(string userName)
        {
            var response = new List<SelectListItem>();

            var rolesRepository =
                _delegationService.GetRoleListByUserNameAndOperationNumber(userName);

            if (rolesRepository.GetListItemRoles != null
                && rolesRepository.GetListItemRoles.Count > 0)
            {
                response = rolesRepository.GetListItemRoles.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.Value,
                    Text = o.Text
                }).ToList();
            }

            return response;
        }

        public IList<ListItemViewModel> GetDisplayedOptions()
        {
            var selectableDisplayed = new List<ListItemViewModel>
            {
                new ListItemViewModel
                {
                    Text = string.Format("{0} {1}", Localization.GetText("Display"), "5"),
                    Value = "5"
                },
                new ListItemViewModel
                {
                    Text = string.Format("{0} {1}", Localization.GetText("Display"), "10"),
                    Value = "10"
                },
                new ListItemViewModel
                {
                    Text = string.Format("{0} {1}", Localization.GetText("Display"), "50"),
                    Value = "50"
                },
                new ListItemViewModel
                {
                    Text = string.Format("{0} {1}", Localization.GetText("Display"), "All"),
                    Value = "All"
                }
            };
            return selectableDisplayed;
        }
    }
}