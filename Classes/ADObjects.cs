using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows;
using System.IO;
using System.Windows.Resources;
using System.Diagnostics;
using System.Drawing;

namespace SystemModerator.Classes
{
    public static class ActiveDirectory
    {
        public enum ObjectTypes
        {
            OrganizationalUnit,
            Computer,
            User,
            Group,
            ForeignSecurityPrincipal,
            All
        }

        /// <summary>
        /// Class containing all methods for managing Organizational Units in Active Directory
        /// </summary>
        public class OrganizationalUnits
        {

        }
        /// <summary>
        /// Class containing all methods for managing Computers in Active Directory
        /// </summary>
        public class Computers
        {

        }
        /// <summary>
        /// Class containing all methods for managing users in Active Directory
        /// </summary>
        public class Users
        {

        }
        
        public static List<ADObject> FetchADObjects(string at, ObjectTypes type)
        {
            string search = "LDAP://" + at;
            string filter = "";
            switch (type)
            {
                case ObjectTypes.OrganizationalUnit:
                    filter = "organizationalUnit";
                    break;
                case ObjectTypes.Computer:
                    filter = "computer";
                    break;
                case ObjectTypes.User:
                    filter = "user";
                    break;
                case ObjectTypes.Group:
                    filter = "group";
                    break;
                case ObjectTypes.ForeignSecurityPrincipal:
                    filter = "foreignSecurityPrincipal";
                    break;
                default:
                    filter = "";
                    break;
            }

            #pragma warning disable CA1416 // Validate platform compatibility
            // I only intend for this application to be running on Windows, so this is beyond
            // my concern. If someone wants a linux desktop build of this they can use wine.'

            // TODO: Redo data loading in from AD so its presented correctly, 

            // Fetch all OU's
            List<ADObject> adInfo = new List<ADObject>();

            DirectoryEntry entry = new DirectoryEntry(search);
            DirectorySearcher searcher = new DirectorySearcher
            {
                // specify that you search for organizational units 
                SearchRoot = entry,
                Filter = filter,
                SearchScope = SearchScope.OneLevel
            };

            foreach (SearchResult result in searcher.FindAll())
            {
                string name = result.Properties["name"].OfType<string>().First();
                string distinguishedName = result.Properties["distinguishedname"].OfType<string>().First();

                string[] objectClass = (string[])result.Properties["objectClass"].OfType<string>();

                ADObject newObject = new(name, objectClass, distinguishedName);

                adInfo.Add(newObject);
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
            SetXamlIcon(currentIcon);
        }
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
            this.Content = stack;
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
        public string Name;
        public string[] ObjectClass;
        public string DistinguishedName;
        public string icon = null;
        public ADObject()
        {

        }
        public ADObject(string _name, string[] _ObjectClass, string _DistinguishedName)
        {
            Name = _name;
            ObjectClass = _ObjectClass;
            DistinguishedName = _DistinguishedName;
            CheckType();
        }
        public bool isContainer
        {
            get { return IsContainer(); }
            private set { }
        }
        public bool isGroup
        {
            get { return IsGroup(); }
            private set { }
        }
        public bool isUser
        {
            get { return IsUser(); }
            private set { }
        }
        public bool isComputer
        {
            get { return IsComputer(); }
            private set { }
        }

        public void CheckType()
        {
            IsContainer();
            IsGroup();
            IsUser();
            IsComputer();
        }
        private bool IsContainer()
        {
            string ContainerIcon = "FolderClosed/FolderClosed_16x.xaml";

            if (ObjectClass.Contains("organizationalUnit")) { icon = ContainerIcon; return true; }
            if(ObjectClass.Contains("container")) { icon = ContainerIcon; return true; }
            if(ObjectClass.Contains("builtinDomain")) { icon = ContainerIcon; return true; }

            return false;
        }
        private bool IsGroup()
        {
            string GroupIcon = "UserGroup/UserGroup_16x.xaml";

            if (ObjectClass.Contains("group")) { icon = GroupIcon; return true; }
            return false;
        }
        private bool IsUser()
        {
            string UserIcon = "User/User_16x.xaml";

            if (ObjectClass.Contains("user")) { icon = UserIcon; return true; }

            if (ObjectClass.Contains("computer")) { return false; }
            return false;
        }
        private bool IsComputer()
        {
            string ComputerIcon = "Computer/Computer_16x.xaml";

            if (ObjectClass.Contains("computer")) { icon = ComputerIcon;  return true; }

            return false;
        }
    }
    public class ADOrganizationalUnit : ADObject
    {
        public new string[] ObjectClass = { "organizationalUnit" };
    }
    public class ADComputer: ADObject
    {
        public new string[] ObjectClass = { "computer" };
    }
    public class ADUser: ADObject
    {
        public new string[] ObjectClass = { "user" };
    }
}
