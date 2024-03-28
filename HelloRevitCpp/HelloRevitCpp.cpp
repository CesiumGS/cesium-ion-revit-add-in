// #include "stdafx.h"
#include "pch.h"
#include "HelloRevitCpp.h"

using namespace System;
using namespace System::IO;
using namespace Autodesk::Revit::ApplicationServices;
using namespace Autodesk::Revit::Attributes;
using namespace Autodesk::Revit::DB;
using namespace Autodesk::Revit::UI;
using namespace Autodesk::Windows;
using namespace System::Windows::Media::Imaging;


using namespace HelloRevitCpp;

//Result Command::Execute(
//	ExternalCommandData^ commandData,
//	String^% message,
//	ElementSet^ elements)
//{
//	TaskDialog::Show("Revit", "Using C++/CLI doesn't seem too bad");
//	return Autodesk::Revit::UI::Result::Succeeded;
//}

Autodesk::Revit::UI::Result HelloRevitCpp::ExternalApplication::OnStartup(UIControlledApplication^ application)
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
        gcnew Autodesk::Revit::UI::PushButtonData("push Button Name", "Export to Cesium 3D Tiles", addInPath, "HelloRevitCpp.ExternalCommand");
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


    auto pushDataButtonAbout = gcnew PushButtonData("About us", "About us", addInPath, "HelloRevitCpp.AboutUs");
    pushDataButtonAbout->LargeImage = gcnew BitmapImage(gcnew Uri(System::IO::Path::Combine(buttonIconsFolder, "logo.png"), UriKind::Absolute));
    pushDataButtonAbout->ToolTip = "About Cesium 3D Tiles";
    pushDataButtonAbout->SetContextualHelp(contexHelp);
    pushDataButtonAbout->LongDescription = "Find out more about 3D Tiles and Cesium.";
    panelAbout->AddItem(pushDataButtonAbout);

	return Autodesk::Revit::UI::Result::Succeeded;
}

Autodesk::Revit::UI::Result HelloRevitCpp::ExternalApplication::OnShutdown(UIControlledApplication^ application)
{
	return Autodesk::Revit::UI::Result::Succeeded;
}

void HelloRevitCpp::ExternalApplication::CreateRibbonTab(Autodesk::Revit::UI::UIControlledApplication^ application, System::String^ ribbonTabName)
{
    Autodesk::Windows::RibbonControl^ ribbon = Autodesk::Windows::ComponentManager::Ribbon;
    auto tab = ribbon->FindTab(ribbonTabName);

    if (tab == nullptr)
    {
        application->CreateRibbonTab(ribbonTabName);
    }
}
