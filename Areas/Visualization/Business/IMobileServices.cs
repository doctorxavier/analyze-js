using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace IDB.Presentation.MVC4.Areas.Visualization.Business {

	/// <summary>
	/// Data service definition
	/// </summary>
	[ServiceContract(Namespace = "IDB.Optima.Api.Visualization")]
	public interface IMobileServices {
		/// <summary>
		/// Get the allowed operations.
		/// </summary>
		/// <param name="username">The username.</param>
		/// <returns></returns>
		[OperationContract]
		AllowedOperationsResponse VisualizationAllowedOperations(string username);

		/// <summary>
		/// Get the visual outputs matrix.
		/// </summary>
		/// <param name="operationNumber">The operation number.</param>
		/// <returns></returns>
		[OperationContract]
		VisualizationDataResponse VisualOutputsMatrixGet(string operationNumber);

	    /// <summary>
	    /// Save the visual data.
	    /// </summary>
	    /// <param name="operation">The operation.</param>
	    /// <param name="vpv">The VPV.</param>
	    /// <param name="vovs">The vovs.</param>
	    /// <param name="userName"></param>
	    [OperationContract]
		void VisualizationDataSave(string operation, VPVVModels vpv, VOVVModel[] vovs, string userName);
	}
}
