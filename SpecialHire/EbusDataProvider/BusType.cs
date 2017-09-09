//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EbusDataProvider
{
    using System;
    using System.Collections.Generic;
    
    public partial class BusType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BusType()
        {
            this.BookingVehicleInfoes = new HashSet<BookingVehicleInfo>();
            this.Buses = new HashSet<Bus>();
        }
    
        public int ID { get; set; }
        public string Type { get; set; }
        public string ImageName { get; set; }
        public int Capacity { get; set; }
        public int Standing { get; set; }
        public int Sitting { get; set; }
        public string Description { get; set; }
        public bool Status { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BookingVehicleInfo> BookingVehicleInfoes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Bus> Buses { get; set; }
    }
}
