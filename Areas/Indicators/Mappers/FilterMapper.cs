using System;
using System.Collections.Generic;
using System.Globalization;

using IDB.MW.Application.IndicatorsModuleNew.ViewModels.Core;
using IDB.MW.Infrastructure.BaseClasses.DataTable;
using IDB.MW.Infrastructure.Configuration;

namespace IDB.Presentation.MVC4.Areas.Indicators.Mappers
{
    public static class FilterMapper
    {
        #region Constants
        public static readonly string DATETIME_PARSE_FORMAT;
        public static readonly string DATETIME_PARSE_FORMAT_UPLOAD;
        #endregion

        #region Constructors
        static FilterMapper()
        {
            DATETIME_PARSE_FORMAT = ConfigurationServiceFactory.Current.GetApplicationSettings().FormatDate;
            DATETIME_PARSE_FORMAT_UPLOAD = ConfigurationServiceFactory.Current.GetApplicationSettings().FormatDateUpload;
        }
        #endregion

        #region Mappers
        public static IndicatorsFilterViewModel ConvertToIndicatorsFilterViewModel(this DataTableRequest filter)
        {
            var result = new IndicatorsFilterViewModel();
            string filterValue = null;
            if (filter.CustomFilters.ContainsKey("Name"))
            {
                filterValue = filter.CustomFilters["Name"];
                if (!string.IsNullOrWhiteSpace(filterValue))
                {
                    result.IndicatorName = filterValue;
                }
            }

            if (filter.CustomFilters.ContainsKey("TemplateVersion"))
            {
                filterValue = filter.CustomFilters["TemplateVersion"];
                if (!string.IsNullOrWhiteSpace(filterValue))
                {
                    result.TemplateVersionId = int.Parse(filterValue);
                }
            }

            if (filter.CustomFilters.ContainsKey("EffectiveDate"))
            {
                filterValue = filter.CustomFilters["EffectiveDate"];
                if (!string.IsNullOrWhiteSpace(filterValue))
                {
                    result.EffectiveDate = DateTime.ParseExact(filterValue, DATETIME_PARSE_FORMAT_UPLOAD, CultureInfo.InvariantCulture);
                }
            }

            if (filter.CustomFilters.ContainsKey("ExpirationDate"))
            {
                filterValue = filter.CustomFilters["ExpirationDate"];
                if (!string.IsNullOrWhiteSpace(filterValue))
                {
                    result.ExpirationDate = DateTime.ParseExact(filterValue, DATETIME_PARSE_FORMAT_UPLOAD, CultureInfo.InvariantCulture);
                }
            }

            result.IsActive = string.Empty;
            if (filter.CustomFilters.ContainsKey("IsActive"))
            {
                result.IsActive = filter.CustomFilters["IsActive"];
            }

            return result;
        }

        public static DataTableRequest ConvertToDataTableRequest(this IndicatorsFilterViewModel filter)
        {
            var request = new DataTableRequest()
            {
                CustomFilters = new Dictionary<string, string>() { { "IsActive", "IsActive" } }
            };

            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.IndicatorName))
                {
                    request.CustomFilters.Add("Name", filter.IndicatorName);
                }

                if (filter.TemplateVersionId.HasValue)
                {
                    request.CustomFilters.Add("TemplateVersion", filter.TemplateVersionId.Value.ToString());
                }

                if (filter.EffectiveDate.HasValue)
                {
                    request.CustomFilters.Add("EffectiveDate", filter.EffectiveDate.Value.ToString(DATETIME_PARSE_FORMAT));
                }

                if (filter.ExpirationDate.HasValue)
                {
                    request.CustomFilters.Add("ExpirationDate", filter.ExpirationDate.Value.ToString(DATETIME_PARSE_FORMAT));
                }

                request.CustomFilters["IsActive"] = filter.IsActive;
            }

            return request;
        }
        #endregion
    }
}