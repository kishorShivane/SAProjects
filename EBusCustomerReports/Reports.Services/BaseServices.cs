using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;

namespace Reports.Services
{
    //public class DutyDayCharMapping
    //{ 
    //    //0 = sunday, 6 = saturday
    //    public int DayId { get; set; }
    //    public string DayChar { get; set; }
    //    public int StartPos { get; set; }
    //    public int EndPos { get; set; }

    //    public List<DutyDayCharMapping> GetAllMappings() {
    //        return  new List<DutyDayCharMapping>
    //        {
    //            new DutyDayCharMapping { DayId=0, DayChar="S", StartPos=0 },
    //            new DutyDayCharMapping { DayId=1, DayChar="M", StartPos=1 },
    //            new DutyDayCharMapping { DayId=2, DayChar="T", StartPos=2 },
    //            new DutyDayCharMapping { DayId=3, DayChar="W", StartPos=3 },
    //            new DutyDayCharMapping { DayId=4, DayChar="T", StartPos=4 },
    //            new DutyDayCharMapping { DayId=5, DayChar="F", StartPos=5 },
    //            new DutyDayCharMapping { DayId=6, DayChar="S", StartPos=6 },
    //            new DutyDayCharMapping { DayId=7, DayChar="H", StartPos=7 },
    //        };
    //    }

        
    //}

    public class BaseServices
    {
        //public DutyDayCharMapping GetMappingForDay(int dayId)
        //{
        //    return new DutyDayCharMapping().GetAllMappings().Where(s => s.DayId.Equals(dayId)).FirstOrDefault();
        //}

        protected string GetConnectionString(string connecKey)
        {
            return ConfigurationManager.ConnectionStrings[connecKey].ConnectionString;
        }

        public DataTable GetCashVsSmartCardUsageByRouteDataTable() {
            var table1 = new DataTable("CashVsSmartCardUsageByRouteDataTable");

            table1.Columns.Add("str_RouteID"); //0
            table1.Columns.Add("str50_RouteName"); //1
            table1.Columns.Add("ColorCodeTickets"); //2
            table1.Columns.Add("ColorCodePasses"); //3

            table1.Columns.Add("int4_JourneyTickets"); //4
            table1.Columns[4].DataType = typeof(double);
            table1.Columns.Add("int4_JourneyTicketsPercent"); //5
            table1.Columns[5].DataType = typeof(double);

            table1.Columns.Add("int4_JourneyPasses"); //6
            table1.Columns[6].DataType = typeof(double);
            table1.Columns.Add("int4_JourneyPassesPercent"); //7
            table1.Columns[7].DataType = typeof(double);

            table1.Columns.Add("int4_JourneyTransfer"); //8
            table1.Columns[8].DataType = typeof(double);
            table1.Columns.Add("int4_JourneyTransferPercent"); //9
            table1.Columns[9].DataType = typeof(double);

            table1.Columns.Add("TotalPassengers"); //10
            table1.Columns[10].DataType = typeof(double);

            table1.Columns.Add("DatesSelected");
            table1.Columns.Add("CompanyName");
            table1.Columns.Add("RoutesSelected");

            return table1;
        }

        public DataTable RevenueByDutyDataSet()
        {
            var table1 = new DataTable("RevByDuty");
            table1.Columns.Add("int4_DutyId"); //0
            table1.Columns[0].DataType = typeof(double);
            table1.Columns.Add("str4_JourneyNo");//1
            table1.Columns.Add("Cash"); //2
            table1.Columns[2].DataType = typeof(double);
            table1.Columns.Add("Value"); //3
            table1.Columns[3].DataType = typeof(double);
            table1.Columns.Add("AdultRevenue"); //4
            table1.Columns[4].DataType = typeof(double);
            table1.Columns.Add("ChildRevenue"); //5
            table1.Columns[5].DataType = typeof(double);
            table1.Columns.Add("AdultNonRevenue"); //6
            table1.Columns[6].DataType = typeof(double);
            table1.Columns.Add("ScholorNonRevenue"); //7
            table1.Columns[7].DataType = typeof(double);
            table1.Columns.Add("AdultTransfer"); //8
            table1.Columns[8].DataType = typeof(int);
            table1.Columns.Add("ScholorTransfer"); //9
            table1.Columns[9].DataType = typeof(int);
            table1.Columns.Add("companyName");//10
            table1.Columns.Add("DateRangeFilter");//11
            table1.Columns.Add("filterDuties");//12
            table1.Columns.Add("Total"); //13
            table1.Columns[13].DataType = typeof(double);
            return table1;
        }

