using Autodesk.Revit.DB;
using CesiumIonRevitAddin.Export;
using CesiumIonRevitAddin.Model;
using CesiumIonRevitAddin.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Media3D;

namespace CesiumIonRevitAddin.Gltf
{
    internal class GltfExportContext : IPhotoRenderContext
    {
        private const char UNDERSCORE = '_';
        private readonly bool verboseLog = true;
        private readonly Preferences preferences;
        private readonly View view;
        private Autodesk.Revit.DB.Element element;
        private readonly List<Document> documents = new List<Document>();
        private bool cancelation;
        private readonly Stack<Autodesk.Revit.DB.Transform> transformStack = new Stack<Autodesk.Revit.DB.Transform>();
        private GltfNode xFormNode;
        private readonly List<GltfAccessor> accessors = new List<GltfAccessor>();
        private readonly List<GltfBufferView> bufferViews = new List<GltfBufferView>();
        private readonly List<GltfBuffer> buffers = new List<GltfBuffer>();
        private readonly List<GltfBinaryData> binaryFileData = new List<GltfBinaryData>();
        private readonly List<GltfScene> scenes = new List<GltfScene>();
        private readonly IndexedDictionary<GltfNode> nodes = new IndexedDictionary<GltfNode>();
        private readonly IndexedDictionary<GltfMesh> meshes = new IndexedDictionary<GltfMesh>();
        private readonly IndexedDictionary<GltfMaterial> materials = new IndexedDictionary<GltfMaterial>();
        private readonly IndexedDictionary<GltfImage> images = new IndexedDictionary<GltfImage>();
        private readonly IndexedDictionary<GltfSampler> samplers = new IndexedDictionary<GltfSampler>();
        private readonly IndexedDictionary<GltfTexture> textures = new IndexedDictionary<GltfTexture>();
        private List<string> extensionsUsed = null;
        private List<string> extensionsRequired = null;
        private readonly Dictionary<string, GltfExtensionSchema> extensions = new Dictionary<string, GltfExtensionSchema>();
        private readonly GltfExtStructuralMetadataExtensionSchema modelExtStructuralMetadataExtensionSchema = new GltfExtStructuralMetadataExtensionSchema();
        private bool khrTextureTransformAdded;
        private bool shouldSkipElement;
        private Autodesk.Revit.DB.Transform linkTransformation;
        private IndexedDictionary<GeometryDataObject> currentGeometry;
        private IndexedDictionary<VertexLookupIntObject> currentVertices;
        private bool materialHasTexture;
        /*
         * Usually, Revit elements export with this stack:
         *  - OnElementBegin
         *    - OnInstanceBegin
         *      - OnPolymesh
         *      - ...
         *    - OnInstanceEnd
         *    - OnInstanceBegin
         *      - OnPolymesh
         *    - OnInstanceEnd
         *    - ...
         *  - OnElementEnd
         *
         *  However, sometimes they export with nothing happening in between OnInstanceBeginAndOnInstanceEnd:
         *  - OnElementBegin
         *    - OnInstanceBegin
         *    - OnInstanceEnd
         *    - OnPolymesh
         *  - OnElementEnd
         *
         *  Revit expects you multiple the transform stack with the verts during OnPolymesh.
         *  So when we add the transform to the points, we need two methods of handling the transform stack.
         */
        private bool onPolymeshFired = false;
#if !REVIT2019 && !REVIT2020 && !REVIT2021 && !REVIT2022
        private readonly Dictionary<string, int> symbolGeometryIdToGltfNode = new Dictionary<string, int>();
#endif

        public GltfExportContext(Document doc, Preferences preferences)
        {
            documents.Add(doc);
            view = doc.ActiveView;

            this.preferences = preferences;
        }

        private Document Doc
        {
            get
            {
                // return linked doc if count > 1
                return documents.Count == 1 ? documents[0] : documents[1];
            }
        }

        private static void Reset() => RevitMaterials.materialIdDictionary.Clear();


        // Classes like SpecTypeId.String contain Text, URI, and more.
        // Rather than test for each individually, we get all properties of the class and check against those.
        static bool IsSpecTypeMatch(ForgeTypeId forgeTypeId, Type specTypeClass)
        {
            if (!specTypeClass.IsClass)
            {
                throw new ArgumentException("The provided type must be a class containing ForgeTypeId properties.", nameof(specTypeClass));
            }

            // Retrieve all public static properties of type ForgeTypeId in the specified class
            var specTypeIds = specTypeClass.GetProperties(BindingFlags.Public | BindingFlags.Static)
                                           .Where(prop => prop.PropertyType == typeof(ForgeTypeId))
                                           .Select(prop => (ForgeTypeId)prop.GetValue(null));

            return specTypeIds.Contains(forgeTypeId);
        }

