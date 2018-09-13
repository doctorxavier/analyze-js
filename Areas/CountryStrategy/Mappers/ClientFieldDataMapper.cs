using System;
using System.Collections.Generic;
using System.Linq;

using IDB.MW.Application.CountryStrategyModule.ViewModels.MonitoringResultMatrix;
using IDB.MW.Application.CountryStrategyModule.ViewModels.ProductProfile;
using IDB.MW.Application.CountryStrategyModule.ViewModels.ResultMatrix;
using IDB.MW.Application.CountryStrategyModule.ViewModels.UseCountry;
using IDB.Presentation.MVC4.Models.ClientFieldData;
using IDB.MW.Application.CountryStrategyModule.Enums;

namespace IDB.Presentation.MVC4.Areas.CountryStrategy.Mappers
{
    public static class ClientFieldDataMapper
    {
        #region Mappers

        #region Result Matrix
        public static ResultMatrixViewModel UpdateResultMatrixViewModel(this ResultMatrixViewModel model, ClientFieldData[] formData)
        {
            var outcomeIdNew = 0;
            var indicatorIdNew = 0;

            model.Components.Clear();

            var fieldsGroupedForComponent = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id)).GroupBy(x => x.Id);
            foreach (var fieldsForComponent in fieldsGroupedForComponent)
            {
                var componentId = fieldsForComponent.Key;
                if (componentId.Contains("new-"))
                {
                    componentId = "0";
                }

                var newComponent = new ComponentViewModel()
                {
                    OperationId = model.OperationId,
                    ComponentId = componentId.ConvertToInt(),
                };
                model.Components.Add(newComponent);

                var field = fieldsForComponent.First(x => x.Name == "Component-OrderNumber");
                if (field != null)
                {
                    newComponent.OrderNumber = field.ConvertToInt();
                }

                field = fieldsForComponent.First(x => x.Name == "PriorityArea");
                newComponent.PriorityArea = field.ConvertToString();

                var fieldsGroupedForObjective = fieldsForComponent
                    .Where(x => x.ExtraData.ContainsKey("data-persist-objectiveid"))
                    .GroupBy(x => x.ExtraData.Single(y => y.Key == "data-persist-objectiveid"));
                foreach (var fieldsForObjective in fieldsGroupedForObjective)
                {
                    var objectiveId = fieldsForObjective.Key.Value;
                    if (objectiveId.Contains("new-"))
                    {
                        objectiveId = "0";
                    }

                    var newObjective = new ObjectiveViewModel()
                    {
                        ComponentId = newComponent.ComponentId,
                        ObjectiveId = objectiveId.ConvertToInt()
                    };
                    newComponent.Objectives.Add(newObjective);

                    field = fieldsForObjective.First(x => x.Name == "Objective-OrderNumber");
                    newObjective.OrderNumber = field.ConvertToInt();

                    field = fieldsForObjective.First(x => x.Name == "GovernmentPriority");
                    newObjective.GovernmentPriority = field.ConvertToString();

                    field = fieldsForObjective.First(x => x.Name == "countryStrategicObjective");
                    newObjective.CountryStrategicObjective = field.ConvertToString();

                    field = fieldsForObjective.First(x => x.Name == "countryStrategicObjectiveES");
                    newObjective.CountryStrategicObjectiveES = field.ConvertToString();

                    field = fieldsForObjective.FirstOrDefault(x => x.Name == "countryStrategicObjectivePT");
                    if (field != null)
                    {
                        newObjective.CountryStrategicObjectivePT = field.ConvertToString();
                    }

                    field = fieldsForObjective.FirstOrDefault(x => x.Name == "countryStrategicObjectiveFR");
                    if (field != null)
                    {
                        newObjective.CountryStrategicObjectiveFR = field.ConvertToString();
                    }

                    field = fieldsForObjective.FirstOrDefault(x => x.Name == "AssociatedExpiredCSObjective");
                    newObjective.AssociatedExpiredCSObjective = field.ConvertToNullableInt();

                    var fieldsGroupedForOutcome = fieldsForObjective
                        .Where(x => x.ExtraData.ContainsKey("data-persist-outcomeid"))
                        .GroupBy(x => x.ExtraData.Single(y => y.Key == "data-persist-outcomeid"));
                    foreach (var fieldsForOutcome in fieldsGroupedForOutcome)
                    {
                        var outcomeId = fieldsForOutcome.Key.Value;
                        if (outcomeId.Contains("new-"))
                        {
                            outcomeIdNew--;
                            outcomeId = outcomeIdNew.ToString();
                        }

                        field = fieldsForOutcome.First(x => x.Name == "ExpectedOutcome");
                        var ecpectedOutcome = field.ConvertToString();

                        var fieldsGroupedForIndicator = fieldsForOutcome
                       .Where(x => x.ExtraData.ContainsKey("data-persist-indicatorid"))
                       .GroupBy(x => x.ExtraData.Single(y => y.Key == "data-persist-indicatorid"));
                        foreach (var fieldsForIndicator in fieldsGroupedForIndicator)
                        {
                            var indicatorId = fieldsForIndicator.Key.Value;
                            if (indicatorId.Contains("new-"))
                            {
                                indicatorIdNew--;
                                indicatorId = indicatorIdNew.ToString();
                            }

                            var newOutcome = new ExpectedOutcomeIndicatorViewModel()
                            {
                                ComponentId = newComponent.ComponentId,
                                ObjectiveId = newObjective.ObjectiveId,
                                ExpectedOutcomeId = outcomeId.ConvertToInt(),
                                IndicatorId = indicatorId.ConvertToInt()
                            };

                            newObjective.ExpectedOutcomeIndicators.Add(newOutcome);

                            newOutcome.ExpectedOutcome = ecpectedOutcome;

                            field = fieldsForIndicator.First(x => x.Name == "Indicator");
                            newOutcome.Indicator = field.ConvertToString();

                            field = fieldsForIndicator.First(x => x.Name == "UnitOfMeasure");
                            newOutcome.UnitOfMeasure = field.ConvertToString();

                            field = fieldsForIndicator.First(x => x.Name == "Baseline");
                            newOutcome.Baseline = field.ConvertToString();

                            field = fieldsForIndicator.First(x => x.Name == "BaselineYear");
                            newOutcome.BaselineYear = field.ConvertToString();

                            field = fieldsForIndicator.First(x => x.Name == "Source");
                            newOutcome.Source = field.ConvertToString();

                            var linkedIndicators = fieldsForIndicator.Where(x => x.Name == "LinkedIndicator");

                            foreach (var linedField in linkedIndicators)
                            {
                                newOutcome.LinkedIndicators.Add(linedField.ConvertToInt());
                            }
                        }
                    }
                }
            }

