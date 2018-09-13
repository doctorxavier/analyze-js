using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using IDB.Architecture.Language;
using IDB.MW.Application.IndicatorsModuleNew.Enums;
using IDB.MW.Application.IndicatorsModuleNew.ViewModels.Core;
using IDB.MW.Application.IndicatorsModuleNew.ViewModels.Core.EditIndicators;
using IDB.MW.Application.IndicatorsModuleNew.ViewModels.Core.EditTemplates;
using IDB.MW.Application.IndicatorsModuleNew.ViewModels.Framework;
using IDB.MW.Application.IndicatorsModuleNew.ViewModels.Fund;
using IDB.MW.Application.IndicatorsModuleNew.ViewModels.StandardizedOutput;
using IDB.MW.Infrastructure.Configuration;
using IDB.Presentation.MVC4.Models.ClientFieldData;

namespace IDB.Presentation.MVC4.Areas.Indicators.Mappers
{
    public static class ClientFieldDataMapper
    {
        #region Constants
        public static readonly string DATETIME_PARSE_FORMAT;
        #endregion

        #region Constructors
        static ClientFieldDataMapper()
        {
            DATETIME_PARSE_FORMAT = ConfigurationServiceFactory.Current.GetApplicationSettings().FormatDateUpload;
        }
        #endregion

        #region Mappers
        #region List
        public static void UpdateTemplateListPage(this TemplatePageViewModel model, ClientFieldData[] formData)
        {
            var fields = formData.Where(x => x.Name == "IsDefault");
            foreach (var field in fields)
            {
                var fieldModel = model.Templates.First(x => x.Id.ToString() == field.Id);
                fieldModel.IsDefault = GetBoolValue(field.Value);
            }

            var finalIds = fields.Select(x => int.Parse(x.Id));
            var idsToRemove = GetIdsToRemove(model.Templates, finalIds, x => x.Id, x => x);

            foreach (var idToRemove in idsToRemove)
            {
                var itemToRemove = model.Templates.First(x => x.Id == idToRemove);
                model.Templates.Remove(itemToRemove);
            }
        }

        public static void UpdateIndicatorsPageViewModel(this IndicatorsPageViewModel model, ClientFieldData[] formData)
        {
            foreach (var field in formData)
            {
                var fieldModel = model.Rows.First(x => x.IndicatorId.ToString() == field.Id);
                fieldModel.IsActive = GetBoolValue(field.Value);
            }
        }
        #endregion

        #region Framework
        public static void UpdateTemplate(this EditTemplatePageViewModel<EditTemplateFrameworkViewModel> model, ClientFieldData[] formData)
        {
            model.UpdateTemplateCommon(formData);

            model.Template.Specific.Types = formData.GetSimpleLevelData("Type");
            model.Template.Specific.PriorityAreas = formData.GetMultiLevelData("PriorityArea");
            model.Template.Specific.Disaggregations = formData.GetMultiLevelData("Disaggregation");
        }

        public static void UpdateEditIndicatorPageViewModelFramework(this EditIndicatorPageViewModel<FWBasicDataViewModel> model, ClientFieldData[] formData)
        {
            model.UpdateEditIndicatorPageViewModelCommon(formData);

            var field = formData.FirstOrDefault(x => x.Name == "priorityAreaId");
            model.Indicator.Specific.PriorityAreaIds.Clear();
            if (field != null)
            {
                var list = field.Value.Split(',').Select(x => int.Parse(x)).ToList();
                model.Indicator.Specific.PriorityAreaIds = list;
            }

            field = formData.FirstOrDefault(x => x.Name == "disaggregationId");
            model.Indicator.Specific.DisaggregationIds.Clear();
            if (field != null)
            {
                var list = field.Value.Split(',').Select(x => int.Parse(x)).ToList();
                model.Indicator.Specific.DisaggregationIds = list;
            }

            model.Indicator.Specific.Subindicators = formData.GetSubindicatorData("CRFSubindicator");

            field = formData.First(x => x.Name == "isActive");
            if (field != null)
            {
                model.Indicator.Specific.IsActive = GetBoolValue(field.Value);
            }

            field = formData.First(x => x.Name == "isContribution");
            if (field != null)
            {
                model.Indicator.Specific.IsContribution = GetBoolValue(field.Value);
            }

            field = formData.First(x => x.Name == "isStrategicAlignment");
            if (field != null)
            {
                model.Indicator.Specific.IsStrategicAlignment = GetBoolValue(field.Value);
            }

            field = formData.First(x => x.Name == "isAlignment");
            if (field != null)
            {
                model.Indicator.Specific.isAlignment = GetBoolValue(field.Value);
            }

            field = formData.First(x => x.Name == "isRegionalContext");

            if (field != null)
            {
                model.Indicator.Specific.IsRegionalContext = GetBoolValue(field.Value);
            }

            field = formData.First(x => x.Name == "isLinkToCS");
            if (field != null)
            {
                model.Indicator.Specific.IsLinkToCS = GetBoolValue(field.Value);
            }

            field = formData.First(x => x.Name == "isRMIndicatorRequirement");
            if (field != null)
            {
                model.Indicator.Specific.IsRMIndicatorRequirement = GetBoolValue(field.Value);
            }

            field = formData.FirstOrDefault(x => x.Name == "typeCRFId");
            model.Indicator.Specific.TypeCRFId = 0;
            if (field != null)
            {
                model.Indicator.Specific.TypeCRFId = int.Parse(field.Value);
            }
        }
        #endregion