        readonly System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        public bool Start()
        {
            stopwatch.Start();

            // Always set to true until Revit 2022 is phased out.
            preferences.SymbolicInstancing = true;
#if REVIT2019 || REVIT2020 || REVIT2021 || REVIT2022
            // Disable instancing for Revit 2022 and earlier, since it does not support GeometrySymbolId.
            preferences.SymbolicInstancing = false;
#endif

            Logger.Enabled = verboseLog;
            cancelation = false;

            Logger.Instance.Log("Beginning export.");

            Reset();

            // Create the glTF temp export directory.
            if (!Directory.Exists(preferences.TempDirectory))
            {
                Directory.CreateDirectory(preferences.TempDirectory);
            }

            float scale = 0.3048f; // Decimal feet to meters

            transformStack.Push(Autodesk.Revit.DB.Transform.Identity);

            // Holds metadata along with georeference transforms.
            var rootNode = new GltfNode
            {
                Name = "rootNode"
            };

            // Aligns to up-axis and contains geometry
            xFormNode = new GltfNode
            {
                Name = "xFormNode",
                Extensions = null // xFormNode should not have any extensions
            };

            // Add a transform that offsets the project to real coordinates
            if (preferences.SharedCoordinates)
            {
                XYZ projectOffset = GeometryUtils.GetProjectOffset(Doc) * scale;
                rootNode.Translation = new List<float>() { (float)projectOffset.X, (float)projectOffset.Z, -(float)projectOffset.Y };
            }

            // Root node rotates to true north, orient node rotates up-axis
            Quaternion rootNodeRotation = Quaternion.Identity;

            // Add a transform that rotates the project to face true north
            if (preferences.TrueNorth)
            {
                var trueNorth = new Quaternion(new Vector3D(0, 1, 0), GeometryUtils.GetProjectTrueNorth(Doc));
                rootNodeRotation = Quaternion.Multiply(rootNodeRotation, trueNorth);
            }

            rootNode.Rotation = new List<double>() { rootNodeRotation.X, rootNodeRotation.Y, rootNodeRotation.Z, rootNodeRotation.W };

            // Orient node is used to rotate to the correct up axis
            Quaternion XFormNodeRotation;
            if (preferences.FlipAxis)
            {
                XFormNodeRotation = new Quaternion(new Vector3D(1, 0, 0), -90);
            }
            else
            {
                XFormNodeRotation = Quaternion.Identity;
            }

            xFormNode.Rotation = new List<double>() { XFormNodeRotation.X, XFormNodeRotation.Y, XFormNodeRotation.Z, XFormNodeRotation.W };
            xFormNode.Scale = new List<double>() { scale, scale, scale }; //Revit internal units are decimal feet - scale to meters


            if (preferences.ExportMetadata)
            {
                extensionsUsed = extensionsUsed ?? new List<string>();
                extensionsUsed.Add("EXT_structural_metadata");
                extensions.Add("EXT_structural_metadata", modelExtStructuralMetadataExtensionSchema);

                var rootSchema = modelExtStructuralMetadataExtensionSchema.GetClass("project") ?? modelExtStructuralMetadataExtensionSchema.AddClass("Project");
                var rootSchemaProperties = new Dictionary<string, object>();
                rootSchema.Add("properties", rootSchemaProperties);

                rootNode.Extensions = rootNode.Extensions ?? new GltfExtensions();
                rootNode.Extensions.EXT_structural_metadata = rootNode.Extensions.EXT_structural_metadata ?? new ExtStructuralMetadata();

                // Loop over all the parameters in the document. Get the parameter's info via the InternalDefinition.
                // Each parameter/InternalDefinition will bind to one or more Categories via a CategorySet.
                // For each parameter, get all all the bound Categories and add them to the glTF schema.
                BindingMap bindingMap = Doc.ParameterBindings;
                var iterator = bindingMap.ForwardIterator();
                while (iterator.MoveNext())
                {
                    Definition definition = iterator.Key;
                    // InstanceBinding and TypeBinding don't seem to have any additional members not provided by the ELementBinding base class
                    var elementBinding = (ElementBinding)iterator.Current;

                    if (definition is InternalDefinition internalDefinition)
                    {
                        ForgeTypeId forgeTypeId = internalDefinition.GetDataType();
                        bool isMeasurable = UnitUtils.IsMeasurableSpec(forgeTypeId);

                        var categoryGltfProperties = new Dictionary<string, Object>
                        {
                            { "name", definition.Name },
                            { "required", false }
                        };

                        if (isMeasurable)
                        {
                            categoryGltfProperties.Add("type", "SCALAR");
                            categoryGltfProperties.Add("componentType", "FLOAT32");
                        }
                        else if (IsSpecTypeMatch(forgeTypeId, typeof(SpecTypeId.String)))
                        {
                            categoryGltfProperties.Add("type", "STRING");
                        }
                        else if (IsSpecTypeMatch(forgeTypeId, typeof(SpecTypeId.Int)) || forgeTypeId == SpecTypeId.Boolean.YesNo)
                        {
                            categoryGltfProperties.Add("type", "SCALAR");
                            categoryGltfProperties.Add("componentType", "INT32");
                        }
                        else
                        {
                            // no mapped ForgeTypeId
                            continue;
                        }

                        CategorySet categories = elementBinding.Categories;
                        foreach (var obj in categories)
                        {
                            var category = (Category)obj;
#if REVIT2022
                        string categoryGltfName = CesiumIonRevitAddin.Utils.Util.GetGltfName(((BuiltInCategory)category.Id.IntegerValue).ToString());
#else
                            string categoryGltfName = CesiumIonRevitAddin.Utils.Util.GetGltfName(category.BuiltInCategory.ToString());
#endif
                            modelExtStructuralMetadataExtensionSchema.AddCategory(categoryGltfName);
                            var gltfClass = modelExtStructuralMetadataExtensionSchema.GetClass(categoryGltfName);
                            var schemaProperties = modelExtStructuralMetadataExtensionSchema.GetProperties(gltfClass);
                            string gltfDefinitionName = Util.GetGltfName(definition.Name);
                            if (schemaProperties.ContainsKey(gltfDefinitionName))
                            {
                                // Duplicate keys can occur if a parameter shares a name.
                                // This has happened before, though one was a shared and one a local parameter.
                                // ID and the results of GetTypeId() (which is not the same) are the only guaranteed unique fields.
                                gltfDefinitionName += "_" + internalDefinition.Id;
                            }
                            schemaProperties.Add(gltfDefinitionName, categoryGltfProperties);
                        }

                    }
                }
            }
            rootNode.Children = new List<int>();
            xFormNode.Children = new List<int>();
            nodes.AddOrUpdateCurrent("rootNode", rootNode);
            nodes.AddOrUpdateCurrent("xFormNode", xFormNode);
            rootNode.Children.Add(nodes.CurrentIndex);

            var defaultScene = new GltfScene();
            defaultScene.Nodes.Add(0);
            scenes.Add(defaultScene);

            return true;
        }

