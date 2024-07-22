using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CesiumIonRevitAddin
{
    internal class Preferences
    {
        // TODO: use user-defined vals
        public bool Materials { get; set; } = true;
        public bool Textures { get; set; } = false;
        // public FormatEnum format = FormatEnum::gltf;
        // TODO: use user-defined vals
        public bool Normals { get; set; } = true;
        public bool Levels { get; set; }
        public bool Properties { get; set; }
        public bool RelocateTo0 { get; set; }
        public bool FlipAxis { get; set; } = true;

        // TODO: needed?
        //#if REVIT2019 || REVIT2020\
        //        DisplayUnitType units;
        //#else
        //        ForgeTypeId units;
        //
        //#endif

        // TODO: use user-defined vals
        public string path = "C:\\Scratch\\CesiumIonRevitAddin\\outfile";
        public string OutputDirectory
        {
            get
            {
                return Path.GetDirectoryName(path);
            }
        }

        public string FileName { get; set; } = "outfile";
    }
}