        public DataTable ScheduleVsOperatedDataSet()
        {
            var table1 = new DataTable("SchOpr");

            table1.Columns.Add("dateSelected"); //0

            table1.Columns.Add("int4_DutyId"); //1
            table1.Columns[1].DataType = typeof(double);

            table1.Columns.Add("str4_JourneyNo");//2

            table1.Columns.Add("DOTRouteNumber");//3

            table1.Columns.Add("float_Distance");//4
            table1.Columns[4].DataType = typeof(double);

            table1.Columns.Add("str7_Contract");//5
            table1.Columns.Add("dat_StartTime");//6
            table1.Columns.Add("dat_EndTime");//7
            table1.Columns.Add("int4_OperatorID");//8
            table1.Columns.Add("dat_JourneyStartTime");//9
            table1.Columns.Add("dat_JourneyStopTime");//10

            table1.Columns.Add("int4_JourneyRevenue");//11
            table1.Columns[11].DataType = typeof(double);

            table1.Columns.Add("int4_JourneyTickets");//12
            table1.Columns[12].DataType = typeof(double);

            table1.Columns.Add("int4_JourneyPasses");//13
            table1.Columns[13].DataType = typeof(double);

            table1.Columns.Add("int4_JourneyNonRevenue");//14
            table1.Columns[14].DataType = typeof(double);

            table1.Columns.Add("int4_JourneyTransfer");//15
            table1.Columns[15].DataType = typeof(double);

            table1.Columns.Add("TripStatus");//16
            table1.Columns.Add("str_BusNr");//17
            table1.Columns.Add("Int_TotalPassengers");//18
            table1.Columns[18].DataType = typeof(double);

            table1.Columns.Add("float_OperDistance");//19
            table1.Columns[19].DataType = typeof(double);

            table1.Columns.Add("companyName");//20 
            table1.Columns.Add("DateRangeFilter");//21 
            table1.Columns.Add("filterContractsRange");//22
            table1.Columns.Add("filterDuties");//23 
            table1.Columns.Add("filterTimeSelected");//24

            return table1;
        }

        public DataTable DailyAuditDataset()
        {
            var table1 = new DataTable("DailyAudit");

            table1.Columns.Add("EmployeeNo");//0
            table1.Columns[0].DataType = typeof(Int64);

            table1.Columns.Add("EmployeeName"); //1
            table1.Columns.Add("Module"); //2
            table1.Columns.Add("Duty"); //3
            table1.Columns.Add("DutyDate"); //4
            table1.Columns.Add("DutySignOn"); //5
            table1.Columns.Add("DutySignOff"); //6
            table1.Columns.Add("BusNumber"); //7
            table1.Columns.Add("EquipmentNumber"); //8
            table1.Columns.Add("FirstRoute"); //9
            table1.Columns.Add("FirstJourney"); //10

            table1.Columns.Add("Revenue"); //11
            table1.Columns[11].DataType = typeof(double);

            table1.Columns.Add("Tickets"); //12
            table1.Columns[12].DataType = typeof(double);

            table1.Columns.Add("Passes"); //13
            table1.Columns[13].DataType = typeof(double);

            table1.Columns.Add("Transfers"); //14
            table1.Columns[14].DataType = typeof(double);

            table1.Columns.Add("modulesignoff"); //15
            table1.Columns.Add("modulesignon"); //16

            table1.Columns.Add("companyName");//17
            table1.Columns.Add("DateRangeFilter");//18
            table1.Columns.Add("StaffsSelected");//19
            table1.Columns.Add("StaffTypesSelected");//20
            table1.Columns.Add("TotalPs");//21
            table1.Columns[21].DataType = typeof(Int32);
            table1.Columns.Add("LocationSelected");//22

            return table1;
        }

