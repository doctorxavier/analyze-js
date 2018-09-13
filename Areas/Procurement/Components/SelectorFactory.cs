using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

using IDB.MW.Infrastructure.Helpers;

namespace IDB.Presentation.MVC4.Areas.BEOProcurement.Components
{
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

        private enum SelectorOptions
        {
            [Description("parent")]
            Parent,

            [Description("closest")]
            Closest,

            [Description("next")]
            Next,
        }
    }
}