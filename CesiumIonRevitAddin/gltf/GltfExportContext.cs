using Autodesk.Revit.DB;
using CesiumIonRevitAddin.Export;
using CesiumIonRevitAddin.Model;
using CesiumIonRevitAddin.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media.Media3D;

namespace CesiumIonRevitAddin.Gltf
{
    internal class GltfExportContext : IPhotoRenderContext
    {
        private const char UNDERSCORE = '_';
        private readonly bool verboseLog = true;
        private Autodesk.Revit.DB.Transform parentTransformInverse = null;
        private readonly Preferences preferences;
        private readonly View view;
        private Autodesk.Revit.DB.Element element;
        private readonly List<Document> documents = new List<Document>();
        private bool cancelation;
        // We record instanceStackDepth separately from getting it via transformStack.Count because there are
        // cases where OnInstanceBegin is immediately followed by OnInstanceEnd, then OnPolymesh is triggered.
        // We detect and handle this case.
        private int instanceStackDepth = 0;
        private readonly Stack<Autodesk.Revit.DB.Transform> transformStack = new Stack<Autodesk.Revit.DB.Transform>();
        private readonly Stack<Autodesk.Revit.DB.Transform> rawTransformStack = new Stack<Autodesk.Revit.DB.Transform>();
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
        private readonly Dictionary<string, GltfExtensionSchema> extensions = new Dictionary<string, GltfExtensionSchema>();
        private readonly GltfExtStructuralMetadataExtensionSchema extStructuralMetadataSchema = new GltfExtStructuralMetadataExtensionSchema();
        private Autodesk.Revit.DB.Transform cachedTransform;
        private bool khrTextureTransformAdded;
        private bool skipElementFlag;
        private Autodesk.Revit.DB.Transform linkTransformation;
        private IndexedDictionary<GeometryDataObject> currentGeometry;
        private IndexedDictionary<VertexLookupIntObject> currentVertices;
        private readonly Dictionary<string, int> geometryDataObjectIndices = new Dictionary<string, int>();
        private bool materialHasTexture;

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


#if REVIT2022 || REVIT2023 || REVIT2024 || REVIT2025
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
#endif

