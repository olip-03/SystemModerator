using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemModerator.Classes
{
    public enum TreeType
    {
        Unset,
        Asset,
        Directory
    }
    public class TreeAsset: IComparable<TreeAsset>
    { // Default values for unset class
        public string name { get; set; } = string.Empty;
        public string path { get; set; } = string.Empty;
        public int assetIndex { get; set; } = -1;
        public TreeType type { get; set; } = TreeType.Unset;

        public int CompareTo(TreeAsset other)
        {
            if(other.name != null)
            { // search names
                return this.name.CompareTo(other.name);
            }
            if(other.path != null)
            { // search path
                return this.path.CompareTo(other.path);
            }
            if(type != TreeType.Unset)
            { // search type
                return this.type.CompareTo(other.type);
            }
            return -1;
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
