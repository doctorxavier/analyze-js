using System.Collections.Generic;
using System.Web.Mvc;

using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Application.OPUSModule.Messages.OperationDataService;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Models.Modal;
using IDB.Architecture.Security.Models.UserIdentity;

namespace IDB.Presentation.MVC4.Areas.Styles.Controllers
{
    public partial class ViewController : BaseController
    {
        public virtual ActionResult Index()
        {
            return RedirectToAction("NewConfluence");
        }

        public virtual ActionResult Legacy()
        {
            DatosCarga(true);

            return View();
        }

        public virtual ActionResult NewConfluence()
        {
            DatosCarga();

            return View();
        }

        public virtual ActionResult Backdoor()
        {
            DatosCarga();

            return View();
        }

        public virtual ActionResult WorkingLegacy()
        {
            DatosCarga(true);

            return View();
        }

        public virtual ActionResult Working()
        {
            DatosCarga();

            return View();
        }

        public virtual ActionResult Rich()
        {
            DatosCarga();

            return View();
        }

        public void DatosCarga(bool isLegacy = false)
        {
            //Viewbag de carga inicial
            if (isLegacy)
            {
                ViewBag.Legacy = true;
            }
            else
            {
                ViewBag.Legacy = false;
            }

            // Listado de atributos de prueba
            var dictionary = new Dictionary<string, string>();
            dictionary.Add("nombre1", "valor1");
            ViewBag.Dictionary = dictionary;

            // Lista de datos del dropdownlis
            var data = new List<System.Web.Mvc.SelectListItem>();

            for (var i = 0; i < 100; i++)
            {
                data.Add(new System.Web.Mvc.SelectListItem()
                {
                    Text = "texto " + i,
                    Value = i.ToString()
                });
            }

            ViewBag.ListItem = data;

            // Lista de datos del dropdownlis
            var data2 = new List<System.Web.Mvc.SelectListItem>();
            for (var i = 0; i < 100; i++)
            {
                data2.Add(new System.Web.Mvc.SelectListItem()
                {
                    Text = "texto 2-" + i,
                    Value = i.ToString()
                });
            }

            ViewBag.ListItem2 = data2;

            var data3 = new List<MultiSelectListItem>();

            for (var i = 0; i < 10; i++)
            {
                data3.Add(new MultiSelectListItem()
                {
                    Text = "texto 3-" + i,
                    Value = i.ToString()
                });
            }

            ViewBag.MultiListItem = data3;

            ViewBag.MultiLenguageSimple = new MVCControls.Confluence.Models.MultiLanguageBoxSimpleModel
            {
                EsAreaName = "es",
                EsAreaValue = "es",
                EsRequired = true,
                EnAreaName = "en",
                EnAreaValue = "en",
                EnRequired = true,
                PtAreaName = "pt",
                PtAreaValue = "pt",
                PtRequired = true,
                FrAreaName = "fr",
                FrAreaValue = "fr"
            };

            ViewBag.MultiLenguage = new MVCControls.Confluence.Models.MultiLanguageBoxModel
            {
                EsTextName = "ES",
                EsTextValue = "ES",
                EsAreaName = "es",
                EsAreaValue = "es",
                EsRequired = true,

                EnTextName = "EN",
                EnTextValue = "EN",
                EnAreaName = "en",
                EnAreaValue = "en",
                EnRequired = true,

                PtTextName = "PT",
                PtTextValue = "PT",
                PtAreaName = "pt",
                PtAreaValue = "pt",
                PtRequired = true,

                FrTextName = "FR",
                FrTextValue = "FR",
                FrAreaName = "fr",
                FrAreaValue = "fr"
            };

            // Botones de ejemplo para modales
            var temButton = new List<ModalButtonsViewModel>();

            temButton.Add(new ModalButtonsViewModel()
            {
                Text = "btn1",
                Type = "buttonBlue",
                CloseAtEnd = true
            });
            temButton.Add(new ModalButtonsViewModel()
            {
                Text = "btn2",
                Type = "buttonBlue"
            });
            temButton.Add(new ModalButtonsViewModel()
            {
                Text = "btn3",
                Type = "buttonBlue"
            });

            ViewBag.ButtonModal = temButton;
        }

        // Datos que consulta el dropdownlis asincronico
        public virtual JsonResult DropDownList(string filter)
        {
            var response = new GetUserNameListResponse
            {
                ListResponse = new List<ListItemViewModel>()
            };

            for (var i = 0; i < 100; i++)
            {
                response.ListResponse.Add(new ListItemViewModel()
                {
                    Text = "texto " + i,
                    Value = i.ToString()
                });
            }

            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}
