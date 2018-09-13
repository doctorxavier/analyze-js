using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;

using IDB.Architecture;
using IDB.Architecture.Extensions;
using IDB.MW.Business.Core.MediaGallery.Contracts;
using IDB.MW.Domain.Contracts.Integration.SharePoint.SharePointCopyService;
using IDB.MW.Domain.Contracts.Integration.SharePoint.SharePointListService;
using IDB.MW.Domain.Contracts.ModelRepositories.Visualization;
using IDB.MW.Domain.Models.Architecture.Visualization;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Application.Components;
using IDB.Architecture.Security;

namespace IDB.Presentation.MVC4.Areas.Visualization.Business
{
    public class SharepointProxy
    {
        public List<MediaModel> MediaSearch(string name,
            DateTime? dateFrom,
            DateTime? dateTo)
        {
            var result = new List<MediaModel>();
            XElement listItems;
            var queryXML = new StringBuilder();
            queryXML.Append("<Query><Where>");

            if (!string.IsNullOrEmpty(name))
            {
                queryXML.Append("<Contains><FieldRef Name='Title'/><Value Type='Text'>");
                queryXML.Append(name);
                queryXML.Append("</Value></Contains>");
            }

            if (dateFrom.HasValue)
            {
                queryXML.Append("<Geq><FieldRef Name='ImageCreateDate'/><Value Type='DateTime'>");
                queryXML.Append(dateFrom.ToString());
                queryXML.Append("</Value></Geq>");
            }

            if (dateTo.HasValue)
            {
                queryXML.Append("<Lt><FieldRef Name='ImageCreateDate'/><Value Type='DateTime'>");
                queryXML.Append(dateTo.ToString());
                queryXML.Append("</Value></Lt>");
            }

            queryXML.Append("</Where></Query>");

            var servicePath = string.Format("{0}/_vti_bin/lists.asmx",
               IntegrationHelper.SPOperationURL);
            var mediaPath = Globals.GetSetting("SPMediaPath");

            string viewFieldsXml =
                    @"<ViewFields>
						<FieldRef Name='Title'/>
						<FieldRef Name='DocIcon'/>
						<FieldRef Name='EncodedAbsUrl'/>
						<FieldRef Name='CreatedDate'/>
					</ViewFields>";

            string queryOptionsXml =
                "<QueryOptions>" +
                    "<IncludeMandatoryColumns>TRUE</IncludeMandatoryColumns>" +
                    "<DateInUtc>TRUE</DateInUtc>" +
                "</QueryOptions>";
            var endPoint = new EndpointAddress(servicePath);
            var binding = GetSharepointBinding();

            string userWebConf = ConfigurationManager.AppSettings["CyberArk:SPServiceAccount_login"];
            string user = userWebConf.Split('|')[0];
            string password = PasswordStorage.GetPasswordStorage().GetPassword(userWebConf);

            using (var factory = new ChannelFactory<ListsSoap>(binding, endPoint))
            {
                using (var proxy = new ListsSoapClient(binding, endPoint))
                {
                    if (proxy.ClientCredentials != null)
                    {
                        proxy.ClientCredentials.Windows.ClientCredential = CredentialCache.DefaultNetworkCredentials;
                        proxy.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;
                        proxy.Endpoint.Address = endPoint;
                        proxy.ClientCredentials.UserName.UserName = user;
                        proxy.ClientCredentials.UserName.Password = password;
                    }

                    listItems = proxy.GetListItems(
                        mediaPath,
                        null,
                        XElement.Parse(queryXML.ToString()),
                        XElement.Parse(viewFieldsXml),
                        null,
                        XElement.Parse(queryOptionsXml),
                        null);
                }
            }

            var responseDocument = new XmlDocument();
            responseDocument.LoadXml(listItems.ToString());
            XmlNodeList elementList = responseDocument.GetElementsByTagName("z:row");

            foreach (XmlNode element in elementList)
            {
                result.Add(new MediaModel()
                {
                    MediaId = -1,
                    Name = GetXMLStringValue(element.Attributes["ows_Title"]),
                    Content = GetXMLStringValue(element.Attributes["ows__Comments"]),
                    Year = GetXMLIntValue(element.Attributes["ows_Created_x0020_Date"]),
                    Location = GetXMLStringValue(element.Attributes["ows_Keywords"]),
                    MediaUrl = GetXMLStringValue(element.Attributes["ows_EncodedAbsUrl"]),
                });
            }

            return result;
        }

        public List<MediaModel> VisualizationMediaSearch(
            string name,
            DateTime? dateFrom,
            DateTime? dateTo)
        {
            var visualizationRepository = Globals.Resolve<IVisualizationRepository>();
            var result = new List<MediaModel>();
            var spMedias = MediaSearch(name, dateFrom, dateTo);
            var dbMedias = visualizationRepository
                .MediaGet(IDBContext.Current.Operation, name, dateFrom, dateTo, true);

            foreach (var media in spMedias)
            {
                var mediaUrl = media.MediaUrl.ToLower();
                if (!dbMedias.Any(dbm => dbm.MediaUrl.ToLower() == mediaUrl) &&
                    !string.IsNullOrEmpty(mediaUrl))
                {
                    var mime = ImageHelper.URLGetMime(mediaUrl);
                    if (mime.StartsWith("image/"))
                    {
                        media.MediaTypeId = MasterDefinitions.GetMaster("MEDIA_TYPE", "IMAGE").MasterId;
                    }
                    else
                    {
                        media.MediaTypeId = MasterDefinitions.GetMaster("MEDIA_TYPE", "VIDEO").MasterId;
                    }

                    media.Name = mediaUrl.SubstringFromLast('/');
                    result.Add(media);
                }
            }

            return result;
        }

