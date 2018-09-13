﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;

using IDB.MW.Infrastructure.Helpers;

namespace IDB.Presentation.MVC4.Areas.CountryStrategy.HtmlHelpers
{
    public static class CollapseSingleButtonExtension
    {
        private static int internalId = 0;
        public static CollapseSingleButton CollapseSingle(this HtmlHelper html, string name)
        {
            if (internalId > 9999)
            {
                internalId = 0;
            }

            internalId++;
            return new CollapseSingleButton(internalId).Name(name);
        }
    }

    public class CollapseSingleButton : IHtmlString
    {
        private string _name;
        private string _htmlClass;
        private IDictionary<string, string> _attributes;
        private string _targetRegionSelector;
        private string _asociatedCollapseAllSelector;
        private string _id;
        private string _duration = "fast";

        public CollapseSingleButton(int id)
        {
            _id = string.Format("collapse-single-{0}", id);
            _attributes = new Dictionary<string, string>();
        }

        public CollapseSingleButton Name(string name)
        {
            _name = name;
            return this;
        }

        public CollapseSingleButton HtmlClass(string htmlClass)
        {
            _htmlClass = htmlClass;
            return this;
        }

        public CollapseSingleButton Attributes(IDictionary<string, string> attributes)
        {
            if (attributes != null)
            {
                _attributes = attributes;
            }

            return this;
        }

        public CollapseSingleButton AddAttributes(string name, string value)
        {
            if (_attributes == null)
            {
                _attributes = new Dictionary<string, string>();
            }

            _attributes.Add(name, value);
            return this;
        }

        public CollapseSingleButton AsociatedCollapseAllSelector(string selector)
        {
            _asociatedCollapseAllSelector = selector;
            return this;
        }

        public CollapseSingleButton TargetRegionSelector(string selector)
        {
            _targetRegionSelector = selector;
            return this;
        }

        public CollapseSingleButton TargetRegionSelector(Action<SelectorFactory> builder)
        {
            var selectorFactory = new SelectorFactory();
            builder(selectorFactory);
            _targetRegionSelector = selectorFactory.ConvertToSelectorCode();
            return this;
        }

        public CollapseSingleButton AnimationDuration(int milliseconds = 400)
        {
            _duration = milliseconds.ToString();
            return this;
        }

        public CollapseSingleButton AnimationSlow()
        {
            _duration = "slow";
            return this;
        }

        public CollapseSingleButton AnimationFast()
        {
            _duration = "fast";
            return this;
        }

        public string ToHtmlString()
        {
            var content = new TagBuilder("span");

            content.MergeAttribute("data-role", "collapse-single", true);
            content.MergeAttribute("data-collapse-single-region", _targetRegionSelector, true);
            content.MergeAttribute("data-collapse-single-duration", _duration, true);

            content.AddCssClass("collapse-single-button");
            if (!string.IsNullOrWhiteSpace(_htmlClass))
            {
                content.AddCssClass(_htmlClass);
            }

            if (string.IsNullOrWhiteSpace(_name))
            {
                _name = _id;
            }

            content.MergeAttribute("data-name", _name, true);
            content.MergeAttribute("id", _id, true);

            if (!string.IsNullOrWhiteSpace(_asociatedCollapseAllSelector))
            {
                content.MergeAttribute("data-collapse-single-collapseAll", _asociatedCollapseAllSelector, true);
            }

            foreach (var attribute in _attributes)
            {
                content.MergeAttribute(attribute.Key, attribute.Value, true);
            }

            var html = new StringBuilder();
            html.Append(content.ToString());
            return html.ToString();
        }

        private enum SelectorOptions
        {
            [Description("parent")]
            Parent,

            [Description("closest")]
            Closest,

            [Description("next")]
            Next,
        }

        public class SelectorFactory
        {
            private Dictionary<SelectorOptions, string> _selector;

            public SelectorFactory()
            {
                _selector = new Dictionary<SelectorOptions, string>();
            }

            public SelectorFactory Parent()
            {
                _selector.Add(SelectorOptions.Parent, null);
                return this;
            }

            public SelectorFactory Closest(string selector)
            {
                _selector.Add(SelectorOptions.Closest, selector);
                return this;
            }

            public SelectorFactory Next()
            {
                _selector.Add(SelectorOptions.Next, null);
                return this;
            }

            public string ConvertToSelectorCode()
            {
                var selector = new StringBuilder("@");

                foreach (var item in _selector)
                {
                    selector.Append(";");
                    selector.Append(item.Key.GetEnumDescription());
                    if (!string.IsNullOrWhiteSpace(item.Value))
                    {
                        selector.Append(string.Format("({0})", item.Value));
                    }
                }

                return selector.ToString();
            }
        }
    }
}