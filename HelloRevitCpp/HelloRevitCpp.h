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
    };

}