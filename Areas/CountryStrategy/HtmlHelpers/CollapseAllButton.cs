using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

using IDB.Architecture.Language;

namespace IDB.Presentation.MVC4.Areas.CountryStrategy.HtmlHelpers
{
    public static class CollapseAllButtonExtension
    {
        private static int internalId = 0;
        public static CollapseAllButton CollapseAll(this HtmlHelper html, string name)
        {
            if (internalId > 9999)
            {
                internalId = 0;
            }

            internalId++;
            return new CollapseAllButton(internalId).Name(name);
        }
    }

    public class CollapseAllButton : IHtmlString
    {
        #region fields
        private string _name;
        private string _htmlClass;
        private IDictionary<string, string> _attributes;
        private string _expandText;
        private string _collapseText;
        private string _collapseSingleSelector;
        private List<string> _collapseRegionSelector;
        private string _id;
        #endregion

        public CollapseAllButton(int id)
        {
            _id = string.Format("collapse-all-{0}", id);
            _collapseRegionSelector = new List<string>();
            _attributes = new Dictionary<string, string>();
        }

        public CollapseAllButton Name(string name)
        {
            _name = name;
            return this;
        }

        public CollapseAllButton HtmlClass(string htmlClass)
        {
            _htmlClass = htmlClass;
            return this;
        }

        public CollapseAllButton Attributes(IDictionary<string, string> attributes)
        {
            if (attributes != null)
            {
                _attributes = attributes;
            }

            return this;
        }

        public CollapseAllButton AddAttributes(string name, string value)
        {
            if (_attributes == null)
            {
                _attributes = new Dictionary<string, string>();
            }

            _attributes.Add(name, value);
            return this;
        }

        public CollapseAllButton ExpandText(string text)
        {
            _expandText = text;
            return this;
        }

        public CollapseAllButton CollapseText(string text)
        {
            _collapseText = text;
            return this;
        }

        public CollapseAllButton CollapseSingleSelector(string selector)
        {
            _collapseSingleSelector = selector;
            return this;
        }

        public CollapseAllButton CollapseRegions(List<string> regions)
        {
            if (regions != null)
            {
                _collapseRegionSelector = regions;
            }

            return this;
        }

        public CollapseAllButton AddRegionToCollapse(string region)
        {
            if (_collapseRegionSelector == null)
            {
                _collapseRegionSelector = new List<string>();
            }

            _collapseRegionSelector.Add(region);
            return this;
        }

        public string ToHtmlString()
        {
            var html = new TagBuilder("div");
            html.MergeAttribute("data-role", "collapse-all", true);

            html.AddCssClass("buttonExpand");
            html.AddCssClass("collapse");
            if (!string.IsNullOrWhiteSpace(_htmlClass))
            {
                html.AddCssClass(_htmlClass);
            }

            var expandText = _expandText;
            if (string.IsNullOrWhiteSpace(expandText))
            {
                expandText = Localization.GetText("GLOBAL.CONTROL.EXPANDALL");
            }

            var collapseText = _collapseText;
            if (string.IsNullOrWhiteSpace(collapseText))
            {
                collapseText = Localization.GetText("GLOBAL.CONTROL.COLLAPSEALL");
            }

            html.MergeAttribute("data-collapse-all-exp-text", expandText, true);
            html.MergeAttribute("data-collapse-all-coll-text", collapseText, true);

            if (string.IsNullOrWhiteSpace(_name))
            {
                _name = _id;
            }

            html.MergeAttribute("data-name", _name, true);
            html.MergeAttribute("id", _id, true);

            foreach (var attribute in _attributes)
            {
                html.MergeAttribute(attribute.Key, attribute.Value, true);
            }

            if (!string.IsNullOrWhiteSpace(_collapseSingleSelector))
            {
                html.MergeAttribute("data-collapse-all-collapse-single", _collapseSingleSelector, true);
            }
            else if (_collapseRegionSelector.Any())
            {
                html.MergeAttribute("data-collapse-all-collapse-region", string.Join(",", _collapseRegionSelector), true);
            }

            var switchButton = new TagBuilder("span");
            switchButton.InnerHtml = "<span></span>";

            var textRegion = new TagBuilder("label");
            textRegion.InnerHtml = collapseText;

            var htmlBuilder = new StringBuilder();

            htmlBuilder.Append(switchButton.ToString());
            htmlBuilder.Append("&nbsp;");
            htmlBuilder.Append(textRegion.ToString());

            html.InnerHtml = htmlBuilder.ToString();

            return html.ToString();
        }
    }
}