        public DataTable DailyAuditByCashierTerminalDataset() {
            var table1 = new DataTable("DailyAudit");

            table1.Columns.Add("EmployeeNo");//0
            //table1.Columns[0].DataType = typeof(Int64);

            table1.Columns.Add("EmployeeName"); //1
            table1.Columns.Add("Module"); //2
            table1.Columns.Add("Duty"); //3
            table1.Columns.Add("DutyDate"); //4
            table1.Columns.Add("DutySignOn"); //5
            table1.Columns.Add("DutySignOff"); //6
            table1.Columns.Add("BusNumber"); //7
            table1.Columns.Add("EquipmentNumber"); //8
            table1.Columns.Add("FirstRoute"); //9
            table1.Columns.Add("FirstJourney"); //10

            table1.Columns.Add("Revenue"); //11
            table1.Columns[11].DataType = typeof(double);

            table1.Columns.Add("Tickets"); //12
            table1.Columns[12].DataType = typeof(double);

            table1.Columns.Add("Passes"); //13
            table1.Columns[13].DataType = typeof(double);

            table1.Columns.Add("Transfers"); //14
            table1.Columns[14].DataType = typeof(double);

            table1.Columns.Add("modulesignoff"); //15
            table1.Columns.Add("modulesignon"); //16

            table1.Columns.Add("companyName");//17
            table1.Columns.Add("DateRangeFilter");//18

            table1.Columns.Add("StaffsSelected");//19
            table1.Columns.Add("casherSelected");//20

            table1.Columns.Add("TotalPs");//21
            table1.Columns[21].DataType = typeof(Int32);

            table1.Columns.Add("LocationSelected");//22
            table1.Columns.Add("TermnalSelected");//23

            table1.Columns.Add("CashierName");//24
            table1.Columns.Add("str4_LocationCode");//25
            table1.Columns.Add("Terminal");//26

            table1.Columns.Add("Cashsignon");//27
            table1.Columns.Add("Cashsignoff");//28

            table1.Columns.Add("CashierNum");//29
            table1.Columns.Add("NonRevenue");//30
            table1.Columns[30].DataType = typeof(double);
            table1.Columns.Add("TransactionDatetime");//31

            return table1;
        }

        public DataTable FormEDataset()
        {
            var table1 = new DataTable("FormE");

            table1.Columns.Add("Contract");//0
            table1.Columns.Add("DOTRoute");//1
            table1.Columns.Add("From");//2
            table1.Columns.Add("To");//3

            table1.Columns.Add("ScheduledTrips");//4
            table1.Columns[4].DataType = typeof(double);
            table1.Columns.Add("OperatedTrips");//5
            table1.Columns[5].DataType = typeof(double);

            table1.Columns.Add("NotOperatedTrips");//6
            table1.Columns[6].DataType = typeof(double);
            table1.Columns.Add("Schedulekilometres");//7
            table1.Columns[7].DataType = typeof(double);
            table1.Columns.Add("OperatedKilometres");//8
            table1.Columns[8].DataType = typeof(double);

            table1.Columns.Add("Tickets");//9
            table1.Columns[9].DataType = typeof(double);
            table1.Columns.Add("Passes");//10
            table1.Columns[10].DataType = typeof(double);

            table1.Columns.Add("Transfers");//11
            table1.Columns[11].DataType = typeof(double);
            table1.Columns.Add("TotalPassengers");//12
            table1.Columns[12].DataType = typeof(Int32);

            table1.Columns.Add("Revenue");//13
            table1.Columns[13].DataType = typeof(double);

            table1.Columns.Add("NonRevenue");//14
            table1.Columns[14].DataType = typeof(double);
            table1.Columns.Add("TotalRevenue");//15
            table1.Columns[15].DataType = typeof(double);

            table1.Columns.Add("AvgPassengerPerTrip");//16
            table1.Columns[16].DataType = typeof(double);
            table1.Columns.Add("AvgRevenuePerTrip");//17
            table1.Columns[17].DataType = typeof(double);

            table1.Columns.Add("DateRangeFilter");//18
            table1.Columns.Add("ContractsFilter");//19

            table1.Columns.Add("companyName");//20

            table1.Columns.Add("DateSelected");//20

            return table1;
        }

        public DataTable FormE2Dataset()
        {
            var table1 = FormEDataset();

            table1.Columns.Add("DutyId");
            table1.Columns.Add("JourneyId");
            table1.Columns.Add("DutiesFilter");

            return table1;
        }

