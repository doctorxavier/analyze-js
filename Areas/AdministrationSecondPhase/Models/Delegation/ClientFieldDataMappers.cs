using System;
using System.Linq;
using System.Collections.Generic;

using IDB.MW.Application.AdministrationModule.ViewModels.Delegation;
using IDB.Presentation.MVC4.Models.ClientFieldData;
using IDB.Presentation.MVC4.Helpers;
using IDB.Architecture;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Domain.Values;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values.Delegation;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.Delegation
{
    public static class ClientFieldDataMappers
    {
        public static UserToAssignViewModel ConvertoToUsertToAssignViewModel(
            this IDB.MW.Application.AdministrationModule.ViewModels.Delegation
            .UserToAssignViewModel usertToAssign)
        {
            var model = new UserToAssignViewModel
            {
                AssignedUserFullName = usertToAssign.AssignedUserFullName,
                AssignedUserName = usertToAssign.AssignedUserName,
                EndDate = usertToAssign.EndDate,
                Reason = usertToAssign.Reason,
                StartDate = usertToAssign.StartDate,
                Type = usertToAssign.Type
            };

            var _catalogService = Globals.Resolve<ICatalogService>();

            model.DisplayReasons = _catalogService.GetListMasterData(MasterType.DELEGATION_REASON);

            if (model.Reason > 0)
            {
                var reasonModel = _catalogService.GetConvergenceMasterDataById(model.Reason).Model;
                switch (IDBContext.Current.CurrentLanguage.ToLower())
                {
                    case "ES":
                        model.DisplayReason = reasonModel.NameEs.Length > 0
                            ? reasonModel.NameEs : reasonModel.NameEn;
                        break;
                    case "PT":
                        model.DisplayReason = reasonModel.NamePt.Length > 0
                            ? reasonModel.NameEs : reasonModel.NameEn;
                        break;
                    case "FR":
                        model.DisplayReason = reasonModel.NameFr.Length > 0
                            ? reasonModel.NameEs : reasonModel.NameEn;
                        break;
                    default:
                        model.DisplayReason = reasonModel.NameEn;
                        break;
                }
            }

            var delegationTypesToBeExcluded = new List<string>
            {
                "DELEGATION"
            };

            model.DisplayTypes = _catalogService.GetListMasterData(MasterType.DELEGATION_TYPE,
                excludeByCode: delegationTypesToBeExcluded);
            if (model.Type == 0)
            {
                var responseType = _catalogService.GetConvergenceMasterDataIdByCode(
                    DelegationGlobalValues.AUTHORIZATION, MasterType.DELEGATION_TYPE);

                if (responseType.IsValid)
                {
                    model.Type = responseType.Id;
                }
            }

            return model;
        }

        public static AuthorizeAllViewModel ConvertToAppModel(
            this AuthorizationAll.AuthorizationAllViewModel model)
        {
            var response = new AuthorizeAllViewModel
            {
                UserName = model.UserName,
                UserToAssign = model.UserToAssign,
                CommentsDelegation = model.CommentsDelegation,
                Documents = model.TableDocument,
                DelegationId = model.DelegationId
            };

            return response;
        }

        public static void UpdateDelegationViewModel(this DelegationViewModel viewModel,
            ClientFieldData[] clientFieldData)
        {
            viewModel.TableDelegator = ConvertTodelegatorTableViewModel(clientFieldData);

            foreach (var country in viewModel.AssignSubDelegation.Countries)
            {
                var countryView = clientFieldData.FirstOrDefault(o =>
                !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(country.CountryControlName));
                if (countryView != null)
                {
                    country.CountrySelected = Convert.ToBoolean(countryView.Value);
                }
            }

            foreach (var division in viewModel.AssignSubDelegation.Divisions)
            {
                var divisionView = clientFieldData.FirstOrDefault(o =>
                !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(division.DivisionControlName));
                if (divisionView != null)
                {
                    division.DivisionSelected = Convert.ToBoolean(divisionView.Value);
                }
            }

            viewModel.AssignSubDelegation.AllDivisionSelected =
                !viewModel.AssignSubDelegation.Divisions.Any(x => !x.DivisionSelected);

            foreach (var operationType in viewModel.AssignSubDelegation.OperationTypes)
            {
                var operationTypeView = clientFieldData
                    .FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name)
                    && o.Name.Equals(operationType.OperationTypeControlName));
                if (operationTypeView != null)
                {
                    operationType.OperationTypeSelected =
                        Convert.ToBoolean(operationTypeView.Value);
                }
            }

            viewModel.ConvertDelegationByAmountToViewModel(clientFieldData);

            var assignedUser = clientFieldData.FirstOrDefault(o =>
            !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("SearchDelegateUser"));
            if (assignedUser != null)
            {
                viewModel.UserToAssign.AssignedUserName = assignedUser.Value;
            }

            var startDate = clientFieldData.FirstOrDefault(o =>
            !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("startDate"));
            if (startDate != null)
            {
                viewModel.UserToAssign.StartDate = Convert.ToDateTime(startDate.Value);
            }

            var endDate = clientFieldData.FirstOrDefault(o =>
            !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("endDate"));
            if (endDate != null)
            {
                viewModel.UserToAssign.EndDate = Convert.ToDateTime(endDate.Value);
            }

            var delegationType = clientFieldData.FirstOrDefault(o =>
            !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("delegationType"));
            if (delegationType != null)
            {
                viewModel.UserToAssign.Type = Convert.ToInt32(delegationType.Value);
            }

            var delegationReason = clientFieldData.FirstOrDefault(o =>
            !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("delegationReason"));
            if (delegationReason != null)
            {
                viewModel.UserToAssign.Reason = Convert.ToInt32(delegationReason.Value);
            }

            viewModel.CommentsDelegation = ConvertCommentsDelegationViewModel(clientFieldData);
            viewModel.TableDocument = ConvertToTableDocument(clientFieldData);
            viewModel.ListOfOperations =
                clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name)
                && o.Name.Equals("operationsToBeSaved")).Value.Split(',')
                .Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
        }

        private static List<TableDelegatorViewModel> ConvertTodelegatorTableViewModel(
            ClientFieldData[] clientFieldData)
        {
            var delegatorTable = new List<TableDelegatorViewModel>();
            var selectDelegator =
                clientFieldData.Where(o =>
                    !string.IsNullOrWhiteSpace(o.Name) &&
                    o.Name.Equals("SelectDelegatorHidden") && o.Value.Contains("|-"));

            if (selectDelegator != null)
            {
                var operationsId = new List<string>();
                var rolesId = new List<string>();

                foreach (var delegator in selectDelegator)
                {
                    operationsId.Add(delegator.Value.Split('|')[0]);
                    rolesId.Add(delegator.Value.Split('|')[1]);
                }

                var operationIdList = clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name)
                && o.Name.Equals("operationIdDelegator")).ToList();
                var orgUnitIdList = clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name)
                && o.Name.Equals("orgUnitIdDelegator")).ToList();
                var roleIdList = clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name)
                && o.Name.Equals("roleIdDelegator")).ToList();
                var userNameList = clientFieldData.Where(o => !string.IsNullOrWhiteSpace(o.Name)
                && o.Name.Equals("userNameDelegator")).ToList();

                delegatorTable = (from operationIdValue in operationIdList
                                      join orgUnitIdValue in orgUnitIdList
                                      on operationIdList.IndexOf(operationIdValue)
                                      equals orgUnitIdList.IndexOf(orgUnitIdValue)
                                      join roleIdValue in roleIdList
                                      on operationIdList.IndexOf(operationIdValue)
                                      equals roleIdList.IndexOf(roleIdValue)
                                      join userNameValue in userNameList
                                      on operationIdList.IndexOf(operationIdValue)
                                      equals userNameList.IndexOf(userNameValue)
                                      where ((operationsId.Any(o => o == operationIdValue.Value)
                                      || operationsId.Any(o => o == orgUnitIdValue.Value))
                                      && rolesId.Any(o => o == roleIdValue.Value))
                                      select new TableDelegatorViewModel
                                      {
                                          OperationId = Convert.ToInt32(operationIdValue.Value),
                                          OrgUnitId = Convert.ToInt32(orgUnitIdValue.Value),
                                          RoleId = Convert.ToInt32(roleIdValue.Value),
                                          UserName = userNameValue.Value
                                      }).Distinct().ToList();
            }

            return delegatorTable.ToList();
        }

        private static List<TableDocumentViewModel> ConvertToTableDocument(
            ClientFieldData[] clientFieldData)
        {
            var documentIdList = clientFieldData.Where(o => o.Name.Equals("DocumentId")).ToList();
            var docNumberList = clientFieldData.Where(o => o.Name.Equals("DocNumber")).ToList();
            var descriptionList = clientFieldData
                .Where(o => o.Name.Equals("Description")).ToList();

            return (from documentId in documentIdList
                    join docNumber in docNumberList
                    on documentIdList.IndexOf(documentId)
                    equals docNumberList.IndexOf(docNumber)
                    join description in descriptionList
                    on documentIdList.IndexOf(documentId)
                    equals descriptionList.IndexOf(description)
                    select new TableDocumentViewModel
                    {
                        DocumentId = int.Parse(documentId.Value),
                        DocNumber = docNumber.Value,
                        Description = description.Value
                    }).ToList();
        }

        private static List<CommentDelegationViewModel> ConvertCommentsDelegationViewModel(
            ClientFieldData[] clientFieldData)
        {
            var commentCommentList = clientFieldData
               .Where(o => o.Name.Equals("commentComment")).ToList();
            var commentIdCommentList = clientFieldData
                .Where(o => o.Name.Equals("commentCommentId")).ToList();
            var userCommentList = clientFieldData
                .Where(o => o.Name.Equals("commentUser")).ToList();

            return (from commentIdComment in commentIdCommentList
                    join commentComment in commentCommentList
                    on commentIdCommentList.IndexOf(commentIdComment)
                    equals commentCommentList.IndexOf(commentComment)
                    join userComment in userCommentList
                    on commentIdCommentList.IndexOf(commentIdComment)
                    equals userCommentList.IndexOf(userComment)
                    select new CommentDelegationViewModel
                    {
                        CommentId = Convert.ToInt32(commentIdComment.Value),
                        Comment = commentComment.Value,
                        User = userComment.Value
                    }).ToList();
        }

        private static void ConvertDelegationByAmountToViewModel(this DelegationViewModel viewModel,
            ClientFieldData[] clientFieldData)
        {
            var chkMinAmounts = clientFieldData.Where(o => o.Name.Equals("chkSetMinAmount"))
                .ToList();
            var chkMaxAmounts = clientFieldData.Where(o => o.Name.Equals("chkSetMaxAmount"))
                .ToList();
            var txtMinAmounts = clientFieldData.Where(o => o.Name.Equals("txt_chkSetMinAmount"))
                .ToList();
            var txtMaxAmounts = clientFieldData.Where(o => o.Name.Equals("txt_chkSetMaxAmount"))
                .ToList();
            var isValidAmount = chkMinAmounts.Count == chkMaxAmounts.Count &&
                chkMaxAmounts.Count == txtMinAmounts.Count &&
                txtMinAmounts.Count == txtMaxAmounts.Count;
            var iDelegateCount = 0;

            foreach (var module in viewModel.AssignPermissionsTask.Modules)
            {
                var moduleView = clientFieldData.FirstOrDefault(o =>
                !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(module.Code));
                if (moduleView != null)
                {
                    module.IsChecked = Convert.ToBoolean(moduleView.Value);
                    foreach (var workflow in module.WorkFlows)
                    {
                        var workflowView = clientFieldData.FirstOrDefault(o =>
                        !string.IsNullOrWhiteSpace(o.Name) && (o.Name.Equals(workflow.Code)
                        || o.Name.Equals("chkAmount_" + workflow.Code)));
                        if (workflowView != null)
                        {
                            workflow.IsChecked = Convert.ToBoolean(workflowView.Value);
                            if (workflow.DelegateByAmount && !module.HaveRule)
                            {
                                if (isValidAmount && chkMinAmounts.Any()
                                    && chkMinAmounts.Count > iDelegateCount)
                                {
                                    if (Convert.ToBoolean(chkMinAmounts[iDelegateCount].Value))
                                    {
                                        var min = txtMinAmounts[iDelegateCount].Value;
                                        decimal minValue;
                                        workflow.MinAmount = decimal.TryParse(min, out minValue)
                                            ? minValue
                                            : (decimal?)null;
                                    }

                                    if (Convert.ToBoolean(chkMaxAmounts[iDelegateCount].Value))
                                    {
                                        var max = txtMaxAmounts[iDelegateCount].Value;
                                        decimal maxValue;
                                        workflow.MaxAmount = decimal.TryParse(max, out maxValue)
                                            ? maxValue
                                            : (decimal?)null;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}