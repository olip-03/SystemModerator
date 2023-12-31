﻿using Newtonsoft.Json;
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
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Markup;
using System.Xml.Linq;
using System.Xaml;
using System.Windows.Resources;

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
        public string Text { get; set; }
        public List<ADOrganizationalUnit> children { get; set; } = new List<ADOrganizationalUnit>();
        public bool populated { get; set; } = false;
        public bool hasSubDirectories = false;
        public bool ignoreExpansionPopulation = false;
        private string currentIcon = null;
        public void SetXamlIcon(string imagePath)
        {
            currentIcon = imagePath;
            if (currentIcon == null) { return; }

            // create stack panel
            StackPanel stack = new StackPanel();
            stack.MaxHeight = 20;
            stack.Orientation = Orientation.Horizontal;

            // Load Image
            Viewbox canvas = new Viewbox();
            StreamResourceInfo sri = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Resources/VSIcons/" + imagePath));
            if (sri != null)
            {
                using (Stream s = sri.Stream)
                {
                    canvas = (Viewbox)System.Windows.Markup.XamlReader.Load(s);
                }
            }
            canvas.VerticalAlignment = VerticalAlignment.Center;
            canvas.Height = 16;
            canvas.Width = 16;

            // Label
            TextBlock lbl = new TextBlock();
            Thickness margin = lbl.Margin;
            margin.Left = 5;
            lbl.Margin = margin;
            lbl.Text = this.Text;
            lbl.VerticalAlignment = VerticalAlignment.Center;
            lbl.MaxHeight = 20;

            // Add into stack
            stack.Children.Add(canvas);
            stack.Children.Add(lbl);

            // assign stack to header
            this.Header = stack;
        }
        public void SetIcon(string imagePath)
        {
            currentIcon = imagePath;
            if(currentIcon == null) { return; }

            // create stack panel
            StackPanel stack = new StackPanel();
            stack.MaxHeight = 20;
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
            this.Header = stack;
        }
        public bool HasSubdirectories()
        {
            bool pass = false;

            DirectoryEntry entry = new DirectoryEntry("LDAP://" + ADObject.DistinguishedName);

            if (String.IsNullOrWhiteSpace(ADObject.DistinguishedName))
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
                pass = true;
                break;
            }
            hasSubDirectories = pass;
            return pass;
        }
        public async Task<bool> HasSubdirectoriesAsync()
        {
            bool pass = false;
            await Task.Run(() => {
                DirectoryEntry entry = new DirectoryEntry("LDAP://" + ADObject.DistinguishedName);

                if (String.IsNullOrWhiteSpace(ADObject.DistinguishedName))
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
                    pass = true;
                    break;
                }
            });
            hasSubDirectories = pass;
            return pass;
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