        public DataTable SmartCardDataset()
        {
            var table1 = new DataTable("SmartCardDataset");

            table1.Columns.Add("ClassID"); //1
            table1.Columns.Add("ClassName"); //2

            table1.Columns.Add("NonRevenue"); //3
            table1.Columns[2].DataType = typeof(double);
            table1.Columns.Add("Revenue"); //4
            table1.Columns[3].DataType = typeof(double);

            table1.Columns.Add("TransDate"); //5
            table1.Columns.Add("SerialNumber"); //6
            table1.Columns.Add("SerialNumberHex"); //7
            table1.Columns.Add("RouteID"); //8
            table1.Columns.Add("JourneyID"); //9
            table1.Columns.Add("OperatorID"); //10
            table1.Columns.Add("ETMID"); //11
            table1.Columns.Add("BusID"); //12
            table1.Columns.Add("DutyID"); //13
            table1.Columns.Add("ModuleID"); //14

            table1.Columns.Add("RevenueBalance"); //15
            table1.Columns[14].DataType = typeof(double);
            table1.Columns.Add("TripBalance"); //16
            table1.Columns[15].DataType = typeof(double);

            table1.Columns.Add("TransDay"); //17

            table1.Columns.Add("TDate"); //18
            table1.Columns.Add("TTime"); //19

            table1.Columns.Add("DateRangeFilter"); //20
            table1.Columns.Add("CardIdFilter"); //21
            table1.Columns.Add("CompanyName"); //22
            table1.Columns.Add("RechargeQuantity"); //23
            table1.Columns.Add("ActiveProductExpiryDate"); //24

            return table1;
        }

        public DataTable YearlyBreakDownDataset()
        {
            var table1 = new DataTable("yearly");

            table1.Columns.Add("MonthsSelected");//1
            table1.Columns.Add("CompanySelected");//2
            table1.Columns.Add("ClassFilterSelected");//3
            table1.Columns.Add("Year2Selected");//4
            table1.Columns.Add("Year1Selected");//5

            table1.Columns.Add("Class");//6

            table1.Columns.Add("Year2Revenue");//7
            table1.Columns[6].DataType = typeof(double);

            table1.Columns.Add("Year1Revenue");//8
            table1.Columns[7].DataType = typeof(double);

            table1.Columns.Add("RevenueDiff");//9
            table1.Columns[8].DataType = typeof(double);

            table1.Columns.Add("RevenueDiffPer");//10
            table1.Columns[9].DataType = typeof(double);

            table1.Columns.Add("Year2NonRevenue");//11
            table1.Columns[10].DataType = typeof(double);

            table1.Columns.Add("Year1NonRevenue");//12
            table1.Columns[11].DataType = typeof(double);

            table1.Columns.Add("NonRevenueDiff");//13
            table1.Columns[12].DataType = typeof(double);

            table1.Columns.Add("NonRevenueDiffPer");//14
            table1.Columns[13].DataType = typeof(double);

            table1.Columns.Add("Year2Passenger");//15
            table1.Columns[14].DataType = typeof(double);

            table1.Columns.Add("Year1Passenger");//16
            table1.Columns[15].DataType = typeof(double);

            table1.Columns.Add("PassengerDiff");//17
            table1.Columns[16].DataType = typeof(double);

            table1.Columns.Add("PassengerDiffPer");//18
            table1.Columns[17].DataType = typeof(double);

            table1.Columns.Add("Grp");//19
            table1.Columns[18].DataType = typeof(Int32);

            table1.Columns.Add("year2RevSum");//20
            table1.Columns[19].DataType = typeof(double);

            table1.Columns.Add("Year1RevSum");//21
            table1.Columns[20].DataType = typeof(double);

            table1.Columns.Add("RevDiffSum");//22
            table1.Columns[21].DataType = typeof(double);


            table1.Columns.Add("Year2NonRevSum");//23
            table1.Columns[22].DataType = typeof(double);

            table1.Columns.Add("Year1NonRevsum");//24
            table1.Columns[23].DataType = typeof(double);

            table1.Columns.Add("NonRevDiffSum");//25
            table1.Columns[24].DataType = typeof(double);


            table1.Columns.Add("Year2PsngSum");//26
            table1.Columns[25].DataType = typeof(double);

            table1.Columns.Add("Year1PsngSum");//27
            table1.Columns[26].DataType = typeof(double);

            table1.Columns.Add("PsngDiffSum");//28
            table1.Columns[27].DataType = typeof(double);

            table1.Columns.Add("RouteFilter");//
            table1.Columns.Add("RouteId");//
            table1.Columns.Add("RouteName");//
          
            return table1;
        }

        public DataTable InspectorReportDataset()
        {
            var table1 = new DataTable("inspector");

            table1.Columns.Add("stagetime"); //1
            table1.Columns.Add("journeyid"); //2
            table1.Columns.Add("routeId"); //3
            table1.Columns.Add("driverno"); //4
            table1.Columns.Add("busid"); //5
            table1.Columns.Add("inspectorno"); //6
            table1.Columns[5].DataType = typeof(Int32);

            table1.Columns.Add("int4_dutyid"); //7
            table1.Columns.Add("int2_stageid"); //8
            table1.Columns.Add("StageName"); //9
            table1.Columns.Add("InsDate"); //10
            table1.Columns.Add("InsTime"); //11

            table1.Columns.Add("DateRangeFilter"); //12
            table1.Columns.Add("CompanyName"); //
            table1.Columns.Add("DriversFilter"); //14
            return table1;
        }

