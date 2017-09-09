using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace EBusReportsService.Models
{
    public class SchVsWorked
    {
        public string dateSelected { get; set; }
        public string int4_DutyId { get; set; }
        public string str4_JourneyNo { get; set; }
        public string DOTRouteNumber { get; set; }
        public float float_Distance { get; set; }
        public string str7_Contract { get; set; }
        public string dat_StartTime { get; set; }
        public string dat_EndTime { get; set; }
        public string int4_OperatorID { get; set; }
        public string dat_JourneyStartTime { get; set; }

        public DateTime? dat_StartTime_
        {
            get
            {
                if (!string.IsNullOrEmpty(dat_StartTime))
                {
                    return DateTime.ParseExact(dat_StartTime, "HH:mm:ss",
                                        CultureInfo.InvariantCulture);
                }
                else
                {
                    return null;
                }
            }
        }

        public DateTime? dat_JourneyStartTime_
        {
            get
            {
                if (!string.IsNullOrEmpty(dat_JourneyStartTime))
                {
                    return DateTime.ParseExact(dat_JourneyStartTime, "HH:mm:ss",
                                        CultureInfo.InvariantCulture);
                }
                else
                {
                    return null;
                }
            }
        }

        public double? JourneyDiffInMins
        {
            get
            {
                if (dat_JourneyStartTime_.HasValue && dat_StartTime_.HasValue)
                {
                    DateTime a = dat_StartTime_.Value;
                    DateTime b = dat_JourneyStartTime_.Value;
                    return Math.Round(b.Subtract(a).TotalMinutes);
                }
                else
                {
                    return null;
                }
            }
        }

        public string dat_JourneyStopTime { get; set; }
        public string int4_JourneyRevenue { get; set; }
        public string int4_JourneyTickets { get; set; }
        public string int4_JourneyPasses { get; set; }
        public string int4_JourneyNonRevenue { get; set; }
        public string int4_JourneyTransfer { get; set; }
        public string TripStatus { get; set; }
        public string str_BusNr { get; set; }
        public Int64 Int_TotalPassengers { get; set; }

        public float float_OperDistance { get; set; }

        public string routeName { get; set; }
        public Boolean bit_ReveOrDead { get; set; }
        public bool IsPosition { get; set; }
    }
}
