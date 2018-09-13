using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using IDB.MW.Domain.Models.Architecture.Visualization;

namespace IDB.Presentation.MVC4.Areas.Visualization.Business
{
    [DataContract(Namespace = "IDB.Optima.Api.Visualization")]
    public class VMVmodel
    {
        [DataMember]
        [Required]
        public int MediaId { get; set; }

        [DataMember]
        public string Content { get; set; }

        [DataMember]
        [Required]
        public int MediaSourceId { get; set; }

        [DataMember]
        [Required]
        public string Name { get; set; }

        [DataMember]
        [Required]
        public bool IsDeleted { get; set; }

        [DataMember]
        public int MediaTypeId { get; set; }

        [DataMember]
        public string MediaUrl { get; set; }

        [DataMember]
        [Required]
        public int Year { get; set; }

        [DataMember]
        [Required]
        public int MediaOrder { get; set; }

        [DataMember]
        public string MediaHistoryStatus { get; set; }

        [DataMember]
        public int? MediaHistoryStatusId { get; set; }

        [DataMember]
        public string[] DocumentId { get; set; }

        [DataMember]
        public string[] DocNumber { get; set; }

        [DataMember]
        public string[] DocumentName { get; set; }

        [DataMember]
        public string MediaSource { get; set; }

        [DataMember]
        public string MediaFile64 { get; set; }

        [DataMember]
        public string MediaTypeCode { get; set; }
    }

    [DataContract(Namespace = "IDB.Optima.Api.Visualization")]
    public class DocumentViewModel
    {
        [DataMember]
        public int DocumentId { get; set; }

        [DataMember]
        public string User { get; set; }

        [DataMember]
        public string Date { get; set; }

        [DataMember]
        public string DocNumber { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string IsDeleted { get; set; }
    }

    [DataContract(Namespace = "IDB.Optima.Api.Visualization")]
    public class VPVVModels
    {
        [DataMember]
        public int VisualProjectId { get; set; }

        [DataMember]
        public int VisualProjectVersionId { get; set; }

        [DataMember]
        [Required]
        public int LocationTypeId { get; set; }

        [DataMember]
        public VMVmodel[] VisualProjectMedia { get; set; }

        [DataMember]
        public DocumentViewModel[] VisualProjectDocument { get; set; }

        [DataMember]
        public string GeolocationWNT { get; set; }

        [DataMember]
        public int ValidationStageId { get; set; }

        [DataMember]
        public string ValidationStage { get; set; }

        [DataMember]
        public DateTime? Modified { get; set; }
    }

    [DataContract(Namespace = "IDB.Optima.Api.Visualization")]
    public class VOVVModel
    {
        [DataMember]
        public string NameEn { get; set; }

        [DataMember]
        public string NameEs { get; set; }

        [DataMember]
        public string NamePt { get; set; }

        [DataMember]
        public string NameFr { get; set; }

        [DataMember]
        public int DeliveryStatusId { get; set; }

        [DataMember]
        public string DeliveryStatus { get; set; }

        [DataMember]
        public int Year { get; set; }

        [DataMember]
        public int LocationTypeId { get; set; }

        [DataMember]
        public string LocationType { get; set; }

        [DataMember]
        public string Level1 { get; set; }

        [DataMember]
        public string Level2 { get; set; }

        [DataMember]
        public string Level3 { get; set; }

        [DataMember]
        public string DescriptionEn { get; set; }

        [DataMember]
        public string DescriptionEs { get; set; }

        [DataMember]
        public string DescriptionPt { get; set; }

        [DataMember]
        public string DescriptionFr { get; set; }

        [DataMember]
        public decimal OutputUnits { get; set; }

        [DataMember]
        public int VisualOutputId { get; set; }

        [DataMember]
        public int OutputYearPlanId { get; set; }

        [DataMember]
        public int VisualOutputVersionId { get; set; }

        [DataMember]
        public VMVmodel[] VisualOutputMedia { get; set; }

        [DataMember]
        public DocumentViewModel[] VisualOutputDocument { get; set; }

        [DataMember]
        public int ValidationStageId { get; set; }

        [DataMember]
        public int? OutputId { get; set; }

        [DataMember]
        public string GeolocationWNT { get; set; }

        [DataMember]
        public bool? IsCompleteToPublish { get; set; }

        [DataMember]
        public string ValidationStage { get; set; }

        [DataMember]
        public DateTime? Modified { get; set; }
    }
}