        public void Finish()
        {
            if (cancelation)
            {
                return;
            }

            FileExport.Run(preferences, bufferViews, buffers, binaryFileData,
                scenes, nodes, meshes, materials, accessors, extensionsUsed, extensionsRequired, extensions, new GltfVersion(), images, textures, samplers);
            Logger.Instance.Log("Completed model export.");
            stopwatch.Stop();
            TimeSpan timeSpan = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds / 10);
            Logger.Instance.Log("Elapsed time: " + elapsedTime);

            // Write out the json for the tiler
            TilerExportUtils.WriteTilerJson(preferences);
        }

        public bool IsCanceled()
        {
            return cancelation;
        }

        private bool shouldLogOnElementEnd = false;

        private string symbolGeometryUniqueId = "";
        private int instanceIndex = -1;
        private bool isChild = false;
        Autodesk.Revit.DB.Transform parentTransformInverse = Autodesk.Revit.DB.Transform.Identity;
        Autodesk.Revit.DB.Transform finalNodeTransform = Autodesk.Revit.DB.Transform.Identity;
        public RenderNodeAction OnElementBegin(ElementId elementId)
        {
            shouldLogOnElementEnd = false;
            symbolGeometryUniqueId = "";
            instanceIndex = -1;
            onPolymeshFired = false;
            isChild = false;
            parentTransformInverse = finalNodeTransform = Autodesk.Revit.DB.Transform.Identity;

            element = Doc.GetElement(elementId);

            if (Util.GetElementIdAsLong(elementId) == 1265228)
            {
                Logger.Instance.Log("at asphalt");
            }

            shouldSkipElement = ShouldSkipElement(element, view, Doc, nodes);
            if (shouldSkipElement)
            {
                return RenderNodeAction.Skip;
            }

            linkTransformation = (element as RevitLinkInstance)?.GetTransform();

            Logger.Instance.Log("Processing element " + element.Name + ", ID: " + element.Id.ToString());
            shouldLogOnElementEnd = true;

            var newNode = new GltfNode();
            nodes.AddOrUpdateCurrent(element.UniqueId, newNode);
            string categoryName = element.Category != null ? element.Category.Name : "Undefined";
            string familyName = GetFamilyName(element);
            newNode.Name = Util.CreateClassName(categoryName, familyName) + ": " + GetTypeNameIfApplicable(elementId);

            // Reset currentGeometry for new element
            if (currentGeometry == null)
            {
                currentGeometry = new IndexedDictionary<GeometryDataObject>();
            }
            else
            {
                currentGeometry.Reset();
            }

            if (currentVertices == null)
            {
                currentVertices = new IndexedDictionary<VertexLookupIntObject>();
            }
            else
            {
                currentVertices.Reset();
            }

#if !REVIT2019 &&!REVIT2020 && !REVIT2021 && !REVIT2022
            if (preferences.SymbolicInstancing)
            {
                // Some FamilyInstances can share the same SymbolGeometryId, but have different vertices.
                // This happens in some translated columns in Snowdon (see elements 1158107, 1161167).
                // Skip these.
                bool hasModifiedGeometry = false;
                if (element is FamilyInstance dummy && dummy.HasModifiedGeometry())
                {
                    hasModifiedGeometry = true;
                }

                if (!hasModifiedGeometry)
                {
                    Options options = new Options();
                    GeometryElement geometryElement = element.get_Geometry(options);
                    if (geometryElement != null)
                    {
                        foreach (GeometryObject geometryObject in geometryElement)
                        {
                            if (geometryObject is GeometryInstance geometryInstance)
                            {
                                SymbolGeometryId symbolGeometryId = geometryInstance.GetSymbolGeometryId();
                                symbolGeometryUniqueId = symbolGeometryId.AsUniqueIdentifier();
                                if (symbolGeometryIdToGltfNode.TryGetValue(symbolGeometryUniqueId, out int index))
                                {
                                    nodes.CurrentItem.Mesh = index;
                                    instanceIndex = index;
                                    break; // Currently only handling the first GeometryInstance
                                }
                            }
                        }
                    }
                }
            }
#endif

            var classMetadata = new Dictionary<string, object>();
            if (preferences.ExportMetadata)
            {
                newNode.Extensions = newNode.Extensions ?? new GltfExtensions();
                newNode.Extensions.EXT_structural_metadata = newNode.Extensions.EXT_structural_metadata ?? new ExtStructuralMetadata();

                newNode.Extensions.EXT_structural_metadata.Class = Util.GetGltfName(Util.CreateClassName(categoryName, familyName));

                modelExtStructuralMetadataExtensionSchema.AddCategory(categoryName);
                classMetadata = modelExtStructuralMetadataExtensionSchema.AddFamily(categoryName, familyName);

                // 1) Add a curated list of class properties from the Element class.
                // See https://www.revitapidocs.com/2024/671c33f6-169b-17ca-583b-42f9df50ace5.htm?section=propertyTableToggle

                newNode.Extensions.EXT_structural_metadata.AddProperty("elementId", elementId);
                modelExtStructuralMetadataExtensionSchema.AddSchemaProperty(categoryName, familyName, "elementId", elementId.GetType());

                string uniqueId = element.UniqueId;
                newNode.Extensions.EXT_structural_metadata.AddProperty("uniqueId", uniqueId);
                modelExtStructuralMetadataExtensionSchema.AddSchemaProperty(categoryName, familyName, "uniqueId", uniqueId.GetType());

                ElementId levelId = element.LevelId;
                newNode.Extensions.EXT_structural_metadata.AddProperty("levelId", levelId);
                modelExtStructuralMetadataExtensionSchema.AddSchemaProperty(categoryName, familyName, "levelId", levelId.GetType());

                var categoryValue = element.Category.Id;
                newNode.Extensions.EXT_structural_metadata.AddProperty("category", categoryValue);
                modelExtStructuralMetadataExtensionSchema.AddSchemaProperty(categoryName, familyName, "category", categoryValue.GetType());

                // 2) Add user-defined parameters
                AddParameterSet(element, newNode, categoryName, familyName, true);

                // 3) Add built-in parameters
                AddParameterSet(element, newNode, categoryName, familyName);

                // 4) Add specialty class data, e.g. if the Element is a Room object
                if (element is FamilyInstance)
                {
                    Autodesk.Revit.DB.Architecture.Room room = ((FamilyInstance)element).Room;
                    if (room != null)
                    {
                        string addedPropertyName = newNode.Extensions.EXT_structural_metadata.AddProperty("roomName", room.Name);
                        modelExtStructuralMetadataExtensionSchema.AddSchemaProperty(categoryName, familyName, addedPropertyName, room.Name.GetType());
                        addedPropertyName = newNode.Extensions.EXT_structural_metadata.AddProperty("roomElementId", room.Id);
                        modelExtStructuralMetadataExtensionSchema.AddSchemaProperty(categoryName, familyName, addedPropertyName, room.Id.GetType());
                        addedPropertyName = newNode.Extensions.EXT_structural_metadata.AddProperty("roomUniqueId", room.UniqueId);
                        modelExtStructuralMetadataExtensionSchema.AddSchemaProperty(categoryName, familyName, addedPropertyName, room.UniqueId.GetType());
                    }
                }

                // 5) Add human-readable category and family names to schema as default properties.
                modelExtStructuralMetadataExtensionSchema.AddDefaultSchemaProperty(categoryName, familyName, "categoryName", categoryName, "Category Name");
                modelExtStructuralMetadataExtensionSchema.AddDefaultSchemaProperty(categoryName, familyName, "familyName", familyName, "Family Name");

                // 6) Resolve manually added properties that are elements with a {parameterName}Name property.
                // E.g., add levelIdName for human readability as opposed to just levelId
                Element level = Doc.GetElement(levelId);
                if (level != null)
                {
                    string levelIdName = level.Name;
                    string addedPropertyName = newNode.Extensions.EXT_structural_metadata.AddProperty("levelIdName", levelIdName);
                    modelExtStructuralMetadataExtensionSchema.AddSchemaProperty(categoryName, familyName, addedPropertyName, levelIdName.GetType());
                }
            }

            // Set parent to Supercomponent in the metadata if it exists.
            if (element is FamilyInstance familyInstance && familyInstance.SuperComponent != null)
            {
                Element superComponent = familyInstance.SuperComponent;

                // It can be possible for an Element's Supercomponent to not be in a View.
                // For example, if you isolate only the "Planting" category in Snowdon,
                // some Elements have a Supercomponent from the "Site" class that will be missing.
                if (nodes.Contains(superComponent.UniqueId))
                {
                    isChild = true;

                    var parentNode = nodes.GetElement(superComponent.UniqueId);
                    parentNode.Children = parentNode.Children ?? new List<int>();
                    parentNode.Children.Add(nodes.CurrentIndex);

                    // It may be possible that the parent instance's transform was not used.
                    // This will happen when OnPolymesh occurs after the OnInstanceEnd event.
                    if (parentNode.Matrix != null) parentTransformInverse = ListToTransform(parentNode.Matrix).Inverse;

                    if (preferences.ExportMetadata)
                    {
                        string superComponentClass = Util.GetGltfName(superComponent.Category.Name);
                        classMetadata["parent"] = superComponentClass;
                    }
                }
                else
                {
                    xFormNode.Children.Add(nodes.CurrentIndex);
                }
            } else
            {
                xFormNode.Children.Add(nodes.CurrentIndex);
            }

            return RenderNodeAction.Proceed;
        }

