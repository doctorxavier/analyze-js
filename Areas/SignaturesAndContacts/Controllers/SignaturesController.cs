using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using IDB.Domain.Attributes;
using IDB.MW.Business.DocumentManagement.Contracts;
using IDB.MW.Business.DocumentManagement.Messages;
using IDB.MW.Domain.Contracts.DomainServices;
using IDB.MW.Domain.Contracts.ModelRepositories.Signature;
using IDB.MW.Domain.Models.Architecture.Enumerations;
using IDB.MW.Domain.Models.Signature;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVCExtensions;
using IDB.MW.Business.DocumentManagement.Enums;
using IDB.Architecture.Security;
using IDB.Architecture.Security.Messages.UserIdentity;

namespace IDB.Presentation.MVC4.Areas.SignaturesAndContacts.Controllers
{
    public partial class SignaturesController : BaseController
    {
        #region wcf services repositories
        public ISignatureModelRepository _signatures = null;

        private CommonDocument DoccumentObject = new CommonDocument();
        public virtual ISignatureModelRepository Signatures
        {
            get { return _signatures; }
            set { _signatures = value; }
        }

        private IDocumentManagementService _documentManagementService;

        public virtual IDocumentManagementService DocumentManagementService
        {
            get { return _documentManagementService; }
            set { _documentManagementService = value; }
        }

        #endregion

        public static byte[] Combine(params byte[][] arrays)
        {
            var document = new byte[arrays.Sum(a => a.Length)];
            var offset = 0;
            foreach (var array in arrays)
            {
                Buffer.BlockCopy(array, 0, document, offset, array.Length);
                offset += array.Length;
            }

            return document;
        }

        public virtual ActionResult IndexSignatures(string username, MessageNotificationCodes messageStatus = MessageNotificationCodes.VoidMessage, string page = "")
        {
            var modelSignatures = Signatures.GetSignatures(username, page);

            if (messageStatus != MessageNotificationCodes.VoidMessage)
            {
                MessageConfiguration message = MessageHandler.setMessage(messageStatus, true, 5);
                ViewData["message"] = message;
            }

            return View(modelSignatures);
        }

        [ExceptionHandling]
        [HasPermission(Permissions = "Signature Write")]
        public virtual ActionResult IndexEditSignatures(string username)
        {
            var modelSignatures = Signatures.GetSignaturesByUsername(username);

            var request = new DownloadRequest()
            {
                DocumentNumber = modelSignatures.DocumentReference
            };
            var getDocument = _documentManagementService.Download(request);
            if (getDocument.IsValid)
            {
                string message = getDocument.Document.FileName + " - " + getDocument.Document.Size + " - " + getDocument.Document.File;
                byte[] byteData = Combine(getDocument.Document.File);

                modelSignatures.image = byteData;
            }

            return View(modelSignatures);
        }

        [HttpPost]
        [HasPermission(Permissions = "Signature Write")]
        public virtual ActionResult IndexUpdateSignaturesUser(SignatureModel signature, HttpPostedFileBase file)
        {
            var messageStatus = MessageNotificationCodes.SaveDataSucessfully;

            bool updated = false;

            var filename = Path.GetFileName(file.FileName);
            var criteria = DoccumentObject.Criteria(DoccumentObject.SIGNATURE, filename, string.Empty, string.Empty, string.Empty);
            string trusteeList = DoccumentObject.TrusteeList(DoccumentObject.SIGNATURE);
            byte[] fileStream = new byte[file.ContentLength];
            file.InputStream.Read(fileStream, 0, fileStream.Length);

            try
            {
              var request = new UploadDocumentRequest
                {
                    AccessInformation = AccessInformationCategoryEnum.CONFIDENTIAL,
                    FileName = file.FileName,
                    FileStream = fileStream,
                    OperationNumber = IDBContext.Current.Operation,
                    BusinessAreaCode = BusinessAreaCodeEnum.BA_SIGNATURES,
                    TrusteeList = trusteeList
                };

                var createDocument = _documentManagementService.Upload(request);
                if (createDocument.IsValid)
                {
                    signature.DocumentReference = createDocument.DocumentNumber;
                    updated = Signatures.UpdateSignatureByUserSignatureId(signature);

                    if (!updated)
                        messageStatus = MessageNotificationCodes.SaveDataFail;
                }
                else
                {
                    messageStatus = MessageNotificationCodes.SaveDataFail;
                }

                return RedirectToAction("IndexSignatures", new { messageStatus = messageStatus });
            }
            catch (Exception)
            {
                messageStatus = MessageNotificationCodes.SaveDataFail;
            }

            return RedirectToAction("IndexSignatures", new { messageStatus = messageStatus });
        }

        public virtual ActionResult IndexViewSignatures(string username)
        {
            var modelSignatures = Signatures.GetSignaturesByUsername(username);

            DownloadRequest request = new DownloadRequest()
            {
                DocumentNumber = modelSignatures.DocumentReference
            };
            var getDocument = _documentManagementService.Download(request);
            if (getDocument.IsValid)
            {
                string message = getDocument.Document.FileName + " - " + getDocument.Document.Size + " - " + getDocument.Document.File;
                byte[] byteData = Combine(getDocument.Document.File);
                modelSignatures.image = byteData;
            }

            return View(modelSignatures);
        }

        [HasPermission(Permissions = "Signature Write")]
        public virtual ActionResult IndexDeleteSiganture()
        {
            return View();
        }

