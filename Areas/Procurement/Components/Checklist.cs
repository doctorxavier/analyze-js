using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

using Newtonsoft.Json;

using IDB.Architecture.Language;
using IDB.MW.Application.BEOProcurementModule.ViewModels.FirmProcurement;

namespace IDB.Presentation.MVC4.Areas.BEOProcurement.Components
{
    public class Checklist : IHtmlString
    {
        #region Constants
        private const string DEFAUL_COL_ITEM_NAME_KEY = "Checklist.ItemName";
        private const string DEFAUL_COL_MANDATORY_KEY = "Checklist.Mandatory";
        private const string DEFAUL_COL_COMPLETED_KEY = "Checklist.Completed";
        private const string DEFAUL_COL_DATE_COMPLETED_KEY = "Checklist.Date.Completed";
        private const string DEFAUL_COL_USER_KEY = "Checklist.User";
        #endregion

        #region fields
        private string _name;
        private string _htmlClass;

        private string _colItemNameText;
        private string _colIsMandatoryText;
        private string _colIsCompletedText;
        private string _colDateCompletesText;
        private string _colUserText;
        private string _currentUser;

        private bool _isEditable;
        private bool _validateOnLoad;
        private bool _canUncompleteAutomatic;
        private bool _scrollToError;

        private List<ChecklistRowViewModel> _items;
        private IDictionary<string, string> _attributes;
        private IDictionary<string, string> _validationMessages;

        private ValidationMessageFormatEnum _validationMessageFormat;

        private bool _isDateCompletedLessOrEqualToday;
        #endregion

        public Checklist()
        {
            _items = new List<ChecklistRowViewModel>();
            _attributes = new Dictionary<string, string>();
            _validationMessages = new Dictionary<string, string>();
            _validateOnLoad = true;
            _canUncompleteAutomatic = true;
            _scrollToError = true;
            _isDateCompletedLessOrEqualToday = false;
        }

        public Checklist Name(string name)
        {
            _name = name;
            return this;
        }

        public Checklist HeaderItemName(string header)
        {
            _colItemNameText = header;
            return this;
        }

        public Checklist HeaderMandatory(string header)
        {
            _colIsMandatoryText = header;
            return this;
        }

        public Checklist HeaderCompleted(string header)
        {
            _colIsCompletedText = header;
            return this;
        }

        public Checklist HeaderDateCompleted(string header)
        {
            _colDateCompletesText = header;
            return this;
        }

        public Checklist HeaderUser(string header)
        {
            _colUserText = header;
            return this;
        }

        public Checklist CurrentUser(string currentUser)
        {
            _currentUser = currentUser;
            return this;
        }

        public Checklist HtmlClass(string htmlClass)
        {
            _htmlClass = htmlClass;
            return this;
        }

        public Checklist Editable(bool isEditable = true)
        {
            _isEditable = isEditable;
            return this;
        }

        public Checklist ValidateOnLoad(bool validate = true)
        {
            _validateOnLoad = validate;
            return this;
        }

        public Checklist ValidationMessageFormat(ValidationMessageFormatEnum format = ValidationMessageFormatEnum.None)
        {
            _validationMessageFormat = format;
            return this;
        }

        public Checklist PermitUncompleteAutoCompleted(bool canUncompleteAutomatic = true)
        {
            _canUncompleteAutomatic = canUncompleteAutomatic;
            return this;
        }

        public Checklist ScrollToError(bool scroll = true)
        {
            _scrollToError = scroll;
            return this;
        }

        public Checklist Items(List<ChecklistRowViewModel> items)
        {
            if (items != null)
            {
                _items = items;
            }

            return this;
        }

        public Checklist AddItem(ChecklistRowViewModel item)
        {
            if (_items == null)
            {
                _items = new List<ChecklistRowViewModel>();
            }

            _items.Add(item);
            return this;
        }

        public Checklist Attributes(IDictionary<string, string> attributes)
        {
            if (attributes != null)
            {
                _attributes = attributes;
            }

            return this;
        }

        public Checklist AddAttributes(string name, string value)
        {
            if (_attributes == null)
            {
                _attributes = new Dictionary<string, string>();
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                _attributes.Add(name, value);
            }

            return this;
        }

        public Checklist ValidationMessages(IDictionary<string, string> messages)
        {
            if (messages != null)
            {
                _validationMessages = messages;
            }

            return this;
        }

        public Checklist AddValidationMessage(string validationName, string message)
        {
            if (_validationMessages == null)
            {
                _validationMessages = new Dictionary<string, string>();
            }

            if (!string.IsNullOrWhiteSpace(validationName))
            {
                _validationMessages.Add(validationName, message);
            }

            return this;
        }

        public Checklist DateCompletedLessOrEqualToday(bool lessOrEqual = true)
        {
            _isDateCompletedLessOrEqualToday = lessOrEqual;
            return this;
        }

