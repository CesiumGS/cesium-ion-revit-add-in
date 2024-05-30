﻿using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using CesiumIonRevitAddin.Utils;
using CesiumIonRevitAddin.Model;

namespace CesiumIonRevitAddin.gltf
{
    internal class GltfExportContext : IPhotoRenderContext
    {

        const char UNDERSCORE = '_';

        public GltfExportContext(Document doc)
        {
            documents.Add(doc);
            view = doc.ActiveView;
        }

        View view;
        Element element;
        List<Document> documents = new List<Document>();
        bool cancelation;
        Stack<Autodesk.Revit.DB.Transform> transformStack = new Stack<Autodesk.Revit.DB.Transform>();
        GltfNode rootNode;
        GltfVersion gltfVersion = new GltfVersion();
        GltfExtStructuralMetadataExtensionSchema extStructuralMetadata = new GltfExtStructuralMetadataExtensionSchema();
        List<GltfAccessor> accessors = new List<GltfAccessor>();
        List<GltfBufferView> bufferViews = new List<GltfBufferView>();
        List<GltfBuffer> buffers = new List<GltfBuffer>();
        List<GltfBinaryData> binaryFileData = new List<GltfBinaryData>();
        List<GltfScene> scenes = new List<GltfScene>();
        IndexedDictionary<GltfNode> nodes = new IndexedDictionary<GltfNode>();
        IndexedDictionary<GltfMesh> meshes = new IndexedDictionary<GltfMesh>();
        IndexedDictionary<GltfMaterial> materials = new IndexedDictionary<GltfMaterial>();
        List<string> extensionsUsed = new List<string>();
        Dictionary<string, GltfExtensionSchema> extensions = new Dictionary<string, GltfExtensionSchema>();

        Document doc
        {
            get
            {
                // return linked doc if count > 1
                return documents.Count == 1 ? documents[0] : documents[1];
            }
        }