            return model;
        }
        #endregion

        #region Use Country System
        public static UseCountrySystemViewModel UpdateUseCountrySystemViewModel(this UseCountrySystemViewModel model, ClientFieldData[] formData)
        {
            //Use of Country Systems Results Matrix
            var outcomeIdNew = 0;
            var indicatorIdNew = 0;

            model.UCSStrategicObjective.Clear();
            var inputsCSResultsMatrix = new string[]
            {
                "operationId",
                "Component-OrderNumber",
                "strtegicObjective",
                "ExpectedOutcome",
                "Indicator",
                "UnitOfMeasure",
                "Baseline",
                "BaselineYear",
                "Source",
                "timming",
                "LinkedIndicator"
            };

            var fieldsCSResultsMatrix = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputsCSResultsMatrix.Any(y => x.Name == y));
            var fieldsCSResultsMatrixGrouped = fieldsCSResultsMatrix.GroupBy(x => x.Id);

            foreach (var cSResultsMatrixItem in fieldsCSResultsMatrixGrouped)
            {
                var strObjectiveId = cSResultsMatrixItem.Key;
                if (strObjectiveId.Contains("new-"))
                {
                    strObjectiveId = "0";
                }

                var newStrObjective = new UCSStrategicObjectiveViewModel()
                {
                    UCSStrategicObjectiveId = strObjectiveId.ConvertToInt(),
                };

                model.UCSStrategicObjective.Add(newStrObjective);

                var field = cSResultsMatrixItem.First(x => x.Name == inputsCSResultsMatrix[0]);
                if (field != null)
                {
                    newStrObjective.OperationId = field.ConvertToInt();
                }

                field = cSResultsMatrixItem.First(x => x.Name == inputsCSResultsMatrix[1]);
                if (field != null)
                {
                    newStrObjective.OrderNumber = field.ConvertToInt();
                }

                field = cSResultsMatrixItem.First(x => x.Name == inputsCSResultsMatrix[2]);
                if (field != null)
                {
                    newStrObjective.StrategicObjectiveTypeId = field.ConvertToInt();
                }

                var fieldsGroupedForOutcome = cSResultsMatrixItem.Where(x => x.ExtraData.ContainsKey("data-persist-outcomeid"))
                    .GroupBy(x => x.ExtraData.Single(y => y.Key == "data-persist-outcomeid"));

                foreach (var outcomeItem in fieldsGroupedForOutcome)
                {
                    var outcomeId = outcomeItem.Key.Value;
                    if (outcomeId.Contains("new-"))
                    {
                        outcomeIdNew--;
                        outcomeId = outcomeIdNew.ToString();
                    }

                    field = outcomeItem.First(x => x.Name == inputsCSResultsMatrix[3]);
                    var ecpectedOutcome = field.ConvertToString();

                    var fieldsGroupedForIndicator = outcomeItem.Where(x => x.ExtraData.ContainsKey("data-persist-indicatorid"))
                    .GroupBy(x => x.ExtraData.Single(y => y.Key == "data-persist-indicatorid"));

                    foreach (var indicatorItem in fieldsGroupedForIndicator)
                    {
                        var indicatorId = indicatorItem.Key.Value;
                        if (indicatorId.Contains("new-"))
                        {
                            indicatorIdNew--;
                            indicatorId = indicatorIdNew.ToString();
                        }

                        var newOutcome = new UCSExpectedOutcomeIndicatorViewModel()
                        {
                            UCSStrategicObjectiveId = newStrObjective.UCSStrategicObjectiveId,
                            OperationId = newStrObjective.OperationId,
                            ExpectedOutcomeId = outcomeId.ConvertToInt(),
                            IndicatorId = indicatorId.ConvertToInt(),
                            ExpectedOutcome = ecpectedOutcome
                        };

                        newStrObjective.ExpectedOutcomeIndicators.Add(newOutcome);

                        field = indicatorItem.First(x => x.Name == inputsCSResultsMatrix[4]);
                        if (field != null)
                        {
                            newOutcome.Indicator = field.ConvertToString();
                        }

                        field = indicatorItem.First(x => x.Name == inputsCSResultsMatrix[5]);
                        if (field != null)
                        {
                            newOutcome.UnitOfMeasure = field.ConvertToString();
                        }

                        field = indicatorItem.First(x => x.Name == inputsCSResultsMatrix[6]);
                        if (field != null)
                        {
                            newOutcome.Baseline = field.ConvertToString();
                        }

                        field = indicatorItem.First(x => x.Name == inputsCSResultsMatrix[7]);
                        if (field != null)
                        {
                            newOutcome.BaselineYear = field.ConvertToString();
                        }

                        field = indicatorItem.First(x => x.Name == inputsCSResultsMatrix[8]);
                        if (field != null)
                        {
                            newOutcome.Source = field.ConvertToString();
                        }

                        field = indicatorItem.First(x => x.Name == inputsCSResultsMatrix[9]);
                        if (field != null)
                        {
                            newOutcome.TimmingTypeId = field.ConvertToInt();
                        }

                        var linkedIndicators = indicatorItem.Where(x => x.Name == "LinkedIndicator");

                        foreach (var linedField in linkedIndicators)
                        {
                            newOutcome.LinkedIndicators.Add(linedField.ConvertToInt());
                        }
                    }
                }
            }

            //Financial Management
            var inputsFinancialManagement = new string[]
            {
                "FinancialManagement-Id",
                "FinancialManagement-BaselineYear",
                "FinancialManagement-Baseline",
                "FinancialManagement-EstimateUseYear",
                "FinancialManagement-EstimatedUse",
                "FinancialManagement-ForeseenActions",
                "FinancialManagement-Comment"
            };

            var fieldsFinancialManagement = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputsFinancialManagement.Any(y => x.Name == y));
            var fieldsFinancialManagementGrouped = fieldsFinancialManagement.GroupBy(x => x.Id);

            foreach (var financialManagementItem in fieldsFinancialManagementGrouped)
            {
                var itemId = financialManagementItem.Key.Contains("new") ? 0 : financialManagementItem.Key.ConvertToInt(false);

                var financialManagementModel = model.FinancialManagement.FirstOrDefault(x => x.SystemRecordId == itemId);

                var financialManagementField = financialManagementItem.FirstOrDefault(x => x.Name == inputsFinancialManagement[1]);
                if (financialManagementField != null)
                {
                    financialManagementModel.BaseLineYear = financialManagementField.ConvertToInt(true);
                }

                financialManagementField = financialManagementItem.FirstOrDefault(x => x.Name == inputsFinancialManagement[2]);
                if (financialManagementField != null)
                {
                    financialManagementModel.BaseLine = financialManagementField.Value;
                }

                financialManagementField = financialManagementItem.FirstOrDefault(x => x.Name == inputsFinancialManagement[3]);
                if (financialManagementField != null)
                {
                    financialManagementModel.EstimateUseYear = financialManagementField.ConvertToInt(true);
                }

                financialManagementField = financialManagementItem.FirstOrDefault(x => x.Name == inputsFinancialManagement[4]);
                if (financialManagementField != null)
                {
                    financialManagementModel.EstimatedUse = financialManagementField.Value;
                }

                financialManagementField = financialManagementItem.FirstOrDefault(x => x.Name == inputsFinancialManagement[5]);
                if (financialManagementField != null)
                {
                    financialManagementModel.ForeseenActionIds = financialManagementField.ConvertToListInt(false);
                }

                financialManagementField = financialManagementItem.FirstOrDefault(x => x.Name == inputsFinancialManagement[6]);
                if (financialManagementField != null)
                {
                    financialManagementModel.Comment = financialManagementField.ConvertToString();
                }
            }

            //Procurement
            var inputsProcurement = new string[]
            {
                "Procurement-Id",
                "Procurement-BaselineYear",
                "Procurement-Baseline",
                "Procurement-EstimateUseYear",
                "Procurement-EstimatedUse",
                "Procurement-ForeseenActions",
                "Procurement-Comment"
            };

            var fieldsProcurement = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputsProcurement.Any(y => x.Name == y));
            var fieldsProcurementGrouped = fieldsProcurement.GroupBy(x => x.Id);

            foreach (var procurementItem in fieldsProcurementGrouped)
            {
                var itemId = procurementItem.Key.Contains("new") ? 0 : procurementItem.Key.ConvertToInt(false);

                var procurementModel = model.Procurement.FirstOrDefault(x => x.SystemRecordId == itemId);

                var procurementField = procurementItem.FirstOrDefault(x => x.Name == inputsProcurement[1]);
                if (procurementField != null)
                {
                    procurementModel.BaseLineYear = procurementField.ConvertToInt(true);
                }

                procurementField = procurementItem.FirstOrDefault(x => x.Name == inputsProcurement[2]);
                if (procurementField != null)
                {
                    procurementModel.BaseLine = procurementField.Value;
                }

                procurementField = procurementItem.FirstOrDefault(x => x.Name == inputsProcurement[3]);
                if (procurementField != null)
                {
                    procurementModel.EstimateUseYear = procurementField.ConvertToInt(true);
                }

                procurementField = procurementItem.FirstOrDefault(x => x.Name == inputsProcurement[4]);
                if (procurementField != null)
                {
                    procurementModel.EstimatedUse = procurementField.Value;
                }

                procurementField = procurementItem.FirstOrDefault(x => x.Name == inputsProcurement[5]);
                if (procurementField != null)
                {
                    procurementModel.ForeseenActionIds = procurementField.ConvertToListInt(false);
                }

                procurementField = procurementItem.FirstOrDefault(x => x.Name == inputsProcurement[6]);
                if (procurementField != null)
                {
                    procurementModel.Comment = procurementField.ConvertToString();
                }
            }

            return model;
        }
        #endregion

        #region Monitoring Result Matrix

        public static CSObjectiveDetailViewModel UpdateUseCountrySystemViewModel(this CSObjectiveDetailViewModel model, ClientFieldData[] formData)
        {
            var field = formData.First(x => x.Name == "page-objectiveId");
            if (field != null)
            {
                model.ObjectiveId = field.ConvertToInt(true);
            }

            field = formData.First(x => x.Name == "page-isAligned");
            if (field != null)
            {
                model.IsAligned = field.ConvertToBool() ?? false;
            }

            model.InvestLoans.Clear();
            model.Guarantees.Clear();
            model.InvestGrants.Clear();
            model.TCOperations.Clear();
            model.MIFLoans.Clear();
            model.MIFEquities.Clear();
            model.MIFGuarantees.Clear();
            model.MIFIvestGrants.Clear();
            model.MIFTCOperations.Clear();

            var IunputsInvestLoans = new string[]
            {
                CSMonitoringTypeTable.IBDLoans.ToString() + "-operationId",
                CSMonitoringTypeTable.IBDLoans.ToString() + "-isManual",
                CSMonitoringTypeTable.IBDLoans.ToString() + "-isImport"
            };

            var IunputsGuarantees = new string[]
            {
                CSMonitoringTypeTable.IDBGuarantees.ToString() + "-operationId",
                CSMonitoringTypeTable.IDBGuarantees.ToString() + "-isManual",
                CSMonitoringTypeTable.IDBGuarantees.ToString() + "-isImport"
            };

            var IunputsInvestGrants = new string[]
            { 
                CSMonitoringTypeTable.IDBInvestGrants.ToString() + "-operationId",
                CSMonitoringTypeTable.IDBInvestGrants.ToString() + "-isManual",
                CSMonitoringTypeTable.IDBInvestGrants.ToString() + "-isImport"
            };

            var IunputsTCOperations = new string[]
            {
                CSMonitoringTypeTable.IDBTechnicalCoop.ToString() + "-operationId",
                CSMonitoringTypeTable.IDBTechnicalCoop.ToString() + "-isManual",
                CSMonitoringTypeTable.IDBTechnicalCoop.ToString() + "-isImport"
            };

            var IunputsMIFLoans = new string[]
            {
                CSMonitoringTypeTable.MIFLoans.ToString() + "-operationId",
                CSMonitoringTypeTable.MIFLoans.ToString() + "-isManual",
                CSMonitoringTypeTable.MIFLoans.ToString() + "-isImport"
            };

            var IunputsMIFEquities = new string[]
            {
                CSMonitoringTypeTable.MIFEquities.ToString() + "-operationId",
                CSMonitoringTypeTable.MIFEquities.ToString() + "-isManual",
                CSMonitoringTypeTable.MIFEquities.ToString() + "-isImport"
            };

            var IunputsMIFGuarantees = new string[]
            {
                CSMonitoringTypeTable.MIFGuarantees.ToString() + "-operationId",
                CSMonitoringTypeTable.MIFGuarantees.ToString() + "-isManual",
                CSMonitoringTypeTable.MIFGuarantees.ToString() + "-isImport"
            };

            var IunputsMIFIvestGrants = new string[]
            {
                CSMonitoringTypeTable.MIFInvestGrants.ToString() + "-operationId",
                CSMonitoringTypeTable.MIFInvestGrants.ToString() + "-isManual",
                CSMonitoringTypeTable.MIFInvestGrants.ToString() + "-isImport"
            };

            var IunputsMIFTCOperations = new string[]
            {
                CSMonitoringTypeTable.MIFTechnicalCoop.ToString() + "-operationId",
                CSMonitoringTypeTable.MIFTechnicalCoop.ToString() + "-isManual",
                CSMonitoringTypeTable.MIFTechnicalCoop.ToString() + "-isImport"
            };

            model.InvestLoans.InitializeOperations(IunputsInvestLoans, formData);
            model.Guarantees.InitializeOperations(IunputsGuarantees, formData);
            model.InvestGrants.InitializeOperations(IunputsInvestGrants, formData);
            model.TCOperations.InitializeOperations(IunputsTCOperations, formData);
            model.MIFLoans.InitializeOperations(IunputsMIFLoans, formData);
            model.MIFEquities.InitializeOperations(IunputsMIFEquities, formData);
            model.MIFGuarantees.InitializeOperations(IunputsMIFGuarantees, formData);
            model.MIFIvestGrants.InitializeOperations(IunputsMIFIvestGrants, formData);
            model.MIFTCOperations.InitializeOperations(IunputsMIFTCOperations, formData);
            return model;
        }
        #endregion

        #region Product Data
        public static BasicDataViewModel UpdateBasicDataViewModel(this BasicDataViewModel model, ClientFieldData[] formData)
        {
            var field = formData.FirstOrDefault(x => x.Name == "gnDocumentNumber");
            model.GNDocumentNumber = field.ConvertToString();

            field = formData.FirstOrDefault(x => x.Name == "ProductYear");
            model.ProductYear = field.ConvertToNullableInt();

            field = formData.FirstOrDefault(x => x.Name == "justification");
            model.Justification = field.ConvertToString();

            field = formData.FirstOrDefault(x => x.Name == "OperationNameEn");
            model.OperationNameEn = field.ConvertToString();

            field = formData.FirstOrDefault(x => x.Name == "OperationNameEs");
            model.OperationNameEs = field.ConvertToString();

            field = formData.FirstOrDefault(x => x.Name == "OperationNamePt");
            model.OperationNamePt = field.ConvertToString();

            field = formData.FirstOrDefault(x => x.Name == "OperationNameFr");
            model.OperationNameFr = field.ConvertToString();

            return model;
        }

        public static ResponsibilityDataViewModel UpdateProductDataResponsibilityData(this ResponsibilityDataViewModel model, ClientFieldData[] formData)
        {
            /* Responsibility Unit(s) */

            var inputsUnits = new string[]
            {
                "responsibleUnitsRole",
                "responsibleUnitsOrganizationalUnit",
                "responsibleUnitsEffortInDays",
            };

            var unitsGrouped = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputsUnits.Any(y => x.Name == y)).GroupBy(x => x.Id);
            model.ResponsibilityUnits.Clear();

            foreach (var unitData in unitsGrouped)
            {
                var unit = new ResponsibleRowViewModel();
                model.ResponsibilityUnits.Add(unit);

                if (!unitData.Key.Contains("new-"))
                {
                    unit.RowId = int.Parse(unitData.Key);
                }

                var fieldCode = unitData.FirstOrDefault(x => x.Name == "responsibleUnitsRole");
                unit.Role = fieldCode.ConvertToInt();

                var field = unitData.FirstOrDefault(x => x.Name == "responsibleUnitsOrganizationalUnit");
                unit.OrganizationalUnitId = field.ConvertToInt();

                field = unitData.FirstOrDefault(x => x.Name == "responsibleUnitsEffortInDays");
                if (field != null)
                {
                    unit.EffortInDays = field.ConvertToInt();
                }
            }

            /*  TEAM   */
            var inputsTeams = new string[]
            {
                "userNameTeam",
                "OrganizationalUnitCode",
                "userNameTeam_text",
                "operationTeamsRole",
            };

            var teamsGrouped = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputsTeams.Any(y => x.Name == y)).GroupBy(x => x.Id);
            model.Team.Clear();
            foreach (var teamData in teamsGrouped)
            {
                var member = new OperationTeamRowViewModel();
                model.Team.Add(member);

                if (!teamData.Key.Contains("new-"))
                {
                    member.RowId = int.Parse(teamData.Key);
                }
                else
                {
                    var fieldCode = teamData.FirstOrDefault(x => x.Name == "OrganizationalUnitCode");
                    member.OrganizationalUnitCode = fieldCode.ConvertToString();
                }

                var field = teamData.FirstOrDefault(x => x.Name == "userNameTeam_text");
                member.Name = field.ConvertToString();

                field = teamData.FirstOrDefault(x => x.Name == "userNameTeam");
                member.UserName = field.ConvertToString();

                field = teamData.FirstOrDefault(x => x.Name == "operationTeamsRole");
                member.Role = field.ConvertToInt();
            }

            return model;
        }
        #endregion

        #endregion

        #region Private Methods

        private static void InitializeOperations(this ICollection<OperationViewModel> operations, string[] inputsGroup, ClientFieldData[] formData)
        {
            var fieldsOperation = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && inputsGroup.Any(y => x.Name == y));
            var fieldsOperationGrouped = fieldsOperation.GroupBy(x => x.Id);

            foreach (var operationInputs in fieldsOperationGrouped)
            {
                var operationModel = new OperationViewModel();

                var operationField = operationInputs.FirstOrDefault(x => x.Name == inputsGroup[0]);
                if (operationField != null)
                {
                    var operationId = operationField.ConvertToInt(false);
                    var modAux = operations.SingleOrDefault(x => x.OperationId == operationId);

                    if (modAux == null)
                    {
                        operationModel.OperationId = operationId;

                        operationField = operationInputs.FirstOrDefault(x => x.Name == inputsGroup[1]);
                        if (operationField != null)
                        {
                            operationModel.IsManual = operationField.ConvertToBool() ?? false;
                        }

                        operationField = operationInputs.FirstOrDefault(x => x.Name == inputsGroup[2]);
                        if (operationField != null)
                        {
                            operationModel.IsImportByObjectiveAsociated = operationField.ConvertToBool() ?? false;
                        }

                        operations.Add(operationModel);
                    }
                    else
                    {
                        operationModel = modAux;
                    }
                }
            }
        }

        private static List<int> ConvertToListInt(this ClientFieldData field, bool returnCeroInFail = false)
        {
            if (field == null || field.Value == null || field.Value.ToString() == string.Empty)
            {
                return new List<int>();
            }

            return field.Value.Split(',').Select(x => x.ToString().ConvertToInt(returnCeroInFail)).ToList();
        }

        private static int ConvertToInt(this ClientFieldData field, bool returnCeroInFail = false)
        {
            if (field == null)
            {
                if (returnCeroInFail)
                {
                    return 0;
                }
                else
                {
                    throw new ArgumentException();
                }
            }

            return field.Value.ConvertToInt(returnCeroInFail);
        }

        private static int ConvertToInt(this string field, bool returnCeroInFail = false)
        {
            int intValue;
            if (!int.TryParse(field, System.Globalization.NumberStyles.Any, null, out intValue))
            {
                if (returnCeroInFail)
                {
                    intValue = 0;
                }
                else
                {
                    throw new ArgumentException();
                }
            }

            return intValue;
        }

        private static int? ConvertToNullableInt(this ClientFieldData field, bool returnNullInFail = false)
        {
            if (field == null)
            {
                return null;
            }

            return field.Value.ConvertToNullableInt(returnNullInFail);
        }

        private static int? ConvertToNullableInt(this string field, bool returnNullInFail = false)
        {
            if (string.IsNullOrWhiteSpace(field))
            {
                return null;
            }

            int? intValue = null;
            int intValueTmp;
            if (!int.TryParse(field, out intValueTmp))
            {
                if (returnNullInFail)
                {
                    return null;
                }
                else
                {
                    throw new ArgumentException();
                }
            }

            intValue = intValueTmp;
            return intValue;
        }

        private static string ConvertToString(this ClientFieldData field)
        {
            if (field == null)
            {
                return null;
            }

            return field.Value;
        }

        private static bool? ConvertToBool(this string field)
        {
            if (field == null || field == string.Empty)
            {
                return null;
            }

            return field.Equals("True");
        }

        private static bool? ConvertToBool(this ClientFieldData field)
        {
            if (field == null || field.Value == null || field.Value == string.Empty)
            {
                return null;
            }

            return field.Value.Equals("True");
        }

        #endregion
    }
}