using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
        public List<ADOrganizationalUnit> children { get; set; } = new List<ADOrganizationalUnit>();
        public bool populated { get; set; } = false;
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
