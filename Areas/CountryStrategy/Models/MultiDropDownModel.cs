using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.CountryStrategy.Models
{
    public static class MultiDropDownExtension
    {
        public static MultiDropDownModel MultiDropDown(this HtmlHelper html, string name)
        {
            return new MultiDropDownModel().Name(name);
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

    public class MultiDropDownModel : IHtmlString
    {
        #region fields
        private string _name;
        private string _htmlClass;
        private string _placeHolder;
        private bool _disable;
        private bool _initialize;
        private bool _groupSelectables;
        private List<MultiDropDownItem> _items;
        private List<string> _selectedValues;
        private IDictionary<string, string> _attributes;
        #endregion

        public MultiDropDownModel()
        {
            _items = new List<MultiDropDownItem>();
            _initialize = true;
            _attributes = new Dictionary<string, string>();
        }

        public MultiDropDownModel Name(string name)
        {
            _name = name;
            return this;
        }

        public MultiDropDownModel PlaceHolder(string placeHolder)
        {
            _placeHolder = placeHolder;
            return this;
        }

        public MultiDropDownModel HtmlClass(string htmlClass)
        {
            _htmlClass = htmlClass;
            return this;
        }

        public MultiDropDownModel Disable(bool disable = true)
        {
            _disable = disable;
            return this;
        }

        public MultiDropDownModel Initialize(bool initialize = true)
        {
            _initialize = initialize;
            return this;
        }

        public MultiDropDownModel CanSelectGroup(bool canSelectGroup = true)
        {
            _groupSelectables = canSelectGroup;
            return this;
        }

        public MultiDropDownModel Items(List<MultiDropDownItem> items)
        {
            if (items != null)
            {
                _items = items;
            }

            return this;
        }

        public MultiDropDownModel AddItem(MultiDropDownItem item)
        {
            if (_items == null)
            {
                _items = new List<MultiDropDownItem>();
            }

            _items.Add(item);
            return this;
        }

        public MultiDropDownModel Attributes(IDictionary<string, string> attributes)
        {
            if (attributes != null)
            {
                _attributes = attributes;
            }

            return this;
        }

        public MultiDropDownModel AddAttributes(string name, string value)
        {
            if (_attributes == null)
            {
                _attributes = new Dictionary<string, string>();
            }

            _attributes.Add(name, value);
            return this;
        }

        public MultiDropDownModel SelectedItems(List<string> selectedValues)
        {
            _selectedValues = selectedValues;
            return this;
        }

        public MultiDropDownModel SelectedItems(List<int> selectedValues)
        {
            if (selectedValues == null)
            {
                _selectedValues = null;
            }
            else
            {
                _selectedValues = selectedValues.Select(x => x.ToString()).ToList();
            }

            return this;
        }

        public string ToHtmlString()
        {
            var html = new TagBuilder("select");

            html.Attributes.Add("multiple", "multiple");
            html.Attributes.Add("data-role", "drop-multiple");
            html.AddCssClass("inputMultiSelect");

            if (_disable)
            {
                html.Attributes.Add("disabled", "disabled");
            }

            html.Attributes.Add("data-bind", "true");
            if (!_initialize)
            {
                html.MergeAttribute("data-bind", "false", true);
            }

            if (_groupSelectables)
            {
                html.Attributes.Add("data-selectable-groups", "true");
            }

            if (!string.IsNullOrWhiteSpace(_htmlClass))
            {
                html.AddCssClass(_htmlClass);
            }

            if (!string.IsNullOrWhiteSpace(_placeHolder))
            {
                html.Attributes.Add("data-placeholder", _placeHolder);
            }

            if (!string.IsNullOrWhiteSpace(_name))
            {
                html.Attributes.Add("name", _name);
            }

            foreach (var attribute in _attributes)
            {
                html.MergeAttribute(attribute.Key, attribute.Value, true);
            }

            if ((_selectedValues != null) && (_items != null))
            {
                foreach (var item in _items.ToPlainItems())
                {
                    item.Selected = _selectedValues.Any(x => x == item.Value);
                }
            }

            if ((_items != null) && _items.Any())
            {
                var innerHtml = new StringBuilder();
                foreach (var item in _items)
                {
                    var tagItem = item.ToTagBuilder();
                    innerHtml.Append(tagItem.ToString(TagRenderMode.Normal));
                }

                html.InnerHtml = innerHtml.ToString();
            }

            html.MergeAttribute("data-placeholder", "(select option)", true);

            var div = new TagBuilder("div");
            div.AddCssClass("chosenMultiSelect");
            div.InnerHtml = html.ToString();

            return div.ToString();
        }
    }

    public class MultiDropDownItem
    {
        public MultiDropDownItem()
        {
            Childrens = new List<MultiDropDownItem>();
        }

        public bool Selected { get; set; }

        public string Value { get; set; }

        public string Text { get; set; }

        public List<MultiDropDownItem> Childrens { get; set; }

        public bool IsParentNode
        {
            get
            {
                return (Childrens != null) && Childrens.Any();
            }
        }

        public TagBuilder ToTagBuilder()
        {
            TagBuilder result = null;
            if (IsParentNode)
            {
                result = new TagBuilder("optgroup");
                result.Attributes.Add("label", Text);
                var childs = new StringBuilder();
                foreach (var childred in Childrens)
                {
                    var childTag = childred.ToTagBuilder();
                    childs.Append(childTag.ToString());
                }

                result.InnerHtml = childs.ToString();
            }
            else
            {
                result = new TagBuilder("option");
                result.Attributes.Add("value", Value);
                if (Selected)
                {
                    result.Attributes.Add("selected", "selected");
                }

                result.SetInnerText(Text);
            }

            return result;
        }
    }
}