        public static bool CanBeLockOrHidden(Element element, View view)
        {
            if (element.CanBeHidden(view))
            {
                return true;
            }

            if (element.Category.CanAddSubcategory || element.Category.AllowsBoundParameters)
            {
                return true;
            }

            return false;
        }

        public static bool ShouldSkipElement(Element currentElement, Autodesk.Revit.DB.View currentView,
            Document currentDocument, IndexedDictionary<GltfNode> nodes)
        {
            if (currentElement == null)
            {
                return true;
            }

            if (currentElement is Level)
            {
                return true;
            }

            if (nodes.Contains(currentElement.UniqueId))
            {
                return true;
            }

            if (!CanBeLockOrHidden(currentElement, currentView))
            {
                return true;
            }

            return false;
        }

        static bool IsKeyValueParameterDupe(string propertyName, ParameterValue parameterValue, ExtStructuralMetadata extStructuralMetadata)
        {
            ParameterValue? savedParameterValue = extStructuralMetadata.GetPropertyValue(propertyName);
            if (savedParameterValue == null) return false;
            return (savedParameterValue == parameterValue);
        }

        void AddParameterSet(Element element, GltfNode newNode, string categoryName, string familyName, bool addOnlyUserDefined = false)
        {
            ParameterSet parameterSet = element.Parameters;
            var elementIdProperties = new HashSet<(Parameter parameter, string propertyId)>(); // save to resolve with additional properties such as a human-readable name
            foreach (Parameter parameter in parameterSet)
            {
                string propertyName = Util.GetGltfName(parameter.Definition.Name);
                ParameterValue parameterValue = Util.GetParameterValue(parameter);

                bool isUserDefined = false;
                if (parameter.Definition is InternalDefinition internalDefinition)
                {
                    isUserDefined = internalDefinition.BuiltInParameter == BuiltInParameter.INVALID;
                }
                bool skipParameter = addOnlyUserDefined != isUserDefined;
                skipParameter |= Util.ShouldFilterMetadata(parameterValue);
                skipParameter |= !parameter.HasValue;
                skipParameter |= IsKeyValueParameterDupe(propertyName, parameterValue, newNode.Extensions.EXT_structural_metadata);

                if (!skipParameter)
                {
                    string addedPropertyId = newNode.Extensions.EXT_structural_metadata.AddProperty(propertyName, parameterValue);
                    modelExtStructuralMetadataExtensionSchema.AddSchemaProperty(categoryName, familyName, addedPropertyId, parameterValue.GetStoredType());

                    if (parameter.StorageType == StorageType.ElementId)
                    {
                        elementIdProperties.Add((parameter, addedPropertyId));
                    }
                }
            }

            // Add additional properties (e.g. 'name') to properties that are ElementIds
            foreach (var (parameter, addedPropertyName) in elementIdProperties)
            {
                ElementId parameterElementId = parameter.AsElementId();
#if REVIT2022 || REVIT2023
                    if (parameterElementId.IntegerValue != -1)
#else
                if (parameterElementId.Value != -1)
#endif
                {
                    Element parameterElement = Doc.GetElement(parameterElementId);
                    if (parameterElement != null)
                    {
                        // Resolve properties by getting the element's Element.Name value for human readability.
                        // The name of the "Name" property. E.g., "type" -> "typeName"
                        string resolvedPropertyName = addedPropertyName + "Name";

                        // skip adding the human-readible name for the "family" parameter.
                        // It resolves to the type name. We add "familyName" as a default schema property.
                        if (resolvedPropertyName == "familyName")
                        {
                            continue;
                        }

                        string propertyValue = parameterElement.Name;
                        newNode.Extensions.EXT_structural_metadata.AddProperty(resolvedPropertyName, propertyValue);
                        modelExtStructuralMetadataExtensionSchema.AddSchemaProperty(categoryName, familyName, resolvedPropertyName, propertyValue.GetType());
                    }
                }
            }
        }

