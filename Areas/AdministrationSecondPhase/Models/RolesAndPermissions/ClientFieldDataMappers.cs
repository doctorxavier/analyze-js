
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.Presentation.MVC4.Models.ClientFieldData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using IDB.MW.Application.AdministrationModule.ViewModels.RolesAndPermissions;
using IDB.MW.Application.AdministrationModule.Services.RolesAndPermissionsService;
using IDB.MW.Application.AdministrationModule.Messages.RolesAndPermissionsService;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.RolesAndPermissions
{
    public static class ClientFieldDataMappers
    {
        #region Mappers

        //private readonly dynamic _viewBag;
        //private readonly ViewModelMapperHelper _viewModelMapperHelper;

        public static void UpdateRoleInformationSaveViewModel(this RoleInformationViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            var idRolePermission = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("idRolePermission"));
            if (idRolePermission != null)
            {
                viewModel.IdRolePermission = Convert.ToInt32(idRolePermission.Value);
            }

            var code = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("FormRoleCodeTextField"));
            if (code != null)
            {
                viewModel.Code = code.Value;
            }


            var roleName = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("FormRoleNameTextField"));
            if (roleName != null)
            {
                viewModel.RoleName = roleName.Value;
            }

            var roleType = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("listRoleType"));
            if (roleType != null)
            {
                viewModel.RoleType = Convert.ToInt32(roleType.Value);
            }

            var roleParent = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("listRoleParent"));
            if (roleParent != null)
            {
                viewModel.RoleParent = roleParent.Value != "" ? Convert.ToInt32(roleParent.Value) : 0;
            }

            var effectiveDate = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("FormEffectiveDate"));
            if (effectiveDate != null)
            {
                viewModel.EfectiveDate = Convert.ToDateTime(effectiveDate.Value);
            }

            var expirationDate = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("FormExpirationDate"));
            if (expirationDate != null)
            {
                viewModel.ExpirationDate = Convert.ToDateTime(expirationDate.Value);
            }

            var roleDescription = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("FormDescription"));
            if (roleDescription != null)
            {
                viewModel.Description = roleDescription.Value;
            }

            var descriptionList = clientFieldData.Where(o => o.Name.Equals("listPermission")).ToList();
            var countryList = clientFieldData.Where(o => o.Name.Equals("listCountry")).ToList();
            var divisionList = clientFieldData.Where(o => o.Name.Equals("listDivision")).ToList();
            var operationTypeList = clientFieldData.Where(o => o.Name.Equals("listOperationType")).ToList();
            var typeList = clientFieldData.Where(o => o.Name.Equals("listType")).ToList();

            viewModel.GlobalPermissionList = (from description in descriptionList
                                          join countryL in countryList on descriptionList.IndexOf(description) equals countryList.IndexOf(countryL)
                                          join divisionL in divisionList on descriptionList.IndexOf(description) equals divisionList.IndexOf(divisionL)
                                          join operationTypeL in operationTypeList on descriptionList.IndexOf(description) equals operationTypeList.IndexOf(operationTypeL)
                                          join typeL in typeList on descriptionList.IndexOf(description) equals typeList.IndexOf(typeL)
                                          select new RowAssignedPermissionsViewModel
                                        {
                                            Country = int.Parse(countryL.Value),
                                            Description = description.Value,
                                            Division = int.Parse(divisionL.Value),
                                            OperationType = int.Parse(operationTypeL.Value),
                                            Permission = description.Value,
                                            Type = typeL.Value == "1" ? true : false

                                        }).ToList();

            var groupIdList = clientFieldData.Where(o => o.Name.Equals("GroupIdValue")).ToList();
            var groupNameList = clientFieldData.Where(o => o.Name.Equals("dropdownfilter")).ToList();
            var securityGroupList = clientFieldData.Where(o => o.Name.Equals("dropdownfilter_text")).ToList();

            viewModel.CountryGroupList = (from groupIdL in groupIdList
                                          join groupNameL in groupNameList on groupIdList.IndexOf(groupIdL) equals groupNameList.IndexOf(groupNameL)
                                          join securityGroupL in securityGroupList on groupIdList.IndexOf(groupIdL) equals securityGroupList.IndexOf(securityGroupL)
                                          where groupNameL.Value != ""
                                          select new RowAssignedGroupViewModel
                                              {
                                                  GroupId = groupIdL.Value != "" ? int.Parse(groupIdL.Value) : 0,
                                                  GroupName = groupNameL.Value,
                                                  SecurityGroup = securityGroupL.Value
                                              }).ToList();
        }
        #endregion

        

    }
}