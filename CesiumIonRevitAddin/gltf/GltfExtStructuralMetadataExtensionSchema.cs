﻿using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using CesiumIonRevitAddin.Utils;

namespace CesiumIonRevitAddin.gltf
{
    using ClassesType = Dictionary<string, object>;
    using ClassType = Dictionary<string, object>;
    using SchemaType = Dictionary<string, object>;
    using PropertyType = Dictionary<string, object>;
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

        public ClassType AddFamily(string categoryName, string familyName)
        {
            var className = Util.CreateClassName(categoryName, familyName);
            var class_ = AddClass(className);
            class_.Add("parent", Util.GetGltfName(categoryName));
            return class_;
        }

        // see https://github.com/CesiumGS/glTF/blob/0dc2f8e299a26f3544d26d8aefbc893b08fc5037/extensions/2.0/Vendor/EXT_structural_metadata/schema/class.property.schema.json
        public void AddProperties(string categoryName, string familyName, Autodesk.Revit.DB.ParameterSet parameterSet)
        {
            var gltfClassName = Util.GetGltfName(Util.CreateClassName(categoryName, familyName));
            ClassType class_ = GetClass(gltfClassName);
            PropertiesType schemaProperties = GetProperties(class_);

            foreach (Parameter parameter in parameterSet) {
                string gltfPropertyName = Util.GetGltfName(parameter.Definition.Name);

                // do not add the parameter if the parent category has it
                var categoryClass = GetClass(Util.GetGltfName(categoryName));
                if (ClassHasProperty(categoryClass, gltfPropertyName)) continue;

                if (!schemaProperties.ContainsKey(gltfPropertyName))
                {
                    schemaProperties.Add(gltfPropertyName, new PropertyType());
                    var schemaProperty = (PropertyType) schemaProperties[gltfPropertyName];

                    // name
                    schemaProperty.Add("name", parameter.Definition.Name);

                    // type
                    var storageType = parameter.StorageType;
                    switch (storageType)
                    {
                        case StorageType.None:
                            {
                                // TODO
                                schemaProperty.Add("type", "None");
                                break;
                            }
                        case StorageType.String:
                        case StorageType.ElementId:
                            schemaProperty.Add("type", "STRING");
                            break;
                        case StorageType.Integer:
                            schemaProperty.Add("type", "SCALAR");
                            schemaProperty.Add("componentType", "INT32");
                            break;
                        case StorageType.Double:
                            schemaProperty.Add("type", "SCALAR");
                            schemaProperty.Add("componentType", "FLOAT32");
                            break;
                        default:
                            // TODO
                            break;
                    }

                    schemaProperty.Add("required", IsRequired(parameter.Definition.Name));
                }
            }
        }

        bool ClassHasProperty(ClassType class_, string propertyGltfName)
        {
            var schemaProperties = GetProperties(class_);
            return schemaProperties.ContainsKey(propertyGltfName);
        }

        static HashSet<string> requiredParameters;
        static bool IsRequired(string categoryName)
        {
            if (requiredParameters == null)
            {
                requiredParameters = new HashSet<string>
                {
                    "Category",
                    "Level",
                    "Family and Type",
                    "Family",
                    "Type",
                    "Family Name",
                    "Type Name",
                    "Type Id"
                };
            }

            if (requiredParameters.Contains(categoryName)) return true;

            return false;
        }
    }
}