using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.OPUS.Models
{
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
}