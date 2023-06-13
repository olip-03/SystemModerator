using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Management.Automation;
using System.Threading.Tasks;
using System.Windows;

namespace SystemModerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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

            // Load computers from Active Directory
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
        
        #endregion
    }
}