        public void OnElementEnd(ElementId elementId)
        {

            // Skip writing out nodes for links.
            if (linkElementIsEnding)
            {
                linkElementIsEnding = false;
                return;
            }

            bool verticesAreBad = currentVertices == null || currentVertices.List.Count == 0;
            if (instanceIndex != -1) verticesAreBad = false; // Vertices check is invalid if this is a GPU instance.
            if (shouldSkipElement || verticesAreBad)
            {
                if (shouldLogOnElementEnd)
                {
                    Logger.Instance.Log($"...Finished Processing element {element.Name}, {element.Id}");
                }
                return;
            }

            var newMesh = new GltfMesh
            {
                Name = element.Name,
                Primitives = new List<GltfMeshPrimitive>()
            };

            if (instanceIndex == -1)
            {
                foreach (KeyValuePair<string, VertexLookupIntObject> kvp in currentVertices.Dict)
                {
                    GeometryDataObject geometryDataObject = currentGeometry.GetElement(kvp.Key);
                    List<double> vertices = geometryDataObject.Vertices;
                    foreach (KeyValuePair<PointIntObject, int> p in kvp.Value)
                    {
                        vertices.Add(p.Key.X);
                        vertices.Add(p.Key.Y);
                        vertices.Add(p.Key.Z);
                    }
                }

                // add all mesh primitives
                foreach (KeyValuePair<string, GeometryDataObject> kvp in currentGeometry.Dict)
                {
                    string name = kvp.Key;
                    GeometryDataObject geometryDataObject = kvp.Value;
                    GltfBinaryData elementBinaryData = GltfExportUtils.AddGeometryBinaryData(
                         buffers,
                         accessors,
                         bufferViews,
                         geometryDataObject,
                         name,
                         (int)Util.GetElementIdAsLong(elementId),
                         preferences.Normals);

                    binaryFileData.Add(elementBinaryData);

                    var meshPrimitive = new GltfMeshPrimitive();
                    meshPrimitive.Attributes.POSITION = elementBinaryData.VertexAccessorIndex;
                    if (preferences.Normals)
                    {
                        meshPrimitive.Attributes.NORMAL = elementBinaryData.NormalsAccessorIndex;
                    }
                    if (elementBinaryData.TexCoordBuffer.Count > 0)
                    {
                        meshPrimitive.Attributes.TEXCOORD_0 = elementBinaryData.TexCoordAccessorIndex;
                    }
                    meshPrimitive.Indices = elementBinaryData.IndexAccessorIndex;
                    if (preferences.Materials)
                    {
                        string materialKey = kvp.Key.Split(UNDERSCORE)[1];
                        if (materials.Contains(materialKey))
                        {
                            GltfMaterial material = materials.Dict[materialKey];
                            if (material.Name != RevitMaterials.INVALID_MATERIAL)
                            {
                                meshPrimitive.Material = materials.GetIndexFromUuid(materialKey);
                            }
                        }
                    }

                    newMesh.Primitives.Add(meshPrimitive);
                }

                meshes.AddOrUpdateCurrent(element.UniqueId, newMesh);
                nodes.CurrentItem.Mesh = meshes.CurrentIndex;
                meshes.CurrentItem.Name = newMesh.Name;

#if !REVIT2019 &&!REVIT2020 && !REVIT2021 && !REVIT2022
                if (IsFirstInstanceableElement)
                {
                    if (symbolGeometryIdToGltfNode.TryGetValue(symbolGeometryUniqueId, out _))
                    {
#if DEBUG
                        System.Diagnostics.Debug.Assert(false, "The key already exists in the dictionary.");
#endif
                    }
                    else
                    {
                        symbolGeometryIdToGltfNode.Add(symbolGeometryUniqueId, meshes.CurrentIndex);
                    }
                }
#endif
            }
            else
            {
                nodes.CurrentItem.Mesh = instanceIndex;
            }

            AddCurrentTransformToNode(nodes.CurrentItem);

            element = Doc.GetElement(elementId);
            Logger.Instance.Log($"...Finished Processing element {element.Name}, {element.Id}");
        }

