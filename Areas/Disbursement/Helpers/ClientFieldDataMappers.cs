using System.Collections.Generic;

using IDB.Presentation.MVC4.Models;

namespace IDB.Presentation.MVC4.Areas.Disbursement.Helpers
{
    public static class ClientFieldDataMappers
    {
        public static List<MW.Domain.Models.Disbursement.ProjectedMonthYearModel> ConvertToModel(
            this List<ProjectedMonthYearModel> viewModel)
        {
            var models = new List<MW.Domain.Models.Disbursement.ProjectedMonthYearModel>();
            if (viewModel != null)
            {
                foreach (var elem in viewModel)
                {
                    var obj = new MW.Domain.Models.Disbursement.ProjectedMonthYearModel
                    {
                        Month = elem.Month,
                        Year = elem.Year,
                        ProjectedAmount = elem.ProjectedAmount == null
                        ? decimal.MinValue : elem.ProjectedAmount.Value
                    };

                    models.Add(obj);
                }
            }

            return models;
        }

        public static List<MW.Domain.Models.Disbursement.ProjectedYearModel> ConvertToModel(
            this List<ProjectedYearModel> viewModel)
        {
            var models = new List<MW.Domain.Models.Disbursement.ProjectedYearModel>();
            if (viewModel != null)
            {
                foreach (var elem in viewModel)
                {
                    var obj = new MW.Domain.Models.Disbursement.ProjectedYearModel
                    {
                        Year = elem.Year,
                        Percentage = elem.Percentage,
                        Total = elem.Total,
                        ProjectedMonths = elem.ProjectedMonths.ConvertToModel()
                    };

                    models.Add(obj);
                }
            }

            return models;
        }

        public static List<MW.Domain.Models.Disbursement.ProjectedMonthModel> ConvertToModel(
            this List<ProjectedMonthModel> viewModel)
        {
            var models = new List<MW.Domain.Models.Disbursement.ProjectedMonthModel>();
            if (viewModel != null)
            {
                foreach (var elem in viewModel)
                {
                    var obj = new MW.Domain.Models.Disbursement.ProjectedMonthModel
                    {
                        Month = elem.Month,
                        ProjectedAmount = elem.ProjectedAmount == null
                        ? decimal.MinValue : elem.ProjectedAmount.Value
                    };

                    models.Add(obj);
                }
            }

            return models;
        }
    }
}