        public bool Start()
        {
            Logger.Enabled = verboseLog;
            cancelation = false;

            Logger.Instance.Log("Beginning export.");

            Reset();

            // Create the glTF temp export directory
            if (!Directory.Exists(preferences.TempDirectory))
            {
                Directory.CreateDirectory(preferences.TempDirectory);
            }

            float scale = 0.3048f; // Decimal feet to meters

            transformStack.Push(Autodesk.Revit.DB.Transform.Identity);

            // Holds metadata along with georeference transforms
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

                //TODO: Implement flip axis support here (not sure if we really need it?)
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
                extensions.Add("EXT_structural_metadata", extStructuralMetadataSchema);

                ProjectInfo projectInfo = Doc.ProjectInformation;

                var rootSchema = extStructuralMetadataSchema.GetClass("project") ?? extStructuralMetadataSchema.AddClass("Project");
                var rootSchemaProperties = new Dictionary<string, object>();
                rootSchema.Add("properties", rootSchemaProperties);

                rootNode.Extensions = rootNode.Extensions ?? new GltfExtensions();
                rootNode.Extensions.EXT_structural_metadata = rootNode.Extensions.EXT_structural_metadata ?? new ExtStructuralMetadata();
                rootNode.Extensions.EXT_structural_metadata.Class = "project";
                AddPropertyInfoProperty("Project Name", projectInfo.Name, rootSchemaProperties, rootNode);
                AddPropertyInfoProperty("Project Number", projectInfo.Number, rootSchemaProperties, rootNode);
                AddPropertyInfoProperty("Client Name", projectInfo.ClientName, rootSchemaProperties, rootNode);
                AddPropertyInfoProperty("Project Address", projectInfo.Address, rootSchemaProperties, rootNode);
                AddPropertyInfoProperty("Building Name", projectInfo.BuildingName, rootSchemaProperties, rootNode);
                AddPropertyInfoProperty("Author", projectInfo.Author, rootSchemaProperties, rootNode);
                AddPropertyInfoProperty("Organization Name", projectInfo.OrganizationName, rootSchemaProperties, rootNode);
                AddPropertyInfoProperty("Organization Description", projectInfo.OrganizationDescription, rootSchemaProperties, rootNode);
                AddPropertyInfoProperty("Issue Date", projectInfo.IssueDate, rootSchemaProperties, rootNode);
                AddPropertyInfoProperty("Project Status", projectInfo.Status, rootSchemaProperties, rootNode);

                // Loop over all the parameters in the document. Get the parameter's info via the InternalDefinition.
                // Each parameter/InternalDefinition will bind to one or more Categories via a CategorySet.
                // For each parameter, get all all the bound Categories and add them to the glTF schema.
#if !REVIT2020 && !REVIT2021
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

                        var categoryGltfProperty = new Dictionary<string, Object>
                    {
                        { "name", definition.Name },
                        { "required", false }
                    };

                        if (isMeasurable)
                        {
                            categoryGltfProperty.Add("type", "SCALAR");
                            categoryGltfProperty.Add("componentType", "FLOAT32");
                        }
                        else if (IsSpecTypeMatch(forgeTypeId, typeof(SpecTypeId.String)))
                        {
                            categoryGltfProperty.Add("type", "STRING");
                        }
                        else if (IsSpecTypeMatch(forgeTypeId, typeof(SpecTypeId.Int)) || forgeTypeId == SpecTypeId.Boolean.YesNo)
                        {
                            categoryGltfProperty.Add("type", "SCALAR");
                            categoryGltfProperty.Add("componentType", "INT32");
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
                            extStructuralMetadataSchema.AddCategory(categoryGltfName);
                            var gltfClass = extStructuralMetadataSchema.GetClass(categoryGltfName);
                            var schemaProperties = extStructuralMetadataSchema.GetProperties(gltfClass);
                            string gltfDefinitionName = Util.GetGltfName(definition.Name);
                            if (schemaProperties.ContainsKey(gltfDefinitionName))
                            {
                                // Duplicate keys can occur if a parameter shares a name.
                                // This has happened before, though one was a shared and one a local parameter.
                                // ID and the results of GetTypeId() (which is not the same) are the only guaranteed unique fields.
                                gltfDefinitionName += "_" + internalDefinition.Id;
                            }
                            schemaProperties.Add(gltfDefinitionName, categoryGltfProperty);
                        }

                    }
                }
#endif
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

        // Add information about the physical Revit building/property (via the project's PropertyInfo) to glTF "properties"
        private static void AddPropertyInfoProperty(string propertyName, string propertyValue, Dictionary<string, object> rootSchemaProperties, GltfNode rootNode)
        {
            if (propertyValue == "")
            {
                return;
            }

            // add to node
            var gltfPropertyName = Utils.Util.GetGltfName(propertyName);
            rootNode.Extensions.EXT_structural_metadata.Properties.Add(gltfPropertyName, propertyValue);

            // add to schema
            var propertySchema = new Dictionary<string, object>();
            rootSchemaProperties.Add(gltfPropertyName, propertySchema);
            propertySchema.Add("name", propertyName);
            propertySchema.Add("type", "STRING");
            propertySchema.Add("required", false);
        }

        public void Finish()
        {
            if (cancelation)
            {
                return;
            }

            FileExport.Run(preferences, bufferViews, buffers, binaryFileData,
                scenes, nodes, meshes, materials, accessors, extensionsUsed, extensions, new GltfVersion(), images, textures, samplers);
            Logger.Instance.Log("Completed model export.");

            // Write out the json for the tiler
            TilerExportUtils.WriteTilerJson(preferences);
        }

        public bool IsCanceled()
        {
            return cancelation;
        }

