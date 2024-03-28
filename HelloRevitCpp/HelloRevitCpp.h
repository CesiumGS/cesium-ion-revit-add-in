#pragma once
using namespace Autodesk::Revit::Attributes;

namespace HelloRevitCpp {

    [Transaction(TransactionMode::Manual)]
    [Regeneration(RegenerationOption::Manual)]
    public ref class ExternalApplication
    : Autodesk::Revit::UI::IExternalApplication
    {
    public:
        //virtual Autodesk::Revit::UI::Result Execute(
        //    Autodesk::Revit::UI::ExternalCommandData^ commandData,
        //    System::String^% message,
        //    Autodesk::Revit::DB::ElementSet^ elements);

        virtual Autodesk::Revit::UI::Result OnStartup(Autodesk::Revit::UI::UIControlledApplication^ application);
        virtual Autodesk::Revit::UI::Result OnShutdown(Autodesk::Revit::UI::UIControlledApplication^ application);
        static void CreateRibbonTab(Autodesk::Revit::UI::UIControlledApplication^ application, System::String^ ribbonTabName);
        void ExportView();

    private: 
        static System::String^ addInPath = ExternalApplication::typeid->Assembly->Location;
        static System::String^ buttonIconsFolder = System::IO::Path::GetDirectoryName(addInPath) + "\\Images\\";
        literal System::String^ RIBBONPANEL = "Cesium Export Panel";
        literal System::String^ RIBBONTAB = "Cesium GS C++";


    };

    [Transaction(TransactionMode::Manual)]
    [Regeneration(RegenerationOption::Manual)]
    public ref class ExportCommand : Autodesk::Revit::UI::IExternalCommand {

    public:
        virtual Autodesk::Revit::UI::Result Execute(
            Autodesk::Revit::UI::ExternalCommandData^ commandData,
            System::String^% message,
            Autodesk::Revit::DB::ElementSet^ elements);    
        static System::Collections::Generic::List<Autodesk::Revit::DB::Element^>^ GetAllVisibleElementsByView(Autodesk::Revit::DB::Document^ doc, Autodesk::Revit::DB::View^ view);
    };
}