using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;

using IDB.MW.Domain.Entities;
using IDB.MW.Application.FinancialPlanModule.ViewModels;
using IDB.MW.Application.FinancialPlanModule.Services;
using IDB.MW.Application.FinancialPlanModule.Messages;
using IDB.Architecture.Language;
using IDB.MW.Application.FinancialPlanModule.Helpers;

namespace IDB.Presentation.MVC4.Areas.FinancialPlan.Models
{
    public static class ViewModelMapperHelper
    {
        public static List<SelectListItem> ConvertToSelectItems(this List<FinancialContractViewModel> contracts)
        {
            var listContract = new List<SelectListItem>();
            bool isFirst = true;
            foreach (var item in contracts)
            {
                listContract.Add(new SelectListItem()
                {
                    Selected = isFirst,
                    Text = item.ContractNumber,
                    Value = item.ContractNumber
                });

                isFirst = false;
            }

            return listContract;
        }

        public static List<SelectListItem> GetListMonth()
        {
            var monthListItem = new List<SelectListItem>();
            var monthList = new List<SelectListItem>();
            for (int i = 1; i <= 12; i++)
            {
                 monthListItem.Add(new SelectListItem
                {
                    Selected = false,
                    Value = i.ToString(),
                    Text = i == 6 ? Localization.GetText("PEP.TabFinancialProgress." + CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i) + "e") :
                            i == 7 ? Localization.GetText("PEP.TabFinancialProgress." + CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i) + "y") :
                            i == 9 ? Localization.GetText("PEP.TabFinancialProgress." + CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i) + "t") :
                            Localization.GetText("PEP.TabFinancialProgress." + CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i))
                });
            }

            monthListItem.First().Selected = true;
            return monthListItem;
        }

        public static List<SelectListItem> GetListYears(int min, int max)
        {
            bool firstmonth = true;
            var yearsListItem = new List<SelectListItem>();
            int year = min;
            int diffYear = max - min;
            for (int i = 0; i <= diffYear; i++)
            {
                yearsListItem.Add(new SelectListItem
                {
                    Selected = firstmonth,
                    Value = year.ToString(),
                    Text = year.ToString()
                });
                firstmonth = false;
                year++;
            }

            return yearsListItem;
        }

        public static List<SelectListItem> ConvertToSelectItems(this List<int> OperationYear)
        {
            var listYear = new List<SelectListItem>();
            bool isFirst = true;

            if (OperationYear == null || !OperationYear.Any())
            {
                OperationYear = new List<int>();
                OperationYear.Add(DateTime.Now.Year);
            }

            var nextYear = OperationYear.Last() + 1;

            if (OperationYear.Count == 1 && nextYear != 0)
            {
                OperationYear.Add(nextYear);
            }
            
            foreach (var item in OperationYear)
            {
                listYear.Add(new SelectListItem()
                {
                    Selected = isFirst,
                    Text = item.ToString(),
                    Value = item.ToString()
                });

                isFirst = false;
            }

            return listYear;
        }

        public static List<SelectListItem> ConvertExechangeToSelectItems(this List<FinancialProjectionsExchange> Exchange)
        {
            var listExchange = new List<SelectListItem>();
            bool isFirst = true;
            if (Exchange == null) return listExchange;
            foreach (var item in Exchange)
            {
                listExchange.Add(new SelectListItem()
                {
                    Selected = isFirst,
                    Text = item.ExchangeName,
                    Value = item.ExchangeValue.ToString()
                });

                isFirst = false;
            }

            return listExchange;
        }

        public static List<SelectListItem> GetTaskTypeForSearch(this List<string> TaskType)
        {
            var listTaskType = TaskType
                .Select(x => new SelectListItem { Text = x, Value = x }).ToList();

            return listTaskType;
        }

        public static List<SelectListItem> GetOperatorsForSearch(this List<string> OperatorSearchType)
        {
            var operatorsSearch = OperatorSearchType
                .Select(x => new SelectListItem { Text = x, Value = x }).ToList();

            return operatorsSearch;
        }
    }
}