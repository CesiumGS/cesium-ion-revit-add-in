using Autodesk.Revit.DB;
using CesiumIonRevitAddin.Utils;
using System;
using System.Collections.Generic;

namespace CesiumIonRevitAddin.Gltf
{
    using ClassesType = Dictionary<string, object>;
    using ClassType = Dictionary<string, object>;
    using PropertiesType = Dictionary<string, object>;
    using PropertyType = Dictionary<string, object>;
    using SchemaType = Dictionary<string, object>;

    internal class GltfExtStructuralMetadataExtensionSchema : GltfExtensionSchema
    {
        public SchemaType schema = new SchemaType();

        public GltfExtStructuralMetadataExtensionSchema()
        {
            schema.Add("id", "customFromRevitDocument");
            schema.Add("classes", new ClassesType()); // ignore IntelliSense

            var elementClass = new ClassType();
            var elementClassProperties = new PropertiesType();
            elementClass.Add("name", "Element");
            elementClass.Add("properties", elementClassProperties);

            var categoryNameProperty = new PropertyType();
            categoryNameProperty.Add("name", "Category Name");
            categoryNameProperty.Add("type", "STRING");
            categoryNameProperty.Add("required", false);
            elementClassProperties.Add("categoryName", categoryNameProperty);

            var uniqueIdProperty = new PropertyType();
            uniqueIdProperty.Add("name", "UniqueId");
            uniqueIdProperty.Add("type", "STRING");
            uniqueIdProperty.Add("required", false);
            elementClassProperties.Add("uniqueId", uniqueIdProperty);

            var levelIdProperty = new PropertyType();
            levelIdProperty.Add("name", "LevelId");
            levelIdProperty.Add("type", "STRING");
            levelIdProperty.Add("required", false);
            elementClassProperties.Add("levelId", levelIdProperty);

            var classes = GetClasses();
            classes.Add("element", elementClass);
        }

        public void AddCategory(string categoryName)
        {
            var addedClass = AddClass(categoryName);
            addedClass.Add("parent", "element");
        }

        public ClassType GetClass(string gltfClassName)
        {
            var classes = GetClasses();
            return classes.TryGetValue(gltfClassName, out var value) ? (ClassType)value : new Dictionary<string, object>();
        }

        public ClassesType GetClasses()
        {
            return (ClassesType)schema["classes"];
        }

        public PropertiesType GetProperties(ClassType class_)
        {
            if (!class_.TryGetValue("properties", out var properties))
            {
                properties = new PropertiesType();
                class_.Add("properties", properties);
            }
            return (PropertiesType)properties;
        }

        public ClassType AddClass(string className)
        {
            ClassesType schemaClasses = GetClasses();
            var gltfClass = new Dictionary<string, object>();

            var gltfName = CesiumIonRevitAddin.Utils.Util.GetGltfName(className);
            if (!schemaClasses.ContainsKey(gltfName))
            {
                gltfClass = new Dictionary<string, object>
                {
                    { "name", className }
                };

                schemaClasses.Add(gltfName, gltfClass);
            }

            return gltfClass;
        }

        public ClassType AddFamily(string categoryName, string familyName)
        {
            var className = Util.CreateClassName(categoryName, familyName);
            var addedClass = AddClass(className);
            addedClass.Add("parent", Util.GetGltfName(categoryName));
            return addedClass;
        }

