using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Protobuild
{
    public class ModuleVersion
    {
        public static readonly ModuleVersion Unversioned = ModuleVersion.CreateNew(0, 0, 0, "Unversioned");

        public static ModuleVersion CreateNew(int major, int minor, int patch, string fragment)
        {
            
        }

        public static ModuleVersion Parse(string @string)
        {
            var regex = new Regex("^(?<major>[0-9]+)\\.(?<minor>[0-9]+)\\.(?<patch>[0-9]+)");
        }

        private ModuleVersion(int major, int minor, int patch, string fragment)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Fragment = fragment;
        }

        /// <summary>
        /// The major component of the version number; the X in "X.0.0".
        /// </summary>
        public int Major { get; }

        /// <summary>
        /// The minor component of the version number; the X in "0.X.0".
        /// </summary>
        public int Minor { get; }

        /// <summary>
        /// The patch component of the version number; the X in "0.0.X".
        /// </summary>
        public int Patch { get; }

        /// <summary>
        /// The fragment component of the version number; the X in "0.0.0-X".  This component of the
        /// version number is omitted if null or an empty string.
        /// </summary>
        public string Fragment { get; }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Fragment))
            {
                return Major + "." + Minor + "." + Patch;
            }

            return Major + "." + Minor + "." + Patch + "-" + Fragment;
        }
    }
}
