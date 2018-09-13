using Castle.Core;
using IDB.Domain.Contracts.Aspects;
using IDB.Domain.Attributes;

using System;
using System.Web.Mvc;
using System.Collections.Generic;
using IDB.MW.Domain.Models.Visualization;
using IDB.MW.Domain.Models.Architecture.Visualization;
using IDB.Presentation.MVC4.Models;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.ResultMatrix.Impacts;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.Visualization;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.ResultMatrix.Outputs;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.MasterDefinitions;
using System.Linq;
using IDB.MW.Domain.Contracts.ModelRepositories.Visualization;
using IDB.MW.Domain.Models.GeoLocationVisualization;




namespace IDB.Presentation.MVC4.Areas.Visualization.Controllers
{

    public partial class GeolocationController : Controller
    {
        #region WFC Services
        private IDB.MW.Domain.Contracts.ModelRepositories.Visualization.IVisualizationModelRepository _ServiceGeolocation = null;
        public IDB.MW.Domain.Contracts.ModelRepositories.Visualization.IVisualizationModelRepository ServiceGeolocation
        {
            get { return _ServiceGeolocation; }
            set { _ServiceGeolocation = value; }
        }
        #endregion


        #region Obtiene el pais enviando un VisualOutputId
        [HttpGet]
        public virtual ActionResult GetDescriptionCountryResponse(int VisualOutputId)
        {
            var Geolocation = ServiceGeolocation.GetDescriptionCountry(VisualOutputId);
            if (Geolocation == null)
                return Json("{}", JsonRequestBehavior.AllowGet);
            else
                return Json(Geolocation, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Obtiene el pais enviando un VisualProjectId
        [HttpGet]
        public virtual ActionResult GetDescriptionCountryVisualProjectResponse(int VisualProjectId)
        {
            var Geolocation = ServiceGeolocation.GetDescriptionCountryVisualProject(VisualProjectId);
            if (Geolocation == null)
                return Json("{}", JsonRequestBehavior.AllowGet);
            else
                return Json(Geolocation, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Actualiza un VisualOutputVersion 
        [HttpGet]
        public virtual ActionResult UpdateVisualOutputVersionResponse(string Level1, string Level2, string Level3, int VisualOutputId)
        {
            var Geolocation = ServiceGeolocation.UpdateVisualOutputVersion(Level1, Level2, Level3, VisualOutputId);
            if (Geolocation == 0)
                return Json("{}", JsonRequestBehavior.AllowGet);
            else
                return Json(Geolocation, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Actualiza un VisualProjectVersion
        [HttpGet]
        public virtual ActionResult UpdateVisualProjectVersionResponse(string Level1, string Level2, string Level3, int VisualProjectId)
        {
            var Geolocation = ServiceGeolocation.UpdateVisualProjectVersion(Level1, Level2, Level3, VisualProjectId);
            if (Geolocation == 0)
                return Json("{}", JsonRequestBehavior.AllowGet);
            else
                return Json(Geolocation, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Obtiene un VisualOutputVersion dependiendo del campo IS_VALIDATED
        [HttpGet]
        public virtual JsonResult GetVisualOutputVersionResponse(int VisualOutputId, bool IsValidate)
        {
            var Geolocation = ServiceGeolocation.GetVisualOutputVersion(VisualOutputId, IsValidate);
            if (Geolocation == null)
                return Json("{}", JsonRequestBehavior.AllowGet);
            else
                return Json(Geolocation, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Obtiene un VisualProjectVersion dependiendo del campo IS_VALIDATED
        [HttpGet]
        public virtual JsonResult GetVisualProjectVersionResponse(int VisualProjectId, bool IsValidate)
        {
            var Geolocation = ServiceGeolocation.GetVisualProjectVersion(VisualProjectId, IsValidate);
            if(Geolocation==null)
                return Json("{}", JsonRequestBehavior.AllowGet);
            else
                return Json(Geolocation, JsonRequestBehavior.AllowGet);
        }
        #endregion
        
        [HttpPost]
        public virtual ActionResult UpdateObjectLocationResponse(GeoLocationObject objectJson)
        {
            string Geolocation="";
            
            if(objectJson.objectType=="")
                Geolocation = "ObjectType can not be null";
            else
                Geolocation = ServiceGeolocation.UpdateObjectLocation(objectJson);

            return Json(Geolocation, JsonRequestBehavior.AllowGet);
        }

    }

}

        
