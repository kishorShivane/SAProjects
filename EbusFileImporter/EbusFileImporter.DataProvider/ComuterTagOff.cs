using System;

namespace EbusFileImporter.DataProvider
{
    public class ComuterTagOff
    {

        public int id_ComuterTagOff { get; set; }
        public Nullable<int> id_Stage { get; set; }
        public Nullable<int> id_Journey { get; set; }
        public Nullable<int> id_Duty { get; set; }
        public Nullable<int> id_Module { get; set; }
        public DateTime TagOffDate { get; set; }
        public DateTime TagOffTime { get; set; }
        public string CardEsn { get; set; }
        public string AllowedAlightStage { get; set; }
        public string AlightStage { get; set; }
        public bool OverrideFlag { get; set; }
        public int JourneysDeducted { get; set; }
        public int InitialJourneyCount { get; set; }
    }
}
