//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace XHTD_SERVICES.Data.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class ScaleBill
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ScaleBill()
        {
            this.ScaleImages = new HashSet<ScaleImage>();
        }
    
        public string Code { get; set; }
        public string OrderCode { get; set; }
        public string ScaleTypeCode { get; set; }
        public string PartnerCode { get; set; }
        public string VehicleCode { get; set; }
        public string DriverName { get; set; }
        public string ItemCode { get; set; }
        public string Note { get; set; }
        public Nullable<double> Weight1 { get; set; }
        public Nullable<double> Weight2 { get; set; }
        public Nullable<System.DateTime> TimeWeight1 { get; set; }
        public Nullable<System.DateTime> TimeWeight2 { get; set; }
        public Nullable<double> Weight { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public string CreateBy { get; set; }
        public string UpdateBy { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
        public Nullable<bool> IsSynced { get; set; }
        public Nullable<System.DateTime> SyncDate { get; set; }
        public string SyncLog { get; set; }
        public string AreaCode { get; set; }
        public string Rfid { get; set; }
        public string BillNumber { get; set; }
        public string InvoiceNumber { get; set; }
        public string InvoiceTemplate { get; set; }
        public string InvoiceSymbol { get; set; }
        public Nullable<bool> IsCanceled { get; set; }
        public Nullable<int> BravoId { get; set; }
        public bool IsSyncToBravo { get; set; }
    
        public virtual MdArea MdArea { get; set; }
        public virtual MdItem MdItem { get; set; }
        public virtual MdPartner MdPartner { get; set; }
        public virtual MdVehicle MdVehicle { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ScaleImage> ScaleImages { get; set; }
    }
}
