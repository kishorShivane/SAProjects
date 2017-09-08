using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Reports.Services.Models
{
    public class StaffEditorViewModel{
        public List<StaffEditor> StaffList { get; set; }
        public List<SelectListItem> Locations { get; set; }
        public List<SelectListItem> StaffTypes { get; set; }
    }

    public class StaffEditor
    {
        [Range(Int32.MinValue, Int32.MaxValue, ErrorMessage = "Please enter valid Number")]
        [Required(ErrorMessage = "Please enter valid Number")]
        public Int32 int4_StaffID { get; set; }

        [Required]
        public string str50_StaffName { get; set; }

        public bool bit_InUse { get; set; }

        public DateTime dat_RecordMod { get; set; }

        [Required]
        public string LocationSelected { get; set; }
        public List<SelectListItem> Locations { get; set; }

        [Required]
        public string SerialNumber { get; set; }

        public Int32 int4_StaffTypeID { get; set; }

        //staff type name
        [Required]
        public string StaffTypeSelected { get; set; }
        public List<SelectListItem> StaffTypes{ get; set; }

        [Required]
        public string str50_LocationGroupName { get; set; }

        public short byt_Level { get; set; }
    }
}
