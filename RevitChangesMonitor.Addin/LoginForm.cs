using Autodesk.Revit.UI;
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

        private async void loginButton_Click(object sender, EventArgs e)
        {
            await TryLogIn();
        }

        

        private async void LoginForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                await TryLogIn(); 
            }
        }

        private async Task TryLogIn()
        {
            var context = AppContext.Instance;
            var ws = context.WebService;

            if (string.IsNullOrEmpty(usernameTextBox.Text))
            {
                MessageBox.Show("Preencha com login e senha.");
                usernameTextBox.Focus();
                return;
            }
            if (string.IsNullOrEmpty(passwordTextBox.Text))
            {
                MessageBox.Show("Preencha com login e senha.");
                passwordTextBox.Focus();
                return;
            }
            if (!await ws.Authenticate(usernameTextBox.Text, passwordTextBox.Text))
            {
                MessageBox.Show("Login ou senha inválidos.");
                return;
            }

            context.LoginInfo = new Models.LoginInformation
            {
                UserName = usernameTextBox.Text,
                Password = passwordTextBox.Text
            };

            Close();
            _logInExternalEvent.Raise();
        }
    }
}
