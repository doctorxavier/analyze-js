using System;
using System.Linq;
using IDB.Architecture;
using IDB.MW.Domain.Contracts.ModelRepositories.Global;
using IDB.Presentation.MVC4.MVCCommon;
using System.Web;
using System.IO;
using IDB.Presentation.MVC4.Helpers;
using AutoMapper;
using IDB.MW.Domain.Entities;
using IDB.MW.Domain.Models.Architecture.Visualization;
using System.Collections.Generic;
using IDB.MW.Domain.Session;

namespace IDB.Presentation.MVC4.Areas.Visualization.Business {

	class Base64file : HttpPostedFileBase {
		Stream stream;
		string fileName;

		public Base64file(string base64Data, string fileName) {
			var bytes = Convert.FromBase64String(base64Data);
			stream = new MemoryStream(bytes);
			this.fileName = fileName;
		}

		public override int ContentLength {
			get { return (int)stream.Length; }
		}

		public override string FileName {
			get { return fileName; }
		}

		public override Stream InputStream {
			get { return stream; }
		}
	}

	public class MobileServices : IMobileServices {

		/// <summary>
		/// Get the allowed operations.
		/// </summary>
		/// <param name="username">The username.</param>
		/// <returns></returns>
		public AllowedOperationsResponse VisualizationAllowedOperations(string username) {
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
            return response;

		}

		/// <summary>
		/// Get the visual outputs matrix.
		/// </summary>
		/// <param name="operationNumber">The operation number.</param>
		/// <returns></returns>
		public VisualizationDataResponse VisualOutputsMatrixGet(string operationNumber) {
			IDBContext.Current.Operation = operationNumber;
            var business = new VisualizationBusinessContext();
			business.Load(operationNumber);
			business.Execute("Visualization.Grid.Load");
			var matrix = business.ResultsMatrixContext.Current;
			var vosId = business
					.VisualOutputs
					.Where(vo => vo.VisualOutputVersionsData.IsNotNull() &&
						vo.VisualOutputVersionsData.OutputYearPlan.IsNotNull())
					.Select(ou => ou.VisualOutputVersionsData.VisualOutputVersionId)
					.ToList();
			var vosGelocation = business
				.VisualizationRepository
				.VisualOutputsLocationDataGet(vosId);
			var outputs = matrix.Components.SelectMany(cp => cp.Outputs);


			foreach (var visualOutput in business.VisualOutputs) {
				var geoData = vosGelocation.FirstOrDefault(geo =>
					visualOutput.VisualOutputVersionsData.IsNotNull() &&
					geo.FK_GEO_OBJECT_ID == visualOutput.VisualOutputVersionsData.VisualOutputVersionId);
				if (geoData.IsNotNull()) {
					visualOutput.VisualOutputVersionsData.GeolocationWNT = geoData.SHAPE;
				}
			}


			foreach (var visualOutput in business.VisualOutputs) {
				visualOutput.VisualOutputVersions = null;
			}

			if (!business.VisualProjects.Any()) {
				business.Execute("Visualization.VP.Create");
			}
			if (business.VisualProjectVersionId != -1) {
				//Get vp geolocation data, only the first point
				var vPGelocation = business
				.VisualizationRepository
				.VisualProjectLocationDataGet(business.VisualProjectVersionId.Value);
				if (vPGelocation.Any()) {
					business.VisualProject.VisualProjectVersionsData.GeolocationWNT =
						vPGelocation.First().SHAPE;
				}
			}
			matrix.OtherCosts = null;

			var components = new List<ComponentsResponse>();

			foreach (var component in matrix.Components) {
				var newComponent = new ComponentsResponse() {
					Statement = component.Statement,
				};
				components.Add(newComponent);
				foreach (var output0 in component.Outputs) {
					var visualOutputs = business
						.VisualOutputs
						.Where(vo => vo.VisualOutputVersionsData.IsNotNull() &&
							vo.VisualOutputVersionsData.OutputYearPlan.IsNotNull() &&
							vo.VisualOutputVersionsData.OutputYearPlan.OutputId == output0.OutputId)
						.ToList();
					newComponent.Outputs.Add(new OutputResponse() {
						Definition = output0.Definition,
						OutputCategory = output0.OutputCategory,
						OutputYearPlans = output0.OutputYearPlans,
						VisualOutputs = visualOutputs
					});
				}
			}

			return new VisualizationDataResponse() {
				VisualProject = business.VisualProject,
				Components = components
			};
		}

