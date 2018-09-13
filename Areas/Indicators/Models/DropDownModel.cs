using System.Collections.Generic;

namespace IDB.Presentation.MVC4.Areas.Indicators.Models
{
    public class DropDownModel
    {
        #region fields
        private string _value;
        private string _name;
        private List<object> _optionList;
        private string _id;
        private string _parentDropdownId;
        private bool _showEmptyOption;
        private string _placeholder;
        private bool _validateOnChange;
        private string _valuePropertyName;
        private string _textPropertyName;
        private Dictionary<string, string> _attributes;
        private bool _required;
        private string _htmlClass;
        #endregion

        #region Properties
        public string Value
        {
            get
            {
                return _value;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public List<object> OptionList
        {
            get
            {
                return _optionList;
            }
        }

        public string Id
        {
            get
            {
                return _id;
            }
        }

        public string ParentDropdownId
        {
            get
            {
                return _parentDropdownId;
            }
        }

        public bool ShowEmptyOption
        {
            get
            {
                return _showEmptyOption;
            }
        }

        public string Placeholder
        {
            get
            {
                return _placeholder;
            }
        }

        public bool ValidateOnChange
        {
            get
            {
                return _validateOnChange;
            }
        }

        public string ValuePropertyName
        {
            get
            {
                return _valuePropertyName;
            }
        }

        public string TextPropertyName
        {
            get
            {
                return _textPropertyName;
            }
        }

        public Dictionary<string, string> Attributes
        {
            get
            {
                return _attributes;
            }
        }

        public bool Required
        {
            get
            {
                return _required;
            }
        }

        public string HtmlClass
        {
            get
            {
                return _htmlClass;
            }
        }
        #endregion

        #region Constructors
        public DropDownModel(
            int? value,
            string name,
            List<object> optionList,
            string id = null,
            string parentDropdownId = null,
            bool required = true,
            bool showEmptyOption = true,
            string placeholder = "(select option)",
            bool validateOnChange = true,
            string valuePropertyName = "Value",
            string textPropertyName = "Text",
            Dictionary<string, string> attributes = null,
            string htmlClass = null)
        {
            _value = null;
            if (value.HasValue)
            {
                _value = value.Value.ToString();
            }

            _name = name;
            _optionList = optionList;
            _id = id;
            _parentDropdownId = parentDropdownId;
            _showEmptyOption = showEmptyOption;
            _placeholder = placeholder;
            _validateOnChange = validateOnChange;
            _valuePropertyName = valuePropertyName;
            _textPropertyName = textPropertyName;
            _required = required;
            _attributes = attributes;
            _htmlClass = htmlClass;
        }

        public DropDownModel(
            string value,
            string name,
            List<object> optionList,
            string id = null,
            string parentDropdownId = null,
            bool required = true,
            bool showEmptyOption = true,
            string placeholder = "(select option)",
            bool validateOnChange = true,
            string valuePropertyName = "Value",
            string textPropertyName = "Text",
            Dictionary<string, string> attributes = null,
            string htmlClass = null)
        {
            _value = value;
            _name = name;
            _optionList = optionList;
            _id = id;
            _parentDropdownId = parentDropdownId;
            _required = required;
            _showEmptyOption = showEmptyOption;
            _placeholder = placeholder;
            _validateOnChange = validateOnChange;
            _valuePropertyName = valuePropertyName;
            _textPropertyName = textPropertyName;
            _attributes = attributes;
            _htmlClass = htmlClass;
        }
        #endregion
    }
}