        public string AddMediaFile(HttpPostedFileBase file)
        {
            var imageTypes = new string[] { "png", "jpg", "jpeg", "gif", "bpm" };
            var videoTypes = new string[] { "avi", "wmf", "mp4", "flv" };
            byte[] fileBytes;
            var fileExtension = Path.GetExtension(file.FileName)
               .ToLower()
               .Trim('.');

            if (!videoTypes.Any(vt => vt == fileExtension) &&
               !imageTypes.Any(it => it == fileExtension))
            {
                throw new Exception("Invalid file type");
            }

            if (file.ContentLength > 0xf00000)
            {
                IDBContext.Current.ErrorMessage("Max allowed size is 15 MB");
            }

            var isImage = imageTypes.Any(it => it == fileExtension);
            if (isImage)
            {
                ImageHelper.ResizeImage(file.InputStream, out fileBytes, fileExtension);
            }
            else
            {
                fileBytes = new byte[file.InputStream.Length];
                file.InputStream.Read(fileBytes, 0, (int)file.InputStream.Length);
            }

            var business = new VisualizationBusinessContext();

            try
            {
                var fileName = file.FileName;

                var spUrl = string.Format("{0}/{1}/{2}",
                   IntegrationHelper.SPOperationURL,
                   Globals.GetSetting("SPMediaPath"),
                   fileName);
                var servicePath = string.Format("{0}/_vti_bin/copy.asmx",
                   IntegrationHelper.SPOperationURL);
                var endPoint = new EndpointAddress(servicePath);
                var binding = GetSharepointBinding();

                string userWebConf = ConfigurationManager.AppSettings["CyberArk:SPServiceAccount_login"];
                string user = userWebConf.Split('|')[0];
                string password = PasswordStorage.GetPasswordStorage().GetPassword(userWebConf);

                using (var factory = new ChannelFactory<ListsSoap>(binding, endPoint))
                {
                    using (var proxy = new CopySoapClient(binding, endPoint))
                    {
                        proxy.Endpoint.Address = new EndpointAddress(servicePath);
                        proxy.ClientCredentials.Windows.ClientCredential = CredentialCache.DefaultNetworkCredentials;
                        proxy.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;
                        proxy.ClientCredentials.UserName.UserName = user;
                        proxy.ClientCredentials.UserName.Password = password;
                        proxy.Open();
                        FieldInformation myFieldInfo = new FieldInformation()
                        {
                            DisplayName = string.Empty,
                            InternalName = string.Empty,
                            Type = FieldType.Text,
                            Value = fileName
                        };
                        CopyResult[] result;

                        proxy.CopyIntoItems(spUrl,
                           new string[] { spUrl },
                           new FieldInformation[] { myFieldInfo },
                           fileBytes,
                           out result);
                        
                        if (result == null || result.Length == 0)
                        {
                            throw new Exception("Error calling copy.asmx service");
                        }

                        if (result[0].ErrorCode != CopyErrorCode.Success)
                        {
                            throw new Exception(string.Format("ErrorCode:{0}. ErrorMessage: {1}",
                                result[0].ErrorCode, 
                                result[0].ErrorMessage));
                        }
                    }
                }

                return spUrl;
            }
            catch (Exception ex)
            {
                Architecture.Logging.Logger.GetLogger()
                    .WriteError(GetType().Name, ex.Message, ex);
                throw;
            }
        }

        public string AddMediaFile(HttpPostedFileBase file, string operationNumber, string fileName)
        {
            var _mediaGalleryService = Globals.Resolve<IMediaGalleryService>();
            var spUrl = _mediaGalleryService.AddMediaFile(operationNumber, fileName, file.InputStream);
            return spUrl;
        }

        private Binding GetSharepointBinding()
        {
            var binding = new BasicHttpBinding();
            binding.ReceiveTimeout =
                binding.CloseTimeout =
                binding.SendTimeout =
                new TimeSpan(0, 10, 0);
            binding.MaxBufferSize = int.MaxValue;
            binding.MaxBufferPoolSize = (long)int.MaxValue;
            binding.MaxReceivedMessageSize = int.MaxValue;
            binding.MessageEncoding = WSMessageEncoding.Text;

            binding.TransferMode = TransferMode.Buffered;
            binding.ReaderQuotas.MaxDepth = 32;
            binding.ReaderQuotas.MaxStringContentLength = 8192;
            binding.ReaderQuotas.MaxArrayLength = 16384;
            binding.ReaderQuotas.MaxBytesPerRead = 4096;
            binding.ReaderQuotas.MaxNameTableCharCount = 16384;
            binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;

            return binding;
        }

        private string GetXMLStringValue(XmlAttribute attribute, string defaultValue = "")
        {
            return attribute == null ? defaultValue : attribute.Value;
        }

        private int? GetXMLIntValue(XmlAttribute attribute)
        {
            return attribute == null ? default(int?) :
                DateTime.Parse(attribute.Value.SubstringFrom('#')).Year;
        }
    }
}