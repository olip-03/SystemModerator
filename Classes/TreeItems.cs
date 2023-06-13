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
    internal class TreeAsset: IComparable<TreeAsset>
    {
        public string name { get; set; } = string.Empty;
        public string path { get; set; } = string.Empty;
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
}