        [HasPermission(Permissions = "Signature Write")]
        public virtual ActionResult DeleteSignatureByUser(SignatureModel signature)
        {
            var messageStatus = MessageNotificationCodes.DeleteDataSucessfully;
            bool deleted = false;

            try
            {
                deleted = Signatures.DeleteSignatureByUserSignatureId(signature.UserSignatureId);

                if (!deleted)
                    messageStatus = MessageNotificationCodes.DeleteDataFail;

                return RedirectToAction("IndexSignatures", new { messageStatus = messageStatus });
            }
            catch (Exception)
            {
                messageStatus = MessageNotificationCodes.DeleteDataFail;
            }

            return RedirectToAction("IndexSignatures", new { messageStatus = messageStatus });
        }

        [HasPermission(Permissions = "Signature Write")]
        public virtual ActionResult IndexCreationSignatures(SignatureModel signature, MessageNotificationCodes messageStatus = MessageNotificationCodes.VoidMessage)
        {
            if (messageStatus != MessageNotificationCodes.VoidMessage)
            {
                MessageConfiguration message = MessageHandler.setMessage(messageStatus, true, 5);
                ViewData["message"] = message;
            }

            return View();
        }

        [HttpPost]
        [HasPermission(Permissions = "Signature Write")]
        public virtual ActionResult IndexFileSignatures(SignatureModel signature, HttpPostedFileBase file, string page = "")
        {
            var messageStatus = MessageNotificationCodes.SaveDataSucessfully;
            bool saved = false;

            var filename = Path.GetFileName(file.FileName);
            var criteria = DoccumentObject.Criteria(DoccumentObject.SIGNATURE, filename, string.Empty, string.Empty, string.Empty);
            string trusteeList = DoccumentObject.TrusteeList(DoccumentObject.SIGNATURE);
            byte[] fileStream = new byte[file.ContentLength];
            file.InputStream.Read(fileStream, 0, fileStream.Length);

            try
            {
                var searchSignatures = Signatures.GetSignatures(signature.UserId, page);

                if (searchSignatures.Count <= 0)
                {
                    var request = new UploadDocumentRequest
                    {
                        AccessInformation = AccessInformationCategoryEnum.CONFIDENTIAL,
                        FileName = file.FileName,
                        FileStream = fileStream,
                        OperationNumber = IDBContext.Current.Operation,
                        BusinessAreaCode = BusinessAreaCodeEnum.BA_SIGNATURES,
                        TrusteeList = trusteeList
                    };
                    var createDocument = _documentManagementService.Upload(request);

                    if (createDocument.IsValid)
                    {
                        signature.DocumentReference = createDocument.DocumentNumber;

                        saved = Signatures.SaveSignatureByUser(signature);

                        if (!saved)
                            messageStatus = MessageNotificationCodes.SaveDataFail;
                    }
                    else
                    {
                        messageStatus = MessageNotificationCodes.SaveDataFail;
                    }

                    return RedirectToAction("IndexSignatures", new { messageStatus = messageStatus });
                }
                else
                {
                    messageStatus = MessageNotificationCodes.SignatureCreatedPreviously;

                    return RedirectToAction("IndexCreationSignatures", new { messageStatus = messageStatus });
                }
            }
            catch (Exception)
            {
                messageStatus = MessageNotificationCodes.SaveDataFail;
            }

            return RedirectToAction("IndexSignatures", new { messageStatus = messageStatus });
        }

        public virtual FileResult GetDocument(string docNum)
        {
            try
            {
                var request = new DownloadRequest() { DocumentNumber = docNum };

                var getDocument = _documentManagementService.Download(request);

                if (getDocument.IsValid)
                {
                    var contentType = DoccumentObject.GetContentType(getDocument.Document.FileName);
                    return File(getDocument.Document.File, contentType, getDocument.Document.FileName);
                }
                else
                {
                    var byteData = Convert.FromBase64String("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");
                    return File(byteData, "Image/png", "Not Available");
                }
            }
            catch
            {
                byte[] byteData = Convert.FromBase64String("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");
                return File(byteData, "Image/png", "Not Available");
            }
        }

        public virtual JsonResult IndexGetUsers(string user)
        {
            List<SignatureUser> lstUser = new List<SignatureUser>();
            var Users_ = UserIdentityManager
                .SearchUsersByNameOrFullName(new GetUserByPCmailOrNameRequest { Search = user });

            int Index_ = 1;
            foreach (var User in Users_.Users)
            {
                if (!string.IsNullOrEmpty(User.FullName))
                {
                    lstUser.Add(new SignatureUser() { IdUser = Index_.ToString(), Nombre = User.FullName });
                }
            }

            return Json(lstUser, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult GetImageSignature(string document)
        {
            string strImage = string.Empty;

            var request = new DownloadRequest()
            {
                DocumentNumber = document
            };

            var getDocument = _documentManagementService.Download(request);
            byte[] byteData = new byte[0];
            if (getDocument.IsValid)
            {
                string message = getDocument.Document.FileName + " - " + getDocument.Document.Size + " - " + getDocument.Document.File;
                byteData = Combine(getDocument.Document.File);
                strImage = Convert.ToBase64String(byteData);
            }
            else
            {
                strImage = "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";
            }

            return Json(strImage, JsonRequestBehavior.AllowGet);
        }

        public class SignatureUser
        {
            public string IdUser { get; set; }
            public string Nombre { get; set; }
        }
    }
}
