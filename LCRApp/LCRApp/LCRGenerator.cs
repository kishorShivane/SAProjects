using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LCRApp
{
    public partial class LCRGenerator : Form
    {
        public LCRGenerator()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
        }


        private void btnCalculate_Click(object sender, EventArgs e)
        {
            var inputValue = txtInput.Text;
            decimal value;
            var parse = Decimal.TryParse(inputValue, out value);
            if (inputValue == string.Empty && !parse)
            {
                MessageBox.Show("Please enter a decimal number");
            }

            lblResult.Text = "LCR Number:" + GenerateLCRCode(value);
        }

        public string GenerateLCRCode(Decimal decValue)
        {
            var hexValue = decValue.ToHexString();
            var result = "FF 00 00 ##HEX## 00 00 ##LCR##";
            var PINFeed = "5A";
            if (hexValue.Length < 8)
            {
                hexValue = hexValue.PadLeft(8, '0'); //RIGHT HERE!!!
            }

            var charHex = hexValue.ToCharArray();
            var hexResult = charHex[0].ToString() + charHex[1].ToString()
                + " " + charHex[2].ToString() + charHex[3].ToString()
                + " " + charHex[4].ToString() + charHex[5].ToString()
                + " " + charHex[6].ToString() + charHex[7].ToString();
            var splitHex = hexResult.Split(' ');
            var LCR = ((((Convert.ToInt32(PINFeed, 16) ^ Convert.ToInt32(splitHex[0], 16))
                ^ Convert.ToInt32(splitHex[1], 16)) ^ Convert.ToInt32(splitHex[2], 16))
                ^ Convert.ToInt32(splitHex[3], 16));

            result = result.Replace("##HEX##", hexResult).Replace("##LCR##", LCR.ToString("X"));

            return result;
        }

    }

    public static class DecimalHelper
    {
        public static string ToHexString(this Decimal dec)
        {
            var sb = new StringBuilder();
            while (dec > 1)
            {
                var r = dec % 16;
                dec /= 16;
                sb.Insert(0, ((int)r).ToString("X"));
            }
            return sb.ToString();
        }
    }
}