        #region Fund
        public static void UpdateTemplate(this EditTemplatePageViewModel model, ClientFieldData[] formData)
        {
            model.UpdateTemplateCommon(formData);
        }

        public static void UpdateEditIndicatorPageViewModelFund(this EditIndicatorPageViewModel<FundBasicDataViewModel> model, ClientFieldData[] formData)
        {
            model.UpdateEditIndicatorPageViewModelCommon(formData);

            model.Indicator.Specific.Disaggretations = formData.GetSubindicatorData("FundDisaggregation");

            var field = formData.First(x => x.Name == "isActive");
            if (field != null)
            {
                model.Indicator.Specific.IsActive = GetBoolValue(field.Value);
            }

            field = formData.First(x => x.Name == "fund");
            if (field != null)
            {
                model.Indicator.Specific.FundId = int.Parse(field.Value);
            }

            field = formData.FirstOrDefault(x => x.Name == "theme");
            model.Indicator.Specific.ThemeIds.Clear();
            if (field != null)
            {
                var list = field.Value.Split(',').Select(x => int.Parse(x)).ToList();
                model.Indicator.Specific.ThemeIds = list;
            }
        }

        #endregion

        #region Standardized
        public static void UpdateTemplate(this EditTemplatePageViewModel<EditTemplateStandardizedViewModel> model, ClientFieldData[] formData)
        {
            model.UpdateTemplateCommon(formData);

            model.Template.Specific.BusinessLines = formData.GetSimpleLevelData("BusinessLine");
            model.Template.Specific.OutputGroups = formData.GetSimpleLevelData("OutputGroups");
        }

        public static void UpdateEditIndicatorPageViewModelStandardized(this EditIndicatorPageViewModel<StandardizedBasicDataViewModel> model, ClientFieldData[] formData)
        {
            model.UpdateEditIndicatorPageViewModelCommon(formData);

            var field = formData.First(x => x.Name == "indicatorNumberPrefix");
            if (field != null)
            {
                model.Indicator.Common.IndicatorNumber = string.Format("{0}.{1}", field.Value, model.Indicator.Common.IndicatorNumber);
            }

            field = formData.First(x => x.Name == "isActive");
            if (field != null)
            {
                model.Indicator.Specific.IsActive = GetBoolValue(field.Value);
            }

            field = formData.First(x => x.Name == "isContribution");
            if (field != null)
            {
                model.Indicator.Specific.IsContribution = GetBoolValue(field.Value);
            }

            field = formData.FirstOrDefault(x => x.Name == "businessLineId");
            model.Indicator.Specific.BusinessLineId = 0;
            if (field != null)
            {
                model.Indicator.Specific.BusinessLineId = int.Parse(field.Value);
            }

            field = formData.FirstOrDefault(x => x.Name == "outputGroupId");
            model.Indicator.Specific.OutputGroupId = 0;
            if (field != null)
            {
                model.Indicator.Specific.OutputGroupId = int.Parse(field.Value);
            }

            field = formData.FirstOrDefault(x => x.Name == "theme");
            model.Indicator.Specific.ThemeIds.Clear();
            if (field != null)
            {
                var list = field.Value.Split(',').Select(x => int.Parse(x)).ToList();
                model.Indicator.Specific.ThemeIds = list;
            }

            model.Indicator.Specific.FundIdsByTheme =
                model.Indicator.Specific.FundIdsByTheme.ConvertFundIdsByTheme(
                    model.Indicator.Specific.ThemeIds, 
                    formData);

            model.Indicator.Specific.Disaggretations = formData.GetSubindicatorData("StandardizedDisaggregation");
        }