        public string ToHtmlString()
        {
            var table = new TagBuilder("table");
            table.MergeAttribute("data-name", _name, true);

            if ((_attributes != null) && _attributes.Any())
            {
                foreach (var attribute in _attributes)
                {
                    table.MergeAttribute(attribute.Key, attribute.Value, true);
                }
            }

            var script = GetScript();

            var strBuilder = new StringBuilder();
            strBuilder.AppendLine(table.ToString());
            strBuilder.AppendLine(script.ToString());
            return strBuilder.ToString();
        }

        private TagBuilder GetScript()
        {
            if (string.IsNullOrWhiteSpace(_colItemNameText))
            {
                _colItemNameText = DEFAUL_COL_ITEM_NAME_KEY;
            }

            if (string.IsNullOrWhiteSpace(_colIsMandatoryText))
            {
                _colIsMandatoryText = DEFAUL_COL_MANDATORY_KEY;
            }

            if (string.IsNullOrWhiteSpace(_colIsCompletedText))
            {
                _colIsCompletedText = DEFAUL_COL_COMPLETED_KEY;
            }

            if (string.IsNullOrWhiteSpace(_colDateCompletesText))
            {
                _colDateCompletesText = DEFAUL_COL_DATE_COMPLETED_KEY;
            }

            if (string.IsNullOrWhiteSpace(_colUserText))
            {
                _colUserText = DEFAUL_COL_USER_KEY;
            }

            string colItemNameText = Localization.GetText(_colItemNameText);
            string colIsMandatoryText = Localization.GetText(_colIsMandatoryText);
            string colIsCompletedText = Localization.GetText(_colIsCompletedText);
            string colDateCompletesText = Localization.GetText(_colDateCompletesText);
            string colUserText = Localization.GetText(_colUserText);

            var items = JsonConvert.SerializeObject(_items, Formatting.None);
            var customErrorMessage = JsonConvert.SerializeObject(_validationMessages, Formatting.None);
            var strBuilder = new StringBuilder();
            var script = new TagBuilder("script");
            script.MergeAttribute("type", "text/javascript", true);

            strBuilder.AppendLine(string.Format("registerCallback(function () {{"));
            strBuilder.AppendLine(string.Format("    var target  = $('[data-name=\"{0}\"]');", _name));
            strBuilder.AppendLine(string.Format("    var options = {{}};"));
            strBuilder.AppendLine(string.Format("    options = $.extend(options, {{ Items: {0}}});", items));
            strBuilder.AppendLine(string.Format("    options = $.extend(options, {{ CssClass: '{0}'}});", _htmlClass));
            strBuilder.AppendLine(string.Format("    options = $.extend(options, {{ HeaderItemName: '{0}'}});", colItemNameText));
            strBuilder.AppendLine(string.Format("    options = $.extend(options, {{ HeaderMandatory: '{0}'}});", colIsMandatoryText));
            strBuilder.AppendLine(string.Format("    options = $.extend(options, {{ HeaderCompleted: '{0}'}});", colIsCompletedText));
            strBuilder.AppendLine(string.Format("    options = $.extend(options, {{ HeaderDateCompleted: '{0}'}});", colDateCompletesText));
            strBuilder.AppendLine(string.Format("    options = $.extend(options, {{ HeaderUser: '{0}'}});", colUserText));
            strBuilder.AppendLine(string.Format("    options = $.extend(options, {{ Editable: {0}}});", _isEditable.ToString().ToLower()));
            strBuilder.AppendLine(string.Format("    options = $.extend(options, {{ ValidateOnLoad: {0}}});", _validateOnLoad.ToString().ToLower()));
            strBuilder.AppendLine(string.Format("    options = $.extend(options, {{ CustomErrorMessage: {0}}});", customErrorMessage));
            strBuilder.AppendLine(string.Format("    options = $.extend(options, {{ ValidationMessageFormat: {0}}});", (int)_validationMessageFormat));
            strBuilder.AppendLine(string.Format("    options = $.extend(options, {{ PermitUncompleteAutoCompleted: {0}}});", _canUncompleteAutomatic.ToString().ToLower()));
            strBuilder.AppendLine(string.Format("    options = $.extend(options, {{ ScrollToError: {0}}});", _scrollToError.ToString().ToLower()));
            strBuilder.AppendLine(string.Format("    options = $.extend(options, {{ CurrentUser: '{0}'}});", _currentUser));
            strBuilder.AppendLine(string.Format("    options = $.extend(options, {{ DateCompletedLessOrEqualToday: {0}}});", _isDateCompletedLessOrEqualToday.ToString().ToLower()));
            strBuilder.AppendLine(string.Format("    target.checklist(options);"));
            strBuilder.AppendLine(string.Format("}});"));

            script.InnerHtml = strBuilder.ToString();

            return script;
        }
    }

    public enum ValidationMessageFormatEnum : int
    {
        None = 0,
        ChecklistItemRed
    }
}