using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml.Linq;
using SystemModerator.Classes;

namespace SystemModerator.Forms
{
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

                    // By setting the IsSelected variable to true here, we also
                    // signal the TreeView's Tree_Browse_SelectedItemChanged method
                    // which will populate the treeview
                    ParentItem.IsSelected = true;
                    ParentItem.IsExpanded = true;
                    ParentItem.SetIcon("server.png");
                    Tree_Browse.Items.Add(ParentItem);
                });
                
                #pragma warning restore CA1416 // Validate platform compatibility
            }).Start();

            initalized = true;
        }

        #region interactions
        /// <summary>
        /// Occurs when a TreeItem with children is opened via the arrow dropdown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeItem_Expanded(object sender, RoutedEventArgs e)
        {
            if(sender is TreeAsset)
            {
                TreeAsset asset = sender as TreeAsset;

                // neccesary for the ListItem_DoubleClick event
                if (asset.ignoreExpansionPopulation) 
                {
                    // reset the flag once were done so we don't always skip
                    // this event
                    asset.ignoreExpansionPopulation = false;
                    return; 
                }

                if(!asset.populated)
                {
                    asset.Items.Clear();
                    new Thread(() =>
                    {
                        string search = "LDAP://" + asset.ADObject.DistinguishedName;

                        #pragma warning disable CA1416 // Validate platform compatibility
                        // I only intend for this application to be running on Windows, so this is beyond
                        // my concern. If someone wants a linux desktop build of this they can use wine.'

                        // Fetch all OU's
                        List<ADOrganizationalUnit> adInfo = new List<ADOrganizationalUnit>();
                        DirectoryEntry entry = new DirectoryEntry(search);
                        DirectorySearcher searcher = new DirectorySearcher
                        {
                            // specify that you search for organizational units 
                            SearchRoot = entry,
                            Filter = "(objectCategory=organizationalUnit)",
                            SearchScope = SearchScope.OneLevel
                        };

                        foreach (SearchResult result in searcher.FindAll())
                        {
                            string name = result.Properties["name"].OfType<string>().First();
                            string objectClass = result.Properties["objectclass"].OfType<string>().First();
                            string distinguishedName = result.Properties["distinguishedname"].OfType<string>().First();

                            this.Dispatcher.Invoke(() =>
                            {
                                TreeAsset ChildItem = new TreeAsset();
                                ChildItem.Text = result.Properties["name"].OfType<string>().First();
                                ChildItem.ADObject = new ADOrganizationalUnit();
                                ChildItem.ADObject.DistinguishedName = result.Properties["distinguishedname"].OfType<string>().First();
                                ChildItem.ADObject.Name = result.Properties["name"].OfType<string>().First();
                                ChildItem.ADObject.ObjectClass = result.Properties["objectclass"].OfType<string>().First();
                                ChildItem.Expanded += TreeItem_Expanded;
                                bool hassSubdirectories = ChildItem.HasSubdirectories();
                                if (hassSubdirectories)
                                {
                                    ChildItem.SetIcon("folders.png");
                                    ChildItem.Items.Add(new TreeAsset());
                                }
                                else
                                {
                                    ChildItem.SetIcon("folder.png");
                                }
                                asset.Items.Add(ChildItem);
                            });
                        }

                        asset.populated = true;
                        #pragma warning restore CA1416 // Validate platform compatibility
                    }).Start();
                }
            }
        }
        /// <summary>
        /// Occurs when the selected item in the tree view is changed (something else is selected)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tree_Browse_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(sender is TreeView) 
            { // check we are receiving the correct type
                TreeView treeView = sender as TreeView;
                TreeAsset asset = treeView.SelectedItem as TreeAsset;

                List_SystemBrowse.Items.Clear();
                this.Dispatcher.Invoke(() =>
                {
                    txt_directorylabel.Content = asset.ADObject.DistinguishedName;
                    if(!asset.populated)
                    {
                        asset.Items.Clear();
                    }
                });

                new Thread(() =>
                {
                    string search = "LDAP://" + asset.ADObject.DistinguishedName;

                    #pragma warning disable CA1416 // Validate platform compatibility
                    // I only intend for this application to be running on Windows, so this is beyond
                    // my concern. If someone wants a linux desktop build of this they can use wine.'

                    // Fetch all OU's
                    List<ADOrganizationalUnit> adInfo = new List<ADOrganizationalUnit>();
                    DirectoryEntry entry = new DirectoryEntry(search);
                    DirectorySearcher searcher = new DirectorySearcher
                    {
                        // specify that you search for organizational units 
                        SearchRoot = entry,
                        Filter = "(objectCategory=organizationalUnit)",
                        SearchScope = SearchScope.OneLevel
                    };

                    foreach (SearchResult result in searcher.FindAll())
                    {
                        string objectClass = result.Properties["objectclass"].OfType<string>().First();
                        string distinguishedName = result.Properties["distinguishedname"].OfType<string>().First();
                        string name = result.Properties["name"].OfType<string>().First();

                        this.Dispatcher.Invoke(() =>
                        {
                            ADOrganizationalUnit adinfo = new ADOrganizationalUnit();
                            adinfo.DistinguishedName = result.Properties["distinguishedname"].OfType<string>().First();
                            adinfo.Name = result.Properties["name"].OfType<string>().First();
                            adinfo.ObjectClass = result.Properties["objectclass"].OfType<string>().First();

                            ADListItem newItem = new ADListItem(name, "folder.png");
                            newItem.ADObject = adinfo;
                            newItem.MouseDoubleClick += ListItem_DoubleClick;
                            List_SystemBrowse.Items.Add(newItem);

                            TreeAsset ChildItem = new TreeAsset();
                            ChildItem.Text = result.Properties["name"].OfType<string>().First();
                            ChildItem.ADObject = adinfo;

                            ChildItem.Expanded += TreeItem_Expanded;
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

                            if (!asset.populated)
                            {
                                asset.Items.Add(ChildItem);
                            }
                        });
                    }

                    asset.populated = true;
                    #pragma warning restore CA1416 // Validate platform compatibility
                }).Start();
            }
        }
        /// <summary>
        /// Occurs when an item in the list view is double clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListItem_DoubleClick(object sender, RoutedEventArgs e)
        {
            if (sender is ADListItem)
            {
                ADListItem listItem = sender as ADListItem;

                TreeAsset selectedItem = Tree_Browse.SelectedItem as TreeAsset;

                if(!selectedItem.IsExpanded)
                {
                    selectedItem.ignoreExpansionPopulation = true;

                    selectedItem.IsExpanded = true;
                }

                if(!selectedItem.populated)
                {
                    RoutedEvent ConditionalClickEvent = EventManager.RegisterRoutedEvent(
                    name: "ConditionalClick",
                    routingStrategy: RoutingStrategy.Bubble,
                    handlerType: typeof(RoutedEventHandler),
                    ownerType: typeof(TreeAsset));
                    RoutedEventArgs routedEventArgs = new(routedEvent: ConditionalClickEvent);

                    TreeItem_Expanded(selectedItem, routedEventArgs);
                }
                foreach (var item in selectedItem.Items)
                {
                    TreeAsset asset = item as TreeAsset;
                    if(asset.ADObject.DistinguishedName == listItem.ADObject.DistinguishedName)
                    {
                        asset.ignoreExpansionPopulation = true;

                        asset.IsExpanded = true;
                        asset.IsSelected = true; // selecting asset will populate it 
                        break;
                    }
                }
            }
        }
        #endregion
    }
}
