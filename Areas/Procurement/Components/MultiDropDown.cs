using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.BEOProcurement.Components
{
    public class MultiDropDown : IHtmlString
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
        private bool _required;
        #endregion

        public MultiDropDown()
        {
            _items = new List<MultiDropDownItem>();
            _initialize = true;
            _attributes = new Dictionary<string, string>();
            _required = false;
        }

        public MultiDropDown Name(string name)
        {
            _name = name;
            return this;
        }

        public MultiDropDown PlaceHolder(string placeHolder)
        {
            _placeHolder = placeHolder;
            return this;
        }

        public MultiDropDown HtmlClass(string htmlClass)
        {
            _htmlClass = htmlClass;
            return this;
        }

        public MultiDropDown Disable(bool disable = true)
        {
            _disable = disable;
            return this;
        }

        public MultiDropDown Initialize(bool initialize = true)
        {
            _initialize = initialize;
            return this;
        }

        public MultiDropDown CanSelectGroup(bool canSelectGroup = true)
        {
            _groupSelectables = canSelectGroup;
            return this;
        }

        public MultiDropDown Items(List<MultiDropDownItem> items)
        {
            if (items != null)
            {
                _items = items;
            }

            return this;
        }

        public MultiDropDown AddItem(MultiDropDownItem item)
        {
            if (_items == null)
            {
                _items = new List<MultiDropDownItem>();
            }

            _items.Add(item);
            return this;
        }

        public MultiDropDown Attributes(IDictionary<string, string> attributes)
        {
            if (attributes != null)
            {
                _attributes = attributes;
            }

            return this;
        }

        public MultiDropDown AddAttributes(string name, string value)
        {
            if (_attributes == null)
            {
                _attributes = new Dictionary<string, string>();
            }

            _attributes.Add(name, value);
            return this;
        }

        public MultiDropDown SelectedItems(List<string> selectedValues)
        {
            _selectedValues = selectedValues;
            return this;
        }

        public MultiDropDown SelectedItems(List<int> selectedValues)
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

        public MultiDropDown Required(bool required = true)
        {
            _required = required;
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

            if (_required)
            {
                html.MergeAttribute("required", "required", true);
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
}