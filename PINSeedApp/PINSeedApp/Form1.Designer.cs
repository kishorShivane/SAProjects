namespace PINSeedApp
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.txtPINSeed = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtDriverNumber = new System.Windows.Forms.TextBox();
            this.btnCalculate = new System.Windows.Forms.Button();
            this.txtDriverCode = new System.Windows.Forms.TextBox();
            this.txtDecimal = new System.Windows.Forms.TextBox();
            this.txtCode = new System.Windows.Forms.TextBox();
            this.Decimal = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(90, 88);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "PIN Seed";
            // 
            // txtPINSeed
            // 
            this.txtPINSeed.Location = new System.Drawing.Point(167, 85);
            this.txtPINSeed.Name = "txtPINSeed";
            this.txtPINSeed.Size = new System.Drawing.Size(100, 20);
            this.txtPINSeed.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(89, 116);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Driver Number";
            // 
            // txtDriverNumber
            // 
            this.txtDriverNumber.Location = new System.Drawing.Point(167, 112);
            this.txtDriverNumber.Name = "txtDriverNumber";
            this.txtDriverNumber.Size = new System.Drawing.Size(100, 20);
            this.txtDriverNumber.TabIndex = 3;
            // 
            // btnCalculate
            // 
            this.btnCalculate.Location = new System.Drawing.Point(176, 172);
            this.btnCalculate.Name = "btnCalculate";
            this.btnCalculate.Size = new System.Drawing.Size(75, 23);
            this.btnCalculate.TabIndex = 4;
            this.btnCalculate.Text = "Calculate";
            this.btnCalculate.UseVisualStyleBackColor = true;
            this.btnCalculate.Click += new System.EventHandler(this.btnCalculate_Click);
            // 
            // txtDriverCode
            // 
            this.txtDriverCode.Enabled = false;
            this.txtDriverCode.Location = new System.Drawing.Point(167, 146);
            this.txtDriverCode.Name = "txtDriverCode";
            this.txtDriverCode.Size = new System.Drawing.Size(100, 20);
            this.txtDriverCode.TabIndex = 5;
            // 
            // txtDecimal
            // 
            this.txtDecimal.Location = new System.Drawing.Point(167, 241);
            this.txtDecimal.Name = "txtDecimal";
            this.txtDecimal.Size = new System.Drawing.Size(100, 20);
            this.txtDecimal.TabIndex = 6;
            // 
            // txtCode
            // 
            this.txtCode.Enabled = false;
            this.txtCode.Location = new System.Drawing.Point(95, 268);
            this.txtCode.Name = "txtCode";
            this.txtCode.Size = new System.Drawing.Size(260, 20);
            this.txtCode.TabIndex = 7;
            // 
            // Decimal
            // 
            this.Decimal.AutoSize = true;
            this.Decimal.Location = new System.Drawing.Point(92, 247);
            this.Decimal.Name = "Decimal";
            this.Decimal.Size = new System.Drawing.Size(45, 13);
            this.Decimal.TabIndex = 8;
            this.Decimal.Text = "Decimal";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(179, 294);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 9;
            this.button1.Text = "Convert";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(133, 149);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(25, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "PIN";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(54, 271);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(28, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "LCR";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(449, 475);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.Decimal);
            this.Controls.Add(this.txtCode);
            this.Controls.Add(this.txtDecimal);
            this.Controls.Add(this.txtDriverCode);
            this.Controls.Add(this.btnCalculate);
            this.Controls.Add(this.txtDriverNumber);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtPINSeed);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPINSeed;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtDriverNumber;
        private System.Windows.Forms.Button btnCalculate;
        private System.Windows.Forms.TextBox txtDriverCode;
        private System.Windows.Forms.TextBox txtDecimal;
        private System.Windows.Forms.TextBox txtCode;
        private System.Windows.Forms.Label Decimal;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
    }
}

