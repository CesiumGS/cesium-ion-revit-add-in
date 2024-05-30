using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CesiumIonRevitAddin
{
    internal class Preferences
    {
        // TODO: use user-defined vals
        public bool materials = true;
        // public FormatEnum format = FormatEnum::gltf;
        // TODO: use user-defined vals
        public bool normals = false;
        public bool levels;
        public bool batchId;
        public bool properties;
        public bool relocateTo0;
        public bool flipAxis = true;
        public bool cesiumNativeExport = false;

        // TODO: needed?
        //#if REVIT2019 || REVIT2020\
        //        DisplayUnitType units;
        //#else
        //        ForgeTypeId units;
        //
        //#endif

        // TODO: use user-defined vals
        string path = "C:\\Scratch\\CesiumIonRevitAddin\\outfile";
        string fileName = "outfile";
    }
}
