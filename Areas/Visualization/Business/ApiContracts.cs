using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using IDB.MW.Domain.Models.Architecture.ResultMatrix.Outputs;

namespace IDB.Presentation.MVC4.Areas.Visualization.Business
{
    [DataContract(Namespace = "IDB.Optima.Api.Visualization")]
    public class AllowedOperationData
    {
        [DataMember]
        public string OperatioNumber { get; set; }

        [DataMember]
        public List<string> Profiles { get; set; }

        [DataMember]
        public string OperationNameEs { get; set; }

        [DataMember]
        public string OperationNameEn { get; set; }

        [DataMember]
        public string OperationNameFr { get; set; }

        [DataMember]
        public string OperationNamePt { get; set; }

        [DataMember]
        public string OperationCountry { get; set; }
    }

    [DataContract(Namespace = "IDB.Optima.Api.Visualization")]
    public class AllowedOperationsResponse
    {
        /// <summary>
        /// Gets or sets the operation list.
        /// </summary>
        /// <value>
        /// The operation list.
        /// </value>
        [DataMember]
        [XmlArray("Operations"), XmlArrayItem(typeof(string), ElementName = "Operation")]
        public List<AllowedOperationData> OperationList { get; set; }

        public AllowedOperationsResponse()
        {
            OperationList = new List<AllowedOperationData>();
        }
    }

    [DataContract(Namespace = "IDB.Optima.Api.Visualization")]
    public class VisualizationDataResponse
    {
        /// <summary>
        /// Gets or sets the visual project.
        /// </summary>
        [DataMember]
        public IDB.MW.Domain.Models.Architecture.Visualization.VisualProjectModel VisualProject { get; set; }

        /// <summary>
        /// Gets or sets the components response.
        /// </summary>
        [DataMember]
        public List<ComponentsResponse> Components { get; set; }

        /// <summary>
        /// Operation data
        /// </summary>
        [DataMember]
        public MW.Domain.Models.Operation.OperationDataModel OperationData { get; set; }
    }

    [DataContract(Namespace = "IDB.Optima.Api.Visualization")]
    public class ComponentsResponse
    {
        public ComponentsResponse()
        {
            Outputs = new List<OutputResponse>();
        }

        [DataMember]
        public string Statement { get; set; }

        [DataMember]
        public int ComponentId { get; set; }

        [DataMember]
        public virtual List<OutputResponse> Outputs { get; set; }
    }

    [DataContract(Namespace = "IDB.Optima.Api.Visualization")]
    public class OutputResponse
    {
        public OutputResponse()
        {
        }

        [DataMember]
        public string Definition { get; set; }

        [DataMember]
        public List<IDB.MW.Domain.Models.Architecture.ResultMatrix.Outputs.OutputYearPlanModel> OutputYearPlans { get; set; }

        [DataMember]
        public IDB.MW.Domain.Models.Architecture.ResultMatrix.Outputs.OutputCategoryModel OutputCategory { get; set; }

        [DataMember]
        public List<IDB.MW.Domain.Models.Architecture.Visualization.VisualOutputModel> VisualOutputs { get; set; }
    }

    [DataContract(Namespace = "IDB.Optima.Api.Visualization")]
    public class SaveViewModelResponse
    {
        [DataMember]
        public VPVVModels Vpv { get; set; }

        [DataMember]
        public VOVVModel[] Vovs { get; set; }        
    }
}