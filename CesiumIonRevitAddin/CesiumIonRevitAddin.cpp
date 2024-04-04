// #include "stdafx.h"
#include "pch.h"
#include "CesiumIonRevitAddin.h"

#include <CesiumGltf/AccessorUtility.h>
#include <CesiumGltf/MeshPrimitive.h>

using namespace System;
using namespace System::IO;
using namespace Autodesk::Revit::ApplicationServices;
using namespace Autodesk::Revit::Attributes;
using namespace Autodesk::Revit::DB;
using namespace Autodesk::Revit::UI;
using namespace Autodesk::Windows;
using namespace System::Windows::Media::Imaging;


using namespace CesiumIonRevitAddin;

//Result Command::Execute(
//	ExternalCommandData^ commandData,
//	String^% message,
//	ElementSet^ elements)
//{
//	TaskDialog::Show("Revit", "Using C++/CLI doesn't seem too bad");
//	return Autodesk::Revit::UI::Result::Succeeded;
//}

Autodesk::Revit::UI::Result CesiumIonRevitAddin::ExternalApplication::OnStartup(UIControlledApplication^ application)
{
    CreateRibbonTab(application, RIBBONTAB);
 
    Autodesk::Revit::UI::RibbonPanel^ panel = nullptr;
    System::Collections::Generic::List<Autodesk::Revit::UI::RibbonPanel^> ^ribbonPanels = application->GetRibbonPanels();
    for each (auto existingPanel in ribbonPanels)
    {
        auto name = existingPanel->Name;
        if (name->Equals(RIBBONPANEL))
        {
            panel = existingPanel;
            break;
        }
    }

    if (panel == nullptr)
    {
        panel = application->CreateRibbonPanel(RIBBONTAB, RIBBONPANEL);
    }

    ContextualHelp^ contexHelp = 
        gcnew Autodesk::Revit::UI::ContextualHelp(Autodesk::Revit::UI::ContextualHelpType::Url, "www.cesium.com");
    PushButtonData^ pushButtonData = 
        gcnew Autodesk::Revit::UI::PushButtonData("push Button Name", "Export to Cesium 3D Tiles", addInPath, "CesiumIonRevitAddin.ExportCommand");
    // pushDataButton.LargeImage = new BitmapImage(new Uri(Path.Combine(buttonIconsFolder, "logo.png"), UriKind.Absolute));
    pushButtonData->LargeImage = gcnew BitmapImage(gcnew Uri(Path::Combine(buttonIconsFolder, "logo.png"), UriKind::Absolute));
    pushButtonData->SetContextualHelp(contexHelp);
    pushButtonData->ToolTip = "Export View to 3D Tiles.";
    pushButtonData->LongDescription = "Export any 3D View into the 3D Tiles ecosystem.";
    panel->AddItem(pushButtonData);

    // look for XXXXXX RibbonPanel, or create it if not already created
    Autodesk::Revit::UI::RibbonPanel^ panelAbout = nullptr;
    for each(auto existingPanel in application->GetRibbonPanels())
    {
        // auto name = existingPanel->Name;
        if (existingPanel->Name->Equals("About"))
        {
            panelAbout = existingPanel;
            break;
        }
    }

    if (!panelAbout)
    {
        panelAbout = application->CreateRibbonPanel(RIBBONTAB, "About");
    }


    auto pushDataButtonAbout = gcnew PushButtonData("About us", "About us", addInPath, "CesiumIonRevitAddin.AboutUs");
    pushDataButtonAbout->LargeImage = gcnew BitmapImage(gcnew Uri(System::IO::Path::Combine(buttonIconsFolder, "logo.png"), UriKind::Absolute));
    pushDataButtonAbout->ToolTip = "About Cesium 3D Tiles";
    pushDataButtonAbout->SetContextualHelp(contexHelp);
    pushDataButtonAbout->LongDescription = "Find out more about 3D Tiles and Cesium.";
    panelAbout->AddItem(pushDataButtonAbout);

	return Autodesk::Revit::UI::Result::Succeeded;
}

Autodesk::Revit::UI::Result CesiumIonRevitAddin::ExternalApplication::OnShutdown(UIControlledApplication^ application)
{
	return Autodesk::Revit::UI::Result::Succeeded;
}

void CesiumIonRevitAddin::ExternalApplication::CreateRibbonTab(Autodesk::Revit::UI::UIControlledApplication^ application, System::String^ ribbonTabName)
{
    Autodesk::Windows::RibbonControl^ ribbon = Autodesk::Windows::ComponentManager::Ribbon;
    auto tab = ribbon->FindTab(ribbonTabName);

    if (tab == nullptr)
    {
        application->CreateRibbonTab(ribbonTabName);
    }
}

void CesiumIonRevitAddin::ExternalApplication::ExportView()
{

}