        // see https://github.com/CesiumGS/glTF/blob/0dc2f8e299a26f3544d26d8aefbc893b08fc5037/extensions/2.0/Vendor/EXT_structural_metadata/schema/class.property.schema.json
        public void AddProperties(string categoryName, string familyName, Autodesk.Revit.DB.ParameterSet parameterSet, HashSet<Parameter> parametersToSkip)
        {
            var gltfClassName = Util.GetGltfName(Util.CreateClassName(categoryName, familyName));
            ClassType class_ = GetClass(gltfClassName);
            PropertiesType schemaProperties = GetProperties(class_);

            foreach (Parameter parameter in parameterSet)
            {
                if (parametersToSkip.Contains(parameter)) continue;

                string gltfPropertyName = Util.GetGltfName(parameter.Definition.Name);

                // do not add the parameter if the category has it
                var categoryClass = GetClass(Util.GetGltfName(categoryName));
                if (ClassHasProperty(categoryClass, gltfPropertyName))
                {
                    continue;
                }

                if (!schemaProperties.ContainsKey(gltfPropertyName))
                {
                    schemaProperties.Add(gltfPropertyName, new PropertyType());
                    var schemaProperty = (PropertyType)schemaProperties[gltfPropertyName];

                    // name
                    schemaProperty.Add("name", parameter.Definition.Name);

                    // type
                    var storageType = parameter.StorageType;
                    switch (storageType)
                    {
                        case StorageType.None:
                            {
                                schemaProperty.Add("type", "STRING");
                                break;
                            }
                        case StorageType.String:
                            schemaProperty.Add("type", "STRING");
                            break;
                        case StorageType.Integer:
                        case StorageType.ElementId:
                            schemaProperty.Add("type", "SCALAR");
                            schemaProperty.Add("componentType", "INT32");
                            break;
                        case StorageType.Double:
                            schemaProperty.Add("type", "SCALAR");
                            schemaProperty.Add("componentType", "FLOAT32");
                            break;
                        default:
                            break;
                    }

                    schemaProperty.Add("required", IsRequired(parameter.Definition.Name));
                }
            }
        }

        public void AddDefaultSchemaProperty(string categoryName, string familyName, string propertyName, string defaultValue, string name)
        {
            var gltfClassName = Util.GetGltfName(Util.CreateClassName(categoryName, familyName));
            ClassType gltfClass = GetClass(gltfClassName);
            PropertiesType schemaProperties = GetProperties(gltfClass);
            if (!schemaProperties.ContainsKey(propertyName))
            {
                schemaProperties.Add(propertyName, new PropertyType());
                var schemaProperty = (PropertyType)schemaProperties[propertyName];
                schemaProperty.Add("name", name);
                schemaProperty.Add("type", "STRING");
                schemaProperty.Add("default", defaultValue);
            }
        }

        public void AddSchemaProperty(string categoryName, string familyName, string propertyName, Type propertyType)
        {
            var gltfClassName = Util.GetGltfName(Util.CreateClassName(categoryName, familyName));
            ClassType gltfClass = GetClass(gltfClassName);
            PropertiesType schemaProperties = GetProperties(gltfClass);
            if (!schemaProperties.ContainsKey(propertyName))
            {
                var schemaProperty = new PropertyType();
                schemaProperty.Add("name", propertyName);

                bool typeHandled = true;

                if (propertyType == typeof(string))
                {
                    schemaProperty.Add("type", "STRING");
                }
                else if (propertyType == typeof(int) || propertyType == typeof(bool))
                {
                    schemaProperty.Add("type", "SCALAR");
                    schemaProperty.Add("componentType", "INT32");
                }
                else if (propertyType == typeof(double) || propertyType == typeof(float))
                {
                    schemaProperty.Add("type", "SCALAR");
                    schemaProperty.Add("componentType", "FLOAT32");
                } else
                {
                    typeHandled = false;
                }

                if (typeHandled)
                {
                    schemaProperties.Add(propertyName, schemaProperty);
                } else
                {
                    Logger.Instance.Log("Error: Cannot handle parameter type " + propertyType + " for property " + propertyName);
                }
            }
        }

        public bool ClassHasProperty(ClassType class_, string propertyGltfName)
        {
            // Some schemaClasses may have no properties, such as schemaClasses for Revit Categories
            if (!class_.ContainsKey("properties"))
            {
                return false;
            }

            var schemaProperties = GetProperties(class_);
            return schemaProperties.ContainsKey(propertyGltfName);
        }


#pragma warning disable S125 // Sections of code should not be commented out
        // private static readonly HashSet<string> requiredParameters;
#pragma warning restore S125 // Sections of code should not be commented out
        public static bool IsRequired(string categoryName)
        {
#pragma warning disable S125 // Sections of code should not be commented out
            // skip marking parameters as "required" for the present

            //if (requiredParameters == null)
            //{
            //    requiredParameters = new HashSet<string>
            //    {
            //        "Category",
            //        "Level",
            //        "Family and Type",
            //        "Family",
            //        "Type",
            //        "Family Name",
            //        "Type Name",
            //        "Type Id"
            //    };
            //}

            //if (requiredParameters.Contains(categoryName)) return true;
#pragma warning restore S125 // Sections of code should not be commented out

            return false;
        }
    }
}
