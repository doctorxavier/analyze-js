using System.Collections.Generic;
using System.Web.Mvc;

using IDB.MW.Domain.Contracts.ModelRepositories.ExecutorContact;
using IDB.Presentation.MVC4.Controllers;
using IDB.MW.Domain.Models.Architecture.Enumerations;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVCExtensions;
using IDB.MW.Domain.Models.ExecutorContactInstitutionModel;
using IDB.MW.Domain.Models.Institution;

namespace IDB.Presentation.MVC4.Areas.SignaturesAndContacts.Controllers
{
    public partial class ExecutorContactController : BaseController
    {
        private IExecutorContactRepository _executor;

        public ExecutorContactController(IExecutorContactRepository Executor)
        {
            _executor = Executor;
        }

        public virtual ActionResult Index(int actveStatus = 0, bool busqueda = false, string OperationNumber = "", string contractNumber = "", string institutionName = "", MessageNotificationCodes messageStatus = MessageNotificationCodes.VoidMessage, string OperationNumber2 = "")
        {
            if (busqueda)
            {
                OperationNumber = OperationNumber2;
                var ExecutorContactSearch = _executor.SearchWithParameters(actveStatus, OperationNumber.Trim(), contractNumber.Trim(), institutionName.Trim());
                int paginacion = int.Parse(System.Configuration.ConfigurationManager.AppSettings["pageSize"].ToString());
                if (messageStatus != MessageNotificationCodes.VoidMessage)
                {
                    // Set message in agreement with the code
                    MessageConfiguration message = MessageHandler.setMessage(messageStatus, true, 5);

                    // Pass message to the view
                    ViewData["message"] = message;
                }
                else
                {
                    if (ExecutorContactSearch.Count >= paginacion)
                    {
                        MessageNotificationCodes messageStatusMax = MessageNotificationCodes.SearchMaxValue;

                        // Set message in agreement with the code
                        MessageConfiguration message = MessageHandler.setMessage(messageStatusMax, true, 8);

                        // Pass message to the view
                        ViewData["message"] = message;
                    }
                }

                ViewBag.Panel = "none";
                return View(ExecutorContactSearch);
            }
            else
            {
                if (messageStatus != MessageNotificationCodes.VoidMessage)
                {
                    // Set message in agreement with the code
                    MessageConfiguration message = MessageHandler.setMessage(messageStatus, true, 5);

                    // Pass message to the view
                    ViewData["message"] = message;
                }

                ViewBag.Panel = "inline-block";
                return View();
            }
        }

        [HasPermission(Permissions = "Contact Write")]
        [HttpPost]
        public virtual ActionResult SaveExecutorContact(ExecutorContactInstitutionModel ExecutorContactEdit)
        {
            if (ExecutorContactEdit.ExecutorContactModel.EXECUTOR_CONTACT_ID == 0)
            {
                var saved = false;

                saved = _executor.AddExecutorContact(ExecutorContactEdit);
                if (saved == true)
                {
                    var messageStatus = MessageNotificationCodes.SaveDataSucessfully;
                    return RedirectToAction("Index", new { messageStatus = messageStatus, busqueda = true });
                }
                else
                {
                    var messageStatus = MessageNotificationCodes.SaveDataFail;
                    return RedirectToAction("Index", new { messageStatus = messageStatus });
                }
            }
            else
            {
                var saved = false;
                saved = _executor.SetContact(ExecutorContactEdit);
                if (saved == true)
                {
                    var messageStatus = MessageNotificationCodes.SaveDataSucessfully;
                    return RedirectToAction("Index", new { messageStatus = messageStatus, busqueda = true });
                }
                else
                {
                    var messageStatus = MessageNotificationCodes.SaveDataFail;
                    return RedirectToAction("Index", new { messageStatus = messageStatus });
                }
            }
        }

        public virtual ActionResult GetExecutorContact(int contactId)
        {
            var modelcontact = _executor.GetExecutorContact(contactId);
            return View(modelcontact);
        }

        [HasPermission(Permissions = "Contact Write")]
        public virtual ActionResult EditExecutorContact(int Contact_ID)
        {
            var modelContactEdit = _executor.GetExecutorContact(Contact_ID);
            ViewBag.institutionEdit = modelContactEdit.InstitutionModel.NM;
            return View(modelContactEdit);
        }

        [HasPermission(Permissions = "Contact Write")]
        public virtual ActionResult DeleteExecutorContact(int Contact_ID)
        {
            var saved = false;

            saved = _executor.DeleteExecutorContact(Contact_ID);
            if (saved == true)
            {
                var messageStatus = MessageNotificationCodes.DeleteDataSucessfully;
                return RedirectToAction("Index", new { messageStatus = messageStatus, busqueda = true });
            }
            else
            {
                var messageStatus = MessageNotificationCodes.DeleteDataFail;
                return RedirectToAction("Index", new { messageStatus = messageStatus });
            }
        }

        [HasPermission(Permissions = "Contact Write")]
        public virtual ActionResult AddExecutorContact()
        {
            return View();
        }

        public virtual JsonResult GetInstitution(string institution = "")
        {
            List<IntitutionModel> lsInsitution = new List<IntitutionModel>();
            lsInsitution = _executor.GetInstitution(institution);
            var jsonResult = Json(lsInsitution, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
    }
}