        public bool Start()
        {
            cancelation = false;

            transformStack.Push(Autodesk.Revit.DB.Transform.Identity);

            var preferences = new Preferences(); // TODO: user user-defined preferences
            rootNode = new GltfNode();
            rootNode.Name = "rootNode";
            rootNode.Rotation = CesiumIonRevitAddin.Transform.ModelRotation.Get(preferences.flipAxis);
            rootNode.Scale = new List<double> { 1.0, 1.0, 1.0 };

            ProjectInfo projectInfo = doc.ProjectInformation;

            gltfVersion.extras.Add("Project Name", projectInfo.Name);
            gltfVersion.extras.Add("Project Number", projectInfo.Number);
            gltfVersion.extras.Add("Client Name", projectInfo.ClientName);
            gltfVersion.extras.Add("Project Address", projectInfo.Address);
            gltfVersion.extras.Add("Building Name", projectInfo.BuildingName);
            gltfVersion.extras.Add("Author", projectInfo.Author);
            gltfVersion.extras.Add("Organization Name", projectInfo.OrganizationName);
            gltfVersion.extras.Add("Organization Description", projectInfo.OrganizationDescription);
            gltfVersion.extras.Add("Issue Date", projectInfo.IssueDate);
            gltfVersion.extras.Add("Project Status", projectInfo.Status);

            BindingMap bindingMap = doc.ParameterBindings;
            var iterator = bindingMap.ForwardIterator();
            while (iterator.MoveNext())
            {
                var definition = iterator.Key;
                // InstanceBinding and TypeBinding don't seem to have any additional members vs ELementBinding base class
                var elementBinding = (ElementBinding)iterator.Current;


                // Get parameter definition information
                string paramDefinitionName = definition.Name;

                // TODO: handle ExternalDefinition
                var internalDefinition = definition as InternalDefinition;
                if (internalDefinition != null)
                {
                    // TODO: keep?
                    // gltfVersion.extras.Add(paramDefinitionName, categoryNames);

                    var forgeTypeId = internalDefinition.GetDataType();
                    // String^ glTFDataType = "";
                    var isMeasurable = UnitUtils.IsMeasurableSpec(forgeTypeId);
                    var categoryGltfProperty = new Dictionary<string, Object>
                    {
                        { "name", definition.Name },
                        { "required", false } // TODO: unsure if false
                    };

                    if (isMeasurable) // TODO: isMeasurable SHOULD equate to Revit's StorageType::Double
                    {
                        //glTFDataType = "SCALAR:Double";
                        categoryGltfProperty.Add("type", "SCALAR");
                        categoryGltfProperty.Add("componentType", "FLOAT32");
                    }
                    else if (forgeTypeId == SpecTypeId.String.Text)
                    {
                        categoryGltfProperty.Add("type", "STRING");
                    }
                    else if (forgeTypeId == SpecTypeId.Int.Integer)
                    {
                        categoryGltfProperty.Add("type", "SCALAR");
                        categoryGltfProperty.Add("componentType", "INT32");
                    }
                    else
                    {
                        Autodesk.Revit.UI.TaskDialog.Show("Error creating category-level glTF type", "definition->Name has no mapped ForgeTypeId");
                    }

                    var categories = elementBinding.Categories;
                    // String^ categoryNames = "";
                    var categoryGltfNames = new List<string>();
                    foreach (var obj in categories)
                    {
                        var category = (Category)obj;
                        var categoryGltfName = CesiumIonRevitAddin.Utils.Util.GetGltfName(category.Name);
                        // categoryNames += category->Name + "; ";

                        extStructuralMetadata.AddCategory(category.Name);
                        var class_ = extStructuralMetadata.GetClass(categoryGltfName);
                        var schemaProperties = extStructuralMetadata.GetProperties(class_);
                        schemaProperties.Add(CesiumIonRevitAddin.Utils.Util.GetGltfName(definition.Name), categoryGltfProperty);
                    }

                    // TODO
                    // gltfVersion.extras.Add(paramDefinitionName + "_glTFType", glTFDataType);

                    //// DEVEL
                    //if (UnitUtils::IsMeasurableSpec(forgeTypeId)) {
                    //	auto discipline = UnitUtils::GetDiscipline(forgeTypeId);
                    //	gltfVersion.extras.Add(paramDefinitionName + "_discipline", discipline->TypeId);
                    //	auto typeCatalog = UnitUtils::GetTypeCatalogStringForSpec(forgeTypeId);
                    //	gltfVersion.extras.Add(paramDefinitionName + "_typeCatalog", typeCatalog);
                    //}
                    //auto isUnit = UnitUtils::IsUnit(forgeTypeId);
                    //gltfVersion.extras.Add(paramDefinitionName + "_isUnit", isUnit);
                    //auto isSymbol = UnitUtils::IsSymbol(forgeTypeId);
                    //gltfVersion.extras.Add(paramDefinitionName + "_isSymbol", isSymbol);

                    // https://forums.autodesk.com/t5/revit-api-forum/conversion-from-internal-to-revit-type-for-unittype/td-p/10452742
                    // "For context, the ForgeTypeId properties directly in the SpecTypeId class identify the measurable data types, like SpecTypeId.Length or SpecTypeId.Mass. "
                    // maybe use SpecTypeId comparisons to get data type
                    //auto isSpec = SpecUtils::IsSpec(forgeTypeId); // "spec" is a unit. Maybe ship those with out units? (What about text?)
                    //gltfVersion.extras.Add(paramDefinitionName + "_IsSpec", isSpec.ToString());

                    //auto humanLabel = LabelUtils::GetLabelForSpec(forgeTypeId);
                    //gltfVersion.extras.Add(paramDefinitionName + "_LabelUtils", humanLabel);

                    // useful?
                    // internalDefinition->GetParameterTypeId();        }
                }
            }

            return true;
        }

        public void Finish()
        {
            if (cancelation)
            {
                return;
            }

            FileExport.Run(bufferViews, buffers, binaryFileData,
                scenes, nodes, meshes, materials, accessors, extensionsUsed, extensions, gltfVersion);
        }

        public bool IsCanceled()
        {
            return cancelation; ;
        }

        //public RenderNodeAction OnViewBegin(ViewNode node)
        //{
        //    throw new NotImplementedException();
        //}

        //public void OnViewEnd(ElementId elementId)
        //{
        //    throw new NotImplementedException();
        //}

