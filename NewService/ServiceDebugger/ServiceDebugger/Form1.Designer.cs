namespace ServiceDebugger
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
            this.button1 = new System.Windows.Forms.Button();
            this.serviceList = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(208, 216);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Run Once";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // serviceList
            // 
            this.serviceList.FormattingEnabled = true;
            this.serviceList.Items.AddRange(new object[] {
            "IN_Import_To_IntellaLend_Service",
            "IN_Export_To_Ephesoft_Service",
            "IN_Email_Service",
            "IN_Box_File_Downloader",
            "IN_Purge_Loan_Service",
            "IN_Move_To_Ephesoft",
            "RB_MTS_IL_EncompassExport",
            "IN_Json_File_Creation_Service"});
            this.serviceList.Location = new System.Drawing.Point(85, 40);
            this.serviceList.Name = "serviceList";
            this.serviceList.Size = new System.Drawing.Size(353, 160);
            this.serviceList.TabIndex = 1;
            this.serviceList.SelectedIndexChanged += new System.EventHandler(this.serviceList_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(85, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "List Of Services :";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(664, 355);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.serviceList);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListBox serviceList;
        private System.Windows.Forms.Label label1;
    }
}