        private static Dictionary<int, List<int>> ConvertFundIdsByTheme(this Dictionary<int, List<int>> list, List<int> ThemeIds, ClientFieldData[] formData)
        {
            if (list == null)
            {
                list = new Dictionary<int, List<int>>();
            }

            list.Clear();

            foreach (var themeId in ThemeIds)
            {
                var fieldName = string.Format("fundIndicatorForTheme-{0}-", themeId);

                var multiIndFields = formData.Where(x => x.Name.Contains(fieldName));

                foreach (var field in multiIndFields)
                {
                    var selectedId = new List<int>();
                    if (list.ContainsKey(themeId))
                    {
                        selectedId = list[themeId];
                    }
                    else
                    {
                        list.Add(themeId, selectedId);
                    }

                    var values = field.Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList();
                    selectedId.AddRange(values);
                }
            }

            return list;
        }
        #endregion

        #endregion

        #region private methods
        private static void UpdateTemplateCommon(this EditTemplatePageViewModel model, ClientFieldData[] formData)
        {
            ClientFieldData field = null;

            field = formData.FirstOrDefault(x => x.Name == "version");
            if (field != null)
            {
                model.Template.Common.Version = field.Value;
            }

            field = formData.FirstOrDefault(x => x.Name == "description");
            if (field != null)
            {
                model.Template.Common.Description = field.Value;
            }

            // Technical Field Data
            // Remove default technical field
            model.Template.Common.TechnicalFields.RemoveAll(x => x.TechnicalFieldTemplateId == 0);
            var techincals = formData.Where(x => x.Name == "technicalFieldName" ||
                                            x.Name.Contains("typeTF-new-") ||
                                            x.Name.Contains("typeTFValue-") ||
                                            x.Name.Contains("IsActive"));

            var oldTechnical = techincals.Where(x => x.ExtraData.ContainsKey("data-persist-old-id"));
            var newTechnical = techincals.Where(x => x.ExtraData.ContainsKey("data-persist-new-id"));

            var oldGrouped = oldTechnical.GroupBy(x => x.ExtraData.FirstOrDefault(y => y.Key == "data-persist-old-id"));
            var newGrouped = newTechnical.GroupBy(x => x.ExtraData.FirstOrDefault(y => y.Key == "data-persist-new-id"));

            List<TechnicalFieldViewModel> newTechnicalFields = new List<TechnicalFieldViewModel>();

            foreach (var oldItem in oldGrouped)
            {
                var technicalModel = model.Template.Common.TechnicalFields.First(x => x.TechnicalFieldTemplateId.ToString() == oldItem.Key.Value);

                field = oldItem.FirstOrDefault(x => x.Name == "technicalFieldName");
                if (field != null)
                {
                    technicalModel.Name = field.Value;
                }

                field = oldItem.FirstOrDefault(x => x.Name.Contains("IsActive"));
                if (field != null)
                {
                    technicalModel.IsActive = !GetBoolValue(field.Value);
                }

                field = oldItem.FirstOrDefault(x => x.Name.Contains("typeTFValue-"));
                if (field != null)
                {
                    technicalModel.Type = field.Value == "text" ? TechnicalFieldTypeEnum.Text : TechnicalFieldTypeEnum.Bool;
                }

                newTechnicalFields.Add(technicalModel);
            }

            foreach (var newItem in newGrouped)
            {
                var technicalModel = new TechnicalFieldViewModel();

                field = newItem.FirstOrDefault(x => x.Name == "technicalFieldName");
                if (field != null)
                {
                    technicalModel.Name = field.Value;
                }

                field = newItem.FirstOrDefault(x => x.Name.Contains("typeTF-new-"));
                if (field != null)
                {
                    technicalModel.Type = field.Value == "text" ? TechnicalFieldTypeEnum.Text : TechnicalFieldTypeEnum.Bool;
                }

                field = newItem.FirstOrDefault(x => x.Name == "IsActive");
                if (field != null)
                {
                    technicalModel.IsActive = !GetBoolValue(field.Value);
                }

                newTechnicalFields.Add(technicalModel);
            }

            model.Template.Common.TechnicalFields = newTechnicalFields;
        }

