using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.Mvc;

using IDB.MW.Infrastructure.Helpers;

namespace IDB.Presentation.MVC4.Areas.CountryStrategy.Models
{
    public static class FilterBoxModelExtension
    {
        public static FilterBoxModel FilterBox(this HtmlHelper html, ColumnType columnType = ColumnType.MD_12)
        {
            return new FilterBoxModel(columnType);
        }
    }

    public class FilterBoxModel : IHtmlString
    {
        #region Properties
        public List<FilterBoxRow> FilterRows { get; set; }
        public ColumnType ColumnType { get; set; }
        public string CssClassMainCol { get; set; }
        public string CssClassMainRow { get; set; }
        public IDictionary<string, string> Attributes { get; set; }
        #endregion

        public FilterBoxModel(ColumnType columnType)
        {
            FilterRows = new List<FilterBoxRow>();
            ColumnType = columnType;
            Attributes = new Dictionary<string, string>();
        }

        public FilterBoxModel ClassMainRow(string cssClass)
        {
            CssClassMainRow = cssClass;
            return this;
        }

        public FilterBoxModel ClassMainCol(string cssClass)
        {
            CssClassMainCol = cssClass;
            return this;
        }

        public FilterBoxModel Rows(Action<FilterBoxRowFactory> builder)
        {
            var factory = new FilterBoxRowFactory(this);
            builder(factory);
            return this;
        }

        public FilterBoxModel SetAttributes(IDictionary<string, string> attributes)
        {
            Attributes = attributes;
            return this;
        }

        public FilterBoxModel AddAttribute(string name, string value)
        {
            if (Attributes == null)
            {
                Attributes = new Dictionary<string, string>();
            }

            Attributes.Add(name, value);
            return this;
        }

        public string ToHtmlString()
        {
            var mainRow = new TagBuilder("div");
            mainRow.AddCssClass("row");
            mainRow.AddCssClass(CssClassMainRow);

            foreach (var attribute in Attributes)
            {
                mainRow.MergeAttribute(attribute.Key, attribute.Value, true);
            }

            var mainCol = new TagBuilder("div");

            mainCol.AddCssClass(ColumnType.GetEnumDescription());
            mainCol.AddCssClass(CssClassMainCol);

            var innerHtml = new StringBuilder();
            var numRow = 0;
            foreach (var row in FilterRows)
            {
                innerHtml.Append(row.ToTagBuilder(numRow, FilterRows.Count).ToString());
                numRow++;
            }

            mainCol.InnerHtml = innerHtml.ToString();
            mainRow.InnerHtml = mainCol.ToString();
            return mainRow.ToString();
        }
    }

    public class FilterBoxRow
    {
        #region Properties
        public List<FilterBoxColumn> FilterColumns { get; set; }
        public string CssClass { get; set; }
        public IDictionary<string, string> Attributes { get; set; }
        #endregion

        public FilterBoxRow()
        {
            FilterColumns = new List<FilterBoxColumn>();
            Attributes = new Dictionary<string, string>();
        }

        public FilterBoxRow Columns(Action<FilterBoxColumnFactory> builder)
        {
            var factory = new FilterBoxColumnFactory(this);
            builder(factory);
            return this;
        }

        public FilterBoxRow Class(string cssClass)
        {
            CssClass = cssClass;
            return this;
        }

        public FilterBoxRow SetAttributes(IDictionary<string, string> attributes)
        {
            Attributes = attributes;
            return this;
        }

        public FilterBoxRow AddAttribute(string name, string value)
        {
            if (Attributes == null)
            {
                Attributes = new Dictionary<string, string>();
            }

            Attributes.Add(name, value);
            return this;
        }

        public TagBuilder ToTagBuilder(int? rowNumber = null, int? totalRows = null)
        {
            TagBuilder result = new TagBuilder("div");
            result.AddCssClass("row");

            foreach (var attribute in Attributes)
            {
                result.MergeAttribute(attribute.Key, attribute.Value, true);
            }

            if (rowNumber.HasValue && totalRows.HasValue)
            {
                if (rowNumber == totalRows - 1)
                {
                    result.AddCssClass("mt10");
                }
                else if (rowNumber > 0)
                {
                    result.AddCssClass("mt20");
                }
            }

            result.AddCssClass(CssClass);

            var innerHtml = new StringBuilder();
            foreach (var column in FilterColumns)
            {
                innerHtml.Append(column.ToTagBuilder().ToString());
            }

            result.InnerHtml = innerHtml.ToString();

            return result;
        }
    }

    public class FilterBoxColumn
    {
        #region Properties
        public ColumnType ColumnType { get; set; }
        public List<IHtmlString> Content { get; set; }
        public string CssClass { get; set; }
        #endregion

        public FilterBoxColumn(ColumnType columnType)
        {
            ColumnType = columnType;
            Content = new List<IHtmlString>();
        }

        public FilterBoxColumn AddContent(IHtmlString content)
        {
            Content.Add(content);
            return this;
        }

        public FilterBoxColumn Class(string cssClass)
        {
            CssClass = cssClass;
            return this;
        }

        public TagBuilder ToTagBuilder()
        {
            TagBuilder result = new TagBuilder("div");
            result.AddCssClass(ColumnType.GetEnumDescription());
            result.AddCssClass(CssClass);

            var innerHtml = new StringBuilder();
            foreach (var content in Content)
            {
                innerHtml.Append(content.ToHtmlString());
            }

            result.InnerHtml = innerHtml.ToString();

            return result;
        }
    }

    public class FilterBoxRowFactory
    {
        #region Fields
        private FilterBoxModel _filterBox;
        #endregion

        public FilterBoxRowFactory(FilterBoxModel filterBox)
        {
            _filterBox = filterBox;
        }

        public FilterBoxRow Add()
        {
            var row = new FilterBoxRow();
            _filterBox.FilterRows.Add(row);
            return row;
        }
    }

    public class FilterBoxColumnFactory
    {
        #region Fields
        private FilterBoxRow _filterBoxRow;
        #endregion

        public FilterBoxColumnFactory(FilterBoxRow filterBoxRow)
        {
            _filterBoxRow = filterBoxRow;
        }

        public FilterBoxColumn Add(ColumnType columnType)
        {
            var column = new FilterBoxColumn(columnType);
            _filterBoxRow.FilterColumns.Add(column);
            return column;
        }
    }

    public enum ColumnType
    {
        [Description("col-md-1")]
        MD_1,
        [Description("col-md-2")]
        MD_2,
        [Description("col-md-3")]
        MD_3,
        [Description("col-md-4")]
        MD_4,
        [Description("col-md-5")]
        MD_5,
        [Description("col-md-6")]
        MD_6,
        [Description("col-md-7")]
        MD_7,
        [Description("col-md-8")]
        MD_8,
        [Description("col-md-9")]
        MD_9,
        [Description("col-md-10")]
        MD_10,
        [Description("col-md-11")]
        MD_11,
        [Description("col-md-12")]
        MD_12
    }
}