        public RenderNodeAction OnInstanceBegin(InstanceNode node)
        {
            transformStack.Push(transformStack.Peek().Multiply(node.GetTransform()));

            return RenderNodeAction.Proceed;
        }

        // for Debug logging purposes, so may have 0 references
#pragma warning disable S1144 // Unused private types or members should be removed
#pragma warning disable IDE0051 // Remove unused private members
        private static string GetTransformDetails(Autodesk.Revit.DB.Transform transform)
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore S1144 // Unused private types or members should be removed
        {
            if (transform == null) return "";

            var x = transform.BasisX;
            var y = transform.BasisY;
            var z = transform.BasisZ;
            var origin = transform.Origin;

            string transformDetails = $"BasisX: ({x.X}, {x.Y}, {x.Z})\n" +
                                      $"BasisY: ({y.X}, {y.Y}, {y.Z})\n" +
                                      $"BasisZ: ({z.X}, {z.Y}, {z.Z})\n" +
                                      $"Origin: ({origin.X}, {origin.Y}, {origin.Z})";

            return transformDetails;
        }

        public void OnInstanceEnd(InstanceNode node)
        {
            // Note: This method is invoked even for instances that were skipped.

            var top = transformStack.Pop();
            // guard against OnInstanceEnd firing immediately after OnInstanceBegin without OnPolymesh in between
            if (onPolymeshFired) finalNodeTransform = top;
        }