Autodesk::Revit::UI::Result CesiumIonRevitAddin::ExportCommand::Execute(Autodesk::Revit::UI::ExternalCommandData^ commandData, System::String^% message, Autodesk::Revit::DB::ElementSet^ elements)
{
    CesiumGltf::Model model;
    model.asset.version = "2.0";
    auto& scene = model.scenes.emplace_back();
    model.scene = 0;
    if (model.scene == 0) {
       Autodesk::Revit::UI::TaskDialog::Show("Cesium GS", "Cesium Native is integrated...");
    }


    //try
    //{
    //    UIApplication^ uiapp = commandData->Application;
    //    UIDocument^ uidoc = uiapp->ActiveUIDocument;
    //    Application^ app = uiapp->Application;
    //    Document^ doc = uidoc->Document;

    //    View^ view = doc->ActiveView;

    //    if (view->GetType()->Name != "View3D")
    //    {
    //        MessageWindow->Show("Wrong View", "You must be in a 3D view to export");
    //        return Result.Succeeded;
    //    }

    //    MainWindow mainWindow = new MainWindow(doc, view);
    //    mainWindow.ShowDialog();

    //    return Result.Succeeded;
    //}
    //catch (Exception ex)
    //{
    //    MessageWindow.Show("Error", ex.Message);
    //    return Result.Failed;
    //}


    // Autodesk::Revit::DB::View3D^ exportView = safe_cast<Autodesk::Revit::DB::View3D^>(this->View);
    Autodesk::Revit::DB::Document^ doc = commandData->Application->ActiveUIDocument->Document; 
    // Autodesk::Revit::DB::View^ activeView = doc->ActiveView;

    // string format = System::String::Concat(".", SettingsConfig.GetValue("format"));
    System::String^ format = "gltf";
    // string fileName = SettingsConfig.GetValue("fileName");
    System::String^ fileName = "outputFileName";
    //bool dialogResult = FilesHelper.AskToSave(ref fileName, string.Empty, format);
    //if (dialogResult != true)
    //{
    //    return;
    //}

    auto directory = fileName->Replace(format, System::String::Empty);
    auto nameOnly = System::IO::Path::GetFileNameWithoutExtension(fileName);

    //SettingsConfig.SetValue("path", directory);
    //SettingsConfig.SetValue("fileName", nameOnly);

    // Document doc = exportView.Document;
    System::Collections::Generic::List<Autodesk::Revit::DB::Element^> ^elementsInView = GetAllVisibleElementsByView(doc, doc->ActiveView);

    if (elementsInView->Count == 0)
    {
        // MessageWindow.Show("No Valid Elements", "There are no valid elements to export in this view");
        return Autodesk::Revit::UI::Result::Failed;
    }

    //int numberRuns = int.Parse(SettingsConfig.GetValue("runs"));
    //int incrementRun = numberRuns + 1;
    //SettingsConfig.SetValue("runs", incrementRun.ToString());

    //ProgressBarWindow progressBar =
    //    ProgressBarWindow.Create(elementsInView.Count + 1, 0, "Converting elements...", this);

    // Use our custom implementation of IExportContext as the exporter context.
//    GLTFExportContext ctx = new GLTFExportContext(doc);
//
//    // Create a new custom exporter with the context.
//    CustomExporter^ exporter = gcnew Autodesk::Revit::DB::CustomExporter(doc, ctx);
//    exporter->ShouldStopOnError = false;
//
//#if REVIT2019
//    exporter.Export(exportView);
//#else
//    exporter->Export(exportView as View);
//#endif
//
//    Thread.Sleep(500);
//    ProgressBarWindow.ViewModel.ProgressBarValue = elementsInView.Count + 1;
//    ProgressBarWindow.ViewModel.ProgressBarPercentage = 100;
//    ProgressBarWindow.ViewModel.Message = "Export completed!";
//    ProgressBarWindow.ViewModel.Action = "Accept";


    return Autodesk::Revit::UI::Result::Succeeded;
}

System::Collections::Generic::List<Autodesk::Revit::DB::Element^>^ CesiumIonRevitAddin::ExportCommand::GetAllVisibleElementsByView(Autodesk::Revit::DB::Document^ doc, Autodesk::Revit::DB::View^ view)
{
    Autodesk::Revit::DB::FilteredElementCollector^ collector = gcnew Autodesk::Revit::DB::FilteredElementCollector(doc, view->Id);
    System::Collections::Generic::List<Autodesk::Revit::DB::Element^>^ result = gcnew System::Collections::Generic::List<Autodesk::Revit::DB::Element^>();

    // Use WherePasses to filter elements
    collector->WhereElementIsNotElementType();

    // Manual filtering equivalent to the LINQ query in C#
    for each (Autodesk::Revit::DB::Element ^ e in collector->ToElements())
    {
        if (e->CanBeHidden(view) && e->Category != nullptr)
        {
            result->Add(e);
        }
    }

    return result;
}