	    /// <summary>
	    /// Save the visual data.
	    /// </summary>
	    /// <param name="operation">The operation.</param>
	    /// <param name="vpv">The VPV.</param>
	    /// <param name="vovs">The vovs.</param>
	    /// <param name="userName"></param>
	    public void VisualizationDataSave(
			string operation,
			VPVVModels vpv,
            VOVVModel[] vovs, string userName)
        {
			var imageTypes = new string[] { "png", "jpg", "jpeg", "gif", "bpm" };

			IDBContext.Current.Operation = operation;
			//check if operation mapping is empty before call to save
			if (vpv.IsNotNull()) {
                var business = new VisualizationBusinessContext();
				vpv.LocationTypeId = MasterDefinitions.GetMaster("UNDEFINED").MasterId;
				business.VisualProjectId = vpv.VisualProjectId;
				business.Load(operation);
				business.ViewModel = vpv;
				if (vpv.VisualProjectMedia.IsNotNull()) {
					foreach (var media in vpv.VisualProjectMedia) {
						//Transform the sended media from base64 and send to sharepoint
						media.MediaUrl = new SharepointProxy().AddMediaFile(
							new Base64file(media.MediaFile64, media.MediaUrl));

						//Get the media source from the sended name for each media
						media.MediaSourceId = MasterDefinitions
							.GetMaster("MEDIA_SOURCE", media.MediaSource).MasterId;
						
						//Set MediaTypeId depending of the media type sended
						var mime = ImageHelper.URLGetMime(media.MediaUrl);
						if (mime.StartsWith("image/")) {
							media.MediaTypeId = MasterDefinitions.GetMaster("MEDIA_TYPE", "IMAGE").MasterId;
						} else {
							media.MediaTypeId = MasterDefinitions.GetMaster("MEDIA_TYPE", "VIDEO").MasterId;
						}
					}
				}
				business.ViewModel = vpv;
				business.Execute("Visualization.VP.Save");
			}

			if (vovs.IsNotNull()) {
                var business = new VisualizationBusinessContext();
					
					business.ViewModel = vpv;
					foreach (var vov in vovs) {
						vov.LocationTypeId = MasterDefinitions.GetMaster("UNDEFINED").MasterId;
						if (vov.VisualOutputId == -1) {
							business.OutputId = vov.OutputId.Value;
						} else {
							business.VisualOutputId = vov.VisualOutputId;
							business.OutputId = business
								.VisualOutputGet(vov.VisualOutputId)
								.VisualOutputVersions
								.First()
								.OutputYearPlan
								.OutputId;
						}
						business.Load(IDBContext.Current.Operation);
						vov.LocationTypeId = MasterDefinitions.GetMasterByName("VO_LOCATION_TYPE", vov.LocationType).MasterId;
						vov.DeliveryStatusId = MasterDefinitions.GetMasterByName("VO_DELIVERY_STATUS", vov.DeliveryStatus).MasterId;
						business.ViewModel = vov;
						
						if (vov.VisualOutputMedia.IsNotNull()) {
							foreach (var media in vov.VisualOutputMedia) {
								//Transform the sended media from base64 and send to sharepoint
								media.MediaUrl = new SharepointProxy().AddMediaFile(
									new Base64file(media.MediaFile64, media.MediaUrl));

								//Get the media source from the sended name for each media
								media.MediaSourceId = MasterDefinitions
									.GetMaster("MEDIA_SOURCE", media.MediaSource).MasterId;
								//get the history status from given information
								media.MediaHistoryStatusId = MasterDefinitions.GetMasterByName(
									"VO_DELIVERY_STATUS", media.MediaHistoryStatus).MasterId;

								//Set MediaTypeId depending of the media type sended
								var mime = ImageHelper.URLGetMime(media.MediaUrl);
								if (mime.StartsWith("image/")) {
									media.MediaTypeId = MasterDefinitions.GetMaster("MEDIA_TYPE", "IMAGE").MasterId;
								} else {
									media.MediaTypeId = MasterDefinitions.GetMaster("MEDIA_TYPE", "VIDEO").MasterId;
								}
							}
						}
						business.Execute("Visualization.VO.Save");
					}
			}
		}

    }
}
