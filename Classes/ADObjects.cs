using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemModerator.Classes
{
    public static class ActiveDirectory
    {
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

    public class ADOrganizationalUnit
    {
        public string Name { get; set; }
        public string ObjectClass { get; set; }
        public string DistinguishedName { get; set; }
    }
    public class ADComputer
    {
        public string Name { get; set; }
        public string ObjectClass { get; set; }
        public string DistinguishedName { get; set; }

    }
}