        private static void UpdateEditIndicatorPageViewModelCommon<T>(this EditIndicatorPageViewModel<T> model, ClientFieldData[] formData) where T : new()
        {
            var field = formData.FirstOrDefault(x => x.Name == "indicatorTemplate");
            if (field != null)
            {
                model.Indicator.Common.TemplateVersionId = int.Parse(field.Value);
            }

            field = formData.First(x => x.Name == "indicatorNumber");
            model.Indicator.Common.IndicatorNumber = null;
            if (field != null)
            {
                var indicatorNumber = field.Value.ToString();
                model.Indicator.Common.IndicatorNumber = indicatorNumber;
            }

            field = formData.First(x => x.Name == "effectiveDate");
            model.Indicator.Common.EffectiveDate = null;
            if (field != null)
            {
                var date = DateTime.ParseExact(field.Value, DATETIME_PARSE_FORMAT, CultureInfo.InvariantCulture);
                model.Indicator.Common.EffectiveDate = date;
            }

            field = formData.First(x => x.Name == "expirationDate");
            model.Indicator.Common.ExpirationDate = null;
            if (field != null)
            {
                var date = DateTime.ParseExact(field.Value, DATETIME_PARSE_FORMAT, CultureInfo.InvariantCulture);
                model.Indicator.Common.ExpirationDate = date;
            }

            field = formData.First(x => x.Name == "indicatorNameEn");
            if (field != null)
            {
                model.Indicator.Common.IndicatorNameEn = field.Value;
            }

            field = formData.FirstOrDefault(x => x.Name == "indicatorNameEs");
            model.Indicator.Common.IndicatorNameEs = string.Empty;
            if (field != null)
            {
                model.Indicator.Common.IndicatorNameEs = field.Value;
            }

            field = formData.FirstOrDefault(x => x.Name == "indicatorNamePt");
            model.Indicator.Common.IndicatorNamePt = string.Empty;
            if (field != null)
            {
                model.Indicator.Common.IndicatorNamePt = field.Value;
            }

            field = formData.FirstOrDefault(x => x.Name == "indicatorNameFr");
            model.Indicator.Common.IndicatorNameFr = string.Empty;
            if (field != null)
            {
                model.Indicator.Common.IndicatorNameFr = field.Value;
            }

            field = formData.FirstOrDefault(x => x.Name == "unitMeasure");
            model.Indicator.Common.UnitMeasureId = null;
            if (field != null)
            {
                model.Indicator.Common.UnitMeasureId = int.Parse(field.Value);
            }

            field = formData.FirstOrDefault(x => x.Name == "unitMeasureEn");
            model.Indicator.Common.UnitMeasureEn = string.Empty;
            if (field != null)
            {
                model.Indicator.Common.UnitMeasureEn = field.Value;
            }

            field = formData.FirstOrDefault(x => x.Name == "unitMeasureEs");
            model.Indicator.Common.UnitMeasureEs = string.Empty;
            if (field != null)
            {
                model.Indicator.Common.UnitMeasureEs = field.Value;
            }

            field = formData.FirstOrDefault(x => x.Name == "unitMeasurePt");
            model.Indicator.Common.UnitMeasurePt = string.Empty;
            if (field != null)
            {
                model.Indicator.Common.UnitMeasurePt = field.Value;
            }

            field = formData.FirstOrDefault(x => x.Name == "unitMeasureFr");
            model.Indicator.Common.UnitMeasureFr = string.Empty;
            if (field != null)
            {
                model.Indicator.Common.UnitMeasureFr = field.Value;
            }

            field = formData.FirstOrDefault(x => x.Name == "strAlingmentImageAsociated");
            model.Indicator.Common.StrAlingImageId = null;
            if (field != null && !string.IsNullOrEmpty(field.Value))
            {
                 model.Indicator.Common.StrAlingImageId = int.Parse(field.Value);
            }

            model.Indicator.Common.TechnicalFields.Clear();
            var technicalFields = formData.Where(x => x.Name == "technicalFieldValue");
            foreach (var item in technicalFields)
            {
                var it = new TechnicalFieldViewModel()
                {
                    TechnicalFieldTemplateId = int.Parse(item.Id),
                    Value = item.Value
                };
                model.Indicator.Common.TechnicalFields.Add(it);
            }
        }

