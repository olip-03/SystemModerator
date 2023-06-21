using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Management.Automation;
using System.Threading.Tasks;
using System.Windows;
using System.Net;
using System.Security;
using System.Diagnostics;
using SystemModerator.Classes;
using Newtonsoft.Json;
using System.Windows.Controls;
using System.DirectoryServices;
using SystemModerator.Forms;
using System.DirectoryServices.AccountManagement;

// Icon pack provided by https://github.com/ionic-team/ionicons

namespace SystemModerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string domain = null;
        public string username = null;
        public string password = null;

        public List<Asset> Assets = new List<Asset>();

        public string domainName = null;

        private PowerShell ps = PowerShell.Create();
        private static Dictionary<string, string> resources = new Dictionary<string, string>()
        {
            { "files/scripts/Main.ps1", "SystemModerator.Resources.Scripts.Main.ps1" },
        };

        public MainWindow()
        {
            InitializeComponent();
            AppData.mainWindow = this;
        }

        // Start application
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Check Domain
            try
            {
                domainName = System.DirectoryServices.ActiveDirectory.Domain.GetComputerDomain().ToString();
            }
            catch (System.DirectoryServices.ActiveDirectory.ActiveDirectoryObjectNotFoundException activeDirectoryNotFound)
            {
                DomainJoin domainJoin = new DomainJoin();
                domainJoin.ShowDialog();

                if(!domainJoin.connected) 
                {
                    return;
                }

                username = domainJoin.username;
                password = domainJoin.password;
                domain = domainJoin.domain;
            }
            catch(Exception ex)
            {

                throw;
            }

            // Initalize UI first
            await InitTreeView();

            // Extract any embedded PowerShell Scripts that the application needs in order to run
            await ExtractAll();
            // Loads all the powershell scripts into the powershell instance created
            string[] filePaths = Directory.GetFiles(@"files/scripts", "*.ps1");
            await LoadPowerShellFiles();

            // Initialize powershell with first-start script
            var progress = new Progress<Tuple<int, string>>(data =>
            { // Create progress reporter
                if (data.Item2 != null)
                {
                    // Report
                    Console.WriteLine(data.Item2.ToString());
                    Trace.WriteLine(data.Item2.ToString());
                }
            });
            await RunFunctions("Initialize", progress);
        }

        #region Methods
        public static async Task<bool> ExtractAll()
        {
            bool pass = false;
            await Task.Run(() =>
            {
                try
                {
                    // Checks for files that exits, remove and that are already there for integrity.
                    if (!Directory.Exists("files")) { Directory.CreateDirectory("files"); }
                    if (!Directory.Exists("files/scripts")) { Directory.CreateDirectory("files/scripts"); }
                    //if (File.Exists("./files/AzureAD.nupkg")) { File.Delete("./files/AzureAD.nupkg"); }
                    //if (File.Exists("./files/ExchangeOnline.nupkg")) { File.Delete("./files/ExchangeOnline.nupkg"); }
                    //if (File.Exists("./files/MSOnline.nupkg")) { File.Delete("./files/MSOnline.nupkg"); }

                    // Extract Package to nupkg file
                    for (int i = 0; i < resources.Count; i++)
                    {
                        string resource = resources.Values.ElementAt(i);
                        string fileName = resources.Keys.ElementAt(i);
                        if (!File.Exists(fileName))
                        {
                            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
                            FileStream fileStream = new FileStream(fileName, FileMode.CreateNew);
                            for (int j = 0; j < stream.Length; j++)
                            {
                                fileStream.WriteByte((byte)stream.ReadByte());
                            }
                            fileStream.Close();
                        }
                    }
                    // Extract nupkg to directory
                    //if (!Directory.Exists("files/extract")) { Directory.CreateDirectory("files/extract"); }
                    //if (!File.Exists("files/extract/AzureAD/AzureAD.psd1")) { ZipFile.ExtractToDirectory("files/AzureAD.nupkg", "files/extract/AzureAD"); }
                    //if (!File.Exists("files/extract/ExchangeOnline/ExchangeOnlineManagement.psd1")) { ZipFile.ExtractToDirectory("files/ExchangeOnline.nupkg", "files/extract/ExchangeOnline"); }
                    //if (!File.Exists("files/extract/MSOnline/MSOnline.psd1")) { ZipFile.ExtractToDirectory("files/MSOnline.nupkg", "files/extract/MSOnline"); }
                    //if (!File.Exists("files/extract/MSOnline/MSOnline.psd1")) { ZipFile.ExtractToDirectory("files/MSOnline.nupkg", "files/extract/MSOnline"); }
                    //if (!File.Exists("files/extract/JS-Flappy-Bird-master/index.html")) { ZipFile.ExtractToDirectory("files/JS-Flappy-Bird-master.zip", "files/extract"); }

                    pass = true;
                }
                catch (Exception ex)
                {
                    pass = false;
                    Console.WriteLine(ex);
                }
            });
            return pass;
        }
        private async Task<bool> LoadPowerShellFiles()
        { // Task that loads all of the files in the 'Scripts' folder into the Powershell instance
            string[] filePaths = null;

            await Task.Run(() =>
            {
                // Go through each file in the 'Scripts' folder and add them to the powershell instance
                filePaths = Directory.GetFiles(@"files/scripts", "*.ps1");
                foreach (string path in filePaths)
                {
                    if (!path.Contains(".Task-Sequence.ps1"))
                    {
                        string readText = File.ReadAllText(path);
                        ps.AddScript(readText);

                        Console.WriteLine("Loaded file " + path);
                        Trace.WriteLine("Loaded file " + path);
                    }
                }

                // Also add the users credentials to a variable that PowerShell can access
                // TODO: Add login page
                SecureString securePassword = new NetworkCredential("", "P@ssw0rd").SecurePassword;
                PSCredential credentials = new PSCredential("UPN", securePassword);

                ps.AddCommand("Set-Variable");
                ps.AddParameter("Name", "o365credential");
                ps.AddParameter("Value", credentials);

                ps.Invoke();
            });
            return true;
        }
        private async Task InitTreeView()
        {
            List<ADOrganizationalUnit> adInfo = new List<ADOrganizationalUnit>();

            // Get info from active directory
#pragma warning disable CA1416 // Validate platform compatibility
            // I only intend for this application to be running on Windows, so this is beyond
            // my concern. If someone wants a linux desktop build of this they can use wine.
            DirectoryEntry entry = new DirectoryEntry("LDAP://" + domain, username, password);
            DirectorySearcher searcher = new DirectorySearcher(entry)
            {
                // specify that you search for organizational units 
                Filter = "(objectCategory=organizationalUnit)",
                SearchScope = SearchScope.OneLevel
            };
            foreach (SearchResult result in searcher.FindAll())
            {
                ADOrganizationalUnit item = new ADOrganizationalUnit();
                item.Name = result.Properties["name"].OfType<string>().First(); 
                item.ObjectClass = result.Properties["objectclass"].OfType<string>().First();
                item.DistinguishedName = result.Properties["distinguishedname"].OfType<string>().First(); 
                adInfo.Add(item);
            }
            #pragma warning restore CA1416 // Validate platform compatibility

            if (adInfo == null || adInfo.Count <= 0) { return; }

            DirectoryEntry RootDirEntry = new DirectoryEntry("LDAP://" + domain);
            string distinguishedName = RootDirEntry.Properties["defaultNamingContext"].Value.ToString();

            TreeAsset ParentItem = new TreeAsset();
            ParentItem.Header = distinguishedName;
            ParentItem.ADObject = new ADOrganizationalUnit();
            ParentItem.Name = System.Environment.UserDomainName;

            ParentItem.ADObject.DistinguishedName = distinguishedName;
            ParentItem.ADObject.Name = distinguishedName;
            ParentItem.ADObject.ObjectClass = "organizationalUnit";

            if (TreeView1.SelectedItem != null)
            {
                ParentItem = TreeView1.SelectedItem as TreeAsset;
            }

            await PopulateFromDN(ParentItem, ParentItem.ADObject.DistinguishedName);

            TreeView1.SelectedItemChanged += treeView_SelectedItemChangeAsync;
            TreeView1.Items.Add(ParentItem);
        }
        public async Task<bool> RunFunctions(string func, IProgress<Tuple<int, string>> data)
        { // Runs the powershell function
            bool pass = false;

            await Task.Run(() =>
            {
                try
                {
                    // Clear previous stream information and add the script
                    ps.Streams.ClearStreams();
                    ps.AddScript(func);

                    // prepare a collection for the output, register event handler
                    var output = new PSDataCollection<string>();
                    output.DataAdded += delegate (object sender, DataAddedEventArgs eventArgs)
                    {
                        var collection = sender as PSDataCollection<string>;
                        if (null != collection)
                        {
                            var outputItem = collection[eventArgs.Index];

                            // Here's where you'd update the form with the new output item
                            data.Report(new Tuple<int, string>(-1, outputItem));
                        }
                    };

                    // invoke the command asynchronously - we'll be relying on the event handler to process the output instead of collecting it here
                    var asyncToken = ps.BeginInvoke<object, string>(null, output);

                    if (asyncToken.AsyncWaitHandle.WaitOne())
                    {
                        if (ps.HadErrors)
                        {
                            foreach (var errorRecord in ps.Streams.Error)
                            {
                                // inspect errors here
                                // alternatively: register an event handler for `powerShell.Streams.Error.DataAdded` event
                                if (!errorRecord.Exception.Message.Contains("The input object cannot be bound to any parameters for the command either because the command does not take pipeline input or the input and its properties do not match any of the parameters that take pipeline input"))
                                { // Just check that it doesn't contain this stupid fucking error that shouldn't exist
                                    output.Add(errorRecord.Exception.Message);
                                }
                            }
                        }

                        // end invocation without collecting output (event handler has already taken care of that)
                        ps.EndInvoke(asyncToken);
                    }
                }
                catch (Exception)
                {

                }
            });
            return pass;
        }
        public async Task PopulateFromDN(TreeAsset asset, string DN)
        {
            List<ADOrganizationalUnit> adInfo = ActiveDirectory.GetOUAt(DN);

            if (asset.populated) return;
            if (adInfo == null) { return; }
            if (adInfo.Count == 0) { asset.SetIcon("folder.png"); }

            foreach (ADOrganizationalUnit item in adInfo)
            {
                TreeAsset ChildItem = new TreeAsset();

                ChildItem.ADObject = item;
                ChildItem.Text = item.Name;
                
                ChildItem.Expanded += treeitem_Expanded;

                bool hassSubdirectories = await ChildItem.HasSubdirectories();
                if(hassSubdirectories)
                {
                    ChildItem.SetIcon("folders.png");
                    ChildItem.Items.Add(new TreeAsset());
                }
                else
                {
                    ChildItem.SetIcon("folder.png");
                }

                asset.children.Add(item);
                asset.Items.Add(ChildItem);
            }
            asset.populated = true;

            // remove any empty spaces
            List<TreeAsset> ToRemove = new List<TreeAsset>();
            foreach (TreeAsset item in asset.Items)
            {
                if (item.Header == null)
                { // clear anything that doesn't have data
                    ToRemove.Add(item);
                }
            }
            foreach (TreeAsset item in ToRemove)
            {
                asset.Items.Remove(item);
            }
        }
        #endregion

        #region interactions
        public async void treeView_SelectedItemChangeAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                TreeAsset asset = (TreeAsset)TreeView1.SelectedItem;

                List_SystemBrowse.Items.Clear();

                if (!asset.populated)
                {
                    await PopulateFromDN(asset, asset.ADObject.DistinguishedName);
                }

                // Display OU in Systems List
                List<ADOrganizationalUnit> OUs = asset.children;
                txt_directorylabel.Content = asset.ADObject.DistinguishedName;
                foreach (var organizationalUnit in OUs)
                {
                    List_SystemBrowse.Items.Add(organizationalUnit.Name);
                }
                // Display Systems in List
                List_SystemBrowse.Items.Add("Fetching Assets...");
                List<ADComputer> PCs = new List<ADComputer>();
                await Task.Run(() =>
                {
                    PCs = ActiveDirectory.GetPCAt(asset.ADObject.DistinguishedName);
                });
                foreach (var PC in PCs)
                {
                    List_SystemBrowse.Items.Add(PC.Name);
                }
                List_SystemBrowse.Items.Remove("Fetching Assets...");
            }
            catch (Exception)
            {

            }
        }
        public async void treeitem_Expanded(object sender, RoutedEventArgs e)
        {
            try
            {
                TreeAsset asset = (TreeAsset)sender;

                if(asset.populated) { return; }
                await PopulateFromDN(asset, asset.ADObject.DistinguishedName);

                List<ADOrganizationalUnit> OUs = asset.children;
                txt_directorylabel.Content = asset.ADObject.DistinguishedName;

                foreach (var organizationalUnit in OUs)
                {
                    List_SystemBrowse.Items.Add(organizationalUnit.Name);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion
    }
}
