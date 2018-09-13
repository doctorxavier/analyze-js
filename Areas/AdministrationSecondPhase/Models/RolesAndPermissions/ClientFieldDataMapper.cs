using System;
using System.Collections.Generic;
using System.Linq;

using IDB.MW.Application.AdministrationModule.ViewModels.RolesAndPermissions;
using IDB.Presentation.MVC4.Models.ClientFieldData;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.RolesAndPermissions
{
    public static class ClientFieldDataMapper
    {
        #region Mappers

        public static void UpdateRoleInformationSaveViewModel(this RoleInformationViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            var idRolePermission = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("idRolePermission"));
            if (idRolePermission != null)
            {
                viewModel.IdRolePermission = Convert.ToInt32(idRolePermission.Value);
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
                viewModel.RoleParent = roleParent.Value != string.Empty ? Convert.ToInt32(roleParent.Value) : (int?)null;
            }

            var effectiveDate = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("FormEffectiveDate"));
            if (effectiveDate != null)
            {
                viewModel.EfectiveDate = Convert.ToDateTime(effectiveDate.Value);
            }

            var expirationDate = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("FormExpirationDate"));
            if (expirationDate != null)
            {
                if (!string.IsNullOrEmpty(expirationDate.Value))
                {
                    viewModel.ExpirationDate = Convert.ToDateTime(expirationDate.Value);
                }
                else
                {
                    viewModel.ExpirationDate = null;
                }
            }

            var roleDescription = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("FormDescription"));
            if (roleDescription != null)
            {
                viewModel.Description = roleDescription.Value;
            }

            var rolePermissionIdList = clientFieldData.Where(o => o.Name.Equals("RolePermissionId")).ToList();
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
                                              join rolePermissionIdL in rolePermissionIdList on descriptionList.IndexOf(description) equals rolePermissionIdList.IndexOf(rolePermissionIdL)
                                              select new RowAssignedPermissionsViewModel
                                              {
                                                  RolePermissionId = int.Parse(rolePermissionIdL.Value),
                                                  Description = description.Value,
                                                  CountryList = countryL.Value != null ? countryL.Value.Split(',').Select(int.Parse).ToList() : new List<int>(),
                                                  DivisionList = divisionL.Value != null ? divisionL.Value.Split(',').Select(int.Parse).ToList() : new List<int>(),
                                                  OperationTypeList = operationTypeL.Value != null ? operationTypeL.Value.Split(',').Select(int.Parse).ToList() : new List<int>(),
                                                  PermissionId = Convert.ToInt32(description.Value),
                                                  Permission = description.Value,
                                                  Type = typeL.Value == "1"
                                              }).ToList();

            var groupIdList = clientFieldData.Where(o => o.Name.Equals("GroupIdValue")).ToList();
            var securityGroupList = clientFieldData.Where(o => o.Name.Equals("SecurityGroup")).ToList();
            var gorupAssignedIdList = clientFieldData.Where(o => o.Name.Equals("RowId")).ToList();

            viewModel.CountryGroupList = (from groupIdL in groupIdList
                                          join securityGroupL in securityGroupList on groupIdList.IndexOf(groupIdL) equals securityGroupList.IndexOf(securityGroupL)
                                          join gorupAssignedIdL in gorupAssignedIdList on groupIdList.IndexOf(groupIdL) equals gorupAssignedIdList.IndexOf(gorupAssignedIdL)
                                          select new RowAssignedGroupViewModel
                                          {
                                              RowId = gorupAssignedIdL.Value != string.Empty ? int.Parse(gorupAssignedIdL.Value) : 0,
                                              GroupId = groupIdL.Value != string.Empty ? int.Parse(groupIdL.Value) : 0,
                                              SecurityGroup = securityGroupL.Value
                                          }).ToList();
        }
        #endregion

        public static void UpdateControlInformationViewModel(this ControlInformationViewModel viewModel,
            ClientFieldData[] clientFieldData)
        {
            var permission =
                   clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("NewPermission"));
            if (permission != null)
            {
                viewModel.Permission = Convert.ToInt32(permission.Value);
            }

            var page =
                   clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("NewPage"));
            if (page != null)
            {
                viewModel.PageId = Convert.ToInt32(page.Value);
            }

            var multidivision = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("NewDivision"));
            if (multidivision != null && multidivision.Value != null)
            {
                var aux = multidivision.Value.Split(',');
                viewModel.DivisionGroup = aux.Select(i => Convert.ToInt32(i)).ToList();
            }
            else
            {
                viewModel.DivisionGroup = new List<int>();
            }

            var multicountry = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("NewCountry"));
            if (multicountry != null && multicountry.Value != null)
            {
                var aux = multicountry.Value.Split(',');
                viewModel.CountryGroup = aux.Select(i => Convert.ToInt32(i)).ToList();
            }
            else
            {
                viewModel.CountryGroup = new List<int>();
            }

            var multioperationtype = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("NewOperationType"));
            if (multioperationtype != null && multioperationtype.Value != null)
            {
                var aux = multioperationtype.Value.Split(',');
                viewModel.OperationTypeGroup = aux.Select(i => Convert.ToInt32(i)).ToList();
            }
            else
            {
                viewModel.OperationTypeGroup = new List<int>();
            }

            var opeStatus = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("NewOperationStatus"));
            if (opeStatus != null && opeStatus.Value != null)
            {
                var aux = opeStatus.Value.Split(',');
                viewModel.OperationStatusGroup = aux.Select(i => Convert.ToInt32(i)).ToList();
            }
            else
            {
                viewModel.OperationStatusGroup = new List<int>();
            }

            var fieldList =
                clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("NewField")).ToList();
            var readonlyList =
                clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("NewReadOnly"))
                    .ToList();
            var requiredList =
                clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("NewRequired"))
                    .ToList();
            var hiddenList =
                clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("NewHidden"))
                    .ToList();

            var field = (from fieldId in fieldList
                        join readonlyId in readonlyList on fieldList.IndexOf(fieldId) equals readonlyList.IndexOf(readonlyId)
                        join requiredId in requiredList on fieldList.IndexOf(fieldId) equals requiredList.IndexOf(requiredId)
                        join hiddenId in hiddenList on fieldList.IndexOf(fieldId) equals hiddenList.IndexOf(hiddenId)
                        select new { fieldID = fieldId, readonlyID = readonlyId, requiredID = requiredId, hiddenID = hiddenId }).ToList();

            viewModel.TableField = (from rule in field
                                    select new TableFieldViewModel
                                    {
                                        Page = viewModel.PageId,
                                        Field = Convert.ToInt32(rule.fieldID.Value),
                                        CountryList = viewModel.CountryGroup,
                                        DivisionList = viewModel.DivisionGroup,
                                        OperationTypeList = viewModel.OperationTypeGroup,
                                        OperationStatusList = viewModel.OperationStatusGroup,
                                        ReadOnly = Convert.ToBoolean(rule.readonlyID.Value),
                                        Required = Convert.ToBoolean(rule.requiredID.Value),
                                        Hidden = Convert.ToBoolean(rule.hiddenID.Value)
                                    }).ToList();
        }

        public static void UpdateAllControlInformationViewModel(this ControlInformationViewModel viewModel,
            ClientFieldData[] clientFieldData)
        {
            var pageList = clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("PageRow")).ToList();
            var controlList = clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("FieldRow")).ToList();
            var countryList = clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("CountryRow")).ToList();
            var divisionList = clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("DivisionRow")).ToList();
            var operationTypeList = clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("OperationTypeRow")).ToList();
            var operationStatusList = clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("OperationStatusRow")).ToList();
            var readonlyList = clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("ReadOnlyRow")).ToList();
            var requiredList = clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("RequiredRow")).ToList();
            var hiddenList = clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("HiddenRow")).ToList();
            viewModel.TableField = (from pageId in pageList
                                    join controlId in controlList on pageList.IndexOf(pageId) equals controlList.IndexOf(controlId)
                                    join countryId in countryList on pageList.IndexOf(pageId) equals countryList.IndexOf(countryId)
                                    join divisionId in divisionList on pageList.IndexOf(pageId) equals divisionList.IndexOf(divisionId)
                                    join operationTypeId in operationTypeList on pageList.IndexOf(pageId) equals operationTypeList.IndexOf(operationTypeId)
                                    join operationStatusId in operationStatusList on pageList.IndexOf(pageId) equals operationStatusList.IndexOf(operationStatusId)
                                    join readonlyId in readonlyList on pageList.IndexOf(pageId) equals readonlyList.IndexOf(readonlyId)
                                    join requiredId in requiredList on pageList.IndexOf(pageId) equals requiredList.IndexOf(requiredId)
                                    join hiddenId in hiddenList on pageList.IndexOf(pageId) equals hiddenList.IndexOf(hiddenId)
                                    select new TableFieldViewModel
                                    {
                                        Page = Convert.ToInt32(pageId.Value),
                                        Field = Convert.ToInt32(controlId.Value),
                                        CountryList = countryId.Value != null ? countryId.Value.Split(',').Select(int.Parse).ToList() : new List<int>(),
                                        DivisionList = divisionId.Value != null ? divisionId.Value.Split(',').Select(int.Parse).ToList() : new List<int>(),
                                        OperationTypeList = operationTypeId.Value != null ? operationTypeId.Value.Split(',').Select(int.Parse).ToList() : new List<int>(),
                                        OperationStatusList = operationStatusId.Value != null ? operationStatusId.Value.Split(',').Select(int.Parse).ToList() : new List<int>(),
                                        ReadOnly = Convert.ToBoolean(readonlyId.Value),
                                        Required = Convert.ToBoolean(requiredId.Value),
                                        Hidden = Convert.ToBoolean(hiddenId.Value)
                                    }).ToList();       

            var permission =
                    clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("PermissionFilter"));
            if (permission != null)
            {
                viewModel.Permission = Convert.ToInt32(permission.Value);
            }
        }
    }
}