        private static List<double> TransformToList(Autodesk.Revit.DB.Transform transform)
        {
            return new List<double>
            {
                transform.BasisX.X, transform.BasisX.Y, transform.BasisX.Z, 0.0,
                transform.BasisY.X, transform.BasisY.Y, transform.BasisY.Z, 0.0,
                transform.BasisZ.X, transform.BasisZ.Y, transform.BasisZ.Z, 0.0,
                transform.Origin.X, transform.Origin.Y, transform.Origin.Z, 1.0
            };
        }

        public static Autodesk.Revit.DB.Transform ListToTransform(List<double> matrix)
        {
            XYZ basisX = new XYZ(matrix[0], matrix[1], matrix[2]);
            XYZ basisY = new XYZ(matrix[4], matrix[5], matrix[6]);
            XYZ basisZ = new XYZ(matrix[8], matrix[9], matrix[10]);
            XYZ origin = new XYZ(matrix[12], matrix[13], matrix[14]);

            Autodesk.Revit.DB.Transform transform = Autodesk.Revit.DB.Transform.Identity;
            transform.BasisX = basisX;
            transform.BasisY = basisY;
            transform.BasisZ = basisZ;
            transform.Origin = origin;

            return transform;
        }

        public RenderNodeAction OnLinkBegin(LinkNode node)
        {
            if (preferences.VerboseLogging) Logger.Instance.Log("Beginning OnLinkBegin...");
            if (!preferences.Links)
            {
                return RenderNodeAction.Skip;
            }

            documents.Add(node.GetDocument());
            transformStack.Push(transformStack.Peek().Multiply(linkTransformation));

            if (preferences.VerboseLogging) Logger.Instance.Log("...OnLinkBegin");
            return RenderNodeAction.Proceed;
        }

        bool linkElementIsEnding = false;
        public void OnLinkEnd(LinkNode node)
        {
            // Note: This method is invoked even for instances that were skipped.

            if (preferences.VerboseLogging) Logger.Instance.Log("Beginning OnLinkEnd...");
            if (!preferences.Links)
            {
                return;
            }

            transformStack.Pop();

            documents.RemoveAt(1); // remove the item added in OnLinkBegin

            if (preferences.VerboseLogging) Logger.Instance.Log("...OnLinkEnd");

            linkElementIsEnding = true;
        }

        public RenderNodeAction OnFaceBegin(FaceNode node)
        {
            // do nothing: This custom exporter is not set to export faces.
            return RenderNodeAction.Proceed;
        }

        public void OnFaceEnd(FaceNode node)
        {
            // no-op: This exporter is not set to export faces.
        }

        public void OnRPC(RPCNode node)
        {
            List<Mesh> rpcMeshes = GeometryUtils.GetMeshes(Doc, element);

            if (rpcMeshes.Count == 0)
            {
                return;
            }

            foreach (Mesh mesh in rpcMeshes)
            {
                int triangles = mesh.NumTriangles;
                if (triangles == 0)
                {
                    continue;
                }

                MaterialUtils.SetMaterial(Doc, preferences, mesh, materials, true);

                GltfExportUtils.AddOrUpdateCurrentItem(nodes, currentGeometry, currentVertices, materials);

                for (int i = 0; i < triangles; i++)
                {
                    MeshTriangle triangle = mesh.get_Triangle(i);
                    if (triangle == null)
                    {
                        continue;
                    }

                    var pts = new List<XYZ>
                    {
                        triangle.get_Vertex(0),
                        triangle.get_Vertex(1),
                        triangle.get_Vertex(2)
                    };

                    GltfExportUtils.AddVerticesAndFaces(currentVertices.CurrentItem, currentGeometry.CurrentItem, pts);

                    if (preferences.Normals)
                    {
                        GltfExportUtils.AddRPCNormals(preferences, triangle, currentGeometry.CurrentItem);
                    }
                }
            }
        }

        public void OnLight(LightNode node)
        {
            // this custom exporter is currently not exporting lights
        }

