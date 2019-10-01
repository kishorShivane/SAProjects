using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbusFileImporter.DataProvider.Models
{
    public partial class Stage
    {
        public int id_Stage { get; set; }
        public int id_Journey { get; set; }
        public int id_Duty { get; set; }
        public int id_Module { get; set; }
        public Nullable<short> int2_StageID { get; set; }
        public Nullable<System.DateTime> dat_StageDate { get; set; }
        public Nullable<System.DateTime> dat_StageTime { get; set; }
        public Nullable<System.DateTime> dat_RecordMod { get; set; }
        public Nullable<int> id_BatchNo { get; set; }
    }

    public class TempStage
    {
        public int id_Stage { get; set; }
        public string TSN { get; set; }
        public string RecordedTime { get; set; }
    }
}
