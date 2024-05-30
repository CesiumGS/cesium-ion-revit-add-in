using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CesiumIonRevitAddin.gltf
{
    using ClassesType = Dictionary<string, object>;
    using ClassType = Dictionary<string, object>;
    using SchemaType = Dictionary<string, object>;
    using PropertiesType = Dictionary<string, object>;

    internal class GltfExtStructuralMetadataExtensionSchema
    {
        public SchemaType schema = new SchemaType();

        public void AddCategory(string categoryName)
        {
            var addedClass = AddClass(categoryName);
            addedClass.Add("parent", "element");
        }

        public ClassType GetClass(string gltfClassName)
        {
            var classes = GetClasses();
            return (ClassType) classes[gltfClassName];
        }

        public ClassesType GetClasses()
        {
            return (ClassesType) schema["classes"];
        }

        public PropertiesType GetProperties(ClassType class_)
        {
            if (!class_.ContainsKey("properties"))
            {
                class_.Add("properties", new PropertiesType());
            }
            return (PropertiesType) class_["properties"];
        }

        private ClassType AddClass(string className)
        {
            ClassesType classes = GetClasses();
            var gltfClass = new Dictionary<string, object>();

            var gltfName = CesiumIonRevitAddin.Utils.Util.GetGltfName(className);
            if (!classes.ContainsKey(gltfName))
            {
                gltfClass = new Dictionary<string, object>();
                gltfClass.Add("name", className);
                // TODO
                //gltfClass->Add("properties", gcnew PropertiesType());
                //PropertiesType^ properties = safe_cast<PropertiesType^>(gltfClass["properties"]);

                classes.Add(gltfName, gltfClass);
            }

            return gltfClass;
        }
    }
}
