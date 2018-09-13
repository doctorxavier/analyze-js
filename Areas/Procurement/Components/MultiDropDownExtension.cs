using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.BEOProcurement.Components
{
    public static class MultiDropDownExtension
    {
        public static MultiDropDown MultiDropDown(this HtmlHelper html, string name)
        {
            return new MultiDropDown().Name(name);
        }

        public static List<string> GetSelectedText(this IEnumerable<MultiDropDownItem> items, IEnumerable<int> selectedValues)
        {
            if ((items == null) || (selectedValues == null))
            {
                return new List<string>();
            }

            var selectedString = selectedValues.Select(x => x.ToString()).ToList();
            return items.GetSelectedText(selectedString);
        }

        public static List<string> GetSelectedText(this IEnumerable<MultiDropDownItem> items, IEnumerable<string> selectedValues)
        {
            var plainList = items.ToPlainItems();
            var selectedItems = plainList.Where(x => selectedValues.Any(y => y == x.Value));
            return selectedItems.Select(x => x.Text).ToList();
        }

        public static Dictionary<string, List<string>> GetSelectedTextMultiLevel(this IEnumerable<MultiDropDownItem> items, IEnumerable<int> selectedValues)
        {
            if ((items == null) || (selectedValues == null))
            {
                return null;
            }

            var result = new Dictionary<string, List<string>>();

            foreach (var item in items)
            {
                var listChildrenSelected = item.Childrens.GetSelectedText(selectedValues);
                if (listChildrenSelected != null && listChildrenSelected.Any())
                {
                    result.Add(item.Text, listChildrenSelected);
                }
            }

            return result;
        }

        public static List<MultiDropDownItem> ToPlainItems(this IEnumerable<MultiDropDownItem> items)
        {
            var plainList = new List<MultiDropDownItem>();
            foreach (var item in items)
            {
                if ((item.Childrens == null) || (!item.Childrens.Any()))
                {
                    plainList.Add(item);
                }
                else
                {
                    var childrenPlain = item.Childrens.ToPlainItems();
                    plainList.AddRange(childrenPlain);
                }
            }

            return plainList;
        }
    }
}