using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SystemModerator.Classes
{
    public enum TreeType
    {
        Unset,
        Asset,
        Directory
    }
    public class TreeAsset: TreeViewItem
    { // Default values for unset class
        public ADOrganizationalUnit ADObject { get; set; }
        public bool populated { get; set; } = false;
        public async Task PopulateFromDN(string DN)
        {
            if (populated) return;
            List<ADOrganizationalUnit> adInfo = new List<ADOrganizationalUnit>();
            #pragma warning disable CA1416 // Validate platform compatibility
            // I only intend for this application to be running on Windows, so this is beyond
            // my concern. If someone wants a linux desktop build of this they can use wine.'
            DirectoryEntry entry = new DirectoryEntry("LDAP://" + ADObject.DistinguishedName);
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
            if (adInfo == null) { return; }

            foreach (ADOrganizationalUnit item in adInfo)
            {
                TreeAsset ChildItem = new TreeAsset();

                ChildItem.ADObject = item;
                ChildItem.Header = item.Name;

                ChildItem.Selected += treeItem_SelectedAsync;
                ChildItem.MouseDoubleClick += treeItem_SelectedAsync;
                ChildItem.Expanded += treeItem_SelectedAsync;

                ChildItem.Items.Add(new TreeAsset());

                this.Items.Add(ChildItem);
            }
            populated = true;
        }
        public async void treeItem_SelectedAsync(object sender, RoutedEventArgs e)
        {
            TreeAsset asset = (TreeAsset)sender;
            if (asset.populated) { return; }

            if (e.RoutedEvent.Name != "Selected")
            {
                asset.IsSelected = true;
            }

            await asset.PopulateFromDN(asset.ADObject.DistinguishedName);

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
    }
    public class Asset
    {
        public string DistinguishedName { get; set; }
        public string DNSHostName { get; set; }
        public string Enabled { get; set; }
        public string Name { get; set; }
        public string ObjectClass { get; set; }
        public string ObjectGUID { get; set; }
        public string SamAccountName { get; set; }
        public string UserPrincipalName { get; set; }
    }
}