        public void OnMaterial(MaterialNode materialNode)
        {
            if (preferences.VerboseLogging) Logger.Instance.Log($"Beginning OnMaterial, id {materialNode.MaterialId}...");

            materialHasTexture = false;
            if (preferences.Materials)
            {
                Export.RevitMaterials.Export(materialNode, Doc, materials, modelExtStructuralMetadataExtensionSchema, samplers, images, textures, ref materialHasTexture, preferences);

                if (!preferences.Textures)
                {
                    materialHasTexture = false;
                }

                if (!khrTextureTransformAdded && materialHasTexture)
                {
                    extensionsUsed = extensionsUsed ?? new List<string>();
                    extensionsUsed.Add("KHR_texture_transform");
                    extensionsRequired = extensionsRequired ?? new List<string>();
                    extensionsRequired.Add("KHR_texture_transform");
                    khrTextureTransformAdded = true;
                }
            }
            if (preferences.VerboseLogging) Logger.Instance.Log("...ending OnMaterial");
        }

        public class SerializableTransform
        {
            public double[,] Matrix { get; set; }

            public SerializableTransform(Autodesk.Revit.DB.Transform transform)
            {
                Matrix = new double[4, 4]
                {
                    { transform.BasisX.X, transform.BasisY.X, transform.BasisZ.X, 0 },
                    { transform.BasisX.Y, transform.BasisY.Y, transform.BasisZ.Y, 0 },
                    { transform.BasisX.Z, transform.BasisY.Z, transform.BasisZ.Z, 0 },
                    { transform.Origin.X, transform.Origin.Y, transform.Origin.Z, 1 }
                };
            }

            public override string ToString()
            {
                var rows = new List<string>();
                for (int i = 0; i < 4; i++)
                {
                    var row = new List<string>();
                    for (int j = 0; j < 4; j++)
                    {
                        row.Add(Matrix[i, j].ToString());
                    }
                    rows.Add(string.Join(",", row));
                }
                return string.Join("\n", rows);
            }
        }

        public class SerializablePoint
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }

            public SerializablePoint(XYZ point)
            {
                X = point.X;
                Y = point.Y;
                Z = point.Z;
            }
        }

        // Return if the currently parsed element is potentially GPU-instancable, but has not been recorded as such yet.
        private bool IsFirstInstanceableElement
        {
            get { return instanceIndex == -1 && symbolGeometryUniqueId != ""; }
        }

        void AddCurrentTransformToNode(GltfNode gltfNode)
        {
            Autodesk.Revit.DB.Transform currentMatrix = finalNodeTransform;

            if (isChild)
            {
                currentMatrix = parentTransformInverse.Multiply(currentMatrix);
            }

            gltfNode.Matrix = currentMatrix.IsIdentity ? null : TransformToList(currentMatrix);
        }

        public void OnPolymesh(PolymeshTopology polymeshTopology)
        {
            onPolymeshFired = true;

            GltfExportUtils.AddOrUpdateCurrentItem(nodes, currentGeometry, currentVertices, materials);

            if (instanceIndex != -1)
            {
                if (preferences.VerboseLogging) Logger.Instance.Log("...GPU instance found, skipping OnPolymesh)");
                return;
            }

            var pts = polymeshTopology.GetPoints();

            foreach (PolymeshFacet facet in polymeshTopology.GetFacets())
            {
                foreach (var vertIndex in facet.GetVertices())
                {
                    XYZ p = pts[vertIndex];
                    int vertexIndex = currentVertices.CurrentItem.AddVertex(new PointIntObject(p));
                    currentGeometry.CurrentItem.Faces.Add(vertexIndex);
                }
            }

            if (preferences.Normals)
            {
                GltfExportUtils.AddNormals(Autodesk.Revit.DB.Transform.Identity, polymeshTopology, currentGeometry.CurrentItem.Normals);
            }

            if (materialHasTexture)
            {
                GltfExportUtils.AddTexCoords(polymeshTopology, currentGeometry.CurrentItem.TexCoords);
            }
        }

        RenderNodeAction IExportContext.OnViewBegin(ViewNode node)
        {
            // do nothing
            return RenderNodeAction.Proceed;
        }

        void IExportContext.OnViewEnd(ElementId elementId)
        {
            // do nothing
        }

        private static string GetFamilyName(Element element)
        {
            if (element == null)
            {
                return "Invalid element";
            }

            if (element is FamilyInstance familyInstance)
            {
                var family = familyInstance.Symbol.Family;
                if (family != null)
                {
                    return family.Name;
                }
                else
                {
                    return "Family not found";
                }
            }
            else
            {
                return "System Family";
            }
        }

        private string GetTypeNameIfApplicable(ElementId elementId)
        {
            var currentElement = Doc.GetElement(elementId);
            if (currentElement == null)
            {
                return "Element not found";
            }

            if (Doc.GetElement(currentElement.GetTypeId()) is ElementType elementType)
            {
                return elementType.Name;
            }
            else
            {
                return "Type not applicable or not found";
            }
        }
    }
}
