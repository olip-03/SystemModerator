using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace SystemModerator.Forms
{
    /// <summary>
    /// Interaction logic for DomainJoin.xaml
    /// </summary>
    public partial class DomainJoin : Window
    {
        public string username = null;
        public string password = null;
        public string domain = null;
        private string error = null;
        public DomainJoin()
        {
            InitializeComponent();
            txt_errormessage.Visibility = Visibility.Hidden;
        }
        public bool connected = false;
        public async Task<bool> TestDomainConnection(string domain, string username, string password)
        {
            bool pass = false;
            await Task.Run(() =>
            {
                try
                {
                    PrincipalContext context = new PrincipalContext(ContextType.Domain, domain, username, password);
                    pass = context.ValidateCredentials(username, password);
                    connected = pass; 
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }
            });

            return pass;
        }

        private async void btn_connect_Click(object sender, RoutedEventArgs e)
        {
            txt_errormessage.Visibility = Visibility.Visible;
            txt_errormessage.Text = $"Connecting to {txt_domainname.Text}";

            username = txt_username.Text;
            password = txt_password.Password;
            domain = txt_domainname.Text;

            bool pass = await TestDomainConnection(domain, username, password);

            if (pass)
            {
                Close();
            }
            else
            {
                txt_errormessage.Text = error;
            }
        }
    }
}
