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

namespace SystemModerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<Asset> Assets = new List<Asset>();

        private PowerShell ps = PowerShell.Create();
        private static Dictionary<string, string> resources = new Dictionary<string, string>()
        {
            { "files/scripts/Main.ps1", "SystemModerator.Resources.Scripts.Main.ps1" },
        };

        public MainWindow()
        {
            InitializeComponent();
        }

        // Start 
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
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

            // Load computers from Active Directory
            progress = new Progress<Tuple<int, string>>(data =>
            { // Update progress reporter
                if (data.Item2 != null)
                {
                    // Get all the  JSON in here
                    List<Asset> models = JsonConvert.DeserializeObject<List<Asset>>(data.Item2.ToString());
                    for (int i = 0; i < models.Count; i++)
                    {
                        Assets.Insert(i, models[i]);
                    }
                }
            });
            await RunFunctions("GetResources", progress);
            await PopulateTreeView();
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
        private async Task<bool> RunFunctions(string func, IProgress<Tuple<int, string>> data)
        { // Runs the powershell function
            bool pass = false;

            await Task.Run(() =>
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
            });
            return pass;
        }
        private async Task PopulateTreeView()
        {
            TreeView1.Items.Clear();

            TreeViewItem ParentItem = new TreeViewItem();
            ParentItem.Header = "Computers";
            TreeView1.Items.Add(ParentItem);
            // Distinguished name should look like this:
            // "CN=F27616,OU=Windows10_MOE,OU=Windows10,OU=Perth,OU=Computers,OU=MGL,DC=monadelphous,DC=com,DC=au"
            foreach (Asset asset in Assets) 
            {
                TreeViewItem Child1Item = new TreeViewItem();
                List<String> DNInfo = asset.DistinguishedName.Split(',').ToList();
                for (int i = 0; i < DNInfo.Count(); i++)
                {
                    string item = DNInfo[i];
                    if(item.Contains("CN="))
                    { // This is the item name
                        item.Replace("CN=", "");
                        DNInfo.RemoveAt(i);
                    }
                    if (item.Contains("OU="))
                    { // This is part of where it is located
                        item.Replace("OU=", "");
                        
                    }
                    if (item.Contains("DC="))
                    { // This is the part of the domain controller
                        item.Replace("DC=", "");
                        DNInfo.RemoveAt(i);
                    }
                }
                Child1Item.Header = asset.Name;
                ParentItem.Items.Add(Child1Item);
            }
        }
        #endregion
    }
}