        public DataTable SalesRouteAnalsisDataset()
        {
            var table1 = new DataTable("SalesRoute");

            table1.Columns.Add("RouteID"); //0
            table1.Columns.Add("RouteName"); //1
            table1.Columns.Add("ClassID"); //2
            table1.Columns[2].DataType = typeof(Int32);

            table1.Columns.Add("ClassName");//3
            table1.Columns.Add("ClassType");//4

            table1.Columns.Add("Revenue");//5
            table1.Columns[5].DataType = typeof(double);
            table1.Columns.Add("NonRevenue");//6
            table1.Columns[6].DataType = typeof(double);
            table1.Columns.Add("AnnulCash");//7
            table1.Columns[7].DataType = typeof(Int32);

            table1.Columns.Add("TxCount");//8
            table1.Columns[8].DataType = typeof(Int32);
            table1.Columns.Add("TicketCount");//9
            table1.Columns[9].DataType = typeof(Int32);

            table1.Columns.Add("dateRange");//10
            table1.Columns.Add("RoutesFilters");//11
            table1.Columns.Add("ClassIdFilters");//12
            table1.Columns.Add("CompanyName");//13
            table1.Columns.Add("ClassGroupFilter");//14
            table1.Columns.Add("AverageTicketValue");//15
            table1.Columns[15].DataType = typeof(double);

            return table1;
        }

        public DataTable ClassSummaryDataset()
        {
            var table1 = new DataTable("ClassSummary");

            table1.Columns.Add("ClassTypeName"); //0
            table1.Columns.Add("Class"); //1
            table1.Columns.Add("ClassGroup");//2

            table1.Columns.Add("Revenue");//3
            table1.Columns[3].DataType = typeof(double);
            table1.Columns.Add("NonRevenue");//4
            table1.Columns[4].DataType = typeof(double);

            table1.Columns.Add("TicketCount");//5
            table1.Columns[5].DataType = typeof(Int32);
            table1.Columns.Add("TripCount");//6
            table1.Columns[6].DataType = typeof(Int32);

            table1.Columns.Add("DateRange");//7
            table1.Columns.Add("ClassFilter");//8
            table1.Columns.Add("ClassGroupFilter");//9
            table1.Columns.Add("RouteFilter");//10
            table1.Columns.Add("CompanyName");//11
            table1.Columns.Add("AverageTicketValue");//12
            table1.Columns[12].DataType = typeof(double);

            return table1;
        }

        public DataTable CashierDataSet()
        {
            var table1 = new DataTable("CashierDataSet");
            table1.Columns.Add("StaffNumber"); //0
            table1.Columns.Add("Date"); //1
            table1.Columns.Add("Revenue"); //2
            table1.Columns[2].DataType = typeof(Decimal);

            table1.Columns.Add("StaffName"); //3
            table1.Columns.Add("CashierID"); //4
            table1.Columns.Add("Location"); //5
            table1.Columns.Add("CashierName"); //6
            table1.Columns.Add("dateFilter"); //7
            table1.Columns.Add("Locations");//8
            table1.Columns.Add("CompanyName");//9
            table1.Columns.Add("DateTime"); //10

            return table1;
        }

        public DataTable CashierReconciliationSummay()
        {
            var table1 = new DataTable("CashierDataSet");
            table1.Columns.Add("StaffNumber"); //0
            table1.Columns.Add("StaffName"); //1
            table1.Columns.Add("Cashier"); //2
            table1.Columns[2].DataType = typeof(Decimal);
            table1.Columns.Add("DutyRevenue"); //3
            table1.Columns[3].DataType = typeof(Decimal);

            table1.Columns.Add("Difference"); //4
            table1.Columns[4].DataType = typeof(Decimal);
            table1.Columns.Add("TransactionDate"); //5
            table1.Columns.Add("StaffSelected"); //6
            table1.Columns.Add("DateFilterSelected");//7
            table1.Columns.Add("LocationSelected");//8
            table1.Columns.Add("Location");//9
            table1.Columns.Add("CashierReason");//10
            table1.Columns.Add("Company");//10

            return table1;
        }
    }
}