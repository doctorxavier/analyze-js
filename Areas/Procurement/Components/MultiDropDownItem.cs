using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.BEOProcurement.Components
{
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