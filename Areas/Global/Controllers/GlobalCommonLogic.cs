using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web.Configuration;

using Newtonsoft.Json;

using IDB.Architecture;
using IDB.Architecture.Language;
using IDB.MW.Domain.Models.Global;
using IDB.MW.Domain.Models.Pagination;
using IDB.MW.Domain.Session;
using IDB.MW.Infrastructure.Data.Optima.Repositories.Models.Global;
using IDB.MW.Domain.Contracts.ModelRepositories.Global;

namespace IDB.Presentation.MVC4.Areas.Global.Controllers
{
    public class GlobalCommonLogic : IDisposable
    {
        public const string StrMachine = "Machine";
        public const string StrK2Server = "K2Server";
        public const string StrHostServerName = "HostServerName";
        public const string StrHostServerPort = "HostServerPort";
        public const string StrProcessId = "ProcessID";

        GlobalModelRepository globals_repository;
        SharePointModelRepositoryClient sharepoint_repository;
        SharePointModelRepositoryClient SharePointRepository
        {
            get
            {
                if (sharepoint_repository == null)
                {
                    sharepoint_repository = new SharePointModelRepositoryClient();
                }

                return sharepoint_repository;
            }
        }

        GlobalModelRepository GlobalsRepository
        {
            get
            {
                if (globals_repository == null)
                {
                    globals_repository = new GlobalModelRepository();
                }

                return globals_repository;
            }
        }

        public static string BuildNotification(
            string JSONnotification, string section, string language)
        {
            Dictionary<string, object> parameters =
                JsonConvert.DeserializeObject<Dictionary<string, object>>(JSONnotification);

            string notification = Localization.GetText(parameters["ID"].ToString() + "-" + section, language);

            if (notification == null)
            {
                return "Error: notification not found.<br/>";
            }

            foreach (var item in parameters)
            {
                notification = notification
                    .Replace("[" + item.Key + "]", item.Value.ToString());
            }

            return notification.Replace("\\n", "<br/>");
        }

        public static string GetK2WorkflowViewDetailURL()
        {
            var sb = new StringBuilder();
            Configuration rootWebConfig = WebConfigurationManager.OpenWebConfiguration("~/");

            if (rootWebConfig.AppSettings.Settings.Count > 0)
            {
                string k2Server = null;
                string hostServerName = null;
                string hostServerPort = null;

                KeyValueConfigurationElement strK2ServerValue = rootWebConfig.AppSettings.Settings[StrK2Server];
                KeyValueConfigurationElement strHostServerNameValue = rootWebConfig.AppSettings.Settings[StrHostServerName];
                KeyValueConfigurationElement strHostServerPortValue = rootWebConfig.AppSettings.Settings[StrHostServerPort];

                k2Server = strK2ServerValue != null ? strK2ServerValue.Value : string.Empty;
                hostServerName = strHostServerNameValue != null ? strHostServerNameValue.Value : string.Empty;
                hostServerPort = strHostServerPortValue != null ? strHostServerPortValue.Value : string.Empty;
                
                sb.Append("https://" + hostServerName);
                sb.Append("/ViewFlow/ViewFlow.aspx?ViewTypeName=ProcessView&");
                sb.Append(StrK2Server + "=" + k2Server + "&");
                sb.Append(StrHostServerName + "=" + hostServerName + "&");
                sb.Append(StrHostServerPort + "=" + hostServerPort + "&");
                sb.Append(StrProcessId + "=");
            }

            return sb.ToString();
        }

        public static string GetOperationDetailURL()
        {
            var url = Globals.GetSetting("BasePath");
            url = url + "/operation/";
            return url;
        }

        public static string GetOperationDraftDetailURL()
        {
            var url = Globals.GetSetting("BasePath");
            url = url + "/operation/Draft/Pages/default?idOperation=";
            return url;
        }

        public static void SetLastOperation(string operation)
        {
            var currentOperations = IDBContext
                .Current
                .GetClientPreference("last_viewed_operations")
                ?? string.Empty;

            var operations = currentOperations
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            var resultOperations = new List<string>();

            //add current operation first
            resultOperations.Add(operation);

            //add all other operations
            resultOperations.AddRange(operations.Where(op => op != operation));

            int operationsToShow = 4;
            IDBContext.Current.AddClientPreference("last_viewed_operations",
                string.Join(",", resultOperations.Take(operationsToShow)));
        }

        public OperationsViewModel GetMyOperations(DataSourceRequest request)
        {
            var model = new OperationsViewModel();

            //get current user id
            var current_user = IDBContext.Current.UserName;

            //get list of operations in wich the current user have access
            var user_operations = sharepoint_repository
                .GetUserOperations(current_user);

            var operation_parameter = string.Join(",", user_operations.ToArray());

            //get current user operations
            GlobalsRepository.GetOperations(operation_parameter, request);

            return model;
        }

        public void Dispose()
        {
            if (globals_repository != null)
                GlobalsRepository.Dispose();
        }

        internal List<OperationViewModel> GetOperationsLastViewed()
        {
            var lastViewed = IDBContext.Current.GetClientPreference("last_viewed_operations");
            var model = new List<OperationViewModel>();

            if (string.IsNullOrEmpty(lastViewed))
            {
                return model;
            }

            var request = new DataSourceRequest();

            //get last viewed operations
            var operations = GlobalsRepository.GetOperations(lastViewed, request);

            //Transform database elements in view elements
            operations.Data.ForEach(operation =>
                model.Add(new OperationViewModel()
                {
                    OperationName = operation.OperationName,
                    OperationNumber = operation.OperationNumber,
                    Loans = operation.Loans,
                    Code = operation.Code,
                    OperationUrl = operation.OperationUrl
                }));
            return model;
        }
    }
}