        // Most instanced elements have this event stack order: OnElementBegin->OnInstanceBegin->(geometry/material events)->OnInstanceEnd->OnElementEnd
        // But some do this: OnElementBegin->OnInstanceBegin->->OnInstanceEnd->(geometry/material events)->OnElementEnd
        // This records if the latter has happened
        private bool onInstanceEndCompleted = false;
        private bool useCurrentInstanceTransform = false;
        private bool shouldLogOnElementEnd = false;
        public RenderNodeAction OnElementBegin(ElementId elementId)
        {
            onInstanceEndCompleted = false;
            useCurrentInstanceTransform = false;
            parentTransformInverse = null;
            shouldLogOnElementEnd = false;

            element = Doc.GetElement(elementId);

            if (!Util.CanBeLockOrHidden(element, view) ||
                (element is Level))
            {
                skipElementFlag = true;
                return RenderNodeAction.Skip;
            }

            if (nodes.Contains(element.UniqueId))
            {
                // Duplicate element, skip adding.
                skipElementFlag = true;
                return RenderNodeAction.Skip;
            }

            if (element is RevitLinkInstance linkInstance)
            {
                linkTransformation = linkInstance.GetTransform();
            }

            Logger.Instance.Log("Processing element " + element.Name + ", ID: " + element.Id.ToString());
            shouldLogOnElementEnd = true;

            var newNode = new GltfNode();

            var classMetadata = new Dictionary<string, object>();
            if (preferences.ExportMetadata)
            {
                newNode.Extensions = newNode.Extensions ?? new GltfExtensions();
                newNode.Extensions.EXT_structural_metadata = newNode.Extensions.EXT_structural_metadata ?? new ExtStructuralMetadata();

                string categoryName = element.Category != null ? element.Category.Name : "Undefined";
                string familyName = GetFamilyName(element);
                newNode.Name = Util.CreateClassName(categoryName, familyName) + ": " + GetTypeNameIfApplicable(elementId);
                newNode.Extensions.EXT_structural_metadata.Class = Util.GetGltfName(Util.CreateClassName(categoryName, familyName));

                newNode.Extensions.EXT_structural_metadata.Properties.Add("uniqueId", element.UniqueId);
                newNode.Extensions.EXT_structural_metadata.Properties.Add("levelId", Util.GetElementIdAsLong(element.LevelId).ToString());

                // create a glTF property from any remaining Revit parameter not explicitly added above
                ParameterSet parameterSet = element.Parameters;
                foreach (Parameter parameter in parameterSet)
                {
                    string propertyName = Util.GetGltfName(parameter.Definition.Name);
                    object paramValue = GetParameterValue(parameter);
                    if (paramValue != null && !newNode.Extensions.EXT_structural_metadata.Properties.ContainsKey(propertyName))
                    {
                        newNode.Extensions.EXT_structural_metadata.Properties.Add(propertyName, paramValue);
                    }
                }

                extStructuralMetadataSchema.AddCategory(categoryName);
                classMetadata = extStructuralMetadataSchema.AddFamily(categoryName, familyName);
                extStructuralMetadataSchema.AddProperties(categoryName, familyName, parameterSet);
            }

            nodes.AddOrUpdateCurrent(element.UniqueId, newNode);

            // set parent to Supercomponent if it exists.
            if (element is FamilyInstance familyInstance && familyInstance.SuperComponent != null)
            {
                Element superComponent = familyInstance.SuperComponent;
                // It can be possible for an Element's Supercomponent to not be in a View.
                // For example, if you isolate only the "Planting" category in Snowdon,
                // some Elements have a Supercomponent from the "Site" class that will be missing.
                if (!nodes.Contains(superComponent.UniqueId))
                {
                    xFormNode.Children.Add(nodes.CurrentIndex);
                }
                else
                {
                    if (preferences.ExportMetadata)
                    {
                        string superComponentClass = Util.GetGltfName(superComponent.Category.Name);
                        classMetadata["parent"] = superComponentClass;
                    }

                    GltfNode parentNode = nodes.GetElement(superComponent.UniqueId);
                    parentNode.Children = parentNode.Children ?? new List<int>();
                    parentNode.Children.Add(nodes.CurrentIndex);
                    useCurrentInstanceTransform = true;

                    var parentInstance = (Instance)superComponent;
                    parentTransformInverse = parentInstance.GetTransform().Inverse;
                }
            }
            else
            {
                xFormNode.Children.Add(nodes.CurrentIndex);
            }

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

            return RenderNodeAction.Proceed;
        }

