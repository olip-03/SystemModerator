using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SystemModerator.Classes;

namespace SystemModerator.Forms
{
    public class ADThreadWorker
    {
        string searchbase;
        private ADCallback_OU OUCallback;
        public ADThreadWorker(ADCallback_OU orgunitCallback)
        {
            OUCallback = orgunitCallback;
        }
    }
    public delegate void ADCallback_OU(ADOrganizationalUnit orgunit);
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Window
    {
        public Home()
        {
            InitializeComponent();
            InitializeView();
        }
        
        private bool initalized = false;
        /// <summary>
        /// Initalizes the user interface, populating data where required.
        /// This method can only, and should only be called once on startup to initalize the view.
        /// </summary>.
        public void InitializeView()
        {
            if(initalized)
            {
                return;
            }

            List_SystemBrowse.Items.Clear();
            Tree_Browse.Items.Clear();

            new Thread(() =>
            {
                // Create root DirectoryEntry
                // TODO: Replace with global static DirectoryEntry that is set on start.
                DirectoryEntry entry = new DirectoryEntry();

                string distinguishedName = entry.Properties["distinguishedname"].Value.ToString();
                TreeAsset ParentItem = null;

                this.Dispatcher.Invoke(() =>
                {
                    ParentItem = new TreeAsset();
                    ParentItem.Text = System.Environment.UserDomainName;
                    ParentItem.ADObject = new ADOrganizationalUnit();
                    ParentItem.ADObject.DistinguishedName = distinguishedName;
                    ParentItem.ADObject.Name = distinguishedName;
                    ParentItem.ADObject.ObjectClass = "organizationalUnit";
                    ParentItem.Name = System.Environment.UserDomainName;

                    txt_directorylabel.Content = distinguishedName;

                    ParentItem.SetIcon("server.png");
                    Tree_Browse.Items.Add(ParentItem);
                });

                DirectorySearcher searcher = new DirectorySearcher
                {
                    // specify that you search for organizational units 
                    SearchRoot = entry,
                    Filter = "(objectCategory=organizationalUnit)",
                    SearchScope = SearchScope.OneLevel
                };
                
                foreach (SearchResult result in searcher.FindAll())
                {
                    ADOrganizationalUnit item = new ADOrganizationalUnit();
                    item.Name = result.Properties["name"].OfType<string>().First();
                    item.ObjectClass = result.Properties["objectclass"].OfType<string>().First();
                    item.DistinguishedName = result.Properties["distinguishedname"].OfType<string>().First();

                    // return to callback
                    // OUCallback(item);
                    this.Dispatcher.Invoke(() => {
                        TreeAsset ChildItem = new TreeAsset();
                        ChildItem.Text = result.Properties["name"].OfType<string>().First();
                        ChildItem.ADObject = new ADOrganizationalUnit();
                        ChildItem.ADObject.DistinguishedName = result.Properties["distinguishedname"].OfType<string>().First();
                        ChildItem.ADObject.Name = result.Properties["name"].OfType<string>().First();
                        ChildItem.ADObject.ObjectClass = result.Properties["objectclass"].OfType<string>().First();

                        ADListItem newItem = new ADListItem(ChildItem.ADObject.Name, "folder.png");

                        bool hassSubdirectories = ChildItem.HasSubdirectories();
                        if (hassSubdirectories)
                        {
                            ChildItem.SetIcon("folders.png");
                            newItem.SetIcon("folders.png");
                            ChildItem.Items.Add(new TreeAsset());
                        }
                        else
                        {
                            ChildItem.SetIcon("folder.png");
                        }

                        List_SystemBrowse.Items.Add(newItem);
                        ParentItem.Items.Add(ChildItem);
                    });
                }
                #pragma warning restore CA1416 // Validate platform compatibility
            }).Start();
            initalized = true;
        }

        #region interactions
        private void Tree_Browse_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(sender is TreeView) 
            { // check we are receiving the correct type
                TreeView treeView = sender as TreeView;
                TreeAsset asset = treeView.SelectedItem as TreeAsset;
                //asset.SetIcon("device-desktop.png");
            }
        }
        #endregion
    }
}
