using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows;

namespace SystemModerator.Classes
{
    public static class ActiveDirectory
    {
        public static Dictionary<string, string> GetComputer(string DN)
        {
            //Get the Container for the Computer
            DirectoryEntry deContainer = new DirectoryEntry("LDAP://" + DN);

            Dictionary<string, string> pcData = new Dictionary<string, string>();
            foreach (var item in deContainer.Properties)
            {
                string test = item.ToString(); 
                //pcData.Add(item, item);
            }
            return pcData;
        }
        public static List<ADOrganizationalUnit> GetOUAt()
        {
            return GetOUAt("");
        }
        public static List<ADOrganizationalUnit> GetOUAt(string searchbase)
        {
            string search = "LDAP://" + searchbase;

            #pragma warning disable CA1416 // Validate platform compatibility
            // I only intend for this application to be running on Windows, so this is beyond
            // my concern. If someone wants a linux desktop build of this they can use wine.'

            // Fetch all OU's
            List<ADOrganizationalUnit> adInfo = new List<ADOrganizationalUnit>();
            DirectoryEntry entry = new DirectoryEntry(search);
            if (String.IsNullOrWhiteSpace(searchbase))
            {
                entry = new DirectoryEntry();
            }
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
                adInfo.Add(item);
            }
            #pragma warning restore CA1416 // Validate platform compatibility
            return adInfo;
        }
        public static List<ADComputer> GetPCAt(string searchbase)
        {
            string search = "LDAP://" + searchbase;

            #pragma warning disable CA1416 // Validate platform compatibility
            // I only intend for this application to be running on Windows, so this is beyond
            // my concern. If someone wants a linux desktop build of this they can use wine.'

            // Fetch all OU's
            List<ADComputer> adInfo = new List<ADComputer>();
            DirectoryEntry entry = new DirectoryEntry(search);
            if (String.IsNullOrWhiteSpace(searchbase))
            {
                entry = new DirectoryEntry();
            }
            DirectorySearcher searcher = new DirectorySearcher
            {
                // specify that you search for organizational units 
                SearchRoot = entry,
                Filter = "(objectCategory=computer)",
                SearchScope = SearchScope.OneLevel
            };

            foreach (SearchResult result in searcher.FindAll())
            {
                ADComputer item = new ADComputer();
                item.Name = result.Properties["name"].OfType<string>().First();
                item.ObjectClass = result.Properties["objectclass"].OfType<string>().First();
                item.DistinguishedName = result.Properties["distinguishedname"].OfType<string>().First();
                adInfo.Add(item);
            }
            #pragma warning restore CA1416 // Validate platform compatibility
            return adInfo;
        }
    }
    // These names fucking SUCK 
    // Come up with something better PLEASE
    public class ADListItem : ListViewItem
    {
        public string Text;
        public ADOrganizationalUnit ADObject { get; set; }

        private string currentIcon;
        public ADListItem(string text, string currentIcon)
        {
            Text = text;
            SetIcon(currentIcon);
        }

        public void SetIcon(string imagePath)
        {
            currentIcon = imagePath;

            // create stack panel
            StackPanel stack = new StackPanel();
            stack.MaxHeight = 20;
            stack.Height = 20;
            stack.Orientation = Orientation.Horizontal;

            // create Image
            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            image.Source = new BitmapImage
                (new Uri("pack://application:,,,/Resources/TablerIcons/" + imagePath));
            image.Width = 20;
            image.Height = 20;

            // Label
            TextBlock lbl = new TextBlock();
            Thickness margin = lbl.Margin;
            margin.Left = 5;
            lbl.Margin = margin;
            lbl.Text = this.Text;
            lbl.VerticalAlignment = VerticalAlignment.Center;
            lbl.MaxHeight = 20;

            // Add into stack
            stack.Children.Add(image);
            stack.Children.Add(lbl);

            // assign stack to header
            this.Content = stack;
            this.Height = 25;
        }
    }
    public class ADObject
    {
        public string Name { get; set; }
        public string ObjectClass { get; set; }
        public string DistinguishedName { get; set; }
    }
    public class ADOrganizationalUnit: ADObject
    {
        public string ObjectClass = "organizationalUnit";
    }
    public class ADComputer: ADObject
    {
        public string ObjectClass = "computer";

    }
}
