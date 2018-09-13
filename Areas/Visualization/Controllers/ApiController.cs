using System.Linq;
using System.Web;
using System.Web.Mvc;

using IDB.Architecture;
using IDB.MW.Domain.Contracts.ModelRepositories.Global;
using IDB.MW.Domain.Contracts.ModelRepositories.Visualization;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Areas.Visualization.Business;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVCExtensions;
using IDB.MW.Application.Components;

namespace IDB.Presentation.MVC4.Areas.Visualization.Controllers
{
    public partial class ApiController : BaseController
    {
        /// <summary>
        /// Third party layers get.
        /// </summary>
        /// <returns>The complete list of existing layers</returns>
        public virtual ActionResult ThirdPartyLayersGet()
        {
            var repository = Globals.Resolve<IVisualizationRepository>();
            return Json(repository.ThirdPartyLayersGet(),
                JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Return the operations in which the user has permission to access and modify
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>List of the operations</returns>
        public virtual ActionResult VisualizationAllowedOperations(
            string username)
        {
            var respository = Globals.Resolve<IGlobalModelRepository>();
            var profiles = Globals
                .GetSetting("VisualizationMobileProfiles")
                .Split(',')
                .ToList();
            var response = new AllowedOperationsResponse();
            respository
                       .UserOperationAccessGet(username, profiles)
                       .ForEach(us => response.OperationList.Add(new AllowedOperationData()
                       {
                           OperatioNumber = us.Operation,
                           Profiles = us.Profiles,
                           OperationNameEs = us.NameEs,
                           OperationNameEn = us.NameEn,
                           OperationNameFr = us.NameFr,
                           OperationNamePt = us.NamePt,
                           OperationCountry = us.OperationCountry
                       }));
            return new XmlResult(response);
        }

        /// <summary>
        /// Save the visual output information
        /// </summary>
        /// <param name="vpv">The VPV (operation mapping) data</param>
        /// <param name="vovs">Array of all vov</param>
        /// <returns></returns>
        public virtual ActionResult VisualizationDataSave(
            VPVVModels vpv,
            VOVVModel[] vovs)
        {
            if (vpv != null)
            {
                var business = new VisualizationBusinessContext();
                vpv.LocationTypeId = MasterDefinitions.GetMaster("UNDEFINED").MasterId;
                business.VisualProjectId = vpv.VisualProjectId;
                business.Load(IDBContext.Current.Operation);
                business.ViewModel = vpv;
                if (vpv.VisualProjectMedia != null)
                {
                    foreach (var media in vpv.VisualProjectMedia)
                    {
                        //TODO: Get the media source from the sended name for each media
                        //TODO: Transform the sended media from base64 and send to sharepoint
                        //new SharepointProxy().AddMediaFile(Request.Files[0]);
                        //TODO: create the media phat based in the sharepoint url+operation+media directory+filename
                        //TODO: Set the URL from in the media model
                        //TODO: Set MediaTypeId depending of the media type sended
                    }
                }

                business.Execute("Visualization.VP.Save");

                //TODO: Update vpv location with the GeolocationWK
                //business.VisualizationRepository.VisualProjectVersionSetGeolocation(vpv.GeolocationWNT);
            }

            if (vovs != null)
            {
                foreach (var vov in vovs)
                {
                    if (vov.VisualOutputMedia != null)
                    {
                        foreach (var media in vov.VisualOutputMedia)
                        {
                            //TODO: Get the media source from the sended name for each media
                            //TODO: Transform the sended media from base64 and send to sharepoint
                            //new SharepointProxy().AddMediaFile(Request.Files[0]);
                            //TODO: create the media phat based in the sharepoint url+operation+media directory+filename
                            //TODO: Set the URL from in the media model
                            //TODO: Set MediaTypeId depending of the media type sended
                        }
                    }

                    vov.DeliveryStatusId = MasterDefinitions
                        .GetMaster(vov.DeliveryStatus, "VO_DELIVERY_STATUS").MasterId;
                }
            }

            return Content(string.Empty);
        }

        private void checkESGPermmision(IVisualizationRepository repository)
        {
            if (!repository.PermissionESGTeamMember(IDBContext.Current.UserName))
            {
                throw new HttpException(403, "Forbidden");
            }
        }
    }
}