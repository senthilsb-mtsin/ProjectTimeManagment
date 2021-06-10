using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MTSEntBlocks.ExceptionBlock.Handlers;
using MTSEntBlocks.AuthenticationBlock;

namespace ServiceController
{
    public partial class ServiceLogon : Form
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string DomainName { get; set; }
        public bool IsServiceAccount { get; set; }

        public ServiceLogon()
        {
            InitializeComponent();
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
         
                if (chkUser.Checked)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    return;
                }

                UserName = txtUsername.Text.Trim();
                Password = txtPassword.Text.Trim();
                DomainName = txtDomain.Text.Trim();

                if (string.IsNullOrEmpty(DomainName))
                {
                    MessageBox.Show("Enter Domain.");
                    txtDomain.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(UserName))
                {
                    MessageBox.Show("Enter Username.");
                    txtUsername.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(Password))
                {
                    MessageBox.Show("Enter Password.");
                    txtPassword.Focus();
                    return;
                }

                if (!LogOn.LogonUser(UserName, Password, DomainName))
                {
                    MessageBox.Show("Incorrect credentials or problem contacting server. Please try again.");
                    ClearTextBoxes();
                    return;
                }
 
            IsServiceAccount = true;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ClearTextBoxes()
        {
            txtDomain.Text = string.Empty;
            txtUsername.Text = string.Empty;
            txtPassword.Text = string.Empty;
            txtDomain.Focus();
        }

        private void chkUser_CheckedChanged(object sender, EventArgs e)
        {
            if (chkUser.Enabled)
            {
                txtDomain.ReadOnly = true;
                txtUsername.ReadOnly = true;
                txtPassword.ReadOnly = true;
            }
            else
            {
                txtDomain.ReadOnly = false;
                txtUsername.ReadOnly = false;
                txtPassword.ReadOnly = false;
            }
        }
    }
}
