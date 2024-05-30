using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CesiumIonRevitAddin.gltf
{
    internal class GltfExportContext : IPhotoRenderContext
    {
        public GltfExportContext(Document doc)
        {
            documents.Add(doc);
            view = doc.ActiveView;
        }

        private View view;
        private List<Document> documents = new List<Document>();
        private bool cancelation;
        private Stack<Autodesk.Revit.DB.Transform> transformStack = new Stack<Autodesk.Revit.DB.Transform>();
        GltfNode rootNode;
        GltfVersion gltfVersion = new GltfVersion();
        GltfExtStructuralMetadataExtensionSchema extStructuralMetadata = new GltfExtStructuralMetadataExtensionSchema();

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
            rootNode.name = "rootNode";
            rootNode.rotation = CesiumIonRevitAddin.Transform.ModelRotation.Get(preferences.flipAxis);
            rootNode.scale = new List<double> { 1.0, 1.0, 1.0 };

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

        }
    }
}