        private static List<AttributeLevelViewModel> GetMultiLevelData(this ClientFieldData[] formData, string name)
        {
            var currentLanguage = Localization.CurrentLanguage;
            var result = new List<AttributeLevelViewModel>();

            var fieldsNameLevel = new string[] 
            {
                string.Format("{0}-levelEn", name),
                string.Format("{0}-levelEs", name),
                string.Format("{0}-levelPt", name),
                string.Format("{0}-levelFr", name),
                string.Format("{0}-level-disabled", name),
            };

            var fieldsNameCategory = new string[] 
            {
                string.Format("{0}-categoryEn", name),
                string.Format("{0}-categoryEs", name),
                string.Format("{0}-categoryPt", name),
                string.Format("{0}-categoryFr", name),
                string.Format("{0}-category-disabled", name),
            };

            var fieldsLevel = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && fieldsNameLevel.Any(y => x.Name == y));
            var fieldsCategory = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && fieldsNameCategory.Any(y => x.Name == y));

            var fieldsLevelGrouped = fieldsLevel.GroupBy(x => x.Id);

            foreach (var levelItem in fieldsLevelGrouped)
            {
                var levelId = 0;
                var levelModel = new AttributeLevelViewModel();
                result.Add(levelModel);

                var levelField = levelItem.FirstOrDefault(x => x.Name == fieldsNameLevel[0]);
                levelModel.NameEn = null;
                if (levelField != null)
                {
                    levelModel.NameEn = levelField.Value;
                }

                levelField = levelItem.FirstOrDefault(x => x.Name == fieldsNameLevel[1]);
                levelModel.NameEs = null;
                if (levelField != null)
                {
                    levelModel.NameEs = levelField.Value;
                }

                levelField = levelItem.FirstOrDefault(x => x.Name == fieldsNameLevel[2]);
                levelModel.NamePt = null;
                if (levelField != null)
                {
                    levelModel.NamePt = levelField.Value;
                }

                levelField = levelItem.FirstOrDefault(x => x.Name == fieldsNameLevel[3]);
                levelModel.NameFr = null;
                if (levelField != null)
                {
                    levelModel.NameFr = levelField.Value;
                }

                levelField = levelItem.FirstOrDefault(x => x.Name == fieldsNameLevel[4]);
                levelModel.IsEnabled = false;
                if (levelField != null)
                {
                    levelModel.IsEnabled = !GetBoolValue(levelField.Value);
                }

                if (!levelItem.Key.Contains("new"))
                {
                    levelId = int.Parse(levelItem.Key);
                    levelModel.LevelId = levelId;
                }

                var categoriesFromLevel = fieldsCategory.Where(x => x.ExtraData.Any(y => (y.Key == "data-persist-parent-id") && (y.Value == levelItem.Key)));
                var categoriesFromLevelGrouped = categoriesFromLevel.GroupBy(x => x.Id);
                foreach (var categoryItem in categoriesFromLevelGrouped)
                {
                    var categoryModel = new AttributeCategoryViewModel()
                    {
                        CreateLanguage = currentLanguage,
                        LevelId = levelId
                    };
                    levelModel.Categories.Add(categoryModel);

                    var field = categoryItem.FirstOrDefault(x => x.Name == fieldsNameCategory[0]);
                    categoryModel.NameEn = null;
                    if (field != null)
                    {
                        categoryModel.NameEn = field.Value;
                    }

                    field = categoryItem.FirstOrDefault(x => x.Name == fieldsNameCategory[1]);
                    categoryModel.NameEs = null;
                    if (field != null)
                    {
                        categoryModel.NameEs = field.Value;
                    }

                    field = categoryItem.FirstOrDefault(x => x.Name == fieldsNameCategory[2]);
                    categoryModel.NamePt = null;
                    if (field != null)
                    {
                        categoryModel.NamePt = field.Value;
                    }

                    field = categoryItem.FirstOrDefault(x => x.Name == fieldsNameCategory[3]);
                    categoryModel.NameFr = null;
                    if (field != null)
                    {
                        categoryModel.NameFr = field.Value;
                    }

                    field = categoryItem.FirstOrDefault(x => x.Name == fieldsNameCategory[4]);
                    categoryModel.IsEnabled = false;
                    if (field != null)
                    {
                        categoryModel.IsEnabled = !GetBoolValue(field.Value);
                    }

                    if (!categoryItem.Key.Contains("new"))
                    {
                        categoryModel.CategoryId = int.Parse(categoryItem.Key);
                    }
                }
            }

