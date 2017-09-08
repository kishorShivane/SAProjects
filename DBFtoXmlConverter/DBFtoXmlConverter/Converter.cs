using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DBFtoXmlConverter
{
    public partial class Converter : Form
    {
        private OleDbConnection connection;
        private string sqlString = "";
        private DataSet myDataSet;
        private OleDbDataAdapter myAdapter;

        public Converter()
        {
            InitializeComponent();
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            var filePath = "";
            var fileName = "";
            var xmlFileName = "Schedule.xml";
            filePath = txtFileName.Text;

            if (filePath != string.Empty && filePath.ToUpper().Contains(".DBF"))
            {
                fileName = filePath.Split('\\').Last();
                filePath = filePath.Replace("\\" + fileName, "");
                ScheduleFile scheduleFile = null;
                connection = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filePath + ";Extended Properties=DBASE III;");
                connection.Open();
                sqlString = "Select * from [" + fileName + "]";
                //Make a DataSet object
                myDataSet = new DataSet();
                //Using the OleDbDataAdapter execute the query
                myAdapter = new OleDbDataAdapter(sqlString, connection);
                //Build the Update and Delete SQL Statements
                OleDbCommandBuilder myBuilder = new OleDbCommandBuilder(myAdapter);
                try
                {
                    //Fill the DataSet with the Table 'bookstock'
                    myAdapter.Fill(myDataSet);

                    if (myDataSet != null && myDataSet.Tables.Count > 0 && myDataSet.Tables[0].Rows.Count > 0)
                    {
                        scheduleFile = MapDataToScheduleFileObject(myDataSet);
                        if (scheduleFile != null)
                        {
                            WriteXML(scheduleFile, filePath, xmlFileName);
                            MessageBox.Show("XML file generated successfully!! - "+ filePath+ "\\"+xmlFileName);
                        }
                    }
                    else
                    {
                        MessageBox.Show("No data in DBF file!!");
                    }
                    connection.Close();
                }
                catch (Exception oEx)
                {
                    throw (oEx);
                }
            }
            else
            {
                MessageBox.Show("Please select a file to upload (.DBF)");
            }
            txtFileName.Text = string.Empty;
        }

        public void WriteXML(ScheduleFile scheduleFile, string filePath, string xmlFileName)
        {
            System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(ScheduleFile));

            var path = filePath + "\\" + xmlFileName;
            System.IO.FileStream file = System.IO.File.Create(path);

            writer.Serialize(file, scheduleFile);
            file.Close();
        }

        private ScheduleFile MapDataToScheduleFileObject(DataSet myDataSet)
        {
            ScheduleFile scheduleFile = null;
            List<Trip> trips = null;
            Trip trip = null;

            scheduleFile = new ScheduleFile();
            scheduleFile.ScheduleDetails = new List<Trip>();
            foreach (DataRow item in myDataSet.Tables[0].Rows)
            {
                trip = new Trip();
                trip.RunningBoard = GetDutyValue(item["DUTY"].ToString().Trim());
                trip.Duty = GetDutyValue(item["DUTY"].ToString().Trim());
                trip.Service = item["ROUTE"].ToString().Trim();
                trip.Journey = GetJourneyValue(item["JOURNEY"].ToString().Trim());
                trip.Direction = GetDIRValue(item["DIR"].ToString().Trim());
                trip.StartTime = GetStartTimeValue(item["JOURNEY"].ToString().Trim());
                trip.Blind = GetDutyValue(item["DUTY"].ToString().Trim());
                scheduleFile.ScheduleDetails.Add(trip);
            }
            return scheduleFile;
        }

        public string GetDIRValue(string dir)
        {
            return dir == "0" ? "Out" : "In";
        }
        public string GetStartTimeValue(string journey)
        {
            var str = journey.ToCharArray();
            return str[0].ToString() + str[1].ToString() + ":" + str[2].ToString() + str[3].ToString();
        }
        public string GetJourneyValue(string journey)
        {
            if (journey.StartsWith("0"))
            {
                var str = journey.ToCharArray();
                for (int i = 0; i < str.Count(); i++)
                {
                    if (str[i] != '0')
                    { break; }
                    else
                    { str[i] = '1'; }
                }
                journey = str[0].ToString() + str[1].ToString() + str[2].ToString() + str[3].ToString();
            }
            return journey;
        }
        public string GetDutyValue(string duty)
        {
            return duty.TrimStart('0');
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            OpenFileDialog op1 = new OpenFileDialog();
            op1.Multiselect = true;
            op1.ShowDialog();
            op1.Filter = "allfiles|*.dbf";
            txtFileName.Text = op1.FileName;
        }
    }

    public class ScheduleFile
    {
        public ScheduleFile()
        {
            Version = new Version() { DataVersion = "15", SchemaVersion = "1.0", StartDate = "2011.06.07T00:00:00", EndDate = "2012.06.07T23:59:59", ChangedBy = "UKBOSSYS121\\Administrator on 23/10/2012 15:10:41 :  0 Additions;  3377 Deletions;  0 Changes." };
        }
        public Version Version { get; set; }
        public List<Trip> ScheduleDetails { get; set; }
    }

    public class Version
    {
        public string DataVersion { get; set; }
        public string SchemaVersion { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string ChangedBy { get; set; }
    }

    public class Trip
    {
        public string RunningBoard { get; set; }
        public string Duty { get; set; }
        public string Service { get; set; }
        public string Journey { get; set; }
        public string Direction { get; set; }
        public string StartTime { get; set; }
        public string Blind { get; set; }
    }
}
