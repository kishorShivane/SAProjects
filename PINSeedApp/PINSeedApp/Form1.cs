using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PINSeedApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            string PINSeed = txtPINSeed.Text;
            string DriverNumber = txtDriverNumber.Text;

            txtDriverCode.Text = GeneratePIN(PINSeed, DriverNumber);
            txtDriverCode.Enabled = true;
        }

        public string GeneratePIN(string PINSeed, string DriverNumber)
        {
            string result = "";
            int temp;

            if (DriverNumber.Length < 6)
            {
                DriverNumber = DriverNumber.PadLeft(6, '0'); //RIGHT HERE!!!
            }

            if (PINSeed.Length < 4)
            {
                PINSeed = PINSeed.PadLeft(4, '0'); //RIGHT HERE!!!
            }

            var charPINSeed = PINSeed.ToCharArray();
            var charDriverNumber = DriverNumber.ToCharArray();
            
            for (var i = 0; i < charDriverNumber.Length; i++)
            {
                if ((i + charPINSeed.Length) <= charDriverNumber.Length + 1)
                {
                    temp = 0;
                    if (i + 3 != charDriverNumber.Length)
                    {
                        temp = temp + (charPINSeed[0] ^ charDriverNumber[i]) + (charPINSeed[1] ^ charDriverNumber[i + 1]) + (charPINSeed[2] ^ charDriverNumber[i + 2]) + (charPINSeed[3] ^ charDriverNumber[i + 3]);
                    }
                    else
                    {
                        temp = temp + (charPINSeed[0] ^ charDriverNumber[i]) + (charPINSeed[1] ^ charDriverNumber[i + 1]) + (charPINSeed[2] ^ charDriverNumber[i + 2]) + (charPINSeed[3] ^ charDriverNumber[0]);
                    }
                    temp = temp & 15;
                    if (temp > 9) { temp = temp - 9; }
                    result = result + temp.ToString();
                }
            }

            return result;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Decimal decValue = Convert.ToDecimal(txtDecimal.Text);
            txtCode.Text = GenerateLCRCode(decValue);
            txtCode.Enabled = true;
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


    public static class HexDecimalHelper
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