            return result;
        }

        private static List<AttributeCategoryViewModel> GetSimpleLevelData(this ClientFieldData[] formData, string name)
        {
            var result = new List<AttributeCategoryViewModel>();

            var fieldsName = new string[] 
            {
                string.Format("{0}-categoryEn", name),
                string.Format("{0}-categoryEs", name),
                string.Format("{0}-categoryPt", name),
                string.Format("{0}-categoryFr", name),
                string.Format("{0}-disabled", name),
                string.Format("{0}-code", name),
            };

            var fields = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && fieldsName.Any(y => x.Name == y));

            var fieldsGrouped = fields.GroupBy(x => x.Id);

            foreach (var item in fieldsGrouped)
            {
                var model = new AttributeCategoryViewModel()
                {
                    CreateLanguage = Localization.CurrentLanguage
                };
                result.Add(model);

                var field = item.FirstOrDefault(x => x.Name == fieldsName[0]);
                model.NameEn = null;
                if (field != null)
                {
                    model.NameEn = field.Value;
                }

                field = item.FirstOrDefault(x => x.Name == fieldsName[1]);
                model.NameEs = null;
                if (field != null)
                {
                    model.NameEs = field.Value;
                }

                field = item.FirstOrDefault(x => x.Name == fieldsName[2]);
                model.NamePt = null;
                if (field != null)
                {
                    model.NamePt = field.Value;
                }

                field = item.FirstOrDefault(x => x.Name == fieldsName[3]);
                model.NameFr = null;
                if (field != null)
                {
                    model.NameFr = field.Value;
                }

                field = item.FirstOrDefault(x => x.Name == fieldsName[4]);
                model.IsEnabled = false;
                if (field != null)
                {
                    model.IsEnabled = !GetBoolValue(field.Value);
                }

                field = item.FirstOrDefault(x => x.Name == fieldsName[5]);
                model.Code = null;
                if (field != null)
                {
                    model.Code = field.Value;
                }

                if (!item.Key.Contains("new"))
                {
                    model.CategoryId = int.Parse(item.Key);
                }
            }

            return result;
        }

        private static List<SubindicatorViewModel> GetSubindicatorData(this ClientFieldData[] formData, string name)
        {
            var result = new List<SubindicatorViewModel>();

            var fieldsName = new string[] 
            {
                string.Format("{0}-nameEn", name),
                string.Format("{0}-nameEs", name),
                string.Format("{0}-namePt", name),
                string.Format("{0}-nameFr", name),
            };

            var fields = formData.Where(x => !string.IsNullOrWhiteSpace(x.Id) && fieldsName.Any(y => x.Name == y));

            var fieldsGrouped = fields.GroupBy(x => x.Id);

            foreach (var item in fieldsGrouped)
            {
                var model = new SubindicatorViewModel();
                result.Add(model);

                var field = item.FirstOrDefault(x => x.Name == fieldsName[0]);
                model.NameEn = null;
                if (field != null)
                {
                    model.NameEn = field.Value;
                }

                field = item.FirstOrDefault(x => x.Name == fieldsName[1]);
                model.NameEs = null;
                if (field != null)
                {
                    model.NameEs = field.Value;
                }

                field = item.FirstOrDefault(x => x.Name == fieldsName[2]);
                model.NamePt = null;
                if (field != null)
                {
                    model.NamePt = field.Value;
                }

                field = item.FirstOrDefault(x => x.Name == fieldsName[3]);
                model.NameFr = null;
                if (field != null)
                {
                    model.NameFr = field.Value;
                }

                if (!item.Key.Contains("new"))
                {
                    model.SubindicatorId = int.Parse(item.Key);
                }
            }

            return result;
        }

        private static IEnumerable<int> GetIdsToRemove<T1, T2>(IEnumerable<T1> originalItems, IEnumerable<T2> finalItems, Func<T1, int> originalItemId, Func<T2, int> finalItemId)
        {
            var originalIds = originalItems.Select(originalItemId).ToList();
            var finalIds = finalItems.Select(finalItemId).ToList();

            var itemsToRemove = originalIds.Where(x => !finalIds.Any(y => x == y)).ToList();
            return itemsToRemove;
        }

        private static bool GetBoolValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            value = value.ToLowerInvariant();
            return value == "true";
        }
        #endregion
    }
}