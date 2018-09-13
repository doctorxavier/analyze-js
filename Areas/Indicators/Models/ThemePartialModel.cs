using System;
using System.Collections.Generic;

namespace IDB.Presentation.MVC4.Areas.Indicators.Models
{
    public class ThemePartialModel
    {
        public ThemePartialModel()
        {
            ThemeList = new List<MultiDropDownItem>();
            FundsIndByTheme = new Dictionary<int, List<Tuple<string, List<MultiDropDownItem>>>>();
            ThemeIds = new List<int>();
            FundIdsByTheme = new Dictionary<int, List<int>>();
        }

        public List<MultiDropDownItem> ThemeList { get; set; }
        public Dictionary<int, List<Tuple<string, List<MultiDropDownItem>>>> FundsIndByTheme { get; set; }
        public List<int> ThemeIds { get; set; }
        public Dictionary<int, List<int>> FundIdsByTheme { get; set; }

        public bool IsReadMode { get; set; }
    }
}