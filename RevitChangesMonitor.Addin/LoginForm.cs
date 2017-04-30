﻿using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RevitChangesMonitor.Addin
{
    public partial class LoginForm : Form
    {
        private ExternalEvent _logInExternalEvent;

        public LoginForm(ExternalEvent logInExternalEvent)
        {
            _logInExternalEvent = logInExternalEvent;
            InitializeComponent();
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(usernameTextBox.Text) || string.IsNullOrEmpty(passwordTextBox.Text))
            {
                MessageBox.Show("Preencha com login e senha.");
                return;
            }

            var context = AppContext.Instance;
            context.LoginInfo = new Models.LoginInformation
            {
                UserName = usernameTextBox.Text,
                Password = passwordTextBox.Text
            };
            _logInExternalEvent.Raise();
            Close();
        }
    }
}