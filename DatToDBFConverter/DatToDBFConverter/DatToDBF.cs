using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatToDBFConverter
{
    public partial class DatToDBF : Form
    {
        public DatToDBF()
        {
            InitializeComponent();
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            var filePath = "";
            var filePathDBF = "";
            filePath = txtFileName.Text;

            if (filePath != string.Empty && filePath.ToUpper().Contains(".DAT"))
            {
                filePathDBF = txtFileName.Text.Substring(0, filePath.LastIndexOf('\\'));
                Stream stream = File.Open(filePath, FileMode.Open);
                StreamReader streamReader = new StreamReader(stream);
                StringBuilder stringBuilder = new StringBuilder();
                string str = string.Empty;
                while (!streamReader.EndOfStream)
                {
                    str = streamReader.ReadLine().ToString().Trim();
                }
                if (str.Length > 0)
                {
                    var processedData = SplitByChunk(str, 28).ToList();
                    var collection = GenerateDBFData(processedData);
                    CreateDBFFile(collection, filePathDBF);
                    MessageBox.Show("DUTY.dbf file is generated in the below Path\n" + filePathDBF);
                }
                stream.Close();
                streamReader.Close();
            }
            else
            {
                MessageBox.Show("Please select a file to upload (.DBF)");
            }
            txtFileName.Text = string.Empty;
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            OpenFileDialog op1 = new OpenFileDialog();
            op1.Multiselect = true;
            op1.ShowDialog();
            op1.Filter = "allfiles|*.dat";
            txtFileName.Text = op1.FileName;
        }

        public IEnumerable<string> SplitByChunk(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }

        public List<DutyDBF> GenerateDBFData(IEnumerable<string> fileContent)
        {
            var collection = new List<DutyDBF>();

            foreach (var str in fileContent)
            {
                var item = new DutyDBF();
                var temp = str.Trim();
                item.BOARD = str.Substring(0, 4);
                item.DUTY = str.Substring(4, 5);
                item.ROUTE = str.Substring(9, 4);
                item.JOURNEY = str.Substring(15, 4);
                item.DIR = str.Substring(19, 1);
                item.START = str.Substring(20, 4);
                item.DAYS = str.Substring(25, 2);
                collection.Add(item);
            }
            return collection;
        }

        private void CreateDBFFile(List<DutyDBF> dbfTableContent, string filePathDBF)
        {
            try
            {
                var folderName = CompanyName + DateTime.Now.ToString("ddMMyyyyHHmmss");
                var provider = System.Configuration.ConfigurationSettings.AppSettings["Provider"];
                provider = provider.Replace("##filepath##", filePathDBF);
                if (!Directory.Exists(filePathDBF))
                {
                    Directory.CreateDirectory(filePathDBF);
                }

                OleDbConnection dBaseConnection = null;

                string TableName = "DUTY";

                using (dBaseConnection = new OleDbConnection(provider))
                {
                    dBaseConnection.Open();

                    OleDbCommand olecommand = dBaseConnection.CreateCommand();

                    if ((System.IO.File.Exists(filePathDBF + "/" + TableName + ".dbf")))
                    {
                        System.IO.File.Delete(filePathDBF + "/" + TableName + ".dbf");
                        olecommand.CommandText = "CREATE TABLE [" + TableName + "] ([BOARD] varchar(7), [DUTY] varchar(7), [ROUTE] varchar(7), [JOURNEY] varchar(4), [DIR] varchar(1), [START] varchar(5), [DAYS] Integer)";
                        olecommand.ExecuteNonQuery();
                    }
                    else
                    {
                        olecommand.CommandText = "CREATE TABLE [" + TableName + "] ([BOARD] varchar(7), [DUTY] varchar(7), [ROUTE] varchar(7), [JOURNEY] varchar(4), [DIR] varchar(1), [START] varchar(5), [DAYS] Integer)";
                        olecommand.ExecuteNonQuery();
                    }

                    OleDbDataAdapter oleadapter = new OleDbDataAdapter(olecommand);
                    OleDbCommand oleinsertCommand = dBaseConnection.CreateCommand();

                    foreach (var item in dbfTableContent)
                    {
                        oleinsertCommand.CommandText = "INSERT INTO [" + TableName + "] ([BOARD], [DUTY],[ROUTE],[JOURNEY],[DIR],[START],[DAYS]) VALUES ('" + item.BOARD + "','" + item.DUTY + "','" + item.ROUTE + "','" + item.JOURNEY + "','" + item.DIR + "','" + item.START + "'," + item.DAYS + ")";
                        oleinsertCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }
    }
}