        public RenderNodeAction OnElementBegin(ElementId elementId)
        {
            element = doc.GetElement(elementId);

            // TODO: add actual user preferences
            var preferences = new Preferences();
            if (!CesiumIonRevitAddin.Utils.Util.CanBeLockOrHidden(element, view) ||
                ((Level)element != null && !preferences.levels))
            {
                // Autodesk::Revit::UI::TaskDialog::Show("GLTFExportContext::OnElementBegin", "skipping element");
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

            var newNode = new GltfNode();
            // TODO: needed?
            // GLTFExtras^ extras = gcnew GLTFExtras();
            // TODO: maybe useful utils for descriptions, names, etc
            //if (preferences->properties)
            //{
            //	newNode->name = Util::ElementDescription(element);
            //	extras->uniqueId = element->UniqueId;
            //	extras->parameters = Util::GetElementParameters(element, true);
            //	extras->elementCategory = element->Category->Name;
            //	extras->elementId = element->Id->IntegerValue;
            //}
            // newNode->extras = extras;

            // TODO: recheck if these are still needed
            var categoryName = element.Category != null ? element.Category.Name : "Undefined";
            var familyName = GetFamilyName(element);
            newNode.Name = Util.CreateClassName(categoryName, familyName) + ": " + GetTypeNameIfApplicable(elementId);
            newNode.extensions.EXT_structural_metadata.Class = Util.GetGltfName(
                Util.CreateClassName(categoryName, familyName)
                );

            // TODO: maybe useful. For example "category" is an ElementId, so will come out as a stringified integer
            //newNode->extensions->EXT_structural_metadata->Properties->Add("elementId", elementId->IntegerValue);
            //newNode->extensions->EXT_structural_metadata->Properties->Add("category", categoryName);
            //newNode->extensions->EXT_structural_metadata->Properties->Add("familyName", familyName);
            //newNode->extensions->EXT_structural_metadata->Properties->Add("typeName", GetTypeNameIfApplicable(elementId));

            //newNode->extensions->EXT_structural_metadata->Properties->Add("testCategoryType", element->Category->CategoryType.ToString());
            //newNode->extensions->EXT_structural_metadata->Properties->Add("testBuiltInCategory", element->Category->BuiltInCategory);
            //newNode->extensions->EXT_structural_metadata->Properties->Add("testSubElementCount", element->GetSubelements()->Count);

            newNode.extensions.EXT_structural_metadata.Properties.Add("uniqueId", element.UniqueId);
            newNode.extensions.EXT_structural_metadata.Properties.Add("levelId", element.LevelId.IntegerValue.ToString());
            newNode.extensions.EXT_structural_metadata.Properties.Add("categoryName", Util.GetGltfName(categoryName));


            // create a glTF property from any remaining Revit parameter not explicitly added above
            var parameterSet = element.Parameters;
            foreach (Parameter parameter in parameterSet)
            {
                string propertyName = Util.GetGltfName(parameter.Definition.Name);
                object paramValue = GetParameterValue(parameter);
                if (paramValue != null && !newNode.extensions.EXT_structural_metadata.Properties.ContainsKey(propertyName))
                {
                    newNode.extensions.EXT_structural_metadata.Properties.Add(propertyName, paramValue);
                }
            }

            extStructuralMetadata.AddCategory(categoryName);
            var class_ = extStructuralMetadata.AddFamily(categoryName, familyName);
            extStructuralMetadata.AddProperties(categoryName, familyName, parameterSet);
            // set parent to Supercomponent if it exists
            if (element is FamilyInstance familyInstance)
            {
                var superComponent = familyInstance.SuperComponent;
                if (superComponent != null)
                {
                    // TODO: looks like superComponent->Id is how to parent non-schema nodes
                    var superComponentClass = Util.GetGltfName(superComponent.Category.Name);
                    class_["parent"] = superComponentClass;
                }
            }

            nodes.AddOrUpdateCurrent(element.UniqueId, newNode);

            rootNode.Children.Add(nodes.CurrentIndex);

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
            // Autodesk::Revit::UI::TaskDialog::Show("Cesium GS", "GLTFExportContext::OnElementEnd");
            if (currentVertices == null || currentVertices.List.Count == 0)
            {
                return;
            }

            if (skipElementFlag)
            {
                skipElementFlag = false;
                return;
            }

            if (!Util.CanBeLockOrHidden(element, view))
            {
                return;
            }

            // create a new mesh for the node (assuming 1 mesh per node w/ multiple primitives on mesh)
            var newMesh = new GltfMesh
            {
                Name = element.Name,
                Primitives = new List<GltfMeshPrimitive>()
            };
            meshes.AddOrUpdateCurrent(element.UniqueId, newMesh);

            nodes.CurrentItem.Mesh = meshes.CurrentIndex;

            // Add vertex data to currentGeometry for each geometry/material pairing
            foreach (KeyValuePair<string, VertexLookupIntObject> kvp in currentVertices.Dict)
            {
                var vertices = currentGeometry.GetElement(kvp.Key).Vertices;
                foreach (KeyValuePair<PointIntObject, int> p in kvp.Value)
                {
                    vertices.Add(p.Key.X);
                    vertices.Add(p.Key.Y);
                    vertices.Add(p.Key.Z);
                }
            }

            // Convert currentGeometry objects into glTFMeshPrimitives
            var preferences = new Preferences(); // TODO: use user-set preferences
            foreach (KeyValuePair<string, GeometryDataObject> kvp in currentGeometry.Dict)
            {
                GltfBinaryData elementBinaryData = GLTFExportUtils.AddGeometryMeta(
                    buffers,
                    accessors,
                    bufferViews,
                    kvp.Value,
                    kvp.Key,
                    elementId.IntegerValue,
                    preferences.Normals);

                binaryFileData.Add(elementBinaryData);

                var materialKey = kvp.Key.Split(UNDERSCORE)[1];
                var meshPrimitive = new GltfMeshPrimitive();

                meshPrimitive.Attributes.POSITION = elementBinaryData.VertexAccessorIndex;

                if (preferences.Normals)
                {
                    meshPrimitive.Attributes.NORMAL = elementBinaryData.NormalsAccessorIndex;
                }

                meshPrimitive.Indices = elementBinaryData.IndexAccessorIndex;

                if (preferences.materials)
                {
                    if (materials.Contains(materialKey))
                    {
                        meshPrimitive.Material = materials.GetIndexFromUuid(materialKey);
                    }
                }

                meshes.CurrentItem.Primitives.Add(meshPrimitive);

                meshes.CurrentItem.Name = element.Name;
            }
        }

        public RenderNodeAction OnInstanceBegin(InstanceNode node)
        {
            throw new NotImplementedException();
        }

        public void OnInstanceEnd(InstanceNode node)
        {
            throw new NotImplementedException();
        }

        public RenderNodeAction OnLinkBegin(LinkNode node)
        {
            throw new NotImplementedException();
        }

        public void OnLinkEnd(LinkNode node)
        {
            throw new NotImplementedException();
        }

        public RenderNodeAction OnFaceBegin(FaceNode node)
        {
            throw new NotImplementedException();
        }

        public void OnFaceEnd(FaceNode node)
        {
            throw new NotImplementedException();
        }

        public void OnRPC(RPCNode node)
        {
            throw new NotImplementedException();
        }

        public void OnLight(LightNode node)
        {
            throw new NotImplementedException();
        }

        public void OnMaterial(MaterialNode node)
        {
            throw new NotImplementedException();
        }

        public void OnPolymesh(PolymeshTopology node)
        {
            throw new NotImplementedException();
        }

        bool skipElementFlag = false;
        Autodesk.Revit.DB.Transform linkTransformation;
        IndexedDictionary<GeometryDataObject> currentGeometry;
        IndexedDictionary<VertexLookupIntObject> currentVertices;

        string GetFamilyName(Element element)
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

        string GetTypeNameIfApplicable(ElementId elementId)
        {
            var element = doc.GetElement(elementId);
            if (element == null) return "Element not found";

            if (doc.GetElement(element.GetTypeId()) is ElementType elementType)
            {
                return elementType.Name;
            }
            else
            {
                return "Type not applicable or not found";
            }
        }

        object GetParameterValue(Autodesk.Revit.DB.Parameter parameter)
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
                    return parameter.AsElementId().IntegerValue.ToString();
                default:
                    return null;
            }
        }
        RenderNodeAction IExportContext.OnViewBegin(ViewNode node)
        {
            throw new NotImplementedException();
        }

        void IExportContext.OnViewEnd(ElementId elementId)
        {
            throw new NotImplementedException();
        }
    }
}