        public void OnElementEnd(ElementId elementId)
        {
            if (skipElementFlag ||
                currentVertices == null ||
                currentVertices.List.Count == 0 ||
                !Util.CanBeLockOrHidden(element, view))
            {
                skipElementFlag = false;
                if (shouldLogOnElementEnd)
                {
                    Logger.Instance.Log("...Finished Processing element " + element.Name);
                }
                return;
            }

            var newMesh = new GltfMesh
            {
                Name = element.Name,
                Primitives = new List<GltfMeshPrimitive>()
            };

            int instanceIndex = -1;
            string geometryDataObjectHash = "";
            var collectionToHash = new Dictionary<string, GeometryDataObject>(); // contains data that will be common across instances

            foreach (KeyValuePair<string, VertexLookupIntObject> kvp in currentVertices.Dict)
            {
                GeometryDataObject geometryDataObject = currentGeometry.GetElement(kvp.Key);
                var vertices = geometryDataObject.Vertices;
                foreach (KeyValuePair<PointIntObject, int> p in kvp.Value)
                {
                    vertices.Add(p.Key.X);
                    vertices.Add(p.Key.Y);
                    vertices.Add(p.Key.Z);
                }

                // also add materials to the hash to test for instancing since in glTF mesh primitives are tied to a material
                if (preferences.Instancing)
                {
                    var materialKey = kvp.Key.Split('_')[1];
                    collectionToHash.Add(materialKey, geometryDataObject);
                }
            }

            if (preferences.Instancing)
            {
                geometryDataObjectHash = ComputeHash(collectionToHash);
                if (geometryDataObjectIndices.TryGetValue(geometryDataObjectHash, out var index))
                {
                    nodes.CurrentItem.Mesh = index;
                    instanceIndex = index;
                }
            }

            if (instanceIndex == -1)
            {
                // add all mesh primitives
                foreach (KeyValuePair<string, GeometryDataObject> kvp in currentGeometry.Dict)
                {
                    var name = kvp.Key;
                    var geometryDataObject = kvp.Value;
                    GltfBinaryData elementBinaryData = GltfExportUtils.AddGeometryMeta(
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
                        var materialKey = kvp.Key.Split(UNDERSCORE)[1];
                        if (materials.Contains(materialKey))
                        {
                            var material = materials.Dict[materialKey];
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
                if (preferences.Instancing)
                {
                    geometryDataObjectIndices.Add(geometryDataObjectHash, meshes.CurrentIndex);
                }
            }

            element = Doc.GetElement(elementId);
            Logger.Instance.Log("...Finished Processing element " + element.Name);
        }

        public RenderNodeAction OnInstanceBegin(InstanceNode node)
        {
            this.instanceStackDepth++;

            var transformationMultiply = CurrentFullTransform.Multiply(node.GetTransform());
            transformStack.Push(transformationMultiply);
            rawTransformStack.Push(node.GetTransform());

            return RenderNodeAction.Proceed;
        }

        // for Debug logging purposes, so may have 0 references
#pragma warning disable S1144 // Unused private types or members should be removed
        private static string GetTransformDetails(Autodesk.Revit.DB.Transform transform)
#pragma warning restore S1144 // Unused private types or members should be removed
        {
            var x = transform.BasisX;
            var y = transform.BasisY;
            var z = transform.BasisZ;
            var origin = transform.Origin;

            string transformDetails = $"BasisX: ({x.X}, {x.Y}, {x.Z})\n" +
                                      $"BasisY: ({y.X}, {y.Y}, {y.Z})\n" +
                                      $"BasisZ: ({z.X}, {z.Y}, {z.Z})\n" +
                                      $"Origin: ({origin.X}, {origin.Y}, {origin.Z})\n";

            return transformDetails;
        }

        public void OnInstanceEnd(InstanceNode node)
        {
            instanceStackDepth--;

            // Note: This method is invoked even for instances that were skipped.

            Autodesk.Revit.DB.Transform transform = transformStack.Pop();
            rawTransformStack.Pop();

            if (!preferences.Instancing)
            {
                return;
            }

            // Do not write to the node if there is an instance stack.
            // This happens with railings because the balusters are sub-instances of the railing instance.
            if (instanceStackDepth > 0)
            {
                return;
            }

            if (!transform.IsIdentity)
            {
                GltfNode currentNode = nodes.CurrentItem;

                var currentNodeTransform = node.GetTransform();
                if (useCurrentInstanceTransform) // for nodes with a non-root parent
                {
                    if (!currentNodeTransform.IsIdentity)
                    {
                        Autodesk.Revit.DB.Transform outgoingMatrix = parentTransformInverse * currentNodeTransform;
                        currentNode.Matrix = TransformToList(outgoingMatrix);
                    }
                }
                else
                {
                    currentNode.Matrix = TransformToList(transform);
                }
            }

            cachedTransform = transform;
            onInstanceEndCompleted = true;
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

        public RenderNodeAction OnLinkBegin(LinkNode node)
        {
            if (!preferences.Links)
            {
                return RenderNodeAction.Skip;
            }

            documents.Add(node.GetDocument());
            transformStack.Push(CurrentFullTransform.Multiply(linkTransformation));

            return RenderNodeAction.Proceed;
        }

        public void OnLinkEnd(LinkNode node)
        {
            if (!preferences.Links)
            {
                return;
            }

            // Note: This method is invoked even for instances that were skipped.
            transformStack.Pop();

            documents.RemoveAt(1); // remove the item added in OnLinkBegin
        }

        public RenderNodeAction OnFaceBegin(FaceNode node)
        {
            // do nothing: This custom exporter is not set to export faces.
            return RenderNodeAction.Proceed;
        }

        public void OnFaceEnd(FaceNode node)
        {
            // no-op: This custom exporter is not set to export faces.
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

        public void OnMaterial(MaterialNode node)
        {
            materialHasTexture = false;
            if (preferences.Materials)
            {
                Export.RevitMaterials.Export(node, Doc, materials, extStructuralMetadataSchema, samplers, images, textures, ref materialHasTexture, preferences);

                if (!preferences.Textures)
                {
                    materialHasTexture = false;
                }

                if (!khrTextureTransformAdded && materialHasTexture)
                {
                    extensionsUsed.Add("KHR_texture_transform");
                    khrTextureTransformAdded = true;
                }
            }
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

        public void OnPolymesh(PolymeshTopology node)
        {
            GltfExportUtils.AddOrUpdateCurrentItem(nodes, currentGeometry, currentVertices, materials);

            var pts = node.GetPoints();
            if (!preferences.Instancing)
            {
                pts = pts.Select(p => CurrentFullTransform.OfPoint(p)).ToList();
            }
            else
            {
                // handle the case of where OnInstanceBegin and OnInstanceEnd occur with no events in between
                // i.e.: OnElementBegin->OnInstanceBegin->OnInstanceEnd->OnPolymesh
                if (onInstanceEndCompleted && instanceStackDepth == 0)
                {
                    Autodesk.Revit.DB.Transform inverse = cachedTransform.Inverse;
                    pts = pts.Select(p => inverse.OfPoint(p)).ToList();
                }
                // Transform stacks above 1 indicate a nested instance.
                // This could be either from a nested instance in a single document. This has happened with part of a chair being an instance inside a chair instance.
                // It would also be via a linked Revit document with its own transform stack.
                // We need to multiply by everything except the transform deepest on the stack.
                else if (rawTransformStack.Count == 2)
                {
                    pts = pts.Select(p => rawTransformStack.Peek().OfPoint(p)).ToList();
                }
                // Convert to a List so we can non-destructively multiply by everything except the base stack item.
                else if (rawTransformStack.Count > 2)
                {
                    var rawTransformArray = rawTransformStack.ToArray();
                    for (int i = 1; i < rawTransformArray.Length; i++)
                    {
                        pts = pts.Select(p => rawTransformArray[i].OfPoint(p)).ToList();
                    }
                }
            }

            foreach (PolymeshFacet facet in node.GetFacets())
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
                GltfExportUtils.AddNormals(preferences, CurrentFullTransform, node, currentGeometry.CurrentItem.Normals);
            }

            if (materialHasTexture)
            {
                GltfExportUtils.AddTexCoords(preferences, node, currentGeometry.CurrentItem.TexCoords);
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

        private static object GetParameterValue(Autodesk.Revit.DB.Parameter parameter)
        {
            switch (parameter.StorageType)
            {
                case StorageType.Integer:
                    return parameter.AsInteger();
                case StorageType.Double:
                    return parameter.AsDouble();
                case StorageType.String:
                    return parameter.AsString();
                case StorageType.ElementId:
                    return Util.GetElementIdAsLong(parameter.AsElementId()).ToString();
                default:
                    return null;
            }
        }

        private Autodesk.Revit.DB.Transform CurrentFullTransform
        {
            get
            {
                return transformStack.Peek();
            }
        }

        public static string ComputeHash(Dictionary<string, GeometryDataObject> collectionToHash)
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore
            };
            string json = JsonConvert.SerializeObject(collectionToHash, settings);

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(json));

                // Convert the byte array to a hexadecimal string
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }
    }
}
